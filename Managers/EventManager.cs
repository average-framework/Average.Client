﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Events;
using SDK.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SDK.Shared;

namespace Average.Client.Managers
{
    public class EventManager : InternalPlugin, IEventManager
    {
        private static Dictionary<string, List<Delegate>> _events = new();
        private static Dictionary<string, List<Delegate>> _nuiEvents = new();

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

        public override void OnInitialized()
        {
            #region Event

            Main.eventHandlers["avg.internal.trigger_event"] += new Action<string, List<object>>(InternalTriggerEvent);

            #endregion
        }

        public void Emit(string eventName, params object[] args)
        {
            if (_events.ContainsKey(eventName))
            {
                // Log.Debug($"Calling event: {eventName}.");
                _events[eventName].ForEach(x => x.DynamicInvoke(args));
            }
            else
            {
                // Log.Debug($"Calling external event: {eventName}.");
                BaseScript.TriggerEvent(eventName, args);
            }
        }

        public void EmitServer(string eventName, params object[] args)
        {
            BaseScript.TriggerServerEvent("avg.internal.trigger_event", eventName, args);
        }

        public static void RegisterInternalNuiCallbackEvent(string eventName, Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate> callback)
        {
            API.RegisterNuiCallbackType(eventName);
            Main.eventHandlers[$"__cfx_nui:{eventName}"] += new Action<IDictionary<string, object>, CallbackDelegate>((body, resultCallback) => callback.Invoke(body, resultCallback));
        }

        internal static void RegisterInternalNuiCallbackEvent(MethodInfo method, UICallbackAttribute eventAttr, object classObj)
        {
            var methodParams = method.GetParameters();
            var callback = (Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate>)Delegate.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);
            
            if (!_nuiEvents.ContainsKey(eventAttr.Name))
                _nuiEvents.Add(eventAttr.Name, new List<Delegate> { callback });
            else
                _nuiEvents[eventAttr.Name].Add(callback);
            
            API.RegisterNuiCallbackType(eventAttr.Name);
            Main.eventHandlers[$"__cfx_nui:{eventAttr.Name}"] += new Action<IDictionary<string, object>, CallbackDelegate>((body, resultCallback) => callback.Invoke(body, resultCallback));
            
            Log.Debug($"Registering [UICallback] attribute: {eventAttr.Name} on method: {method.Name}, args count: {methodParams.Count()}");
        }
        
        public void UnregisterInternalEvent(string eventName)
        {
            if (_events.ContainsKey(eventName))
            {
                _events.Remove(eventName);
                Log.Debug($"Unregister event: {eventName}");
            }
            else
            {
                Log.Error($"Unable to unregister event: {eventName}.");
            }
        }

        public void UnregisterInternalEventAction(string eventName, Delegate action)
        {
            if (_events.ContainsKey(eventName) && _events[eventName].Contains(action))
            {
                _events[eventName].Remove(action);
                Log.Debug($"Unregister event action: {eventName}");
            }
            else
            {
                Log.Error($"Unable to unregister event action: {eventName}.");
            }
        }

        internal static void RegisterInternalEvent(MethodInfo method, ClientEventAttribute eventAttr, object classObj)
        {
            var methodParams = method.GetParameters();
            var action = Delegate.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);

            if (!_events.ContainsKey(eventAttr.Event))
                _events.Add(eventAttr.Event, new List<Delegate> { action });
            else
                _events[eventAttr.Event].Add(action);
            
            Log.Debug($"Registering [Event] attribute: {eventAttr.Event} on method: {method.Name}, args count: {methodParams.Count()}");
        }

        #region Internal

        private void InternalTriggerEvent(string eventName, List<object> args) => Emit(eventName, args.ToArray());

        #endregion

        #region Event

        public async void OnGameEventTriggered(string name, int[] data)
        {
            await Main.loader.IsReady();
            GameEventTriggered?.Invoke(this, new GameEventTriggeredEventArgs(name, data));
            Emit("GameEvent", name, data);
        }

        public async void OnResourceStop(string resource)
        {
            await Main.loader.IsReady();
            ResourceStop?.Invoke(this, new ResourceStopEventArgs(resource));
            Emit("ResourceStop", resource);
        }

        public async void OnResourceStart(string resource)
        {
            await Main.loader.IsReady();
            ResourceStart?.Invoke(this, new ResourceStartEventArgs(resource));
            Emit("ResourceStart", resource);
        }

        public async void OnClientResourceStart(string resource)
        {
            await Main.loader.IsReady();
            ClientResourceStart?.Invoke(null, new ClientResourceStartEventArgs(resource));
            Emit("ClientResourceStart", resource);
        }

        public async void OnClientResourceStop(string resource)
        {
            await Main.loader.IsReady();
            ClientResourceStop?.Invoke(this, new ClientResourceStopEventArgs(resource));
            Emit("ClientResourceStop", resource);
        }

        public async void OnResourceStarting(string resource)
        {
            await Main.loader.IsReady();
            ResourceStarting?.Invoke(this, new ResourceStartingEventArgs(resource));
            Emit("ResourceStarting", resource);
        }

        public async void OnClientMapStart(string resource)
        {
            await Main.loader.IsReady();
            ClientMapStart?.Invoke(this, new ClientMapStartEventArgs(resource));
            Emit("ClientMapStart", resource);
        }

        public async void OnClientMapStop(string resource)
        {
            await Main.loader.IsReady();
            ClientMapStop?.Invoke(this, new ClientMapStopEventArgs(resource));
            Emit("ClientMapStop", resource);
        }

        public async void OnClientGameTypeStart(string resource)
        {
            await Main.loader.IsReady();
            ClientGameTypeStart?.Invoke(this, new ClientMapGameTypeStartEventArgs(resource));
            Emit("ClientGameTypeStart", resource);
        }

        public async void OnClientGameTypeStop(string resource)
        {
            await Main.loader.IsReady();
            ClientGameTypeStop?.Invoke(this, new ClientMapGameTypeStopEventArgs(resource));
            Emit("ClientGameTypeStop", resource);
        }

        public async void OnPlayerActivated()
        {
            await Main.loader.IsReady();
            PlayerActivated?.Invoke(this, new PlayerActivatedEventArgs());
            Emit("PlayerActivated");
        }

        public async void OnSessionInitialized()
        {
            await Main.loader.IsReady();
            SessionInitialized?.Invoke(this, new SessionInitializedEventArgs());
            Emit("SessionInitialized");
        }

        #endregion
    }
}
