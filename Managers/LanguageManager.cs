using SDK.Client;
using SDK.Client.Interfaces;
using System.Collections.Generic;

namespace Average.Client.Managers
{
    public class LanguageManager : InternalPlugin, ILanguageManager
    {
        private readonly Dictionary<string, string> _language;
        
        public string Current { get; }

        public LanguageManager()
        {
            var config = Configuration.ParseToObject("config.json");
            Current = (string)config["Language"];
            _language = Configuration.ParseToDictionary($"languages/{Current}.json");
        }

        private bool KeyExist(string key) => _language.ContainsKey(key);

        public string Get(string key) => KeyExist(key) ? _language[key] : string.Empty;
    }
}
