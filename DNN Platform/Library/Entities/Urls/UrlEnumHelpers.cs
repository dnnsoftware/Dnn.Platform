// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
