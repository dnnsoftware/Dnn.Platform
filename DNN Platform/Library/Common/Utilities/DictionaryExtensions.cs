// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;

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
