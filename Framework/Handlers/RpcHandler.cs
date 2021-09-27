using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Handlers
{
    internal class RpcHandler : IHandler
    {
        public RpcHandler()
        {

        }

        [ClientEvent("rpc:native_call")]
        private void OnNativeCall(uint native, params object[] args)
        {
            var newArgs = new List<InputArgument>();
            
            foreach(var arg in args)
            {
                Logger.Debug("Convert arg to type: " + arg.GetType());
                var inputArg = (InputArgument)Convert.ChangeType(arg, arg.GetType());
                newArgs.Add(inputArg);
            }

            Call(native, newArgs.ToArray());
        }
    }
}
