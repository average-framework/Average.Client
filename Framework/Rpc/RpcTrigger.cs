using Average.Shared.Rpc;
using CitizenFX.Core;

namespace Average.Client.Framework.Rpc
{
    public class RpcTrigger
    {
        public void Trigger(RpcMessage message)
        {
            BaseScript.TriggerServerEvent(message.Event, message.ToJson());
        }
    }
}
