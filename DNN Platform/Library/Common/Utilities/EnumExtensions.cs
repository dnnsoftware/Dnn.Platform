// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for enumeration of KeyValue Paire.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// To the key value pairs.
        /// </summary>
        /// <param name="enumType">Type of the enum defined by GetType.</param>
        /// <returns>A list of Key Value pairs.</returns>
        public static List<KeyValuePair<int, string>> ToKeyValuePairs(this Enum enumType)
        {
            var pairs = new List<KeyValuePair<int, string>>();

            var names = Enum.GetNames(enumType.GetType());
            var values = Enum.GetValues(enumType.GetType());
            for (var i = 0; i < values.Length; i++)
            {
                pairs.Add(new KeyValuePair<int, string>((int)values.GetValue(i), names[i]));
            }

            return pairs;
        }
    }
}
