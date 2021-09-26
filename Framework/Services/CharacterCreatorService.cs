using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Events;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Managers;
using Average.Client.Menu;
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

namespace Average.Client.Framework.Services
{
    internal class CharacterCreatorService : IService
    {
        private MenuContainer faceMenu;
        private MenuContainer faceFeaturesMenu;
        private MenuContainer bodyMenu;
        private MenuContainer clothesMenu;
        private MenuContainer infoMenu;
        private MenuContainer faceOverlayMenu;
        private MenuContainer attributesMenu;

        // Info Menu
        private ListItem genderItem;
        private Vector2InputItem firstAndLastNameItem;
        private Vector2InputItem nationalityAndCityBirthItem;
        private Vector3InputItem dateOfBirthItem;
        private SelectorItem<float> pedScaleItem;
        private SelectorItem<int> culturesItem;
        private SelectorItem<int> headsItem;
        private SelectorItem<int> bodyItem;
        private SelectorItem<int> legsItem;

        // Cloth Menu
        private SelectorItem<int> hatsItem;
        private SelectorItem<int> eyewearItem;
        private SelectorItem<int> neckwearItem;
        private SelectorItem<int> necktiesItem;
        private SelectorItem<int> shirtsItem;
        private SelectorItem<int> suspendersItem;
        private SelectorItem<int> vestItem;
        private SelectorItem<int> coatsItem;
        private SelectorItem<int> coatsClosedItem;
        private SelectorItem<int> ponchosItem;
        private SelectorItem<int> cloakItem;
        private SelectorItem<int> glovesItem;
        private SelectorItem<int> ringsRightHandItem;
        private SelectorItem<int> ringsLeftHandItem;
        private SelectorItem<int> braceletsItem;
        private SelectorItem<int> gunbeltItem;
        private SelectorItem<int> beltItem;
        private SelectorItem<int> buckleItem;
        private SelectorItem<int> holstersCrossdrawItem;
        private SelectorItem<int> holstersLeftItem;
        private SelectorItem<int> holstersRightItem;
        private SelectorItem<int> pantsItem;
        private SelectorItem<int> skirtsItem;
        private SelectorItem<int> bootsItem;
        private SelectorItem<int> chapsItem;
        private SelectorItem<int> spursItem;
        private SelectorItem<int> spatsItem;
        private SelectorItem<int> satchelsItem;
        private SelectorItem<int> masksItem;
        private SelectorItem<int> masksLargeItem;
        private SelectorItem<int> loadoutsItem;
        private SelectorItem<int> legAttachmentsItem;
        private SelectorItem<int> gauntletsItem;
        private SelectorItem<int> accessoriesItem;
        private SelectorItem<int> sheathsItem;
        private SelectorItem<int> apronsItem;
        private SelectorItem<int> femaleUnknow01Item;
        private SelectorItem<int> talismanBeltItem;
        private SelectorItem<int> talismanHolsterItem;
        private SelectorItem<int> talismanSatchelItem;
        private SelectorItem<int> talismanWristItem;

        // Face Menu
        private SelectorItem<int> beardChopsItem;
        private SelectorItem<int> mustacheItem;
        private SelectorItem<int> mustacheMpItem;
        private SelectorItem<int> goateesItem;
        private SelectorItem<int> teethItem;
        private SelectorItem<int> hairItem;
        private SelectorItem<int> eyesItem;
        private SelectorItem<float> headWidthItem;
        private SelectorItem<float> eyebrowHeightItem;
        private SelectorItem<float> eyebrowWidthItem;
        private SelectorItem<float> eyebrowDepthItem;
        private SelectorItem<float> earsWidthItem;
        private SelectorItem<float> earsAngleItem;
        private SelectorItem<float> earsHeightItem;
        private SelectorItem<float> earsLobeSizeItem;
        private SelectorItem<float> cheeckBonesHeightItem;
        private SelectorItem<float> cheeckBonesWidthItem;
        private SelectorItem<float> cheeckBonesDepthItem;
        private SelectorItem<float> jawHeightItem;
        private SelectorItem<float> jawWidthItem;
        private SelectorItem<float> jawDepthItem;
        private SelectorItem<float> chinHeightItem;
        private SelectorItem<float> chinWidthItem;
        private SelectorItem<float> chinDepthItem;
        private SelectorItem<float> eyeLidHeightItem;
        private SelectorItem<float> eyeLidWidthItem;
        private SelectorItem<float> eyesDepthItem;
        private SelectorItem<float> eyesAngleItem;
        private SelectorItem<float> eyesDistanceItem;
        private SelectorItem<float> eyesHeightItem;
        private SelectorItem<float> noseWidthItem;
        private SelectorItem<float> noseSizeItem;
        private SelectorItem<float> noseHeightItem;
        private SelectorItem<float> noseAngleItem;
        private SelectorItem<float> noseCurvatureItem;
        private SelectorItem<float> noStrilsDistanceItem;
        private SelectorItem<float> mouthWidthItem;
        private SelectorItem<float> mouthDepthItem;
        private SelectorItem<float> mouthXPosItem;
        private SelectorItem<float> mouthYPosItem;
        private SelectorItem<float> upperLipHeightItem;
        private SelectorItem<float> upperLipWidthItem;
        private SelectorItem<float> upperLipDepthItem;
        private SelectorItem<float> lowerLipHeightItem;
        private SelectorItem<float> lowerLipWidthItem;
        private SelectorItem<float> lowerLipDepthItem;

        // Body Menu
        private SelectorItem<int> bodyTypesItem;
        private SelectorItem<int> waistTypesItem;

        // Face Overlay Menu
        private SelectorItem<int> overlayTypeItem;
        private SelectorItem<int> overlayItem;
        private SelectorItem<int> overlayVarItem;
        private SelectorItem<int> overlayPrimaryColorItem;
        private SelectorItem<int> overlaySecondaryColorItem;
        private SelectorItem<int> overlayTertiaryColorItem;
        private SelectorItem<int> overlayPaletteItem;
        private SelectorItem<float> overlayOpacityItem;
        private CheckboxItem overlayVisibilityItem;

        private Gender gender = Gender.Male;

        private int textureId = -1;
        private int defaultCamera;
        private int faceCamera;
        private int bodyCamera;
        private int footCamera;
        private int currentCamIndex;

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
        private readonly ThreadManager _threadManager;
        private readonly EventManager _eventManager;
        private readonly LanguageService _languageService;
        private readonly CharacterService _characterService;
        private readonly UIService _uiService;

        private readonly JObject _config;

        private int _locationIndex = 0;

        public bool IsOpen { get; private set; }

        public CharacterCreatorService(UIService uiService, CharacterService characterService, MenuService menuService, ThreadManager threadManager, EventManager eventManager, LanguageService languageService)
        {
            _menuService = menuService;
            _threadManager = threadManager;
            _eventManager = eventManager;
            _languageService = languageService;
            _characterService = characterService;
            _uiService = uiService;

            _config = Configuration.ParseToObject("configs/character_creator.json");

            // Events
            _eventManager.ResourceStop += OnResourceStop;
        }

        private async Task WeatherUpdate()
        {
            NetworkClockTimeOverride(10, 0, 0, 0, true);
            Call(0xD74ACDF7DB8114AF, false);
            Call(0x59174F1AFE095B5A, (uint)Weather.Sunny, true, true, true, 1f, false);

            await BaseScript.Delay(0); // Delete ?
        }

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            _uiService.LoadFrame("menu");
            return result;
        }

        [UICallback("frame_ready")]
        private CallbackDelegate OnFrameReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            _uiService.Show("menu");
            return result;
        }

        [UICallback("menu/keypress")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (IsOpen)
            {
                var key = int.Parse(data["key"].ToString());

                if (key == 37)
                {
                    // Left

                    var heading = GetEntityHeading(PlayerPedId());
                    heading -= 45f;
                    TaskAchieveHeading(PlayerPedId(), heading, 750);
                }
                else if (key == 39)
                {
                    // Right

                    var heading = GetEntityHeading(PlayerPedId());
                    heading += 45f;
                    TaskAchieveHeading(PlayerPedId(), heading, 750);
                }
                else if (key == 38)
                {
                    // Top

                    if (!IsCamInterpolating(defaultCamera) && !IsCamInterpolating(faceCamera) && !IsCamInterpolating(bodyCamera) && !IsCamInterpolating(footCamera))
                    {
                        if (currentCamIndex < 3)
                        {
                            currentCamIndex += 1;

                            SwitchCamera(currentCamIndex);

                            switch (currentCamIndex)
                            {
                                case 0:
                                    break;
                                case 1:
                                    SetCamActiveWithInterp(faceCamera, defaultCamera, 750, 1, 0);
                                    break;
                                case 2:
                                    SetCamActiveWithInterp(bodyCamera, faceCamera, 750, 1, 0);
                                    break;
                                case 3:
                                    SetCamActiveWithInterp(footCamera, bodyCamera, 750, 1, 0);
                                    break;
                            }
                        }
                    }
                }
                else if (key == 40)
                {
                    // Bottom

                    if (!IsCamInterpolating(defaultCamera) && !IsCamInterpolating(faceCamera) && !IsCamInterpolating(bodyCamera) && !IsCamInterpolating(footCamera))
                    {
                        if (currentCamIndex > 0)
                        {
                            currentCamIndex -= 1;

                            SwitchCamera(currentCamIndex);

                            switch (currentCamIndex)
                            {
                                case 0:
                                    SetCamActiveWithInterp(defaultCamera, faceCamera, 750, 1, 0);
                                    break;
                                case 1:
                                    SetCamActiveWithInterp(faceCamera, bodyCamera, 750, 1, 0);
                                    break;
                                case 2:
                                    SetCamActiveWithInterp(bodyCamera, footCamera, 750, 1, 0);
                                    break;
                                case 3:
                                    break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private async void CreateCharacter()
        {
            if (firstAndLastNameItem.Input1.Value.ToString().Length < firstAndLastNameItem.Input1.MinLength) return;
            if (firstAndLastNameItem.Input2.Value.ToString().Length < firstAndLastNameItem.Input2.MinLength) return;

            if (nationalityAndCityBirthItem.Input1.Value.ToString().Length < nationalityAndCityBirthItem.Input1.MinLength) return;
            if (nationalityAndCityBirthItem.Input2.Value.ToString().Length < nationalityAndCityBirthItem.Input2.MinLength) return;

            if (!dateOfBirthItem.Input1.Value.ToString().All(char.IsNumber)) return;
            if (!dateOfBirthItem.Input2.Value.ToString().All(char.IsNumber)) return;
            if (!dateOfBirthItem.Input3.Value.ToString().All(char.IsNumber)) return;

            if (dateOfBirthItem.Input1.Value.ToString().Length < dateOfBirthItem.Input1.MinLength) return;
            if (dateOfBirthItem.Input2.Value.ToString().Length < dateOfBirthItem.Input2.MinLength) return;
            if (dateOfBirthItem.Input3.Value.ToString().Length < dateOfBirthItem.Input3.MinLength) return;

            if (int.Parse(dateOfBirthItem.Input1.Value.ToString()) < 1 || int.Parse(dateOfBirthItem.Input1.Value.ToString()) > 31) return;
            if (int.Parse(dateOfBirthItem.Input2.Value.ToString()) < 1 || int.Parse(dateOfBirthItem.Input2.Value.ToString()) > 12) return;
            if (int.Parse(dateOfBirthItem.Input3.Value.ToString()) < 1820 || int.Parse(dateOfBirthItem.Input3.Value.ToString()) > 1880) return;

            var characterData = new CharacterData
            {
                CreationDate = DateTime.Now,
                Firstname = firstAndLastNameItem.Input1.Value.ToString(),
                Lastname = firstAndLastNameItem.Input2.Value.ToString(),
                Nationality = nationalityAndCityBirthItem.Input1.Value.ToString(),
                CityOfBirth = nationalityAndCityBirthItem.Input2.Value.ToString(),
                DateOfBirth = $"{dateOfBirthItem.Input1.Value}/{dateOfBirthItem.Input2.Value}/{dateOfBirthItem.Input3.Value}",
                Skin = new SkinData
                {
                    Gender = gender,
                    Scale = pedScaleItem.Value,
                    BodyType = _characterService.bodyTypes[bodyTypesItem.Value],
                    WaistType = _characterService.waistTypes[waistTypesItem.Value],
                    Body = uint.Parse(CharacterUtilities.Origins[culturesItem.Value].Bodies[bodyItem.Value], NumberStyles.HexNumber),
                    Head = uint.Parse(CharacterUtilities.Origins[culturesItem.Value].Heads[headsItem.Value], NumberStyles.HexNumber),
                    Legs = uint.Parse(CharacterUtilities.Origins[culturesItem.Value].Legs[legsItem.Value], NumberStyles.HexNumber),

                    Albedo = textureType["albedo"],
                    Normal = textureType["normal"],
                    Material = textureType["material"],

                    OverlaysData = characterOverlays.Values.ToList(),

                    CheeckBonesDepth = cheeckBonesDepthItem.Value,
                    CheeckBonesWidth = cheeckBonesWidthItem.Value,
                    CheeckBonesHeight = cheeckBonesHeightItem.Value,
                    ChinDepth = chinDepthItem.Value,
                    ChinHeight = chinHeightItem.Value,
                    ChinWidth = chinWidthItem.Value,
                    EarsAngle = earsAngleItem.Value,
                    EarsHeight = earsHeightItem.Value,
                    EarsLobeSize = earsLobeSizeItem.Value,
                    EarsWidth = earsWidthItem.Value,
                    EyesAngle = eyesAngleItem.Value,
                    EyebrowDepth = eyebrowDepthItem.Value,
                    EyebrowHeight = eyebrowHeightItem.Value,
                    EyebrowWidth = eyebrowWidthItem.Value,
                    EyeLidHeight = eyeLidHeightItem.Value,
                    EyeLidWidth = eyeLidWidthItem.Value,
                    EyesDepth = eyesDepthItem.Value,
                    EyesDistance = eyesDistanceItem.Value,
                    EyesHeight = eyesHeightItem.Value,
                    HeadWidth = headWidthItem.Value,
                    JawDepth = jawDepthItem.Value,
                    JawHeight = jawHeightItem.Value,
                    JawWidth = jawWidthItem.Value,
                    LowerLipDepth = lowerLipDepthItem.Value,
                    LowerLipHeight = lowerLipHeightItem.Value,
                    LowerLipWidth = lowerLipWidthItem.Value,
                    MouthDepth = mouthDepthItem.Value,
                    MouthWidth = mouthWidthItem.Value,
                    MouthXPos = mouthXPosItem.Value,
                    MouthYPos = mouthYPosItem.Value,
                    NoseAngle = noseAngleItem.Value,
                    NoseCurvature = noseCurvatureItem.Value,
                    NoseHeight = noseHeightItem.Value,
                    NoseSize = noseSizeItem.Value,
                    NoseWidth = noseWidthItem.Value,
                    NoStrilsDistance = noStrilsDistanceItem.Value,
                    UpperLipDepth = upperLipDepthItem.Value,
                    UpperLipHeight = upperLipHeightItem.Value,
                    UpperLipWidth = upperLipWidthItem.Value,
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
                Economy = new EconomyData
                {
                    Money = (decimal)_config["DefaultMoney"],
                    Bank = (decimal)_config["DefaultBank"]
                },
                Position = new PositionData((float)_config["Locations"][_locationIndex]["X"], (float)_config["Locations"][_locationIndex]["Y"], (float)_config["Locations"][_locationIndex]["Z"], (float)_config["Locations"][_locationIndex]["H"]),
            };

            _menuService.Close();
            Unfocus();

            _characterService.Create(characterData);
            _eventManager.EmitServer("character:character_created");

            await FadeOut(500);
            await BaseScript.Delay(1000);

            RenderScriptCams(false, false, 0, true, true, 0);
            SetEntityCoords(PlayerPedId(), -169.93f, 626.56f, 114.23f, true, true, true, false);

            PauseClock(false, 0);
            SetWeatherTypeFrozen(false);
            FreezeEntityPosition(PlayerPedId(), false);

            _threadManager.StopThread(WeatherUpdate);

            IsOpen = false;

            //await Character.Load();

            //Storage.Create(StorageDataType.PlayerInventory, 25f, Character.Current.License);

            await BaseScript.Delay(1000);

            //await Storage.LoadInventory(Character.Current.License);
            //await Storage.IsReady();
            //await Storage.IsItemsInfoReady();

            //Export.CallMethod("World.SetCanChangeWorld", true);

            NetworkEndTutorialSession();

            await FadeIn(500);
        }

        private void SwitchCamera(int type)
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

            var headHeight = GetPedBoneCoords(ped, 168, 0f, 0f, 0f).Z + pedScaleItem.Value - 0.4f;
            var bodyHeight = GetPedBoneCoords(ped, 420, 0f, 0f, 0f).Z + pedScaleItem.Value - 0.8f;

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
            _threadManager.StartThread(WeatherUpdate);

            await _characterService.SpawnPed(gender);

            var ped = PlayerPedId();
            var x = (float)_config["CreationPosition"]["X"];
            var y = (float)_config["CreationPosition"]["Y"];
            var z = (float)_config["CreationPosition"]["Z"];
            var h = (float)_config["CreationPosition"]["H"];

            RequestCollisionAtCoord(x, y, z);
            Call(0x239A3351AC1DA385, ped, x, y, z, false, false, false, true);
            SetEntityHeading(ped, h);
            Call(0xEA23C49EAA83ACFB, x, y, z, h, true, true, false);

            var timer = GetGameTimer();
            while (!HasCollisionLoadedAroundEntity(ped) && (GetGameTimer() - timer) < 5000) await BaseScript.Delay(0);

            IsOpen = true;
            _menuService.CanCloseMenu = false;

            SetWeatherType((uint)Weather.Sunny, true, true, true, 0f, false);
            SetWeatherTypeFrozen(true);
            NetworkClockTimeOverride(12, 0, 0, 1, true);
            PauseClock(true, 0);

            FreezeEntityPosition(ped, false);

            InitClothMenu();
            InitFaceMenu();
            InitBodyMenu();
            InitFaceFeaturesMenu();
            InitFaceOverlayMenu();
            InitInfoMenu();

            var tabMenu = new TabContainer();
            tabMenu.AddItem(new TabItem("./img/note.png", null, infoMenu, true));
            tabMenu.AddItem(new TabItem("./img/head.png", null, faceMenu));
            tabMenu.AddItem(new TabItem("./img/torso.png", null, bodyMenu));
            tabMenu.AddItem(new TabItem("./img/shirt.png", null, clothesMenu));
            tabMenu.AddItem(new TabItem("./img/stats.png", null, attributesMenu));

            _menuService.SetTabMenu(tabMenu);
            await _menuService.Open(infoMenu);

            Focus();

            InitCamera(new Vector3(x, y, z), h);
            InitDefaultPed();
        }

        private void InitDefaultPed()
        {
            origin = gender == Gender.Male ? CharacterUtilities.Origins[0] : CharacterUtilities.Origins[6];
            origins = CharacterUtilities.Origins.Where(x => x.Gender == gender).ToList();

            culturesItem.Text = $"{_languageService.Get("Client.CharacterCreator.Origin")}";

            headsItem.MinValue = 0;
            headsItem.MaxValue = origin.Heads.Count - 1;
            headsItem.Value = 0;

            bodyItem.MinValue = 0;
            bodyItem.MaxValue = origin.Bodies.Count - 1;
            bodyItem.Value = 0;

            legsItem.MinValue = 0;
            legsItem.MaxValue = origin.Legs.Count - 1;
            legsItem.Value = 0;

            UpdatePedBodyComponent(origin.Heads, headsItem, _languageService.Get(OutfitComponents.Heads));
            UpdatePedBodyComponent(origin.Bodies, bodyItem, _languageService.Get(OutfitComponents.Torsos));
            UpdatePedBodyComponent(origin.Legs, legsItem, _languageService.Get(OutfitComponents.Legs));

            RandomizeFace();

            InitPedOverlay();
            InitDefaultPedComponents();
        }

        private async void InitDefaultPedComponents()
        {
            // Define max cloth value by gender
            skirtsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Loadouts).Count - 1;
            hairItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Hairs).Count - 1;
            eyesItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Eyes).Count - 1;
            teethItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Teeth).Count - 1;
            braceletsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Bracelts).Count - 1;
            ringsLeftHandItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.RingsLeftHand).Count - 1;
            ringsRightHandItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.RingsRightHand).Count - 1;
            hatsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Hats).Count - 1;
            shirtsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Shirts).Count - 1;
            vestItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Vests).Count - 1;
            pantsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Pants).Count - 1;
            neckwearItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Neckwear).Count - 1;
            bootsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Boots).Count - 1;
            accessoriesItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Accessories).Count - 1;
            spursItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Spurs).Count - 1;
            chapsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Chaps).Count - 1;
            cloakItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Cloaks).Count - 1;
            masksItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Masks).Count - 1;
            spatsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Spats).Count - 1;
            gauntletsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Gauntlets).Count - 1;
            necktiesItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Neckties).Count - 1;
            suspendersItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Suspenders).Count - 1;
            gunbeltItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Gunbelts).Count - 1;
            beltItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Belts).Count - 1;
            buckleItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Beltbuckles).Count - 1;
            coatsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Coats).Count - 1;
            ponchosItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Ponchos).Count - 1;
            glovesItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Gloves).Count - 1;
            satchelsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Satchels).Count - 1;
            legAttachmentsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.LegAttachements).Count - 1;
            holstersCrossdrawItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.HolsterCrossdraw).Count - 1;
            holstersLeftItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.HolstersLeft).Count - 1;
            holstersRightItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.HolstersRight).Count - 1;
            eyewearItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Eyewear).Count - 1;
            masksLargeItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.MasksLarge).Count - 1;
            coatsClosedItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.CoatsClosed).Count - 1;
            loadoutsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Loadouts).Count - 1;
            sheathsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Sheaths).Count - 1;
            apronsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Aprons).Count - 1;
            beardChopsItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.BeardChops).Count - 1;
            mustacheItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Mustache).Count - 1;
            mustacheMpItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.MustacheMP).Count - 1;
            goateesItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.Goatees).Count - 1;
            femaleUnknow01Item.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.HairAccessories).Count - 1;
            talismanBeltItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.TalismanBelt).Count - 1;
            talismanHolsterItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.TalismanHolster).Count - 1;
            talismanSatchelItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.TalismanSatchel).Count - 1;
            talismanWristItem.MaxValue = GetOutfitComponentsByCategory(OutfitComponents.TalismanWrist).Count - 1;

            if (gender == Gender.Male)
            {
                var beardType = new Random(Environment.TickCount).Next(0, 1);

                if (beardType == 0)
                {
                    beardChopsItem.Value = new Random(Environment.TickCount + 1).Next(0, GetOutfitComponentsByCategory(OutfitComponents.BeardChops).Count - 1);
                    goateesItem.Value = new Random(Environment.TickCount + 2).Next(0, GetOutfitComponentsByCategory(OutfitComponents.Goatees).Count - 1);
                    mustacheItem.Value = new Random(Environment.TickCount + 3).Next(0, GetOutfitComponentsByCategory(OutfitComponents.Mustache).Count - 1);
                }
                else
                {
                    mustacheMpItem.Value = new Random(Environment.TickCount + 4).Next(0, GetOutfitComponentsByCategory(OutfitComponents.MustacheMP).Count - 1);
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

            if (sheathsItem.MaxValue > -1) sheathsItem.Visible = true;
            else sheathsItem.Visible = false;

            if (apronsItem.MaxValue > -1) apronsItem.Visible = true;
            else apronsItem.Visible = false;

            if (talismanBeltItem.MaxValue > -1) talismanBeltItem.Visible = true;
            else talismanBeltItem.Visible = false;

            if (talismanHolsterItem.MaxValue > -1) talismanHolsterItem.Visible = true;
            else talismanHolsterItem.Visible = false;

            if (talismanSatchelItem.MaxValue > -1) talismanSatchelItem.Visible = true;
            else talismanSatchelItem.Visible = false;

            if (talismanWristItem.MaxValue > -1) talismanWristItem.Visible = true;
            else talismanWristItem.Visible = false;

            if (holstersCrossdrawItem.MaxValue > -1) holstersCrossdrawItem.Visible = true;
            else holstersCrossdrawItem.Visible = false;

            if (holstersRightItem.MaxValue > -1) holstersRightItem.Visible = true;
            else holstersRightItem.Visible = false;

            if (masksLargeItem.MaxValue > -1) masksLargeItem.Visible = true;
            else masksLargeItem.Visible = false;

            hairItem.Value = new Random(Environment.TickCount + 5).Next(-1, GetOutfitComponentsByCategory(OutfitComponents.Hairs).Count - 1);
            eyesItem.Value = new Random(Environment.TickCount + 6).Next(-1, GetOutfitComponentsByCategory(OutfitComponents.Eyes).Count - 1);
            teethItem.Value = new Random(Environment.TickCount + 7).Next(-1, GetOutfitComponentsByCategory(OutfitComponents.Teeth).Count - 1);

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

            bodyTypesItem.Value = new Random(Environment.TickCount + 8).Next(0, _characterService.bodyTypes.Count - 1);
            waistTypesItem.Value = new Random(Environment.TickCount + 9).Next(0, _characterService.waistTypes.Count - 1);

            SetPedBodyComponent(_characterService.bodyTypes, bodyTypesItem.Value);
            SetPedBodyComponent(_characterService.waistTypes, waistTypesItem.Value);

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
            infoMenu = new MenuContainer(_languageService.Get("Client.CharacterCreator.Info").ToUpper(), "Informations");
            _menuService.AddMenuToHistory(infoMenu);

            genderItem = new ListItem(_languageService.Get("Client.CharacterCreator.Sex"), 0, new Dictionary<string, object>
            {
                { _languageService.Get("Client.CharacterCreator.Male"), 0 },
                { _languageService.Get("Client.CharacterCreator.Female"), 1 }
            }, (async (index, value) =>
            {
                gender = (Gender)(int)value.Values.ToList().ElementAt(index);
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

                //await BaseScript.Delay(100);

                UpdatePedVariation();
                //SetModelAsNoLongerNeeded(model);

                await BaseScript.Delay(750);

                await FadeIn(250);

                if (gender == Gender.Female)
                {
                    clothesMenu.AddItem(skirtsItem);
                    clothesMenu.AddItem(femaleUnknow01Item);

                    beardChopsItem.Visible = false;
                    mustacheItem.Visible = false;
                    mustacheMpItem.Visible = false;
                    goateesItem.Visible = false;
                }
                else
                {
                    clothesMenu.RemoveItem(skirtsItem);
                    clothesMenu.RemoveItem(femaleUnknow01Item);

                    beardChopsItem.Visible = true;
                    mustacheItem.Visible = true;
                    mustacheMpItem.Visible = true;
                    goateesItem.Visible = true;
                }
            }));

            firstAndLastNameItem = new Vector2InputItem(
                new Vector2Input(_languageService.Get("Client.CharacterCreator.Firstname"), "", "", 0, 20, ""),
                new Vector2Input(_languageService.Get("Client.CharacterCreator.Lastname"), "", "", 0, 20, ""), null);
            nationalityAndCityBirthItem = new Vector2InputItem(
               new Vector2Input(_languageService.Get("Client.CharacterCreator.Nationality"), "", "", 0, 30, ""),
               new Vector2Input(_languageService.Get("Client.CharacterCreator.PlaceOfBirth"), "", "", 0, 30, ""), null);
            dateOfBirthItem = new Vector3InputItem(_languageService.Get("Client.CharacterCreator.DateOfBirth"), new Vector3Input("01", "", 1, 2, ""), new Vector3Input("12", "", 1, 2, ""), new Vector3Input("1800", "", 4, 4, ""), null);

            pedScaleItem = new SelectorItem<float>($"{_languageService.Get("Client.CharacterCreator.Height")}: ", 0.95f, 1.05f, 1f, 0.01f, async (item) =>
            {
                var ped = PlayerPedId();
                var size = (int)((item.Value - 0.2f) * 100f); //0.2
                pedScaleItem.Text = $"{_languageService.Get("Client.CharacterCreator.Height")}: 1m{ (size == 100 ? "00" : size.ToString()) }";

                GameAPI.SetPedScale(PlayerPedId(), item.Value);

                //await BaseScript.Delay(100);

                //var headHeight = GetPedBoneCoords(ped, 168, 0f, 0f, 0f).Z + item.Value - 0.4f;
                //PointCamAtCoord(faceCamera, spawnPosition.X + 0.1f, spawnPosition.Y, headHeight);
                //SetCamCoord(faceCamera, spawnPosition.X + 1f, spawnPosition.Y + 1f, headHeight);

                //var bodyHeight = GetPedBoneCoords(ped, 420, 0f, 0f, 0f).Z + pedScaleItem.Value - 0.8f;
                //PointCamAtCoord(bodyCamera, spawnPosition.X + 0.1f, spawnPosition.Y, bodyHeight);
                //SetCamCoord(bodyCamera, spawnPosition.X + 1f, spawnPosition.Y + 2f, bodyHeight);
            });

            // Default ped culture
            origin = CharacterUtilities.Origins[0];

            // Default ped head texture
            textureType["albedo"] = GetHashKey(origin.HeadTexture);

            // Select culture by gender
            origins = CharacterUtilities.Origins.Where(x => x.Gender == gender).ToList();

            culturesItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Origin"), 0, origins.Count - 1, 0, 1, (item) =>
            {
                culturesItem.Text = $"{_languageService.Get("Client.CharacterCreator.Origin")}";
                origin = origins[culturesItem.Value];
                textureType["albedo"] = GetHashKey(origin.HeadTexture);

                headsItem.MinValue = 0;
                headsItem.MaxValue = origin.Heads.Count - 1;
                headsItem.Value = 0;

                bodyItem.MinValue = 0;
                bodyItem.MaxValue = origin.Bodies.Count - 1;
                bodyItem.Value = 0;

                legsItem.MinValue = 0;
                legsItem.MaxValue = origin.Legs.Count - 1;
                legsItem.Value = 0;

                UpdatePedBodyComponent(origin.Heads, headsItem, _languageService.Get(OutfitComponents.Heads));
                UpdatePedBodyComponent(origin.Bodies, bodyItem, _languageService.Get(OutfitComponents.Torsos));
                UpdatePedBodyComponent(origin.Legs, legsItem, _languageService.Get(OutfitComponents.Legs));
            });

            headsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Heads), 0, origin.Heads.Count - 1, 0, 1, (item) =>
            {
                UpdatePedBodyComponent(origin.Heads, headsItem, _languageService.Get(OutfitComponents.Heads));
            });

            bodyItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Torsos), 0, origin.Bodies.Count - 1, 0, 1, (item) =>
            {
                UpdatePedBodyComponent(origin.Bodies, bodyItem, _languageService.Get(OutfitComponents.Torsos));
            });

            legsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Legs), 0, origin.Legs.Count - 1, 0, 1, (item) =>
            {
                UpdatePedBodyComponent(origin.Legs, legsItem, _languageService.Get(OutfitComponents.Legs));
            });

            infoMenu.AddItem(genderItem);
            infoMenu.AddItem(firstAndLastNameItem);
            infoMenu.AddItem(nationalityAndCityBirthItem);
            infoMenu.AddItem(dateOfBirthItem);
            infoMenu.AddItem(dateOfBirthItem);
            infoMenu.AddItem(pedScaleItem);
            infoMenu.AddItem(culturesItem);
            infoMenu.AddItem(headsItem);
            infoMenu.AddItem(bodyItem);
            infoMenu.AddItem(legsItem);
            infoMenu.AddItem(new ButtonItem("Créer le personnage", item => CreateCharacter()));
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
                    return _characterService.clothes.Where(x => x.CategoryHash == categoryHash && x.PedType == (int)gender).ToList();
            }

            return  _characterService.clothes.Where(x => x.CategoryHash == categoryHash && x.IsMultiplayer && x.PedType == (int)gender).ToList();
        }

        private void InitFaceMenu()
        {
            faceMenu = new MenuContainer(_languageService.Get("Client.CharacterCreator.Faces").ToUpper(), "Visage");

            eyesItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Eyes"), 0, GetOutfitComponentsByCategory(OutfitComponents.Eyes).Count - 1, 0, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Eyes, item);
            });

            hairItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Hair"), -1, GetOutfitComponentsByCategory(OutfitComponents.Hairs).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Hairs, item);
            });

            beardChopsItem = new SelectorItem<int>("Favori", -1, GetOutfitComponentsByCategory(OutfitComponents.BeardChops).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.BeardChops, item);
            }, gender == Gender.Male);

            mustacheItem = new SelectorItem<int>("Moustache", -1, GetOutfitComponentsByCategory(OutfitComponents.Mustache).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Mustache, item);
            }, gender == Gender.Male);

            mustacheMpItem = new SelectorItem<int>("Barbe", -1, GetOutfitComponentsByCategory(OutfitComponents.MustacheMP).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.MustacheMP, item);
            }, gender == Gender.Male);

            goateesItem = new SelectorItem<int>("Boucs", -1, GetOutfitComponentsByCategory(OutfitComponents.Goatees).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Goatees, item);
            }, gender == Gender.Male);

            teethItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Teeth"), 0, GetOutfitComponentsByCategory(OutfitComponents.Teeth).Count - 1, 0, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Teeth, item);
            });

            faceMenu.AddItem(eyesItem);
            faceMenu.AddItem(hairItem);
            faceMenu.AddItem(teethItem);
            faceMenu.AddItem(beardChopsItem);
            faceMenu.AddItem(mustacheItem);
            faceMenu.AddItem(mustacheMpItem);
            faceMenu.AddItem(goateesItem);
        }

        private void InitFaceFeaturesMenu()
        {
            faceFeaturesMenu = new MenuContainer(_languageService.Get("Client.CharacterCreator.FaceTraits").ToUpper(), "Modification des vêtements");
            faceMenu.AddItem(new ButtonContainer(_languageService.Get("Client.CharacterCreator.FaceTraits"), faceFeaturesMenu));

            faceFeaturesMenu.AddItem(new ButtonItem(_languageService.Get("Client.CharacterCreator.RandomFace"), (item) =>
            {
                RandomizeFace();
            }));

            headWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.HeadWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.HeadWidth, item.Value);
                characterFaceParts[CharacterFacePart.HeadWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(headWidthItem);

            eyebrowHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyebrowsHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyebrowHeight, item.Value);
                characterFaceParts[CharacterFacePart.EyebrowHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyebrowHeightItem);

            eyebrowWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyebrowsWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyebrowWidth, item.Value);
                characterFaceParts[CharacterFacePart.EyebrowWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyebrowWidthItem);

            eyebrowDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyebrowsDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyebrowDepth, item.Value);
                characterFaceParts[CharacterFacePart.EyebrowDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyebrowDepthItem);

            earsWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EarsWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EarsWidth, item.Value);
                characterFaceParts[CharacterFacePart.EarsWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(earsWidthItem);

            earsAngleItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EarsCurvature"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EarsAngle, item.Value);
                characterFaceParts[CharacterFacePart.EarsAngle] = item.Value;
            });
            faceFeaturesMenu.AddItem(earsAngleItem);

            earsHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EarsSize"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EarsHeight, item.Value);
                characterFaceParts[CharacterFacePart.EarsHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(earsHeightItem);

            earsLobeSizeItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LobeSize"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EarsLobeSize, item.Value);
                characterFaceParts[CharacterFacePart.EarsLobeSize] = item.Value;
            });
            faceFeaturesMenu.AddItem(earsLobeSizeItem);

            cheeckBonesHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.CheekbonesHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.CheeckBonesHeight, item.Value);
                characterFaceParts[CharacterFacePart.CheeckBonesHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(cheeckBonesHeightItem);

            cheeckBonesWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.CheekbonesWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.CheeckBonesWidth, item.Value);
                characterFaceParts[CharacterFacePart.CheeckBonesWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(cheeckBonesWidthItem);

            cheeckBonesDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.CheekbonesDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.CheeckBonesDepth, item.Value);
                characterFaceParts[CharacterFacePart.CheeckBonesDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(cheeckBonesDepthItem);

            jawHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.JawHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.JawHeight, item.Value);
                characterFaceParts[CharacterFacePart.JawHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(jawHeightItem);

            jawWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.JawWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.JawWidth, item.Value);
                characterFaceParts[CharacterFacePart.JawWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(jawWidthItem);

            jawDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.JawDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.JawDepth, item.Value);
                characterFaceParts[CharacterFacePart.JawDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(jawDepthItem);

            chinHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.ChinHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.ChinHeight, item.Value);
                characterFaceParts[CharacterFacePart.ChinHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(chinHeightItem);

            chinWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.ChinWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.ChinWidth, item.Value);
                characterFaceParts[CharacterFacePart.ChinWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(chinWidthItem);

            chinDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.ChinDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.ChinDepth, item.Value);
                characterFaceParts[CharacterFacePart.ChinDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(chinDepthItem);

            eyeLidHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyelidHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyeLidHeight, item.Value);
                characterFaceParts[CharacterFacePart.EyeLidHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyeLidHeightItem);

            eyeLidWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyelidWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyeLidWidth, item.Value);
                characterFaceParts[CharacterFacePart.EyeLidWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyeLidWidthItem);

            eyesDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyesDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyesDepth, item.Value);
                characterFaceParts[CharacterFacePart.EyesDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyesDepthItem);

            eyesAngleItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyesAngle"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyesAngle, item.Value);
                characterFaceParts[CharacterFacePart.EyesAngle] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyesAngleItem);

            eyesDistanceItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyesDistance"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyesDistance, item.Value);
                characterFaceParts[CharacterFacePart.EyesDistance] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyesDistanceItem);

            eyesHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.EyesHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.EyesHeight, item.Value);
                characterFaceParts[CharacterFacePart.EyesHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(eyesHeightItem);

            noseWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.NoseWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.NoseWidth, item.Value);
                characterFaceParts[CharacterFacePart.NoseWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(noseWidthItem);

            noseSizeItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.NoseSize"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.NoseSize, item.Value);
                characterFaceParts[CharacterFacePart.NoseSize] = item.Value;
            });
            faceFeaturesMenu.AddItem(noseSizeItem);

            noseHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.NoseHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.NoseHeight, item.Value);
                characterFaceParts[CharacterFacePart.NoseHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(noseHeightItem);

            noseAngleItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.NoseAngle"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.NoseAngle, item.Value);
                characterFaceParts[CharacterFacePart.NoseAngle] = item.Value;
            });
            faceFeaturesMenu.AddItem(noseAngleItem);

            noseCurvatureItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.NoseCurvature"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.NoseCurvature, item.Value);
                characterFaceParts[CharacterFacePart.NoseCurvature] = item.Value;
            });
            faceFeaturesMenu.AddItem(noseCurvatureItem);

            noStrilsDistanceItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.NostrilsDistance"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.NoStrilsDistance, item.Value);
                characterFaceParts[CharacterFacePart.NoStrilsDistance] = item.Value;
            });
            faceFeaturesMenu.AddItem(noStrilsDistanceItem);

            mouthWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.MouthWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.MouthWidth, item.Value);
                characterFaceParts[CharacterFacePart.MouthWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(mouthWidthItem);

            mouthDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.MouthDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.MouthDepth, item.Value);
                characterFaceParts[CharacterFacePart.MouthDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(mouthDepthItem);

            mouthXPosItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.MouthHorzPos"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.MouthXPos, item.Value);
                characterFaceParts[CharacterFacePart.MouthXPos] = item.Value;
            });
            faceFeaturesMenu.AddItem(mouthXPosItem);

            mouthYPosItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.MouthVertPos"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.MouthYPos, item.Value);
                characterFaceParts[CharacterFacePart.MouthYPos] = item.Value;
            });
            faceFeaturesMenu.AddItem(mouthYPosItem);

            upperLipHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LipsSupHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.UpperLipHeight, item.Value);
                characterFaceParts[CharacterFacePart.UpperLipHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(upperLipHeightItem);

            upperLipWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LipsSupWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.UpperLipWidth, item.Value);
                characterFaceParts[CharacterFacePart.UpperLipWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(upperLipWidthItem);

            upperLipDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LipsSupDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.UpperLipDepth, item.Value);
                characterFaceParts[CharacterFacePart.UpperLipDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(upperLipDepthItem);

            lowerLipHeightItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LipsInfHeight"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.LowerLipHeight, item.Value);
                characterFaceParts[CharacterFacePart.LowerLipHeight] = item.Value;
            });
            faceFeaturesMenu.AddItem(lowerLipHeightItem);

            lowerLipWidthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LipsInfWidth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.LowerLipWidth, item.Value);
                characterFaceParts[CharacterFacePart.LowerLipWidth] = item.Value;
            });
            faceFeaturesMenu.AddItem(lowerLipWidthItem);

            lowerLipDepthItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.LipsInfDepth"), -1f, 1f, 0f, 0.1f, (item) =>
            {
                SetPedFaceFeature(CharacterFacePart.LowerLipDepth, item.Value);
                characterFaceParts[CharacterFacePart.LowerLipDepth] = item.Value;
            });
            faceFeaturesMenu.AddItem(lowerLipDepthItem);
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
            faceOverlayMenu = new MenuContainer(_languageService.Get("Client.CharacterCreator.Facies").ToString().ToUpper(), "Modification du visage");
            faceMenu.AddItem(new ButtonContainer(_languageService.Get("Client.CharacterCreator.Facies"), faceOverlayMenu));

            var overlays = CharacterUtilities.FaceOverlays.Where(x => x.Name == "eyebrows").ToList();
            var overlayInfo = overlays[0];

            characterOverlays[overlayInfo.Name].TextureId = overlayInfo.Id;
            characterOverlays[overlayInfo.Name].TextureNormal = overlayInfo.Normal;
            characterOverlays[overlayInfo.Name].TextureMaterial = overlayInfo.Material;

            overlayTypeItem = new SelectorItem<int>("", 0, CharacterUtilities.FaceOverlayLayers.Count - 1, 0, 1, async (item) =>
            {
                var overlay = characterOverlays.ElementAt(item.Value).Value;

                overlays = CharacterUtilities.FaceOverlays.Where(x => x.Name == overlay.Name).ToList();

                if (overlays.Exists(x => x.Id == overlay.Id))
                {
                    overlayInfo = overlays.Find(x => x.Id == overlay.Id);
                }
                else
                {
                    overlayInfo = overlays[0];
                }

                await BaseScript.Delay(0);

                overlayTypeItem.Text = overlay.Name;

                overlayItem.MaxValue = overlays.Count - 1;
                overlayItem.Value = overlays.IndexOf(overlayInfo);
                overlayItem.Text = $"{_languageService.Get("Client.CharacterCreator.Style")}";

                overlayInfo = overlays[overlayItem.Value];

                characterOverlays[overlayInfo.Name].TextureId = overlayInfo.Id;
                characterOverlays[overlayInfo.Name].TextureNormal = overlayInfo.Normal;
                characterOverlays[overlayInfo.Name].TextureMaterial = overlayInfo.Material;

                overlayVisibilityItem.Checked = characterOverlays[overlayInfo.Name].TextureVisibility;
                overlayVisibilityItem.OnRender(_uiService);

                switch (overlayInfo.Name)
                {
                    case "eyeliners":
                        overlayItem.Visible = false;
                        overlayVarItem.Visible = true;
                        overlayVarItem.MaxValue = 15;
                        break;
                    case "shadows":
                        overlayItem.Visible = false;
                        overlayVarItem.Visible = true;
                        overlayVarItem.MaxValue = 5;
                        break;
                    case "lipsticks":
                        overlayItem.Visible = false;
                        overlayVarItem.Visible = true;
                        overlayVarItem.MaxValue = 7;
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

                overlayTypeItem.OnRender(_uiService);
                overlayItem.OnRender(_uiService);

                overlayVarItem.Value = characterOverlays[overlayInfo.Name].Variante;
                overlayVarItem.Text = $"{_languageService.Get("Client.CharacterCreator.Variant")}";
                overlayVarItem.OnRender(_uiService);

                overlayPrimaryColorItem.Value = characterOverlays[overlayInfo.Name].PalettePrimaryColor;
                overlayPrimaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.PrimaryColor")}";
                overlayPrimaryColorItem.OnRender(_uiService);

                overlaySecondaryColorItem.Value = characterOverlays[overlayInfo.Name].PaletteSecondaryColor;
                overlaySecondaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.SecondaryColor")}";
                overlaySecondaryColorItem.OnRender(_uiService);

                overlayTertiaryColorItem.Value = characterOverlays[overlayInfo.Name].PaletteTertiaryColor;
                overlayTertiaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.TertiaryColor")}";
                overlayTertiaryColorItem.OnRender(_uiService);

                overlayPaletteItem.MinValue = 0;
                overlayPaletteItem.MaxValue = _characterService.colorPalettes.Count - 1;
                overlayPaletteItem.Value = _characterService.colorPalettes.IndexOf(characterOverlays[overlayInfo.Name].Palette.ToString("X"));
                overlayPaletteItem.Text = $"{_languageService.Get("Client.CharacterCreator.Palette")}";
                overlayPaletteItem.OnRender(_uiService);

                overlayOpacityItem.Value = characterOverlays[overlayInfo.Name].Opacity;
                overlayOpacityItem.Text = $"{_languageService.Get("Client.CharacterCreator.Opacity")}";
                overlayOpacityItem.OnRender(_uiService);

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Style"), 0, overlays.Count - 1, 0, 1, async (item) =>
            {
                overlayItem.Text = $"{_languageService.Get("Client.CharacterCreator.Style")}";

                overlayInfo = overlays[overlayItem.Value];

                characterOverlays[overlayInfo.Name].TextureId = overlayInfo.Id;
                characterOverlays[overlayInfo.Name].TextureNormal = overlayInfo.Normal;
                characterOverlays[overlayInfo.Name].TextureMaterial = overlayInfo.Material;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayVisibilityItem = new CheckboxItem(_languageService.Get("Client.CharacterCreator.Visible"), false, async (item) =>
            {
                characterOverlays[overlayInfo.Name].TextureVisibility = item.Checked;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayVarItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Variant"), 0, 254, 0, 1, async (item) =>
            {
                overlayVarItem.Text = $"{_languageService.Get("Client.CharacterCreator.Variant")}";

                characterOverlays[overlayInfo.Name].Variante = overlayVarItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            }, false);

            overlayPrimaryColorItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.PrimaryColor"), 0, 254, 0, 1, async (item) =>
            {
                overlayPrimaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.PrimaryColor")}";

                characterOverlays[overlayInfo.Name].PalettePrimaryColor = overlayPrimaryColorItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlaySecondaryColorItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.SecondaryColor"), 0, 254, 0, 1, async (item) =>
            {
                overlaySecondaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.SecondaryColor")}";

                characterOverlays[overlayInfo.Name].PaletteSecondaryColor = overlaySecondaryColorItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayTertiaryColorItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.TertiaryColor"), 0, 254, 0, 1, async (item) =>
            {
                overlayTertiaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.TertiaryColor")}";

                characterOverlays[overlayInfo.Name].PaletteTertiaryColor = overlayTertiaryColorItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayPaletteItem = new SelectorItem<int>(_languageService.Get("Client.CharacterCreator.Palette"), 0, _characterService.colorPalettes.Count - 1, 0, 1, async (item) =>
            {
                overlayPaletteItem.Text = $"{_languageService.Get("Client.CharacterCreator.Palette")}";

                characterOverlays[overlayInfo.Name].Palette = uint.Parse(_characterService.colorPalettes[overlayPaletteItem.Value], NumberStyles.AllowHexSpecifier);

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            overlayOpacityItem = new SelectorItem<float>(_languageService.Get("Client.CharacterCreator.Opacity"), 0f, 1f, 1f, 0.1f, async (item) =>
            {
                overlayOpacityItem.Text = $"{_languageService.Get("Client.CharacterCreator.Opacity")}";

                characterOverlays[overlayInfo.Name].Opacity = overlayOpacityItem.Value;

                await BaseScript.Delay(0);

                UpdateOverlay();
            });

            faceOverlayMenu.AddItem(overlayTypeItem);
            faceOverlayMenu.AddItem(overlayItem);
            faceOverlayMenu.AddItem(overlayVisibilityItem);
            faceOverlayMenu.AddItem(overlayVarItem);
            faceOverlayMenu.AddItem(overlayPrimaryColorItem);
            faceOverlayMenu.AddItem(overlaySecondaryColorItem);
            faceOverlayMenu.AddItem(overlayTertiaryColorItem);
            faceOverlayMenu.AddItem(overlayPaletteItem);
            faceOverlayMenu.AddItem(overlayOpacityItem);

            overlayTypeItem.Text = CharacterUtilities.FaceOverlays[overlayTypeItem.Value].Name;

            overlayItem.Text = $"{_languageService.Get("Client.CharacterCreator.Style")}";
            overlayVarItem.Text = $"{_languageService.Get("Client.CharacterCreator.Variant")}";
            overlayPrimaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.PrimaryColor")}";
            overlaySecondaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.SecondaryColor")}";
            overlayTertiaryColorItem.Text = $"{_languageService.Get("Client.CharacterCreator.TertiaryColor")}";
            overlayPaletteItem.Text = $"{_languageService.Get("Client.CharacterCreator.Palette")}";
            overlayOpacityItem.Text = $"{_languageService.Get("Client.CharacterCreator.Opacity")}";
        }

        private void InitBodyMenu()
        {
            bodyMenu = new MenuContainer(_languageService.Get("Client.CharacterCreator.Body").ToUpper(), "Corps");

            bodyTypesItem = new SelectorItem<int>("Morphologie", 0, _characterService.bodyTypes.Count - 1, 0, 1, (item) =>
            {
                bodyTypesItem.Text = $"{_languageService.Get("Client.CharacterCreator.Morphology")}";

                SetPedBodyComponent((uint)_characterService.bodyTypes[bodyTypesItem.Value]);
            });

            waistTypesItem = new SelectorItem<int>("Poids", 0, _characterService.waistTypes.Count - 1, 0, 1, (item) =>
            {
                waistTypesItem.Text = $"{_languageService.Get("Client.CharacterCreator.Weight")}";

                SetPedBodyComponent((uint)_characterService.waistTypes[waistTypesItem.Value]);
            });

            bodyMenu.AddItem(bodyTypesItem);
            bodyMenu.AddItem(waistTypesItem);
        }

        private async void InitClothMenu()
        {
            clothesMenu = new MenuContainer(_languageService.Get("Client.CharacterCreator.Clothes").ToUpper(), "Vêtements");

            hatsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Hats), -1, GetOutfitComponentsByCategory(OutfitComponents.Hats).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Hats, item);
            });

            eyewearItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Eyewear), -1, GetOutfitComponentsByCategory(OutfitComponents.Eyewear).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Eyewear, item);
            });

            neckwearItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Neckwear), -1, GetOutfitComponentsByCategory(OutfitComponents.Neckwear).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Neckwear, item);
            });

            necktiesItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Neckties), -1, GetOutfitComponentsByCategory(OutfitComponents.Neckties).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Neckties, item);
            });

            shirtsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Shirts), -1, GetOutfitComponentsByCategory(OutfitComponents.Shirts).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Shirts, item);
            });

            suspendersItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Suspenders), -1, GetOutfitComponentsByCategory(OutfitComponents.Suspenders).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Suspenders, item);
            });

            vestItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Vests), -1, GetOutfitComponentsByCategory(OutfitComponents.Vests).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Vests, item);
            });

            coatsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Coats), -1, GetOutfitComponentsByCategory(OutfitComponents.Coats).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Coats, item);
            });

            coatsClosedItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.CoatsClosed), -1, GetOutfitComponentsByCategory(OutfitComponents.CoatsClosed).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.CoatsClosed, item);
            });

            ponchosItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Ponchos), -1, GetOutfitComponentsByCategory(OutfitComponents.Ponchos).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Ponchos, item);
            });

            cloakItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Cloaks), -1, GetOutfitComponentsByCategory(OutfitComponents.Cloaks).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Cloaks, item);
            });

            glovesItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Gloves), -1, GetOutfitComponentsByCategory(OutfitComponents.Gloves).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Gloves, item);
            });

            ringsRightHandItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.RingsRightHand), -1, GetOutfitComponentsByCategory(OutfitComponents.RingsRightHand).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.RingsRightHand, item);
            });

            ringsLeftHandItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.RingsLeftHand), -1, GetOutfitComponentsByCategory(OutfitComponents.RingsLeftHand).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.RingsLeftHand, item);
            });

            braceletsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Bracelts), -1, GetOutfitComponentsByCategory(OutfitComponents.Bracelts).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Bracelts, item);
            });

            gunbeltItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Gunbelts), -1, GetOutfitComponentsByCategory(OutfitComponents.Gunbelts).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Gunbelts, item);
            });

            beltItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Belts), -1, GetOutfitComponentsByCategory(OutfitComponents.Belts).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Belts, item);
            });

            buckleItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Beltbuckles), -1, GetOutfitComponentsByCategory(OutfitComponents.Beltbuckles).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Beltbuckles, item);
            });

            holstersCrossdrawItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.HolsterCrossdraw), -1, GetOutfitComponentsByCategory(OutfitComponents.HolsterCrossdraw).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.HolsterCrossdraw, item);
            });

            holstersLeftItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.HolstersLeft), -1, GetOutfitComponentsByCategory(OutfitComponents.HolstersLeft).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.HolstersLeft, item);
            });

            holstersRightItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.HolstersRight), -1, GetOutfitComponentsByCategory(OutfitComponents.HolstersRight).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.HolstersRight, item);
            });

            pantsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Pants), -1, GetOutfitComponentsByCategory(OutfitComponents.Pants).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Pants, item);
            });

            skirtsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Skirts), -1, GetOutfitComponentsByCategory(OutfitComponents.Skirts).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Skirts, item);
            });

            bootsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Boots), -1, GetOutfitComponentsByCategory(OutfitComponents.Boots).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Boots, item);
            });

            chapsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Chaps), -1, GetOutfitComponentsByCategory(OutfitComponents.Chaps).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Chaps, item);
            });

            spursItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Spurs), -1, GetOutfitComponentsByCategory(OutfitComponents.Spurs).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Spurs, item);
            });

            spatsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Spats), -1, GetOutfitComponentsByCategory(OutfitComponents.Spats).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Spats, item);
            });

            satchelsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Satchels), -1, GetOutfitComponentsByCategory(OutfitComponents.Satchels).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Satchels, item);
            });

            masksItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Masks), -1, GetOutfitComponentsByCategory(OutfitComponents.Masks).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Masks, item);
            });

            masksLargeItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.MasksLarge), -1, GetOutfitComponentsByCategory(OutfitComponents.MasksLarge).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.MasksLarge, item);
            });

            loadoutsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Loadouts), -1, GetOutfitComponentsByCategory(OutfitComponents.Loadouts).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Loadouts, item);
            });

            legAttachmentsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.LegAttachements), -1, GetOutfitComponentsByCategory(OutfitComponents.LegAttachements).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.LegAttachements, item);
            });

            gauntletsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Gauntlets), -1, GetOutfitComponentsByCategory(OutfitComponents.Gauntlets).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Gauntlets, item);
            });

            accessoriesItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Accessories), -1, GetOutfitComponentsByCategory(OutfitComponents.Accessories).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Accessories, item);
            });

            sheathsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Sheaths), -1, GetOutfitComponentsByCategory(OutfitComponents.Sheaths).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Sheaths, item);
            });

            apronsItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.Aprons), -1, GetOutfitComponentsByCategory(OutfitComponents.Aprons).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.Aprons, item);
            });

            femaleUnknow01Item = new SelectorItem<int>(_languageService.Get(OutfitComponents.HairAccessories), -1, GetOutfitComponentsByCategory(OutfitComponents.HairAccessories).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.HairAccessories, item);
            });

            talismanBeltItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.TalismanBelt), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanBelt).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.TalismanBelt, item);
            });

            talismanHolsterItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.TalismanHolster), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanHolster).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.TalismanHolster, item);
            });

            talismanSatchelItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.TalismanSatchel), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanSatchel).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.TalismanSatchel, item);
            });

            talismanWristItem = new SelectorItem<int>(_languageService.Get(OutfitComponents.TalismanWrist), -1, GetOutfitComponentsByCategory(OutfitComponents.TalismanWrist).Count - 1, -1, 1, async (item) =>
            {
                await SetPedComponent(OutfitComponents.TalismanWrist, item);
            });

            clothesMenu.AddItem(new LabelItem("Tête"));
            clothesMenu.AddItem(hatsItem);
            clothesMenu.AddItem(eyewearItem);
            clothesMenu.AddItem(masksItem);
            clothesMenu.AddItem(masksLargeItem);
            clothesMenu.AddItem(new LabelItem("Nuque"));
            clothesMenu.AddItem(neckwearItem);
            clothesMenu.AddItem(necktiesItem);
            clothesMenu.AddItem(new LabelItem("Haut du corps"));
            clothesMenu.AddItem(shirtsItem);
            clothesMenu.AddItem(vestItem);
            clothesMenu.AddItem(suspendersItem);
            clothesMenu.AddItem(ponchosItem);
            clothesMenu.AddItem(cloakItem);
            clothesMenu.AddItem(chapsItem);
            clothesMenu.AddItem(coatsItem);
            clothesMenu.AddItem(coatsClosedItem);
            clothesMenu.AddItem(new LabelItem("Bas du corps"));
            clothesMenu.AddItem(pantsItem);
            if (gender == Gender.Female) clothesMenu.AddItem(skirtsItem);
            clothesMenu.AddItem(beltItem);
            clothesMenu.AddItem(buckleItem);
            clothesMenu.AddItem(legAttachmentsItem);
            clothesMenu.AddItem(new LabelItem("Bottes"));
            clothesMenu.AddItem(bootsItem);
            clothesMenu.AddItem(spursItem);
            clothesMenu.AddItem(spatsItem);
            clothesMenu.AddItem(new LabelItem("Mains / Bras"));
            clothesMenu.AddItem(glovesItem);
            clothesMenu.AddItem(gauntletsItem);
            clothesMenu.AddItem(ringsRightHandItem);
            clothesMenu.AddItem(ringsLeftHandItem);
            clothesMenu.AddItem(braceletsItem);
            clothesMenu.AddItem(new LabelItem("Accessoires"));
            clothesMenu.AddItem(satchelsItem);
            clothesMenu.AddItem(accessoriesItem);
            if (gender == Gender.Female) clothesMenu.AddItem(femaleUnknow01Item);
            clothesMenu.AddItem(new LabelItem("Ceintures d'arme / Holsters"));
            clothesMenu.AddItem(gunbeltItem);
            clothesMenu.AddItem(loadoutsItem);
            clothesMenu.AddItem(holstersCrossdrawItem);
            clothesMenu.AddItem(holstersLeftItem);
            clothesMenu.AddItem(holstersRightItem);
            clothesMenu.AddItem(new LabelItem("Etuis"));
            clothesMenu.AddItem(sheathsItem);
            clothesMenu.AddItem(apronsItem);

            if (gender == Gender.Male)
            {
                clothesMenu.AddItem(new LabelItem("Talismans"));
                clothesMenu.AddItem(talismanBeltItem);
                clothesMenu.AddItem(talismanHolsterItem);
                clothesMenu.AddItem(talismanSatchelItem);
                clothesMenu.AddItem(talismanWristItem);
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
            var values = new Dictionary<int, float>();

            for (int i = 0; i < _characterService.faceParts.Count; i++)
            {
                var part = int.Parse(_characterService.faceParts[i], NumberStyles.AllowHexSpecifier);
                var rand = new Random(Environment.TickCount * (i == 0 ? 1 : i)).Next(-10, 10) / 10f;
                SetPedFaceFeature(part, rand);
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

        private void UpdatePedBodyComponent(List<string> components, SelectorItem<int> item, string text)
        {
            var component = components[item.Value];
            item.Text = text;
            item.OnRender(_uiService);

            UpdatePedVariation(PlayerPedId());
            SetPedComponentEnabled(PlayerPedId(), (uint)FromHexToHash(component), true, true, false);
        }

        private async Task SetPedComponent(string categoryHash, SelectorItem<int> item)
        {
            try
            {
                var clothes = GetOutfitComponentsByCategory(categoryHash);
                //var clothes = _characterService.clothes.Where(x => x.CategoryHash == categoryHash && x.PedType == (int)gender).ToList();
                var clothName = _languageService.Get(categoryHash);

                if (item.Value == -1)
                {
                    RemovePedComponent(uint.Parse(categoryHash, NumberStyles.AllowHexSpecifier));
                    characterClothes[categoryHash] = 0;
                    item.Text = $"Aucun(e) {clothName}";
                    item.OnRender(_uiService);
                }
                else
                {
                    if (item.Value > clothes.Count)
                    {
                        item.Value = clothes.Count;
                    }

                    if (item.Value < 0)
                    {
                        item.Value = 0;
                    }

                    var cloth = clothes[item.Value];
                    var component = uint.Parse(cloth.Hash, NumberStyles.AllowHexSpecifier);

                    SetPedComponentEnabled(PlayerPedId(), component, true, cloth.IsMultiplayer, false);
                    characterClothes[cloth.CategoryHash] = component;
                    UpdatePedVariation();

                    item.Text = clothName;
                    item.OnRender(_uiService);
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
