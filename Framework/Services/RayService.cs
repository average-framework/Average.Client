using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Ray;
using Average.Shared.Enums;
using CitizenFX.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class RayService : IService
    {
        private RaycastHit _currentRay;
        private bool _isFocusActive;

        private string _currentGroup;
        private int _lastEntityHit = 0;

        private readonly List<string> _containerHistories = new();

        public bool CanCloseMenu { get; set; } = true;
        public bool IsOpen { get; private set; }
        public float TargetRange => 6f;

        public Action<RaycastHit, bool> CrossairCondition { get; set; }

        private const int CrossairTransitionDuration = 150;

        private readonly UIService _uiService;

        private readonly RayGroupList _rayGroupList = new();

        public RayService(UIService uiService)
        {
            _uiService = uiService;

            _currentGroup = "main";

            AddGroupToHistory(_currentGroup);
            CrossairCondition = DefaultCrossairCondition;

            Logger.Debug("RayService Initialized successfully");
        }

        #region UI Callbacks

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            Logger.Error("OnWindowReady");

            // Load menu in html page
            _uiService.LoadFrame("ray");
            _uiService.SetZIndex("ray", 80000);

            return result;
        }

        [UICallback("frame_ready")]
        private CallbackDelegate OnFrameReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (data.TryGetValue("frame", out var frame))
            {
                if ((string)frame == "ray")
                {
                    Logger.Error("OnFrameReady");
                }
            }

            return result;
        }

        [UICallback("ray/on_click")]
        private CallbackDelegate OnClickMenu(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (data.TryGetValue("id", out var id))
            {
                var item = _rayGroupList[_currentGroup][id.ToString()];
                if (item == null) return result;

                item.Action?.Invoke(_currentRay);

                if (item.CloseMenuOnAction)
                {
                    CloseMenu();
                    Unfocus();
                }
            }

            return result;
        }

        [UICallback("ray/keydown")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (data.TryGetValue("key", out var key))
            {
                // Escape key
                if ((int)key == 27)
                {
                    if (IsOpen)
                    {
                        if (_containerHistories.Count > 0)
                        {
                            var currentGroupIndex = _containerHistories.IndexOf(_currentGroup);

                            if (currentGroupIndex > 0)
                            {
                                var parent = _containerHistories[currentGroupIndex - 1];

                                Open(parent);
                                _containerHistories.RemoveAt(currentGroupIndex);
                            }
                            else
                            {
                                OnPrevious();
                                Unfocus();
                            }
                        }
                        else
                        {
                            if (CanCloseMenu)
                            {
                                OnPrevious();
                                Unfocus();
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Thread

        [Thread]
        private async Task CrossairUpdate()
        {
            _currentRay = GetTarget(PlayerPedId(), TargetRange);
            CrossairCondition.Invoke(_currentRay, _lastEntityHit != _currentRay.EntityHit);
            await BaseScript.Delay(100);
        }

        [Thread]
        private async Task KeyboardUpdate()
        {
            if (IsControlJustReleased(0, 0x8CC9CD42) && _rayGroupList.Count > 0)
            {
                ShowMenu();
                SetVisibility(true);
                await ShowGroup();
                _uiService.FocusFrame("ray");
                _uiService.Focus();
            }
        }

        #endregion

        internal void DefaultCrossairCondition(RaycastHit ray, bool state)
        {
            if (IsPedInMeleeCombat(PlayerPedId()))
            {
                CloseMenu();
                SetCrossairVisibility(false, CrossairTransitionDuration);
                return;
            }

            if (ray.Hit && ray.EntityType != (int)EntityType.Map && state)
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

        private void SetVisibility(bool isVisible, int fadeDuration = 100) => _uiService.SendNui("ray", "visibility", new
        {
            isVisible,
            fade = fadeDuration
        });

        internal RayGroup Group(string groupName)
        {
            if (!_rayGroupList.GroupExists(groupName))
            {
                _rayGroupList.Add(new RayGroup(groupName));
            }

            return _rayGroupList[groupName];
        }

        private async Task ShowGroup()
        {
            var items = _rayGroupList["main"];

            foreach (var item in items)
            {
                var result = await item.Condition(_currentRay);

                item.IsVisible = result;
                Logger.Debug($"Draw item: {item.Text}: {result}, {item.IsVisible}");
            }

            OnRender(items);
        }

        internal async void Open(string groupName)
        {
            _currentGroup = groupName;

            var group = _rayGroupList[groupName];

            foreach (var item in group)
            {
                var result = await item.Condition(_currentRay);

                item.IsVisible = result;
            }

            AddGroupToHistory(_currentGroup);
            OnRender(group);
            SetCrossairVisibility(true, CrossairTransitionDuration);
            ShowMenu();

            IsOpen = true;
        }

        private void OnPrevious()
        {
            if (IsOpen)
            {
                IsOpen = false;

                _currentGroup = "main";

                ClearHistory();
                AddGroupToHistory(_currentGroup);

                if (_currentRay.Hit && _currentRay.EntityType != (int)EntityType.Map)
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

        internal void ClearHistory() => _containerHistories.Clear();

        internal bool Exists(string groupName) => _containerHistories.Exists(x => x == groupName);

        private void AddGroupToHistory(string groupName)
        {
            if (!Exists(groupName))
            {
                _containerHistories.Add(groupName);
            }
        }

        private void RemoveGroupInHistory(string groupName)
        {
            if (Exists(groupName))
            {
                _containerHistories.Remove(groupName);
            }
        }
    }
}
