using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Commands
{
    internal class ObjectStreamingCommand : ICommand
    {
        private readonly ObjectStreamingService _objectStreamingService;

        public ObjectStreamingCommand(ObjectStreamingService objectStreamingService)
        {
            _objectStreamingService = objectStreamingService;
        }

        [ClientCommand("debug:create_obj", 10000)]
        private void CreateCommand()
        {
            var pos = GetEntityCoords(PlayerPedId(), true, true);
            _objectStreamingService.CreateRegisteredEntity((uint)GetHashKey("p_waterpump01x"), pos, Vector3.Zero, true);
        }

        [ClientCommand("debug:mass_create", 10000)]
        private void MassCreateCommand(int propsNumber)
        {
            var pos = GetEntityCoords(PlayerPedId(), true, true);

            for (int i = 0; i < propsNumber; i++)
            {
                pos += new Vector3(2f, 0f, 0f);
                _objectStreamingService.CreateRegisteredEntity((uint)GetHashKey("p_waterpump01x"), pos, Vector3.Zero, true);

                Logger.Warn($"Create props {i} on {pos}");
            }
        }
    }
}
