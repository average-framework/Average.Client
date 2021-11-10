using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Average.Shared.DataModels;

namespace Average.Client.Commands
{
    internal class InventoryCommand : ICommand
    {
        private readonly InventoryService _inventoryService;

        public InventoryCommand(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // DEBUG ------------------------------------
        [ClientCommand("storage:open_chest")]
        private async void OpenChest()
        {
            var chestId = "enterprise_bill&joe";

            var storage = await _inventoryService.Get(chestId);
            if (storage == null) return;

            _inventoryService.LoadInventory(storage);
            _inventoryService.Open();
            _inventoryService.OpenChest(storage);
        }
        // ------------------------------------------

        [ClientCommand("storage:add_item")]
        private void AddItem(string itemName, int itemCount)
        {
            var storage = _inventoryService.Inventory;

            Logger.Error($"Try to add item: {itemName} count: {itemCount}");

            _inventoryService.AddItem(new StorageItemData(itemName, itemCount), storage);
        }

        [ClientCommand("storage:remove_item_on_slot")]
        private void RemoveItemOnSlot(int slotId)
        {
            var storage = _inventoryService.Inventory;

            _inventoryService.RemoveItemOnSlot(storage, slotId);
        }


        [ClientCommand("giveitem")]
        private void GiveItem(int playerId, string itemName, int itemCount)
        {
            _inventoryService.GiveItem(playerId, itemName, itemCount);
        }
    }
}
