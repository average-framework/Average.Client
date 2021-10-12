using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using CitizenFX.Core.Native;
using System.Collections.Generic;

namespace Average.Client.Framework.Handlers
{
    internal class UIHandler : IHandler
    {
        private readonly UIService _uiService;

        public UIHandler(UIService uiService)
        {
            _uiService = uiService;
        }

        [ClientEvent("ui:register_nui_events")]
        private void OnRegisterNuiEvents(List<object> events)
        {
            _uiService.RegisterUIServerCallback(events);
        }

        [ClientEvent("ui:load_frame")]
        private void OnLoadFrame(string frameName) => _uiService.LoadFrame(frameName);

        [ClientEvent("ui:destroy_frame")]
        private void OnDestroyFrame(string frameName) => _uiService.DestroyFrame(frameName);

        [ClientEvent("ui:send_message")]
        private void OnSendMessage(string frame, string requestType, string message) => _uiService.SendNui(frame, requestType, message);

        [ClientEvent("ui:frame_focus")]
        private void OnFrameFocus(string frameName)
        {
            _uiService.FocusFrame(frameName);
        }

        [ClientEvent("ui:focus")]
        private void OnFocus(bool showCursor)
        {
            _uiService.Focus(showCursor);
        }

        [ClientEvent("ui:unfocus")]
        private void OnUnfocus()
        {
            _uiService.Unfocus();
        }

        [ClientEvent("ui:emit")]
        private void OnEmit(string message)
        {
            _uiService.Emit(message);
        }

        [ClientEvent("ui:show")]
        private void OnShow(string frameName)
        {
            _uiService.Show(frameName);
        }

        [ClientEvent("ui:hide")]
        private void OnHide(string frameName)
        {
            _uiService.Hide(frameName);
        }

        [ClientEvent("ui:fadein")]
        private void OnFadeIn(string frameName, int duration)
        {
            _uiService.FadeIn(frameName, duration);
        }

        [ClientEvent("ui:fadeout")]
        private void OnFadeOut(string frameName, int duration)
        {
            _uiService.FadeOut(frameName, duration);
        }

        [ClientEvent("ui:zindex")]
        private void OnSetZIndex(string frameName, int zIndex)
        {
            _uiService.SetZIndex(frameName, zIndex);
        }
    }
}
