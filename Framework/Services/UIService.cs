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

        internal void FocusFrame(string frame) => Emit(new
        {
            eventName = "ui:focus",
            frame
        });

        internal void Focus(bool showCursor = true)
        {
            SetNuiFocus(true, showCursor);
        }

        internal void Unfocus() => SetNuiFocus(false, false);

        internal async void Emit(object message) => SendNuiMessage(message.ToJson());

        internal void SendNui(string frame, string requestType, object message = null) => Emit(new
        {
            eventName = "ui:emit",
            frame,
            requestType,
            message = message ?? new { }
        });

        internal void LoadFrame(string frame) => Emit(new
        {
            eventName = "ui:load_frame",
            frame
        });

        internal void DestroyFrame(string frame) => Emit(new
        {
            eventName = "ui:destroy_frame",
            frame
        });

        internal void Show(string frame) => Emit(new
        {
            eventName = "ui:show",
            frame
        });

        internal void Hide(string frame) => Emit(new
        {
            eventName = "ui:hide",
            frame
        });

        internal void FadeIn(string frame, int fadeDuration = 100) => Emit(new
        {
            eventName = "ui:fadein",
            frame,
            fade = fadeDuration
        });

        internal void FadeOut(string frame, int fadeDuration = 100) => Emit(new
        {
            eventName = "ui:fadeout",
            frame,
            fade = fadeDuration
        });

        internal void SetZIndex(string frame, int zIndex) => Emit(new
        {
            eventName = "ui:zindex",
            frame,
            zIndex
        });
    }
}
