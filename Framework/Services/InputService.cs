using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class InputService : IService
    {
        private readonly EventService _eventService;
        private readonly List<Input> _inputs = new();

        public class Input
        {
            public string Id { get; set; }
            public uint Key { get; set; }
            public bool IsValidate { get; set; }
        }

        public InputService(EventService eventService)
        {
            _eventService = eventService;
        }

        [Thread]
        private async Task Update()
        {
            if (_inputs.Count > 0)
            {
                for (int i = 0; i < _inputs.Count; i++)
                {
                    var input = _inputs[i];

                    if (input.IsValidate && IsControlJustReleased(0, input.Key))
                    {
                        _eventService.EmitServer("input:triggered", input.Id);
                    }
                }
            }
            else
            {
                await BaseScript.Delay(1000);
            }
        }

        internal void SetInputState(string inputId, bool isValidate)
        {
            var index = _inputs.FindIndex(x => x.Id == inputId);

            if (index > -1)
            {
                _inputs[index].IsValidate = isValidate;
            }
        }

        internal Input GetInput(string inputId)
        {
            return _inputs.Find(x => x.Id == inputId);
        }

        internal bool IsRegisterKey(Input input)
        {
            return _inputs.Exists(x => x.Id == input.Id);
        }

        internal void RegisterInput(Input input)
        {
            _inputs.Add(input);
        }

        internal void UnregisterInput(Input input)
        {
            _inputs.Remove(input);
        }
    }
}
