using Average.Client.Framework.Services;

namespace Average.Client.Framework.Interfaces
{
    interface IMenuInfo
    {
        string Id { get; set; }
        object OnRender();
        void OnUpdate(UIService uiService);
    }
}
