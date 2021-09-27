using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;

namespace Average.Client.Framework.Handlers
{
    internal class CommandHandler : IHandler
    {
        private readonly CommandService _commandManager;

        public CommandHandler(CommandService commandManager)
        {
            _commandManager = commandManager;
        }

        [ClientEvent("command:register_commands")]
        private void OnRegisterCommands(string json)
        {
            _commandManager.Reflect(json);
        }
    }
}