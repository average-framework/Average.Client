using CitizenFX.Core;
using SDK.Client;
using SDK.Client.Plugins;
using SDK.Shared;
using SDK.Shared.Plugins;
using Shared.Server.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Menu.Client
{
    [MainScript]
    public class Menu : Plugin, IMenuClient_Menu
    {
        List<MenuContainer> menus = new List<MenuContainer>();
        List<string> history = new List<string>();

        public MenuContainer MainMenu { get; set; }
        public MenuContainer OldMenu { get; private set; }
        public MenuContainer CurrentMenu { get; private set; }
        public bool CanCloseMenu { get; set; } = true;
        public bool IsOpen { get; private set; }

        public delegate void OnMenuChange(MenuContainer oldMenu, MenuContainer currentMenu);
        public delegate void OnMenuClose(MenuContainer currentMenu);

        public event OnMenuChange OnMenuChangeHandler;
        public event OnMenuClose OnMenuCloseHandler;

        protected virtual void OnMenuChangeReached(MenuContainer oldMenu, MenuContainer currentMenu) => OnMenuChangeHandler?.Invoke(oldMenu, currentMenu);
        protected virtual void OnMenuCloseReached(MenuContainer currentMenu) => OnMenuCloseHandler?.Invoke(currentMenu);

        public Menu(Framework framework, PluginInfo pluginInfo) : base(framework, pluginInfo)
        {

        }

        public override async Task OnReady()
        {
            await Overlay.Load();
        }

        public override async Task UIReady()
        {
            await Overlay.Show();
        }

        #region Nui Callback

        [UICallback("menu/on_click")]
        CallbackDelegate OnClick(IDictionary<string, object> data, CallbackDelegate result)
        {
            var name = data["name"].ToString();
            var item = CurrentMenu.GetItem(name);

            switch (item)
            {
                case MenuItem menuItem:
                    if (menuItem.TargetContainer != null)
                    {
                        history.Add(CurrentMenu.Name);
                        OpenMenu(menuItem.TargetContainer.Name);
                    }

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem);
                    break;
                case MenuBarItem menuItem:
                    break;
                case MenuCheckboxItem menuItem:
                    menuItem.Checked = !menuItem.Checked;

                    if (menuItem.Action != null)
                    {
                        menuItem.Action.Invoke(menuItem);
                        result(menuItem.Checked);
                    }
                    break;
                case MenuItemSlider<int> menuItem:
                    menuItem.Value = int.Parse(data["value"].ToString());

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem.Value);
                    break;
                case MenuItemSlider<float> menuItem:
                    menuItem.Value = float.Parse(data["value"].ToString());

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem.Value);
                    break;
                case MenuItemList menuItem:
                    if (menuItem.Action != null)
                    {
                        if (data["operator"].ToString() == "-")
                        {
                            if (menuItem.Index != 0)
                                menuItem.Index--;
                        }
                        else if (data["operator"].ToString() == "+")
                        {
                            if (menuItem.Index != menuItem.Values.Count - 1)
                                menuItem.Index++;
                        }

                        menuItem.Action.Invoke(menuItem.Index, menuItem.Values[menuItem.Index]);
                        result(menuItem.Values[menuItem.Index].Value);
                    }
                    break;
                case MenuTextboxItem menuItem:
                    menuItem.Value = data["value"];

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem.Value);
                    break;
                case MenuTextAreaItem menuItem:
                    menuItem.Value = data["value"];

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem.Value);
                    break;
                case MenuSliderSelectorItem<int> menuItem:
                    if (data.ContainsKey("value")) menuItem.Value = int.Parse(data["value"].ToString());

                    if (data.ContainsKey("operator"))
                    {
                        var op = data["operator"].ToString();

                        if (op == "-")
                        {
                            if (menuItem.Value <= menuItem.MinValue)
                                menuItem.Value = menuItem.MinValue;
                            else
                                menuItem.Value -= menuItem.Step;
                        }
                        else if (op == "+")
                        {
                            if (menuItem.Value >= menuItem.MaxValue)
                                menuItem.Value = menuItem.MaxValue;
                            else
                                menuItem.Value += menuItem.Step;
                        }

                        result(menuItem.Value);
                    }

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem);
                    break;
                case MenuSliderSelectorItem<float> menuItem:
                    if (data.ContainsKey("value")) menuItem.Value = float.Parse(data["value"].ToString());

                    if (data.ContainsKey("operator"))
                    {
                        var op = data["operator"].ToString();

                        if (op == "-")
                        {
                            if (menuItem.Value <= menuItem.MinValue)
                                menuItem.Value = menuItem.MinValue;
                            else
                                menuItem.Value -= menuItem.Step;
                        }
                        else if (op == "+")
                        {
                            if (menuItem.Value >= menuItem.MaxValue)
                                menuItem.Value = menuItem.MaxValue;
                            else
                                menuItem.Value += menuItem.Step;
                        }

                        result(menuItem.Value);
                    }

                    if (menuItem.Action != null) menuItem.Action.Invoke(menuItem);
                    break;
            }

            return result;
        }

        [UICallback("menu/on_previous")]
        CallbackDelegate OnPrevious(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (IsOpen)
            {
                if (history.Count > 0)
                {
                    var containerIndex = history.Count - 1;
                    var parent = history[containerIndex];

                    OpenMenu(parent);

                    history.RemoveAt(containerIndex);
                }
                else
                {
                    if (CanCloseMenu)
                    {
                        CloseMenu();
                        ClearHistory();
                        Unfocus();
                    }
                }
            }

            return result;
        }

        #endregion

        #region Nui Methods

        public async Task UpdateRender(MenuContainer menuContainer)
        {
            var items = new List<object>();

            foreach (var item in menuContainer.Items)
            {
                switch (item)
                {
                    case MenuItem menuItem:
                        items.Add(new
                        {
                            type = "menu_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            hasTarget = menuItem.TargetContainer != null ? true : false,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuCheckboxItem menuItem:
                        items.Add(new
                        {
                            type = "menu_checkbox_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            isChecked = menuItem.Checked,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuItemSlider<int> menuItem:
                        items.Add(new
                        {
                            type = "menu_slider_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuItemSlider<float> menuItem:
                        items.Add(new
                        {
                            type = "menu_slider_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuSliderSelectorItem<int> menuItem:
                        items.Add(new
                        {
                            type = "menu_slider_selector_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuSliderSelectorItem<float> menuItem:
                        items.Add(new
                        {
                            type = "menu_slider_selector_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuItemList menuItem:
                        items.Add(new
                        {
                            type = "menu_list_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            itemName = menuItem.Values[menuItem.Index].Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuTextboxItem menuItem:
                        items.Add(new
                        {
                            type = "menu_textbox_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            placeholder = menuItem.Placeholder,
                            pattern = menuItem.Pattern,
                            minLength = menuItem.MinLength,
                            maxLength = menuItem.MaxLength,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuTextAreaItem menuItem:
                        items.Add(new
                        {
                            type = "menu_textarea_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            placeholder = menuItem.Placeholder,
                            pattern = menuItem.Pattern,
                            minLength = menuItem.MinLength,
                            maxLength = menuItem.MaxLength,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuBarItem menuItem:
                        items.Add(new
                        {
                            type = "menu_stats_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                }
            }

            await SendNUI(new
            {
                request = "menu.updateRender",
                items
            });
        }

        public async Task OpenMenu(string name)
        {
            if (Exist(name))
            {
                IsOpen = true;

                if (OldMenu != CurrentMenu)
                {
                    OldMenu = CurrentMenu;
                }

                CurrentMenu = GetContainer(name);
                MainMenu = CurrentMenu;

                await UpdateRender(CurrentMenu);

                await SendNUI(new
                {
                    request = "menu.open",
                    name = CurrentMenu.Name,
                    title = CurrentMenu.Title
                });

                OnMenuChangeReached(OldMenu, CurrentMenu);
            }
        }

        public async Task OpenMenu(MenuContainer menu)
        {
            if (Exist(menu))
            {
                IsOpen = true;

                if (OldMenu != CurrentMenu)
                {
                    OldMenu = CurrentMenu;
                }

                CurrentMenu = menu;
                MainMenu = CurrentMenu;

                await UpdateRender(CurrentMenu);

                await SendNUI(new
                {
                    request = "menu.open",
                    name = CurrentMenu.Name,
                    title = CurrentMenu.Title
                });

                OnMenuChangeReached(OldMenu, CurrentMenu);
            }
        }

        public async Task CloseMenu()
        {
            if (IsOpen)
            {
                IsOpen = false;

                await SendNUI(new
                {
                    request = "menu.close"
                });

                OnMenuCloseReached(CurrentMenu);
            }
        }

        #endregion

        #region Methods

        public void ClearHistory() => history.Clear();

        public bool Exist(MenuContainer menuContainer) => menus.Contains(menuContainer);

        public bool Exist(string menuName) => menus.Exists(x => x.Name == menuName);

        public MenuContainer GetContainer(string menuName) => menus.Find(x => x.Name == menuName);

        public void CreateSubMenu(MenuContainer menuContainer)
        {
            if (!Exist(menuContainer)) menus.Add(menuContainer);
        }

        public void RemoveSubMenu(MenuContainer menuContainer)
        {
            if (Exist(menuContainer)) menus.Remove(menuContainer);
        }

        public void RemoveSubMenu(string menuName)
        {
            if (Exist(menuName)) menus.Remove(menus.Find(x => x.Name == menuName));
        }

        #endregion
    }
}
