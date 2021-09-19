using System;
using System.Collections.Generic;
using System.Linq;

namespace Average.Client.Framework.Helpers
{
    public static class ConversionHelpers
    {
        public static Dictionary<string, uint> ConvertToUintDictionary(Dictionary<string, object> data)
        {
            var temp = new Dictionary<string, uint>();
            data.ToList().ForEach(x => temp.Add(x.Key, Convert.ToUInt32(x.Value)));
            return temp;
        }
    }
}
