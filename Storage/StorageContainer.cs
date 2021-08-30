using System.Collections.Generic;
using System.ComponentModel;
using Average.Client.Managers;
using SDK.Client.Diagnostics;
using SDK.Shared.DataModels;
using static SDK.Client.GameAPI;

namespace Average.Client.Storage
{
    public class StorageContainer
    {
        public string Name { get; }
        public double Weight { get; private set; }
        public double MaxWeight { get; set; }
        public StorageDataType StorageType { get; }

        private readonly CharacterManager _character;
        private readonly StorageManager _storage;

        public StorageContainer(StorageManager storage, CharacterManager character, StorageDataType storageType)
        {
            _character = character;
            _storage = storage;

            Name = RandomString();
            StorageType = storageType;
        }

        public double CalculateWeight(StorageData data)
        {
            var weight = 0d;
            data.Items.ForEach(x => weight += _storage.GetItemInfo(x.Name).Weight * x.Count);
            Weight = weight;
            return weight;
        }

        public bool HasFreeSpace(double itemWeight) => Weight + itemWeight <= MaxWeight;
        
        public bool HasFreeSpace(StorageData data) => CalculateWeight(data) <= MaxWeight;

        public bool ItemExistById(string uniqueId, StorageData data) => data.Items.Exists(x => x.UniqueId == uniqueId);
        
        public bool ItemExistByName(string name, StorageData data) => data.Items.Exists(x => x.Name == name);

        public StorageItemData? GetItemById(string uniqueId, StorageData data) => data.Items.Find(x => x.UniqueId == uniqueId);
        
        public StorageItemData? GetItemByName(string name, StorageData data) => data.Items.Find(x => x.Name == name);
        
        public void ResetWeight() => Weight = 0;

        public void AddItem(string name, int count, StorageData storageData, Dictionary<string, object> data = null)
        {
            var info = _storage.GetItemInfo(name);
            var newItem = new StorageItemData(name, 1, new Dictionary<string, object>());
            var weight = info.Weight * count;

            newItem.Data = data ?? new Dictionary<string, object>();

            if (!info.Stackable)
                newItem.Count = 1;

            if (!ItemExistByName(newItem.Name, storageData))
            {
                if (HasFreeSpace(weight))
                {
                    newItem.Count = count;
                    storageData.Items.Add(newItem);

                    Weight = CalculateWeight(storageData);
                    UpdateRender(storageData);
                }
                else
                {
                    Log.Debug("[Storage] Vous n'avez plus de place.s");
                }
            }
            else
            {
                if (HasFreeSpace(weight))
                {
                    if (!info.Stackable)
                    {
                        storageData.Items.Add(newItem);
                    }
                    else
                    {
                        storageData.Items.Find(x => x.Name == name).Count += count;
                        newItem.Count = storageData.Items.Find(x => x.Name == name).Count;
                    }
                }
                else
                {
                    Log.Debug("[Storage] Vous n'avez plus de place.");

                    // Corrige un problème d'affichage
                    newItem.Count = storageData.Items.Find(x => x.Name == newItem.Name).Count;
                }

                Weight = CalculateWeight(storageData);
                UpdateRender(storageData);
            }
        }

        public void RemoveItemByName(string name, int count, StorageData storage)
        {
            if (ItemExistByName(name, storage))
            {
                var item = storage.Items.Find(x => x.Name == name);

                if (item.Count - count >= 0)
                {
                    var newCount = item.Count - count;
                    item.Count = newCount;

                    if (item.Count == 0)
                    {
                        storage.Items.RemoveAll(x => x.Name == name);
                    }

                    Weight = CalculateWeight(storage);
                    UpdateRender(storage);
                }
            }
            else
            {
                Log.Debug("[Storage] Cet item n'éxiste pas.");
            }
        }

        public void RemoveItemById(string uniqueId, int count, StorageData storage)
        {
            if (ItemExistById(uniqueId, storage))
            {
                var item = storage.Items.Find(x => x.UniqueId == uniqueId);

                if (item.Count - count >= 0)
                {
                    item.Count -= count;

                    if (item.Count == 0)
                    {
                        storage.Items.RemoveAll(x => x.UniqueId == uniqueId);
                    }

                    Weight = CalculateWeight(storage);
                    UpdateRender(storage);
                }
            }
            else
            {
                Log.Debug($"[Storage] Cet item n'éxiste pas: {uniqueId}");
            }
        }

        public void UpdateRender(StorageData storage, CharacterData characterData = null)
        {
            var items = new List<object>();

            foreach (var item in storage.Items)
            {
                var contextMenu = new List<object>();
                var info = _storage.GetItemInfo(item.Name);

                if (info.Name == "money")
                {
                    if (characterData == null)
                    {
                        info.Text = "$" + ConvertDecimalToString(_character.Current.Economy.Money);
                    }
                    else
                    {
                        info.Text = "$" + ConvertDecimalToString(characterData.Economy.Money);
                    }
                }

                if (info.ContextMenu != null)
                {
                    foreach (var c in info.ContextMenu.Items)
                    {
                        c.Id = RandomString();

                        contextMenu.Add(new
                        {
                            name = c.Name,
                            id = c.Id,
                            text = c.Text,
                            emoji = c.Emoji,
                            eventName = c.EventName
                        });
                    }
                }

                items.Add(new
                {
                    id = item.UniqueId,
                    text = info.Text,
                    img = info.Img,
                    count = item.Count,
                    menu = contextMenu
                });
            }

            var weight = CalculateWeight(storage);
            var maxWeight = storage.MaxWeight;

            switch (StorageType)
            {
                case StorageDataType.PlayerInventory:
                    SendNUI(new
                    {
                        eventName = "avg.internal",
                        on = "inventory.updateRender",
                        plugin = "storage",
                        weight = weight.ToString("0.00"),
                        maxWeight,
                        items
                    });
                    break;
                case StorageDataType.VehicleInventory:
                    SendNUI(new
                    {
                        eventName = "avg.internal",
                        on = "chest.updateRender",
                        plugin = "storage",
                        weight = weight.ToString("0.00"),
                        maxWeight,
                        items
                    });
                    break;
                case StorageDataType.Chest:
                    SendNUI(new
                    {
                        eventName = "avg.internal",
                        on = "chest.updateRender",
                        plugin = "storage",
                        weight = weight.ToString("0.00"),
                        maxWeight,
                        items
                    });
                    break;
            }
        }
    }
}
