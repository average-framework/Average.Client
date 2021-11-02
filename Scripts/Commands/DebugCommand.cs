using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts.Commands
{
    internal class DebugCommand : ICommand
    {
        private readonly CharacterService _characterService;
        private readonly UIService _uiService;
        private readonly WorldService _worldService;

        public DebugCommand(CharacterService characterService, UIService uiService, WorldService worldService)
        {
            _characterService = characterService;
            _uiService = uiService;
            _worldService = worldService;
        }

        [ClientCommand("debug.gotow")]
        private async void OnGotow()
        {
            var waypointCoords = GetWaypointCoords();
            await _characterService.Teleport(PlayerPedId(), waypointCoords);
        }
    }
}
