using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Average.Client.Models;
using CitizenFX.Core;
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

namespace Average.Client.Controllers
{
    public class CraftController : InternalPlugin, ICraftController
    {
        public StorageContextMenu CraftContextMenu { get; private set; }

        public string Name { get; private set; }
        public bool IsCrafting { get; private set; }

        private bool _lastIsCraftTableNear;
        private bool _isCraftTableNear;

        private const float _cullingRadius = 25f;
        private const uint _key = (uint) Keys.X;
        
        private Craft _crafts;
        
        public override void OnInitialized()
        {
            Name = RandomString();
            
            _crafts = Configuration.Parse<Craft>("configs/recipes.json");
            
            Task.Factory.StartNew(async () =>
            {
                await Character.IsReady();
                
                Thread.StartThread(Update);
                Thread.StartThread(MarkerUpdate);
            });
        }

        #region Command

        [ClientCommand("craft.simulate_open", "owner", 4)]
        private void OpenCommand(string craftTableName)
        {
            var craftTable = GetCraftTable(craftTableName);

            if (craftTable == null)
            {
                Log.Error($"[Craft] Unable to open craft table: {craftTableName}");
                return;
            }
            
            Open(GetCraftTable(craftTableName));
        }

        #endregion

        public void AddTemporaryRecipes(Craft.CraftTable craftTable)
        {
            _crafts.CraftTables.Add(craftTable);
        }

        private async Task MarkerUpdate()
        {
            if (Character.Current == null) return;
            
            var ped = PlayerPedId();
            var pos = GetEntityCoords(ped, true, true);

            if (_crafts.CraftTables.Count > 0)
            {
                for (var i = 0; i < _crafts.CraftTables.Count; i++)
                {
                    var craftTable = _crafts.CraftTables[i];
                    
                    if (craftTable.Jobs.Exists(x => x.Name == Character.Current.Job.Name && x.Role == Character.Current.Job.Role.Name) || craftTable.Jobs.Count == 0)
                    {
                        if (GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, craftTable.Interact.Position.X, craftTable.Interact.Position.Y, craftTable.Interact.Position.Z, true) <= craftTable.Interact.Radius + _cullingRadius)
                        {
                            Call(0x2A32FAA57B937173, (uint)MarkerType.Halo, craftTable.Interact.Position.X,
                                craftTable.Interact.Position.Y, craftTable.Interact.Position.Z - 0.98f, 0, 0, 0, 0, 0, 0,
                                craftTable.Interact.Radius, craftTable.Interact.Radius, 0.2f, 255, 255, 255, 255, 0, 0, 2, 0, 0, 0, 0);
                        }
                    }
                }
            }
            else
            {
                Thread.StopThread(MarkerUpdate);
            }
        }

        private async Task Update()
        {
            if (Character.Current == null) return;
            
            var ped = PlayerPedId();
            var pos = GetEntityCoords(ped, true, true);
            var nearest = _crafts.CraftTables.Find(x =>
                GetDistanceBetweenCoords(pos.X, pos.Y, pos.Z, x.Interact.Position.X, x.Interact.Position.Y,
                    x.Interact.Position.Z, true) <= x.Interact.Radius && x.Jobs.Exists(x => x.Name == Character.Current.Job.Name && x.Role == Character.Current.Job.Role.Name) ? true : x.Jobs.Count == 0);

            if (nearest == null)
            {
                if (_isCraftTableNear)
                {
                    _isCraftTableNear = false;
                }

                await BaseScript.Delay(1000);
            }
            else
            {
                if (!_isCraftTableNear)
                {
                    _isCraftTableNear = true;
                }

                if (IsControlJustReleased(0, _key))
                {
                    Event.Emit("Craft.Open", nearest.Name);
                }
            }

            if (_lastIsCraftTableNear != _isCraftTableNear)
            {
                _lastIsCraftTableNear = _isCraftTableNear;

                if (_lastIsCraftTableNear)
                {
                    Event.Emit("Craft.IsCraftTableNear", true, nearest.Name);
                    Event.Emit("UI.ChangeHudVisibility", true);
                }
                else
                {
                    Event.Emit("Craft.IsCraftTableNear", false, nearest.Name);
                    Event.Emit("UI.ChangeHudVisibility", false);
                }
            }
        }

        public bool IsCraftTableExist(string name) => _crafts.CraftTables.Exists(x => x.Name == name);
        public Craft.CraftTable GetCraftTable(string name) => _crafts.CraftTables.Find(x => x.Name == name);

        public bool IsRecipeExistForItem(string itemName) => _crafts.Recipes.Exists(x => x.GiveItem.Name == itemName);
        public Craft.Recipe GetRecipeByItemName(string itemName) => _crafts.Recipes.Find(x => x.GiveItem.Name == itemName);
        
        public void Open(Craft.CraftTable craftTable = null)
        {
            Storage.Open();
            Storage.InventoryContainer.CalculateWeight(Storage.CurrentInventoryData);
            Storage.InventoryContainer.MaxWeight = Storage.CurrentInventoryData.MaxWeight;
            Storage.InventoryContainer.ResetWeight();

            UpdateRender(craftTable);
        }

        private bool CanBuildRecipe(StorageItemData item)
        {
            var canBuild = true;
            var currentRecipe = _crafts.Recipes.Find(x => x.GiveItem.Name == item.Name);

            foreach (var need in currentRecipe.RequiredItems)
            {
                var it = Storage.GetInventoryItemByName(need.Name);

                if (it != null)
                {
                    if (it.Count < need.Count)
                    {
                        canBuild = false;
                    }
                }
                else
                {
                    canBuild = false;
                }
            }

            return canBuild;
        }

        public void UpdateRender(Craft.CraftTable craftTable)
        {
            var items = new List<object>();

            foreach (var recipe in _crafts.Recipes)
            {
                CraftContextMenu = new StorageContextMenu();

                var contextMenu = new List<object>();
                var info = Storage.GetItemInfo(recipe.GiveItem.Name);
                var item = new StorageItemData(recipe.GiveItem.Name, 1);
                var rs = RandomString();

                foreach (var need in recipe.RequiredItems)
                {
                    var it = Storage.GetInventoryItemByName(need.Name);
                    var inf = Storage.GetItemInfo(need.Name);
                    var rsId = RandomString();

                    CraftContextMenu.Items.Add(new StorageContextItem
                    {
                        Name = need.Name,
                        Emoji = "",
                        Text = $"{inf.Text} [{(it == null ? 0 : it.Count)}/{need.Count}]",
                        EventName = need.Name,
                        Id = rsId,
                        Action = (storage, item, ray) => { Log.Debug("test"); }
                    });

                    contextMenu.Add(new
                    {
                        name = need.Name,
                        id = rsId,
                        text = $"{inf.Text} [{(it == null ? 0 : it.Count)}/{need.Count}]",
                        emoji = "",
                        eventName = need.Name
                    });
                }

                var contextMenuItem = new StorageContextItem
                {
                    Name = recipe.GiveItem.Name,
                    Emoji = "",
                    Text = "Assembler",
                    EventName = "create",
                    Id = rs,
                    Action = async (storage, item, ray) =>
                    {
                        var currentRecipe = _crafts.Recipes.Find(x => x.GiveItem.Name == item.Name);

                        if (CanBuildRecipe(item))
                        {
                            if (!IsCrafting)
                            {
                                var needFreeSpace = currentRecipe.GiveItem.Count * storage.GetItemInfo(currentRecipe.GiveItem.Name).Weight;

                                if (Storage.InventoryContainer.HasFreeSpace(needFreeSpace))
                                {
                                    IsCrafting = true;

                                    Notification.Schedule("ASSEMBLAGE", "Assemblage en cours..", currentRecipe.AssemblyTime * 1000);

                                    await BaseScript.Delay(currentRecipe.AssemblyTime * 1000);

                                    var failChance = new Random(Environment.TickCount).Next(0, 100);
                                    // await BaseScript.Delay(0);

                                    if (CanBuildRecipe(item))
                                    {
                                        if (failChance < currentRecipe.FailChance)
                                        {
                                            // assembly failed                      
                                            var requiredItems = new List<string>();
                                            currentRecipe.RequiredItems.ForEach(x => requiredItems.Add($"{x.Count} {storage.GetItemInfo(x.Name).Text}"));

                                            // foreach (var need in currentRecipe.Needs)
                                            // {
                                            //     if (need.ItemName == "consumable_bottle" || need.ItemName == "consumable_bottle_1_3" || need.ItemName == "consumable_bottle_2_3")
                                            //     {
                                            //         storage.AddInventoryItem("consumable_bottle_empty", need.ItemCount);
                                            //     }
                                            //
                                            //     storage.RemoveInventoryItemByName(need.ItemName, need.ItemCount);
                                            // }

                                            Notification.Schedule("ASSEMBLAGE", $"Echec de l'assemblage, vous avez perdu {string.Join(", ", requiredItems)}.", 5000);
                                        }
                                        else
                                        {
                                            // assembly success
                                            currentRecipe.RequiredItems.ForEach(x => storage.RemoveInventoryItemByName(x.Name, x.Count));
                                            
                                            storage.AddInventoryItem(currentRecipe.GiveItem.Name, currentRecipe.GiveItem.Count, currentRecipe.GiveItem.Data);
                                            Open(craftTable);

                                            Notification.Schedule("ASSEMBLAGE", $"Vous avez confectionner {currentRecipe.GiveItem.Count} {currentRecipe.GiveItem.Name}(s).", 5000);
                                        }
                                    }
                                    else
                                    {
                                        Notification.Schedule("ASSEMBLAGE", "Impossible d'assembler cette recette car il manque certains composants.", 5000);
                                    }

                                    IsCrafting = false;
                                }
                                else
                                {
                                    Notification.Schedule("ASSEMBLAGE", "Vous n'avez pas assez de place dans votre inventaire.", 5000);
                                }
                            }
                            else
                            {
                                Notification.Schedule("ASSEMBLAGE", "Vous ne pouvez crafter qu'un seul item à la fois.", 5000);
                            }
                        }
                        else
                        {
                            Notification.Schedule("ASSEMBLAGE", "Vous ne pouvez pas craft cet item.", 5000);
                        }
                    }
                };

                CraftContextMenu.Items.Add(contextMenuItem);

                contextMenu.Add(new
                {
                    name = recipe.GiveItem.Name,
                    id = rs,
                    text = "Assembler",
                    emoji = "",
                    eventName = "create"
                });

                if (craftTable == null)
                {
                    // Ajoute les items qui n'on pas besoin d'une table de craft
                    var needCraftTable = false;

                    foreach (var table in _crafts.CraftTables)
                    {
                        if (table.Recipes.Exists(x => x == item.Name))
                        {
                            needCraftTable = true;
                        }
                    }

                    if (!needCraftTable)
                    {
                        string text;

                        if (!recipe.GiveItem.Data.ContainsKey("GIVE_AMMO_COUNT"))
                        {
                            text = recipe.GiveItem.Count + " " + info.Text;
                        }
                        else
                        {
                            text = recipe.GiveItem.Data["GIVE_AMMO_COUNT"] + " " + info.Text;
                        }

                        items.Add(new
                        {
                            id = item.UniqueId,
                            text,
                            img = info.Img,
                            menu = contextMenu
                        });
                    }
                }
                else
                {
                    // Ajoute les items qui on besoin d'une table de craft
                    var blockDuplicate = false;

                    foreach (var r in craftTable.Recipes)
                    {
                        if (item.Name == r)
                        {
                            string text;

                            if (!recipe.GiveItem.Data.ContainsKey("GIVE_AMMO_COUNT"))
                            {
                                text = recipe.GiveItem.Count + " " + info.Text;
                            }
                            else
                            {
                                text = recipe.GiveItem.Data["GIVE_AMMO_COUNT"] + " " + info.Text;
                            }

                            items.Add(new
                            {
                                id = item.UniqueId,
                                text,
                                img = info.Img,
                                menu = contextMenu
                            });
                        }
                        else
                        {
                            var needCraftTable = false;

                            foreach (var table in _crafts.CraftTables)
                            {
                                if (table.Recipes.Exists(x => x == item.Name))
                                {
                                    needCraftTable = true;
                                }
                            }

                            if (!blockDuplicate)
                            {
                                blockDuplicate = true;

                                if (!needCraftTable)
                                {
                                    string text;

                                    if (!recipe.GiveItem.Data.ContainsKey("GIVE_AMMO_COUNT"))
                                    {
                                        text = recipe.GiveItem.Count + " " + info.Text;
                                    }
                                    else
                                    {
                                        text = recipe.GiveItem.Data["GIVE_AMMO_COUNT"] + " " + info.Text;
                                    }

                                    items.Add(new
                                    {
                                        id = item.UniqueId,
                                        text,
                                        img = info.Img,
                                        menu = contextMenu
                                    });
                                }
                            }
                        }
                    }
                }
            }
            
            SendNUI(new
            {
                eventName = "avg.internal",
                on = "craft.updateRender",
                plugin = "storage",
                items
            });
        }

        #region Nui

        [UICallback("storage/craft/clickContextMenu")]
        private CallbackDelegate OnCraftClickContextMenu(IDictionary<string, object> data, CallbackDelegate result)
        {
            var name = data["name"].ToString();
            var eventName = data["eventName"].ToString();

            var context = CraftContextMenu.GetContext(eventName);

            if (context == null) return result;

            context.Action.Invoke(Storage, new StorageItemData(name, 0), null);

            return result;
        }

        #endregion
    }
}