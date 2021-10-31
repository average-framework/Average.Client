using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.IoC;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Average.Client.Framework.Services
{
    internal class CommandService : IService
    {
        internal class Command
        {
            public ClientCommandAttribute Attribute { get; }
            public CommandAliasAttribute Alias { get; }
            public Delegate Action { get; }

            public Command(ClientCommandAttribute attribute, CommandAliasAttribute alias, Delegate action)
            {
                Attribute = attribute;
                Alias = alias;
                Action = action;
            }
        }

        private readonly Container _container;
        private readonly RpcService _rpc;

        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        private readonly List<Command> _commands = new();

        public CommandService(Container container, RpcService rpc)
        {
            _container = container;
            _rpc = rpc;

            Logger.Debug("CommandManager Initialized successfully");
        }

        internal void Reflect()
        {
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            // Register client commands
            foreach (var serviceType in types)
            {
                if (_container.IsRegistered(serviceType))
                {
                    // Continue if the service have the same type of this class
                    if (serviceType == GetType()) continue;

                    // Get service instance
                    var service = _container.Resolve(serviceType);
                    var methods = serviceType.GetMethods(flags);

                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<ClientCommandAttribute>();
                        if (attr == null) continue;
                        var aliasAttr = method.GetCustomAttribute<CommandAliasAttribute>();

                        RegisterInternalClientCommand(attr, aliasAttr, service, method);
                    }
                }
            }
        }

        private void RegisterClientCommand(string commandName, Delegate action)
        {
            API.RegisterCommand(commandName, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                var newArgs = new List<object>();
                var methodParams = action.Method.GetParameters();

                if (args.Count == methodParams.Length)
                {
                    try
                    {
                        args.ForEach(x => newArgs.Add(Convert.ChangeType(x, methodParams[args.FindIndex(p => p == x)].ParameterType)));
                        action.DynamicInvoke(newArgs.ToArray());
                    }
                    catch
                    {
                        Logger.Error($"Unable to convert client command arguments.");
                    }
                }
                else
                {
                    var usage = "";
                    methodParams.ToList().ForEach(x => usage += $"<[{x.ParameterType.Name}] {x.Name}> ");
                    Logger.Error($"Invalid client command usage: {commandName} {usage}.");
                }
            }), false);

            Logger.Debug($"[CommandService] Registering [ClientCommand]: {commandName} on method: {action.Method.Name}.");
        }

        internal void RegisterInternalClientCommand(ClientCommandAttribute cmdAttr, CommandAliasAttribute aliasAttr, object classObj, MethodInfo method)
        {
            var methodParams = method.GetParameters();
            var action = Delegate.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);

            RegisterClientCommand(cmdAttr.Command, action);

            if (aliasAttr != null)
            {
                foreach (var alias in aliasAttr.Alias)
                {
                    RegisterClientCommand(alias, action);
                }
            }

            _commands.Add(new Command(cmdAttr, aliasAttr, action));
        }

        private void RegisterCommand(string commandName)
        {
            API.RegisterCommand(commandName, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // Need to add an rpc check, if the command have no valid argument or error, we need to get a command result from the server
                _rpc.OnResponse<bool, string>("command:execute", (success, errorMessage) =>
                {
                    if (!success)
                    {
                        Logger.Error(errorMessage);
                    }
                }).Emit(commandName, args);
            }), false);
        }
    }
}
