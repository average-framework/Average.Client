using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Rpc;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            Logger.Debug("Try to reflect");

            try
            {
                var commands = JsonConvert.DeserializeObject<List<JObject>>(json);

                Logger.Debug("Reflect commands: " + commands.Count());

                foreach (var command in commands)
                {
                    Logger.Debug("command: " + command);

                    var commandName = (string)command["Command"];
                    var alias = JArray.Parse(command["Alias"].ToString()).Cast<string>();
                    RegisterInternalCommand(commandName, alias.ToArray());
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error on reflect: " + ex.Message + "\n" + ex.StackTrace); ;
            }
        }

        private void RegisterCommand(string commandName)
        {
            API.RegisterCommand(commandName, new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                // Need to add an rpc check, if the command have no valid argument, we need to get a command result from the server

                _rpc.Event("client:execute_command").On<bool, string>((success, errorMessage) => 
                {
                    if (!success)
                    {
                        Logger.Error(errorMessage);
                    }
                }).Emit(commandName, args);
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
