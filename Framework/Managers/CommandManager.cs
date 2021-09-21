using Average.Client.Framework.Diagnostics;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Average.Client.Framework.Managers
{
    internal class CommandManager
    {
        private readonly EventManager _eventManager;
        private readonly List<Tuple<string, string[]>> _commands = new List<Tuple<string, string[]>>();

        public CommandManager(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        internal void Reflect(string json)
        {
            var commands = JArray.Parse(json).Cast<dynamic>().ToList();

            Logger.Debug("Reflect commands: " + commands.Count());

            foreach (var command in commands)
            {
                var commandName = (string)command["Command"];
                var alias = ((JArray)JArray.Parse(command["Alias"].ToString())).Cast<string>();
                RegisterInternalCommand(commandName, alias.ToArray());
            }
        }

        private void RegisterCommand(string commandName)
        {
            API.RegisterCommand(commandName, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // Need to add an rpc check, if the command have no valid argument, we need to get a command result from the server
                _eventManager.EmitServer("client-command:execute", args.ToArray());
            }), false);
        }

        private void RegisterInternalCommand(string commandName, string[] commandAlias)
        {
            RegisterCommand(commandName);
            commandAlias.ToList().ForEach(x => RegisterCommand(x));
            _commands.Add(new Tuple<string, string[]>(commandName, commandAlias));

            Logger.Debug($"Registering [Command]: {commandName} with alias: [{string.Join(", ", commandAlias)}]");
        }

        internal IEnumerable<string> GetCommands() => _commands.Select(x => x.Item1);
        internal IEnumerable<string> GetCommandAlias(string commandName) => _commands.Find(x => x.Item1 == commandName).Item2;
    }
}
