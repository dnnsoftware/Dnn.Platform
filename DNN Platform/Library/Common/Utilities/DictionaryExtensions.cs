using System;
using System.Collections;

namespace DotNetNuke.Common.Utilities
{
    public static class DictionaryExtensions
    {
        public static T GetValue<T>(this IDictionary bag, object key, T defaultValue)
        {
            var value = bag[key] ?? defaultValue;
            return (T)value;
        }

        public static void SetValue<T>(this IDictionary bag, object key, T value, T defaultValue)
        {
            if (Equals(defaultValue, value))
            {
                bag.Remove(key);
            }
            else
            {
                bag[key] = value;
            }
        }
    }
}
