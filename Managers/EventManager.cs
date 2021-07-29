using CitizenFX.Core;
using SDK.Client.Diagnostics;
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
    }
}
