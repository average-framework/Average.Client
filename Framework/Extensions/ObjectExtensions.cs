using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Average.Client.Framework.Extensions
{
    internal static class ObjectExtensions
    {
        internal static string ToJson(this object source)
        {
            return JsonConvert.SerializeObject(source, Formatting.None);
        }

        internal static T Deserialize<T>(this object source)
        {
            if (source.GetType() == typeof(JArray))
            {
                return ((JArray)source).ToObject<T>();
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(source.ToString());
            }
            catch
            {
                try
                {
                    return (T)System.Convert.ChangeType(source, typeof(T));
                }
                catch
                {

                }
            }

            return default;
        }

        internal static T ToType<T>(this object source) => JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(source), (T)Activator.CreateInstance(typeof(T)));
    }
}
