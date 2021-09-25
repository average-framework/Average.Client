using Average.Client.Menu;

namespace Average.Client.Interfaces
{
    internal interface IMenuItem
    {
        string Name { get; }
        bool Visible { get; }
        MenuContainer Parent { get; set; }
    }
}
