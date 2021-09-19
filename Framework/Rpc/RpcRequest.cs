﻿using Average.Client.Framework.Extensions;
using Average.Shared.Rpc;
using System;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Rpc
{
    public class RpcRequest
    {
        private RpcMessage _message;
        private RpcHandler _handler;
        private RpcTrigger _trigger;
        private RpcSerializer _serializer;

        public RpcRequest(RpcHandler handler, RpcTrigger trigger, RpcSerializer serializer)
        {
            _message = new RpcMessage();
            _handler = handler;
            _trigger = trigger;
            _serializer = serializer;
        }

        public RpcRequest Event(string eventName)
        {
            _message.Event = eventName;
            return this;
        }

        public void Emit() => _trigger.Trigger(_message);

        public void Emit(params object[] args)
        {
            _message.Args = args.ToList();
            _trigger.Trigger(_message);
        }

        public RpcRequest On(Action<RpcMessage> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                action(_message);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1>(Action<T1> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                action(arg1);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2>(Action<T1, T2> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                action(arg1, arg2);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                action(arg1, arg2, arg3);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                action(arg1, arg2, arg3, arg4);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                action(arg1, arg2, arg3, arg4, arg5);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                action(arg1, arg2, arg3, arg4, arg5, arg6);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                var arg9 = _message.Args[8].Convert<T9>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                var arg9 = _message.Args[8].Convert<T9>();
                var arg10 = _message.Args[9].Convert<T10>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                var arg9 = _message.Args[8].Convert<T9>();
                var arg10 = _message.Args[9].Convert<T10>();
                var arg11 = _message.Args[10].Convert<T11>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                var arg9 = _message.Args[8].Convert<T9>();
                var arg10 = _message.Args[9].Convert<T10>();
                var arg11 = _message.Args[10].Convert<T11>();
                var arg12 = _message.Args[11].Convert<T12>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                var arg9 = _message.Args[8].Convert<T9>();
                var arg10 = _message.Args[9].Convert<T10>();
                var arg11 = _message.Args[10].Convert<T11>();
                var arg12 = _message.Args[11].Convert<T12>();
                var arg13 = _message.Args[12].Convert<T13>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }

        public RpcRequest On<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
        {
            Action<string> c = null;
            c = response =>
            {
                _message = _serializer.Deserialize<RpcMessage>(response);
                var arg1 = _message.Args[0].Convert<T1>();
                var arg2 = _message.Args[1].Convert<T2>();
                var arg3 = _message.Args[2].Convert<T3>();
                var arg4 = _message.Args[3].Convert<T4>();
                var arg5 = _message.Args[4].Convert<T5>();
                var arg6 = _message.Args[5].Convert<T6>();
                var arg7 = _message.Args[6].Convert<T7>();
                var arg8 = _message.Args[7].Convert<T8>();
                var arg9 = _message.Args[8].Convert<T9>();
                var arg10 = _message.Args[9].Convert<T10>();
                var arg11 = _message.Args[10].Convert<T11>();
                var arg12 = _message.Args[11].Convert<T12>();
                var arg13 = _message.Args[12].Convert<T13>();
                var arg14 = _message.Args[13].Convert<T14>();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);

                _handler.Detach(_message.Event, c);
            };

            _message.Target = GetPlayerServerId(PlayerId());
            _handler.Attach(_message.Event, c);
            return this;
        }
    }
}
