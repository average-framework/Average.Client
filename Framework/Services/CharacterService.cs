using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Models;
using Average.Server.DataModels;
using Average.Shared.Enums;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class CharacterService : IService
    {
        private int _blockedClothesState;
        private float _scale = 1f;

        private readonly List<Cloth> _clothes;

        public CharacterService()
        {
            _clothes = Configuration.Parse<List<Cloth>>("utilities/clothes.json");
        }

        internal async Task SetAppearance(int ped, Gender gender, uint origin, uint head, uint body, uint legs, float scale, uint bodyType, uint waistType, ClothesData clothes, FaceData face, FaceOverlayData faceOverlay, TextureData texture)
        {
            _scale = scale;

            SetPedBody(ped, gender, head, body, legs);
            SetPedBodyComponents(bodyType, waistType);

            await SetPedClothes(ped, gender, clothes);
            await SetFaceOverlays(ped, faceOverlay, texture);

            SetPedFaceFeatures(face);

            SetPedComponentDisabled(ped, 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(ped, 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
        }

        internal void SetPedBody(int ped, Gender gender, uint head, uint body, uint legs)
        {
            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, head, false, true, true);

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, body, false, true, true);

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, legs, false, true, true);

            // Remove default pants
            RemovePedComponent(CharacterClothComponents.Pants);
        }

        internal void SetPedFaceFeatures(FaceData faceData)
        {
            SetPedFaceFeature(CharacterFacePart.CheeckBonesDepth, faceData.CheeckBonesDepth);
            SetPedFaceFeature(CharacterFacePart.CheeckBonesHeight, faceData.CheeckBonesHeight);
            SetPedFaceFeature(CharacterFacePart.CheeckBonesWidth, faceData.CheeckBonesWidth);
            SetPedFaceFeature(CharacterFacePart.ChinDepth, faceData.ChinDepth);
            SetPedFaceFeature(CharacterFacePart.ChinHeight, faceData.ChinHeight);
            SetPedFaceFeature(CharacterFacePart.ChinWidth, faceData.ChinWidth);
            SetPedFaceFeature(CharacterFacePart.EarsAngle, faceData.EarsAngle);
            SetPedFaceFeature(CharacterFacePart.EarsHeight, faceData.EarsHeight);
            SetPedFaceFeature(CharacterFacePart.EarsLobeSize, faceData.EarsLobeSize);
            SetPedFaceFeature(CharacterFacePart.EarsWidth, faceData.EarsWidth);
            SetPedFaceFeature(CharacterFacePart.EyebrowDepth, faceData.EyebrowDepth);
            SetPedFaceFeature(CharacterFacePart.EyebrowHeight, faceData.EyebrowHeight);
            SetPedFaceFeature(CharacterFacePart.EyebrowWidth, faceData.EyebrowWidth);
            SetPedFaceFeature(CharacterFacePart.EyeLidHeight, faceData.EyeLidHeight);
            SetPedFaceFeature(CharacterFacePart.EyeLidWidth, faceData.EyeLidWidth);
            SetPedFaceFeature(CharacterFacePart.EyesAngle, faceData.EyesAngle);
            SetPedFaceFeature(CharacterFacePart.EyesDepth, faceData.EyesDepth);
            SetPedFaceFeature(CharacterFacePart.EyesDistance, faceData.EyesDistance);
            SetPedFaceFeature(CharacterFacePart.EyesHeight, faceData.EyesHeight);
            SetPedFaceFeature(CharacterFacePart.HeadWidth, faceData.HeadWidth);
            SetPedFaceFeature(CharacterFacePart.JawDepth, faceData.JawDepth);
            SetPedFaceFeature(CharacterFacePart.JawHeight, faceData.JawHeight);
            SetPedFaceFeature(CharacterFacePart.JawWidth, faceData.JawWidth);
            SetPedFaceFeature(CharacterFacePart.LowerLipDepth, faceData.LowerLipDepth);
            SetPedFaceFeature(CharacterFacePart.LowerLipHeight, faceData.LowerLipHeight);
            SetPedFaceFeature(CharacterFacePart.LowerLipWidth, faceData.LowerLipWidth);
            SetPedFaceFeature(CharacterFacePart.MouthDepth, faceData.MouthDepth);
            SetPedFaceFeature(CharacterFacePart.MouthWidth, faceData.MouthWidth);
            SetPedFaceFeature(CharacterFacePart.MouthXPos, faceData.MouthXPos);
            SetPedFaceFeature(CharacterFacePart.MouthYPos, faceData.MouthYPos);
            SetPedFaceFeature(CharacterFacePart.NoseAngle, faceData.NoseAngle);
            SetPedFaceFeature(CharacterFacePart.NoseCurvature, faceData.NoseCurvature);
            SetPedFaceFeature(CharacterFacePart.NoseHeight, faceData.NoseHeight);
            SetPedFaceFeature(CharacterFacePart.NoseSize, faceData.NoseSize);
            SetPedFaceFeature(CharacterFacePart.NoseWidth, faceData.NoseWidth);
            SetPedFaceFeature(CharacterFacePart.NoStrilsDistance, faceData.NoStrilsDistance);
            SetPedFaceFeature(CharacterFacePart.UpperLipDepth, faceData.UpperLipDepth);
            SetPedFaceFeature(CharacterFacePart.UpperLipHeight, faceData.UpperLipHeight);
            SetPedFaceFeature(CharacterFacePart.UpperLipWidth, faceData.UpperLipWidth);
        }

        internal async Task SetPedCloth(int ped, uint cloth)
        {
            // Empty cloth
            if (cloth == 0) return;

            var clothInfo = _clothes.Find(x => x.Hash == cloth.ToString("X8"));
            var category = uint.Parse(clothInfo.CategoryHash, NumberStyles.AllowHexSpecifier);

            // Cloth does not exists in clothes.json file
            if (clothInfo == null) return;

            while (!Call<bool>(0xFB4891BD7578CDC1, ped, category))
            {
                Call(0xD710A5007C2AC539, ped, category, 1);
                Call(0xDF631E4BCE1B1FC4, ped, category, 0, 1);
                Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);

                var metaped = Call<int>(0xEC9A1261BF0CE510, ped);
                //var category = Call<uint>(0x5FF9A878C3D115B8, cloth.Value, metaped, c.IsMultiplayer);

                Call(0x59BD177A1A48600A, ped, category);
                Call(0xD3A7B003ED343FD9, ped, cloth, false, clothInfo.IsMultiplayer, true);

                await BaseScript.Delay(250);

                Logger.Debug("reapply: " + metaped + ", " + cloth + ", " + category + ", " + category);
            }

            if (!Call<bool>(0xFB4891BD7578CDC1, ped, category))
            {
                // cloth is not applied
                _blockedClothes.Add(clothInfo.CategoryHash);

                Logger.Warn($"[Character] Unable to loading cloth: {clothInfo.CategoryHash}, {clothInfo.Hash}");
            }
        }

        private List<string> _blockedClothes = new List<string>();

        internal async Task SetPedClothes(int ped, Gender gender, ClothesData clothes)
        {
            while (!Call<bool>(0xA0BC8FAED8CFEB3C, ped)) await BaseScript.Delay(250);

            await SetPedCloth(ped, clothes.Torso);
            await SetPedCloth(ped, clothes.Leg);
            await SetPedCloth(ped, clothes.Head);
            await SetPedCloth(ped, clothes.Hair);
            await SetPedCloth(ped, clothes.Teeth);
            await SetPedCloth(ped, clothes.Eye);

            if (gender == Gender.Male)
            {
                await SetPedCloth(ped, clothes.Goatee);
                await SetPedCloth(ped, clothes.BeardChop);
                await SetPedCloth(ped, clothes.Mustache);
                await SetPedCloth(ped, clothes.MustacheMP);
            }

            await SetPedCloth(ped, clothes.Shirt);
            await SetPedCloth(ped, clothes.Vest);
            await SetPedCloth(ped, clothes.Pant);
            await SetPedCloth(ped, clothes.Skirt);
            await SetPedCloth(ped, clothes.Apron);
            await SetPedCloth(ped, clothes.Cloak);
            await SetPedCloth(ped, clothes.Coat);
            await SetPedCloth(ped, clothes.CoatClosed);
            await SetPedCloth(ped, clothes.Chap);
            await SetPedCloth(ped, clothes.Necktie);
            await SetPedCloth(ped, clothes.Neckwear);
            await SetPedCloth(ped, clothes.Poncho);
            await SetPedCloth(ped, clothes.Boot);
            await SetPedCloth(ped, clothes.Spur);
            await SetPedCloth(ped, clothes.Hat);
            await SetPedCloth(ped, clothes.Mask);
            await SetPedCloth(ped, clothes.MaskLarge);
            await SetPedCloth(ped, clothes.Eyewear);
            await SetPedCloth(ped, clothes.RingLeftHand);
            await SetPedCloth(ped, clothes.RingRightHand);
            await SetPedCloth(ped, clothes.Glove);
            await SetPedCloth(ped, clothes.Bracelt);
            await SetPedCloth(ped, clothes.Gauntlet);
            await SetPedCloth(ped, clothes.Suspender);
            await SetPedCloth(ped, clothes.Belt);
            await SetPedCloth(ped, clothes.Beltbuckle);
            await SetPedCloth(ped, clothes.Gunbelt);
            await SetPedCloth(ped, clothes.Loadout);
            await SetPedCloth(ped, clothes.Armor);
            await SetPedCloth(ped, clothes.Badge);
            await SetPedCloth(ped, clothes.HolsterCrossdraw);
            await SetPedCloth(ped, clothes.HolsterLeft);
            await SetPedCloth(ped, clothes.HolsterRight);
            await SetPedCloth(ped, clothes.LegAttachement);
            await SetPedCloth(ped, clothes.Sheath);
            await SetPedCloth(ped, clothes.Spat);
            await SetPedCloth(ped, clothes.Accessory);
            await SetPedCloth(ped, clothes.TalismanBelt);
            await SetPedCloth(ped, clothes.TalismanHolster);
            await SetPedCloth(ped, clothes.TalismanSatchel);
            await SetPedCloth(ped, clothes.TalismanWrist);
            await SetPedCloth(ped, clothes.Satchel);

            if (gender == Gender.Female)
            {
                await SetPedCloth(ped, clothes.FemaleUnknow01);
            }

            Call(0x704C908E9C405136, ped);
            Call(0xAAB86462966168CE, ped, 1);
            Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);

            if (_blockedClothes.Count > 0 && _blockedClothesState <= 2)
            {
                _blockedClothesState++;
                Logger.Warn($"[Character] Restart clothes loading.. Attempt [{_blockedClothesState}/3]");
                await RespawnPed(gender);
            }
            else
            {
                Logger.Info("[Character] Clothes successfully loaded.");
            }
        }

        public async Task RespawnPed(Gender gender)
        {
            var model = gender == 0 ? (uint)GetHashKey("mp_male") : (uint)GetHashKey("mp_female");
            await LoadModel(model);

            SetPlayerModel(model);
            SetPedOutfitPreset(PlayerPedId(), 0);

            await BaseScript.Delay(1000);

            SetPedComponentDisabled(PlayerPedId(), 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(PlayerPedId(), 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
            SetModelAsNoLongerNeeded(model);

            Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, PlayerPedId());
        }

        public async Task SetPedOutfit(int ped, Dictionary<string, uint> newClothes, int delay = 100)
        {
            foreach (var cloth in newClothes)
            {
                if (cloth.Value != 0)
                {
                    var c = _clothes.Find(x => x.Hash == cloth.Value.ToString("X8"));
                    var categoryHash = uint.Parse(cloth.Key, NumberStyles.AllowHexSpecifier);

                    while (!Call<bool>(0xFB4891BD7578CDC1, ped, categoryHash))
                    {
                        Call(0xD710A5007C2AC539, ped, categoryHash, 1);
                        Call(0xDF631E4BCE1B1FC4, ped, categoryHash, 0, 1);
                        Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);
                        var metaped = Call<int>(0xEC9A1261BF0CE510, ped);
                        var category = Call<uint>(0x5FF9A878C3D115B8, cloth.Value, metaped, c.IsMultiplayer);
                        Call(0x59BD177A1A48600A, ped, categoryHash);
                        Call(0xD3A7B003ED343FD9, ped, cloth.Value, false, c.IsMultiplayer, true);
                        await BaseScript.Delay(250);
                        Logger.Debug("reapply: " + metaped + ", " + cloth.Key + ", " + categoryHash + ", " + category);
                    }
                }
            }

            Call(0x704C908E9C405136, ped);
            Call(0xAAB86462966168CE, ped, 1);
            Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);

            Logger.Info("[Character] Clothes successfully loaded.");
        }

        internal void SetPedBodyComponents(uint bodyType, uint waistType)
        {
            SetPedBodyComponent(bodyType);
            SetPedBodyComponent(waistType);
        }

        internal async Task SetFaceOverlay(int ped, int textureId, OverlayData overlay)
        {
            if (overlay.TextureVisibility)
            {
                var overlayId = AddPedOverlay(textureId, overlay.TextureId, overlay.TextureNormal, overlay.TextureMaterial, overlay.TextureColorType, overlay.TextureOpacity, overlay.TextureUnk);

                if (overlay.TextureColorType == 0)
                {
                    SetPedOverlayPalette(textureId, overlayId, overlay.Palette);
                    SetPedOverlayPaletteColour(textureId, overlayId, overlay.PalettePrimaryColor, overlay.PaletteSecondaryColor, overlay.PaletteTertiaryColor);
                }

                SetPedOverlayVariation(textureId, overlayId, overlay.Variante);
                SetPedOverlayOpacity(textureId, overlayId, overlay.Opacity);
            }

            while (!IsPedTextureValid(textureId)) await BaseScript.Delay(0);

            OverrideTextureOnPed(ped, (uint)GetHashKey("heads"), textureId);
            UpdatePedTexture(textureId);
            UpdatePedVariation();
        }

        internal async Task SetFaceOverlays(int ped, FaceOverlayData faceOverlay, TextureData texture)
        {
            var textureId = -1;

            if (textureId != -1)
            {
                ResetPedTexture2(textureId);
                DeletePedTexture(textureId);
            }

            textureId = Call<int>(0xC5E7204F322E49EB, texture.Albedo, texture.Normal, texture.Material);

            await SetFaceOverlay(ped, textureId, faceOverlay.Acne);
            await SetFaceOverlay(ped, textureId, faceOverlay.Ageing);
            await SetFaceOverlay(ped, textureId, faceOverlay.Beardstabble);
            await SetFaceOverlay(ped, textureId, faceOverlay.Blush);
            await SetFaceOverlay(ped, textureId, faceOverlay.Complex);
            await SetFaceOverlay(ped, textureId, faceOverlay.Disc);
            await SetFaceOverlay(ped, textureId, faceOverlay.Eyebrows);
            await SetFaceOverlay(ped, textureId, faceOverlay.Eyeliners);
            await SetFaceOverlay(ped, textureId, faceOverlay.Foundation);
            await SetFaceOverlay(ped, textureId, faceOverlay.Freckles);
            await SetFaceOverlay(ped, textureId, faceOverlay.Grime);
            await SetFaceOverlay(ped, textureId, faceOverlay.Hair);
            await SetFaceOverlay(ped, textureId, faceOverlay.Lipsticks);
            await SetFaceOverlay(ped, textureId, faceOverlay.Moles);
            await SetFaceOverlay(ped, textureId, faceOverlay.Paintedmasks);
            await SetFaceOverlay(ped, textureId, faceOverlay.Scars);
            await SetFaceOverlay(ped, textureId, faceOverlay.Shadows);
            await SetFaceOverlay(ped, textureId, faceOverlay.Spots);
        }

        internal void RemoveAllClothes()
        {
            var ped = PlayerPedId();

            RemovePedComponent(ped, CharacterClothComponents.Accessories);
            RemovePedComponent(ped, CharacterClothComponents.Armors);
            RemovePedComponent(ped, CharacterClothComponents.Badges);
            RemovePedComponent(ped, CharacterClothComponents.Mustache);
            RemovePedComponent(ped, CharacterClothComponents.MustacheMP);
            RemovePedComponent(ped, CharacterClothComponents.Beltbuckles);
            RemovePedComponent(ped, CharacterClothComponents.Belts);
            RemovePedComponent(ped, CharacterClothComponents.Boots);
            RemovePedComponent(ped, CharacterClothComponents.Bracelts);
            RemovePedComponent(ped, CharacterClothComponents.Chaps);
            RemovePedComponent(ped, CharacterClothComponents.Cloaks);
            RemovePedComponent(ped, CharacterClothComponents.Coats);
            RemovePedComponent(ped, CharacterClothComponents.Eyes);
            RemovePedComponent(ped, CharacterClothComponents.Eyewear);
            RemovePedComponent(ped, CharacterClothComponents.Gauntlets);
            RemovePedComponent(ped, CharacterClothComponents.Gloves);
            RemovePedComponent(ped, CharacterClothComponents.Gunbelts);
            RemovePedComponent(ped, CharacterClothComponents.Hairs);
            RemovePedComponent(ped, CharacterClothComponents.Hats);
            RemovePedComponent(ped, CharacterClothComponents.Loadouts);
            RemovePedComponent(ped, CharacterClothComponents.Masks);
            RemovePedComponent(ped, CharacterClothComponents.Neckties);
            RemovePedComponent(ped, CharacterClothComponents.Neckwear);
            RemovePedComponent(ped, CharacterClothComponents.Pants);
            RemovePedComponent(ped, CharacterClothComponents.Ponchos);
            RemovePedComponent(ped, CharacterClothComponents.Satchels);
            RemovePedComponent(ped, CharacterClothComponents.Shirts);
            RemovePedComponent(ped, CharacterClothComponents.Skirts);
            RemovePedComponent(ped, CharacterClothComponents.Spats);
            RemovePedComponent(ped, CharacterClothComponents.Spurs);
            RemovePedComponent(ped, CharacterClothComponents.Suspenders);
            RemovePedComponent(ped, CharacterClothComponents.Teeth);
            RemovePedComponent(ped, CharacterClothComponents.Vests);
            RemovePedComponent(ped, CharacterClothComponents.LegAttachements);
            RemovePedComponent(ped, CharacterClothComponents.RingsLeftHand);
            RemovePedComponent(ped, CharacterClothComponents.RingsRightHand);
            RemovePedComponent(ped, CharacterClothComponents.HolsterCrossdraw);
            RemovePedComponent(ped, CharacterClothComponents.HolstersLeft);
            RemovePedComponent(ped, CharacterClothComponents.HolstersRight);
            RemovePedComponent(ped, CharacterClothComponents.TalismanHolster);
            RemovePedComponent(ped, CharacterClothComponents.TalismanBelt);
            RemovePedComponent(ped, CharacterClothComponents.TalismanSatchel);
            RemovePedComponent(ped, CharacterClothComponents.TalismanWrist);
            RemovePedComponent(ped, CharacterClothComponents.Sheaths);
            RemovePedComponent(ped, CharacterClothComponents.Aprons);
            RemovePedComponent(ped, CharacterClothComponents.Goatees);
            RemovePedComponent(ped, CharacterClothComponents.MasksLarge);
            RemovePedComponent(ped, CharacterClothComponents.CoatsClosed);
            RemovePedComponent(ped, CharacterClothComponents.BeardChops);
            RemovePedComponent(ped, CharacterClothComponents.FemaleUnknow01);
        }
    }
}
