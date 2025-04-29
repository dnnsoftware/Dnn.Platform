// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

/// <summary>An HTTP handler for generating an RSS feed.</summary>
public class SyndicationHandlerBase : GenericRssHttpHandlerBase
{
    private int moduleId = Null.NullInteger;

    private int tabId = Null.NullInteger;

    /// <summary>Gets the portal settings.</summary>
    public PortalSettings Settings => Globals.GetPortalSettings();

    /// <summary>Gets the tab ID of the request.</summary>
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

    /// <summary>Gets the module ID of the request.</summary>
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

    /// <summary>Gets the HTTP request.</summary>
    public HttpRequest Request => HttpContext.Current.Request;
}
