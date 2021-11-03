using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
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

        private readonly NotificationService _notificationService;

        public DebugCommand(CharacterService characterService, UIService uiService, WorldService worldService, NotificationService notificationService)
        {
            _characterService = characterService;
            _uiService = uiService;
            _worldService = worldService;
            _notificationService = notificationService;
        }

        [ClientCommand("debug.gotow")]
        private async void OnGotow()
        {
            var waypointCoords = GetWaypointCoords();
            await _characterService.Teleport(PlayerPedId(), waypointCoords);
        }

        [ClientCommand("debug.test", 100000)]
        private async void OnTest()
        {
            var waypointCoords = GetWaypointCoords();
            await _characterService.Teleport(PlayerPedId(), waypointCoords);
        }

        [ClientCommand("debug:create_notification")]
        private void OnCreateNotification(string count, int duration, int fadeInDuration, int fadeOutDuration)
        {
            Logger.Debug("0");
            _notificationService.Schedule(count, "../src/img/cross.png", duration, fadeInDuration, fadeOutDuration);
        }
    }
}
