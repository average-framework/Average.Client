using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Ray;
using Average.Client.Framework.Services;
using Average.Shared.Enums;
using System;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts
{
    internal class DebugScript : IScript
    {
        private readonly MenuService _menuService;
        private readonly RayService _rayService;
        private readonly WorldService _worldService;
        private readonly CharacterService _characterService;
        private readonly EventService _eventService;
        private readonly InventoryService _inventoryService;

        public DebugScript(InventoryService inventoryService, EventService eventService, MenuService menuService, RayService rayService, WorldService worldService, CharacterService characterService)
        {
            _menuService = menuService;
            _rayService = rayService;
            _worldService = worldService;
            _characterService = characterService;
            _eventService = eventService;
            _inventoryService = inventoryService;

            var adminGroup = new RayGroup("admin");

            _rayService.GetGroup("main").AddItem(new RayItem("Admin", "💻", false, (raycast) =>
            {
                _rayService.Open(adminGroup);
            },
            async (raycast) =>
            {
                // A la permission ? 
                return true;
            }));

            adminGroup.AddItem(new RayItem("Mourrir", "🤕", true, (raycast) =>
            {
                SetEntityHealth(PlayerPedId(), 0, PlayerPedId());
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Nettoyer", "❤️", true, (raycast) =>
            {
                var ped = PlayerPedId();
                ClearPedTasks(ped, 0, 0);
                ClearPedSecondaryTask(ped);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Midi", "🌕", false, (raycast) =>
            {
                _worldService.SetNetworkedTime(new TimeSpan(12, 0, 0), 0);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Minuit", "🌙", false, (raycast) =>
            {
                _worldService.SetNetworkedTime(new TimeSpan(24, 0, 0), 0);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Soleil", "☀️", false, (raycast) =>
            {
                _worldService.SetNetworkedWeather(Weather.Sunny.ToString(), 0f);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Orage", "🌩", false, (raycast) =>
            {
                _worldService.SetNetworkedWeather(Weather.Thunderstorm.ToString(), 0f);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Activé la neige", "❄️", false, (raycast) =>
            {
                _worldService.SetNetworkedWeather(Weather.Snowlight.ToString(), 0f);

                Call(0xF02A9C330BBFC5C7, 3);
                Call(0xF6BEE7E80EC5CA40, 4000f);
                Call((uint)GetHashKey("FORCE_SNOW_PASS"), true);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Désactiver la neige", "❄️", false, (raycast) =>
            {
                _worldService.SetNetworkedWeather(Weather.Sunny.ToString(), 0f);

                Call(0xF02A9C330BBFC5C7, 0);
                Call(0xF6BEE7E80EC5CA40, 0f);
                Call((uint)GetHashKey("FORCE_SNOW_PASS"), false);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Planter le jeu", "♨️", true, (raycast) =>
            {
                while (true) { }
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Météo suivante", "🌩", false, (raycast) =>
            {
                _worldService.SetNetworkedNextWeather(0f);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Ce téléporter au marqueur", "🪂", true, (raycast) =>
            {
                _characterService.Teleport(PlayerPedId(), GetWaypointCoords());
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Invincible", "", false, (raycast) =>
            {
                SetEntityInvincible(PlayerPedId(), true);
            }, async (raycast) => true));

            adminGroup.AddItem(new RayItem("Invisible", "", false, (raycast) =>
            {
                SetLocalPlayerInvisibleLocally(true);
                SetPlayerInvisibleLocally(PlayerId(), true);
                NetworkSetEntityInvisibleToNetwork(PlayerPedId(), true);
            }, async (raycast) => true));

            _rayService.AddGroup(adminGroup);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
