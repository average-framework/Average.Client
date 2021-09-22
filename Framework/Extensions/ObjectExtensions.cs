using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Average.Client.Framework.Extensions
{
    public static class ObjectExtentions
    {
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
    }
}
