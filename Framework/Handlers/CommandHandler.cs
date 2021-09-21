using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Managers;
using Average.Shared.Attributes;

namespace Average.Client.Framework.Handlers
{
    internal class CommandHandler : IHandler
    {
        private readonly CommandManager _commandManager;

        public CommandHandler(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        [ClientEvent("client-command:register_commands")]
        private void OnRegisterCommands(string json)
        {
            Logger.Debug("Register commands: " + json);
            _commandManager.Reflect(json);
        }
    }
}