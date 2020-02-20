// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Specialized;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public abstract class FriendlyUrlProviderBase
    {
        protected UrlFormatType UrlFormat { get; private set; }

        internal FriendlyUrlProviderBase(NameValueCollection attributes)
        {
            if (!String.IsNullOrEmpty(attributes["urlFormat"]))
            {
                switch (attributes["urlFormat"].ToLowerInvariant())
                {
                    case "searchfriendly":
                        UrlFormat = UrlFormatType.SearchFriendly;
                        break;
                    case "humanfriendly":
                        UrlFormat = UrlFormatType.HumanFriendly;
                        break;
                    case "advanced":
                    case "customonly":
                        UrlFormat = UrlFormatType.Advanced;
                        break;
                    default:
                        UrlFormat = UrlFormatType.SearchFriendly;
                        break;
                }
            }
        }

        internal abstract string FriendlyUrl(TabInfo tab, string path);

        internal abstract string FriendlyUrl(TabInfo tab, string path, string pageName);

        internal abstract string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings portalSettings);

        internal abstract string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias);
    }
}
