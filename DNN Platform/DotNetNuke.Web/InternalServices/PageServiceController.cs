// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    [DnnAuthorize]
    [DnnPageEditor]
    public class PageServiceController : DnnApiController
    {
        private int? _portalId;

        protected int PortalId
        {
            get
            {
                if (!this._portalId.HasValue)
                {
                    this._portalId = this.PortalSettings.ActiveTab.IsSuperTab ? -1 : this.PortalSettings.PortalId;
                }

                return this._portalId.Value;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPagePermission]
        public HttpResponseMessage PublishPage(PublishPageDto dto)
        {
            var tabId = this.Request.FindTabId();

            TabPublishingController.Instance.SetTabPublishing(tabId, this.PortalId, dto.Publish);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateCustomUrl(SaveUrlDto dto)
        {
            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            bool modified;

            // Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(this.PortalSettings.PortalId)));

            // now clean the path
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
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

    public class PublishPageDto
    {
        public bool Publish { get; set; }
    }
}
