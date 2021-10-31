using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Storage;
using CitizenFX.Core;
using System.Threading.Tasks;
using static Average.Client.Framework.Storage.StorageItemInfo;

namespace Average.Client.Framework.Services
{
    internal class InventoryItemsService : IService
    {
        private readonly InventoryService _inventoryService;

        public InventoryItemsService(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;

            Task.Factory.StartNew(async () =>
            {
                Logger.Error("Waiting for registering items..");

                while (_inventoryService.Items == null)
                {
                    await BaseScript.Delay(1000);
                }

                Logger.Error("Try to register items..");

                RegisterItems();

                Logger.Error("Items registered");
            });

        }

        private void RegisterItems()
        {
            _inventoryService.SetItemInfo("money", GetMoneyItemInfo());
            _inventoryService.SetItemInfo("apple", GetAppleItemInfo());
        }

        #region Registered Items

        private StorageItemInfo GetMoneyItemInfo() => new()
        {
            SplitValueType = typeof(decimal),
            OnStacking = (source, destination) =>
            {
                var cash = decimal.Parse(source.Data["cash"].ToString());
                var destCash = decimal.Parse(destination.Data["cash"].ToString());
                destination.Data["cash"] = cash + destCash;
            },
            OnRenderStacking = (item) =>
            {
                return "$" + item.Data["cash"];
            },
            OnSplit = (item, splitValue, splitType) =>
            {
                var cash = (decimal)item.Data["cash"];

                switch (splitType)
                {
                    case SplitType.BaseItem:
                        cash -= (decimal)splitValue;
                        item.Data["cash"] = cash;
                        break;
                    case SplitType.TargetItem:
                        item.Data["cash"] = splitValue;
                        break;
                }
            },
            SplitCondition = (item) =>
            {
                return (decimal)item.Data["cash"] != 1;
            },
            OnStackCombine = (source, destination) =>
            {
                var cash = decimal.Parse(source.Data["cash"].ToString());
                var destCash = decimal.Parse(destination.Data["cash"].ToString());
                destination.Data["cash"] = cash + destCash;
            },
            ContextMenu = GetMoneyContextMenu()
        };

        private StorageItemInfo GetAppleItemInfo() => new()
        {
            SplitValueType = typeof(int),
            ContextMenu = GetAppleContextMenu()
        };

        #endregion

        #region Context Menu

        internal StorageContextMenu GetMoneyContextMenu() => new(new StorageContextItem
        {
            EventName = "drop",
            Emoji = "",
            Text = "Jeter",
            Action = (storageData, itemData, raycast) =>
            {
                Logger.Debug("item: " + itemData.Name + ", " + raycast.EntityHit);
            }
        },
        GetMoneySplitContextItem());

        internal StorageContextMenu GetAppleContextMenu() => new(new StorageContextItem
        {
            EventName = "drop",
            Emoji = "",
            Text = "Jeter",
            Action = (storageData, itemData, raycast) =>
            {
                Logger.Debug("item: " + itemData.Name + ", " + raycast.EntityHit);
            }
        },
        GetDefaultSplitContextItem());

        #endregion

        #region Context Items

        internal StorageContextItem GetMoneySplitContextItem() => new()
        {
            EventName = "split",
            Emoji = "✂️",
            Text = "Séparer",
            Action = (storageData, itemData, raycast) =>
            {
                Logger.Debug("Split decimal item: " + itemData.Name);

                var info = _inventoryService.GetItemInfo(itemData.Name);
                var minValue = 1m;
                var maxValue = decimal.Parse(itemData.Data["cash"].ToString());

                _inventoryService.ShowSplitMenu(storageData.Type, info, itemData.SlotId, minValue, maxValue, minValue);
            }
        };

        internal StorageContextItem GetDefaultSplitContextItem() => new()
        {
            EventName = "split",
            Emoji = "✂️",
            Text = "Séparer",
            Action = (storageData, itemData, raycast) =>
            {
                Logger.Debug("Split int item: " + itemData.Name);

                var info = _inventoryService.GetItemInfo(itemData.Name);
                var minValue = 1;
                var maxValue = itemData.Count;

                _inventoryService.ShowSplitMenu(storageData.Type, info, itemData.SlotId, minValue, maxValue, minValue);
            }
        };

        #endregion
    }
}
