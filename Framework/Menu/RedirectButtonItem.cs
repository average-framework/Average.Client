﻿using Average.Client.Framework.Interfaces;
using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class RedirectButtonItem : IPrimaryMenuItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public bool Disabled { get; set; }

        [JsonIgnore]
        public MenuContainer MenuTarget { get; set; }

        [JsonIgnore]
        public Action<RedirectButtonItem> OnClick { get; }

        public RedirectButtonItem(string text, MenuContainer menuTarget, Action<RedirectButtonItem> onClick, bool visible = true, bool disabled = false)
        {
            Id = RandomString();
            Text = text;
            MenuTarget = menuTarget;
            OnClick = onClick;
            Visible = visible;
            Disabled = disabled;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            visible = Visible,
            disabled = Disabled
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}