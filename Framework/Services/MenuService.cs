using Average.Client.Framework.Events;
using Average.Client.Framework.Interfaces;
using Average.Client.Menu;
using Average.Shared.Attributes;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Average.Client.Framework.Services
{
    internal class MenuService : IService
    {
        private readonly UIService _uiService;

        private MenuContainer _currentMenu;
        private MenuContainer _oldMenu;
        private TabContainer _currentTabMenu;

        private readonly List<MenuContainer> _containerHistories = new();

        public bool IsOpen { get; private set; }
        public bool CanCloseMenu { get; set; }

        public MenuService(UIService uiService)
        {
            _uiService = uiService;
        }

        public event EventHandler<MenuChangeEventArgs> MenuChanged;
        public event EventHandler<MenuCloseEventArgs> MenuClosed;

        [UICallback("menu/on_tab_click")]
        private CallbackDelegate OnTabClick(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (!data.ContainsKey("name"))
            {
                return result;
            }

            if (_currentMenu == null)
            {
                return result;
            }

            var name = data["name"].ToString();
            var tab = _currentTabMenu.GetItem(name);

            for (int i = 0; i < _currentTabMenu.Items.Count; i++)
            {
                _currentTabMenu.Items[i].IsSelected = false;
            }

            tab.IsSelected = true;

            if (tab.TargetContainer != null)
            {
                Open(tab.TargetContainer);
            }

            tab.Action?.Invoke(tab);

            return result;
        }

        [UICallback("menu/on_click")]
        private CallbackDelegate OnClick(IDictionary<string, object> data, CallbackDelegate result)
        {
            if (!data.ContainsKey("name"))
            {
                return result;
            }    

            var name = data["name"].ToString();
            var item = _currentMenu.GetItem(name);

            switch (item)
            {
                case ButtonItem menuItem:
                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);
                    break;
                case LabelItem menuItem:
                    break;
                case RichTextItem menuItem:
                    break;
                case ButtonContainer menuItem:
                    if (menuItem.Target != null)
                    {
                        _containerHistories.Add(_currentMenu);
                        Open(menuItem.Target);
                    }

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);
                    break;
                case BarItem menuItem:
                    break;
                case CheckboxItem menuItem:
                    menuItem.Checked = bool.Parse(data["isChecked"].ToString());

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem);

                    result(menuItem.Checked);
                    break;
                case TwoCheckboxItem menuItem:
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
                case SelectorItem<int> menuItem:
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
                case SelectorItem<float> menuItem:
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
                case ListItem menuItem:
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
                        result(menuItem.ShowValue ? menuItem.Values.ToList().ElementAt(menuItem.Index).Value : menuItem.Values.ToList().ElementAt(menuItem.Index).Key);
                    }
                    break;
                case TextBoxItem menuItem:
                    menuItem.Value = data["value"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Value);
                    break;
                case Vector2InputItem menuItem:
                    if (data.ContainsKey("value1"))
                        menuItem.Input1.Value = data["value1"];
                    if (data.ContainsKey("value2"))
                        menuItem.Input2.Value = data["value2"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Input1.Value, menuItem.Input2.Value);
                    break;
                case Vector3InputItem menuItem:
                    if (data.ContainsKey("value1"))
                        menuItem.Input1.Value = data["value1"];
                    if (data.ContainsKey("value2"))
                        menuItem.Input2.Value = data["value2"];
                    if (data.ContainsKey("value3"))
                        menuItem.Input3.Value = data["value3"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Input1.Value, menuItem.Input2.Value, menuItem.Input3.Value);
                    break;
                case TextAreaItem menuItem:
                    menuItem.Value = data["value"];

                    if (menuItem.Action != null)
                        menuItem.Action.Invoke(menuItem.Value);
                    break;
            }

            return result;
        }

        [UICallback("menu/keydown")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> data, CallbackDelegate result)
        {
            var key = int.Parse(data["key"].ToString());

            if (IsOpen && key == 27)
            {
                if (key == 27)
                {
                    if (_containerHistories.Count > 0)
                    {
                        var containerIndex = _containerHistories.Count - 1;
                        var parent = _containerHistories[containerIndex];

                        Open(parent);

                        _containerHistories.RemoveAt(containerIndex);
                    }
                    else
                    {
                        if (CanCloseMenu)
                        {
                            Close();
                            _uiService.Unfocus();
                        }
                    }
                }
            }

            return result;
        }

        private void OnMenuChanged(MenuContainer oldMenu, MenuContainer currentMenu)
        {
            MenuChanged?.Invoke(this, new MenuChangeEventArgs(oldMenu, currentMenu));
        }

        private void OnMenuClosed(MenuContainer currentMenu)
        {
            MenuClosed?.Invoke(this, new MenuCloseEventArgs(currentMenu));
        }

        public void UpdateRender(MenuContainer menuContainer, TabContainer menuTabContainer)
        {
            var items = new List<object>();
            var tabs = new List<object>();

            foreach (var item in menuContainer.Items)
            {
                switch (item)
                {
                    case ButtonItem menuItem:
                        items.Add(new
                        {
                            type = "button",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            visible = menuItem.Visible
                        });
                        break;
                    case LabelItem menuItem:
                        items.Add(new
                        {
                            type = "label",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            visible = menuItem.Visible
                        });
                        break;
                    case RichTextItem menuItem:
                        items.Add(new
                        {
                            type = "richtext",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            visible = menuItem.Visible
                        });
                        break;
                    case ButtonContainer menuItem:
                        items.Add(new
                        {
                            type = "button_container",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            hasTarget = menuItem.Target != null,
                            visible = menuItem.Visible
                        });
                        break;
                    case CheckboxItem menuItem:
                        items.Add(new
                        {
                            type = "checkbox",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            isChecked = menuItem.Checked,
                            visible = menuItem.Visible
                        });
                        break;
                    case TwoCheckboxItem menuItem:
                        items.Add(new
                        {
                            type = "two_checkbox",
                            name = menuItem.Name,
                            text1 = menuItem.Input1.Text,
                            text2 = menuItem.Input2.Text,
                            isChecked1 = menuItem.Input1.IsChecked,
                            isChecked2 = menuItem.Input2.IsChecked,
                            visible = menuItem.Visible
                        });
                        break;
                    case SelectorItem<int> menuItem:
                        items.Add(new
                        {
                            type = "selector",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            //min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            //step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case SelectorItem<float> menuItem:
                        items.Add(new
                        {
                            type = "selector",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            //min = menuItem.MinValue,
                            max = menuItem.MaxValue,
                            //step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                    case ListItem menuItem:
                        items.Add(new
                        {
                            type = "list",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            itemName = menuItem.ShowValue ? menuItem.Values.ToList().ElementAt(menuItem.Index).Value : menuItem.Values.ToList().ElementAt(menuItem.Index).Key,
                            visible = menuItem.Visible
                        });
                        break;
                    case TextBoxItem menuItem:
                        items.Add(new
                        {
                            type = "textbox",
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
                    case Vector2InputItem menuItem:
                        items.Add(new
                        {
                            type = "vector2input",
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
                    case Vector3InputItem menuItem:
                        items.Add(new
                        {
                            type = "vector3input",
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
                    case TextAreaItem menuItem:
                        items.Add(new
                        {
                            type = "textarea",
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
                    case BarItem menuItem:
                        items.Add(new
                        {
                            type = "bar",
                            name = menuItem.Name,
                            text = menuItem.Text,
                            description = menuItem.Description,
                            step = menuItem.Step,
                            value = menuItem.Value,
                            visible = menuItem.Visible
                        });
                        break;
                }
            }

            if (menuTabContainer != null)
            {
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
            }

            _uiService.SendNui("menu", "render", new
            {
                items,
                tabs
            });
        }

        public async Task Open(MenuContainer menu)
        {
            IsOpen = true;

            if (_oldMenu != _currentMenu)
            {
                _oldMenu = _currentMenu;
            }

            _currentMenu = menu;
            //MainMenu = _currentMenu;

            UpdateRender(_currentMenu, _currentTabMenu);

            _uiService.SendNui("menu", "open", new
            {
                name = _currentMenu.Name,
                title = _currentMenu.Title,
                description = _currentMenu.Description,
            });

            OnMenuChanged(_oldMenu, _currentMenu);
        }

        public void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
                ClearHistory();

                _uiService.SendNui("menu", "close");

                OnMenuClosed(_currentMenu);
            }
        }

        public void ClearHistory() => _containerHistories.Clear();

        public bool Exists(MenuContainer menuContainer) => _containerHistories.Exists(x => x == menuContainer);

        public void AddMenuToHistory(MenuContainer menuContainer)
        {
            if (!Exists(menuContainer))
            {
                _containerHistories.Add(menuContainer);
            }
        }

        public void RemoveMenuInHistory(MenuContainer menuContainer)
        {
            if (Exists(menuContainer))
            {
                _containerHistories.Remove(menuContainer);
            }
        }

        public void SetTabMenu(TabContainer menuTabContainer)
        {
            _currentTabMenu = menuTabContainer;
        }
    }
}