// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    public class UrlEnumHelpers
    {
        public static BrowserTypes FromString(string value)
        {
            var result = BrowserTypes.Normal;
            switch (value.ToLowerInvariant())
            {
                case "mobile":
                    result = BrowserTypes.Mobile;
                    break;
            }

            return result;
        }
    }
}
