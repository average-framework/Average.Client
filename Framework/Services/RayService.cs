using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Shared.Enums;
using Average.Shared.Models;
using CitizenFX.Core;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class RayService : IService
    {
        private readonly UIService _uiService;
        private readonly ClientService _clientService;

        private bool _isFocusActive;

        public Action<RaycastHit> CrossairCondition { get; set; }

        private const int CrossairTransitionDuration = 150;
        private const int CrossairRefreshInterval = 100;

        public RayService(UIService uiService, ClientService clientService)
        {
            _uiService = uiService;
            _clientService = clientService;

            CrossairCondition = DefaultCrossairCondition;

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

        private void SetVisibility(bool isVisible, int fadeDuration = 100) => _uiService.SendNui("ray", "visibility", new
        {
            isVisible,
            fade = fadeDuration
        });

        internal void ShowMenu()
        {
            _uiService.SendNui("ray", "open", new { });
        }

        internal void CloseMenu()
        {
            _uiService.SendNui("ray", "close", new { });
        }

        internal void SetCrossairVisibility(bool isVisible, int fadeDuration = 100) => _uiService.SendNui("ray", "crossair", new
        {
            isVisible,
            fade = fadeDuration
        });
    }
}
