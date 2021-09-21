using Average.Client.Framework.Diagnostics;
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
        }

        internal void OnClientInitialized()
        {
            // Initialize client
            _eventManager.EmitServer("client:initialized");
        }
    }
}
