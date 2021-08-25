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
        private List<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>> _commands = new List<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>>();

        private void RegisterCommandInternal(string command, object classObj, MethodInfo method, ClientCommandAttribute commandAttr)
        {
            var methodParams = method.GetParameters();

            API.RegisterCommand(command, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                if (await Main.permissionManager.HasPermission(commandAttr.PermissionName, commandAttr.PermissionLevel) || commandAttr.PermissionName == null)
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
                            Log.Error($"Unable to convert command arguments.");
                        }
                    }
                    else
                    {
                        var usage = "";
                        methodParams.ToList().ForEach(x => usage += $"<[{x.ParameterType.Name}] {x.Name}> ");
                        Log.Error($"Invalid command usage: {command} {usage}.");
                    }
                }
                else
                {
                    Log.Error($"Unable to execute this command.");
                }
            }), false);

            Log.Debug($"Registering [Command] attribute: {command} on method: {method.Name}");
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
                Log.Debug($"Registering {aliasAttr.Alias.Length} alias for command: {commandAttr.Command} [{string.Join(", ", aliasAttr.Alias)}]");
            }

            _commands.Add(new Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>(commandAttr, aliasAttr));
        }

        public IEnumerable<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>> GetCommands() => _commands.AsEnumerable();

        public ClientCommandAttribute GetCommand(string command) => _commands.Find(x => x.Item1.Command == command).Item1;

        public ClientCommandAliasAttribute GetCommandAlias(string command) => _commands.Find(x => x.Item1.Command == command).Item2;

        public int Count() => _commands.Count();
    }
}
