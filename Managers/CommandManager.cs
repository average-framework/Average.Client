using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SDK.Client.Interfaces;

namespace Average.Client.Managers
{
    public class CommandManager : InternalPlugin, ICommandManager
    {
        private static List<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>> _commands = new List<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>>();

        private static PermissionManager _permissionManager;
        
        public override void OnInitialized()
        {
            _permissionManager = Permission;
        }

        internal static void RegisterInternalCommand(ClientCommandAttribute commandAttr, ClientCommandAliasAttribute aliasAttr, object classObj, MethodInfo method)
        {
            API.RegisterCommand(commandAttr.Command, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                ExecuteCommand(commandAttr, method, classObj, args.ToArray());
            }), false);

            if (aliasAttr != null)
            {
                foreach (var alias in aliasAttr.Alias)
                {
                    API.RegisterCommand(alias, new Action<int, List<object>, string>(async (source, args, raw) =>
                    {
                        ExecuteCommand(commandAttr, method, classObj, args.ToArray());
                    }), false);
                }
            }
            
            _commands.Add(new Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>(commandAttr, aliasAttr));
            
            Log.Debug($"Registering [Command] attribute: {commandAttr.Command} on method: {method.Name} with alias: {(aliasAttr != null ? $"[{string.Join(", ", aliasAttr.Alias)}]" : "empty")}");
        }

        private static async void ExecuteCommand(ClientCommandAttribute commandAttr, MethodInfo method, object classObj, params object[] args)
        {
            var methodParams = method.GetParameters();
            
            if (await _permissionManager.HasPermission(commandAttr.PermissionName, commandAttr.PermissionLevel) || string.IsNullOrEmpty(commandAttr.PermissionName))
            {
                var newArgs = new List<object>();

                if (args.Count() == methodParams.Length)
                {
                    try
                    {
                        args.ToList().ForEach(x => newArgs.Add(Convert.ChangeType(x, methodParams[args.ToList().FindIndex(p => p == x)].ParameterType)));
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
                    Log.Error($"Invalid command usage: {commandAttr.Command} {usage}.");
                }
            }
            else
            {
                Log.Error($"Unable to execute this command.");
            }
        }

        public IEnumerable<Tuple<ClientCommandAttribute, ClientCommandAliasAttribute>> GetCommands() => _commands.AsEnumerable();

        public ClientCommandAttribute GetCommand(string command) => _commands.Find(x => x.Item1.Command == command).Item1;

        public ClientCommandAliasAttribute GetCommandAlias(string command) => _commands.Find(x => x.Item1.Command == command).Item2;

        public int Count() => _commands.Count();
    }
}
