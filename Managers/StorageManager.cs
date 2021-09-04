using System.Collections.Generic;
using System.Threading.Tasks;
using Average.Client.Storage;
using CitizenFX.Core;
using Newtonsoft.Json;
using SDK.Client;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.Models;
using SDK.Client.Storage;
using SDK.Client.Utils;
using SDK.Shared;
using SDK.Shared.DataModels;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class StorageManager : InternalPlugin, IStorageManager
    {
        private bool _isReady;

        private static EventManager _event;
        
        public List<StorageItemInfo> ItemsInfo { get; private set; }
        
        public decimal InventoryDecimalInput { get; private set; } = 1.0m;
        public int InventoryIntegerInput { get; private set; } = 1;
        public decimal ChestDecimalInput { get; private set; } = 1.0m;
        public int ChestIntegerInput { get; private set; } = 1;

        public StorageData? CurrentInventoryData { get; private set; }
        public StorageData? CurrentChestData { get; private set; }
        
        public IStorageContainer? InventoryContainer { get; private set; }
        public IStorageContainer? ChestContainer { get; private set; }
        
        public bool IsOpen { get; set; }
        public bool CanOpen { get; set; }
        public bool IsChestCurrentlyUsed { get; set; }
        public bool IsAnotherPlayerInventory { get; set; }

        public override void OnInitialized()
        {
            _event = Event;
            
            #region Rpc

            Rpc.Event("Storage.GetRegisteredItems").On<List<StorageItemInfo>>(OnGetRegisteredItemsEvent).Emit();

            #endregion

            InventoryContainer = new StorageContainer(this, Character, StorageDataType.PlayerInventory);

            Task.Factory.StartNew(async () =>
            {
                await Character.IsReady();
                await IsItemsInfoReady();

                RegisterItems();
                    
                CurrentInventoryData = await LoadInventory(Character.Current.RockstarId);
                
                CanOpen = true;
                
                await Show("player_" + Character.Current.RockstarId);
                Thread.StartThread(KeyboardUpdate);
            });
        }

        #region Command

        [ClientCommand("storage.add_item", "admin", 4)]
        private void AddItemCommand(string itemName, int itemCount)
        {
            AddInventoryItem(itemName, itemCount);
        }

        #endregion
        
        private void RegisterItems()
        {
            SetContextMenu("money", GetMoneyContextMenu(GetItemInfo("money")));
        }

        #region Context Menu

        public StorageContextMenu GetDefaultContextMenu(StorageItemInfo info) => new(new StorageContextItem
        {
            Name = info.Name,
            EventName = "moveToChest",
            Text = "Déplacer vers le coffre",
            Emoji = "➡",
            Action = (storage, item, ray) =>
            {
                storage.MoveItemFromInventoryToChest(item.UniqueId, storage.InventoryIntegerInput);
                _event.EmitServer("Storage.Refresh",
                    storage.CurrentChestData.StorageId);
            }
        }, new StorageContextItem
        {
            Name = info.Name,
            EventName = "moveToInventory",
            Text = "Déplacer vers l'inventaire",
            Emoji = "⬅",
            Action = (storage, item, ray) =>
            {
                if (storage.IsAnotherPlayerInventory)
                {
                    storage.MoveItemFromChestToInventory(item.UniqueId, storage.ChestIntegerInput);
                    storage.RemoveItemToPlayerFrontOfMe(item.Name, item.Count);
                }
                else
                {
                    storage.MoveItemFromChestToInventory(item.UniqueId, storage.ChestIntegerInput);
                    _event.EmitServer("Storage.Refresh",
                        storage.CurrentChestData.StorageId);
                }
            }
        }, new StorageContextItem
        {
            Name = info.Name,
            EventName = "give",
            Text = "Donner",
            Emoji = "🎁",
            Action = (storage, item, ray) =>
            {
                storage.GiveItemToPlayerFromOfMe(item, storage.InventoryIntegerInput);
            }
        }, new StorageContextItem
        {
            Name = info.Name,
            EventName = "drop",
            Text = "Jeter",
            Emoji = "❌",
            Action = (storage, item, ray) =>
            {
                storage.RemoveInventoryItemById(item.UniqueId, storage.InventoryIntegerInput);
            }
        });

        public static StorageContextMenu GetMoneyContextMenu(StorageItemInfo info) => new StorageContextMenu(new StorageContextItem
        {
            Name = info.Name,
            EventName = "give",
            Text = "Donner",
            Emoji = "🎁",
            Action = (storage, item, ray) =>
            {
                storage.GiveMoneyToPlayerFrontOfMe(storage.InventoryDecimalInput);
            }
        });
        
        #endregion
        
        #region Thread

        private async Task KeyboardUpdate()
        {
            if (CanOpen)
            {
                if (IsControlJustReleased(0, (uint)Keys.I))
                {
                    Open();
                    await Show("player_" + Character.Current.RockstarId);
                }

                // if (IsControlJustReleased(0, (uint)Keys.N6))
                // {
                //     Open();
                //     Main.GetScript<CraftContainer>().ShowBuildMenu();
                // }
            }
            else
            {
                await BaseScript.Delay(1000);
            }
        }

        #endregion

        public async Task IsReady()
        {
            while (CurrentInventoryData == null) await BaseScript.Delay(0);
        }
        
        public async Task IsItemsInfoReady()
        {
            while (ItemsInfo == null) await BaseScript.Delay(0);
        }

        public void Remove(string storageId)
        {
            Event.EmitServer("Storage.Remove", storageId);
        }

        public void Create(StorageDataType storageType, float maxWeight, string customId = null)
        {
            var storageId = "";

            switch (storageType)
            {
                case StorageDataType.PlayerInventory:
                    storageId = "player_" + customId;
                    break;
                case StorageDataType.VehicleInventory:
                    storageId = "vehicle_" + customId;
                    break;
                case StorageDataType.Chest:
                    storageId = "chest_" + customId;
                    break;
            }

            var storage = new StorageData(storageId, maxWeight, new List<StorageItemData>());
            Event.EmitServer("Storage.Create", JsonConvert.SerializeObject(storage));
        }

        public async Task<StorageData> LoadInventory(string storageId)
        {
            CurrentInventoryData = null;

            Rpc.Event("Storage.GetInventory").On<StorageData>(storageData =>
            {
                CurrentInventoryData = storageData;
            }).Emit("player_" + storageId);

            while (CurrentInventoryData == null) await BaseScript.Delay(0);

            if (!CurrentInventoryData.Items.Exists(x => x.Name == "money"))
            {
                AddInventoryItem("money", 1);
                SaveInventory();
            }

            return CurrentInventoryData;
        }
        
        public async Task<StorageData> LoadChest(string storageId)
        {
            CurrentChestData = null;

            Rpc.Event("Storage.GetChest").On<StorageData>((storageData) =>
            {
                CurrentChestData = storageData;
            }).Emit(storageId);

            while (CurrentChestData == null) await BaseScript.Delay(0);
            return CurrentChestData;
        }
        
        public async void SetContextMenu(string itemName, StorageContextMenu context)
        {
            await IsItemsInfoReady();
            var itemInfo = GetItemInfo(itemName);
            itemInfo.ContextMenu = context;
        }
        
        public StorageItemData GetItemInStorage(StorageData storage, string uniqueId) => storage.Items.Find(x => x.UniqueId == uniqueId);
        
        public StorageItemInfo GetItemInfo(string itemName) => ItemsInfo.Find(x => x.Name == itemName);
        
        public void GiveMoneyToPlayerFrontOfMe(decimal amount)
        {
            var ray = GetTarget(PlayerPedId(), 6f);

            if (ray.Hit)
            {
                if (ray.EntityType == 1)
                {
                    if (DoesEntityExist(ray.EntityHit))
                    {
                        if (IsPedAPlayer(ray.EntityHit))
                        {
                            foreach (int player in GetActivePlayers())
                            {
                                var tempPed = GetPlayerPed(player);

                                if (tempPed == ray.EntityHit)
                                {
                                    var targetServerId = GetPlayerServerId(player);

                                    if (amount <= Character.Current.Economy.Money)
                                    {
                                        Character.RemoveMoney(amount);
                                        InventoryContainer.UpdateRender(CurrentInventoryData);
                                        Event.EmitServer("Storage.GiveMoneyToPlayer", targetServerId, amount);
                                    }
                                    else
                                    {
                                        Notification.Schedule("INVENTAIRE", "Vous ne pouvez pas donner plus d'argent que vous n'en avez.", 3000);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public void GiveItemToPlayerFromOfMe(StorageItemData giveItem, int itemCount, bool removeOnGive = true)
        {
            var ray = GetTarget(PlayerPedId(), 6f);

            if (ray.Hit)
            {
                if (ray.EntityType == 1)
                {
                    if (DoesEntityExist(ray.EntityHit))
                    {
                        foreach (int player in GetActivePlayers())
                        {
                            var tempPed = GetPlayerPed(player);

                            if (tempPed == ray.EntityHit)
                            {
                                var targetServerId = GetPlayerServerId(player);
                                var item = CurrentInventoryData.Items.Find(x => x.Name == giveItem.Name);

                                if (item == null) return;

                                if (itemCount <= item.Count)
                                {
                                    Rpc.Event("Storage.HasFreeSpace").On<bool>((hasFreeSpace) =>
                                    {
                                        if (hasFreeSpace)
                                        {
                                            if (removeOnGive)
                                            {
                                                RemoveInventoryItemById(giveItem.UniqueId, itemCount);
                                                InventoryContainer.UpdateRender(CurrentInventoryData);
                                            }
                                            
                                            Event.EmitServer("Storage.GiveItemToPlayer", targetServerId, JsonConvert.SerializeObject(giveItem), itemCount, false);
                                        }
                                    }).Emit(targetServerId);
                                }
                                else
                                {
                                    Notification.Schedule("INVENTAIRE", $"Vous ne pouvez pas donner plus de {GetItemInfo(item.Name).Text} que vous n'en possèdez.", 3000);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public void RemoveItemToPlayerFrontOfMe(string itemName, int itemCount)
        {
            var ray = GetTarget(PlayerPedId(), 6f);

            if (ray.Hit)
            {
                if (ray.EntityType == 1)
                {
                    if (DoesEntityExist(ray.EntityHit))
                    {
                        foreach (int player in GetActivePlayers())
                        {
                            var tempPed = GetPlayerPed(player);

                            if (tempPed == ray.EntityHit)
                            {
                                var targetServerId = GetPlayerServerId(player);

                                Rpc.Event("Storage.HasFreeSpace").On<bool>((hasFreeSpace) =>
                                {
                                    if (hasFreeSpace)
                                    {
                                        Event.EmitServer("Storage.RemoveItem", targetServerId, itemName, itemCount);
                                    }
                                }).Emit(targetServerId);
                            }
                        }
                    }
                }
            }
        }
        
        public void RemoveItemToPlayerFrontOfMeById(string uniqueId, int itemCount)
        {
            var ray = GetTarget(PlayerPedId(), 6f);

            if (ray.Hit)
            {
                if (ray.EntityType == 1)
                {
                    if (DoesEntityExist(ray.EntityHit))
                    {
                        foreach (int player in GetActivePlayers())
                        {
                            var tempPed = GetPlayerPed(player);

                            if (tempPed == ray.EntityHit)
                            {
                                var targetServerId = GetPlayerServerId(player);

                                Rpc.Event("Storage.HasFreeSpace").On<bool>((hasFreeSpace) =>
                                {
                                    if (hasFreeSpace)
                                    {
                                        Event.EmitServer("Storage.RemoveItem", targetServerId, uniqueId, itemCount);
                                    }
                                }).Emit(targetServerId);
                            }
                        }
                    }
                }
            }
        }
        
        public void MoveItemFromInventoryToChest(string uniqueId, int count)
        {
            if (ChestContainer == null || CurrentChestData == null) return;
            
            if (InventoryContainer.ItemExistById(uniqueId, CurrentInventoryData))
            {
                var item = GetItemInStorage(CurrentInventoryData, uniqueId);
                var info = GetItemInfo(item.Name);

                if (count <= item.Count)
                {
                    if (ChestContainer.HasFreeSpace(info.Weight * count))
                    {
                        AddChestItem(info.Name, count);
                        RemoveInventoryItemById(item.UniqueId, count);
                        InventoryContainer.UpdateRender(CurrentInventoryData);
                        ChestContainer.UpdateRender(CurrentChestData);
                    }
                }
            }
        }
        
        public void MoveItemFromChestToInventory(string uniqueId, int count)
        {
            if (ChestContainer == null || CurrentChestData == null) return;
            
            if (ChestContainer.ItemExistById(uniqueId, CurrentChestData))
            {
                var item = GetItemInStorage(CurrentChestData, uniqueId);
                var info = GetItemInfo(item.Name);

                if (count <= item.Count)
                {
                    if (InventoryContainer.HasFreeSpace(info.Weight * count))
                    {
                        AddInventoryItem(info.Name, count);
                        RemoveChestItem(item.UniqueId, count);
                        InventoryContainer.UpdateRender(CurrentInventoryData);
                        ChestContainer.UpdateRender(CurrentChestData);
                    }
                }
            }
        }
        
        public void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
                ChestContainer = null;
                IsAnotherPlayerInventory = false;
                IsChestCurrentlyUsed = false;
                CurrentChestData = null;

                if (AnimpostfxIsRunning(PostEffect.PauseMenuIn))
                    AnimpostfxStop(PostEffect.PauseMenuIn);
                
                SendNUI(new
                {
                    eventName = "avg.internal",
                    on = "storage.close",
                    plugin = "storage"
                });
                Unfocus();
                
                Event.Emit("Storage.Close");
            }
        }
        
        public void Open()
        {
            IsOpen = true;

            if (!AnimpostfxIsRunning(PostEffect.PauseMenuIn))
                AnimpostfxPlay(PostEffect.PauseMenuIn);
            
            SendNUI(new
            {
                eventName = "avg.internal",
                on = "storage.open",
                plugin = "storage"
            });
            Focus();
            
            Event.Emit("Storage.Open");
        }

        public async Task Show(string storageId, bool isPlayerInventory = false, CharacterData characterData = null)
        {
            if (storageId.StartsWith("player_") && !isPlayerInventory)
            {
                InventoryContainer.CalculateWeight(CurrentInventoryData);
                InventoryContainer.MaxWeight = CurrentInventoryData.MaxWeight;
                InventoryContainer.ResetWeight();
                InventoryContainer.UpdateRender(CurrentInventoryData);
            }
            else if (storageId.StartsWith("player_") && isPlayerInventory)
            {
                CurrentChestData = await LoadChest(storageId);

                ChestContainer = new StorageContainer(this, Character, StorageDataType.Chest);
                ChestContainer.CalculateWeight(CurrentChestData);
                ChestContainer.MaxWeight = CurrentChestData.MaxWeight;
                ChestContainer.ResetWeight();
                ChestContainer.UpdateRender(CurrentChestData, characterData);
            }
            else if (storageId.StartsWith("vehicle_") || storageId.StartsWith("chest_"))
            {
                CurrentChestData = await LoadChest(storageId);

                if (storageId.StartsWith("vehicle_"))
                {
                    ChestContainer = new StorageContainer(this, Character, StorageDataType.VehicleInventory);
                }
                else if (storageId.StartsWith("chest_"))
                {
                    ChestContainer = new StorageContainer(this, Character, StorageDataType.Chest);
                }

                ChestContainer.CalculateWeight(CurrentChestData);
                ChestContainer.MaxWeight = CurrentChestData.MaxWeight;
                ChestContainer.ResetWeight();
                ChestContainer.UpdateRender(CurrentChestData);
            }
        }

        #region Inventory

        public void SaveInventory()
        {
            var itemsData = new List<StorageItemData>();

            foreach (var item in CurrentInventoryData.Items)
            {
                var newItem = new StorageItemData(item.Name, item.Count);
                newItem.Data = item.Data;
                itemsData.Add(newItem);
            }

            var storage = new StorageData(CurrentInventoryData.StorageId, CurrentInventoryData.MaxWeight, itemsData);

            Event.EmitServer("Storage.Save", JsonConvert.SerializeObject(storage));
        }
        
        public void AddInventoryItem(string name, int count, Dictionary<string, object> data = null)
        {
            InventoryContainer.AddItem(name, count, CurrentInventoryData, data);
            SaveInventory();
        }
        
        public void RemoveChestItem(string uniqueId, int count)
        {
            if (ChestContainer != null)
            {
                var chestItem = ChestContainer.GetItemById(uniqueId, CurrentChestData);

                if (chestItem == null)
                {
                    var invItem = InventoryContainer.GetItemById(uniqueId, CurrentInventoryData);
                    if (invItem == null) return;

                    InventoryContainer.RemoveItemById(uniqueId, count, CurrentInventoryData);
                    SaveInventory();
                }
                else
                {
                    ChestContainer.RemoveItemById(uniqueId, count, CurrentChestData);
                    SaveChest();
                }
            }
            else
            {
                var invItem = InventoryContainer.GetItemById(uniqueId, CurrentInventoryData);
                if (invItem == null) return;

                InventoryContainer.RemoveItemById(uniqueId, count, CurrentInventoryData);
                SaveInventory();
            }
        }
        public void RemoveInventoryItemByName(string name, int count)
        {
            InventoryContainer.RemoveItemByName(name, count, CurrentInventoryData);
            SaveInventory();
        }
        public void RemoveInventoryItemById(string uniqueId, int count)
        {
            InventoryContainer.RemoveItemById(uniqueId, count, CurrentInventoryData);
            SaveInventory();
        }
        
        public bool HaveItem(string name) => CurrentInventoryData.Items.Exists(x => x.Name == name);

        public bool HaveItemCount(string name, int count)
        {
            var item = CurrentInventoryData.Items.Find(x => x.Name == name);
            if (item == null) return false;
            if (item.Count >= count)return true;
            return false;
        }
        
        public StorageItemData GetInventoryItemByName(string itemName) => CurrentInventoryData.Items.Find(x => x.Name == itemName);
        public StorageItemData GetChestItemByName(string itemName) => CurrentChestData.Items.Find(x => x.Name == itemName);

        #endregion

        #region Chest

        public void SaveChest()
        {
            var itemsData = new List<StorageItemData>();

            foreach (var item in CurrentChestData.Items)
                itemsData.Add(new StorageItemData(item.Name, item.Count));

            var storage = new StorageData(CurrentChestData.StorageId, CurrentChestData.MaxWeight, itemsData);

            Event.EmitServer("Storage.Save", JsonConvert.SerializeObject(storage));
        }
        
        public void AddChestItem(string name, int count)
        {
            ChestContainer.AddItem(name, count, CurrentChestData);
            SaveChest();
        }

        #endregion

        #region Nui

        [UICallback("window_ready")]
        private CallbackDelegate WindowReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            // Load menu in html page
            SendNUI(new
            {
                eventName = "avg.internal.load",
                plugin = "storage",
                fileName = "index.html"
            });
            return result;
        }

        [UICallback("storage/avg.ready")]
        private CallbackDelegate Ready(IDictionary<string, object> data, CallbackDelegate result)
        {
            _isReady = true;
            return result;
        }

        [UICallback("storage/close")]
        private CallbackDelegate OnClose(IDictionary<string, object> data, CallbackDelegate result)
        {
            Log.Warn("Close");
            Close();
            return result;
        }
        
        [UICallback("storage/inventory/clickContextMenu")]
        private CallbackDelegate OnInventoryClickContextMenu(IDictionary<string, object> data, CallbackDelegate result)
        {
            var name = data["name"].ToString();
            var tempId = data["tempId"].ToString();
            var eventName = data["eventName"].ToString();
            var itemInfo = GetItemInfo(name);

            if (itemInfo == null) return result;
            var context = itemInfo.ContextMenu.GetContext(eventName);
            if (context == null) return result;
            var item = GetItemInStorage(CurrentInventoryData, tempId);
            if (item == null) return result;

            context.Action.Invoke(this, item, GetTarget(PlayerPedId(), 6f));
            return result;
        }
        
        [UICallback("storage/inventory/inputItemCount")]
        private CallbackDelegate OnInventoryInputCount(IDictionary<string, object> data, CallbackDelegate result)
        {
            var val = data["value"].ToString();

            if (!string.IsNullOrEmpty(val))
            {
                decimal decimalInput;
                int integerInput;
                
                decimal.TryParse(val, out decimalInput);
                int.TryParse(val, out integerInput);

                InventoryDecimalInput = decimalInput;
                InventoryIntegerInput = integerInput;
                
                if (InventoryIntegerInput < 1)
                {
                    InventoryIntegerInput = 1;
                }

                if (InventoryDecimalInput <= 0m)
                {
                    InventoryDecimalInput = 0m;
                }
            }

            // Règle un problème d'affichage (l'item affichais la valeur précédente sans raison)
            InventoryContainer.UpdateRender(CurrentInventoryData);

            return result;
        }
        
        [UICallback("storage/chest/clickContextMenu")]
        private CallbackDelegate OnChestClickContextMenu(IDictionary<string, object> data, CallbackDelegate result)
        {
            var name = data["name"].ToString();
            var tempId = data["tempId"].ToString();
            var eventName = data["eventName"].ToString();
            var itemInfo = GetItemInfo(name);

            if (itemInfo == null) return result;
            var context = itemInfo.ContextMenu.GetContext(eventName);
            if (context == null) return result;
            var item = GetItemInStorage(CurrentChestData, tempId);
            if (item == null) return result;

            context.Action.Invoke(this, item, GetTarget(PlayerPedId(), 6f));

            return result;
        }
        
        [UICallback("storage/chest/inputItemCount")]
        private CallbackDelegate OnChestInputCount(IDictionary<string, object> data, CallbackDelegate result)
        {
            var val = data["value"].ToString();

            if (!string.IsNullOrEmpty(val))
            {
                decimal decimalInput;
                int integerInput;
                
                decimal.TryParse(val, out decimalInput);
                int.TryParse(val, out integerInput);

                ChestDecimalInput = decimalInput;
                ChestIntegerInput = integerInput;
                
                if (ChestIntegerInput < 1)
                {
                    ChestIntegerInput = 1;
                }

                if (ChestDecimalInput <= 0m)
                {
                    ChestDecimalInput = 0m;
                }
            }

            // Règle un problème d'affichage (l'item affichais la valeur précédente sans raison)
            ChestContainer.UpdateRender(CurrentChestData);

            return result;
        }

        #endregion

        #region Event

        [ClientEvent("Storage.RemoveItem")]
        private void OnRemoveItemEvent(string itemName, int itemCount)
        {
            RemoveInventoryItemByName(itemName, itemCount);
            InventoryContainer.UpdateRender(CurrentInventoryData);
        }
        
        [ClientEvent("Storage.RemoveItemById")]
        private void OnRemoveItemByIdEvent(string uniqueId, int itemCount)
        {
            RemoveInventoryItemById(uniqueId, itemCount);
            InventoryContainer.UpdateRender(CurrentInventoryData);
        }

        [ClientEvent("Storage.GiveItemToPlayer")]
        private void OnGiveItemToPlayerEvent(string itemJson, int itemCount)
        {
            var item = JsonConvert.DeserializeObject<StorageItemData>(itemJson);
            var itemInfo = GetItemInfo(item.Name);

            if (itemInfo != null)
            {
                item.Count = itemCount;
                AddInventoryItem(item.Name, item.Count, item.Data);
                InventoryContainer.UpdateRender(CurrentInventoryData);
                Notification.Schedule("INVENTAIRE", $"Vous avez reçus {item.Count} {GetItemInfo(item.Name).Text}", 3000);
            }
            else
            {
                Log.Error("L'item que vous avez reçus n'existe pas ou plus: " + item.Name);
            }
        }

        [ClientEvent("Storage.GiveMoneyToPlayer")]
        private void OnGiveMoneyToPlayerEvent(string amount)
        {
            var decAmount = decimal.Parse(amount);
            
            Character.AddMoney(decAmount);
            InventoryContainer.UpdateRender(CurrentInventoryData);
            Notification.Schedule("INVENTAIRE", $"Vous avez reçus ${ConvertDecimalToString(decAmount)}", 3000);
        }

        [ClientEvent("Storage.Updated")]
        private async void OnUpdatedEvent(string storageId)
        {
            if (storageId.StartsWith("chest_") || storageId.StartsWith("vehicle_"))
            {
                if (CurrentChestData == null) return;
                if (CurrentChestData.StorageId == storageId) await Show(storageId);
            }
        }

        #endregion

        #region Rpc

        private void OnGetRegisteredItemsEvent(List<StorageItemInfo> items)
        {
            ItemsInfo = items;
        }

        #endregion
    }
}