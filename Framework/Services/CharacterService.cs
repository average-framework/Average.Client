using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Managers;
using Average.Client.Models;
using Average.Shared.DataModels;
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

        public readonly List<Cloth> clothes;
        public readonly List<int> bodyTypes;
        public readonly List<int> waistTypes;
        public readonly List<string> faceParts;
        public readonly List<string> colorPalettes;

        private readonly EventManager _eventManager;

        private List<string> _blockedClothes = new();

        public CharacterService(EventManager eventManager)
        {
            _eventManager = eventManager;

            clothes = Configuration.Parse<List<Cloth>>("utilities/clothes.json");
            bodyTypes = Configuration.Parse<List<int>>("utilities/body_types.json");
            waistTypes = Configuration.Parse<List<int>>("utilities/waist_types.json");
            faceParts = Configuration.Parse<List<string>>("utilities/face_parts.json");
            colorPalettes = Configuration.Parse<List<string>>("utilities/color_palettes.json");
        }

        internal void Create(CharacterData characterData)
        {
            _eventManager.EmitServer("character:create_character", characterData.ToJson());
        }

        internal async Task SetAppearance(int ped, CharacterData characterData)
        {
            _scale = characterData.Skin.Scale;

            SetPedBody(ped, characterData.Skin.Gender, characterData.Skin.Head, characterData.Skin.Body, characterData.Skin.Legs);
            SetPedBodyComponents((uint)characterData.Skin.BodyType, (uint)characterData.Skin.WaistType);

            await SetPedClothes(ped, characterData.Skin.Gender, characterData.Outfit);
            await SetFaceOverlays(ped, characterData.Skin);

            SetPedFaceFeatures(characterData.Skin);

            SetPedComponentDisabled(ped, 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(ped, 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
        }

        private void SetPedBody(int ped, Gender gender, uint head, uint body, uint legs)
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

        private void SetPedFaceFeatures(SkinData skin)
        {
            SetPedFaceFeature(CharacterFacePart.CheeckBonesDepth, skin.CheeckBonesDepth);
            SetPedFaceFeature(CharacterFacePart.CheeckBonesHeight, skin.CheeckBonesHeight);
            SetPedFaceFeature(CharacterFacePart.CheeckBonesWidth, skin.CheeckBonesWidth);
            SetPedFaceFeature(CharacterFacePart.ChinDepth, skin.ChinDepth);
            SetPedFaceFeature(CharacterFacePart.ChinHeight, skin.ChinHeight);
            SetPedFaceFeature(CharacterFacePart.ChinWidth, skin.ChinWidth);
            SetPedFaceFeature(CharacterFacePart.EarsAngle, skin.EarsAngle);
            SetPedFaceFeature(CharacterFacePart.EarsHeight, skin.EarsHeight);
            SetPedFaceFeature(CharacterFacePart.EarsLobeSize, skin.EarsLobeSize);
            SetPedFaceFeature(CharacterFacePart.EarsWidth, skin.EarsWidth);
            SetPedFaceFeature(CharacterFacePart.EyebrowDepth, skin.EyebrowDepth);
            SetPedFaceFeature(CharacterFacePart.EyebrowHeight, skin.EyebrowHeight);
            SetPedFaceFeature(CharacterFacePart.EyebrowWidth, skin.EyebrowWidth);
            SetPedFaceFeature(CharacterFacePart.EyeLidHeight, skin.EyeLidHeight);
            SetPedFaceFeature(CharacterFacePart.EyeLidWidth, skin.EyeLidWidth);
            SetPedFaceFeature(CharacterFacePart.EyesAngle, skin.EyesAngle);
            SetPedFaceFeature(CharacterFacePart.EyesDepth, skin.EyesDepth);
            SetPedFaceFeature(CharacterFacePart.EyesDistance, skin.EyesDistance);
            SetPedFaceFeature(CharacterFacePart.EyesHeight, skin.EyesHeight);
            SetPedFaceFeature(CharacterFacePart.HeadWidth, skin.HeadWidth);
            SetPedFaceFeature(CharacterFacePart.JawDepth, skin.JawDepth);
            SetPedFaceFeature(CharacterFacePart.JawHeight, skin.JawHeight);
            SetPedFaceFeature(CharacterFacePart.JawWidth, skin.JawWidth);
            SetPedFaceFeature(CharacterFacePart.LowerLipDepth, skin.LowerLipDepth);
            SetPedFaceFeature(CharacterFacePart.LowerLipHeight, skin.LowerLipHeight);
            SetPedFaceFeature(CharacterFacePart.LowerLipWidth, skin.LowerLipWidth);
            SetPedFaceFeature(CharacterFacePart.MouthDepth, skin.MouthDepth);
            SetPedFaceFeature(CharacterFacePart.MouthWidth, skin.MouthWidth);
            SetPedFaceFeature(CharacterFacePart.MouthXPos, skin.MouthXPos);
            SetPedFaceFeature(CharacterFacePart.MouthYPos, skin.MouthYPos);
            SetPedFaceFeature(CharacterFacePart.NoseAngle, skin.NoseAngle);
            SetPedFaceFeature(CharacterFacePart.NoseCurvature, skin.NoseCurvature);
            SetPedFaceFeature(CharacterFacePart.NoseHeight, skin.NoseHeight);
            SetPedFaceFeature(CharacterFacePart.NoseSize, skin.NoseSize);
            SetPedFaceFeature(CharacterFacePart.NoseWidth, skin.NoseWidth);
            SetPedFaceFeature(CharacterFacePart.NoStrilsDistance, skin.NoStrilsDistance);
            SetPedFaceFeature(CharacterFacePart.UpperLipDepth, skin.UpperLipDepth);
            SetPedFaceFeature(CharacterFacePart.UpperLipHeight, skin.UpperLipHeight);
            SetPedFaceFeature(CharacterFacePart.UpperLipWidth, skin.UpperLipWidth);
        }

        private async Task SetPedCloth(int ped, uint cloth)
        {
            // Empty cloth
            if (cloth == 0) return;

            var clothInfo = clothes.Find(x => x.Hash == cloth.ToString("X8"));
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

        private async Task SetPedClothes(int ped, Gender gender, OutfitData outfitData)
        {
            while (!Call<bool>(0xA0BC8FAED8CFEB3C, ped)) await BaseScript.Delay(250);

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

            if (_blockedClothes.Count > 0 && _blockedClothesState <= 2)
            {
                _blockedClothesState++;
                Logger.Debug($"[Character] Restart clothes loading.. Attempt [{_blockedClothesState}/3]");
                await SpawnPed(gender);
            }
            else
            {
                Logger.Info("[Character] Clothes successfully loaded.");
            }
        }

        public async Task SpawnPed(Gender gender)
        {
            var model = gender == Gender.Male ? (uint)GetHashKey("mp_male") : (uint)GetHashKey("mp_female");

            await LoadModel(model);
            SetPlayerModel(model);
            SetPedOutfitPreset(PlayerPedId(), 0);

            await BaseScript.Delay(1000);

            SetPedComponentDisabled(PlayerPedId(), 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(PlayerPedId(), 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
            SetModelAsNoLongerNeeded(model);

            Call(0xAAA34F8A7CB32098, PlayerPedId());
            Call(0xF25DF915FA38C5F3, PlayerPedId());
            Call(0x4E4B996C928C7AA6, PlayerId());
            ClearPlayerWantedLevel(PlayerId());

            Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, PlayerPedId());
        }

        public async Task SetPedOutfit(int ped, Dictionary<string, uint> newClothes, int delay = 100)
        {
            foreach (var cloth in newClothes)
            {
                if (cloth.Value != 0)
                {
                    var c = clothes.Find(x => x.Hash == cloth.Value.ToString("X8"));
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

        private void SetPedBodyComponents(uint bodyType, uint waistType)
        {
            SetPedBodyComponent(bodyType);
            SetPedBodyComponent(waistType);
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
