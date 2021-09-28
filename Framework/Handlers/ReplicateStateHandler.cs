using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using System;
using System.Linq;

namespace Average.Client.Framework.Handlers
{
    internal class ReplicateStateHandler : IHandler
    {
        private readonly ReplicateStateService _replicateStateService;

        public ReplicateStateHandler(ReplicateStateService replicateStateService)
        {
            _replicateStateService = replicateStateService;

            Logger.Debug("ReplicateStateService Initialized successfully");
        }

        [ClientEvent("replicate:property_value")]
        private void OnReplicatePropertyValue(string attrName, object value)
        {
            var states = _replicateStateService.GetReplicatedStates().Where(x => x.Attribute.Name == attrName).ToList();

            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];

                // Need to add this for few number type, int32 can be converted to int16 and result to an convertion error without this line
                var newStateVal = Convert.ChangeType(value, state.Property.PropertyType);

                if (newStateVal.GetType() == state.Property.PropertyType)
                {
                    state.Property.SetValue(state.ClassObj, newStateVal, null);
                }
                else
                {
                    Logger.Error($"Unable to replicate value on property: [{state.Attribute.Name}] {state.Property.Name}. The type is not the same [{string.Join(", ", newStateVal.GetType(), state.Property.PropertyType)}]");
                }
            }
        }
    }
}
