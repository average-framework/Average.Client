using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Server.DataModels;
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
        private async void OnSetAppearance(int gender, uint origin, uint head, uint body, uint bodyType, uint waistType, uint legs, float scale, string textureJson, string faceJson, string faceOverlayJson, string clothesJson)
        {
            var player = PlayerId();
            var ped = GetPlayerPed(player);

            Logger.Debug("Set ped appearance: " + ped + ", " + player);

            var texture = textureJson.Deserialize<TextureData>();
            var face = faceJson.Deserialize<FaceData>();
            var faceOverlay = faceOverlayJson.Deserialize<FaceOverlayData>();
            var clothes = clothesJson.Deserialize<ClothesData>();

            await _characterService.SetAppearance(ped, (Gender)gender, origin, head, body, legs, scale, bodyType, waistType, clothes, face, faceOverlay, texture);
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

        [ClientEvent("character:respawn_ped")]
        private async void OnRespawnPed(int gender)
        {
            await _characterService.RespawnPed((Gender)gender);
        }
    }
}
