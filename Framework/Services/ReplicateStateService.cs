using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.IoC;
using Average.Shared.Attributes;
using Average.Shared.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Average.Client.Framework.Services
{
    internal class ReplicateStateService : IService
    {
        private readonly Container _container;
        private readonly EventService _eventService;
        private readonly ThreadService _threadService;
        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private readonly List<GetReplicatedState> _getReplicatedStates = new();

        private const int SyncRate = 60;

        public ReplicateStateService(Container container, EventService eventService, ThreadService threadService)
        {
            _container = container;
            _eventService = eventService;
            _threadService = threadService;
        }

        internal void Reflect()
        {
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            foreach (var type in types)
            {
                if (_container.IsRegistered(type))
                {
                    // Continue if the service have the same type of this class
                    if (type == GetType()) continue;

                    var service = _container.Resolve(type);
                    var properties = type.GetProperties(flags);

                    for (int i = 0; i < properties.Length; i++)
                    {
                        var property = properties[i];
                        var attrs = property.GetCustomAttributes().ToList();

                        if (attrs != null && attrs.Exists(x => x.GetType() == typeof(GetReplicatedAttribute)))
                        {
                            var attr = attrs.Find(x => x.GetType() == typeof(GetReplicatedAttribute)) as GetReplicatedAttribute;
                            RegisterInternalGetReplicatedState(attr, service, ref property);
                        }
                    }
                }
            }
        }

        private object GetPropertyValue(PropertyInfo property, object classObj)
        {
            if (property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(classObj, null);
            }

            return null;
        }

        internal void RegisterInternalGetReplicatedState(GetReplicatedAttribute attr, object classObj, ref PropertyInfo property)
        {
            if (property.CanWrite && property.CanRead)
            {
                _getReplicatedStates.Add(new GetReplicatedState(attr, property, classObj));
                Logger.Debug($"Registering [GetReplicated]: {attr.Name} on property: {property.Name}.");
            }
            else
            {
                Logger.Error($"Unable to register [GetReplicated]: {attr.Name} on property: {property.Name}. This property need to have a getter & setter.");
            }
        }

        private void OnInternalSetReplicateState(string attrName, object value)
        {
            var states = _getReplicatedStates.Where(x => x.Attribute.Name == attrName).ToList();

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

        internal IEnumerable<GetReplicatedState> GetReplicatedStates() => _getReplicatedStates.AsEnumerable();
    }
}
