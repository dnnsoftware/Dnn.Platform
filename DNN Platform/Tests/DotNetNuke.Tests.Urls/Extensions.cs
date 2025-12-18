// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static string GetValue(this Dictionary<string, string> dic, string key)
        {
            return GetValue(dic, key, string.Empty);
        }

        public static string GetValue(this Dictionary<string, string> dic, string key, string defaultValue)
        {
            var returnValue = defaultValue;
            if (dic.ContainsKey(key))
            {
                returnValue = dic[key];
            }

            return returnValue;
        }
    }
}
