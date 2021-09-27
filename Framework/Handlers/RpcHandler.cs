using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Handlers
{
    internal class RpcHandler : IHandler
    {
        public RpcHandler()
        {

        }

        [ClientEvent("rpc:native_call")]
        private void OnNativeCall(object native, List<object> args)
        {
            Logger.Debug("Native Call: " + native + ", " + native.GetType());

            var newArgs = new List<InputArgument>();

            try
            {
                foreach (var arg in args)
                {
                    if (arg is int @int)
                    {
                        var networkIdExists = NetworkDoesNetworkIdExist(@int);

                        if (networkIdExists)
                        {
                            // This argument is an networked entity
                            var entity = NetworkGetEntityFromNetworkId(@int);
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

                Logger.Debug("Execute: " + string.Join(", ", newArgs));

                switch (native)
                {
                    case long:
                        Call((long)native, newArgs.ToArray());
                        Logger.Debug("long");
                        break;
                    case ulong:
                        Call((ulong)native, newArgs.ToArray());
                        Logger.Debug("ulong");
                        break;
                }
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
