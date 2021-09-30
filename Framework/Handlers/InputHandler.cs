using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using System.Collections.Generic;
using static Average.Client.Framework.Services.InputService;

namespace Average.Client.Framework.Handlers
{
    internal class InputHandler : IHandler
    {
        private readonly InputService _inputService;

        public InputHandler(InputService inputService)
        {
            _inputService = inputService;
        }

        [ClientEvent("input:register_inputs")]
        private void OnRegisterInputs(string inputsJson)
        {
            var inputs = inputsJson.Deserialize<List<Input>>();
            inputs.ForEach(x => _inputService.RegisterInput(x));
        }

        [ClientEvent("input:set_state")]
        private void OnSetInputState(string inputId, bool isValidate)
        {
            _inputService.SetInputState(inputId, isValidate);
        }
    }
}
