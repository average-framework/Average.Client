using SDK.Client;
using System.Collections.Generic;

namespace Average.Client.Managers
{
    public class LanguageManager
    {
        Dictionary<string, string> language = new Dictionary<string, string>();
        public string Current { get; private set; }

        public LanguageManager() => Load();

        void Load()
        {
            var config = Configuration.Parse("config.json");
            Current = (string)config["Language"];
            language = Configuration.ParseToDictionary($"languages/{Current}.json");
        }

        bool KeyExist(string key) => language.ContainsKey(key);

        public string Get(string key) => KeyExist(key) ? language[key] : string.Empty;
    }
}
