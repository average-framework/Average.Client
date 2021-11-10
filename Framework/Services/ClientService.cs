using Average.Client.Framework.Interfaces;
using System.Collections.Generic;

namespace Average.Client.Framework.Services
{
    internal class ClientService : IService
    {
        private readonly EventService _eventService;

        private Dictionary<string, object> _sharedData = new();

        public ClientService(EventService eventService)
        {
            _eventService = eventService;
        }

        internal T GetSharedData<T>(string key)
        {
            if (_sharedData.ContainsKey(key))
            {
                return (T)_sharedData[key];
            }
            else
            {
                return default;
            }
        }

        internal void ShareData(string key, object value, bool @override = true)
        {
            if (_sharedData.ContainsKey(key))
            {
                if (@override)
                {
                    _sharedData[key] = value;
                }
            }
            else
            {
                _sharedData.Add(key, value);
            }

            //_eventService.EmitServer("client:share_data", key, value, @override);
        }

        internal void UnshareData(string key)
        {
            if (_sharedData.ContainsKey(key))
            {
                _sharedData.Remove(key);
            }

            //_eventService.EmitServer("client:unshare_data", key);
        }
    }
}
