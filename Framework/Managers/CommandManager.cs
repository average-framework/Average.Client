using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Rpc;
using Average.Shared.Models;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Average.Client.Framework.Managers
{
    internal class CommandManager
    {
        private readonly EventManager _eventManager;
        private readonly RpcRequest _rpc;
        private readonly List<Tuple<string, string[]>> _commands = new List<Tuple<string, string[]>>();

        public CommandManager(EventManager eventManager, RpcRequest rpc)
        {
            _eventManager = eventManager;
            _rpc = rpc;

            Logger.Debug("CommandManager Initialized successfully");
        }

        internal void Reflect(string json)
        {
            var commands = JsonConvert.DeserializeObject<List<CommandModel>>(json);
            commands.ForEach(x => RegisterInternalCommand(x));
        }

        private void RegisterCommand(string commandName)
        {
            API.RegisterCommand(commandName, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // Need to add an rpc check, if the command have no valid argument or error, we need to get a command result from the server

                _rpc.Event("command:execute").On<bool, string>((success, errorMessage) =>
                {
                    if (!success)
                    {
                        Logger.Error(errorMessage);
                    }
                }).Emit(commandName, args);
            }), false);
        }

        private void RegisterInternalCommand(CommandModel command)
        {
            RegisterCommand(command.Name);
            command.Alias.ToList().ForEach(x => RegisterCommand(x));

            _commands.Add(new Tuple<string, string[]>(command.Name, command.Alias));
            Logger.Debug($"Registering [Command]: {command.Name} with alias: [{string.Join(", ", command.Alias)}]");
        }

        internal IEnumerable<string> GetCommands() => _commands.Select(x => x.Item1);
        internal IEnumerable<string> GetCommandAlias(string commandName) => _commands.Find(x => x.Item1 == commandName).Item2;
    }
}
