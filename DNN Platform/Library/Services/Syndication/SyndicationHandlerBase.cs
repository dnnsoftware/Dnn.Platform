// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    public class SyndicationHandlerBase : GenericRssHttpHandlerBase
    {
        private int moduleId = Null.NullInteger;

        private int tabId = Null.NullInteger;

        public PortalSettings Settings
        {
            get
            {
                return Globals.GetPortalSettings();
            }
        }

        public int TabId
        {
            get
            {
                if (this.tabId == Null.NullInteger && this.Request.QueryString["tabid"] != null)
                {
                    if (!int.TryParse(this.Request.QueryString["tabid"], out this.tabId))
                    {
                        this.tabId = Null.NullInteger;
                    }
                }

                return this.tabId;
            }
        }

        public int ModuleId
        {
            get
            {
                if (this.moduleId == Null.NullInteger && this.Request.QueryString["moduleid"] != null)
                {
                    if (!int.TryParse(this.Request.QueryString["moduleid"], out this.moduleId))
                    {
                        this.moduleId = Null.NullInteger;
                    }
                }

                return this.moduleId;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }
    }
}
