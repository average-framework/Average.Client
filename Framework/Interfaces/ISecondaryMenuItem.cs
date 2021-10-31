using Average.Client.Framework.Services;

namespace Average.Client.Framework.Interfaces
{
    interface ISecondaryMenuItem
    {
        string Id { get; }
        bool Visible { get; set; }
        bool Disabled { get; set; }
        object OnRender();
        void OnUpdate(UIService uiService);
    }
}
