using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Average.Client.Framework.Extensions
{
    internal static class StringExtensions
    {
        internal static JObject ToJObject(this string json)
        {
            return JObject.Parse(json);
        }

        internal static JArray ToJArray(this string json)
        {
            return JArray.Parse(json);
        }

        internal static T DeserializeJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
