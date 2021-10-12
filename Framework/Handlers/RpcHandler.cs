using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Handlers
{
    internal class RpcHandler : IHandler
    {
        private readonly RpcService _rpcService;

        public RpcHandler(RpcService rpcService)
        {
            _rpcService = rpcService;

            rpcService.OnRequest<object, string, List<object>>("rpc:native_call_result", (cb, native, resultType, args) =>
            {
                var newArgs = new List<InputArgument>();

                try
                {
                    foreach (var arg in args)
                    {
                        if (arg is string @str)
                        {
                            if (str.StartsWith("netid:"))
                            {
                                var netId = int.Parse(@str.Substring(6));

                                // This argument is an networked entity
                                var networkIdExists = NetworkDoesNetworkIdExist(netId);

                                if (networkIdExists)
                                {
                                    var entity = NetworkGetEntityFromNetworkId(netId);
                                    newArgs.Add(entity);
                                }
                                else
                                {
                                    newArgs.Add(arg.ToInputArgument());
                                }
                            }
                            else if (str.StartsWith("player:"))
                            {
                                var playerId = int.Parse(@str.Substring(7));

                                // This argument is an player
                                newArgs.Add(playerId);
                            }
                            else
                            {
                                newArgs.Add(arg.ToInputArgument());
                            }
                        }
                        else
                        {
                            newArgs.Add(arg.ToInputArgument());
                        }
                    }

                    var type = Type.GetType(resultType);

                    switch (native)
                    {
                        case long:
                            dynamic result = null;

                            if (type == typeof(bool))
                                result = RpcCall((long)native, default(bool), newArgs.ToArray());
                            else if (type == typeof(sbyte))
                                result = RpcCall((long)native, default(sbyte), newArgs.ToArray());
                            else if (type == typeof(byte))
                                result = RpcCall((long)native, default(byte), newArgs.ToArray());
                            else if (type == typeof(short))
                                result = RpcCall((long)native, default(short), newArgs.ToArray());
                            else if (type == typeof(ushort))
                                result = RpcCall((long)native, default(ushort), newArgs.ToArray());
                            else if (type == typeof(int))
                                result = RpcCall((long)native, default(int), newArgs.ToArray());
                            else if (type == typeof(uint))
                                result = RpcCall((long)native, default(uint), newArgs.ToArray());
                            else if (type == typeof(long))
                                result = RpcCall((long)native, default(long), newArgs.ToArray());
                            else if (type == typeof(ulong))
                                result = RpcCall((long)native, default(ulong), newArgs.ToArray());
                            else if (type == typeof(float))
                                result = RpcCall((long)native, default(float), newArgs.ToArray());
                            else if (type == typeof(double))
                                result = RpcCall((long)native, default(double), newArgs.ToArray());
                            else if (type == typeof(string))
                                result = RpcCall((long)native, default(string), newArgs.ToArray());
                            else if (type == typeof(Vector3))
                                result = RpcCall((long)native, default(Vector3), newArgs.ToArray());

                            cb(result);
                            break;
                        case ulong:
                            result = null;

                            if (type == typeof(bool))
                                result = RpcCall((ulong)native, default(bool), newArgs.ToArray());
                            else if (type == typeof(sbyte))
                                result = RpcCall((ulong)native, default(sbyte), newArgs.ToArray());
                            else if (type == typeof(byte))
                                result = RpcCall((ulong)native, default(byte), newArgs.ToArray());
                            else if (type == typeof(short))
                                result = RpcCall((ulong)native, default(short), newArgs.ToArray());
                            else if (type == typeof(ushort))
                                result = RpcCall((ulong)native, default(ushort), newArgs.ToArray());
                            else if (type == typeof(int))
                                result = RpcCall((ulong)native, default(int), newArgs.ToArray());
                            else if (type == typeof(uint))
                                result = RpcCall((ulong)native, default(uint), newArgs.ToArray());
                            else if (type == typeof(long))
                                result = RpcCall((ulong)native, default(long), newArgs.ToArray());
                            else if (type == typeof(ulong))
                                result = RpcCall((ulong)native, default(ulong), newArgs.ToArray());
                            else if (type == typeof(float))
                                result = RpcCall((ulong)native, default(float), newArgs.ToArray());
                            else if (type == typeof(double))
                                result = RpcCall((ulong)native, default(double), newArgs.ToArray());
                            else if (type == typeof(string))
                                result = RpcCall((ulong)native, default(string), newArgs.ToArray());
                            else if (type == typeof(Vector3))
                                result = RpcCall((ulong)native, default(Vector3), newArgs.ToArray());

                            cb(result);
                            break;
                    }

                    //Logger.Debug("Native Call: " + native + ", " + string.Join(", ", args));
                }
                catch (Exception ex)
                {
                    switch (native)
                    {
                        case long:
                            Logger.Error($"Unable to call native: {(Hash)native}. Error: {ex.Message}\n{ex.StackTrace}.");
                            break;
                        case ulong:
                            Logger.Error($"Unable to call native: {(ulong)native:X8}. Error: {ex.Message}\n{ex.StackTrace}.");
                            break;
                    }
                }
            });
        }

        [ClientEvent("rpc:send_response")]
        private void OnReceiveResponse(string @event, string response)
        {
            _rpcService.TriggerResponse(@event, response);
        }

        [ClientEvent("rpc:trigger_event")]
        private void OnTriggerEvent(string @event, string request)
        {
            _rpcService.OnInternalRequest(@event, request);
        }

        [ClientEvent("rpc:native_call")]
        private void OnNativeCall(object native, List<object> args)
        {
            var newArgs = new List<InputArgument>();

            try
            {
                foreach (var arg in args)
                {
                    if (arg is string @str)
                    {
                        if (str.StartsWith("netid:"))
                        {
                            var netId = int.Parse(@str.Substring(6));

                            // This argument is an networked entity
                            var networkIdExists = NetworkDoesNetworkIdExist(netId);

                            if (networkIdExists)
                            {
                                var entity = NetworkGetEntityFromNetworkId(netId);
                                newArgs.Add(entity);
                            }
                            else
                            {
                                newArgs.Add(arg.ToInputArgument());
                            }
                        }
                        else if (str.StartsWith("player:"))
                        {
                            var playerId = int.Parse(@str.Substring(7));

                            // This argument is an player
                            newArgs.Add(playerId);
                        }
                        else
                        {
                            newArgs.Add(arg.ToInputArgument());
                        }
                    }
                    else
                    {
                        newArgs.Add(arg.ToInputArgument());
                    }
                }

                switch (native)
                {
                    case long:
                        Call((long)native, newArgs.ToArray());
                        //Logger.Debug("long: " + (Hash)(long)native);
                        break;
                    case ulong:
                        Call((ulong)native, newArgs.ToArray());
                        //Logger.Debug("ulong: " + (Hash)(ulong)native);
                        break;
                }

                //Logger.Debug("Native Call: " + native + ", " + string.Join(", ", newArgs));
            }
            catch (Exception ex)
            {
                switch (native)
                {
                    case long:
                        Logger.Error($"Unable to call native: {(Hash)native}. Error: {ex.Message}\n{ex.StackTrace}.");
                        break;
                    case ulong:
                        Logger.Error($"Unable to call native: {(ulong)native:X8}. Error: {ex.Message}\n{ex.StackTrace}.");
                        break;
                }
            }
        }
    }
}
