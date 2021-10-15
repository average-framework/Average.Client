using Average.Client.Framework.Interfaces;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class WorldService : IService
    {
        private readonly RpcService _rpcService;

        public WorldService(RpcService rpcService)
        {
            _rpcService = rpcService;

            _rpcService.OnRequest<float>("world:get_entity_front_of_player", (cb, range) =>
            {
                var raycast = GetTarget(PlayerPedId(), range);
                cb(raycast);
            });
        }
    }
}
