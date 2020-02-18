// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Tests.Urls
{
    public static class Extensions
    {
        public static string GetValue(this Dictionary<string, string> dic, string key)
        {
            return GetValue(dic, key, String.Empty);
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
