// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    public class SyndicationHandlerBase : GenericRssHttpHandlerBase
    {
        private int _moduleId = Null.NullInteger;

        private int _tabId = Null.NullInteger;

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
                if (this._tabId == Null.NullInteger && this.Request.QueryString["tabid"] != null)
                {
                    if (!int.TryParse(this.Request.QueryString["tabid"], out this._tabId))
                    {
                        this._tabId = Null.NullInteger;
                    }
                }

                return this._tabId;
            }
        }

        public int ModuleId
        {
            get
            {
                if (this._moduleId == Null.NullInteger && this.Request.QueryString["moduleid"] != null)
                {
                    if (!int.TryParse(this.Request.QueryString["moduleid"], out this._moduleId))
                    {
                        this._moduleId = Null.NullInteger;
                    }
                }

                return this._moduleId;
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
