using Average.Client.Framework.Menu;
using System;

namespace Average.Client.Framework.Events
{
    internal class MenuChangeEventArgs : EventArgs
    {
        public MenuContainer OldMenu { get; }
        public MenuContainer CurrentMenu { get; }

        public MenuChangeEventArgs(MenuContainer oldMenu, MenuContainer currentMenu)
        {
            OldMenu = oldMenu;
            CurrentMenu = currentMenu;
        }
    }
}
