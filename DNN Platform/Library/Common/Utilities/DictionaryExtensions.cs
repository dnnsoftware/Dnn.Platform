﻿using System;
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

        public static int GetValue(this IDictionary bag, string key, int defaultValue)
        {
            int value;
            if (bag[key] != null && int.TryParse(bag[key].ToString(), out value))
            {
                return value;
            }
            return defaultValue;
        }

        public static bool GetValue(this IDictionary bag, string key, bool defaultValue)
        {
            var value = (bag[key] ?? defaultValue).ToString();
            switch (value.ToLower())
            {
                case "true":
                case "on":
                case "1":
                case "yes":
                    return true;
                default:
                    return false;
            }
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
