using CitizenFX.Core;
using Newtonsoft.Json;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Rpc;
using SDK.Client.Utils;
using SDK.Shared.DataModels;
using SDK.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class CharacterManager : ICharacterManager
    {
        Logger logger;
        EventManager eventManager;
        RpcRequest rpc;

        int textureId = -1;
        const int RefreshPedScaleInternal = 5000;

        public List<CharacterCloth> Clothes { get; private set; }
        public List<PedCulture> PedCultures { get; private set; }
        public List<int> BodyTypes { get; private set; }
        public List<int> WaistTypes { get; private set; }


        public CharacterData Current { get; private set; }

        public CharacterManager(Logger logger, ThreadManager thread, EventManager eventManager, RpcRequest rpc)
        {
            this.logger = logger;
            this.eventManager = eventManager;
            this.rpc = rpc;

            Clothes = Configuration.Parse<List<CharacterCloth>>("utils/clothes.json");
            BodyTypes = Configuration.Parse<List<int>>("utils/body_types.json");
            WaistTypes = Configuration.Parse<List<int>>("utils/waist_types.json");
            PedCultures = CharacterUtils.PedCultures;

            thread.StartThread(PedScaleUpdate);
        }

        #region Threads

        protected async Task PedScaleUpdate()
        {
            if (Current == null)
                return;

            GameAPI.SetPedScale(PlayerPedId(), Current.Scale);
            await BaseScript.Delay(RefreshPedScaleInternal);
        }

        #endregion

        #region Methods

        public async Task IsReady()
        {
            while (Current == null) await BaseScript.Delay(0);
        }

        public async Task<bool> Exist()
        {
            var receive = false;
            var result = false;

            rpc.Event("Character.Exist").On<bool>((exist) =>
            {
                result = exist;
                receive = true;
            }).Emit();

            while (!receive) await BaseScript.Delay(0);
            return result;
        }

        public async Task Load()
        {
            Current = null;

            logger.Debug("Getting character..");

            rpc.Event("Character.Load").On<CharacterData>(data =>
            {
                logger.Debug("Getted character: " + (data == null ? "No character" : data.RockstarId));
                Current = data;
            }).Emit();

            while (Current == null) await BaseScript.Delay(0);
        }

        public async Task Reload()
        {
            var health = Current.Core.Health;
            var hunger = Current.Core.Hunger;
            var thirst = Current.Core.Thirst;

            Current = null;

            logger.Debug("Getting character..");

            rpc.Event("Character.Load").On<CharacterData>(data =>
            {
                Current = data;
                Current.Core.Health = health;
                Current.Core.Hunger = hunger;
                Current.Core.Thirst = thirst;
            }).Emit();

            while (Current == null) await BaseScript.Delay(0);
        }

        public async Task SetPed(uint model, int variante)
        {
            RequestModel(model, true);

            while (!HasModelLoaded(model)) await BaseScript.Delay(0);

            SetPlayerModel(model);
            SetPedOutfitPreset(PlayerPedId(), variante);
        }

        public async Task LoadSkin()
        {
            SetPedBody();
            SetPedBodyComponents();

            await UpdateOverlay();
            await SetPedClothes();
            await SetPedFaceFeatures();

            await BaseScript.Delay(1000);

            SetPedComponentDisabled(PlayerPedId(), 0x3F1F01E5, 0, false);
            SetPedComponentDisabled(PlayerPedId(), 0xDA0E2C55, 0, false);

            await BaseScript.Delay(1000);

            UpdatePedVariation();
        }

        public async Task SaveAsync()
        {
            await BaseScript.Delay(1000 * 60 * 5);

            var ped = PlayerPedId();
            var coords = GetEntityCoords(ped, true, true);
            var heading = GetEntityHeading(ped);
            Current.Position = new PositionData(coords.X, coords.Y, coords.Z, heading);

            Save();
        }

        public void SavePosition()
        {
            var ped = PlayerPedId();
            var coords = GetEntityCoords(ped, true, true);
            var heading = GetEntityHeading(ped);
            Current.Position = new PositionData(coords.X, coords.Y, coords.Z, heading);
        }

        public void Save()
        {
            var ped = PlayerPedId();
            Current.Core.Health = GetEntityHealth(ped);
            eventManager.EmitServer("Character.Save", JsonConvert.SerializeObject(Current));
        }

        public void Create(CharacterData data) => eventManager.EmitServer("Character.Save", JsonConvert.SerializeObject(data));

        public void SetPedBody()
        {
            var ped = PlayerPedId();
            var cultures = PedCultures.Where(x => x.Gender == Current.SexType).ToList();
            var culture = cultures[Current.Culture];
            var head = culture.Heads[Current.Head];
            var body = culture.Body[Current.Body];
            var legs = culture.Legs[Current.Legs];
            var headTexture = culture.HeadTexture;

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, FromHexToHash(head), false, true, true);

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, FromHexToHash(body), false, true, true);

            Call(0x704C908E9C405136, ped);
            Call(0xD3A7B003ED343FD9, ped, FromHexToHash(legs), false, true, true);

            // Remove default pants
            RemovePedComponent(CharacterClothComponents.Pants);
        }

        async Task SetPedFaceFeatures()
        {
            foreach (var part in Current.FaceParts)
                SetPedFaceFeature(part.Key, part.Value);
        }

        async Task SetPedClothes()
        {
            foreach (var cloth in Current.Clothes)
            {
                if (cloth.Value != 0)
                {
                    var c = Clothes.Find(x => x.Hash == cloth.Value.ToString("X8"));
                    SetPedComponentEnabled(PlayerPedId(), cloth.Value, true, c.IsMultiplayer, false);
                    await BaseScript.Delay(100);
                    UpdatePedVariation();
                    await BaseScript.Delay(100);
                }
            }
        }

        public async Task SetPedOutfit(Dictionary<string, uint> newClothes, int delay = 100)
        {
            foreach (var cloth in newClothes)
            {
                if (cloth.Value != 0)
                {
                    var c = Clothes.Find(x => x.Hash == cloth.Value.ToString("X8"));
                    SetPedComponentEnabled(PlayerPedId(), cloth.Value, true, c.IsMultiplayer, false);
                    await BaseScript.Delay(delay);
                    UpdatePedVariation();
                    await BaseScript.Delay(delay);
                }
            }
        }

        void SetPedBodyComponents()
        {
            SetPedBodyComponent((uint)BodyTypes[Current.BodyType]);
            SetPedBodyComponent((uint)WaistTypes[Current.WaistType]);
        }

        public async Task UpdateOverlay()
        {
            var ped = PlayerPedId();

            if (textureId != -1)
            {
                ResetPedTexture2(textureId);
                DeletePedTexture(textureId);
            }

            textureId = Call<int>(0xC5E7204F322E49EB, Current.Texture["albedo"], Current.Texture["normal"], Current.Texture["material"]);

            foreach (var layer in Current.Overlays.Values)
            {
                if (layer.Visibility != 0)
                {
                    int overlayId = AddPedOverlay(textureId, layer.TxId, layer.TxNormal, layer.TxMaterial, layer.TxColorType, layer.TxOpacity, layer.TxUnk);

                    if (layer.TxColorType == 0)
                    {
                        SetPedOverlayPalette(textureId, overlayId, layer.Palette);
                        SetPedOverlayPaletteColour(textureId, overlayId, layer.PaletteColorPrimary, layer.PaletteColorSecondary, layer.PaletteColorTertiary);
                    }

                    SetPedOverlayVariation(textureId, overlayId, layer.Var);
                    SetPedOverlayOpacity(textureId, overlayId, layer.Opacity);
                }

                while (!IsPedTextureValid(textureId)) await BaseScript.Delay(0);

                OverrideTextureOnPed(ped, (uint)GetHashKey("heads"), textureId);
                UpdatePedTexture(textureId);
                UpdatePedVariation();
            }
        }

        public void SetMoney(decimal amount)
        {
            Current.Economy.Money = amount;
            Save();
        }

        public void SetBank(decimal amount)
        {
            Current.Economy.Bank = amount;
            Save();
        }

        public void AddMoney(decimal amount)
        {
            Current.Economy.Money += amount;
            Save();
        }

        public void AddBank(decimal amount)
        {
            Current.Economy.Bank += amount;
            Save();
        }

        public void RemoveMoney(decimal amount)
        {
            var newAmount = Current.Economy.Money - amount;

            if (newAmount >= 0)
            {
                Current.Economy.Money -= amount;
            }

            Save();
        }

        public void RemoveBank(decimal amount)
        {
            var newAmount = Current.Economy.Bank - amount;

            if (newAmount >= 0)
            {
                Current.Economy.Bank -= amount;
            }

            Save();
        }

        public void RemoveAllClothes()
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

        #endregion

        #region Events

        [EventHandler("Character.SetPed")]
        private async void OnSetPedEvent(uint model, int variante) => await SetPed(model, variante);

        #endregion
    }
}
