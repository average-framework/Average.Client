using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;

namespace Average.Client.Handlers
{
    internal class ClientHandler : IHandler
    {
        private readonly EventService _eventService;

        public ClientHandler(EventService eventService)
        {
            _eventService = eventService;
        }

        internal void OnClientInitialized()
        {
            // Initialize client
            _eventService.EmitServer("client:initialized");
        }
    }
}
