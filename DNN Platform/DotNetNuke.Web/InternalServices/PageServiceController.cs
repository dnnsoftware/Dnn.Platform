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

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    [DnnPageEditor]
    public class PageServiceController : DnnApiController
    {
        private int? _portalId;
        protected int PortalId
        {
            get
            {
                if (!_portalId.HasValue)
                {
                    _portalId = PortalSettings.ActiveTab.IsSuperTab ? -1 : PortalSettings.PortalId;
                }
                return _portalId.Value;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPagePermission]
        public HttpResponseMessage PublishPage(PublishPageDto dto)
        {
            var tabId = Request.FindTabId();
            
            TabPublishingController.Instance.SetTabPublishing(tabId, PortalId, dto.Publish);
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateCustomUrl(SaveUrlDto dto)
        {
            var urlPath = dto.Path.ValueOrEmpty().TrimStart('/');
            bool modified;
            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs( UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(PortalSettings.PortalId)) );
            
            //now clean the path
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("CustomUrlPathCleaned.Error", Localization.GlobalResourceFile),
                        SuggestedUrlPath = "/" + urlPath
                    });
            }

            //Validate for uniqueness
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, -1, PortalSettings, out modified);
            if (modified)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Success = false,
                        ErrorMessage = Localization.GetString("UrlPathNotUnique.Error", Localization.GlobalResourceFile),
                        SuggestedUrlPath = "/" + urlPath
                    });
            }

            var tab = PortalSettings.ActiveTab;
            var cultureCode = LocaleController.Instance.GetLocales(PortalId)
                                    .Where(l => l.Value.KeyID == dto.LocaleKey)
                                    .Select(l => l.Value.Code)
                                    .SingleOrDefault();

            if (dto.StatusCodeKey.ToString(CultureInfo.InvariantCulture) == "200")
            {
                //We need to check if we are updating a current url or creating a new 200
                var tabUrl = tab.TabUrls.SingleOrDefault(t => t.SeqNum == dto.Id
                                                                && t.HttpStatus == "200");
                if (tabUrl == null)
                {
                    //Just create Url
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
                                        IsSystem = dto.IsSystem // false
                                    };
                    TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
                }
                else
                {
                    //Change the original 200 url to a redirect
                    tabUrl.HttpStatus = "301";
                    tabUrl.SeqNum = dto.Id;
                    TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);

                    //Add new custom url
                    tabUrl.Url = dto.Path.ValueOrEmpty();
                    tabUrl.HttpStatus = "200";
                    tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                    TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
                }
            }
            else
            {
                //Just update the url
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
                                        IsSystem = dto.IsSystem // false
                                    };
                TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
            }


            var response = new
            {
                Success = true,
            };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }

    public class PublishPageDto
    {
        public bool Publish { get; set; }
    }
}
