using Average.Client.Framework.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Average.Client
{
    internal static class Configuration
    {
        internal static JObject ParseToObject(string filePath)
        {
            try
            {
                var json = LoadResourceFile(GetCurrentResourceName(), filePath);
                return JObject.Parse(json);
            }
            catch
            {
                Logger.Error($"[Configuration] Unable to find or convert this file: {filePath}");
            }

            return null;
        }

        internal static dynamic ParseToDynamic(string filePath)
        {
            try
            {
                var json = LoadResourceFile(GetCurrentResourceName(), filePath);
                return JsonConvert.DeserializeObject(json);
            }
            catch
            {
                Logger.Error($"[Configuration] Unable to find or convert this file: {filePath}");
            }

            return null;
        }

        internal static Dictionary<string, string> ParseToDictionary(string filePath)
        {
            try
            {
                var json = LoadResourceFile(GetCurrentResourceName(), filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch
            {
                Logger.Error($"[Configuration] Unable to find or convert this file: {filePath}");
            }

            return null;
        }

        internal static JArray ParseToArray(string filePath)
        {
            try
            {
                var json = LoadResourceFile(GetCurrentResourceName(), filePath);
                return JArray.Parse(json);
            }
            catch
            {
                Logger.Error($"[Configuration] Unable to find or convert this file: {filePath}");
            }

            return null;
        }

        internal static T Parse<T>(string filePath)
        {
            try
            {
                var json = LoadResourceFile(GetCurrentResourceName(), filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                Logger.Error($"[Configuration] Unable to find or convert this file: {filePath}");
            }

            return default;
        }
    }
}
