using Average.Client.Managers;
using CitizenFX.Core;
using SDK.Client.Events;
using SDK.Client.Interfaces;
using SDK.Client.Menu;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SDK.Client.GameAPI;

namespace Average.Client.Menu
{
    public class MenuManager : IMenuManager
    {
        bool isReady;

        List<MenuContainer> menus = new List<MenuContainer>();
        List<MenuContainer> history = new List<MenuContainer>();

        public MenuContainer MainMenu { get; set; }
        public MenuContainer OldMenu { get; private set; }
        public MenuContainer CurrentMenu { get; private set; }
        public bool CanCloseMenu { get; set; } = true;
        public bool IsOpen { get; private set; }

        public event EventHandler<MenuChangeEventArgs> MenuChanged;
        public event EventHandler<MenuCloseEventArgs> MenuClosed;

        public MenuManager(EventManager eventManager)
        {
            eventManager.RegisterInternalNUICallbackEvent("menu/avg.ready", Ready);
            eventManager.RegisterInternalNUICallbackEvent("menu/on_click", OnClick);
            eventManager.RegisterInternalNUICallbackEvent("menu/on_previous", OnPrevious);

            // Load menu in html page
            SendNUI(new
            {
                eventName = "avg.internal.load",
                plugin = "menu",
                fileName = "index.html"
            });
        }

        #region Nui Callback

        CallbackDelegate Ready(IDictionary<string, object> data, CallbackDelegate result)
        {
            Debug.WriteLine("Menu is fucking ready !!!");

            isReady = true;

            return result;
        }

        //[UICallback("menu/on_click")]
        CallbackDelegate OnClick(IDictionary<string, object> data, CallbackDelegate result)
        {
            var name = data["name"].ToString();
            var item = CurrentMenu.GetItem(name);

            switch (item)
            {
                case MenuItem menuItem:
                    if (menuItem.TargetContainer != null)
                    {
                        history.Add(CurrentMenu);
                        OpenMenu(menuItem.TargetContainer);
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

        //[UICallback("menu/on_previous")]
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

        public void OnMenuChanged(MenuContainer oldMenu, MenuContainer currentMenu)
        {
            if (MenuChanged != null)
            {
                MenuChanged(null, new MenuChangeEventArgs(oldMenu, currentMenu));
            }
        }

        public void OnMenuClosed(MenuContainer currentMenu)
        {
            if (MenuClosed != null)
            {
                MenuClosed(null, new MenuCloseEventArgs(currentMenu));
            }
        }

        #region Nui Methods

        public void UpdateRender(MenuContainer menuContainer)
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
                            type = "menu_bar_item",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                }
            }

            SendNUI(new
            {
                request = "menu.update_render",
                items
            });
        }

        public async Task OpenMenu(MenuContainer menu)
        {
            Debug.WriteLine("Try to open menu: " + menu.Title + ", " + Exist(menu));

            while (!isReady) await BaseScript.Delay(250);

            Debug.WriteLine("Open menu: " + menu.Title + ", " + Exist(menu));

            if (Exist(menu))
            {
                IsOpen = true;

                if (OldMenu != CurrentMenu)
                {
                    OldMenu = CurrentMenu;
                }

                CurrentMenu = menu;
                MainMenu = CurrentMenu;

                Debug.WriteLine("Open menu update render: " + menu.Title);

                UpdateRender(CurrentMenu);

                Debug.WriteLine("Update render");

                SendNUI(new
                {
                    eventName = "avg.internal",
                    on = "menu.open",
                    plugin = "menu",
                    name = CurrentMenu.Name,
                    title = CurrentMenu.Title
                });

                Debug.WriteLine("Open menu 1");

                OnMenuChanged(OldMenu, CurrentMenu);
            }
        }

        public void CloseMenu()
        {
            if (IsOpen)
            {
                IsOpen = false;

                SendNUI(new
                {
                    request = "menu.close"
                });

                OnMenuClosed(CurrentMenu);
            }
        }

        #endregion

        #region Methods

        public void ClearHistory() => history.Clear();

        public bool Exist(MenuContainer menuContainer) => menus.Exists(x => x == menuContainer);

        public void CreateSubMenu(MenuContainer menuContainer)
        {
            if (!Exist(menuContainer)) menus.Add(menuContainer);
        }

        public void RemoveSubMenu(MenuContainer menuContainer)
        {
            if (Exist(menuContainer)) menus.Remove(menuContainer);
        }

        #endregion
    }
}
