using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.DataModels;
using Average.Shared.Enums;
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
        private async void OnRemoveAllClothes()
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
        private async void OnRespawnPed(string characterJson)
        {
            var characterData = characterJson.Deserialize<CharacterData>();
            Logger.Warn("Start");
            await _characterService.SpawnPed(characterData.Skin.Gender);
            Logger.Warn("Middle");
            await _characterService.SetAppearance(PlayerPedId(), characterData);
            Logger.Warn("End: " + PlayerPedId());

            ShutdownLoadingScreen();
            await FadeIn(1000);
        }
    }
}
