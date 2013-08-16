using System;
using System.Collections.Generic;

namespace DotNetNuke.Common.Utilities
{
    public static class EnumExtensions
    {
        public static List<KeyValuePair<int, string>> ToKeyValuePairs(this Enum enumType)
        {
            var pairs = new List<KeyValuePair<int, string>>();
            
            var names = Enum.GetNames(enumType.GetType());
            var values = Enum.GetValues(enumType.GetType());
            for (var i = 0; i < values.Length; i++)
            {
                pairs.Add(new KeyValuePair<int, string>((int) values.GetValue(i), names[i]));
            }
            return pairs;
        }
    }
}
