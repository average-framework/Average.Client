using CitizenFX.Core;
using SDK.Client.Diagnostics;
using SDK.Client.Events;
using SDK.Client.Interfaces;
using SDK.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Average.Managers
{
    public class EventManager : IEventManager
    {
        Dictionary<string, Delegate> events;
        Logger logger;

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

        public EventManager(EventHandlerDictionary eventHandlers, Logger logger)
        {
            this.logger = logger;
            events = new Dictionary<string, Delegate>();

            eventHandlers["avg.internal.trigger_event"] += new Action<string, List<object>>(InternalTriggerEvent);
        }

        public void Emit(string eventName, params object[] args)
        {
            if (events.ContainsKey(eventName))
            {
                events[eventName].DynamicInvoke(args);
            }
        }

        public void EmitServer(string eventName, params object[] args)
        {
            BaseScript.TriggerServerEvent("avg.internal.trigger_event", eventName, args);
        }

        public void RegisterInternalEvent(string eventName, Delegate action)
        {
            if (!events.ContainsKey(eventName))
            {
                events.Add(eventName, action);
                logger.Debug($"Register event: {eventName}");
            }
            else
            {
                logger.Error($"Unable to register event: {eventName}, an event have already been registered with this event name.");
            }
        }

        public void UnregisterInternalEvent(string eventName, Delegate action)
        {
            if (events.ContainsKey(eventName))
            {
                events.Remove(eventName);
                logger.Debug($"Unregister event: {eventName}");
            }
            else
            {
                logger.Error($"Unable to unregister event: {eventName}.");
            }
        }

        public void RegisterEvent(MethodInfo method, EventAttribute eventAttr, object classObj)
        {
            var methodParams = method.GetParameters();

            if (!events.ContainsKey(eventAttr.Event))
            {
                var action = Action.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);
                events.Add(eventAttr.Event, action);

                logger.Debug($"Registering [Event] attribute: {eventAttr.Event} on method: {method.Name}, args count: {methodParams.Count()}");
            }
            else
            {
                logger.Error($"Unable to register [Event] attribute: {eventAttr.Event} on method: {method.Name}, an event have already been registered with this event name.");
            }
        }

        #region Internal

        internal void InternalTriggerEvent(string eventName, List<object> args)
        {
            Emit(eventName, args.ToArray());
        }

        #endregion

        #region Events

        public void OnGameEventTriggered(string name, int[] data)
        {
            if (GameEventTriggered != null)
            {
                GameEventTriggered(null, new GameEventTriggeredEventArgs(name, data));
            }
        }

        public void OnResourceStop(string resource)
        {
            if (ResourceStop != null)
            {
                ResourceStop(null, new ResourceStopEventArgs(resource));
            }
        }

        public void OnResourceStart(string resource)
        {
            if (ResourceStart != null)
            {
                ResourceStart(null, new ResourceStartEventArgs(resource));
            }
        }

        public void OnClientResourceStart(string resource)
        {
            if (ClientResourceStart != null)
            {
                ClientResourceStart(null, new ClientResourceStartEventArgs(resource));
            }
        }

        public void OnClientResourceStop(string resource)
        {
            if (ClientResourceStop != null)
            {
                ClientResourceStop(null, new ClientResourceStopEventArgs(resource));
            }
        }

        public void OnResourceStarting(string resource)
        {
            if (ResourceStarting != null)
            {
                ResourceStarting(null, new ResourceStartingEventArgs(resource));
            }
        }

        public void OnClientMapStart(string resource)
        {
            if (ClientMapStart != null)
            {
                ClientMapStart(null, new ClientMapStartEventArgs(resource));
            }
        }

        public void OnClientMapStop(string resource)
        {
            if (ClientMapStop != null)
            {
                ClientMapStop(null, new ClientMapStopEventArgs(resource));
            }
        }

        public void OnClientGameTypeStart(string resource)
        {
            if (ClientGameTypeStart != null)
            {
                ClientGameTypeStart(null, new ClientMapGameTypeStartEventArgs(resource));
            }
        }

        public void OnClientGameTypeStop(string resource)
        {
            if (ClientGameTypeStop != null)
            {
                ClientGameTypeStop(null, new ClientMapGameTypeStopEventArgs(resource));
            }
        }

        public void OnPlayerActivated()
        {
            if (PlayerActivated != null)
            {
                PlayerActivated(null, new PlayerActivatedEventArgs());
            }
        }

        public void OnSessionInitialized()
        {
            if (SessionInitialized != null)
            {
                SessionInitialized(null, new SessionInitializedEventArgs());
            }
        }

        #endregion
    }
}
