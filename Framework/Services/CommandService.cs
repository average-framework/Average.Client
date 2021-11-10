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
        private readonly UserService _userService;

        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        private readonly List<Command> _commands = new();

        public CommandService(Container container, RpcService rpc, UserService userService)
        {
            _container = container;
            _rpc = rpc;
            _userService = userService;

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

                        if (_userService.User == null) return;

                        var command = GetCommand(commandName);
                        if (command == null) return;

                        if (_userService.User.PermissionLevel >= command.Attribute.PermissionLevel)
                        {
                            action.DynamicInvoke(newArgs.ToArray());
                        }
                        else
                        {
                            Logger.Error("You have not the permission level for this command.");
                        }
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

        private void RegisterInternalClientCommand(ClientCommandAttribute cmdAttr, CommandAliasAttribute aliasAttr, object classObj, MethodInfo method)
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

        internal Command GetCommand(string commandName) => _commands.Find(x => x.Attribute.Command == commandName);
    }
}
