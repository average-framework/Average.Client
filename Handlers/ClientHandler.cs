using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Events;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Managers;

namespace Average.Client.Handlers
{
    internal class ClientHandler : IHandler
    {
        private readonly EventManager _eventManager;

        public ClientHandler(EventManager eventManager)
        {
            _eventManager = eventManager;

            _eventManager.ResourceStart += OnResourceStart;
        }

        private void OnResourceStart(object sender, ResourceStartEventArgs e)
        {
            if (e.Resource == "avg")
            {
                // Initialize client
                Logger.Debug("Average started");

                _eventManager.EmitServer("client:game_initialized");
            }
        }
    }
}
