using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Shared.Rpc;
using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Average.Client.Framework.Services
{
    internal class RpcService : IService
    {
        private readonly EventService _eventService;
        private readonly Dictionary<string, Delegate> _events = new();

        internal delegate object RpcCallback(params object[] args);

        private RpcMessage _preparedRequest;

        public RpcService(EventService eventService)
        {
            _eventService = eventService;
        }

        internal void TriggerResponse(string @event, string response)
        {
            if (_events.ContainsKey(@event))
            {
                _events[@event].DynamicInvoke(response);
            }
        }

        public void Emit(params object[] args)
        {
            _preparedRequest.Args = args.ToList();
            _eventService.EmitServer("rpc:trigger_event", _preparedRequest.Event, _preparedRequest.ToJson());
        }

        private void Register(string @event, Delegate callback)
        {
            if (!_events.ContainsKey(@event))
            {
                _events.Add(@event, callback);
            }
        }

        private void Unregister(string @event)
        {
            if (_events.ContainsKey(@event))
            {
                _events.Remove(@event);
            }
        }

        private void OnInternalResponse(string @event, Delegate callback)
        {
            _preparedRequest = new RpcMessage(@event);

            Action<string> action = null;
            action = response =>
            {
                Unregister(@event);

                var message = response.Deserialize<RpcMessage>();
                var @params = callback.Method.GetParameters().ToList();
                var newArgs = new List<object>();

                for (int i = 0; i < message.Args.Count; i++)
                {
                    var arg = message.Args[i];
                    var param = @params[i];

                    if (arg.GetType() != param.ParameterType)
                    {
                        if (arg.GetType() == typeof(JArray))
                        {
                            // Need to convert arg or type JArray to param type if is it not the same
                            var array = arg as JArray;
                            var newArg = array.ToObject(param.ParameterType);
                            newArgs.Add(newArg);
                        }
                        else
                        {
                            // Need to convert arg type to param type if is it not the same
                            var newArg = Convert.ChangeType(arg, param.ParameterType);
                            newArgs.Add(newArg);
                        }
                    }
                    else
                    {
                        newArgs.Add(arg);
                    }
                }

                callback.DynamicInvoke(newArgs.ToArray());
            };

            Register(@event, action);
        }

        internal void OnInternalRequest(string @event, string request)
        {
            var message = request.Deserialize<RpcMessage>();

            if (_events.ContainsKey(@event))
            {
                var newArgs = new List<object>();

                newArgs.Add(new RpcCallback(args =>
                {
                    var response = new RpcMessage();
                    response.Event = @event;
                    response.Args = args.ToList();

                    _eventService.EmitServer("rpc:send_response", @event, response.ToJson());
                    return args;
                }));

                // Need to skip two first args (RpcCallback) args
                var methodParams = _events[@event].Method.GetParameters().Skip(1).ToList();

                for (int i = 0; i < methodParams.Count; i++)
                {
                    var arg = message.Args[i];
                    var param = methodParams[i];

                    if (arg.GetType() != param.ParameterType)
                    {
                        if (arg.GetType() == typeof(JArray))
                        {
                            // Need to convert arg or type JArray to param type if is it not the same
                            var array = arg as JArray;
                            var newArg = array.ToObject(param.ParameterType);
                            newArgs.Add(newArg);
                        }
                        else
                        {
                            // Need to convert arg type to param type if is it not the same
                            var newArg = Convert.ChangeType(arg, param.ParameterType);
                            newArgs.Add(newArg);
                        }
                    }
                    else
                    {
                        newArgs.Add(arg);
                    }
                }

                _events[@event].DynamicInvoke(newArgs.ToArray());
            }
        }

        #region OnResponse<,>

        public RpcService OnResponse(string @event, Action callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1>(string @event, Action<T1> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2>(string @event, Action<T1, T2> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3>(string @event, Action<T1, T2, T3> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4>(string @event, Action<T1, T2, T3, T4> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4, T5>(string @event, Action<T1, T2, T3, T4, T5> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4, T5, T6>(string @event, Action<T1, T2, T3, T4, T5, T6> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4, T5, T6, T7>(string @event, Action<T1, T2, T3, T4, T5, T6, T7> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4, T5, T6, T7, T8>(string @event, Action<T1, T2, T3, T4, T5, T6, T7, T8> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string @event, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        public RpcService OnResponse<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string @event, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
        {
            OnInternalResponse(@event, callback);
            return this;
        }

        #endregion

        #region OnRequest<,>

        public void OnRequest(string @event, Action<RpcCallback> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1>(string @event, Action<RpcCallback, T1> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2>(string @event, Action<RpcCallback, T1, T2> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3>(string @event, Action<RpcCallback, T1, T2, T3> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4>(string @event, Action<RpcCallback, T1, T2, T3, T4> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4, T5>(string @event, Action<RpcCallback, T1, T2, T3, T4, T5> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4, T5, T6>(string @event, Action<RpcCallback, T1, T2, T3, T4, T5, T6> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4, T5, T6, T7>(string @event, Action<RpcCallback, T1, T2, T3, T4, T5, T6, T7> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4, T5, T6, T7, T8>(string @event, Action<RpcCallback, T1, T2, T3, T4, T5, T6, T7, T8> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string @event, Action<RpcCallback, T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
        {
            Register(@event, callback);
        }

        public void OnRequest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string @event, Action<RpcCallback, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
        {
            Register(@event, callback);
        }

        #endregion

        #region Request<,>

        private async Task WaitResponse(object value)
        {
            while (value == null) await BaseScript.Delay(1);
        }

        public async Task<Tuple<T1>> Request<T1>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1>(@event, arg0 =>
            {
                result = Tuple.Create(arg0);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1>;
        }

        public async Task<Tuple<T1, T2>> Request<T1, T2>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1, T2>(@event, (arg0, arg1) =>
            {
                result = Tuple.Create(arg0, arg1);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1, T2>;
        }

        public async Task<Tuple<T1, T2, T3>> Request<T1, T2, T3>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1, T2, T3>(@event, (arg0, arg1, arg2) =>
            {
                result = Tuple.Create(arg0, arg1, arg2);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1, T2, T3>;
        }

        public async Task<Tuple<T1, T2, T3, T4>> Request<T1, T2, T3, T4>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1, T2, T3, T4>(@event, (arg0, arg1, arg2, arg3) =>
            {
                result = Tuple.Create(arg0, arg1, arg2, arg3);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1, T2, T3, T4>;
        }

        public async Task<Tuple<T1, T2, T3, T4, T5>> Request<T1, T2, T3, T4, T5>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1, T2, T3, T4, T5>(@event, (arg0, arg1, arg2, arg3, arg4) =>
            {
                result = Tuple.Create(arg0, arg1, arg2, arg3, arg4);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1, T2, T3, T4, T5>;
        }

        public async Task<Tuple<T1, T2, T3, T4, T5, T6>> Request<T1, T2, T3, T4, T5, T6>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1, T2, T3, T4, T5, T6>(@event, (arg0, arg1, arg2, arg3, arg4, arg5) =>
            {
                result = Tuple.Create(arg0, arg1, arg2, arg3, arg4, arg5);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1, T2, T3, T4, T5, T6>;
        }

        public async Task<Tuple<T1, T2, T3, T4, T5, T6, T7>> Request<T1, T2, T3, T4, T5, T6, T7>(string @event, params object[] args)
        {
            object result = null;

            OnResponse<T1, T2, T3, T4, T5, T6, T7>(@event, (arg0, arg1, arg2, arg3, arg4, arg5, arg6) =>
            {
                result = Tuple.Create(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
            }).Emit(args);

            while (result == null) await BaseScript.Delay(1);
            return result as Tuple<T1, T2, T3, T4, T5, T6, T7>;
        }

        #endregion
    }
}
