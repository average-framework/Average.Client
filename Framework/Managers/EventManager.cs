using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Events;
using Average.Client.Framework.IoC;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Average.Client.Framework.Managers
{
    internal class EventManager
    {
        private readonly Container _container;
        private readonly EventHandlerDictionary _eventHandlers;
        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private readonly Dictionary<string, List<Delegate>> _events = new();

        public EventManager(Container container, EventHandlerDictionary eventHandlers)
        {
            _container = container;
            _eventHandlers = eventHandlers;

            _eventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
            _eventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            _eventHandlers["onResourceStarting"] += new Action<string>(OnResourceStarting);
            _eventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            _eventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);
            _eventHandlers["gameEventTriggered"] += new Action<string, int[]>(OnGameEventTriggered);
            _eventHandlers["onClientMapStart"] += new Action<string>(OnClientMapStart);
            _eventHandlers["onClientMapStop"] += new Action<string>(OnClientMapStop);
            _eventHandlers["onClientGameTypeStart"] += new Action<string>(OnClientGameTypeStart);
            _eventHandlers["onClientGameTypeStop"] += new Action<string>(OnClientGameTypeStop);
            _eventHandlers["playerActivated"] += new Action(OnPlayerActivated);
            _eventHandlers["sessionInitialized"] += new Action(OnSessionInitialized);
            _eventHandlers["populationPedCreating"] += new Action<float, float, float, uint, dynamic>(OnPopulationPedCreating);

            _eventHandlers["server-event:triggered"] += new Action<string, List<object>>(OnTriggerEvent);

            Logger.Debug("EventManager Initialized successfully");
        }

        public event EventHandler<ResourceStartEventArgs> ResourceStart;
        public event EventHandler<ResourceStopEventArgs> ResourceStop;
        public event EventHandler<ResourceStartingEventArgs> ResourceStarting;
        public event EventHandler<ClientResourceStartEventArgs> ClientResourceStart;
        public event EventHandler<ClientResourceStopEventArgs> ClientResourceStop;
        public event EventHandler<GameEventTriggeredEventArgs> GameEventTriggered;
        public event EventHandler<ClientMapStartEventArgs> ClientMapStart;
        public event EventHandler<ClientMapStopEventArgs> ClientMapStop;
        public event EventHandler<ClientMapGameTypeStartEventArgs> ClientGameTypeStart;
        public event EventHandler<ClientMapGameTypeStopEventArgs> ClientGameTypeStop;
        public event EventHandler<PlayerActivatedEventArgs> PlayerActivated;
        public event EventHandler<SessionInitializedEventArgs> SessionInitialized;
        public event EventHandler<PopulationPedCreatingEventArgs> PopulationPedCreating;

        internal void Reflect()
        {
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            // Register client events
            foreach (var service in types)
            {
                if (_container.IsRegistered(service))
                {
                    // Continue if the service have the same type of this class
                    if (service == GetType()) continue;

                    // Get service instance
                    var _service = _container.Resolve(service);
                    var methods = service.GetMethods(flags);

                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<ClientEventAttribute>();
                        if (attr == null) continue;

                        RegisterInternalEvent(attr, _service, method);
                    }
                }
            }
        }

        private void RegisterEvent(string eventName, Delegate action)
        {
            if (!_events.ContainsKey(eventName))
            {
                _events.Add(eventName, new List<Delegate> { action });
            }
            else
            {
                _events[eventName].Add(action);
            }

            Logger.Debug($"Registering [ClientEvent]: {eventName} on method: {action.Method.Name}.");
        }

        //private void UnregisterEvent(string eventName)
        //{
        //    if (_events.ContainsKey(eventName))
        //    {
        //        _events.Remove(eventName);

        //        Logger.Debug($"Removing [ClientEvent]: {eventName}");
        //    }
        //    else
        //    {
        //        Logger.Debug($"Unable to remove [ClientEvent]: {eventName}");
        //    }
        //}

        internal void RegisterInternalEvent(ClientEventAttribute eventAttr, object classObj, MethodInfo method)
        {
            RegisterEvent(eventAttr.Event, Delegate.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method));
        }

        public void Emit(string eventName, params object[] args)
        {
            try
            {
                if (_events.ContainsKey(eventName))
                {
                    _events[eventName].ForEach(x => x.DynamicInvoke(args));

                    for(int i = 0; i < _events.Count; i++)
                    {
                        var e = _events.ElementAt(i);
                        Logger.Warn("Emit event: " + e.Key + ", " + e.Value.Count);

                        for(int o = 0; o < e.Value.Count; o++)
                        {
                            var del = e.Value.ElementAt(o);
                            Logger.Warn("Emit delegate: " + del.Method.Name + ", " + del.Method.GetParameters().Count());

                            foreach(var param in del.Method.GetParameters())
                            {
                                Logger.Warn("param: " + param.Name + ", " + param.ParameterType);
                            }
                        }
                    }
                }
            }
            catch
            {
                Logger.Error("Unable to call event: " + eventName + ", " + string.Join(", ", args.GetType()));
            }
        }

        public void EmitServer(string eventName, params object[] args)
        {
            BaseScript.TriggerServerEvent("client-event:triggered", eventName, args);
        }

        private void OnTriggerEvent(string eventName, List<object> args)
        {
            Logger.Debug("Receive event from server: " + eventName + ", " + string.Join(", ", args));

            Emit(eventName, args.ToArray());
        }

        #region Internal Events

        private void OnGameEventTriggered(string name, int[] data)
        {
            GameEventTriggered?.Invoke(this, new GameEventTriggeredEventArgs(name, data));
            EmitServer("client:game_event", name, data);
        }

        private void OnResourceStart(string resource)
        {
            ResourceStart?.Invoke(this, new ResourceStartEventArgs(resource));
            EmitServer("client:resource_start", resource);
        }

        private void OnResourceStop(string resource)
        {
            ResourceStop?.Invoke(this, new ResourceStopEventArgs(resource));
            EmitServer("client:resource_stop", resource);
        }

        private void OnClientResourceStart(string resource)
        {
            ClientResourceStart?.Invoke(null, new ClientResourceStartEventArgs(resource));
            EmitServer("client:client_resource_start", resource);
        }

        private void OnClientResourceStop(string resource)
        {
            ClientResourceStop?.Invoke(this, new ClientResourceStopEventArgs(resource));
            EmitServer("client:client_resource_stop", resource);
        }

        private void OnResourceStarting(string resource)
        {
            ResourceStarting?.Invoke(this, new ResourceStartingEventArgs(resource));
            EmitServer("client:resource_starting", resource);
        }

        private void OnClientMapStart(string resource)
        {
            ClientMapStart?.Invoke(this, new ClientMapStartEventArgs(resource));
            EmitServer("client:client_map_start", resource);
        }

        private void OnClientMapStop(string resource)
        {
            ClientMapStop?.Invoke(this, new ClientMapStopEventArgs(resource));
            EmitServer("client:client_map_stop", resource);
        }

        private void OnClientGameTypeStart(string resource)
        {
            ClientGameTypeStart?.Invoke(this, new ClientMapGameTypeStartEventArgs(resource));
            EmitServer("client:client_game_type_start", resource);
        }

        private void OnClientGameTypeStop(string resource)
        {
            ClientGameTypeStop?.Invoke(this, new ClientMapGameTypeStopEventArgs(resource));
            EmitServer("client:client_game_type_stop", resource);
        }

        private void OnPlayerActivated()
        {
            PlayerActivated?.Invoke(this, new PlayerActivatedEventArgs());
            EmitServer("client:player_activated");
        }

        private void OnSessionInitialized()
        {
            SessionInitialized?.Invoke(this, new SessionInitializedEventArgs());
            EmitServer("client:session_initialized");
        }

        private void OnPopulationPedCreating(float x, float y, float z, uint model, dynamic overrideCalls)
        {
            PopulationPedCreating?.Invoke(this, new PopulationPedCreatingEventArgs(new Vector3(x, y, z), model, overrideCalls));
            Emit("client:population_ped_creating", new PopulationPedCreatingEventArgs(new Vector3(x, y, z), model, overrideCalls));
        }

        #endregion
    }
}
