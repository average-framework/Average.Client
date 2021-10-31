using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Menu;
using Average.Client.Framework.Services;
using Average.Shared.Attributes;
using CitizenFX.Core;
using System;
using System.Collections.Generic;

namespace Average.Client.Framework.Handlers
{
    internal class MenuHandler : IHandler
    {
        private readonly UIService _uiService;
        private readonly MenuService _menuService;

        public MenuHandler(UIService uiService, MenuService menuService)
        {
            _uiService = uiService;
            _menuService = menuService;
        }

        [UICallback("window_ready")]
        private CallbackDelegate OnWindowReady(IDictionary<string, object> args, CallbackDelegate cb)
        {
            Logger.Error("Window Ready 2");

            _menuService.OnClientWindowInitialized();

            return cb;
        }

        [UICallback("menu/on_click")]
        private CallbackDelegate OnClick(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var id = (string)args["id"];
            var type = (string)args["type"];

            var isPrimary = _menuService.currentMenu.GetPrimaryItem(id) != null;
            var isSecondary = _menuService.currentMenu.GetSecondaryItem(id) != null;

            Logger.Error("Result: " + isPrimary + ", " + isSecondary);

            if (isPrimary)
            {
                var primaryItem = _menuService.currentMenu.GetPrimaryItem(id);

                Logger.Error("Result 2: " + primaryItem.GetType().Name + ", " + string.Join(", ", args));

                switch (primaryItem)
                {
                    case ButtonItem menuItem:
                        menuItem.OnClick?.Invoke(menuItem);
                        break;
                    case RedirectButtonItem menuItem:
                        _menuService.Open(menuItem.MenuTarget);
                        menuItem.OnClick?.Invoke(menuItem);
                        break;
                    case StoreButtonItem menuItem:
                        menuItem.OnClick?.Invoke(menuItem);
                        break;
                    case TextboxItem menuItem:
                        var value = args["value"];
                        menuItem.Value = value;
                        menuItem.OnInput?.Invoke(menuItem, value);
                        break;
                    case Vector2Item menuItem:
                        var primaryValue = args["primaryValue"];
                        var secondaryValue = args["secondaryValue"];
                        menuItem.Primary.Value = primaryValue;
                        menuItem.Secondary.Value = secondaryValue;
                        menuItem.OnInput?.Invoke(menuItem, primaryValue, secondaryValue);
                        break;
                    case Vector3Item menuItem:
                        primaryValue = args["primaryValue"];
                        secondaryValue = args["secondaryValue"];
                        var tertiaryValue = args["tertiaryValue"];
                        menuItem.Primary.Value = primaryValue;
                        menuItem.Secondary.Value = secondaryValue;
                        menuItem.Tertiary.Value = tertiaryValue;
                        menuItem.OnInput?.Invoke(menuItem, primaryValue, secondaryValue, tertiaryValue);
                        break;
                    case SelectItem menuItem:
                        var selectTypeString = args["selectType"];
                        var selectType = SelectItem.SelectType.Previous;

                        switch (selectTypeString)
                        {
                            case "-":
                                selectType = SelectItem.SelectType.Previous;
                                menuItem.CurrentIndex--;

                                if(menuItem.CurrentIndex < 0)
                                {
                                    menuItem.CurrentIndex = menuItem.Items.Count - 1;
                                }
                                break;
                            case "+":
                                selectType = SelectItem.SelectType.Next;
                                menuItem.CurrentIndex++;
                                
                                if(menuItem.CurrentIndex > menuItem.Items.Count - 1)
                                {
                                    menuItem.CurrentIndex = 0;
                                }
                                break;
                        }

                        try
                        {
                            var newItem = menuItem.Items[menuItem.CurrentIndex];
                            menuItem.OnSelect?.Invoke(menuItem, selectType, newItem);
                            menuItem.OnUpdate(_uiService);
                        }
                        catch(Exception ex)
                        {
                            Logger.Error("Ex: " + ex.StackTrace);
                        }
                        break;
                    case SliderItem menuItem:
                        value = args["value"];
                        menuItem.Value = value;
                        menuItem.OnInput?.Invoke(menuItem, value);
                        break;
                    case CheckboxItem menuItem:
                        var isChecked = (bool)args["isChecked"];
                        menuItem.IsChecked = isChecked;
                        menuItem.OnChange?.Invoke(menuItem, isChecked);
                        break;
                }
            }

            if (isSecondary)
            {
                var secondaryItem = _menuService.currentMenu.GetSecondaryItem(id);

                switch (secondaryItem)
                {
                    case BottomButtonItem menuItem:
                        menuItem.OnClick?.Invoke(menuItem);
                        break;
                }
            }

            return cb;
        }

        [UICallback("menu/keydown")]
        private CallbackDelegate OnKeydown(IDictionary<string, object> args, CallbackDelegate cb)
        {
            var key = int.Parse(args["key"].ToString());

            if (_menuService.IsOpen && key == 27)
            {
                Logger.Error("Histories: " + _menuService.histories.Count);

                if (_menuService.histories.Count > 0)
                {
                    var currentGroupIndex = _menuService.histories.FindIndex(x => x.Id == _menuService.currentMenu.Id);

                    Logger.Error("CurrentGroupIndex: " + currentGroupIndex + ", " + _menuService.histories.Count);

                    if (currentGroupIndex > 0)
                    {
                        var parent = _menuService.histories[currentGroupIndex - 1];

                        Logger.Error("Parent: " + parent.Id + ", " + parent.BannerTitle);

                        _menuService.Open(parent);
                        _menuService.histories.RemoveAt(currentGroupIndex);
                    }
                    else
                    {
                        _menuService.Close();
                        _uiService.Unfocus();
                    }
                }
                else
                {
                    _menuService.Close();
                    _uiService.Unfocus();
                }
            }

            return cb;
        }
    }
}
