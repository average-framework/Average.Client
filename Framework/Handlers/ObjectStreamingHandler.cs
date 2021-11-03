using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Handlers
{
    internal class ObjectStreamingHandler : IHandler
    {
        private readonly ObjectStreamingService _objectStreamingService;
        private readonly EventService _eventService;

        public ObjectStreamingHandler(EventService eventService, ObjectStreamingService objectStreamingService)
        {
            _eventService = eventService;
            _objectStreamingService = objectStreamingService;
            
            _eventService.ResourceStop += OnResourceStop;
        }

        private void OnResourceStop(object sender, Events.ResourceStopEventArgs e)
        {
            if (e.Resource == "avg")
            {
                for (int i = 0; i < _objectStreamingService.registeredProps.Count; i++)
                {
                    var entity = _objectStreamingService.registeredProps[i].Handle;
                    DeleteEntity(ref entity);
                    DeleteObject(ref entity);
                }

                _objectStreamingService.registeredProps.Clear();
            }
        }
    }
}
