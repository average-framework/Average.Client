using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.IoC;
using Average.Shared.Attributes;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class UIService : IService
    {
        private readonly Container _container;
        private readonly EventService _eventService;
        private readonly RpcService _rpcService;

        private readonly Dictionary<string, List<Delegate>> _nuiEvents = new();
        private readonly List<string> _nuiServerEvents = new();

        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private readonly EventHandlerDictionary _eventHandlers;

        public UIService(Container container, EventService eventService, EventHandlerDictionary eventHandlers, RpcService rpcService)
        {
            _container = container;
            _eventService = eventService;
            _eventHandlers = eventHandlers;
            _rpcService = rpcService;
        }

        internal void Reflect()
        {
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            // Register client nui events
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
                        var attr = method.GetCustomAttribute<UICallbackAttribute>();
                        if (attr == null) continue;

                        RegisterInternalUICallbackEvent(attr, _service, method);
                    }
                }
            }
        }

        internal void RegisterUIServerCallback(List<object> events)
        {
            foreach (var @event in events)
            {
                Func<IDictionary<string, object>, CallbackDelegate, Task<CallbackDelegate>> callback = async (body, cb) =>
                {
                    var haveResponse = false;

                    _rpcService.OnResponse<List<object>>(@event.ToString(), async (args) =>
                    {
                        Logger.Error($"Receive UI Redirection from server with data: {string.Join(", ", args)}.");

                        cb(args.ToJson());
                        haveResponse = true;
                    }).Emit(new Dictionary<string, object>(body));

                    while (!haveResponse) await BaseScript.Delay(0);
                    return cb;
                };

                RegisterNuiCallbackType(@event.ToString());
                _eventHandlers[$"__cfx_nui:{@event}"] += new Action<IDictionary<string, object>, CallbackDelegate>(async (body, resultCallback) => await callback.Invoke(body, resultCallback));
            }
        }

        private void RegisterInternalUICallbackEvent(UICallbackAttribute eventAttr, object classObj, MethodInfo method)
        {
            var methodParams = method.GetParameters();
            var callback = (Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate>)Delegate.CreateDelegate(Expression.GetDelegateType((from parameter in method.GetParameters() select parameter.ParameterType).Concat(new[] { method.ReturnType }).ToArray()), classObj, method);

            if (!_nuiEvents.ContainsKey(eventAttr.Name))
            {
                _nuiEvents.Add(eventAttr.Name, new List<Delegate> { callback });
            }
            else
            {
                _nuiEvents[eventAttr.Name].Add(callback);
            }

            RegisterNuiCallbackType(eventAttr.Name);
            _eventHandlers[$"__cfx_nui:{eventAttr.Name}"] += new Action<IDictionary<string, object>, CallbackDelegate>((body, resultCallback) => callback.Invoke(body, resultCallback));

            Logger.Debug($"Registering [UICallback]: {eventAttr.Name} on method: {method.Name}.");
        }

        internal async Task ShutdownLoadingScreen()
        {
            API.ShutdownLoadingScreen();
            while (IsLoadingScreenActive()) await BaseScript.Delay(0);
        }

        internal async Task FadeIn(int duration = 1000)
        {
            await GameAPI.FadeIn(duration);
        }

        internal async Task FadeOut(int duration = 1000)
        {
            await GameAPI.FadeOut(duration);
        }

        internal void FocusFrame(string frame) => Emit(new
        {
            eventName = "ui:focus",
            frame
        });

        internal void Focus(bool showCursor = true)
        {
            SetNuiFocus(true, showCursor);
        }

        internal void Unfocus() => SetNuiFocus(false, false);
        internal async void Emit(object message) => SendNuiMessage(message.ToJson());

        internal void SendNui(string frame, string requestType, object message = null) => Emit(new
        {
            eventName = "ui:emit",
            frame,
            requestType,
            message = message ?? new { }
        });

        internal void LoadFrame(string frame) => Emit(new
        {
            eventName = "ui:load_frame",
            frame
        });

        internal void DestroyFrame(string frame) => Emit(new
        {
            eventName = "ui:destroy_frame",
            frame
        });

        internal void Show(string frame) => Emit(new
        {
            eventName = "ui:show",
            frame
        });

        internal void Hide(string frame) => Emit(new
        {
            eventName = "ui:hide",
            frame
        });

        internal void FadeIn(string frame, int fadeDuration = 100) => Emit(new
        {
            eventName = "ui:fadein",
            frame,
            fade = fadeDuration
        });

        internal void FadeOut(string frame, int fadeDuration = 100) => Emit(new
        {
            eventName = "ui:fadeout",
            frame,
            fade = fadeDuration
        });

        internal void SetZIndex(string frame, int zIndex) => Emit(new
        {
            eventName = "ui:zindex",
            frame,
            zIndex
        });
    }
}
