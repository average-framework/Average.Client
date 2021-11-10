using Average.Client.Framework.Attributes;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using static Average.Client.Framework.GameAPI;
using static Average.Client.Framework.Services.NotificationService;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts.Commands
{
    internal class DebugCommand : ICommand
    {
        private readonly CharacterService _characterService;
        private readonly WorldService _worldService;

        private readonly NotificationService _notificationService;

        public DebugCommand(CharacterService characterService, WorldService worldService, NotificationService notificationService)
        {
            _characterService = characterService;
            _worldService = worldService;
            _notificationService = notificationService;
        }

        [ClientCommand("debug.gotow", 100)]
        private async void OnGotow()
        {
            await _characterService.Teleport(PlayerPedId(), GetWaypointCoords());
        }

        [ClientCommand("debug:create_not_ico", 1000)]
        private void OnCreateNotificationIco(int count, int duration, int fadeInDuration, int fadeOutDuration)
        {
            _notificationService.Schedule(new NotificationIcoModel(NotificationCountType.Increase, count, "../src/img/inventory_items/consumable_herb_ginseng.png", duration, fadeInDuration, fadeOutDuration));
        }

        [ClientCommand("debug:create_not_store", 1000)]
        private void OnCreateNotificationStore(string text, int count, int duration, int fadeInDuration, int fadeOutDuration)
        {
            _notificationService.Schedule(new NotificationStoreModel(NotificationCountType.Increase, text, count, duration, fadeInDuration, fadeOutDuration));
        }

        [ClientCommand("debug:create_not_helptext", 1000)]
        private void OnCreateNotificationHelptext(string text, int duration, int fadeInDuration, int fadeOutDuration)
        {
            _notificationService.Schedule(new NotificationHelpTextModel(text, duration, fadeInDuration, fadeOutDuration));
        }

        [ClientCommand("debug:vs", 1000)]
        private void OnVisualSettings(string settingName, float value)
        {
            SetVisualSettings(settingName, value);
        }

        [ClientCommand("debug:ped_config_flag")]
        private void OnPedConfigFlag(int config, bool enabled)
        {
            Call(0x1913FE4CBF41C463, PlayerPedId(), config, enabled);
        }
    }
}
