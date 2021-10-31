using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using CitizenFX.Core;
using System;
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
            public string Id { get; }
            public Control Key { get; }
            public bool LastConditionState { get; set; }
            public Func<bool> Condition { get; }
            public Action<bool> OnStateChanged { get; }
            public Action OnKeyReleased { get; }

            public Input(Control key, Func<bool> condition, Action<bool> onStateChanged, Action onKeyReleased)
            {
                Id = Guid.NewGuid().ToString();
                Key = key;
                Condition = condition;
                OnStateChanged = onStateChanged;
                OnKeyReleased = onKeyReleased;
            }
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
                    var isValidate = input.Condition.Invoke();

                    if(input.LastConditionState != isValidate)
                    {
                        input.LastConditionState = isValidate;
                        input.OnStateChanged?.Invoke(isValidate);
                    }

                    if (isValidate && IsControlJustReleased(0, (uint)input.Key))
                    {
                        input.OnKeyReleased();
                    }
                }
            }
            else
            {
                await BaseScript.Delay(1000);
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

        internal InputService RegisterKey(Input input)
        {
            _inputs.Add(input);
            return this;
        }

        internal InputService UnregisterKey(Input input)
        {
            _inputs.Remove(input);
            return this;
        }
    }
}
