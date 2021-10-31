using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using static Average.Client.Framework.GameAPI;

namespace Average.Client.Framework.Menu
{
    internal class StoreMenuInfo : IMenuInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Weight { get; set; }
        public decimal SellablePrice { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        public StoreMenuInfo(string defaultTitle, string defaultDescription, string defaultWeight, decimal defaultSellablePrice, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Title = defaultTitle;
            Description = defaultDescription;
            Weight = defaultWeight;
            SellablePrice = defaultSellablePrice;
            Visible = visible;
            Disabled = disabled;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            title = Title,
            description = Description,
            weight = Weight,
            sellablePrice = SellablePrice,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
