
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Specialized;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Entities.Urls
{
    public abstract class FriendlyUrlProviderBase
    {
        protected UrlFormatType UrlFormat { get; private set; }

        internal FriendlyUrlProviderBase(NameValueCollection attributes)
        {
            if (!string.IsNullOrEmpty(attributes["urlFormat"]))
            {
                switch (attributes["urlFormat"].ToLowerInvariant())
                {
                    case "searchfriendly":
                        this.UrlFormat = UrlFormatType.SearchFriendly;
                        break;
                    case "humanfriendly":
                        this.UrlFormat = UrlFormatType.HumanFriendly;
                        break;
                    case "advanced":
                    case "customonly":
                        this.UrlFormat = UrlFormatType.Advanced;
                        break;
                    default:
                        this.UrlFormat = UrlFormatType.SearchFriendly;
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
