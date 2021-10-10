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
            var ped = PlayerPedId();
            var characterData = characterJson.Deserialize<CharacterData>();
            await _characterService.SetAppearance(ped, characterData);
        }

        [ClientEvent("character:remove_all_clothes")]
        private void OnRemoveAllClothes()
        {
            _characterService.RemoveAllClothes(PlayerPedId());
        }

        [ClientEvent("character:set_outfit")]
        private async void OnRemoveAllClothes(string outfitJson)
        {
            var outfit = outfitJson.Deserialize<Dictionary<string, uint>>();
            await _characterService.SetPedOutfit(PlayerPedId(), outfit);
        }

        [ClientEvent("character:spawn_ped")]
        private async void OnSpawnPed(string characterJson)
        {
            NetworkStartSoloTutorialSession();

            var characterData = characterJson.Deserialize<CharacterData>();
            var ped = await _characterService.SpawnPed(characterData.Skin.Gender);
            var tempPos = new Vector3(0f, 0f, 105f);

            FreezeEntityPosition(ped, true);
            SetEntityCoords(ped, characterData.Position.X, characterData.Position.Y, characterData.Position.Z, true, true, true, false);
            RequestCollisionAtCoord(characterData.Position.X, characterData.Position.Y, characterData.Position.Z);

            var time = GetGameTimer();
            while (!HasCollisionLoadedAroundEntity(ped) && (GetGameTimer() - time) < 5000) await BaseScript.Delay(0);

            if (!HasCollisionLoadedAroundEntity(ped))
            {
                SetEntityCoords(ped, tempPos.X, tempPos.Y, tempPos.Z, true, true, true, false);
                SetEntityHeading(ped, 0f);
            }

            await _characterService.SetAppearance(ped, characterData);

            FreezeEntityPosition(ped, false);

            NetworkEndTutorialSession();
            ShutdownLoadingScreen();

            await FadeIn(1000);
        }
    }
}
