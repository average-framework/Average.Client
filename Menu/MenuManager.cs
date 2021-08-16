using Average.Client.Managers;
using CitizenFX.Core;
using SDK.Client.Events;
using SDK.Client.Interfaces;
using SDK.Client.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SDK.Client.GameAPI;

namespace Average.Client.Menu
{
    public class MenuManager : IMenuManager
    {
        bool isReady;

        List<MenuContainer> containerHistories = new List<MenuContainer>();

        public MenuContainer MainMenu { get; set; }
        public MenuContainer OldMenu { get; private set; }
        public MenuContainer CurrentMenu { get; private set; }
        public MenuTabContainer CurrentTabMenu { get; private set; }
        public bool CanCloseMenu { get; set; } = true;
        public bool IsOpen { get; private set; }

        public event EventHandler<MenuChangeEventArgs> MenuChanged;
        public event EventHandler<MenuCloseEventArgs> MenuClosed;

        public MenuManager(EventManager eventManager)
        {
            eventManager.RegisterInternalNUICallbackEvent("window_ready", WindowReady);
            eventManager.RegisterInternalNUICallbackEvent("menu/avg.ready", Ready);
            eventManager.RegisterInternalNUICallbackEvent("menu/on_click", OnClick);
            eventManager.RegisterInternalNUICallbackEvent("menu/on_tab_click", OnTabClick);
            eventManager.RegisterInternalNUICallbackEvent("menu/on_previous", OnPrevious);
        }

        #region Nui Callback

        CallbackDelegate WindowReady(IDictionary<string, object> data, CallbackDelegate result)
        {
            // Load menu in html page
            SendNUI(new
            {
                eventName = "avg.internal.load",
                plugin = "menu",
                fileName = "index.html"
            });
            return result;
        }

        CallbackDelegate Ready(IDictionary<string, object> data, CallbackDelegate result)
        {
            isReady = true;
            return result;
        }

        CallbackDelegate OnTabClick(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (!data.ContainsKey("name")) return result;

            var name = data["name"].ToString();
            var tab = CurrentTabMenu.GetItem(name);

            Task.Factory.StartNew(async () => 
            {
                for (int i = 0; i < CurrentTabMenu.Items.Count; i++)
                {
                    CurrentTabMenu.Items[i].IsSelected = false;
                    CurrentTabMenu.Items[i].UpdateRender();
                }

                tab.IsSelected = true;

                if (tab.TargetContainer != null)
                    await OpenMenu(tab.TargetContainer);

                tab.Action?.Invoke(tab);
            });

            return result;
        }
        
        CallbackDelegate OnClick(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (!data.ContainsKey("name")) return result;

            var name = data["name"].ToString();
            var item = CurrentMenu.GetItem(name);

            switch (item)
            {
                case MenuButtonItem menuItem:
                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);
                    break;
                case MenuLabelItem menuItem:
                    break;
                case MenuRichTextItem menuItem:
                    break;
                case MenuButtonContainer menuItem:
                    if (menuItem.TargetContainer != null)
                    {
                        containerHistories.Add(CurrentMenu);
                        OpenMenu(menuItem.TargetContainer);
                    }

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);
                    break;
                case MenuBarItem menuItem:
                    break;
                case MenuCheckboxItem menuItem:
                    menuItem.Checked = bool.Parse(data["isChecked"].ToString());

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);

                    result(menuItem.Checked);
                    break;
                case MenuTwoCheckboxItem menuItem:
                    menuItem.Input1.IsChecked = bool.Parse(data["isChecked1"].ToString());
                    menuItem.Input2.IsChecked = bool.Parse(data["isChecked2"].ToString());

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);

                    result(new
                    {
                        isChecked1 = menuItem.Input1.IsChecked,
                        isChecked2 = menuItem.Input2.IsChecked
                    });
                    break;
                case MenuSelectorItem<int> menuItem:
                    if (menuItem.Action != null)
                    {
                        if (data["operator"].ToString() == "-")
                        {
                            if (menuItem.Value <= menuItem.MinValue)
                                menuItem.Value = menuItem.MinValue;
                            else
                                menuItem.Value -= menuItem.Step;
                        }
                        else if (data["operator"].ToString() == "+")
                        {
                            if (menuItem.Value >= menuItem.MaxValue)
                                menuItem.Value = menuItem.MaxValue;
                            else
                                menuItem.Value += menuItem.Step;
                        }

                        menuItem.Action.Invoke(menuItem);
                        result(menuItem.Value + "/" + menuItem.MaxValue);
                    }
                    break;
                case MenuSelectorItem<float> menuItem:
                    if (menuItem.Action != null)
                    {
                        if (data["operator"].ToString() == "-")
                        {
                            if (menuItem.Value <= menuItem.MinValue)
                                menuItem.Value = menuItem.MinValue;
                            else
                                menuItem.Value -= menuItem.Step;
                        }
                        else if (data["operator"].ToString() == "+")
                        {
                            if (menuItem.Value >= menuItem.MaxValue)
                                menuItem.Value = menuItem.MaxValue;
                            else
                                menuItem.Value += menuItem.Step;
                        }

                        menuItem.Action.Invoke(menuItem);
                        result(menuItem.Value.ToString("0.00") + "/" + menuItem.MaxValue.ToString("0.00"));
                    }
                    break;
                case MenuItemList menuItem:
                    if (menuItem.Action != null)
                    {
                        if (data["operator"].ToString() == "-")
                        {
                            if (menuItem.Index != 0)
                                menuItem.Index--;
                            else
                                menuItem.Index = menuItem.Values.Count - 1;
                        }
                        else if (data["operator"].ToString() == "+")
                        {
                            if (menuItem.Index != menuItem.Values.Count - 1)
                                menuItem.Index++;
                            else
                                menuItem.Index = 0;
                        }

                        menuItem.Action.Invoke(menuItem.Index, menuItem.Values);
                        result(menuItem.Values.ToList().ElementAt(menuItem.Index).Key);
                    }
                    break;
                case MenuTextboxItem menuItem:
                    menuItem.Value = data["value"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Value);
                    break;
                case MenuVector2InputItem menuItem:
                    if (data.ContainsKey("value1"))
                        menuItem.Input1.Value = data["value1"];
                    if (data.ContainsKey("value2"))
                        menuItem.Input2.Value = data["value2"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Input1.Value, menuItem.Input2.Value);
                    break;
                case MenuVector3InputItem menuItem:
                    if (data.ContainsKey("value1"))
                        menuItem.Input1.Value = data["value1"];
                    if (data.ContainsKey("value2"))
                        menuItem.Input2.Value = data["value2"];
                    if (data.ContainsKey("value3"))
                        menuItem.Input3.Value = data["value3"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Input1.Value, menuItem.Input2.Value, menuItem.Input3.Value);
                    break;
                case MenuTextAreaItem menuItem:
                    menuItem.Value = data["value"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Value);
                    break;
            }

            return result;
        }

        CallbackDelegate OnPrevious(IDictionary<string, object> data, CallbackDelegate result)
        {
            Task.Factory.StartNew(async () =>
            {
                if (IsOpen)
                {
                    if (containerHistories.Count > 0)
                    {
                        var containerIndex = containerHistories.Count - 1;
                        var parent = containerHistories[containerIndex];

                        await OpenMenu(parent);

                        containerHistories.RemoveAt(containerIndex);
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
            });

            return result;
        }

        #endregion

        public void OnMenuChanged(MenuContainer oldMenu, MenuContainer currentMenu)
        {
            if (MenuChanged != null)
                MenuChanged(null, new MenuChangeEventArgs(oldMenu, currentMenu));
        }

        public void OnMenuClosed(MenuContainer currentMenu)
        {
            if (MenuClosed != null)
                MenuClosed(null, new MenuCloseEventArgs(currentMenu));
        }

        #region Nui Methods

        public void SelectTab(ref MenuTabItem item)
        {
            for(int i = 0; i < CurrentMenu.Items.Count; i++)
            {
                var current = CurrentMenu.Items[i];

                if(current is MenuTabItem)
                {
                    (current as MenuTabItem).IsSelected = false;
                }
            }

            item.IsSelected = true;
        }

        public void UpdateRender(MenuContainer menuContainer, MenuTabContainer menuTabContainer)
        {
            var items = new List<object>();
            var tabs = new List<object>();

            foreach (var item in menuContainer.Items)
            {
                switch (item)
                {
                    case MenuButtonItem menuItem:
                        items.Add(new
                        {
                            type = "menu_button",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuLabelItem menuItem:
                        items.Add(new
                        {
                            type = "menu_label",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuRichTextItem menuItem:
                        items.Add(new
                        {
                            type = "menu_richtext",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuButtonContainer menuItem:
                        items.Add(new
                        {
                            type = "menu_button_container",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            hasTarget = menuItem.TargetContainer != null ? true : false,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuCheckboxItem menuItem:
                        items.Add(new
                        {
                            type = "menu_checkbox",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            isChecked = menuItem.Checked,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuTwoCheckboxItem menuItem:
                        items.Add(new
                        {
                            type = "menu_two_checkbox",
                            name = menuItem.Name,
                            text1 = menuItem.Input1.Text,
                            text2 = menuItem.Input2.Text,
                            isChecked1 = menuItem.Input1.IsChecked,
                            isChecked2 = menuItem.Input2.IsChecked,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuSelectorItem<int> menuItem:
                        items.Add(new
                        {
                            type = "menu_selector",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            //min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            //step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuSelectorItem<float> menuItem:
                        items.Add(new
                        {
                            type = "menu_selector",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            //min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            //step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuItemList menuItem:
                        Debug.WriteLine("item list: " + menuItem.Values.ToList().ElementAt(menuItem.Index).Key + ", " + menuItem.Index);
                        items.Add(new
                        {
                            type = "menu_list",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            itemName = menuItem.Values.ToList().ElementAt(menuItem.Index).Key,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuTextboxItem menuItem:
                        items.Add(new
                        {
                            type = "menu_textbox",
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
                    case MenuVector2InputItem menuItem:
                        items.Add(new
                        {
                            type = "menu_vector2_input",
                            name = menuItem.Name,
                            text1 = menuItem.Input1.Text,
                            placeholder1 = menuItem.Input1.PlaceHolder,
                            pattern1 = menuItem.Input1.Pattern,
                            minLength1 = menuItem.Input1.MinLength,
                            maxLength1 = menuItem.Input1.MaxLength,
                            value1 = menuItem.Input1.Value,
                            text2 = menuItem.Input2.Text,
                            placeholder2 = menuItem.Input2.PlaceHolder,
                            pattern2 = menuItem.Input2.Pattern,
                            minLength2 = menuItem.Input2.MinLength,
                            maxLength2 = menuItem.Input2.MaxLength,
                            value2 = menuItem.Input2.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuVector3InputItem menuItem:
                        items.Add(new
                        {
                            type = "menu_vector3_input",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            placeholder1 = menuItem.Input1.PlaceHolder,
                            pattern1 = menuItem.Input1.Pattern,
                            minLength1 = menuItem.Input1.MinLength,
                            maxLength1 = menuItem.Input1.MaxLength,
                            value1 = menuItem.Input1.Value,
                            placeholder2 = menuItem.Input2.PlaceHolder,
                            pattern2 = menuItem.Input2.Pattern,
                            minLength2 = menuItem.Input2.MinLength,
                            maxLength2 = menuItem.Input2.MaxLength,
                            value2 = menuItem.Input2.Value,
                            placeholder3 = menuItem.Input3.PlaceHolder,
                            pattern3 = menuItem.Input3.Pattern,
                            minLength3 = menuItem.Input3.MinLength,
                            maxLength3 = menuItem.Input3.MaxLength,
                            value3 = menuItem.Input3.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case MenuTextAreaItem menuItem:
                        items.Add(new
                        {
                            type = "menu_textarea",
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
                            type = "menu_bar",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                }
            }

            foreach (var item in menuTabContainer.Items)
            {
                tabs.Add(new
                {
                    name = item.Name,
                    iconpath = item.IconPath,
                    visible = item.Visible,
                    isSelected = item.IsSelected
                });
            }

            SendNUI(new
            {
                eventName = "avg.internal",
                on = "menu.update_render",
                plugin = "menu",
                items,
                tabs
            });
        }

        public async Task OpenMenu(MenuContainer menu)
        {
            while (!isReady) await BaseScript.Delay(250);

            if (Exist(menu))
            {
                IsOpen = true;

                if (OldMenu != CurrentMenu)
                    OldMenu = CurrentMenu;

                CurrentMenu = menu;
                MainMenu = CurrentMenu;

                UpdateRender(CurrentMenu, CurrentTabMenu);

                SendNUI(new
                {
                    eventName = "avg.internal",
                    on = "menu.open",
                    plugin = "menu",
                    name = CurrentMenu.Name,
                    title = CurrentMenu.Title
                });

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

        public void ClearHistory() => containerHistories.Clear();

        public bool Exist(MenuContainer menuContainer) => containerHistories.Exists(x => x == menuContainer);

        public void AddMenuToHistory(MenuContainer menuContainer)
        {
            if (!Exist(menuContainer)) containerHistories.Add(menuContainer);
        }

        public void RemoveMenuInHistory(MenuContainer menuContainer)
        {
            if (Exist(menuContainer)) containerHistories.Remove(menuContainer);
        }

        public void SetTabMenu(MenuTabContainer menuTabContainer)
        {
            CurrentTabMenu = menuTabContainer;
        }

        #endregion
    }
}
