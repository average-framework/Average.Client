﻿using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
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
                        else
                        {
                            if (str.StartsWith("player:"))
                            {
                                var playerId = int.Parse(@str.Substring(7));

                                // This argument is an player
                                newArgs.Add(playerId);
                            }
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
                        break;
                    case ulong:
                        Call((ulong)native, newArgs.ToArray());
                        break;
                }

                Logger.Debug("Native Call: " + native + ", " + string.Join(", ", args));
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
