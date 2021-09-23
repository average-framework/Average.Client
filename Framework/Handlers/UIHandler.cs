using Average.Client.Framework.Attributes;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using CitizenFX.Core.Native;

namespace Average.Client.Framework.Handlers
{
    internal class UIHandler : IHandler
    {
        public UIHandler()
        {

        }

        [ClientEvent("ui:load_frame")]
        private void OnLoadFrame(string frame) => API.SendNuiMessage(new
        {
            eventName = "ui:load_frame",
            frame
        }.ToJson());

        [ClientEvent("ui:destroy_frame")]
        private void OnDestroyFrame(string frame) => API.SendNuiMessage(new
        {
            eventName = "ui:destroy_frame",
            frame
        }.ToJson());

        [ClientEvent("ui:emit")]
        private void OnSendMessage(string frame, string requestType, string message) => API.SendNuiMessage(new
        {
            eventName = "ui:emit",
            frame,
            requestType,
            message
        }.ToJson());
    }
}
