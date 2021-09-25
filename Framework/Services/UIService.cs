using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class UIService : IService
    {
        public UIService()
        {

        }

        internal async Task ShutdownLoadingScreen()
        {
            API.ShutdownLoadingScreen();
            while (IsLoadingScreenActive()) await BaseScript.Delay(0);
        }

        internal async Task FadeIn(int duration = 1000)
        {
            await GameAPI.FadeIn(duration);
        }

        internal async Task FadeOut(int duration = 1000)
        {
            await GameAPI.FadeOut(duration);
        }

        internal void Focus(bool showCursor = true) => SetNuiFocus(true, showCursor);
        internal void Unfocus() => SetNuiFocus(false, false);

        internal async void _SendNui(object message)
        {
            //await BaseScript.Delay(0);
            SendNuiMessage(message.ToJson());
        }

        internal void SendNui(string frame, string requestType, object message = null) => _SendNui(new
        {
            eventName = "ui:emit",
            frame,
            requestType,
            message = (message == null ? new { } : message)
        });

        internal void LoadFrame(string frame) => _SendNui(new
        {
            eventName = "ui:load_frame",
            frame
        });

        internal void DestroyFrame(string frame) => _SendNui(new
        {
            eventName = "ui:destroy_frame",
            frame
        });

        internal void Show(string frame) => _SendNui(new
        {
            eventName = "ui:show",
            frame
        });

        internal void Hide(string frame) => _SendNui(new
        {
            eventName = "ui:hide",
            frame
        });
    }
}
