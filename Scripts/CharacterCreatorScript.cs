using Average.Client.Framework;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Events;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Menu;
using Average.Client.Framework.Services;
using Average.Client.Models;
using Average.Shared.DataModels;
using Average.Shared.Enums;
using Average.Shared.Utilities;
using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Scripts
{
    internal class CharacterCreatorScript : IService
    {
        // Containers
        private MenuContainer faceMenu;
        private MenuContainer faceFeaturesMenu;
        private MenuContainer clothesMenu;
        private MenuContainer infoMenu;
        private MenuContainer faceOverlayMenu;
        private MenuContainer attributesMenu;

        private TopContainer infoTopContainer;
        private BottomContainer infoBottomContainer;

        private TopContainer faceTopContainer;
        private BottomContainer faceBottomContainer;

        private TopContainer faceFeaturesTopContainer;
        private BottomContainer faceFeaturesBottomContainer;

        private TopContainer clothesTopContainer;
        private BottomContainer clothesBottomContainer;

        private TopContainer faceOverlayTopContainer;
        private BottomContainer faceOverlayBottomContainer;

        private TopContainer attributesTopContainer;
        private BottomContainer attributesBottomContainer;

        // Info Menu
        private SelectItem genderItem;
        private Vector2Item firstAndLastNameItem;
        private Vector2Item nationalityAndCityBirthItem;
        private Vector3Item dateOfBirthItem;
        private SelectSliderItem pedScaleItem;
        private SelectSliderItem culturesItem;
        private SelectSliderItem headsItem;
        private SelectSliderItem bodyItem;
        private SelectSliderItem legsItem;

        // Cloth Menu
        private SelectSliderItem hatsItem;
        private SelectSliderItem eyewearItem;
        private SelectSliderItem neckwearItem;
        private SelectSliderItem necktiesItem;
        private SelectSliderItem shirtsItem;
        private SelectSliderItem suspendersItem;
        private SelectSliderItem vestItem;
        private SelectSliderItem coatsItem;
        private SelectSliderItem coatsClosedItem;
        private SelectSliderItem ponchosItem;
        private SelectSliderItem cloakItem;
        private SelectSliderItem glovesItem;
        private SelectSliderItem ringsRightHandItem;
        private SelectSliderItem ringsLeftHandItem;
        private SelectSliderItem braceletsItem;
        private SelectSliderItem gunbeltItem;
        private SelectSliderItem beltItem;
        private SelectSliderItem buckleItem;
        private SelectSliderItem holstersCrossdrawItem;
        private SelectSliderItem holstersLeftItem;
        private SelectSliderItem holstersRightItem;
        private SelectSliderItem pantsItem;
        private SelectSliderItem skirtsItem;
        private SelectSliderItem bootsItem;
        private SelectSliderItem chapsItem;
        private SelectSliderItem spursItem;
        private SelectSliderItem spatsItem;
        private SelectSliderItem satchelsItem;
        private SelectSliderItem masksItem;
        private SelectSliderItem masksLargeItem;
        private SelectSliderItem loadoutsItem;
        private SelectSliderItem legAttachmentsItem;
        private SelectSliderItem gauntletsItem;
        private SelectSliderItem accessoriesItem;
        private SelectSliderItem sheathsItem;
        private SelectSliderItem apronsItem;
        private SelectSliderItem femaleUnknow01Item;
        private SelectSliderItem talismanBeltItem;
        private SelectSliderItem talismanHolsterItem;
        private SelectSliderItem talismanSatchelItem;
        private SelectSliderItem talismanWristItem;

        // Face Menu
        private SelectSliderItem beardChopsItem;
        private SelectSliderItem mustacheItem;
        private SelectSliderItem mustacheMpItem;
        private SelectSliderItem goateesItem;
        private SelectSliderItem teethItem;
        private SelectSliderItem hairItem;
        private SelectSliderItem eyesItem;

        private SelectSliderItem headWidthItem;
        private SelectSliderItem eyebrowHeightItem;
        private SelectSliderItem eyebrowWidthItem;
        private SelectSliderItem eyebrowDepthItem;
        private SelectSliderItem earsWidthItem;
        private SelectSliderItem earsAngleItem;
        private SelectSliderItem earsHeightItem;
        private SelectSliderItem earsLobeSizeItem;
        private SelectSliderItem cheeckBonesHeightItem;
        private SelectSliderItem cheeckBonesWidthItem;
        private SelectSliderItem cheeckBonesDepthItem;
        private SelectSliderItem jawHeightItem;
        private SelectSliderItem jawWidthItem;
        private SelectSliderItem jawDepthItem;
        private SelectSliderItem chinHeightItem;
        private SelectSliderItem chinWidthItem;
        private SelectSliderItem chinDepthItem;
        private SelectSliderItem eyeLidHeightItem;
        private SelectSliderItem eyeLidWidthItem;
        private SelectSliderItem eyesDepthItem;
        private SelectSliderItem eyesAngleItem;
        private SelectSliderItem eyesDistanceItem;
        private SelectSliderItem eyesHeightItem;
        private SelectSliderItem noseWidthItem;
        private SelectSliderItem noseSizeItem;
        private SelectSliderItem noseHeightItem;
        private SelectSliderItem noseAngleItem;
        private SelectSliderItem noseCurvatureItem;
        private SelectSliderItem noStrilsDistanceItem;
        private SelectSliderItem mouthWidthItem;
        private SelectSliderItem mouthDepthItem;
        private SelectSliderItem mouthXPosItem;
        private SelectSliderItem mouthYPosItem;
        private SelectSliderItem upperLipHeightItem;
        private SelectSliderItem upperLipWidthItem;
        private SelectSliderItem upperLipDepthItem;
        private SelectSliderItem lowerLipHeightItem;
        private SelectSliderItem lowerLipWidthItem;
        private SelectSliderItem lowerLipDepthItem;

        // Body Menu
        private SelectSliderItem bodyTypesItem;
        private SelectSliderItem waistTypesItem;

        // Face Overlay Menu
        private SelectSliderItem overlayTypeItem;
        private SelectSliderItem overlayItem;
        private SelectSliderItem overlayVarItem;
        private SelectSliderItem overlayPrimaryColorItem;
        private SelectSliderItem overlaySecondaryColorItem;
        private SelectSliderItem overlayTertiaryColorItem;
        private SelectSliderItem overlayPaletteItem;
        private SelectSliderItem overlayOpacityItem;
        private CheckboxItem overlayVisibilityItem;

        private Gender gender = Gender.Male;

        private int textureId = -1;
        public int defaultCamera;
        public int faceCamera;
        public int bodyCamera;
        public int footCamera;
        public int currentCamIndex;

        private PedOrigin origin;
        private List<PedOrigin> origins;

        private Dictionary<string, dynamic> textureType = new Dictionary<string, dynamic>();

        private Dictionary<int, float> characterFaceParts = new Dictionary<int, float>
        {
            { CharacterFacePart.CheeckBonesDepth, 0f },
            { CharacterFacePart.CheeckBonesHeight, 0f },
            { CharacterFacePart.CheeckBonesWidth, 0f },
            { CharacterFacePart.ChinDepth, 0f },
            { CharacterFacePart.ChinHeight, 0f },
            { CharacterFacePart.ChinWidth, 0f },
            { CharacterFacePart.EarsAngle, 0f },
            { CharacterFacePart.EarsHeight, 0f },
            { CharacterFacePart.EarsLobeSize, 0f },
            { CharacterFacePart.EarsWidth, 0f },
            { CharacterFacePart.EyebrowDepth, 0f },
            { CharacterFacePart.EyebrowHeight, 0f },
            { CharacterFacePart.EyebrowWidth, 0f },
            { CharacterFacePart.EyeLidHeight, 0f },
            { CharacterFacePart.EyeLidWidth, 0f },
            { CharacterFacePart.EyesAngle, 0f },
            { CharacterFacePart.EyesDepth, 0f },
            { CharacterFacePart.EyesDistance, 0f },
            { CharacterFacePart.EyesHeight, 0f },
            { CharacterFacePart.HeadWidth, 0f },
            { CharacterFacePart.JawDepth, 0f },
            { CharacterFacePart.JawHeight, 0f },
            { CharacterFacePart.JawWidth, 0f },
            { CharacterFacePart.LowerLipDepth, 0f },
            { CharacterFacePart.LowerLipHeight, 0f },
            { CharacterFacePart.LowerLipWidth, 0f },
            { CharacterFacePart.MouthDepth, 0f },
            { CharacterFacePart.MouthWidth, 0f },
            { CharacterFacePart.MouthXPos, 0f },
            { CharacterFacePart.MouthYPos, 0f },
            { CharacterFacePart.NoseAngle, 0f },
            { CharacterFacePart.NoseCurvature, 0f },
            { CharacterFacePart.NoseHeight, 0f },
            { CharacterFacePart.NoseSize, 0f },
            { CharacterFacePart.NoseWidth, 0f },
            { CharacterFacePart.NoStrilsDistance, 0f },
            { CharacterFacePart.UpperLipDepth, 0f },
            { CharacterFacePart.UpperLipHeight, 0f },
            { CharacterFacePart.UpperLipWidth, 0f },
        };

        private Dictionary<string, uint> characterClothes = new Dictionary<string, uint>()
        {
            { OutfitComponents.Accessories, 0 },
            { OutfitComponents.Badges, 0 },
            { OutfitComponents.Armors, 0 },
            { OutfitComponents.Beltbuckles, 0 },
            { OutfitComponents.Belts, 0 },
            { OutfitComponents.Boots, 0 },
            { OutfitComponents.Bracelts, 0 },
            { OutfitComponents.Chaps, 0 },
            { OutfitComponents.Cloaks, 0 },
            { OutfitComponents.Eyes, 0 },
            { OutfitComponents.Eyewear, 0 },
            { OutfitComponents.Gauntlets, 0 },
            { OutfitComponents.Gloves, 0 },
            { OutfitComponents.Gunbelts, 0 },
            { OutfitComponents.Hairs, 0 },
            { OutfitComponents.Hats, 0 },
            { OutfitComponents.Heads, 0 },
            { OutfitComponents.LegAttachements, 0 },
            { OutfitComponents.Legs, 0 },
            { OutfitComponents.Loadouts, 0 },
            { OutfitComponents.Neckties, 0 },
            { OutfitComponents.Neckwear, 0 },
            { OutfitComponents.Pants, 0 },
            { OutfitComponents.Ponchos, 0 },
            { OutfitComponents.RingsLeftHand, 0 },
            { OutfitComponents.RingsRightHand, 0 },
            { OutfitComponents.Satchels, 0 },
            { OutfitComponents.Shirts, 0 },
            { OutfitComponents.Skirts, 0 },
            { OutfitComponents.Spats, 0 },
            { OutfitComponents.Spurs, 0 },
            { OutfitComponents.Suspenders, 0 },
            { OutfitComponents.Teeth, 0 },
            { OutfitComponents.Torsos, 0 },
            { OutfitComponents.Vests, 0 },
            // New
            { OutfitComponents.Coats, 0 },
            { OutfitComponents.CoatsClosed, 0 },
            { OutfitComponents.Masks, 0 },
            { OutfitComponents.MasksLarge, 0 },
            { OutfitComponents.HolsterCrossdraw, 0 },
            { OutfitComponents.HolstersLeft, 0 },
            { OutfitComponents.HolstersRight, 0 },
            { OutfitComponents.Sheaths, 0 },
            { OutfitComponents.Aprons, 0 },
            { OutfitComponents.BeardChops, 0 },
            { OutfitComponents.Mustache, 0 },
            { OutfitComponents.MustacheMP, 0 },
            { OutfitComponents.Goatees, 0 },
            { OutfitComponents.HairAccessories, 0 },
            { OutfitComponents.TalismanBelt, 0 },
            { OutfitComponents.TalismanHolster, 0 },
            { OutfitComponents.TalismanSatchel, 0 },
            { OutfitComponents.TalismanWrist, 0 },
        };

        private Dictionary<string, OverlayData> characterOverlays = new Dictionary<string, OverlayData>
        {
            { "eyebrows", new OverlayData("eyebrows") },
            { "scars", new OverlayData("scars", 1) },
            { "eyeliners", new OverlayData("eyeliners") },
            { "lipsticks", new OverlayData("lipsticks") },
            { "acne", new OverlayData("acne", 1) },
            { "shadows", new OverlayData("shadows") },
            { "beardstabble", new OverlayData("beardstabble") },
            { "paintedmasks", new OverlayData("paintedmasks") },
            { "ageing", new OverlayData("ageing", 1) },
            { "blush", new OverlayData("blush") },
            { "complex", new OverlayData("complex", 1) },
            { "disc", new OverlayData("disc", 1) },
            { "foundation", new OverlayData("foundation") },
            { "freckles", new OverlayData("freckles", 1) },
            { "grime", new OverlayData("grime") },
            { "hair", new OverlayData("hair") },
            { "moles", new OverlayData("moles", 1) },
            { "spots", new OverlayData("spots", 1) },
        };

        private readonly MenuService _menuService;
        private readonly ThreadService _threadService;
        private readonly EventService _eventService;
        private readonly LanguageService _languageService;
        private readonly CharacterService _characterService;
        private readonly UIService _uiService;

        private readonly JObject _config;

        private readonly int _locationIndex = 0;

        public bool IsOpen { get; private set; }

        public CharacterCreatorScript(UIService uiService, CharacterService characterService, MenuService menuService, ThreadService threadService, EventService eventService, LanguageService languageService)
        {
            _menuService = menuService;
            _threadService = threadService;
            _eventService = eventService;
            _languageService = languageService;
            _characterService = characterService;
            _uiService = uiService;

            _config = Configuration.ParseToObject("configs/character_creator.json");

            // Menu
            _menuService.CanCloseMenu = false;

            // Events
            _eventService.ResourceStop += OnResourceStop;

            infoTopContainer = new();
            infoBottomContainer = new();
            infoMenu = new(infoTopContainer, infoBottomContainer, null);
            infoMenu.BannerTitle = _languageService.Get("Client.CharacterCreator.Info");

            faceTopContainer = new();
            faceBottomContainer = new();
            faceMenu = new(faceTopContainer, faceBottomContainer, null);
            faceMenu.BannerTitle = _languageService.Get("Client.CharacterCreator.Faces");

            faceFeaturesTopContainer = new();
            faceFeaturesBottomContainer = new();
            faceFeaturesMenu = new(faceFeaturesTopContainer, faceFeaturesBottomContainer, null);
            faceFeaturesMenu.BannerTitle = _languageService.Get("Client.CharacterCreator.FaceTraits");

            clothesTopContainer = new();
            clothesBottomContainer = new();
            clothesMenu = new(clothesTopContainer, clothesBottomContainer, null);
            clothesMenu.BannerTitle = "Vêtements";

            faceOverlayTopContainer = new();
            faceOverlayBottomContainer = new();
            faceOverlayMenu = new(faceOverlayTopContainer, faceOverlayBottomContainer, null);
            faceOverlayMenu.BannerTitle = "Visage avancé";

            attributesTopContainer = new();
            attributesBottomContainer = new();
            attributesMenu = new(attributesTopContainer, attributesBottomContainer, null);
            attributesMenu.BannerTitle = "Attributs";
        }

        private async Task WeatherUpdate()
        {
            NetworkClockTimeOverride(10, 0, 0, 0, true);
            Call(0xD74ACDF7DB8114AF, false);
            Call(0x59174F1AFE095B5A, (uint)Weather.Sunny, true, true, true, 1f, false);

            await BaseScript.Delay(0); // Delete ?
        }

        private async void CreateCharacter()
        {
            if (firstAndLastNameItem.Primary.Validate != null)
            {
                if (!firstAndLastNameItem.Primary.Validate.Invoke(firstAndLastNameItem)) return;
            }

            if (firstAndLastNameItem.Secondary.Validate != null)
            {
                if (!firstAndLastNameItem.Secondary.Validate.Invoke(firstAndLastNameItem)) return;
            }

            if (nationalityAndCityBirthItem.Primary.Validate != null)
            {
                if (!nationalityAndCityBirthItem.Primary.Validate.Invoke(nationalityAndCityBirthItem)) return;
            }

            if (nationalityAndCityBirthItem.Secondary.Validate != null)
            {
                if (!nationalityAndCityBirthItem.Secondary.Validate.Invoke(nationalityAndCityBirthItem)) return;
            }

            if (dateOfBirthItem.Primary.Validate != null)
            {
                if (!dateOfBirthItem.Primary.Validate.Invoke(dateOfBirthItem)) return;
            }

            if (dateOfBirthItem.Secondary.Validate != null)
            {
                if (!dateOfBirthItem.Secondary.Validate.Invoke(dateOfBirthItem)) return;
            }

            if (dateOfBirthItem.Tertiary.Validate != null)
            {
                if (!dateOfBirthItem.Tertiary.Validate.Invoke(dateOfBirthItem)) return;
            }

            Logger.Error("3");

            var characterId = RandomString();

            var characterData = new CharacterData
            {
                CharacterId = characterId,
                CreationDate = DateTime.Now,
                Firstname = firstAndLastNameItem.Primary.Value.ToString(),
                Lastname = firstAndLastNameItem.Secondary.Value.ToString(),
                Nationality = nationalityAndCityBirthItem.Primary.Value.ToString(),
                CityOfBirth = nationalityAndCityBirthItem.Secondary.Value.ToString(),
                DateOfBirth = $"{dateOfBirthItem.Primary.Value}/{dateOfBirthItem.Secondary.Value}/{dateOfBirthItem.Tertiary.Value}",
                Skin = new SkinData
                {
                    Gender = gender,
                    Scale = (float)pedScaleItem.Value,
                    BodyType = _characterService._bodyTypes[(int)bodyTypesItem.Value],
                    WaistType = _characterService._waistTypes[(int)waistTypesItem.Value],
                    Body = uint.Parse(CharacterUtilities.Origins[(int)culturesItem.Value].Bodies[(int)bodyItem.Value], NumberStyles.HexNumber),
                    Head = uint.Parse(CharacterUtilities.Origins[(int)culturesItem.Value].Heads[(int)headsItem.Value], NumberStyles.HexNumber),
                    Legs = uint.Parse(CharacterUtilities.Origins[(int)culturesItem.Value].Legs[(int)legsItem.Value], NumberStyles.HexNumber),

                    Albedo = textureType["albedo"],
                    Normal = textureType["normal"],
                    Material = textureType["material"],

                    OverlaysData = characterOverlays.Values.ToList(),

                    CheeckBonesDepth = (float)cheeckBonesDepthItem.Value,
                    CheeckBonesWidth = (float)cheeckBonesWidthItem.Value,
                    CheeckBonesHeight = (float)cheeckBonesHeightItem.Value,
                    ChinDepth = (float)chinDepthItem.Value,
                    ChinHeight = (float)chinHeightItem.Value,
                    ChinWidth = (float)chinWidthItem.Value,
                    EarsAngle = (float)earsAngleItem.Value,
                    EarsHeight = (float)earsHeightItem.Value,
                    EarsLobeSize = (float)earsLobeSizeItem.Value,
                    EarsWidth = (float)earsWidthItem.Value,
                    EyesAngle = (float)eyesAngleItem.Value,
                    EyebrowDepth = (float)eyebrowDepthItem.Value,
                    EyebrowHeight = (float)eyebrowHeightItem.Value,
                    EyebrowWidth = (float)eyebrowWidthItem.Value,
                    EyeLidHeight = (float)eyeLidHeightItem.Value,
                    EyeLidWidth = (float)eyeLidWidthItem.Value,
                    EyesDepth = (float)eyesDepthItem.Value,
                    EyesDistance = (float)eyesDistanceItem.Value,
                    EyesHeight = (float)eyesHeightItem.Value,
                    HeadWidth = (float)headWidthItem.Value,
                    JawDepth = (float)jawDepthItem.Value,
                    JawHeight = (float)jawHeightItem.Value,
                    JawWidth = (float)jawWidthItem.Value,
                    LowerLipDepth = (float)lowerLipDepthItem.Value,
                    LowerLipHeight = (float)lowerLipHeightItem.Value,
                    LowerLipWidth = (float)lowerLipWidthItem.Value,
                    MouthDepth = (float)mouthDepthItem.Value,
                    MouthWidth = (float)mouthWidthItem.Value,
                    MouthXPos = (float)mouthXPosItem.Value,
                    MouthYPos = (float)mouthYPosItem.Value,
                    NoseAngle = (float)noseAngleItem.Value,
                    NoseCurvature = (float)noseCurvatureItem.Value,
                    NoseHeight = (float)noseHeightItem.Value,
                    NoseSize = (float)noseSizeItem.Value,
                    NoseWidth = (float)noseWidthItem.Value,
                    NoStrilsDistance = (float)noStrilsDistanceItem.Value,
                    UpperLipDepth = (float)upperLipDepthItem.Value,
                    UpperLipHeight = (float)upperLipHeightItem.Value,
                    UpperLipWidth = (float)upperLipWidthItem.Value,
                },
                Outfit = new OutfitData
                {
                    Accessory = characterClothes[OutfitComponents.Accessories],
                    Apron = characterClothes[OutfitComponents.Aprons],
                    Armor = characterClothes[OutfitComponents.Armors],
                    Badge = characterClothes[OutfitComponents.Badges],
                    BeardChop = characterClothes[OutfitComponents.BeardChops],
                    Belt = characterClothes[OutfitComponents.Belts],
                    Beltbuckle = characterClothes[OutfitComponents.Beltbuckles],
                    Boot = characterClothes[OutfitComponents.Boots],
                    Bracelt = characterClothes[OutfitComponents.Bracelts],
                    Chap = characterClothes[OutfitComponents.Chaps],
                    Cloak = characterClothes[OutfitComponents.Cloaks],
                    Coat = characterClothes[OutfitComponents.Coats],
                    CoatClosed = characterClothes[OutfitComponents.CoatsClosed],
                    Eye = characterClothes[OutfitComponents.Eyes],
                    Eyewear = characterClothes[OutfitComponents.Eyewear],
                    FemaleUnknow01 = characterClothes[OutfitComponents.HairAccessories],
                    Gauntlet = characterClothes[OutfitComponents.Gauntlets],
                    Glove = characterClothes[OutfitComponents.Gloves],
                    Goatee = characterClothes[OutfitComponents.Goatees],
                    Gunbelt = characterClothes[OutfitComponents.Gunbelts],
                    Hair = characterClothes[OutfitComponents.Hairs],
                    Hat = characterClothes[OutfitComponents.Hats],
                    Head = characterClothes[OutfitComponents.Heads],
                    HolsterCrossdraw = characterClothes[OutfitComponents.HolsterCrossdraw],
                    HolsterLeft = characterClothes[OutfitComponents.HolstersLeft],
                    HolsterRight = characterClothes[OutfitComponents.HolstersRight],
                    Leg = characterClothes[OutfitComponents.Legs],
                    LegAttachement = characterClothes[OutfitComponents.LegAttachements],
                    Loadout = characterClothes[OutfitComponents.Loadouts],
                    Mask = characterClothes[OutfitComponents.Masks],
                    MaskLarge = characterClothes[OutfitComponents.MasksLarge],
                    Mustache = characterClothes[OutfitComponents.Mustache],
                    MustacheMP = characterClothes[OutfitComponents.MustacheMP],
                    Necktie = characterClothes[OutfitComponents.Neckties],
                    Neckwear = characterClothes[OutfitComponents.Neckwear],
                    Pant = characterClothes[OutfitComponents.Pants],
                    Poncho = characterClothes[OutfitComponents.Ponchos],
                    RingLeftHand = characterClothes[OutfitComponents.RingsLeftHand],
                    RingRightHand = characterClothes[OutfitComponents.RingsRightHand],
                    Satchel = characterClothes[OutfitComponents.Satchels],
                    Sheath = characterClothes[OutfitComponents.Sheaths],
                    Shirt = characterClothes[OutfitComponents.Shirts],
                    Skirt = characterClothes[OutfitComponents.Skirts],
                    Spat = characterClothes[OutfitComponents.Spats],
                    Spur = characterClothes[OutfitComponents.Spurs],
                    Suspender = characterClothes[OutfitComponents.Suspenders],
                    TalismanBelt = characterClothes[OutfitComponents.TalismanBelt],
                    TalismanHolster = characterClothes[OutfitComponents.TalismanHolster],
                    TalismanSatchel = characterClothes[OutfitComponents.TalismanSatchel],
                    TalismanWrist = characterClothes[OutfitComponents.TalismanWrist],
                    Teeth = characterClothes[OutfitComponents.Teeth],
                    Torso = characterClothes[OutfitComponents.Torsos],
                    Vest = characterClothes[OutfitComponents.Vests],
                },
                Position = new PositionData((float)_config["Locations"][_locationIndex]["X"], (float)_config["Locations"][_locationIndex]["Y"], (float)_config["Locations"][_locationIndex]["Z"], (float)_config["Locations"][_locationIndex]["H"]),
                Data = new Dictionary<string, object>()
            };

            _menuService.Close();
            Unfocus();

            _characterService.Create(characterData);
            _eventService.EmitServer("character:character_created", characterData.CharacterId);

            await FadeOut(500);
            await BaseScript.Delay(1000);

            RenderScriptCams(false, false, 0, true, true, 0);
            SetEntityCoords(PlayerPedId(), -169.93f, 626.56f, 114.23f, true, true, true, false);

            PauseClock(false, 0);
            SetWeatherTypeFrozen(false);
            FreezeEntityPosition(PlayerPedId(), false);

            _threadService.StopThread(WeatherUpdate);

            IsOpen = false;

            await BaseScript.Delay(1000);

            NetworkEndTutorialSession();

            await FadeIn(500);
        }

        internal void SwitchCamera(int type)
        {
            switch (type)
            {
                case 0:
                    SetCamActive(defaultCamera, true);
                    SetCamActive(faceCamera, false);
                    SetCamActive(bodyCamera, false);
                    SetCamActive(footCamera, false);
                    break;
                case 1:
                    SetCamActive(defaultCamera, false);
                    SetCamActive(faceCamera, true);
                    SetCamActive(bodyCamera, false);
                    SetCamActive(footCamera, false);
                    break;
                case 2:
                    SetCamActive(defaultCamera, false);
                    SetCamActive(faceCamera, false);
                    SetCamActive(bodyCamera, true);
                    SetCamActive(footCamera, false);
                    break;
                case 3:
                    SetCamActive(defaultCamera, false);
                    SetCamActive(faceCamera, false);
                    SetCamActive(bodyCamera, false);
                    SetCamActive(footCamera, true);
                    break;
            }
        }

        private void InitCamera(Vector3 spawnPosition, float heading)
        {
            var ped = PlayerPedId();

            var defaultCamCoordOffset = _config["DefaultCamera"]["CamCoordOffset"];
            var defaultPointCamOffset = _config["DefaultCamera"]["PointCamOffset"];
            var defaultCamFov = (float)_config["DefaultCamera"]["Fov"];

            var faceCamCoordOffset = _config["FaceCamera"]["CamCoordOffset"];
            var facePointCamOffset = _config["FaceCamera"]["PointCamOffset"];
            var faceCamFov = (float)_config["FaceCamera"]["Fov"];

            var bodyCamCoordOffset = _config["BodyCamera"]["CamCoordOffset"];
            var bodyPointCamOffset = _config["BodyCamera"]["PointCamOffset"];
            var bodyCamFov = (float)_config["BodyCamera"]["Fov"];

            var footCamCoordOffset = _config["FootCamera"]["CamCoordOffset"];
            var footPointCamOffset = _config["FootCamera"]["PointCamOffset"];
            var footCamFov = (float)_config["FootCamera"]["Fov"];

            var headHeight = GetPedBoneCoords(ped, 168, 0f, 0f, 0f).Z + (float)pedScaleItem.Value - 0.4f;
            var bodyHeight = GetPedBoneCoords(ped, 420, 0f, 0f, 0f).Z + (float)pedScaleItem.Value - 0.8f;

            defaultCamera = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", spawnPosition.X + (float)defaultCamCoordOffset[0], spawnPosition.Y + (float)defaultCamCoordOffset[1], spawnPosition.Z + (float)defaultCamCoordOffset[2], 0.0f, 0.0f, heading, defaultCamFov, false, 0);
            faceCamera = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", spawnPosition.X + (float)faceCamCoordOffset[0], spawnPosition.Y + (float)faceCamCoordOffset[1], spawnPosition.Z + (float)faceCamCoordOffset[2], 0.0f, 0.0f, heading, faceCamFov, false, 0);
            bodyCamera = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", spawnPosition.X + (float)bodyCamCoordOffset[0], spawnPosition.Y + (float)bodyCamCoordOffset[1], spawnPosition.Z + (float)bodyCamCoordOffset[2], 0.0f, 0.0f, heading, bodyCamFov, false, 0);
            footCamera = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", spawnPosition.X + (float)footCamCoordOffset[0], spawnPosition.Y + (float)footCamCoordOffset[1], spawnPosition.Z + (float)footCamCoordOffset[2], 0.0f, 0.0f, heading, footCamFov, false, 0);

            PointCamAtCoord(defaultCamera, spawnPosition.X + (float)defaultPointCamOffset[0], spawnPosition.Y + (float)defaultPointCamOffset[1], spawnPosition.Z + (float)defaultPointCamOffset[2]);

            PointCamAtCoord(faceCamera, spawnPosition.X + (float)facePointCamOffset[0], spawnPosition.Y + (float)facePointCamOffset[1], headHeight + (float)facePointCamOffset[2]);
            SetCamCoord(faceCamera, spawnPosition.X + (float)faceCamCoordOffset[0], spawnPosition.Y + (float)faceCamCoordOffset[1], headHeight + (float)faceCamCoordOffset[2]);

            PointCamAtCoord(bodyCamera, spawnPosition.X + (float)bodyPointCamOffset[0], spawnPosition.Y + (float)bodyPointCamOffset[1], bodyHeight + (float)bodyPointCamOffset[2]);
            SetCamCoord(bodyCamera, spawnPosition.X + (float)bodyCamCoordOffset[0], spawnPosition.Y + (float)bodyCamCoordOffset[1], bodyHeight + (float)bodyCamCoordOffset[2]);

            PointCamAtCoord(footCamera, spawnPosition.X + (float)footPointCamOffset[0], spawnPosition.Y + (float)footPointCamOffset[1], bodyHeight + (float)footPointCamOffset[2]);
            SetCamCoord(footCamera, spawnPosition.X + (float)footCamCoordOffset[0], spawnPosition.Y + (float)footCamCoordOffset[1], bodyHeight + (float)footCamCoordOffset[2]);

            SwitchCamera(0);
            RenderScriptCams(true, true, 750, true, true, 0);
        }

        internal async void StartCreator()
        {
            _threadService.StartThread(WeatherUpdate);

            await _characterService.SpawnPed(gender);

            var ped = PlayerPedId();
            var x = (float)_config["CreationPosition"]["X"];
            var y = (float)_config["CreationPosition"]["Y"];
            var z = (float)_config["CreationPosition"]["Z"];
            var h = (float)_config["CreationPosition"]["H"];

            RequestCollisionAtCoord(x, y, z);
            Call(0xEA23C49EAA83ACFB, x, y, z, h, true, true, false);

            var timer = GetGameTimer();
            while (!HasCollisionLoadedAroundEntity(ped) && GetGameTimer() - timer < 5000) await BaseScript.Delay(0);

            SetEntityCoords(ped, x, y, z, true, true, true, true);
            SetEntityHeading(ped, h);

            SetWeatherType((uint)Weather.Sunny, true, true, true, 0f, false);
            SetWeatherTypeFrozen(true);
            NetworkClockTimeOverride(12, 0, 0, 1, true);
            PauseClock(true, 0);

            FreezeEntityPosition(ped, false);

            InitClothMenu();
            InitFaceMenu();
            InitfaceFeaturesMenu();
            InitFaceOverlayMenu();
            InitInfoMenu();

            _menuService.Open(infoMenu);
            _uiService.FocusFrame("menu");
            _uiService.Focus();

            InitCamera(new Vector3(x, y, z), h);
            InitDefaultPed();

            IsOpen = true;
        }

        private void InitDefaultPed()
        {
            origin = gender == Gender.Male ? CharacterUtilities.Origins[0] : CharacterUtilities.Origins[6];
            origins = CharacterUtilities.Origins.Where(x => x.Gender == gender).ToList();

            culturesItem.Text = $"Ethnie: {culturesItem.Value}";
            culturesItem.OnUpdate(_uiService);

            headsItem.Min = 0;
            headsItem.Max = origin.Heads.Count - 1;
            headsItem.Value = 0;
            headsItem.OnUpdate(_uiService);

            bodyItem.Min = 0;
            bodyItem.Max = origin.Bodies.Count - 1;
            bodyItem.Value = 0;
            bodyItem.OnUpdate(_uiService);

            legsItem.Min = 0;
            legsItem.Max = origin.Legs.Count - 1;
            legsItem.Value = 0;
            legsItem.OnUpdate(_uiService);

            UpdatePedBodyComponent(origin.Heads, headsItem, _languageService.Get(OutfitComponents.Heads));
            UpdatePedBodyComponent(origin.Bodies, bodyItem, _languageService.Get(OutfitComponents.Torsos));
            UpdatePedBodyComponent(origin.Legs, legsItem, _languageService.Get(OutfitComponents.Legs));

            RandomizeFace();

            InitPedOverlay();
            InitDefaultPedComponents();
        }

        private async void InitDefaultPedComponents()
        {
            var ped = PlayerPedId();

            // Define max cloth value by gender
            skirtsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Loadouts).Count - 1;
            hairItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Hairs).Count - 1;
            eyesItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Eyes).Count - 1;
            teethItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Teeth).Count - 1;
            braceletsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Bracelts).Count - 1;
            ringsLeftHandItem.Max = GetOutfitComponentsByCategory(OutfitComponents.RingsLeftHand).Count - 1;
            ringsRightHandItem.Max = GetOutfitComponentsByCategory(OutfitComponents.RingsRightHand).Count - 1;
            hatsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Hats).Count - 1;
            shirtsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Shirts).Count - 1;
            vestItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Vests).Count - 1;
            pantsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Pants).Count - 1;
            neckwearItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Neckwear).Count - 1;
            bootsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Boots).Count - 1;
            accessoriesItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Accessories).Count - 1;
            spursItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Spurs).Count - 1;
            chapsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Chaps).Count - 1;
            cloakItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Cloaks).Count - 1;
            masksItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Masks).Count - 1;
            spatsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Spats).Count - 1;
            gauntletsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Gauntlets).Count - 1;
            necktiesItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Neckties).Count - 1;
            suspendersItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Suspenders).Count - 1;
            gunbeltItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Gunbelts).Count - 1;
            beltItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Belts).Count - 1;
            buckleItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Beltbuckles).Count - 1;
            coatsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Coats).Count - 1;
            ponchosItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Ponchos).Count - 1;
            glovesItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Gloves).Count - 1;
            satchelsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Satchels).Count - 1;
            legAttachmentsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.LegAttachements).Count - 1;
            holstersCrossdrawItem.Max = GetOutfitComponentsByCategory(OutfitComponents.HolsterCrossdraw).Count - 1;
            holstersLeftItem.Max = GetOutfitComponentsByCategory(OutfitComponents.HolstersLeft).Count - 1;
            holstersRightItem.Max = GetOutfitComponentsByCategory(OutfitComponents.HolstersRight).Count - 1;
            eyewearItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Eyewear).Count - 1;
            masksLargeItem.Max = GetOutfitComponentsByCategory(OutfitComponents.MasksLarge).Count - 1;
            coatsClosedItem.Max = GetOutfitComponentsByCategory(OutfitComponents.CoatsClosed).Count - 1;
            loadoutsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Loadouts).Count - 1;
            sheathsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Sheaths).Count - 1;
            apronsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Aprons).Count - 1;
            beardChopsItem.Max = GetOutfitComponentsByCategory(OutfitComponents.BeardChops).Count - 1;
            mustacheItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Mustache).Count - 1;
            mustacheMpItem.Max = GetOutfitComponentsByCategory(OutfitComponents.MustacheMP).Count - 1;
            goateesItem.Max = GetOutfitComponentsByCategory(OutfitComponents.Goatees).Count - 1;
            femaleUnknow01Item.Max = GetOutfitComponentsByCategory(OutfitComponents.HairAccessories).Count - 1;
            talismanBeltItem.Max = GetOutfitComponentsByCategory(OutfitComponents.TalismanBelt).Count - 1;
            talismanHolsterItem.Max = GetOutfitComponentsByCategory(OutfitComponents.TalismanHolster).Count - 1;
            talismanSatchelItem.Max = GetOutfitComponentsByCategory(OutfitComponents.TalismanSatchel).Count - 1;
            talismanWristItem.Max = GetOutfitComponentsByCategory(OutfitComponents.TalismanWrist).Count - 1;

            skirtsItem.OnUpdate(_uiService);
            hairItem.OnUpdate(_uiService);
            eyesItem.OnUpdate(_uiService);
            teethItem.OnUpdate(_uiService);
            braceletsItem.OnUpdate(_uiService);
            ringsLeftHandItem.OnUpdate(_uiService);
            ringsRightHandItem.OnUpdate(_uiService);
            hatsItem.OnUpdate(_uiService);
            shirtsItem.OnUpdate(_uiService);
            vestItem.OnUpdate(_uiService);
            pantsItem.OnUpdate(_uiService);
            neckwearItem.OnUpdate(_uiService);
            bootsItem.OnUpdate(_uiService);
            accessoriesItem.OnUpdate(_uiService);
            spursItem.OnUpdate(_uiService);
            chapsItem.OnUpdate(_uiService);
            cloakItem.OnUpdate(_uiService);
            masksItem.OnUpdate(_uiService);
            spatsItem.OnUpdate(_uiService);
            gauntletsItem.OnUpdate(_uiService);
            necktiesItem.OnUpdate(_uiService);
            suspendersItem.OnUpdate(_uiService);
            gunbeltItem.OnUpdate(_uiService);
            beltItem.OnUpdate(_uiService);
            buckleItem.OnUpdate(_uiService);
            coatsItem.OnUpdate(_uiService);
            ponchosItem.OnUpdate(_uiService);
            glovesItem.OnUpdate(_uiService);
            satchelsItem.OnUpdate(_uiService);
            legAttachmentsItem.OnUpdate(_uiService);
            holstersCrossdrawItem.OnUpdate(_uiService);
            holstersLeftItem.OnUpdate(_uiService);
            holstersRightItem.OnUpdate(_uiService);
            eyewearItem.OnUpdate(_uiService);
            masksLargeItem.OnUpdate(_uiService);
            coatsClosedItem.OnUpdate(_uiService);
            loadoutsItem.OnUpdate(_uiService);
            sheathsItem.OnUpdate(_uiService);
            apronsItem.OnUpdate(_uiService);
            beardChopsItem.OnUpdate(_uiService);
            mustacheItem.OnUpdate(_uiService);
            mustacheMpItem.OnUpdate(_uiService);
            goateesItem.OnUpdate(_uiService);
            femaleUnknow01Item.OnUpdate(_uiService);
            talismanBeltItem.OnUpdate(_uiService);
            talismanHolsterItem.OnUpdate(_uiService);
            talismanSatchelItem.OnUpdate(_uiService);
            talismanWristItem.OnUpdate(_uiService);

            if (gender == Gender.Male)
            {
                var beardType = new Random(Environment.TickCount).Next(0, 1);

                if (beardType == 0)
                {
                    beardChopsItem.Value = new Random(Environment.TickCount + 1).Next(0, GetOutfitComponentsByCategory(OutfitComponents.BeardChops).Count - 1);
                    goateesItem.Value = new Random(Environment.TickCount + 2).Next(0, GetOutfitComponentsByCategory(OutfitComponents.Goatees).Count - 1);
                    mustacheItem.Value = new Random(Environment.TickCount + 3).Next(0, GetOutfitComponentsByCategory(OutfitComponents.Mustache).Count - 1);

                    //beardChopsItem.OnUpdate(_uiService);
                    //goateesItem.OnUpdate(_uiService);
                    //mustacheItem.OnUpdate(_uiService);
                }
                else
                {
                    mustacheMpItem.Value = new Random(Environment.TickCount + 4).Next(0, GetOutfitComponentsByCategory(OutfitComponents.MustacheMP).Count - 1);
                    //mustacheMpItem.OnUpdate(_uiService);
                }

                // Male
                beardChopsItem.Visible = true;
                goateesItem.Visible = true;
                mustacheItem.Visible = true;
                mustacheMpItem.Visible = true;

                // Female
                skirtsItem.Visible = false;
                femaleUnknow01Item.Visible = false;
            }
            else
            {
                // Male
                beardChopsItem.Visible = false;
                goateesItem.Visible = false;
                mustacheItem.Visible = false;
                mustacheMpItem.Visible = false;

                // Female
                skirtsItem.Visible = true;
                femaleUnknow01Item.Visible = true;
            }

            beardChopsItem.OnUpdate(_uiService);
            goateesItem.OnUpdate(_uiService);
            mustacheItem.OnUpdate(_uiService);
            mustacheMpItem.OnUpdate(_uiService);

            skirtsItem.OnUpdate(_uiService);
            femaleUnknow01Item.OnUpdate(_uiService);

            if ((int)sheathsItem.Max > -1) sheathsItem.Visible = true;
            else sheathsItem.Visible = false;

            if ((int)apronsItem.Max > -1) apronsItem.Visible = true;
            else apronsItem.Visible = false;

            if ((int)talismanBeltItem.Max > -1) talismanBeltItem.Visible = true;
            else talismanBeltItem.Visible = false;

            if ((int)talismanHolsterItem.Max > -1) talismanHolsterItem.Visible = true;
            else talismanHolsterItem.Visible = false;

            if ((int)talismanSatchelItem.Max > -1) talismanSatchelItem.Visible = true;
            else talismanSatchelItem.Visible = false;

            if ((int)talismanWristItem.Max > -1) talismanWristItem.Visible = true;
            else talismanWristItem.Visible = false;

            if ((int)holstersCrossdrawItem.Max > -1) holstersCrossdrawItem.Visible = true;
            else holstersCrossdrawItem.Visible = false;

            if ((int)holstersRightItem.Max > -1) holstersRightItem.Visible = true;
            else holstersRightItem.Visible = false;

            if ((int)masksLargeItem.Max > -1) masksLargeItem.Visible = true;
            else masksLargeItem.Visible = false;

            sheathsItem.OnUpdate(_uiService);
            apronsItem.OnUpdate(_uiService);
            talismanBeltItem.OnUpdate(_uiService);
            talismanHolsterItem.OnUpdate(_uiService);
            talismanSatchelItem.OnUpdate(_uiService);
            talismanWristItem.OnUpdate(_uiService);
            holstersCrossdrawItem.OnUpdate(_uiService);
            holstersRightItem.OnUpdate(_uiService);
            masksLargeItem.OnUpdate(_uiService);

            hairItem.Value = new Random(Environment.TickCount + 5).Next(-1, GetOutfitComponentsByCategory(OutfitComponents.Hairs).Count - 1);
            eyesItem.Value = new Random(Environment.TickCount + 6).Next(-1, GetOutfitComponentsByCategory(OutfitComponents.Eyes).Count - 1);
            teethItem.Value = new Random(Environment.TickCount + 7).Next(-1, GetOutfitComponentsByCategory(OutfitComponents.Teeth).Count - 1);

            hairItem.OnUpdate(_uiService);
            eyesItem.OnUpdate(_uiService);
            teethItem.OnUpdate(_uiService);

            if (gender == Gender.Male)
            {
                await SetPedComponent(OutfitComponents.BeardChops, beardChopsItem);
                await SetPedComponent(OutfitComponents.Mustache, mustacheItem);
                await SetPedComponent(OutfitComponents.MustacheMP, mustacheMpItem);
                await SetPedComponent(OutfitComponents.Goatees, goateesItem);
            }

            await SetPedComponent(OutfitComponents.Hairs, hairItem);
            await SetPedComponent(OutfitComponents.Eyes, eyesItem);
            await SetPedComponent(OutfitComponents.Teeth, teethItem);

            RemovePedComponent(OutfitComponents.Pants);

            bodyTypesItem.Value = new Random(Environment.TickCount + 8).Next(0, _characterService._bodyTypes.Count - 1);
            waistTypesItem.Value = new Random(Environment.TickCount + 9).Next(0, _characterService._waistTypes.Count - 1);

            bodyTypesItem.OnUpdate(_uiService);
            waistTypesItem.OnUpdate(_uiService);

            SetPedBodyComponent(ped, _characterService._bodyTypes, (int)bodyTypesItem.Value);
            SetPedBodyComponent(ped, _characterService._waistTypes, (int)waistTypesItem.Value);

            await SetPedComponent(OutfitComponents.Hats, hatsItem);
            await SetPedComponent(OutfitComponents.Eyewear, eyewearItem);
            await SetPedComponent(OutfitComponents.Neckwear, neckwearItem);
            await SetPedComponent(OutfitComponents.Neckties, necktiesItem);
            await SetPedComponent(OutfitComponents.Shirts, shirtsItem);
            await SetPedComponent(OutfitComponents.Suspenders, suspendersItem);
            await SetPedComponent(OutfitComponents.Vests, vestItem);
            await SetPedComponent(OutfitComponents.Coats, coatsItem);
            await SetPedComponent(OutfitComponents.CoatsClosed, coatsClosedItem);
            await SetPedComponent(OutfitComponents.Ponchos, ponchosItem);
            await SetPedComponent(OutfitComponents.Cloaks, cloakItem);
            await SetPedComponent(OutfitComponents.Gloves, glovesItem);
            await SetPedComponent(OutfitComponents.RingsRightHand, ringsRightHandItem);
            await SetPedComponent(OutfitComponents.RingsLeftHand, ringsLeftHandItem);
            await SetPedComponent(OutfitComponents.Bracelts, braceletsItem);
            await SetPedComponent(OutfitComponents.Gunbelts, gunbeltItem);
            await SetPedComponent(OutfitComponents.Belts, beltItem);
            await SetPedComponent(OutfitComponents.Beltbuckles, buckleItem);
            await SetPedComponent(OutfitComponents.HolsterCrossdraw, holstersCrossdrawItem);
            await SetPedComponent(OutfitComponents.HolstersLeft, holstersLeftItem);
            await SetPedComponent(OutfitComponents.HolstersRight, holstersRightItem);
            await SetPedComponent(OutfitComponents.Pants, pantsItem);

            if (gender == Gender.Female)
            {
                await SetPedComponent(OutfitComponents.Skirts, skirtsItem);
                await SetPedComponent(OutfitComponents.HairAccessories, femaleUnknow01Item);
            }

            await SetPedComponent(OutfitComponents.Boots, bootsItem);
            await SetPedComponent(OutfitComponents.Chaps, chapsItem);
            await SetPedComponent(OutfitComponents.Spurs, spursItem);
            await SetPedComponent(OutfitComponents.Spats, spatsItem);
            await SetPedComponent(OutfitComponents.Satchels, satchelsItem);
            await SetPedComponent(OutfitComponents.Masks, masksItem);
            await SetPedComponent(OutfitComponents.MasksLarge, masksLargeItem);
            await SetPedComponent(OutfitComponents.Loadouts, loadoutsItem);
            await SetPedComponent(OutfitComponents.LegAttachements, legAttachmentsItem);
            await SetPedComponent(OutfitComponents.Gauntlets, gauntletsItem);
            await SetPedComponent(OutfitComponents.Accessories, accessoriesItem);
            await SetPedComponent(OutfitComponents.Sheaths, sheathsItem);
            await SetPedComponent(OutfitComponents.Aprons, apronsItem);
            await SetPedComponent(OutfitComponents.TalismanBelt, talismanBeltItem);
            await SetPedComponent(OutfitComponents.TalismanHolster, talismanHolsterItem);
            await SetPedComponent(OutfitComponents.TalismanSatchel, talismanSatchelItem);
            await SetPedComponent(OutfitComponents.TalismanWrist, talismanWristItem);

            UpdatePedVariation(PlayerPedId());
        }

        private void InitInfoMenu()
        {
            var ped = PlayerPedId();

            var genders = new List<object>
            {
                "Homme",
                "Femme",
                "Autre"
            };

            genderItem = new(_languageService.Get("Client.CharacterCreator.Sex"), genders, async (item, selectType, value) =>
            {
                switch ((string)value)
                {
                    case "Homme":
                        gender = Gender.Male;
                        break;
                    case "Femme":
                        gender = Gender.Female;
                        break;
                    case "Autre":
                        gender = Gender.Other;
                        break;
                }

                var model = gender == Gender.Male ? (uint)GetHashKey("mp_male") : (uint)GetHashKey("mp_female");

                await FadeOut(250);
                await LoadModel((uint)GetHashKey("mp_male"));
                await LoadModel((uint)GetHashKey("mp_female"));

                SetPlayerModel(model);
                SetPedOutfitPreset(PlayerPedId(), 0);

                await BaseScript.Delay(0);
                InitDefaultPed();
                await BaseScript.Delay(0);

                SetPedComponentDisabled(PlayerPedId(), 0x3F1F01E5, 0, false);
                SetPedComponentDisabled(PlayerPedId(), 0xDA0E2C55, 0, false);

                UpdatePedVariation();

                await BaseScript.Delay(750);

                await FadeIn(250);

                if (gender == Gender.Female)
                {
                    clothesTopContainer.AddItem(skirtsItem);
                    clothesTopContainer.AddItem(femaleUnknow01Item);

                    beardChopsItem.Visible = false;
                    mustacheItem.Visible = false;
                    mustacheMpItem.Visible = false;
                    goateesItem.Visible = false;
                }
                else
                {
                    clothesTopContainer.RemoveItem(skirtsItem);
                    clothesTopContainer.RemoveItem(femaleUnknow01Item);

                    beardChopsItem.Visible = true;
                    mustacheItem.Visible = true;
                    mustacheMpItem.Visible = true;
                    goateesItem.Visible = true;
                }

                beardChopsItem.OnUpdate(_uiService);
                mustacheItem.OnUpdate(_uiService);
                mustacheMpItem.OnUpdate(_uiService);
                goateesItem.OnUpdate(_uiService);
            }, 0);

            firstAndLastNameItem = new("Identité", new(0, 20, _languageService.Get("Client.CharacterCreator.Firstname"), "", (item) => item.Primary.Value.ToString().Length > item.Primary.MinLength), new(0, 20, _languageService.Get("Client.CharacterCreator.Lastname"), "", (item) => item.Secondary.Value.ToString().Length > item.Secondary.MinLength), null);
            nationalityAndCityBirthItem = new("Origines", new(0, 30, _languageService.Get("Client.CharacterCreator.Nationality"), "", (item) => item.Primary.Value.ToString().Length > item.Primary.MinLength), new(0, 30, _languageService.Get("Client.CharacterCreator.PlaceOfBirth"), "", (item) => item.Secondary.Value.ToString().Length > item.Secondary.MinLength), null);
            dateOfBirthItem = new(_languageService.Get("Client.CharacterCreator.DateOfBirth"), new(2, 2, "01", "", (item) =>
            {
                var primaryValue = int.Parse(item.Primary.Value.ToString());

                if (!primaryValue.ToString().All(char.IsNumber)) return false;
                if (primaryValue.ToString().Length > item.Primary.MinLength) return false;
                if (primaryValue < 1 || primaryValue > 31) return false;

                return true;
            }), new(2, 2, "05", "", (item) =>
            {
                var secondaryValue = int.Parse(item.Secondary.Value.ToString());

                if (!secondaryValue.ToString().All(char.IsNumber)) return false;
                if (secondaryValue.ToString().Length > item.Secondary.MinLength) return false;
                if (secondaryValue < 1 || secondaryValue > 12) return false;

                return true;
            }), new(4, 4, "1865", "", (item) =>
            {
                var tertiaryValue = int.Parse(item.Tertiary.Value.ToString());

                if (!tertiaryValue.ToString().All(char.IsNumber)) return false;
                if (tertiaryValue.ToString().Length > item.Tertiary.MinLength) return false;
                if (tertiaryValue < 1820 || tertiaryValue > 1880) return false;

                return true;
            }), null);

            pedScaleItem = new($"{_languageService.Get("Client.CharacterCreator.Height")}: ", 0.95f, 1.05f, 0.01f, 1f, typeof(float), async (item, selectType, value) =>
            {
                var ped = PlayerPedId();
                var size = (int)(((float)value - 0.2f) * 100f); //0.2

                pedScaleItem.Text = $"{_languageService.Get("Client.CharacterCreator.Height")}: 1m{ (size == 100 ? "00" : size.ToString()) }";
                pedScaleItem.OnUpdate(_uiService);

                GameAPI.SetPedScale(PlayerPedId(), (float)value);
            });

            // Default ped culture
            origin = CharacterUtilities.Origins[0];

            // Default ped head texture
            textureType["albedo"] = GetHashKey(origin.HeadTexture);

            // Select culture by gender
            origins = CharacterUtilities.Origins.Where(x => x.Gender == gender).ToList();

            culturesItem = new("Ethnie: 0", 0, origins.Count - 1, 1, 0, typeof(int), (item, selectType, value) =>
            {
                culturesItem.Text = $"Ethnie: {value}";
                culturesItem.OnUpdate(_uiService);

                origin = origins[(int)culturesItem.Value];
                textureType["albedo"] = GetHashKey(origin.HeadTexture);

                headsItem.Min = 0;
                headsItem.Max = origin.Heads.Count - 1;
                headsItem.Value = 0;
                headsItem.OnUpdate(_uiService);

                bodyItem.Min = 0;
                bodyItem.Max = origin.Bodies.Count - 1;
                bodyItem.Value = 0;
                bodyItem.OnUpdate(_uiService);

                legsItem.Min = 0;
                legsItem.Max = origin.Legs.Count - 1;
                legsItem.Value = 0;
                legsItem.OnUpdate(_uiService);

                UpdatePedBodyComponent(origin.Heads, headsItem, _languageService.Get(OutfitComponents.Heads));
                UpdatePedBodyComponent(origin.Bodies, bodyItem, _languageService.Get(OutfitComponents.Torsos));
                UpdatePedBodyComponent(origin.Legs, legsItem, _languageService.Get(OutfitComponents.Legs));
            });

            headsItem = new(_languageService.Get(OutfitComponents.Heads), 0, origin.Heads.Count - 1, 1, 0, typeof(int), (item, selectType, value) =>
            {
                UpdatePedBodyComponent(origin.Heads, headsItem, _languageService.Get(OutfitComponents.Heads));
            });

            bodyItem = new(_languageService.Get(OutfitComponents.Torsos), 0, origin.Bodies.Count - 1, 1, 0, typeof(int), (item, selectType, value) =>
            {
                UpdatePedBodyComponent(origin.Bodies, bodyItem, _languageService.Get(OutfitComponents.Torsos));
            });

            legsItem = new(_languageService.Get(OutfitComponents.Legs), 0, origin.Legs.Count - 1, 1, 0, typeof(int), (item, selectType, value) =>
            {
                UpdatePedBodyComponent(origin.Legs, legsItem, _languageService.Get(OutfitComponents.Legs));
            });

            bodyTypesItem = new("Morphologie", 0, _characterService._bodyTypes.Count - 1, 1, 0, typeof(int), (item, selectType, value) =>
            {
                bodyTypesItem.Text = $"{_languageService.Get("Client.CharacterCreator.Morphology")}";

                SetPedBodyComponent(ped, (uint)_characterService._bodyTypes[(int)bodyTypesItem.Value]);
            });

            waistTypesItem = new("Poids", 0, _characterService._waistTypes.Count - 1, 1, 0, typeof(int), (item, selectType, value) =>
            {
                waistTypesItem.Text = $"{_languageService.Get("Client.CharacterCreator.Weight")}";

                SetPedBodyComponent(ped, (uint)_characterService._waistTypes[(int)waistTypesItem.Value]);
            });

            infoTopContainer.AddItem(genderItem);
            infoTopContainer.AddItem(firstAndLastNameItem);
            infoTopContainer.AddItem(nationalityAndCityBirthItem);
            infoTopContainer.AddItem(dateOfBirthItem);
            infoTopContainer.AddItem(dateOfBirthItem);
            infoTopContainer.AddItem(pedScaleItem);
            infoTopContainer.AddItem(culturesItem);
            infoTopContainer.AddItem(headsItem);
            infoTopContainer.AddItem(bodyItem);
            infoTopContainer.AddItem(legsItem);
            infoTopContainer.AddItem(bodyTypesItem);
            infoTopContainer.AddItem(waistTypesItem);

            infoBottomContainer.AddItem(new BottomButtonItem("Visage", item =>
            {
                _menuService.Open(faceMenu);
            }));

            infoBottomContainer.AddItem(new BottomButtonItem("Vêtements", item =>
            {
                _menuService.Open(clothesMenu);
            }));

            infoBottomContainer.AddItem(new BottomButtonItem("Attributs", item =>
            {
                _menuService.Open(attributesMenu);
            }));

            infoBottomContainer.AddItem(new BottomButtonItem("Créer le personnage", item =>
            {
                CreateCharacter();
            }));
        }

        private List<Cloth> GetOutfitComponentsByCategory(string categoryHash)
        {
            switch (categoryHash)
            {
                case OutfitComponents.TalismanBelt:
                case OutfitComponents.TalismanHolster:
                case OutfitComponents.TalismanSatchel:
                case OutfitComponents.TalismanWrist:
                case OutfitComponents.Aprons:
                case OutfitComponents.HairAccessories:
                case OutfitComponents.Gunbelts:
                case OutfitComponents.Goatees:
                case OutfitComponents.BeardChops:
                case OutfitComponents.Mustache:
                case OutfitComponents.MustacheMP:
                case OutfitComponents.Belts:
                case OutfitComponents.Beltbuckles:
                case OutfitComponents.Badges:
                case OutfitComponents.Armors:
                case OutfitComponents.Satchels:
                case OutfitComponents.Sheaths:
                case OutfitComponents.Skirts:
                case OutfitComponents.Spats:
                case OutfitComponents.Spurs:
                case OutfitComponents.Suspenders:
                case OutfitComponents.Hats:
                case OutfitComponents.Eyes:
                case OutfitComponents.Eyewear:
                case OutfitComponents.Neckties:
                case OutfitComponents.Neckwear:
                    return _characterService._clothes.Where(x => x.CategoryHash == categoryHash && x.PedType == (int)gender).ToList();
            }

            return _characterService._clothes.Where(x => x.CategoryHash == categoryHash && x.IsMultiplayer && x.PedType == (int)gender).ToList();
        }

        private void InitFaceMenu()
        {
            eyesItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Eyes"), 0, GetOutfitComponentsByCategory(OutfitComponents.Eyes).Count - 1, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Eyes, item);
            });

            hairItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Hair"), -1, GetOutfitComponentsByCategory(OutfitComponents.Hairs).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Hairs, item);
            });

            beardChopsItem = new SelectSliderItem("Favori", -1, GetOutfitComponentsByCategory(OutfitComponents.BeardChops).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.BeardChops, item);
            }, gender == Gender.Male);

            mustacheItem = new SelectSliderItem("Moustache", -1, GetOutfitComponentsByCategory(OutfitComponents.Mustache).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Mustache, item);
            }, gender == Gender.Male);

            mustacheMpItem = new SelectSliderItem("Barbe", -1, GetOutfitComponentsByCategory(OutfitComponents.MustacheMP).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.MustacheMP, item);
            }, gender == Gender.Male);

            goateesItem = new SelectSliderItem("Boucs", -1, GetOutfitComponentsByCategory(OutfitComponents.Goatees).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Goatees, item);
            }, gender == Gender.Male);

            teethItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Teeth"), 0, GetOutfitComponentsByCategory(OutfitComponents.Teeth).Count - 1, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Teeth, item);
            });

            faceTopContainer.AddItem(eyesItem);
            faceTopContainer.AddItem(hairItem);
            faceTopContainer.AddItem(teethItem);
            faceTopContainer.AddItem(beardChopsItem);
            faceTopContainer.AddItem(mustacheItem);
            faceTopContainer.AddItem(mustacheMpItem);
            faceTopContainer.AddItem(goateesItem);

            faceBottomContainer.AddItem(new BottomButtonItem(_languageService.Get("Client.CharacterCreator.FaceTraits"), (item) =>
            {
                _menuService.Open(faceFeaturesMenu);
            }));

            faceBottomContainer.AddItem(new BottomButtonItem(_languageService.Get("Client.CharacterCreator.Facies"), (item) =>
            {
                _menuService.Open(faceOverlayMenu);
            }));
        }

        private void InitfaceFeaturesMenu()
        {
            var ped = PlayerPedId();

            //faceTopContainer.AddItem(new RedirectButtonItem(_languageService.Get("Client.CharacterCreator.FaceTraits"), faceFeaturesMenu, (item) => { }));

            faceFeaturesTopContainer.AddItem(new ButtonItem(_languageService.Get("Client.CharacterCreator.RandomFace"), (item) =>
            {
                RandomizeFace();
            }));

            headWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.HeadWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.HeadWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.HeadWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(headWidthItem);

            eyebrowHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyebrowsHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyebrowHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyebrowHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyebrowHeightItem);

            eyebrowWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyebrowsWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyebrowWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyebrowWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyebrowWidthItem);

            eyebrowDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyebrowsDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyebrowDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyebrowDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyebrowDepthItem);

            earsWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EarsWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EarsWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.EarsWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(earsWidthItem);

            earsAngleItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EarsCurvature"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EarsAngle, (float)item.Value);
                characterFaceParts[CharacterFacePart.EarsAngle] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(earsAngleItem);

            earsHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EarsSize"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EarsHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.EarsHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(earsHeightItem);

            earsLobeSizeItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LobeSize"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EarsLobeSize, (float)item.Value);
                characterFaceParts[CharacterFacePart.EarsLobeSize] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(earsLobeSizeItem);

            cheeckBonesHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.CheekbonesHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.CheeckBonesHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.CheeckBonesHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(cheeckBonesHeightItem);

            cheeckBonesWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.CheekbonesWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.CheeckBonesWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.CheeckBonesWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(cheeckBonesWidthItem);

            cheeckBonesDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.CheekbonesDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.CheeckBonesDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.CheeckBonesDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(cheeckBonesDepthItem);

            jawHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.JawHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.JawHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.JawHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(jawHeightItem);

            jawWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.JawWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.JawWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.JawWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(jawWidthItem);

            jawDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.JawDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.JawDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.JawDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(jawDepthItem);

            chinHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.ChinHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.ChinHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.ChinHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(chinHeightItem);

            chinWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.ChinWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.ChinWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.ChinWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(chinWidthItem);

            chinDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.ChinDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.ChinDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.ChinDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(chinDepthItem);

            eyeLidHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyelidHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyeLidHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyeLidHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyeLidHeightItem);

            eyeLidWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyelidWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyeLidWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyeLidWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyeLidWidthItem);

            eyesDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyesDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyesDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyesDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyesDepthItem);

            eyesAngleItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyesAngle"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyesAngle, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyesAngle] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyesAngleItem);

            eyesDistanceItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyesDistance"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyesDistance, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyesDistance] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyesDistanceItem);

            eyesHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.EyesHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.EyesHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.EyesHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(eyesHeightItem);

            noseWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.NoseWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.NoseWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.NoseWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(noseWidthItem);

            noseSizeItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.NoseSize"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.NoseSize, (float)item.Value);
                characterFaceParts[CharacterFacePart.NoseSize] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(noseSizeItem);

            noseHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.NoseHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.NoseHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.NoseHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(noseHeightItem);

            noseAngleItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.NoseAngle"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.NoseAngle, (float)item.Value);
                characterFaceParts[CharacterFacePart.NoseAngle] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(noseAngleItem);

            noseCurvatureItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.NoseCurvature"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.NoseCurvature, (float)item.Value);
                characterFaceParts[CharacterFacePart.NoseCurvature] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(noseCurvatureItem);

            noStrilsDistanceItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.NostrilsDistance"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.NoStrilsDistance, (float)item.Value);
                characterFaceParts[CharacterFacePart.NoStrilsDistance] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(noStrilsDistanceItem);

            mouthWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.MouthWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.MouthWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.MouthWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(mouthWidthItem);

            mouthDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.MouthDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.MouthDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.MouthDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(mouthDepthItem);

            mouthXPosItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.MouthHorzPos"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.MouthXPos, (float)item.Value);
                characterFaceParts[CharacterFacePart.MouthXPos] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(mouthXPosItem);

            mouthYPosItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.MouthVertPos"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.MouthYPos, (float)item.Value);
                characterFaceParts[CharacterFacePart.MouthYPos] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(mouthYPosItem);

            upperLipHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LipsSupHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.UpperLipHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.UpperLipHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(upperLipHeightItem);

            upperLipWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LipsSupWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.UpperLipWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.UpperLipWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(upperLipWidthItem);

            upperLipDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LipsSupDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.UpperLipDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.UpperLipDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(upperLipDepthItem);

            lowerLipHeightItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LipsInfHeight"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.LowerLipHeight, (float)item.Value);
                characterFaceParts[CharacterFacePart.LowerLipHeight] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(lowerLipHeightItem);

            lowerLipWidthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LipsInfWidth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.LowerLipWidth, (float)item.Value);
                characterFaceParts[CharacterFacePart.LowerLipWidth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(lowerLipWidthItem);

            lowerLipDepthItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.LipsInfDepth"), -1f, 1f, 0.1f, 0f, typeof(float), (item, selectType, value) =>
            {
                SetPedFaceFeature(ped, CharacterFacePart.LowerLipDepth, (float)item.Value);
                characterFaceParts[CharacterFacePart.LowerLipDepth] = (float)item.Value;
            });
            faceFeaturesTopContainer.AddItem(lowerLipDepthItem);
        }

        private void InitPedOverlay()
        {
            if (gender == Gender.Male)
            {
                textureType["albedo"] = GetHashKey("mp_head_mr1_sc08_c0_000_ab");
                textureType["normal"] = GetHashKey("mp_head_mr1_000_nm");
                textureType["material"] = 0x7FC5B1E1;
                textureType["color_type"] = 1;
                textureType["texture_opacity"] = 1.0f;
                textureType["unk_arg"] = 0;
            }
            else
            {
                textureType["albedo"] = GetHashKey("mp_head_fr1_sc08_c0_000_ab");
                textureType["normal"] = GetHashKey("head_fr1_mp_002_nm");
                textureType["material"] = 0x7FC5B1E1;
                textureType["color_type"] = 1;
                textureType["texture_opacity"] = 1.0f;
                textureType["unk_arg"] = 0;
            }
        }

        private void InitFaceOverlayMenu()
        {
            var overlays = CharacterUtilities.FaceOverlays.Where(x => x.Name == "eyebrows").ToList();
            var overlayInfo = overlays[0];

            characterOverlays[overlayInfo.Name].TextureId = overlayInfo.Id;
            characterOverlays[overlayInfo.Name].TextureNormal = overlayInfo.Normal;
            characterOverlays[overlayInfo.Name].TextureMaterial = overlayInfo.Material;

            overlayTypeItem = new SelectSliderItem("", 0, CharacterUtilities.FaceOverlayLayers.Count - 1, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                var overlay = characterOverlays.ElementAt((int)item.Value).Value;
                overlays = CharacterUtilities.FaceOverlays.Where(x => x.Name == overlay.Name).ToList();

                if (overlays.Exists(x => x.Id == overlay.TextureId))
                {
                    overlayInfo = overlays.Find(x => x.Id == overlay.TextureId);
                }
                else
                {
                    overlayInfo = overlays[0];
                }

                //if (overlays.Exists(x => x.Id == overlay.Id))
                //{
                //    overlayInfo = overlays.Find(x => x.Id == overlay.Id);
                //}
                //else
                //{
                //    overlayInfo = overlays[0];
                //}

                overlayTypeItem.Text = overlay.Name;

                overlayItem.Max = overlays.Count - 1;
                overlayItem.Value = overlays.IndexOf(overlayInfo);
                overlayItem.Text = $"{_languageService.Get("Client.CharacterCreator.Style")}";
                overlayItem.OnUpdate(_uiService);

                overlayInfo = overlays[(int)overlayItem.Value];

                characterOverlays[overlayInfo.Name].TextureId = overlayInfo.Id;
                characterOverlays[overlayInfo.Name].TextureNormal = overlayInfo.Normal;
                characterOverlays[overlayInfo.Name].TextureMaterial = overlayInfo.Material;

                overlayVisibilityItem.IsChecked = characterOverlays[overlayInfo.Name].TextureVisibility;
                overlayVisibilityItem.OnUpdate(_uiService);

                switch (overlayInfo.Name)
                {
                    case "eyeliners":
                        overlayItem.Visible = false;
                        overlayVarItem.Visible = true;
                        overlayVarItem.Max = 15;
                        break;
                    case "shadows":
                        overlayItem.Visible = false;
                        overlayVarItem.Visible = true;
                        overlayVarItem.Max = 5;
                        break;
                    case "lipsticks":
                        overlayItem.Visible = false;
                        overlayVarItem.Visible = true;
                        overlayVarItem.Max = 7;
                        break;
                    default:
                        overlayItem.Visible = true;
                        overlayVarItem.Visible = false;
                        break;
                }

                switch (overlay.TextureColorType)
                {
                    case 0:
                        overlayPaletteItem.Visible = true;
                        overlayPrimaryColorItem.Visible = true;
                        overlaySecondaryColorItem.Visible = true;
                        overlayTertiaryColorItem.Visible = true;
                        break;
                    case 1:
                        overlayPaletteItem.Visible = false;
                        overlayPrimaryColorItem.Visible = false;
                        overlaySecondaryColorItem.Visible = false;
                        overlayTertiaryColorItem.Visible = false;
                        break;
                    case 2:
                        overlayPaletteItem.Visible = true;
                        overlayPrimaryColorItem.Visible = true;
                        overlaySecondaryColorItem.Visible = true;
                        overlayTertiaryColorItem.Visible = true;
                        break;
                }

                overlayTypeItem.OnUpdate(_uiService);
                overlayItem.OnUpdate(_uiService);

                overlayVarItem.Value = characterOverlays[overlayInfo.Name].Variante;
                overlayVarItem.Text = $"{_languageService.Get("Client.CharacterCreator.Variant")}";
                overlayVarItem.OnUpdate(_uiService);

                overlayPrimaryColorItem.Value = characterOverlays[overlayInfo.Name].PalettePrimaryColor;
                overlayPrimaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.PrimaryColor")}";
                overlayPrimaryColorItem.OnUpdate(_uiService);

                overlaySecondaryColorItem.Value = characterOverlays[overlayInfo.Name].PaletteSecondaryColor;
                overlaySecondaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.SecondaryColor")}";
                overlaySecondaryColorItem.OnUpdate(_uiService);

                overlayTertiaryColorItem.Value = characterOverlays[overlayInfo.Name].PaletteTertiaryColor;
                overlayTertiaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.TertiaryColor")}";
                overlayTertiaryColorItem.OnUpdate(_uiService);

                overlayPaletteItem.Min = 0;
                overlayPaletteItem.Max = _characterService._colorPalettes.Count - 1;
                overlayPaletteItem.Value = _characterService._colorPalettes.IndexOf(characterOverlays[overlayInfo.Name].Palette.ToString("X"));
                overlayPaletteItem.Text = $"{_languageService.Get("Client.CharacterCreator.Palette")}";
                overlayPaletteItem.OnUpdate(_uiService);

                overlayOpacityItem.Value = characterOverlays[overlayInfo.Name].Opacity;
                overlayOpacityItem.Text = $"{_languageService.Get("Client.CharacterCreator.Opacity")}";
                overlayOpacityItem.OnUpdate(_uiService);

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Style"), 0, overlays.Count - 1, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                overlayItem.Text = $"{_languageService.Get("Client.CharacterCreator.Style")}";

                overlayInfo = overlays[(int)overlayItem.Value];

                characterOverlays[overlayInfo.Name].TextureId = overlayInfo.Id;
                characterOverlays[overlayInfo.Name].TextureNormal = overlayInfo.Normal;
                characterOverlays[overlayInfo.Name].TextureMaterial = overlayInfo.Material;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayVisibilityItem = new CheckboxItem(_languageService.Get("Client.CharacterCreator.Visible"), false, async (item, value) =>
            {
                characterOverlays[overlayInfo.Name].TextureVisibility = item.IsChecked;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayVarItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Variant"), 0, 254, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                overlayVarItem.Text = $"{_languageService.Get("Client.CharacterCreator.Variant")}";

                characterOverlays[overlayInfo.Name].Variante = (int)overlayVarItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            }, false);

            overlayPrimaryColorItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.PrimaryColor"), 0, 254, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                overlayPrimaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.PrimaryColor")}";

                characterOverlays[overlayInfo.Name].PalettePrimaryColor = (int)overlayPrimaryColorItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlaySecondaryColorItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.SecondaryColor"), 0, 254, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                overlaySecondaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.SecondaryColor")}";

                characterOverlays[overlayInfo.Name].PaletteSecondaryColor = (int)overlaySecondaryColorItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayTertiaryColorItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.TertiaryColor"), 0, 254, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                overlayTertiaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.TertiaryColor")}";

                characterOverlays[overlayInfo.Name].PaletteTertiaryColor = (int)overlayTertiaryColorItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayPaletteItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Palette"), 0, _characterService._colorPalettes.Count - 1, 1, 0, typeof(int), async (item, selectType, value) =>
            {
                overlayPaletteItem.Text = $"{_languageService.Get("Client.CharacterCreator.Palette")}";

                characterOverlays[overlayInfo.Name].Palette = uint.Parse(_characterService._colorPalettes[(int)overlayPaletteItem.Value], NumberStyles.AllowHexSpecifier);

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayOpacityItem = new SelectSliderItem(_languageService.Get("Client.CharacterCreator.Opacity"), 0f, 1f, 0.1f, 1f, typeof(float), async (item, selectType, value) =>
            {
                overlayOpacityItem.Text = $"{_languageService.Get("Client.CharacterCreator.Opacity")}";

                characterOverlays[overlayInfo.Name].Opacity = (float)overlayOpacityItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            faceOverlayTopContainer.AddItem(overlayTypeItem);
            faceOverlayTopContainer.AddItem(overlayItem);
            faceOverlayTopContainer.AddItem(overlayVisibilityItem);
            faceOverlayTopContainer.AddItem(overlayVarItem);
            faceOverlayTopContainer.AddItem(overlayPrimaryColorItem);
            faceOverlayTopContainer.AddItem(overlaySecondaryColorItem);
            faceOverlayTopContainer.AddItem(overlayTertiaryColorItem);
            faceOverlayTopContainer.AddItem(overlayPaletteItem);
            faceOverlayTopContainer.AddItem(overlayOpacityItem);

            overlayTypeItem.Text = CharacterUtilities.FaceOverlays[(int)overlayTypeItem.Value].Name;

            overlayItem.Text = $"{_languageService.Get("Client.CharacterCreator.Style")}";
            overlayVarItem.Text = $"{_languageService.Get("Client.CharacterCreator.Variant")}";
            overlayPrimaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.PrimaryColor")}";
            overlaySecondaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.SecondaryColor")}";
            overlayTertiaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.TertiaryColor")}";
            overlayPaletteItem.Text = $"{_languageService.Get("Client.CharacterCreator.Palette")}";
            overlayOpacityItem.Text = $"{_languageService.Get("Client.CharacterCreator.Opacity")}";
        }

        private async void InitClothMenu()
        {
            hatsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Hats), -1, GetOutfitComponentsByCategory(OutfitComponents.Hats).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Hats, item);
            });

            eyewearItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Eyewear), -1, GetOutfitComponentsByCategory(OutfitComponents.Eyewear).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Eyewear, item);
            });

            neckwearItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Neckwear), -1, GetOutfitComponentsByCategory(OutfitComponents.Neckwear).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Neckwear, item);
            });

            necktiesItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Neckties), -1, GetOutfitComponentsByCategory(OutfitComponents.Neckties).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Neckties, item);
            });

            shirtsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Shirts), -1, GetOutfitComponentsByCategory(OutfitComponents.Shirts).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Shirts, item);
            });

            suspendersItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Suspenders), -1, GetOutfitComponentsByCategory(OutfitComponents.Suspenders).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Suspenders, item);
            });

            vestItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Vests), -1, GetOutfitComponentsByCategory(OutfitComponents.Vests).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Vests, item);
            });

            coatsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Coats), -1, GetOutfitComponentsByCategory(OutfitComponents.Coats).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Coats, item);
            });

            coatsClosedItem = new SelectSliderItem(_languageService.Get(OutfitComponents.CoatsClosed), -1, GetOutfitComponentsByCategory(OutfitComponents.CoatsClosed).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.CoatsClosed, item);
            });

            ponchosItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Ponchos), -1, GetOutfitComponentsByCategory(OutfitComponents.Ponchos).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Ponchos, item);
            });

            cloakItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Cloaks), -1, GetOutfitComponentsByCategory(OutfitComponents.Cloaks).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Cloaks, item);
            });

            glovesItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Gloves), -1, GetOutfitComponentsByCategory(OutfitComponents.Gloves).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Gloves, item);
            });

            ringsRightHandItem = new SelectSliderItem(_languageService.Get(OutfitComponents.RingsRightHand), -1, GetOutfitComponentsByCategory(OutfitComponents.RingsRightHand).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.RingsRightHand, item);
            });

            ringsLeftHandItem = new SelectSliderItem(_languageService.Get(OutfitComponents.RingsLeftHand), -1, GetOutfitComponentsByCategory(OutfitComponents.RingsLeftHand).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.RingsLeftHand, item);
            });

            braceletsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Bracelts), -1, GetOutfitComponentsByCategory(OutfitComponents.Bracelts).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Bracelts, item);
            });

            gunbeltItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Gunbelts), -1, GetOutfitComponentsByCategory(OutfitComponents.Gunbelts).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Gunbelts, item);
            });

            beltItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Belts), -1, GetOutfitComponentsByCategory(OutfitComponents.Belts).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Belts, item);
            });

            buckleItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Beltbuckles), -1, GetOutfitComponentsByCategory(OutfitComponents.Beltbuckles).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Beltbuckles, item);
            });

            holstersCrossdrawItem = new SelectSliderItem(_languageService.Get(OutfitComponents.HolsterCrossdraw), -1, GetOutfitComponentsByCategory(OutfitComponents.HolsterCrossdraw).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.HolsterCrossdraw, item);
            });

            holstersLeftItem = new SelectSliderItem(_languageService.Get(OutfitComponents.HolstersLeft), -1, GetOutfitComponentsByCategory(OutfitComponents.HolstersLeft).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.HolstersLeft, item);
            });

            holstersRightItem = new SelectSliderItem(_languageService.Get(OutfitComponents.HolstersRight), -1, GetOutfitComponentsByCategory(OutfitComponents.HolstersRight).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.HolstersRight, item);
            });

            pantsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Pants), -1, GetOutfitComponentsByCategory(OutfitComponents.Pants).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Pants, item);
            });

            skirtsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Skirts), -1, GetOutfitComponentsByCategory(OutfitComponents.Skirts).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Skirts, item);
            });

            bootsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Boots), -1, GetOutfitComponentsByCategory(OutfitComponents.Boots).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Boots, item);
            });

            chapsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Chaps), -1, GetOutfitComponentsByCategory(OutfitComponents.Chaps).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Chaps, item);
            });

            spursItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Spurs), -1, GetOutfitComponentsByCategory(OutfitComponents.Spurs).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Spurs, item);
            });

            spatsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Spats), -1, GetOutfitComponentsByCategory(OutfitComponents.Spats).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Spats, item);
            });

            satchelsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Satchels), -1, GetOutfitComponentsByCategory(OutfitComponents.Satchels).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Satchels, item);
            });

            masksItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Masks), -1, GetOutfitComponentsByCategory(OutfitComponents.Masks).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Masks, item);
            });

            masksLargeItem = new SelectSliderItem(_languageService.Get(OutfitComponents.MasksLarge), -1, GetOutfitComponentsByCategory(OutfitComponents.MasksLarge).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.MasksLarge, item);
            });

            loadoutsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Loadouts), -1, GetOutfitComponentsByCategory(OutfitComponents.Loadouts).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Loadouts, item);
            });

            legAttachmentsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.LegAttachements), -1, GetOutfitComponentsByCategory(OutfitComponents.LegAttachements).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.LegAttachements, item);
            });

            gauntletsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Gauntlets), -1, GetOutfitComponentsByCategory(OutfitComponents.Gauntlets).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Gauntlets, item);
            });

            accessoriesItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Accessories), -1, GetOutfitComponentsByCategory(OutfitComponents.Accessories).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Accessories, item);
            });

            sheathsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Sheaths), -1, GetOutfitComponentsByCategory(OutfitComponents.Sheaths).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Sheaths, item);
            });

            apronsItem = new SelectSliderItem(_languageService.Get(OutfitComponents.Aprons), -1, GetOutfitComponentsByCategory(OutfitComponents.Aprons).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.Aprons, item);
            });

            femaleUnknow01Item = new SelectSliderItem(_languageService.Get(OutfitComponents.HairAccessories), -1, GetOutfitComponentsByCategory(OutfitComponents.HairAccessories).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.HairAccessories, item);
            });

            talismanBeltItem = new SelectSliderItem(_languageService.Get(OutfitComponents.TalismanBelt), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanBelt).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.TalismanBelt, item);
            });

            talismanHolsterItem = new SelectSliderItem(_languageService.Get(OutfitComponents.TalismanHolster), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanHolster).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.TalismanHolster, item);
            });

            talismanSatchelItem = new SelectSliderItem(_languageService.Get(OutfitComponents.TalismanSatchel), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanSatchel).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.TalismanSatchel, item);
            });

            talismanWristItem = new SelectSliderItem(_languageService.Get(OutfitComponents.TalismanWrist), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanWrist).Count - 1, 1, -1, typeof(int), async (item, selectType, value) =>
            {
                await SetPedComponent(OutfitComponents.TalismanWrist, item);
            });

            clothesTopContainer.AddItem(new LabelSeparatorItem("Tête", "../src/img/inventory_items/clothing_generic_mask.png"));
            clothesTopContainer.AddItem(hatsItem);
            clothesTopContainer.AddItem(eyewearItem);
            clothesTopContainer.AddItem(masksItem);
            clothesTopContainer.AddItem(masksLargeItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Nuque", "../src/img/inventory_items/kit_bandana.png"));
            clothesTopContainer.AddItem(neckwearItem);
            clothesTopContainer.AddItem(necktiesItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Haut du corps", "../src/img/inventory_items/clothing_generic_vest.png"));
            clothesTopContainer.AddItem(shirtsItem);
            clothesTopContainer.AddItem(vestItem);
            clothesTopContainer.AddItem(suspendersItem);
            clothesTopContainer.AddItem(ponchosItem);
            clothesTopContainer.AddItem(cloakItem);
            clothesTopContainer.AddItem(chapsItem);
            clothesTopContainer.AddItem(coatsItem);
            clothesTopContainer.AddItem(coatsClosedItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Bas du corps", "../src/img/inventory_items/clothing_generic_pants.png"));
            clothesTopContainer.AddItem(pantsItem);
            if (gender == Gender.Female) clothesTopContainer.AddItem(skirtsItem);
            clothesTopContainer.AddItem(beltItem);
            clothesTopContainer.AddItem(buckleItem);
            clothesTopContainer.AddItem(legAttachmentsItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Bottes", "../src/img/inventory_items/clothing_generic_boots.png"));
            clothesTopContainer.AddItem(bootsItem);
            clothesTopContainer.AddItem(spursItem);
            clothesTopContainer.AddItem(spatsItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Main / Bras", "../src/img/inventory_items/clothing_generic_glove.png"));
            clothesTopContainer.AddItem(glovesItem);
            clothesTopContainer.AddItem(gauntletsItem);
            clothesTopContainer.AddItem(ringsRightHandItem);
            clothesTopContainer.AddItem(ringsLeftHandItem);
            clothesTopContainer.AddItem(braceletsItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Accessoires", "../src/img/inventory_items/clothing_generic_belt_accs.png"));
            clothesTopContainer.AddItem(satchelsItem);
            clothesTopContainer.AddItem(accessoriesItem);
            if (gender == Gender.Female) clothesTopContainer.AddItem(femaleUnknow01Item);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Ceintures d'armes / Holsters", "../src/img/inventory_items/upgrade_bandolier.png"));
            clothesTopContainer.AddItem(gunbeltItem);
            clothesTopContainer.AddItem(loadoutsItem);
            clothesTopContainer.AddItem(holstersCrossdrawItem);
            clothesTopContainer.AddItem(holstersLeftItem);
            clothesTopContainer.AddItem(holstersRightItem);
            clothesTopContainer.AddItem(new LabelSeparatorItem("Etuis", "../src/img/inventory_items/upgrade_offhand_holster.png"));
            clothesTopContainer.AddItem(sheathsItem);
            clothesTopContainer.AddItem(apronsItem);

            if (gender == Gender.Male)
            {
                clothesTopContainer.AddItem(new LabelSeparatorItem("Talismans", "../src/img/inventory_items/provision_talisman_raven_claw.png"));
                clothesTopContainer.AddItem(talismanBeltItem);
                clothesTopContainer.AddItem(talismanHolsterItem);
                clothesTopContainer.AddItem(talismanSatchelItem);
                clothesTopContainer.AddItem(talismanWristItem);
            }

            await SetPedComponent(OutfitComponents.Hats, hatsItem);
            await SetPedComponent(OutfitComponents.Eyewear, eyewearItem);
            await SetPedComponent(OutfitComponents.Neckwear, neckwearItem);
            await SetPedComponent(OutfitComponents.Neckties, necktiesItem);
            await SetPedComponent(OutfitComponents.Shirts, shirtsItem);
            await SetPedComponent(OutfitComponents.Suspenders, suspendersItem);
            await SetPedComponent(OutfitComponents.Vests, vestItem);
            await SetPedComponent(OutfitComponents.Coats, coatsItem);
            await SetPedComponent(OutfitComponents.CoatsClosed, coatsClosedItem);
            await SetPedComponent(OutfitComponents.Ponchos, ponchosItem);
            await SetPedComponent(OutfitComponents.Cloaks, cloakItem);
            await SetPedComponent(OutfitComponents.Gloves, glovesItem);
            await SetPedComponent(OutfitComponents.RingsRightHand, ringsRightHandItem);
            await SetPedComponent(OutfitComponents.RingsLeftHand, ringsLeftHandItem);
            await SetPedComponent(OutfitComponents.Bracelts, braceletsItem);
            await SetPedComponent(OutfitComponents.Gunbelts, gunbeltItem);
            await SetPedComponent(OutfitComponents.Belts, beltItem);
            await SetPedComponent(OutfitComponents.Beltbuckles, buckleItem);
            await SetPedComponent(OutfitComponents.HolsterCrossdraw, holstersCrossdrawItem);
            await SetPedComponent(OutfitComponents.HolstersLeft, holstersLeftItem);
            await SetPedComponent(OutfitComponents.HolstersRight, holstersRightItem);
            await SetPedComponent(OutfitComponents.Pants, pantsItem);
            await SetPedComponent(OutfitComponents.Boots, bootsItem);
            await SetPedComponent(OutfitComponents.Chaps, chapsItem);
            await SetPedComponent(OutfitComponents.Spurs, spursItem);
            await SetPedComponent(OutfitComponents.Spats, spatsItem);
            await SetPedComponent(OutfitComponents.Satchels, satchelsItem);
            await SetPedComponent(OutfitComponents.Masks, masksItem);
            await SetPedComponent(OutfitComponents.MasksLarge, masksLargeItem);
            await SetPedComponent(OutfitComponents.Loadouts, loadoutsItem);
            await SetPedComponent(OutfitComponents.LegAttachements, legAttachmentsItem);
            await SetPedComponent(OutfitComponents.Gauntlets, gauntletsItem);
            await SetPedComponent(OutfitComponents.Accessories, accessoriesItem);
            await SetPedComponent(OutfitComponents.Sheaths, sheathsItem);
            await SetPedComponent(OutfitComponents.Aprons, apronsItem);
            await SetPedComponent(OutfitComponents.TalismanBelt, talismanBeltItem);
            await SetPedComponent(OutfitComponents.TalismanHolster, talismanHolsterItem);
            await SetPedComponent(OutfitComponents.TalismanSatchel, talismanSatchelItem);
            await SetPedComponent(OutfitComponents.TalismanWrist, talismanWristItem);

            if (gender == Gender.Female)
            {
                await SetPedComponent(OutfitComponents.Skirts, skirtsItem);
                await SetPedComponent(OutfitComponents.HairAccessories, femaleUnknow01Item);
            }
            else
            {

            }
        }

        private void RandomizeFace()
        {
            var ped = PlayerPedId();
            var values = new Dictionary<int, float>();

            for (int i = 0; i < _characterService._faceParts.Count; i++)
            {
                var part = int.Parse(_characterService._faceParts[i], NumberStyles.AllowHexSpecifier);
                var rand = new Random(Environment.TickCount * i * part).Next(-10, 10) / 10f;

                SetPedFaceFeature(ped, part, rand);
                values.Add(part, rand);

                characterFaceParts[part] = rand;
            }

            // Set item face part value
            headWidthItem.Value = values[CharacterFacePart.HeadWidth];
            eyebrowHeightItem.Value = values[CharacterFacePart.EyebrowHeight];
            eyebrowWidthItem.Value = values[CharacterFacePart.EyebrowWidth];
            eyebrowDepthItem.Value = values[CharacterFacePart.EyebrowDepth];
            earsWidthItem.Value = values[CharacterFacePart.EarsWidth];
            earsHeightItem.Value = values[CharacterFacePart.EarsHeight];
            earsAngleItem.Value = values[CharacterFacePart.EarsAngle];
            earsLobeSizeItem.Value = values[CharacterFacePart.EarsLobeSize];
            cheeckBonesHeightItem.Value = values[CharacterFacePart.CheeckBonesHeight];
            cheeckBonesDepthItem.Value = values[CharacterFacePart.CheeckBonesDepth];
            cheeckBonesWidthItem.Value = values[CharacterFacePart.CheeckBonesWidth];
            jawHeightItem.Value = values[CharacterFacePart.JawHeight];
            jawDepthItem.Value = values[CharacterFacePart.JawDepth];
            jawWidthItem.Value = values[CharacterFacePart.JawWidth];
            chinHeightItem.Value = values[CharacterFacePart.ChinHeight];
            chinWidthItem.Value = values[CharacterFacePart.ChinWidth];
            chinDepthItem.Value = values[CharacterFacePart.ChinDepth];
            eyeLidHeightItem.Value = values[CharacterFacePart.EyeLidHeight];
            eyeLidWidthItem.Value = values[CharacterFacePart.EyeLidWidth];
            eyesDepthItem.Value = values[CharacterFacePart.EyesDepth];
            eyesAngleItem.Value = values[CharacterFacePart.EyesAngle];
            eyesDistanceItem.Value = values[CharacterFacePart.EyesDistance];
            eyesHeightItem.Value = values[CharacterFacePart.EyesHeight];
            noseWidthItem.Value = values[CharacterFacePart.NoseWidth];
            noseSizeItem.Value = values[CharacterFacePart.NoseSize];
            noseHeightItem.Value = values[CharacterFacePart.NoseHeight];
            noseCurvatureItem.Value = values[CharacterFacePart.NoseCurvature];
            noseAngleItem.Value = values[CharacterFacePart.NoseAngle];
            noStrilsDistanceItem.Value = values[CharacterFacePart.NoStrilsDistance];
            mouthDepthItem.Value = values[CharacterFacePart.MouthDepth];
            mouthWidthItem.Value = values[CharacterFacePart.MouthWidth];
            mouthXPosItem.Value = values[CharacterFacePart.MouthXPos];
            mouthYPosItem.Value = values[CharacterFacePart.MouthYPos];
            upperLipDepthItem.Value = values[CharacterFacePart.UpperLipDepth];
            upperLipHeightItem.Value = values[CharacterFacePart.UpperLipHeight];
            upperLipWidthItem.Value = values[CharacterFacePart.UpperLipWidth];
            lowerLipDepthItem.Value = values[CharacterFacePart.LowerLipDepth];
            lowerLipHeightItem.Value = values[CharacterFacePart.LowerLipHeight];
            lowerLipWidthItem.Value = values[CharacterFacePart.LowerLipWidth];

            headWidthItem.OnUpdate(_uiService);
            eyebrowHeightItem.OnUpdate(_uiService);
            eyebrowWidthItem.OnUpdate(_uiService);
            eyebrowDepthItem.OnUpdate(_uiService);
            earsWidthItem.OnUpdate(_uiService);
            earsHeightItem.OnUpdate(_uiService);
            earsAngleItem.OnUpdate(_uiService);
            earsLobeSizeItem.OnUpdate(_uiService);
            cheeckBonesHeightItem.OnUpdate(_uiService);
            cheeckBonesDepthItem.OnUpdate(_uiService);
            cheeckBonesWidthItem.OnUpdate(_uiService);
            jawHeightItem.OnUpdate(_uiService);
            jawDepthItem.OnUpdate(_uiService);
            jawWidthItem.OnUpdate(_uiService);
            chinHeightItem.OnUpdate(_uiService);
            chinWidthItem.OnUpdate(_uiService);
            chinDepthItem.OnUpdate(_uiService);
            eyeLidHeightItem.OnUpdate(_uiService);
            eyeLidWidthItem.OnUpdate(_uiService);
            eyesDepthItem.OnUpdate(_uiService);
            eyesAngleItem.OnUpdate(_uiService);
            eyesDistanceItem.OnUpdate(_uiService);
            eyesHeightItem.OnUpdate(_uiService);
            noseWidthItem.OnUpdate(_uiService);
            noseSizeItem.OnUpdate(_uiService);
            noseHeightItem.OnUpdate(_uiService);
            noseCurvatureItem.OnUpdate(_uiService);
            noseAngleItem.OnUpdate(_uiService);
            noStrilsDistanceItem.OnUpdate(_uiService);
            mouthDepthItem.OnUpdate(_uiService);
            mouthWidthItem.OnUpdate(_uiService);
            mouthXPosItem.OnUpdate(_uiService);
            mouthYPosItem.OnUpdate(_uiService);
            upperLipDepthItem.OnUpdate(_uiService);
            upperLipHeightItem.OnUpdate(_uiService);
            upperLipWidthItem.OnUpdate(_uiService);
            lowerLipDepthItem.OnUpdate(_uiService);
            lowerLipHeightItem.OnUpdate(_uiService);
            lowerLipWidthItem.OnUpdate(_uiService);
        }

        private async void UpdateOverlay()
        {
            var ped = PlayerPedId();

            if (textureId != -1)
            {
                ResetPedTexture2(textureId);
                DeletePedTexture(textureId);
            }

            textureId = Call<int>(0xC5E7204F322E49EB, textureType["albedo"], textureType["normal"], textureType["material"]);

            foreach (var layer in characterOverlays.Values)
            {
                if (layer.TextureVisibility)
                {
                    var overlayId = AddPedOverlay(textureId, layer.TextureId, layer.TextureNormal, layer.TextureMaterial, layer.TextureColorType, layer.TextureOpacity, layer.TextureUnk);

                    if (layer.TextureColorType == 0)
                    {
                        SetPedOverlayPalette(textureId, overlayId, layer.Palette);
                        SetPedOverlayPaletteColour(textureId, overlayId, layer.PalettePrimaryColor, layer.PaletteSecondaryColor, layer.PaletteTertiaryColor);
                    }

                    SetPedOverlayVariation(textureId, overlayId, layer.Variante);
                    SetPedOverlayOpacity(textureId, overlayId, layer.Opacity);
                }

                while (!IsPedTextureValid(textureId)) await BaseScript.Delay(0);

                OverrideTextureOnPed(ped, (uint)GetHashKey("heads"), textureId);
                UpdatePedTexture(textureId);
                UpdatePedVariation();
            }
        }

        private void UpdatePedBodyComponent(List<string> components, SelectSliderItem item, string text)
        {
            var component = components[(int)item.Value];
            item.Text = text;
            item.OnUpdate(_uiService);

            UpdatePedVariation(PlayerPedId());
            SetPedComponentEnabled(PlayerPedId(), (uint)FromHexToHash(component), true, true, false);
        }

        private async Task SetPedComponent(string categoryHash, SelectSliderItem item)
        {
            try
            {
                var clothes = GetOutfitComponentsByCategory(categoryHash);
                //var clothes = _characterService.clothes.Where(x => x.CategoryHash == categoryHash && x.PedType == (int)gender).ToList();
                var clothName = _languageService.Get(categoryHash);

                if ((int)item.Value == -1)
                {
                    RemovePedComponent(uint.Parse(categoryHash, NumberStyles.AllowHexSpecifier));
                    characterClothes[categoryHash] = 0;
                    item.Text = $"Aucun(e) {clothName}";
                    item.OnUpdate(_uiService);
                }
                else
                {
                    if ((int)item.Value > clothes.Count)
                    {
                        item.Value = clothes.Count;
                    }

                    if ((int)item.Value < 0)
                    {
                        item.Value = 0;
                    }

                    var cloth = clothes[(int)item.Value];
                    var component = uint.Parse(cloth.Hash, NumberStyles.AllowHexSpecifier);

                    SetPedComponentEnabled(PlayerPedId(), component, true, cloth.IsMultiplayer, false);
                    characterClothes[cloth.CategoryHash] = component;
                    UpdatePedVariation();

                    item.Text = clothName;
                    item.OnUpdate(_uiService);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[SetPedComponent] Index is to high");
            }
        }

        private void OnResourceStop(object sender, ResourceStopEventArgs e)
        {
            if (e.Resource == "avg")
            {
                SetCamActive(defaultCamera, false);
                SetCamActive(faceCamera, false);
                SetCamActive(bodyCamera, false);
                SetCamActive(footCamera, false);

                RenderScriptCams(false, true, 1000, true, true, 0);

                DestroyCam(defaultCamera, true);
                DestroyCam(faceCamera, true);
                DestroyCam(bodyCamera, true);
                DestroyCam(footCamera, true);
            }
        }
    }
}
