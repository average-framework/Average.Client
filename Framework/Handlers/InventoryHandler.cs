using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Client.Framework.Storage;
using Average.Shared.Attributes;
using Average.Shared.DataModels;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Average.Client.Framework.Handlers
{
    internal class InventoryHandler : IHandler
    {
        private readonly InventoryService _inventoryService;
        private readonly WorldService _worldService;

        public InventoryHandler(InventoryService inventoryService, WorldService worldService)
        {
            _inventoryService = inventoryService;
            _worldService = worldService;
        }

        [ClientEvent("inventory:init")]
        private void OnInventoryInit(string itemsJson, string storageDataJson)
        {
            var items = itemsJson.Deserialize<List<StorageItemInfo>>();
            var storageData = storageDataJson.Deserialize<StorageData>();

            _inventoryService.Items = items;
            _inventoryService.Inventory = storageData;
        }

        [ClientEvent("inventory:giveitem")]
        private void OnGiveItem(int playerId, string itemName, int itemCount)
        {
            _inventoryService.GiveItem(playerId, itemName, itemCount);
        }

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> args, CallbackDelegate cb)
        {
            _inventoryService.OnClientWindowInitialized();

            return cb;
        }

        [UICallback("frame_ready")]
        private CallbackDelegate OnFrameReady(IDictionary<string, object> args, CallbackDelegate cb)
        {
            if (args.TryGetValue("frame", out object frame))
            {
                if ((string)frame == "storage")
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await BaseScript.Delay(1000);

                        _inventoryService.InitSlots(StorageDataType.Player, InventoryService.InventorySlotCount);
                        _inventoryService.InitSlots(StorageDataType.Chest, InventoryService.ChestSlotCount);

                        await BaseScript.Delay(100);

                        while (_inventoryService.Inventory == null)
                        {
                            await BaseScript.Delay(250);
                        }

                        _inventoryService.LoadInventory(_inventoryService.Inventory);
                        _inventoryService.SetTime(_worldService.Time);
                    });
                }
            }

            return cb;
        }

        [UICallback("storage/item_info")]
        private CallbackDelegate OnItemInfo(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var slotId = int.Parse(args["slotId"].ToString());
            var storage = _inventoryService.Inventory;
            if (storage == null) return cb;

            var item = _inventoryService.GetItemOnSlot(slotId, storage);
            if (item == null) return cb;

            var info = _inventoryService.GetItemInfo(item.Name);

            cb(new
            {
                title = info.Title,
                description = info.Description,
                weight = (info.Weight * item.Count).ToString("0.00"),
                isSellable = info.IsSellable ? "Vendable" : "Invendable"
            });

            return cb;
        }

        [UICallback("storage/drop_slot")]
        private CallbackDelegate OnDropSlot(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var slotId = int.Parse(args["slotId"].ToString());
            var targetSlotId = int.Parse(args["targetSlotId"].ToString());
            var slotSourceType = (string)args["slotSourceType"];
            var slotTargetType = (string)args["slotTargetType"];

            Logger.Error("Drop slot target 1: " + string.Join(", ", slotId, slotSourceType, targetSlotId, slotTargetType));

            StorageData sourceStorage = null;
            StorageData destinationStorage = null;

            switch (slotSourceType)
            {
                case "inv":
                    sourceStorage = _inventoryService.Inventory;
                    break;
                case "chest":
                    sourceStorage = _inventoryService.CurrentChest;
                    break;
            }

            switch (slotTargetType)
            {
                case "inv":
                    destinationStorage = _inventoryService.Inventory;
                    break;
                case "chest":
                    destinationStorage = _inventoryService.CurrentChest;
                    break;
            }

            if (sourceStorage == null) return cb;
            if (destinationStorage == null) return cb;

            _inventoryService.MoveItemOnStorage(slotId, targetSlotId, sourceStorage, destinationStorage);

            return cb;
        }

        [UICallback("storage/keydown")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var key = int.Parse((string)args["key"]);

            if (_inventoryService.IsOpen && key == 27)
            {
                _inventoryService.Close();
            }

            return cb;
        }

        [UICallback("storage/close")]
        private CallbackDelegate OnClose(IDictionary<string, object> args, CallbackDelegate cb)
        {
            Logger.Error("storage:close triggered: " + string.Join(", ", args));
            _inventoryService.Close();

            return cb;
        }

        [UICallback("storage/context_menu")]
        private CallbackDelegate OnContextMenu(IDictionary<string, object> args, CallbackDelegate cb)
        {
            Logger.Error("Item click");

            Task.Factory.StartNew(async () =>
            {
                Logger.Error("storage/context_menu triggered: " + string.Join(", ", args));

                var slotId = int.Parse(args["slotId"].ToString());
                var slotSourceType = (string)args["slotSourceType"];
                var eventName = (string)args["eventName"];

                if (slotSourceType == "inv")
                {
                    var storage = _inventoryService.Inventory;
                    if (storage == null) return;

                    var item = _inventoryService.GetItemOnSlot(slotId, storage);
                    if (item == null) return;

                    await _inventoryService.OnStorageContextMenu(item.Name, slotId, eventName, storage);
                }
                else if (slotSourceType == "chest")
                {
                    var storage = _inventoryService.CurrentChest;
                    if (storage == null) return;

                    var item = _inventoryService.GetItemOnSlot(slotId, storage);
                    if (item == null) return;

                    await _inventoryService.OnStorageContextMenu(item.Name, slotId, eventName, storage);
                }
            });

            return cb;
        }

        [UICallback("storage/split/result")]
        private CallbackDelegate OnSplitResult(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var slotId = int.Parse(args["slotId"].ToString());
            var minValue = args["minValue"];
            var maxValue = args["maxValue"];
            var value = args["value"];
            var slotType = (string)args["slotType"];

            if (slotType == "inv")
            {
                var storage = _inventoryService.Inventory;
                if (storage == null) return cb;

                _inventoryService.OnSplitItem(slotId, minValue, maxValue, value, storage);
            }
            else if (slotType == "chest")
            {
                var storage = _inventoryService.CurrentChest;
                if (storage == null) return cb;

                _inventoryService.OnSplitItem(slotId, minValue, maxValue, value, storage);
            }

            Logger.Debug("Split result: " + slotId + ", " + minValue + ", " + maxValue + ", " + value);

            return cb;
        }

        //[UICallback("storage/inv/split/close")]
        //private void OnInventorySplitMenuClosed(Client client, Dictionary<string, object> args, RpcCallback cb)
        //{

        //}

        //[UICallback("storage/veh/split/close")]
        //private void OnVehicleSplitMenuClosed(Client client, Dictionary<string, object> args, RpcCallback cb)
        //{

        //}

        //[UICallback("storage/chest/split/close")]
        //private void OnChestSplitMenuClosed(Client client, Dictionary<string, object> args, RpcCallback cb)
        //{

        //}
    }
}
