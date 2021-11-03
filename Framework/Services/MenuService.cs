using Average.Client.Framework.Events;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Menu;
using System;
using System.Collections.Generic;

namespace Average.Client.Framework.Services
{
    internal class MenuService : IService
    {
        private readonly UIService _uiService;

        private MenuContainer _oldMenu;
        public MenuContainer currentMenu;
        public readonly List<MenuContainer> histories = new();

        public bool IsOpen { get; set; }
        public bool CanCloseMenu { get; set; } = true;

        public MenuService(UIService uiService)
        {
            _uiService = uiService;
        }

        public event EventHandler<MenuChangeEventArgs> MenuChanged;
        public event EventHandler<MenuCloseEventArgs> MenuClosed;

        private void OnMenuChanged(MenuContainer oldMenu, MenuContainer currentMenu)
        {
            MenuChanged?.Invoke(this, new MenuChangeEventArgs(oldMenu, currentMenu));
        }

        private void OnMenuClosed(MenuContainer currentMenu)
        {
            MenuClosed?.Invoke(this, new MenuCloseEventArgs(currentMenu));
        }

        internal void OnClientWindowInitialized()
        {
            _uiService.LoadFrame("menu");
            _uiService.SetZIndex("menu", 80000);
        }

        private void OnRender(MenuContainer menuContainer) => _uiService.SendNui("menu", "render", new
        {
            topContainer = menuContainer.TopContainer.OnRender(),
            bottomContainer = menuContainer.BottomContainer != null ? menuContainer.BottomContainer.OnRender() : null,
            middleContainer = menuContainer.MiddleContainer != null ? menuContainer.MiddleContainer.OnRender() : null
        });

        internal void Open(MenuContainer menu)
        {
            IsOpen = true;

            if (_oldMenu != currentMenu)
            {
                _oldMenu = currentMenu;
            }

            currentMenu = menu;

            AddHistory(currentMenu);
            OnRender(currentMenu);

            _uiService.SendNui("menu", "open", new
            {
                id = currentMenu.Id,
                bannerTitle = currentMenu.BannerTitle
            });

            OnMenuChanged(_oldMenu, currentMenu);
        }

        internal void Close()
        {
            if (IsOpen)
            {
                _uiService.SendNui("menu", "close");

                ClearHistory();
                OnMenuClosed(currentMenu);

                IsOpen = false;
            }
        }

        internal void ClearHistory()
        {
            histories.Clear();
        }

        internal void AddHistory(MenuContainer menuContainer)
        {
            if (!histories.Exists(x => x.Id == menuContainer.Id))
            {
                histories.Add(menuContainer);
            }
        }

        internal void RemoveHistory(MenuContainer menuContainer)
        {
            if (histories.Exists(x => x.Id == menuContainer.Id))
            {
                histories.Remove(menuContainer);
            }
        }
    }
}