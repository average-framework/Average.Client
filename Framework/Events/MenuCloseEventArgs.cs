using Average.Client.Framework.Menu;
using System;

namespace Average.Client.Framework.Events
{
    internal class MenuCloseEventArgs : EventArgs
    {
        public MenuContainer CurrentMenu { get; }

        public MenuCloseEventArgs(MenuContainer currentMenu)
        {
            CurrentMenu = currentMenu;
        }
    }
}
