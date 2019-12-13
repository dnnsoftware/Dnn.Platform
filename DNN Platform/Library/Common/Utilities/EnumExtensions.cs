#region Usings
using System;
using System.Collections.Generic;
#endregion

namespace DotNetNuke.Common.Utilities
{
    /// <summary>
    /// Extensions for enumeration of KeyValue Paire
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// To the key value pairs.
        /// </summary>
        /// <param name="enumType">Type of the enum defined by GetType.</param>
        /// <returns>A list of Key Value pairs</returns>
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
