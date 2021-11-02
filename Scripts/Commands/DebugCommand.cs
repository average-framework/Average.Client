using Average.Client.Framework.Attributes;
using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Menu;
using Average.Client.Framework.Services;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;
using static Average.Client.Framework.GameAPI;
using Average.Shared.Enums;

namespace Average.Client.Scripts.Commands
{
    internal class DebugCommand : ICommand
    {
        private readonly CharacterService _characterService;
        private readonly MenuService _menuService;
        private readonly UIService _uiService;
        private readonly WorldService _worldService;

        private readonly TopContainer topContainer = new();
        private readonly BottomContainer bottomContainer = new();
        private readonly StatsMenuInfo middleContainer;

        private readonly MenuContainer testMenu;
        private readonly MenuContainer twoMenu;

        public DebugCommand(CharacterService characterService, MenuService menuService, UIService uiService, WorldService worldService)
        {
            _characterService = characterService;
            _menuService = menuService;
            _uiService = uiService;
            _worldService = worldService;

            topContainer.AddItem(new ButtonItem("Enculer 2", (item) =>
            {
                Logger.Error("Je suis un enculer");
                item.Text = "Je ne suis pas un enculer";
                item.OnUpdate(_uiService);
            }));

            twoMenu = new MenuContainer(new TopContainer(), new BottomContainer(), new StatsMenuInfo());
            twoMenu.BannerTitle = "Enculer";

            topContainer.AddItem(new RedirectButtonItem("Banane", twoMenu, (item) =>
            {
                Logger.Error("Je suis une banane");
                item.Text = "Je ne suis pas une banane";
                item.OnUpdate(_uiService);
            }));

            topContainer.AddItem(new StoreButtonItem("Carabine", 14.57m, (item) =>
            {
                Logger.Error("Payer: $" + item.Amount);
                //item.Text = "Je ne suis pas une banane";
                item.OnUpdate(_uiService);
            }));

            topContainer.AddItem(new TextboxItem("Firstname", 5, 20, "Prénom", "", (item, value) =>
            {
                Logger.Error("Value: " + item.Value + ", " + value);
                //item.Text = "Je ne suis pas une banane";
                item.Disabled = true;
                item.OnUpdate(_uiService);
            }));

            topContainer.AddItem(new Vector2Item("Informations", new Vector2Input(5, 20, "Prénom", ""), new Vector2Input(5, 20, "Nom", ""), (item, primaryValue, secondaryValue) => 
            {
                item.Text = (string)primaryValue + " " + (string)secondaryValue;
                item.OnUpdate(_uiService);
            }));

            topContainer.AddItem(new Vector3Item("Informations 2", new Vector3Input(5, 20, "Prénom", ""), new Vector3Input(5, 20, "Nom", ""), new Vector3Input(5, 20, "Mdrrr", "Cuillere"), (item, primaryValue, secondaryValue, tertiaryValue) =>
            {
                item.Text = (string)primaryValue + " " + (string)secondaryValue + " " + (string)tertiaryValue;
                item.OnUpdate(_uiService);
            }));

            var genders = new List<object> { "Homme", "Femme", "Autres?" };

            topContainer.AddItem(new SelectItem("Genre", genders, (item, type, value) => 
            {
                Logger.Error("Select: " + type + ", " + value);
                item.Text = "Sel: " + value;
                item.OnUpdate(_uiService);
            }, 0));

            topContainer.AddItem(new SliderItem("Offset", 0.1, 1.0, 0.1, 0.0, typeof(int), (item, value) =>
            {
                Logger.Error("Value: " + item.Value + ", " + value);
                item.Text = "Offset: " + value;
                item.OnUpdate(_uiService);
            }));

            topContainer.AddItem(new CheckboxItem("Offset", true, (item, isChecked) =>
            {
                Logger.Error("Value: " + item.IsChecked + ", " + isChecked);
                item.Text = "Checked ? : " + isChecked;
                item.OnUpdate(_uiService);
            }));

            topContainer.AddItem(new SelectSliderItem("Offset 2", 0, 100, 20, 0, typeof(int), (item, selectType, value) =>
            {
                Logger.Error("Select: " + selectType + ", " + value + ", " + value.GetType().Name);
            }));

            middleContainer = new StatsMenuInfo();
            middleContainer.AddItem(new StatItem("Vie", StatBarType.Five, 0, 100, 35));

            var label = new LabelItem("0.00");
            bottomContainer.AddItem(label);

            bottomContainer.AddItem(new BottomButtonItem("Acheter", (item) =>
            {
                Logger.Error("Acheter");
                middleContainer.Items[0].Label = "Enculer";
                middleContainer.Items[0].Value = 75;
                middleContainer.OnUpdate(_uiService);

                label.Text = "75";
                label.OnUpdate(_uiService);
            }));

            testMenu = new MenuContainer(topContainer, bottomContainer, middleContainer);
            testMenu.BannerTitle = "Enculer 1";
        }

        [ClientCommand("debug.gotow")]
        private async void OnGotow()
        {
            var waypointCoords = GetWaypointCoords();
            await _characterService.Teleport(PlayerPedId(), waypointCoords);
        }

        [ClientCommand("debug.open")]
        private async void OnOpen()
        {
            Logger.Error("On Open: " + testMenu.BannerTitle);
            _menuService.Open(testMenu);
            _uiService.FocusFrame("menu");
            _uiService.Focus();
        }

        [ClientCommand("debug.close")]
        private async void OnClose()
        {
            Logger.Error("On Close");
            _menuService.Close();
            _uiService.Unfocus();
        }

        [ClientCommand("debug.snow")]
        private void OnSnow()
        {
            _worldService.SetWeather((uint)Weather.Snowlight, 0);

            Call(0xF02A9C330BBFC5C7, 3);
            Call(0xF6BEE7E80EC5CA40, 4000f);
            Call((uint)GetHashKey("FORCE_SNOW_PASS"), true);
        }
    }
}
