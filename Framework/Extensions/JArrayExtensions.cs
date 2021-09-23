using Average.Client.Framework.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Average.Client.Framework.Extensions
{
    internal static class JArrayExtensions
    {
        internal static List<T> ToList<T>(this JArray array)
        {
            return array.Cast<T>().ToList();
        }
    }
}
