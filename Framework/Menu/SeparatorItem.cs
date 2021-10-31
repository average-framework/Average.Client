using Average.Client.Framework.Services;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class SeparatorItem : IPrimaryMenuItem
    {
        internal enum SeparatorType
        {
            Up,
            Down,
            UpArrow,
            DownArrow,
        }

        public string Id { get; private set; }
        public SeparatorType Type { get; private set; }
        public bool Visible { get; set; }

        public SeparatorItem(SeparatorType separatorType, bool visible = true)
        {
            Id = RandomString();
            Type = separatorType;
            Visible = visible;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            separatorType = Type,
            visible = Visible
        };

        public void OnUpdate(UIService uiService)
        {

        }
    }
}
