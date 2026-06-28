// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A web API controller for pages.</summary>
[DnnAuthorize]
[DnnPageEditor]
public partial class PageServiceController : DnnApiController
{
    private readonly IPortalController portalController;
    private readonly IHostSettings hostSettings;
    private readonly IHostSettingsService hostSettingsService;
    private int? portalId;

    /// <summary>Initializes a new instance of the <see cref="PageServiceController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public PageServiceController()
        : this(null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PageServiceController"/> class.</summary>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    public PageServiceController(IPortalController portalController, IHostSettings hostSettings, IHostSettingsService hostSettingsService)
    {
        this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
    }

    /// <summary>Gets the portal ID.</summary>
    protected int PortalId
    {
        get
        {
            if (!this.portalId.HasValue)
            {
                this.portalId = this.PortalSettings.ActiveTab.IsSuperTab ? -1 : this.PortalSettings.PortalId;
            }

            return this.portalId.Value;
        }
    }

    /// <summary>Publishes a page.</summary>
    /// <param name="dto">The publish request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnPagePermission]
    [DnnDeprecated(10, 3, 3, "Use overload taking PublishPageRequest")]
    public partial HttpResponseMessage PublishPage(PublishPageDto dto)
        => this.PublishPage(dto?.ToPublishPageRequest());

    /// <summary>Publishes a page.</summary>
    /// <param name="requestBody">The publish request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnPagePermission]
    public HttpResponseMessage PublishPage(PublishPageRequest requestBody)
    {
        var tabId = this.Request.FindTabId();

        TabPublishingController.Instance.SetTabPublishing(tabId, this.PortalId, requestBody.Publish);

        return this.Request.CreateResponse(HttpStatusCode.OK);
    }

    /// <summary>Updates a custom URL for a page.</summary>
    /// <param name="dto">The update request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage UpdateCustomUrl(SaveUrlDto dto)
    {
        var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');

        // Clean Url
        var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(this.portalController, this.hostSettings, this.hostSettingsService, this.PortalSettings.PortalId)));

        // now clean the path
        urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out var modified);
        if (modified)
        {
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error", Localization.GlobalResourceFile),
                    SuggestedUrlPath = "/" + urlPath,
                });
        }

        // Validate for uniqueness
        urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, this.PortalSettings, out modified);
        if (modified)
        {
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    Success = false,
                    ErrorMessage = Localization.GetString("UrlPathNotUnique.Error", Localization.GlobalResourceFile),
                    SuggestedUrlPath = "/" + urlPath,
                });
        }

        var tab = this.PortalSettings.ActiveTab;
        var cultureCode = LocaleController.Instance.GetLocales(this.PortalId)
            .Where(l => l.Value.KeyID == dto.LocaleKey)
            .Select(l => l.Value.Code)
            .SingleOrDefault();

        if (dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture) == "200")
        {
            // We need to check if we are updating a current url or creating a new 200
            var tabUrl = tab.TabUrls.SingleOrDefault(t => t.SeqNum == dto.Id
                                                          && t.HttpStatus == "200");
            if (tabUrl == null)
            {
                // Just create Url
                tabUrl = new TabUrlInfo
                {
                    TabId = tab.TabID,
                    SeqNum = dto.Id,
                    PortalAliasId = dto.SiteAliasKey,
                    PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                    QueryString = dto.QueryString.ValueOrEmpty(),
                    Url = dto.Path.ValueOrEmpty(),
                    CultureCode = cultureCode,
                    HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                    IsSystem = dto.IsSystem, // false
                };
                TabController.Instance.SaveTabUrl(tabUrl, this.PortalId, true);
            }
            else
            {
                // Change the original 200 url to a redirect
                tabUrl.HttpStatus = "301";
                tabUrl.SeqNum = dto.Id;
                TabController.Instance.SaveTabUrl(tabUrl, this.PortalId, true);

                // Add new custom url
                tabUrl.Url = dto.Path.ValueOrEmpty();
                tabUrl.HttpStatus = "200";
                tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                TabController.Instance.SaveTabUrl(tabUrl, this.PortalId, true);
            }
        }
        else
        {
            // Just update the url
            var tabUrl = new TabUrlInfo
            {
                TabId = tab.TabID,
                SeqNum = dto.Id,
                PortalAliasId = dto.SiteAliasKey,
                PortalAliasUsage = (PortalAliasUsageType)dto.SiteAliasUsage,
                QueryString = dto.QueryString.ValueOrEmpty(),
                Url = dto.Path.ValueOrEmpty(),
                CultureCode = cultureCode,
                HttpStatus = dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture),
                IsSystem = dto.IsSystem, // false
            };
            TabController.Instance.SaveTabUrl(tabUrl, this.PortalId, true);
        }

        var response = new
        {
            Success = true,
        };

        return this.Request.CreateResponse(HttpStatusCode.OK, response);
    }
}
