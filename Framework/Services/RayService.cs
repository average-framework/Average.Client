using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Ray;
using Average.Shared.Enums;
using Average.Shared.Models;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Average.Client.Framework.Services.InputService;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class RayService : IService
    {
        private readonly UIService _uiService;
        private readonly InputService _inputService;
        private readonly ClientService _clientService;

        private bool _isFocusActive;
        public RayGroup currentGroup;

        private readonly List<RayGroup> _menus = new();
        private readonly RayGroup _mainGroup = new("main");
        public readonly List<RayGroup> histories = new();

        public bool IsOpen { get; set; }

        public Action<RaycastHit> CrossairCondition { get; set; }

        private const int CrossairTransitionDuration = 150;
        private const int CrossairRefreshInterval = 100;

        public RayService(UIService uiService, ClientService clientService, InputService inputService)
        {
            _uiService = uiService;
            _clientService = clientService;
            _inputService = inputService;

            CrossairCondition = DefaultCrossairCondition;

            var testGroup = new RayGroup("testGroup");

            _mainGroup.AddItem(new RayItem("Debug 1", "", false,
                action: (raycast) =>
                {
                    Logger.Debug("Debug 1");
                    Open(testGroup);
                },
                condition: async (raycast) =>
                {
                    return true;
                }));

            testGroup.AddItem(new RayItem("Debug 2", "", true,
                action: (raycast) =>
                {
                    Logger.Debug("Debug 2");
                },
                condition: async (raycast) =>
                {
                    return true;
                }));

            AddGroup(_mainGroup);
            AddGroup(testGroup);

            // Inputs
            _inputService.RegisterKey(new Input((Control)0x8CC9CD42,
            condition: () =>
            {
                return _menus.Count > 0;
            },
            onStateChanged: (state) =>
            {
                Logger.Debug($"Client can open/close N3 menu");
            },
            onKeyReleased: async () =>
            {
                ShowMenu();
                SetVisibility(true);
                ShowGroup();

                _uiService.FocusFrame("ray");
                _uiService.Focus();

                SetCrossairVisibility(true, CrossairTransitionDuration);

                Logger.Debug($"Client open N3 menu");
            }));

            currentGroup = _mainGroup;
            AddHistory(currentGroup);

            Logger.Debug("RayService Initialized successfully");
        }

        #region Thread

        [Thread]
        private async Task CrossairUpdate()
        {
            var currentRaycastTarget = _clientService.GetSharedData<RaycastHit>("Character:CurrentRaycast");
            if (currentRaycastTarget == null) return;

            CrossairCondition.Invoke(currentRaycastTarget);
            await BaseScript.Delay(CrossairRefreshInterval);
        }

        #endregion

        internal void DefaultCrossairCondition(RaycastHit ray)
        {
            if (IsPedInMeleeCombat(PlayerPedId()))
            {
                CloseMenu();
                SetCrossairVisibility(false, CrossairTransitionDuration);
                return;
            }

            if (ray.Hit && ray.EntityType != (int)EntityType.Map)
            {
                if (!_isFocusActive)
                {
                    _isFocusActive = true;

                    ShowMenu();
                    SetVisibility(false);
                    SetCrossairVisibility(true, CrossairTransitionDuration);
                }
            }
            else
            {
                if (_isFocusActive)
                {
                    _isFocusActive = false;

                    ShowMenu();
                    SetVisibility(false);
                    SetCrossairVisibility(false, CrossairTransitionDuration);
                }
            }
        }

        internal void OnClientWindowInitialized()
        {
            _uiService.LoadFrame("ray");
            _uiService.SetZIndex("ray", 60000);
        }

        internal void OnClientInitialized()
        {
            currentGroup = _mainGroup;
            AddHistory(_mainGroup);
            IsOpen = false;
        }

        internal void OnClick(string itemId)
        {
            var raycast = _clientService.GetSharedData<RaycastHit>("Character:CurrentRaycast") ?? new RaycastHit();

            var item = _menus.Find(x => x.Name == currentGroup.Name)[itemId];
            if (item == null) return;

            item.Action?.Invoke(raycast);

            if (item.CloseMenuOnAction)
            {
                OnPrevious();
                CloseMenu();

                _uiService.Unfocus();
            }
        }

        internal RayGroup GetGroup(string groupName)
        {
            return _menus.Find(x => x.Name == groupName);
        }

        internal void AddGroup(RayGroup group)
        {
            if (!_menus.Exists(x => x.Name == group.Name))
            {
                _menus.Add(group);
            }
        }

        private async void ShowGroup()
        {
            var raycast = _clientService.GetSharedData<RaycastHit>("Character:CurrentRaycast") ?? new RaycastHit();
            var items = _menus.Find(x => x.Name == _mainGroup.Name);

            foreach (var item in items)
            {
                var result = await item.Condition(raycast);
                item.IsVisible = result;
            }

            OnRender(items);
        }

        internal async void Open(RayGroup group)
        {
            var raycast = _clientService.GetSharedData<RaycastHit>("Character:CurrentRaycast") ?? new RaycastHit();

            currentGroup = group;

            foreach (var item in group)
            {
                var result = await item.Condition(raycast);
                item.IsVisible = result;
            }

            AddHistory(group);
            OnRender(group);
            SetCrossairVisibility(true, CrossairTransitionDuration);
            ShowMenu();

            if (!IsOpen)
            {
                IsOpen = true;
            }
        }

        internal void OnPrevious()
        {
            if (IsOpen)
            {
                IsOpen = false;
                currentGroup = _mainGroup;

                ClearHistory();
                AddHistory(_mainGroup);

                var raycast = _clientService.GetSharedData<RaycastHit>("Character:CurrentRaycast") ?? new RaycastHit();

                if (raycast.Hit && raycast.EntityType != (int)EntityType.Map)
                {
                    SetVisibility(false);
                    SetCrossairVisibility(true, CrossairTransitionDuration);
                }
                else
                {
                    CloseMenu();
                    SetCrossairVisibility(false, CrossairTransitionDuration);
                }
            }
        }

        private void OnRender(RayGroup group) => _uiService.SendNui("ray", "render", new
        {
            items = group.Items
        });

        internal void AddHistory(RayGroup group)
        {
            if (!histories.Exists(x => x.Name == group.Name))
            {
                histories.Add(group);
            }
        }


        internal void RemoveHistory(RayGroup group)
        {
            if (histories.Exists(x => x.Name == group.Name))
            {
                histories.Remove(group);
            }
        }

        internal void ClearHistory()
        {
            histories.Clear();
        }

        private void SetVisibility(bool isVisible, int fadeDuration = 100) => _uiService.SendNui("ray", "visibility", new
        {
            isVisible,
            fade = fadeDuration
        });

        internal void ShowMenu()
        {
            _uiService.SendNui("ray", "open", new { });
            IsOpen = true;
        }

        internal void CloseMenu()
        {
            _uiService.SendNui("ray", "close", new { });
            IsOpen = false;
        }

        internal void SetCrossairVisibility(bool isVisible, int fadeDuration = 100) => _uiService.SendNui("ray", "crossair", new
        {
            isVisible,
            fade = fadeDuration
        });
    }
}
