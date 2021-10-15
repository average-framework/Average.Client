using Average.Client.Framework.Services;
using Average.Client.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Ray
{
    internal class RayItem
    {
        public string Id { get; } = RandomString();
        public string Text { get; }
        public string Emoji { get; }

        [JsonIgnore]
        public bool CloseMenuOnAction { get; }

        public bool IsVisible { get; set; }

        [JsonIgnore]
        public Func<RaycastHit, Task<bool>> Condition { get; }

        [JsonIgnore]
        public Action<RaycastHit> Action { get; }

        public RayItem(string text, string emoji, bool closeMenuOnAction, Action<RaycastHit> action, Func<RaycastHit, Task<bool>> condition)
        {
            Text = text;
            Emoji = emoji;
            CloseMenuOnAction = closeMenuOnAction;
            Action = action;
            Condition = condition;
        }

        private void OnRender(UIService uiService) => uiService.SendNui("ray", "render_item", new
        {
            id = Id,
            text = Text,
            emoji = Emoji,
            isVisible = IsVisible
        });
    }
}
