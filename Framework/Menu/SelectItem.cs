﻿using Average.Client.Framework.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Average.Shared.SharedAPI;

namespace Average.Client.Framework.Menu
{
    internal class SelectItem : IPrimaryMenuItem
    {
        public string Id { get; private set; }
        public string Text { get; set; }
        public bool Visible { get; set; }
        public List<object> Items { get; set; }
        public int CurrentIndex { get; set; }

        internal enum SelectType
        {
            Previous,
            Next
        }

        [JsonIgnore]
        public Action<SelectItem, SelectType, object> OnSelect { get; }

        public SelectItem(string text, List<object> items, Action<SelectItem, SelectType, object> onSelect, int defaultItemIndex = 0, bool visible = true)
        {
            Id = RandomString();
            Text = text;
            Items = items;
            CurrentIndex = defaultItemIndex;
            OnSelect = onSelect;
            Visible = visible;
        }

        public object OnRender() => new
        {
            type = GetType().Name,
            id = Id,
            text = Text,
            item = Items[CurrentIndex],
            visible = Visible
        };

        public void OnUpdate(UIService uiService) => uiService.SendNui("menu", "render_item", OnRender());
    }
}
