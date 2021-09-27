using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Handlers
{
    internal class RpcHandler : IHandler
    {
        public RpcHandler()
        {

        }

        [ClientEvent("rpc:native_call")]
        private void OnNativeCall(long native, List<object> args)
        {
            var newArgs = new List<InputArgument>();

            try
            {
                foreach (var arg in args)
                {
                    if (arg is int @int)
                    {
                        var networkIdExists = API.NetworkDoesNetworkIdExist(@int);

                        if (networkIdExists)
                        {
                            // This int argument is an networked entity
                            var entity = API.NetworkGetEntityFromNetworkId(@int);
                            newArgs.Add(entity);
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

                Call(native, newArgs.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to call native: {(Hash)native}. Error: {ex.Message}\n{ex.StackTrace}.");
            }
        }
    }
}
