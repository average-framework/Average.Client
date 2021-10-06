using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Average.Client.Framework.Extensions
{
    internal static class ObjectExtensions
    {
        internal static string ToJson(this object source, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(source, formatting);
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

        internal static InputArgument ToInputArgument(this object source)
        {
            InputArgument val = null;

            switch (source)
            {
                case bool:
                    val = (bool)Convert.ChangeType(source, typeof(bool));
                    break;
                case sbyte:
                    val = (sbyte)Convert.ChangeType(source, typeof(sbyte));
                    break;
                case byte:
                    val = (byte)Convert.ChangeType(source, typeof(byte));
                    break;
                case short:
                    val = (short)Convert.ChangeType(source, typeof(short));
                    break;
                case ushort:
                    val = (ushort)Convert.ChangeType(source, typeof(ushort));
                    break;
                case int:
                    val = (int)Convert.ChangeType(source, typeof(int));
                    break;
                case uint:
                    val = (uint)Convert.ChangeType(source, typeof(uint));
                    break;
                case long:
                    val = (long)Convert.ChangeType(source, typeof(long));
                    break;
                case ulong:
                    val = (ulong)Convert.ChangeType(source, typeof(ulong));
                    break;
                case float:
                    val = (float)Convert.ChangeType(source, typeof(float));
                    break;
                case double:
                    val = (double)Convert.ChangeType(source, typeof(double));
                    break;
                case Enum:
                    val = (Enum)Convert.ChangeType(source, typeof(Enum));
                    break;
                case string:
                    val = (string)Convert.ChangeType(source, typeof(string));
                    break;
                case Vector3:
                    val = (Vector3)Convert.ChangeType(source, typeof(Vector3));
                    break;
            }

            return val;
        }
    }
}
