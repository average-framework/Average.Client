using Average.Shared.DataModels;
using Average.Shared.Models;
using Newtonsoft.Json;
using System;

namespace Average.Client.Framework.Storage
{
    internal class StorageContextItem
    {
        public string Text { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string EventName { get; set; }

        [JsonIgnore]
        public Action<StorageData, StorageItemData, RaycastHit> Action { get; set; }
    }
}