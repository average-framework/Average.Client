using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Models;
using Average.Shared.DataModels;
using Average.Shared.Enums;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class CharacterService : IService
    {
        private float _scale = 1f;

        public readonly List<Cloth> clothes;
        public readonly List<int> bodyTypes;
        public readonly List<int> waistTypes;
        public readonly List<string> faceParts;
        public readonly List<string> colorPalettes;

        private readonly EventService _eventService;

        public CharacterService(EventService eventService)
        {
            _eventService = eventService;

            clothes = Configuration.Parse<List<Cloth>>("utilities/clothes.json");
            bodyTypes = Configuration.Parse<List<int>>("utilities/body_types.json");
            waistTypes = Configuration.Parse<List<int>>("utilities/waist_types.json");
            faceParts = Configuration.Parse<List<string>>("utilities/face_parts.json");
            colorPalettes = Configuration.Parse<List<string>>("utilities/color_palettes.json");
        }

        internal void Create(CharacterData characterData)
        {
            try
            {
                _eventService.EmitServer("character:create_character", characterData.ToJson());
            }
            catch (Exception ex)
            {
                Logger.Debug("Create: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        internal async Task SetAppearance(int ped, CharacterData characterData)
        {
            _scale = characterData.Skin.Scale;

            SetPedBody(ped, characterData.Skin.Head, characterData.Skin.Body, characterData.Skin.Legs);
            SetPedBodyComponents(ped, (uint)characterData.Skin.BodyType, (uint)characterData.Skin.WaistType);

            await SetPedClothes(ped, characterData.Skin.Gender, characterData.Outfit);

            SetPedFaceFeatures(ped, characterData.Skin);
            
            await SetFaceOverlays(ped, characterData.Skin);
            await BaseScript.Delay(1000);

            SetPedComponentDisabled(ped, 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(ped, 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
        }

        private void SetPedBody(int ped, uint head, uint body, uint legs)
        {
            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, head, false, true, true);

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, body, false, true, true);

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, legs, false, true, true);

            // Remove default pants
            RemovePedComponent(OutfitComponents.Pants);
        }

        private void SetPedFaceFeatures(int ped, SkinData skin)
        {
            SetPedFaceFeature(ped, CharacterFacePart.CheeckBonesDepth, skin.CheeckBonesDepth);
            SetPedFaceFeature(ped, CharacterFacePart.CheeckBonesHeight, skin.CheeckBonesHeight);
            SetPedFaceFeature(ped, CharacterFacePart.CheeckBonesWidth, skin.CheeckBonesWidth);
            SetPedFaceFeature(ped, CharacterFacePart.ChinDepth, skin.ChinDepth);
            SetPedFaceFeature(ped, CharacterFacePart.ChinHeight, skin.ChinHeight);
            SetPedFaceFeature(ped, CharacterFacePart.ChinWidth, skin.ChinWidth);
            SetPedFaceFeature(ped, CharacterFacePart.EarsAngle, skin.EarsAngle);
            SetPedFaceFeature(ped, CharacterFacePart.EarsHeight, skin.EarsHeight);
            SetPedFaceFeature(ped, CharacterFacePart.EarsLobeSize, skin.EarsLobeSize);
            SetPedFaceFeature(ped, CharacterFacePart.EarsWidth, skin.EarsWidth);
            SetPedFaceFeature(ped, CharacterFacePart.EyebrowDepth, skin.EyebrowDepth);
            SetPedFaceFeature(ped, CharacterFacePart.EyebrowHeight, skin.EyebrowHeight);
            SetPedFaceFeature(ped, CharacterFacePart.EyebrowWidth, skin.EyebrowWidth);
            SetPedFaceFeature(ped, CharacterFacePart.EyeLidHeight, skin.EyeLidHeight);
            SetPedFaceFeature(ped, CharacterFacePart.EyeLidWidth, skin.EyeLidWidth);
            SetPedFaceFeature(ped, CharacterFacePart.EyesAngle, skin.EyesAngle);
            SetPedFaceFeature(ped, CharacterFacePart.EyesDepth, skin.EyesDepth);
            SetPedFaceFeature(ped, CharacterFacePart.EyesDistance, skin.EyesDistance);
            SetPedFaceFeature(ped, CharacterFacePart.EyesHeight, skin.EyesHeight);
            SetPedFaceFeature(ped, CharacterFacePart.HeadWidth, skin.HeadWidth);
            SetPedFaceFeature(ped, CharacterFacePart.JawDepth, skin.JawDepth);
            SetPedFaceFeature(ped, CharacterFacePart.JawHeight, skin.JawHeight);
            SetPedFaceFeature(ped, CharacterFacePart.JawWidth, skin.JawWidth);
            SetPedFaceFeature(ped, CharacterFacePart.LowerLipDepth, skin.LowerLipDepth);
            SetPedFaceFeature(ped, CharacterFacePart.LowerLipHeight, skin.LowerLipHeight);
            SetPedFaceFeature(ped, CharacterFacePart.LowerLipWidth, skin.LowerLipWidth);
            SetPedFaceFeature(ped, CharacterFacePart.MouthDepth, skin.MouthDepth);
            SetPedFaceFeature(ped, CharacterFacePart.MouthWidth, skin.MouthWidth);
            SetPedFaceFeature(ped, CharacterFacePart.MouthXPos, skin.MouthXPos);
            SetPedFaceFeature(ped, CharacterFacePart.MouthYPos, skin.MouthYPos);
            SetPedFaceFeature(ped, CharacterFacePart.NoseAngle, skin.NoseAngle);
            SetPedFaceFeature(ped, CharacterFacePart.NoseCurvature, skin.NoseCurvature);
            SetPedFaceFeature(ped, CharacterFacePart.NoseHeight, skin.NoseHeight);
            SetPedFaceFeature(ped, CharacterFacePart.NoseSize, skin.NoseSize);
            SetPedFaceFeature(ped, CharacterFacePart.NoseWidth, skin.NoseWidth);
            SetPedFaceFeature(ped, CharacterFacePart.NoStrilsDistance, skin.NoStrilsDistance);
            SetPedFaceFeature(ped, CharacterFacePart.UpperLipDepth, skin.UpperLipDepth);
            SetPedFaceFeature(ped, CharacterFacePart.UpperLipHeight, skin.UpperLipHeight);
            SetPedFaceFeature(ped, CharacterFacePart.UpperLipWidth, skin.UpperLipWidth);
        }

        private async Task SetPedCloth(int ped, uint cloth)
        {
            // No cloth
            if (cloth == 0) return;

            var clothInfo = clothes.Find(x => x.Hash == cloth.ToString("X8"));

            // Cloth does not exists in clothes.json file
            if (clothInfo == null) return;

            var category = uint.Parse(clothInfo.CategoryHash, NumberStyles.AllowHexSpecifier);
            var metaped = Call<int>(0xEC9A1261BF0CE510, ped);
            var time = GetGameTimer();

            while (!Call<bool>(0xFB4891BD7578CDC1, ped, category) && (GetGameTimer() - time) < 1000)
            {
                Call(0xD710A5007C2AC539, ped, category, 1);
                Call(0xDF631E4BCE1B1FC4, ped, category, 0, 1);
                Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);

                Call(0x59BD177A1A48600A, ped, category);
                Call(0xD3A7B003ED343FD9, ped, cloth, true, clothInfo.IsMultiplayer, false);

                await BaseScript.Delay(250);
            }
        }

        internal async Task SetPedClothes(int ped, Gender gender, OutfitData outfitData)
        {
            await SetPedCloth(ped, outfitData.Torso);
            await SetPedCloth(ped, outfitData.Leg);
            await SetPedCloth(ped, outfitData.Head);
            await SetPedCloth(ped, outfitData.Hair);
            await SetPedCloth(ped, outfitData.Teeth);
            await SetPedCloth(ped, outfitData.Eye);

            if (gender == Gender.Male)
            {
                await SetPedCloth(ped, outfitData.Goatee);
                await SetPedCloth(ped, outfitData.BeardChop);
                await SetPedCloth(ped, outfitData.Mustache);
                await SetPedCloth(ped, outfitData.MustacheMP);
            }

            await SetPedCloth(ped, outfitData.Shirt);
            await SetPedCloth(ped, outfitData.Vest);
            await SetPedCloth(ped, outfitData.Pant);
            await SetPedCloth(ped, outfitData.Skirt);
            await SetPedCloth(ped, outfitData.Apron);
            await SetPedCloth(ped, outfitData.Cloak);
            await SetPedCloth(ped, outfitData.Coat);
            await SetPedCloth(ped, outfitData.CoatClosed);
            await SetPedCloth(ped, outfitData.Chap);
            await SetPedCloth(ped, outfitData.Necktie);
            await SetPedCloth(ped, outfitData.Neckwear);
            await SetPedCloth(ped, outfitData.Poncho);
            await SetPedCloth(ped, outfitData.Boot);
            await SetPedCloth(ped, outfitData.Spur);
            await SetPedCloth(ped, outfitData.Hat);
            await SetPedCloth(ped, outfitData.Mask);
            await SetPedCloth(ped, outfitData.MaskLarge);
            await SetPedCloth(ped, outfitData.Eyewear);
            await SetPedCloth(ped, outfitData.RingLeftHand);
            await SetPedCloth(ped, outfitData.RingRightHand);
            await SetPedCloth(ped, outfitData.Glove);
            await SetPedCloth(ped, outfitData.Bracelt);
            await SetPedCloth(ped, outfitData.Gauntlet);
            await SetPedCloth(ped, outfitData.Suspender);
            await SetPedCloth(ped, outfitData.Belt);
            await SetPedCloth(ped, outfitData.Beltbuckle);
            await SetPedCloth(ped, outfitData.Gunbelt);
            await SetPedCloth(ped, outfitData.Loadout);
            await SetPedCloth(ped, outfitData.Armor);
            await SetPedCloth(ped, outfitData.Badge);
            await SetPedCloth(ped, outfitData.HolsterCrossdraw);
            await SetPedCloth(ped, outfitData.HolsterLeft);
            await SetPedCloth(ped, outfitData.HolsterRight);
            await SetPedCloth(ped, outfitData.LegAttachement);
            await SetPedCloth(ped, outfitData.Sheath);
            await SetPedCloth(ped, outfitData.Spat);
            await SetPedCloth(ped, outfitData.Accessory);
            await SetPedCloth(ped, outfitData.TalismanBelt);
            await SetPedCloth(ped, outfitData.TalismanHolster);
            await SetPedCloth(ped, outfitData.TalismanSatchel);
            await SetPedCloth(ped, outfitData.TalismanWrist);
            await SetPedCloth(ped, outfitData.Satchel);

            if (gender == Gender.Female)
            {
                await SetPedCloth(ped, outfitData.FemaleUnknow01);
            }

            Call(0x704C908E9C405136, ped);
            Call(0xAAB86462966168CE, ped, 1);
            Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);
        }

        public async Task<int> SpawnPed(Gender gender)
        {
            var model = gender == Gender.Male ? (uint)GetHashKey("mp_male") : (uint)GetHashKey("mp_female");

            await LoadModel(model);
            SetPlayerModel(model);

            var ped = PlayerPedId();

            SetPedOutfitPreset(ped, 0);

            await BaseScript.Delay(1000);

            SetPedComponentDisabled(ped, 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(ped, 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
            SetModelAsNoLongerNeeded(model);

            Call(0xAAA34F8A7CB32098, ped);
            Call(0xF25DF915FA38C5F3, ped);
            Call(0x4E4B996C928C7AA6, PlayerId());

            Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped);

            return ped;
        }

        public async Task SetPedOutfit(int ped, Dictionary<string, uint> newClothes, int delay = 250)
        {
            foreach (var cloth in newClothes)
            {
                if (cloth.Value != 0)
                {
                    var c = clothes.Find(x => x.Hash == cloth.Value.ToString("X8"));
                    var categoryHash = uint.Parse(cloth.Key, NumberStyles.AllowHexSpecifier);
                    var time = GetGameTimer();

                    while (!Call<bool>(0xFB4891BD7578CDC1, ped, categoryHash) && (GetGameTimer() - time < 5000))
                    {
                        Call(0xD710A5007C2AC539, ped, categoryHash, 1);
                        Call(0xDF631E4BCE1B1FC4, ped, categoryHash, 0, 1);
                        Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);

                        var metaped = Call<int>(0xEC9A1261BF0CE510, ped);
                        var category = Call<uint>(0x5FF9A878C3D115B8, cloth.Value, metaped, c.IsMultiplayer);

                        Call(0x59BD177A1A48600A, ped, categoryHash);
                        Call(0xD3A7B003ED343FD9, ped, cloth.Value, true, c.IsMultiplayer, false);

                        await BaseScript.Delay(delay);
                    }
                }
            }

            Call(0x704C908E9C405136, ped);
            Call(0xAAB86462966168CE, ped, 1);
            Call(0xCC8CA3E88256E58F, ped, 0, 1, 1, 1, 0);
        }

        private void SetPedBodyComponents(int ped, uint bodyType, uint waistType)
        {
            SetPedBodyComponent(ped, bodyType);
            SetPedBodyComponent(ped, waistType);
        }

        private async Task SetFaceOverlay(int ped, int textureId, OverlayData overlay)
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

        private async Task SetFaceOverlays(int ped, SkinData skin)
        {
            var textureId = -1;

            if (textureId != -1)
            {
                ResetPedTexture2(textureId);
                DeletePedTexture(textureId);
            }

            textureId = Call<int>(0xC5E7204F322E49EB, skin.Albedo, skin.Normal, skin.Material);

            foreach (var overlay in skin.OverlaysData)
            {
                await SetFaceOverlay(ped, textureId, overlay);
            }
        }

        internal void RemoveAllClothes(int ped)
        {
            RemovePedComponent(ped, OutfitComponents.Accessories);
            RemovePedComponent(ped, OutfitComponents.Armors);
            RemovePedComponent(ped, OutfitComponents.Badges);
            RemovePedComponent(ped, OutfitComponents.Mustache);
            RemovePedComponent(ped, OutfitComponents.MustacheMP);
            RemovePedComponent(ped, OutfitComponents.Beltbuckles);
            RemovePedComponent(ped, OutfitComponents.Belts);
            RemovePedComponent(ped, OutfitComponents.Boots);
            RemovePedComponent(ped, OutfitComponents.Bracelts);
            RemovePedComponent(ped, OutfitComponents.Chaps);
            RemovePedComponent(ped, OutfitComponents.Cloaks);
            RemovePedComponent(ped, OutfitComponents.Coats);
            RemovePedComponent(ped, OutfitComponents.Eyes);
            RemovePedComponent(ped, OutfitComponents.Eyewear);
            RemovePedComponent(ped, OutfitComponents.Gauntlets);
            RemovePedComponent(ped, OutfitComponents.Gloves);
            RemovePedComponent(ped, OutfitComponents.Gunbelts);
            RemovePedComponent(ped, OutfitComponents.Hairs);
            RemovePedComponent(ped, OutfitComponents.Hats);
            RemovePedComponent(ped, OutfitComponents.Loadouts);
            RemovePedComponent(ped, OutfitComponents.Masks);
            RemovePedComponent(ped, OutfitComponents.Neckties);
            RemovePedComponent(ped, OutfitComponents.Neckwear);
            RemovePedComponent(ped, OutfitComponents.Pants);
            RemovePedComponent(ped, OutfitComponents.Ponchos);
            RemovePedComponent(ped, OutfitComponents.Satchels);
            RemovePedComponent(ped, OutfitComponents.Shirts);
            RemovePedComponent(ped, OutfitComponents.Skirts);
            RemovePedComponent(ped, OutfitComponents.Spats);
            RemovePedComponent(ped, OutfitComponents.Spurs);
            RemovePedComponent(ped, OutfitComponents.Suspenders);
            RemovePedComponent(ped, OutfitComponents.Teeth);
            RemovePedComponent(ped, OutfitComponents.Vests);
            RemovePedComponent(ped, OutfitComponents.LegAttachements);
            RemovePedComponent(ped, OutfitComponents.RingsLeftHand);
            RemovePedComponent(ped, OutfitComponents.RingsRightHand);
            RemovePedComponent(ped, OutfitComponents.HolsterCrossdraw);
            RemovePedComponent(ped, OutfitComponents.HolstersLeft);
            RemovePedComponent(ped, OutfitComponents.HolstersRight);
            RemovePedComponent(ped, OutfitComponents.TalismanHolster);
            RemovePedComponent(ped, OutfitComponents.TalismanBelt);
            RemovePedComponent(ped, OutfitComponents.TalismanSatchel);
            RemovePedComponent(ped, OutfitComponents.TalismanWrist);
            RemovePedComponent(ped, OutfitComponents.Sheaths);
            RemovePedComponent(ped, OutfitComponents.Aprons);
            RemovePedComponent(ped, OutfitComponents.Goatees);
            RemovePedComponent(ped, OutfitComponents.MasksLarge);
            RemovePedComponent(ped, OutfitComponents.CoatsClosed);
            RemovePedComponent(ped, OutfitComponents.BeardChops);
            RemovePedComponent(ped, OutfitComponents.HairAccessories);
        }
    }
}
