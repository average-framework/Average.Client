using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Menu;
using Average.Client.Framework.Services;
using System;

namespace Average.Client.Scripts
{
    internal class DebugScript : IScript
    {
        private readonly MenuService _menuService;

        public DebugScript(MenuService menuService)
        {
            _menuService = menuService;

            //var topContainer = new TopContainer();
            //var bottomContainer = new BottomContainer();
            //var middleContainer = new StatsMenuInfo();
            //var testMenu = new MenuContainer(topContainer, bottomContainer, middleContainer);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
