using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.DataModels;
using CitizenFX.Core;
using System.Collections.Generic;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Handlers
{
    internal class CharacterHandler : IHandler
    {
        private readonly CharacterService _characterService;

        public CharacterHandler(CharacterService characterService)
        {
            _characterService = characterService;
        }

        [ClientEvent("character:set_ped")]
        private async void OnSetPed(uint model, int variante)
        {
            await LoadModel(model);

            SetPlayerModel(model);
            SetPedOutfitPreset(PlayerPedId(), variante);
        }

        [ClientEvent("character:set_appearance")]
        private async void OnSetAppearance(string characterJson)
        {
            var player = PlayerId();
            var ped = GetPlayerPed(player);

            Logger.Debug("Set ped appearance: " + ped + ", " + player);

            var characterData = characterJson.Deserialize<CharacterData>();

            await _characterService.SetAppearance(ped, characterData);
        }

        [ClientEvent("character:remove_all_clothes")]
        private void OnRemoveAllClothes()
        {
            _characterService.RemoveAllClothes();
        }

        [ClientEvent("character:set_outfit")]
        private async void OnRemoveAllClothes(string outfitJson)
        {
            var outfit = outfitJson.Deserialize<Dictionary<string, uint>>();
            var player = PlayerId();
            var ped = GetPlayerPed(player);

            await _characterService.SetPedOutfit(ped, outfit);
        }

        [ClientEvent("character:spawn_ped")]
        private async void OnSpawnPed(string characterJson)
        {
            NetworkStartSoloTutorialSession();

            var ped = PlayerPedId();
            var characterData = characterJson.Deserialize<CharacterData>();

            await _characterService.SpawnPed(characterData.Skin.Gender);

            RequestCollisionAtCoord(characterData.Position.X, characterData.Position.Y, characterData.Position.Z);
            Call(0xEA23C49EAA83ACFB, characterData.Position.X, characterData.Position.Y, characterData.Position.Z, characterData.Position.H, true, true, false);

            var timer = GetGameTimer();
            while (!HasCollisionLoadedAroundEntity(ped) && GetGameTimer() - timer < 5000) await BaseScript.Delay(0);

            SetEntityCoords(ped, characterData.Position.X, characterData.Position.Y, characterData.Position.Z, true, true, true, false);
            SetEntityHeading(ped, characterData.Position.H);

            await _characterService.SetAppearance(ped, characterData);

            NetworkEndTutorialSession();

            ShutdownLoadingScreen();
            await FadeIn(1000);
        }
    }
}
