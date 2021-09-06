using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using Client.Core.Enums;
using SDK.Client.Diagnostics;
using SDK.Client.Interfaces;
using SDK.Client.RayMenu;
using SDK.Client.Utils;
using SDK.Shared;
using static CitizenFX.Core.Native.API;
using static SDK.Client.GameAPI;

namespace Average.Client.Managers
{
    public class RayMenuManager : InternalPlugin, IRayMenuManager
    {
        private RaycastHit _currentRay;
        private bool _isFocusActive;
        private bool _isReady;
        
        private string _currentGroup;
        private int _lastEntityHit = 0;

        private Dictionary<string, List<RayItem>> _itemGroups = new(); 
        
        private List<string> _containerHistories = new();
        
        public bool CanCloseMenu { get; set; } = true;
        public bool IsOpen { get; private set; }
        public float TargetRange => 6f;
        
        public Action<RaycastHit, bool> CrossairCondition { get; set; }
        
        private const int CrossairTransitionDuration = 150;
        
        public override void OnInitialized()
        {
            RayMenu.CreateGroup("main");
            _currentGroup = "main";
            AddGroupToHistory(_currentGroup);
            
            EventManager.RegisterInternalNuiCallbackEvent("window_ready", WindowReady);
            EventManager.RegisterInternalNuiCallbackEvent("raymenu/avg.ready", Ready);

            CrossairCondition = DefaultCrossairCondition;
            
            Task.Factory.StartNew(async () =>
            {
                await User.IsReady();
                await Character.IsReady();

                Thread.StartThread(CrossairUpdate);
                Thread.StartThread(KeyboardUpdate);
            });
        }

        public void DefaultCrossairCondition(RaycastHit ray, bool state)
        {
            if (IsPedInMeleeCombat(PlayerPedId()))
            {
                HideMenu();
                SetCrossairVisibility(CrossairTransitionDuration, false);
                return;
            }
            
            if (ray.Hit && ray.EntityType != (int)EntityType.Map && state)
            {
                if (!_isFocusActive)
                {
                    _isFocusActive = true;
                    ShowMenu();
                    SetContainerVisibility(false);
                    SetCrossairVisibility(CrossairTransitionDuration, true);
                }
            }
            else
            {
                if (_isFocusActive)
                {
                    _isFocusActive = false;
                    ShowMenu();
                    SetContainerVisibility(false);
                    SetCrossairVisibility(CrossairTransitionDuration, false);
                }
            }
        }

        private void SetContainerVisibility(bool visible) =>  SendNUI(new
        {
            eventName = "avg.internal",
            on = "raymenu.container",
            plugin = "raymenu",
            visible
        });
        
        #region Thread

        private async Task CrossairUpdate()
        {
            _currentRay = GetTarget(PlayerPedId(), TargetRange);
            CrossairCondition.Invoke(_currentRay, _lastEntityHit != _currentRay.EntityHit);
            await BaseScript.Delay(100);
        }

        private async Task KeyboardUpdate()
        {
            if (IsControlJustReleased(0, (uint)Keys.X))
            {
                ShowMenu();
                SetContainerVisibility(true);
                await ShowItemsByConditions();
                Focus();
            }
        }

        #endregion
        
        public void CreateGroup(string groupName)
        {
            if (!GroupExist(groupName))
            {
                _itemGroups.Add(groupName, new());
            }
            else
            {
                Log.Error($"[RayMenu] Unable to create group: {groupName} because an another group already exist with this name.");
            }
        }

        public void RemoveGroup(string groupName)
        {
            if (GroupExist(groupName))
            {
                _itemGroups.Remove(groupName);
            }
        }

        public bool GroupExist(string groupName)
        {
            return _itemGroups.ContainsKey(groupName);
        }
        
        public List<RayItem> GetGroupItems(string groupName)
        {
            if (GroupExist(groupName))
            {
                return _itemGroups[groupName];
            }

            throw new Exception($"[RayMenu] Unable to get items in group: {groupName}");
        }

        public RayItem GetItemInGroup(string groupName, string itemId)
        {
            var items = GetGroupItems(groupName);
            var item = items.Find(x => x.Id == itemId);
            return item;
        }

        public void AddItemInGroup(string groupName, RayItem item)
        {
            var items = GetGroupItems(groupName);
            items.Add(item);
        }

        public void RemoveItemInGroup(string groupName, RayItem item)
        {
            var items = GetGroupItems(groupName);
            items.Remove(item);
        }

        private async Task ShowItemsByConditions()
        {
            var items = GetGroupItems("main");
            
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var result = await item.Condition(_currentRay);
                item.IsVisible = result;
                Log.Debug($"Draw item: {item.Text}: {result}, {item.IsVisible}");
            }

            UpdateRender(items);
        }
        
        public async void OpenMenu(string groupName)
        {
            _currentGroup = groupName;
            
            var items = GetGroupItems(groupName);
            
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var result = await item.Condition(_currentRay);
                item.IsVisible = result;
            }

            AddGroupToHistory(_currentGroup);
            UpdateRender(items);
            SetCrossairVisibility(CrossairTransitionDuration, true);
            ShowMenu();
            Focus();

            IsOpen = true;
        }

        public void CloseMenu()
        {
            if (IsOpen)
            {
                IsOpen = false;

                _currentGroup = "main";
                
                ClearHistory();
                Unfocus();
                
                AddGroupToHistory(_currentGroup);

                if (_currentRay.Hit && _currentRay.EntityType != (int) EntityType.Map)
                {
                    Log.Warn("visible crossair");
                    SetContainerVisibility(false);
                    SetCrossairVisibility(CrossairTransitionDuration, true);
                }
                else
                {
                    HideMenu();
                    SetCrossairVisibility(CrossairTransitionDuration,  false);
                }
            }
        }

        private void UpdateRender(List<RayItem> items) => SendNUI(new
        {
            eventName = "avg.internal",
            on = "raymenu.update_render",
            plugin = "raymenu",
            items
        });

        public void ShowMenu()
        {
            SendNUI(new
            {
                eventName = "avg.internal",
                on = "raymenu.open",
                plugin = "raymenu",
            });
            
            IsOpen = true;
        }

        public void HideMenu()
        {
            SendNUI(new
            {
                eventName = "avg.internal",
                on = "raymenu.close",
                plugin = "raymenu",
            });
        }
        
        public void SetCrossairVisibility(int fadeDuration, bool enabled) => SendNUI(new
        {
            eventName = "avg.internal",
            on = "raymenu.crossair",
            plugin = "raymenu",
            enabled,
            fade = fadeDuration
        });

        public void ClearHistory() => _containerHistories.Clear();

        public bool Exist(string groupName) => _containerHistories.Exists(x => x == groupName);

        public void AddGroupToHistory(string groupName)
        {
            if (!Exist(groupName))
                _containerHistories.Add(groupName);
        }

        public void RemoveGroupInHistory(string groupName)
        {
            if (Exist(groupName))
                _containerHistories.Remove(groupName);
        }
        
        #region NUI Callback

        private CallbackDelegate WindowReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            // Load menu in html page
            SendNUI(new
            {
                eventName = "avg.internal.load",
                plugin = "raymenu",
                fileName = "index.html",
                zIndex = 80000
            });
            return result;
        }

        private CallbackDelegate Ready(IDictionary<string, object> data, CallbackDelegate result)
        {
            _isReady = true;
            return result;
        }

        [UICallback("raymenu/click_context")]
        private CallbackDelegate OnContextMenu(IDictionary<string, object> data, CallbackDelegate result)
        {
            var id = data["id"].ToString();
            var item = GetItemInGroup(_currentGroup, id);

            if (item == null) return result;
            
            item.Action?.Invoke(_currentRay);
            
            if (item.CloseMenuOnAction)
                CloseMenu();

            return result;
        }

        [UICallback("raymenu/on_previous")]
        private CallbackDelegate OnPrevious(IDictionary<string, object> data, CallbackDelegate result)
        {
            var key = int.Parse(data["key"].ToString());

            // escape key
            if (key == 27)
            {
                if (IsOpen)
                {
                    if (_containerHistories.Count > 0)
                    {
                        var currentGroupIndex = _containerHistories.IndexOf(_currentGroup);

                        if (currentGroupIndex > 0)
                        {
                            var parent = _containerHistories[currentGroupIndex - 1];

                            OpenMenu(parent);
                            _containerHistories.RemoveAt(currentGroupIndex);   
                        }
                        else
                        {
                            CloseMenu();
                            Unfocus();
                        }
                    }
                    else
                    {
                        if (CanCloseMenu)
                        {
                            CloseMenu();
                            Unfocus();
                        }
                    }
                }   
            }

            return result;
        }
        
        #endregion
    }
}