using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class GameService : IService
    {
        private readonly UIService _uiService;

        public GameService(UIService uiService)
        {
            _uiService = uiService;

            _uiService.Unfocus();

            SetRelationShipBetweenGroups(5, (uint)GetHashKey("PLAYER"), (uint)GetHashKey("PLAYER"));
            SetMinimapHideFow(true);
            SetHudComponents();
            LoadInteriors();
            SetVisualSettings();

            Logger.Debug("GameService Initialized successfully");
        }

        private void SetHudComponents()
        {
            HideHudComponent(HudComponent.SkillCards);
            HideHudComponent(HudComponent.HonorMoneyCards);
            HideHudComponent(HudComponent.ActionWheelItems);
            HideHudComponent(HudComponent.MPMoney);
            HideHudComponent(HudComponent.UnkSpMoney);
            HideHudComponent(HudComponent.Minimap);
        }

        private void LoadInteriors()
        {
            // Bank
            LoadInterior(12290, "val_bank_front_windows");
            // Saloon
            LoadInterior(21250, "front_windows");
            LoadInterior(21250, "6_chair_poker_set");
            // Hotel
            LoadInterior(37122, "MUD3_val_hotel_room1b_curtain");
            LoadInterior(37122, "MUD3_val_hotel_room1b_prayercurtain");
            LoadInterior(37122, "LevDes_Val_MUD3");
            LoadInterior(37122, "mud3_val_hotel_room_curtain01");
            LoadInterior(37122, "val_hotel_int_rentSign");
            // General Store
            LoadInterior(45826, "val_genstore_night_light");
            // Doctor
            LoadInterior(1026, "_s_candyBag01x_red_group");
            LoadInterior(1026, "_s_inv_medicine01x_dressing");
            LoadInterior(1026, "_s_inv_medicine01x_group");
            LoadInterior(1026, "_s_inv_tabacco01x_dressing");
            LoadInterior(1026, "_s_inv_tabacco01x_group");
            // Sheriff
            LoadInterior(7170, "val_jail_int_walla");
            LoadInterior(7170, "val_jail_int_wallb");
            // Smuggl
            LoadInterior(77313, "mp006_mshine_hidden_door");
            UnloadInterior(77313, "mp006_mshine_hidden_door_open");
            UnloadInterior(77313, "mp006_mshine_band2");
            LoadInterior(77313, "mp006_mshine_band1c");
            LoadInterior(77313, "mp006_mshine_bar_benchAndFrame");
            UnloadInterior(77313, "mp006_mshine_dressing_1");
            LoadInterior(77313, "mp006_mshine_hidden_door_open");
            UnloadInterior(77313, "mp006_mshine_location1");
            UnloadInterior(77313, "mp006_mshine_pic_02");
            LoadInterior(77313, "mp006_mshine_shelfwall1");
            LoadInterior(77313, "mp006_mshine_shelfwall2");
            LoadInterior(77313, "mp006_mshine_Still_02");
            LoadInterior(77313, "mp006_mshine_still_hatch");
            LoadInterior(77313, "mp006_mshine_theme_goth");
        }

        private void SetVisualSettings()
        {
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.A", 0.1f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.B", 0.015f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.C", 0.65f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.D", 0.1f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.E", 0.115f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.F", 0.055f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.W", 11.2f);
            GameAPI.SetVisualSettings("Tonemapping.bright.filmic.W.HDR", 1.0f);
            GameAPI.SetVisualSettings("Tonemapping.dark.filmic.A", 0.1f);
            GameAPI.SetVisualSettings("Tonemapping.dark.filmic.B", 0.025f);
            GameAPI.SetVisualSettings("misc.SunRise.StartHour", 5.0f);
            GameAPI.SetVisualSettings("misc.SunRise.EndHour", 7.0f);
            GameAPI.SetVisualSettings("misc.SunSet.StartHour", 17.0f);
            GameAPI.SetVisualSettings("misc.SunSet.EndHour", 20.0f);
            GameAPI.SetVisualSettings("misc.SunRise.Light.StartHour", 5.0f);
            GameAPI.SetVisualSettings("misc.SunSet.Light.StartHour", 20.0f);
            GameAPI.SetVisualSettings("sharpen.intensity", 0.4f);
            GameAPI.SetVisualSettings("sky.MoonIntensity", 0.04f);
            GameAPI.SetVisualSettings("stars.dense.intensity", 1.0f);
            GameAPI.SetVisualSettings("stars.sparse.intensity", 5.0f);
            GameAPI.SetVisualSettings("sky.AtmosphereThickness", 10.0f);
        }
    }
}
