using Newtonsoft.Json;

namespace Average.Client.Framework.Extensions
{
    public static class ObjectExtentions
    {
        public static T Convert<T>(this object source) => JsonConvert.DeserializeObject<T>(source.ToString());
    }
}
