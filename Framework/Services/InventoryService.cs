using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Storage;
using Average.Shared.DataModels;
using Average.Shared.Enums;
using Average.Shared.Events;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static Average.Client.Framework.Services.InputService;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class InventoryService
    {
        private readonly InputService _inputService;
        private readonly WorldService _worldService;
        private readonly UIService _uiService;
        private readonly ClientService _clientService;
        private readonly EventService _eventService;
        private readonly RpcService _rpcService;

        public const int InventorySlotCount = 20;
        public const int VehicleSlotCount = 8;
        public const int ChestSlotCount = 8;
        public const int BankSlotCount = 4;
        public const int TradeSlotCount = 16;

        private const bool SaveOnChanged = true;

        public List<StorageItemInfo> Items { get; set; }

        public bool IsOpen { get; set; }
        public StorageData Inventory { get; set; }
        public StorageData CurrentChest { get; set; }
        public bool IsInventoryOpen { get; set; }
        public bool IsChestOpen { get; set; }

        public InventoryService(RpcService rpcService, EventService eventService, InputService inputService, WorldService worldService, UIService uiService, ClientService clientService)
        {
            _inputService = inputService;
            _worldService = worldService;
            _uiService = uiService;
            _clientService = clientService;
            _eventService = eventService;
            _rpcService = rpcService;

            // Events
            _worldService.TimeChanged += OnTimeChanged;

            // Inputs
            _inputService.RegisterKey(new Input((Control)0xC1989F95,
            condition: () =>
            {
                return true;
            },
            onStateChanged: (state) =>
            {
                Logger.Debug($"Client can open/close inventory");
            },
            onKeyReleased: () =>
            {
                Open();
                OpenInventory();
                Logger.Debug($"Client open inventory");
            }));

            Logger.Debug("InventoryService Initialized successfully");
        }

        private void OnTimeChanged(object sender, WorldTimeEventArgs e)
        {
            SetTime(e.Time);
        }

        internal void OnClientWindowInitialized()
        {
            _uiService.LoadFrame("storage");
            _uiService.SetZIndex("storage", 90000);
        }

        internal void Update(StorageData storageData)
        {
            _eventService.EmitServer("inventory:update", storageData.ToJson());
        }

        internal void SetItemInfo(string itemName, StorageItemInfo itemInfo)
        {
            var info = Items.Find(x => x.Name == itemName);
            info.OnStacking = itemInfo.OnStacking;
            info.OnRenderStacking = itemInfo.OnRenderStacking;
            info.OnSplit = itemInfo.OnSplit;
            info.SplitCondition = itemInfo.SplitCondition;
            info.OnStackCombine = itemInfo.OnStackCombine;
            info.OnRenderSplit = itemInfo.OnRenderSplit;
            info.ContextMenu = itemInfo.ContextMenu;
        }

        internal void InitSlots(StorageDataType storageType, int slotCount)
        {
            var type = GetStorageTypeString(storageType);

            _uiService.SendNui("storage", "init", new
            {
                slotCount,
                type
            });
        }

        internal void LoadInventory(StorageData storageData)
        {
            foreach (var item in storageData.Items)
            {
                SetItemOnEmptySlot(storageData, item);
            }
        }

        internal async Task<StorageData> Get(string storageId)
        {
            StorageData data = null;

            _rpcService.OnResponse<StorageData>("inventory:get", (storageData) =>
            {
                data = storageData;
            }).Emit(storageId);

            while (data == null)
            {
                await BaseScript.Delay(250);
            }

            return data;
        }

        internal async void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;

                if (!Call<bool>(0x4A123E85D7C4CA0B, PostEffect.PauseMenuIn))
                {
                    Call(0x4102732DF6B4005F, PostEffect.PauseMenuIn);
                }

                _uiService.SendNui("storage", "open");
                _uiService.Focus();
            }
        }

        internal void OpenInventory()
        {
            if (!IsInventoryOpen)
            {
                _uiService.SendNui("storage", "inventory_open");

                IsInventoryOpen = true;
            }
        }

        internal void OpenChest(StorageData storageData)
        {
            if (!IsChestOpen)
            {
                CurrentChest = storageData;
                _uiService.SendNui("storage", "chest_open");

                IsChestOpen = true;
            }
        }

        internal async void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;

                if (Call<bool>(0x4A123E85D7C4CA0B, PostEffect.PauseMenuIn))
                {
                    Call(0xB4FD7446BAB2F394, PostEffect.PauseMenuIn);
                }

                _uiService.SendNui("storage", "close");
                _uiService.Unfocus();
            }
        }

        internal void CloseInventory()
        {
            if (IsInventoryOpen)
            {
                _uiService.SendNui("storage", "inventory_close");

                IsInventoryOpen = false;
            }
        }

        internal void CloseChest()
        {
            if (IsChestOpen)
            {
                _uiService.SendNui("storage", "chest_close");
                CurrentChest = null;

                IsChestOpen = false;
            }
        }

        internal bool HaveItem(string itemName, StorageData storageData)
        {
            return storageData.Items.Exists(x => x.Name == itemName);
        }

        internal bool HaveItemCount(string itemName, int itemCount, StorageData storageData)
        {
            var item = storageData.Items.Find(x => x.Name == itemName);
            return item != null && item.Count >= itemCount;
        }

        internal StorageItemData GetItemFromStorage(int slotId, StorageData storageData)
        {
            return storageData.Items.Find(x => x.SlotId == slotId);
        }

        internal StorageItemInfo GetItemInfo(string itemName)
        {
            return Items.Find(x => x.Name == itemName);
        }

        internal void SetInventoryWeight(StorageData storageData)
        {
            var type = GetStorageTypeString(storageData.Type);

            _uiService.SendNui("storage", "setWeight", new
            {
                weight = CalculateWeight(storageData).ToString("0.00"),
                maxWeight = storageData.MaxWeight.ToString("0.00"),
                type
            });
        }

        internal void SetTime(TimeSpan time)
        {
            _uiService.SendNui("storage", "setTime", new
            {
                time = string.Format($"{(time.Hours < 10 ? "0" + time.Hours : time.Hours)}h{(time.Minutes < 10 ? "0" + time.Minutes : time.Minutes)}", time)
            });
        }

        internal void SetTemperature(int temperature)
        {
            _uiService.SendNui("storage", "setTemperature", new
            {
                temperature = temperature + "°C"
            });
        }

        internal double CalculateWeight(StorageData storageData)
        {
            var weight = 0d;
            storageData.Items.ForEach(x => weight += GetItemInfo(x.Name).Weight * x.Count);
            return weight;
        }

        internal bool HasFreeSpaceForWeight(StorageItemData itemData, StorageData storageData)
        {
            var info = GetItemInfo(itemData.Name);
            var totalNeededWeight = CalculateWeight(storageData) + itemData.Count * info.Weight;

            Logger.Debug("Weight: " + totalNeededWeight + ", " + storageData.MaxWeight);
            Logger.Debug("Has Free Space: " + (totalNeededWeight <= storageData.MaxWeight));

            if (totalNeededWeight > storageData.MaxWeight)
            {
                if (info.CanBeStacked && info.OnStacking != null)
                {
                    return true;
                }
            }

            return totalNeededWeight <= storageData.MaxWeight;
        }

        internal bool HasFreeSpace(StorageData storageData)
        {
            return CalculateWeight(storageData) <= storageData.MaxWeight;
        }

        internal bool IsSlotAvailable(int slotId, StorageData storageData)
        {
            return !storageData.Items.Exists(x => x.SlotId == slotId);
        }

        internal bool ItemExistsByName(string itemName, StorageData storageData)
        {
            return storageData.Items.Exists(x => x.Name == itemName);
        }

        internal StorageItemData GetItemOnSlot(int slotId, StorageData storageData)
        {
            return storageData.Items.Find(x => x.SlotId == slotId);
        }

        internal StorageItemData GetItemByName(string itemName, StorageData storageData)
        {
            return storageData.Items.Find(x => x.Name == itemName);
        }

        internal bool IsSlotExistsWithItemName(string itemName, StorageData storageData)
        {
            return storageData.Items.Exists(x => x.Name == itemName);
        }

        internal int GetAvailableSlot(StorageData storageData)
        {
            var slotCount = 0;

            switch (storageData.Type)
            {
                case StorageDataType.Player:
                    slotCount = InventorySlotCount;
                    break;
                case StorageDataType.Vehicle:
                    slotCount = VehicleSlotCount;
                    break;
                case StorageDataType.Chest:
                    slotCount = ChestSlotCount;
                    break;
                case StorageDataType.Bank:
                    slotCount = BankSlotCount;
                    break;
                case StorageDataType.Trade:
                    slotCount = TradeSlotCount;
                    break;
            }


            for (int i = 0; i < slotCount; i++)
            {
                if (!storageData.Items.Exists(x => x.SlotId == i))
                {
                    return i;
                }
            }

            // No available slot
            return -1;
        }

        private string GetStorageTypeString(StorageDataType storageType)
        {
            var type = "";

            switch (storageType)
            {
                case StorageDataType.Player:
                    type = "inv";
                    break;
                case StorageDataType.Vehicle:
                    type = "veh";
                    break;
                case StorageDataType.Chest:
                    type = "chest";
                    break;
                case StorageDataType.Bank:
                    type = "bank";
                    break;
                case StorageDataType.Trade:
                    type = "trade";
                    break;
            }

            return type;
        }

        internal void ShowSplitMenu(StorageDataType storageType, StorageItemInfo itemInfo, int slotId, object minValue, object maxValue, object defaultValue)
        {
            var type = GetStorageTypeString(storageType);

            _uiService.SendNui("storage", "showInvSplitMenu", new
            {
                slotId,
                title = itemInfo.Text,
                img = itemInfo.Img,
                defaultValue,
                minValue,
                maxValue,

                type
            });
        }

        internal void OnSplitItem(int slotId, object minValue, object maxValue, object value, StorageData storage)
        {
            SplitItem(slotId, minValue, maxValue, value, storage);
        }

        internal void SplitItem(int slotId, object minValue, object maxValue, object value, StorageData storageData)
        {
            var item = GetItemOnSlot(slotId, storageData);
            if (item == null) return;

            var info = GetItemInfo(item.Name);
            minValue = Convert.ChangeType(minValue, info.SplitValueType);
            maxValue = Convert.ChangeType(maxValue, info.SplitValueType);
            value = Convert.ChangeType(value, info.SplitValueType);

            switch (value)
            {
                case int convertedValue:
                    if (item.Count == (int)minValue) return;

                    var valResult = 0;

                    if (convertedValue == (int)minValue || convertedValue == (int)maxValue)
                    {
                        // 15 - 15 = 0
                        // 15 - (15 - 1) = f:14 / s:1
                        valResult = (int)maxValue - 1;
                    }
                    else
                    {
                        //valResult = (int)maxValue - (int)convertedValue;
                        valResult = convertedValue;
                    }

                    if (info.OnSplit != null)
                    {
                        // Split custom
                        // Application des modifications sur l'item après le split
                        info.OnSplit.Invoke(item, valResult, StorageItemInfo.SplitType.BaseItem);
                    }
                    else
                    {
                        // Split par défaut
                        item.Count = valResult;
                    }

                    var newSlotId = GetAvailableSlot(storageData);
                    var newItem = new StorageItemData(item.Name, (int)maxValue - valResult);
                    newItem.SlotId = newSlotId;

                    Logger.Error("New slot id after split: " + slotId + ", " + newSlotId + ", " + storageData.Id + ", " + storageData.Type);

                    var newDictionary = item.Data.ToDictionary(entry => entry.Key, entry => entry.Value);
                    newItem.Data = newDictionary;

                    if (storageData == null) return;

                    storageData.Items.Add(newItem);
                    SetItemOnEmptySlot(storageData, newItem);

                    // Appel l'action par defaut
                    // Met à jour l'affichage du premier item
                    UpdateSlotRender(item, info, storageData);

                    if (SaveOnChanged)
                    {
                        Update(storageData);
                    }
                    break;
                case decimal convertedValue:
                    var canSplit = info.SplitCondition != null && info.SplitCondition.Invoke(item);
                    if (!canSplit) return;

                    var valDecResult = 0m;

                    if (convertedValue == (decimal)minValue || convertedValue == (decimal)maxValue)
                    {
                        // 15 - 15 = 0
                        // 15 - (15 - 1) = f:14 / s:1
                        valDecResult = (decimal)maxValue - 1;
                    }
                    else
                    {
                        //valResult = (decimal)maxValue - (decimal)convertedValue;
                        valDecResult = convertedValue;
                    }

                    if (info.OnSplit != null)
                    {
                        info.OnSplit.Invoke(item, valDecResult, StorageItemInfo.SplitType.BaseItem);
                    }

                    newSlotId = GetAvailableSlot(storageData);
                    newItem = new StorageItemData(item.Name, 1);
                    newItem.SlotId = newSlotId;

                    // Besoin de copier les données, sinon les données du base item et du target item sont encore lier
                    newDictionary = item.Data.ToDictionary(entry => entry.Key, entry => entry.Value);

                    newItem.Data = newDictionary;
                    info.OnSplit.Invoke(newItem, valDecResult, StorageItemInfo.SplitType.TargetItem);

                    if (storageData == null) return;

                    storageData.Items.Add(newItem);
                    SetItemOnEmptySlot(storageData, newItem);

                    // Appel l'action par defaut
                    // Met à jour l'affichage du premier item
                    UpdateSlotRender(item, info, storageData);

                    if (SaveOnChanged)
                    {
                        Update(storageData);
                    }
                    break;
            }
        }

        internal void AddItem(StorageItemData newItem, StorageData storageData, bool createInNewSlot = false, int createInSlotId = 0)
        {
            var info = GetItemInfo(newItem.Name);

            if (info == null)
            {
                Logger.Error("This item template doesn't exists");
                return;
            }

            newItem.Count = (newItem.Count > 0 ? newItem.Count : newItem.Count = 1);
            newItem.Data ??= new Dictionary<string, object>();

            if (info.DefaultData != null)
            {
                foreach (var d in info.DefaultData)
                {
                    if (!newItem.Data.ContainsKey(d.Key))
                    {
                        newItem.Data.Add(d.Key, d.Value);
                    }
                }
            }

            var availableSlot = -1;

            if (HasFreeSpaceForWeight(newItem, storageData))
            {
                if (info.CanBeStacked)
                {
                    if (IsSlotExistsWithItemName(newItem.Name, storageData))
                    {
                        var slot = GetItemByName(newItem.Name, storageData);
                        if (slot == null) return;

                        availableSlot = slot.SlotId;

                        // Besoin d'assigner a newItem le SlotId existant
                        newItem.SlotId = slot.SlotId;
                    }
                    else
                    {
                        availableSlot = GetAvailableSlot(storageData);
                    }
                }
                else
                {
                    availableSlot = GetAvailableSlot(storageData);
                }

                if (availableSlot != -1 || createInNewSlot)
                {
                    if (createInNewSlot)
                    {
                        availableSlot = createInSlotId;
                    }

                    if (IsSlotAvailable(availableSlot, storageData))
                    {
                        if (!createInNewSlot)
                        {
                            // Créer un nouvelle item dans un slot disponible
                            newItem.SlotId = availableSlot;
                            storageData.Items.Add(newItem);

                            if (info.OnStacking != null)
                            {
                                newItem.Count = 1;
                            }

                            SetItemOnEmptySlot(storageData, newItem);
                        }
                        else
                        {
                            newItem.SlotId = createInSlotId;
                            storageData.Items.Add(newItem);

                            if (info.OnStacking != null)
                            {
                                newItem.Count = 1;
                            }

                            SetItemOnEmptySlot(storageData, newItem);
                        }

                        if (SaveOnChanged)
                        {
                            Update(storageData);
                        }
                    }
                    else
                    {
                        if (info.CanBeStacked)
                        {
                            // Modifie un item dans un slot existant
                            // Modifie la quantité de l'item sur un slot existant

                            if (info.OnStacking != null)
                            {
                                newItem.Count = 1;

                                // Appel une action définis
                                var targetItem = GetItemOnSlot(availableSlot, storageData);
                                var itemIndex = storageData.Items.FindIndex(x => x.SlotId == targetItem.SlotId);

                                storageData.Items.RemoveAt(itemIndex);
                                storageData.Items.Add(targetItem);

                                StackItemOnSlot(storageData, newItem, targetItem);

                                if (SaveOnChanged)
                                {
                                    Update(storageData);
                                }
                            }
                            else
                            {
                                if (!createInNewSlot)
                                {
                                    // Appel l'action par defaut
                                    var itemInstance = storageData.Items.Find(x => x.SlotId == newItem.SlotId);

                                    StackItemOnSlot(storageData, newItem, itemInstance);

                                    if (SaveOnChanged)
                                    {
                                        Update(storageData);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Créer un nouvelle item dans un slot disponible
                            newItem.SlotId = availableSlot;
                            storageData.Items.Add(newItem);

                            SetItemOnEmptySlot(storageData, newItem);

                            if (SaveOnChanged)
                            {
                                Update(storageData);
                            }
                        }
                    }
                }
                else
                {
                    Logger.Error($"No slot available for: {storageData.Type} -> {storageData.StorageId}");
                }
            }
            else
            {
                Logger.Debug($"[InventoryService] Unable to add item because you have not enought place.");
            }
        }

        internal void StackItemOnSlot(StorageData storageData, StorageItemData source, StorageItemData destination)
        {
            var info = GetItemInfo(destination.Name);

            object itemStackValue = null;

            if (info.CanBeStacked)
            {
                if (info.OnStacking != null)
                {
                    // Modifie le nombre d'item sur l'instance de l'item
                    info.OnStacking.Invoke(source, destination);
                }
                else
                {
                    // Modifie le nombre d'item sur l'instance de l'item
                    destination.Count += source.Count;
                }
            }
            else
            {
                // Modifie le nombre d'item sur l'instance de l'item
                destination.Count += source.Count;
            }

            if (info.CanBeStacked && info.OnRenderStacking != null)
            {
                itemStackValue = info.OnRenderStacking.Invoke(destination);
            }
            else
            {
                itemStackValue = destination.Count;
            }

            var type = GetStorageTypeString(storageData.Type);

            _uiService.SendNui("storage", "stackItemOnSlot", new
            {
                slotId = destination.SlotId,
                count = itemStackValue,
                img = info.Img,

                type
            });

            SetInventoryWeight(storageData);
        }

        internal void MoveItemOnStorage(int slotId, int targetSlotId, StorageData sourceStorage, StorageData destinationStorage)
        {
            if (sourceStorage.Type == destinationStorage.Type)
            {
                // Inventaire -> Inventaire

                if (slotId == targetSlotId) return;

                // On déplace l'item sur un slot définis
                SetItemOnSlot(destinationStorage, slotId, targetSlotId);
            }
            else
            {
                // Inventaire -> Coffre

                // Récupère l'item dans l'inventaire du joueur
                var item = GetItemOnSlot(slotId, sourceStorage);
                if (item == null) return;

                // Besoin de créer une copie de l'item avant de l'ajouter dans le coffre
                // pour éviter que l'instance de l'item dans le coffre soit la même que celle de l'inventaire
                var newItem = new StorageItemData(item.Name, item.Count);
                newItem.SlotId = targetSlotId;

                // Créer une copie des données de l'item (sans l'instance) et les copies sur newItem
                var newDictionary = item.Data.ToDictionary(entry => entry.Key, entry => entry.Value);
                newItem.Data = newDictionary;

                if (IsSlotAvailable(newItem.SlotId, destinationStorage))
                {
                    // On vérifie que l'inventaire à assez de place pour recevoir l'item provenant du coffre
                    if (!HasFreeSpaceForWeight(newItem, destinationStorage))
                    {
                        Logger.Error("Pas assez de place dans le coffre pour ajouter l'item de l'inventaire: " + newItem.Name + ", " + newItem.Count);
                        return;
                    }

                    // Ajoute l'item dans le coffre sur un slot spécifique
                    AddItem(newItem, destinationStorage, true, targetSlotId);

                    // Supprime l'item de l'inventaire
                    RemoveItemOnSlot(sourceStorage, slotId);

                    // Met à jour l'inventaire et le coffre dans la base de donnée
                    Update(sourceStorage);
                    Update(destinationStorage);
                }
                else
                {
                    // Récupère l'instance de l'item dans le coffre
                    var itemInstance = destinationStorage.Items.Find(x => x.SlotId == targetSlotId);
                    if (itemInstance == null) return;

                    if (itemInstance.Name == newItem.Name)
                    {
                        // Les items sont identique, on les stack

                        // On vérifie que l'inventaire à assez de place pour recevoir l'item provenant du coffre
                        if (!HasFreeSpaceForWeight(newItem, destinationStorage))
                        {
                            Logger.Error("Pas assez de place dans le coffre pour stack l'item de l'inventaire: " + itemInstance.Name + ", " + itemInstance.Count);
                            return;
                        }

                        // Besoin de stack l'item dans le coffre
                        StackItemOnSlot(destinationStorage, newItem, itemInstance);

                        // Supprime l'item de l'inventaire
                        RemoveItemOnSlot(sourceStorage, slotId);

                        // Met à jour l'inventaire et le coffre dans la base de donnée
                        Update(sourceStorage);
                        Update(destinationStorage);
                    }
                    else
                    {
                        // Les items sont différent, on les alternes

                        var sourceCopy = new StorageItemData(item.Name, item.Count);
                        sourceCopy.SlotId = item.SlotId;
                        var sourceDictionary = item.Data.ToDictionary(entry => entry.Key, entry => entry.Value);
                        sourceCopy.Data = sourceDictionary;

                        var destinationCopy = new StorageItemData(itemInstance.Name, itemInstance.Count);
                        destinationCopy.SlotId = itemInstance.SlotId;
                        var destinationDictionary = itemInstance.Data.ToDictionary(entry => entry.Key, entry => entry.Value);
                        destinationCopy.Data = destinationDictionary;

                        // On vérifie que l'inventaire à assez de place pour recevoir l'item provenant du coffre
                        if (!HasFreeSpaceForWeight(destinationCopy, sourceStorage))
                        {
                            Logger.Error("Pas assez de place dans l'inventaire pour recevoir l'item du coffre: " + destinationCopy.Name + ", " + destinationCopy.Count);
                            return;
                        }

                        // On vérifie que le coffre à assez de place pour recevoir l'item provenant de l'inventaire
                        if (!HasFreeSpaceForWeight(sourceCopy, destinationStorage))
                        {
                            Logger.Error("Pas assez de place dans le coffre pour recevoir l'item de l'inventaire: " + sourceCopy.Name + ", " + sourceCopy.Count);
                            return;
                        }

                        // On supprime l'item de l'inventaire ce trouvant sur le slotId définis avant d'ajouter le nouvelle item
                        RemoveItemOnSlot(sourceStorage, slotId);

                        // On supprime l'item du coffre ce trouvant sur le slotId définis avant d'ajouter le nouvelle item
                        RemoveItemOnSlot(destinationStorage, targetSlotId);

                        // Ajoute l'item du coffre dans l'inventaire
                        AddItem(destinationCopy, sourceStorage, true, slotId);

                        // Ajoute l'item de l'inventaire dans le coffre
                        AddItem(sourceCopy, destinationStorage, true, targetSlotId);
                    }
                }
            }
        }

        internal void SetItemOnSlot(StorageData storageData, int currentSlotId, int targetSlotId)
        {
            var item = GetItemOnSlot(currentSlotId, storageData);
            if (item == null) return;

            var info = GetItemInfo(item.Name);

            var targetItem = GetItemOnSlot(targetSlotId, storageData);
            var haveTarget = targetItem is not null;

            var type = GetStorageTypeString(storageData.Type);

            // La cible peu soit être un slot d'item ou un slot vide
            if (haveTarget)
            {
                var targetInfo = GetItemInfo(targetItem.Name);

                if (info.Name != targetInfo.Name)
                {
                    // Les items n'ont pas le même nom
                    // On alterne le slotId des cibles pour inverser leur position dans l'interface
                    item.SlotId = targetSlotId;
                    targetItem.SlotId = currentSlotId;

                    // Alterne la position de deux slot, ItemA -> ItemB, ItemB -> ItemA
                    // Si la propriété "CanBeStacked" à la valeur true, les items ne doivent pas être alterner mais "additionner"
                    _uiService.SendNui("storage", "setItemOnSlot", new
                    {
                        // Base Slot
                        slotId = item.SlotId,
                        count = (info.CanBeStacked && info.OnRenderStacking != null) ? info.OnRenderStacking.Invoke(item) : item.Count,
                        img = info.Img,

                        // Target Slot
                        targetSlotId = targetItem.SlotId,
                        targetCount = (targetInfo.CanBeStacked && targetInfo.OnRenderStacking != null) ? targetInfo.OnRenderStacking.Invoke(targetItem) : targetItem.Count,
                        targetImg = targetInfo.Img,
                        contextItems = GetItemContextMenu(targetItem.Name),
                        type
                    });
                }
                else
                {
                    // Les deux items ont le même nom
                    StackCombineItem(storageData, item, targetItem);
                }

                if (SaveOnChanged)
                {
                    Update(storageData);
                }
            }
            else
            {
                item.SlotId = targetSlotId;

                // Déplace l'item vers une case vide, Item -> Slot vide
                _uiService.SendNui("storage", "moveItemOnEmptySlot", new
                {
                    // Base Slot
                    slotId = currentSlotId,

                    // Target Slot
                    targetSlotId = targetSlotId,
                    targetCount = (info.CanBeStacked && info.OnRenderStacking != null) ? info.OnRenderStacking.Invoke(item) : item.Count,
                    targetImg = info.Img,
                    contextItems = GetItemContextMenu(item.Name),
                    type
                });

                if (SaveOnChanged)
                {
                    Update(storageData);
                }
            }
        }

        private void UpdateSlotRender(StorageItemData itemData, StorageItemInfo itemInfo, StorageData storageData)
        {
            var type = GetStorageTypeString(storageData.Type);

            _uiService.SendNui("storage", "updateSlotRender", new
            {
                slotId = itemData.SlotId,
                count = (itemInfo.CanBeStacked && itemInfo.OnRenderStacking != null) ? itemInfo.OnRenderStacking.Invoke(itemData) : itemData.Count,
                img = itemInfo.Img,

                type
            });
        }

        private void StackCombineItem(StorageData storageData, StorageItemData source, StorageItemData destination)
        {
            var info = GetItemInfo(source.Name);
            var targetInfo = GetItemInfo(destination.Name);

            if (targetInfo.CanBeStacked)
            {
                if (targetInfo.OnStackCombine != null && targetInfo.OnRenderStacking != null)
                {
                    // Action définis ex:(money)
                    targetInfo.OnStackCombine.Invoke(source, destination);
                }
                else
                {
                    // Action par defaut ex:(apple)
                    destination.Count += source.Count;
                }

                UpdateSlotRender(destination, targetInfo, storageData);
                RemoveItemOnSlot(storageData, source.SlotId);

                SetInventoryWeight(storageData);
            }
        }

        internal void SetItemCountOnSlot(StorageData storageData, int slotId, int itemCount)
        {
            var item = GetItemOnSlot(slotId, storageData);
            if (item == null) return;

            item.Count = itemCount;

            var info = GetItemInfo(item.Name);
            UpdateSlotRender(item, info, storageData);

            SetInventoryWeight(storageData);
        }

        internal void RemoveItemOnSlot(StorageData storageData, int slotId)
        {
            var type = GetStorageTypeString(storageData.Type);

            var itemIndex = storageData.Items.FindIndex(x => x.SlotId == slotId);
            if (itemIndex == -1) return;

            storageData.Items.RemoveAt(itemIndex);

            _uiService.SendNui("storage", "removeItemOnSlot", new
            {
                slotId,

                type
            });

            SetInventoryWeight(storageData);
        }

        internal void SetItemOnEmptySlot(StorageData storageData, StorageItemData storageItemData)
        {
            var info = GetItemInfo(storageItemData.Name);
            var type = GetStorageTypeString(storageData.Type);

            _uiService.SendNui("storage", "setItemOnEmptySlot", new
            {
                slotId = storageItemData.SlotId,
                count = (info.CanBeStacked && info.OnRenderStacking != null) ? info.OnRenderStacking.Invoke(storageItemData) : storageItemData.Count,
                img = info.Img,
                contextItems = GetItemContextMenu(storageItemData.Name),

                type
            });

            SetInventoryWeight(storageData);
        }

        private List<object> GetItemContextMenu(string itemName)
        {
            var contextMenu = new List<object>();
            var info = GetItemInfo(itemName);

            Logger.Error("GetItemContextMenu: " + itemName + ", " + (info.ContextMenu == null));

            if (info.ContextMenu != null)
            {
                foreach (var contextItem in info.ContextMenu.Items)
                {
                    contextMenu.Add(new
                    {
                        text = contextItem.Text,
                        emoji = contextItem.Emoji,
                        eventName = contextItem.EventName
                    });
                }
            }

            return contextMenu;
        }

        internal async Task OnStorageContextMenu(string itemName, int slotId, string eventName, StorageData storageData)
        {
            var info = GetItemInfo(itemName);

            var context = info.ContextMenu.GetContext(eventName);
            if (context == null) return;

            var item = GetItemOnSlot(slotId, storageData);
            if (item == null) return;

            var raycast = GetTarget(PlayerPedId(), 6f);

            context.Action.Invoke(storageData, item, raycast);
        }
    }
}
