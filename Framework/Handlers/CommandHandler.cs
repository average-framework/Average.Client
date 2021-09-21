using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Managers;

namespace Average.Client.Framework.Handlers
{
    internal class CommandHandler : IHandler
    {
        private readonly CommandManager _commandManager;

        public CommandHandler(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        [ClientEvent("command:register_commands")]
        private void OnRegisterCommands(string json)
        {
            Logger.Debug("command json: " + json);
            Logger.Debug("command is null ? " + (_commandManager == null));

            try
            {
                _commandManager.Reflect(json);
            }
            catch
            {
                Logger.Error("reflect");
            }
        }
    }
}