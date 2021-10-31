using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class StoreButtonItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public decimal Amount { get; set; }
        public bool Visible { get; set; }

        [JsonIgnore]
        public Action<StoreButtonItem> OnClick { get; }

        public StoreButtonItem(string text, decimal amount, Action<StoreButtonItem> onClick, bool visible = true)
        {
            Id = RandomString();
            Text = text;
            OnClick = onClick;
            Amount = amount;
            Visible = visible;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            dollar = "$" + Amount.ToString("0.00").Split('.')[0],
            cent = Amount.ToString("0.00").Split('.')[1],
            visible = Visible
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
