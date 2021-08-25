using SDK.Client;
using SDK.Client.Interfaces;
using System.Collections.Generic;

namespace Average.Client.Managers
{
    public class LanguageManager : ILanguageManager
    {
        private readonly Dictionary<string, string> _language;
        
        public string Current { get; private set; }

        public LanguageManager()
        {
            var config = Configuration.Parse("config.json");
            Current = (string)config["Language"];
            _language = Configuration.ParseToDictionary($"languages/{Current}.json");
        }

        bool KeyExist(string key) => _language.ContainsKey(key);

        public string Get(string key) => KeyExist(key) ? _language[key] : string.Empty;
    }
}
