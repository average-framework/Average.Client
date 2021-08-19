using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Average.Client.Managers
{
    public class CommandManager : ICommandManager
    {
        Logger logger;
        PermissionManager permission;

        List<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>> Commands { get; }

        public CommandManager(Logger logger, PermissionManager permission)
        {
            this.logger = logger;
            this.permission = permission;

            Commands = new List<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>>();
        }

        internal void RegisterCommandInternal(string command, object classObj, MethodInfo method, ClientCommandAttribute commandAttr)
        {
            var methodParams = method.GetParameters();

            API.RegisterCommand(command, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                if (await permission.HasPermission(commandAttr.PermissionName, commandAttr.PermissionLevel) || commandAttr.PermissionName == null)
                {
                    var newArgs = new List<object>();

                    if (args.Count == methodParams.Length)
                    {
                        try
                        {
                            args.ForEach(x => newArgs.Add(Convert.ChangeType(x, methodParams[args.FindIndex(p => p == x)].ParameterType)));
                            method.Invoke(classObj, newArgs.ToArray());
                        }
                        catch
                        {
                            logger.Error($"Unable to convert command arguments.");
                        }
                    }
                    else
                    {
                        var usage = "";
                        methodParams.ToList().ForEach(x => usage += $"<[{x.ParameterType.Name}] {x.Name}> ");
                        logger.Error($"Invalid command usage: {command} {usage}.");
                    }
                }
                else
                {
                    logger.Error($"Unable to execute this command.");
                }
            }), false);

            logger.Debug($"Registering [Command] attribute: {command} on method: {method.Name}");
        }

        public void RegisterCommand(ClientCommandAttribute commandAttr, ClientCommandAliasAttribute aliasAttr, MethodInfo method, object classObj)
        {
            if (commandAttr == null)
                return;

            var methodParams = method.GetParameters();

            RegisterCommandInternal(commandAttr.Command, classObj, method, commandAttr);

            if (aliasAttr != null)
            {
                aliasAttr.Alias.ToList().ForEach(x => RegisterCommandInternal(x, classObj, method, commandAttr));
                logger.Debug($"Registering {aliasAttr.Alias.Length} alias for command: {commandAttr.Command} [{string.Join(", ", aliasAttr.Alias)}]");
            }

            Commands.Add(new Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>(commandAttr, aliasAttr));
        }

        public IEnumerable<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>> GetCommands() => Commands.AsEnumerable();

        public ClientCommandAttribute GetCommand(string command) => Commands.Find(x => x.Item1.Command == command).Item1;

        public ClientCommandAliasAttribute GetCommandAlias(string command) => Commands.Find(x => x.Item1.Command == command).Item2;

        public int Count() => Commands.Count();
    }
}
