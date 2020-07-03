// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Seo.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Seo.Components;
    using Dnn.PersonaBar.Seo.Services.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Sitemap;
    using DotNetNuke.Services.Url.FriendlyUrl;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = "Dnn.Seo")]
    public class SeoController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SeoController));
        private static readonly string LocalResourcesFile = Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Seo/App_LocalResources/Seo.resx");
        private readonly Components.SeoController _controller = new Components.SeoController();

        public SeoController(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;
        }

        protected INavigationManager NavigationManager { get; }

        /// GET: api/SEO/GetGeneralSettings
        /// <summary>
        /// Gets general SEO settings.
        /// </summary>
        /// <returns>General SEO settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetGeneralSettings()
        {
            try
            {
                var urlSettings = new FriendlyUrlSettings(this.PortalId);

                var replacementCharacterList = new List<KeyValuePair<string, string>>();
                replacementCharacterList.Add(new KeyValuePair<string, string>(Localization.GetString("minusCharacter", LocalResourcesFile), "-"));
                replacementCharacterList.Add(new KeyValuePair<string, string>(Localization.GetString("underscoreCharacter", LocalResourcesFile), "_"));

                var deletedPageHandlingTypes = new List<KeyValuePair<string, string>>();
                deletedPageHandlingTypes.Add(new KeyValuePair<string, string>(Localization.GetString("Do404Error", LocalResourcesFile), "Do404Error"));
                deletedPageHandlingTypes.Add(new KeyValuePair<string, string>(Localization.GetString("Do301RedirectToPortalHome", LocalResourcesFile), "Do301RedirectToPortalHome"));

                var response = new
                {
                    Success = true,
                    Settings = new
                    {
                        EnableSystemGeneratedUrls = urlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing,
                        urlSettings.ReplaceSpaceWith,
                        urlSettings.ForceLowerCase,
                        urlSettings.AutoAsciiConvert,
                        urlSettings.ForcePortalDefaultLanguage,
                        DeletedTabHandlingType = urlSettings.DeletedTabHandlingType.ToString(),
                        urlSettings.RedirectUnfriendly,
                        urlSettings.RedirectWrongCase
                    },
                    ReplacementCharacterList = replacementCharacterList,
                    DeletedPageHandlingTypes = deletedPageHandlingTypes
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateGeneralSettings
        /// <summary>
        /// Updates SEO general settings.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateGeneralSettings(UpdateGeneralSettingsRequest request)
        {
            try
            {
                string characterSub = FriendlyUrlSettings.ReplaceSpaceWithNothing;
                if (request.EnableSystemGeneratedUrls)
                {
                    characterSub = request.ReplaceSpaceWith;
                }
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.ReplaceSpaceWithSetting, characterSub, false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.DeletedTabHandlingTypeSetting, request.DeletedTabHandlingType, false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.ForceLowerCaseSetting, request.ForceLowerCase ? "Y" : "N", false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.RedirectUnfriendlySetting, request.RedirectUnfriendly ? "Y" : "N", false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.RedirectMixedCaseSetting, request.RedirectWrongCase.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.UsePortalDefaultLanguageSetting, request.ForcePortalDefaultLanguage.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, FriendlyUrlSettings.AutoAsciiConvertSetting, request.AutoAsciiConvert.ToString(), false);

                DataCache.ClearPortalCache(this.PortalId, false);
                DataCache.ClearTabsCache(this.PortalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetRegexSettings
        /// <summary>
        /// Gets SEO regex settings.
        /// </summary>
        /// <returns>General SEO regex settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetRegexSettings()
        {
            try
            {
                var urlSettings = new FriendlyUrlSettings(this.PortalId);

                var response = new
                {
                    Success = true,
                    Settings = new
                    {
                        urlSettings.IgnoreRegex,
                        urlSettings.DoNotRewriteRegex,
                        urlSettings.UseSiteUrlsRegex,
                        urlSettings.DoNotRedirectRegex,
                        urlSettings.DoNotRedirectSecureRegex,
                        urlSettings.ForceLowerCaseRegex,
                        urlSettings.NoFriendlyUrlRegex,
                        urlSettings.DoNotIncludeInPathRegex,
                        urlSettings.ValidExtensionlessUrlsRegex,
                        urlSettings.RegexMatch
                    }
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateRegexSettings
        /// <summary>
        /// Updates SEO regex settings.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateRegexSettings(UpdateRegexSettingsRequest request)
        {
            try
            {
                List<KeyValuePair<string, string>> validationErrors = new List<KeyValuePair<string, string>>();
                if (!ValidateRegex(request.IgnoreRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("IgnoreRegex", Localization.GetString("ignoreRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.DoNotRewriteRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotRewriteRegex", Localization.GetString("doNotRewriteRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.UseSiteUrlsRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("UseSiteUrlsRegex", Localization.GetString("siteUrlsOnlyRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.DoNotRedirectRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotRedirectRegex", Localization.GetString("doNotRedirectUrlRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.DoNotRedirectSecureRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotRedirectSecureRegex", Localization.GetString("doNotRedirectHttpsUrlRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.ForceLowerCaseRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("ForceLowerCaseRegex", Localization.GetString("preventLowerCaseUrlRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.NoFriendlyUrlRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("NoFriendlyUrlRegex", Localization.GetString("doNotUseFriendlyUrlsRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.DoNotIncludeInPathRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("DoNotIncludeInPathRegex", Localization.GetString("keepInQueryStringRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.ValidExtensionlessUrlsRegex))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("ValidExtensionlessUrlsRegex", Localization.GetString("urlsWithNoExtensionRegExInvalidPattern", LocalResourcesFile)));
                }
                if (!ValidateRegex(request.RegexMatch))
                {
                    validationErrors.Add(new KeyValuePair<string, string>("RegexMatch", Localization.GetString("validFriendlyUrlRegExInvalidPattern", LocalResourcesFile)));
                }

                if (validationErrors.Count > 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Errors = validationErrors });
                }
                else
                {
                    // if no errors, update settings in db
                    this.UpdateRegexSettingsInternal(request);
                    // clear cache
                    this.ClearCache();

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetSitemapSettings
        /// <summary>
        /// Gets sitemap settings.
        /// </summary>
        /// <param></param>
        /// <returns>Data of sitemap settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetSitemapSettings()
        {
            try
            {
                var portalAlias = !string.IsNullOrEmpty(this.PortalSettings.DefaultPortalAlias)
                                ? this.PortalSettings.DefaultPortalAlias
                                : this.PortalSettings.PortalAlias.HTTPAlias;

                var str = PortalController.GetPortalSetting("SitemapMinPriority", this.PortalId, "0.1");
                float sitemapMinPriority;
                if (!float.TryParse(str, out sitemapMinPriority))
                {
                    sitemapMinPriority = 0.1f;
                }

                str = PortalController.GetPortalSetting("SitemapExcludePriority", this.PortalId, "0.1");
                float sitemapExcludePriority;
                if (!float.TryParse(str, out sitemapExcludePriority))
                {
                    sitemapExcludePriority = 0.1f;
                }

                var settings = new
                {
                    SitemapUrl = Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx",
                    SitemapLevelMode = PortalController.GetPortalSettingAsBoolean("SitemapLevelMode", this.PortalId, false),
                    SitemapMinPriority = sitemapMinPriority,
                    SitemapIncludeHidden = PortalController.GetPortalSettingAsBoolean("SitemapIncludeHidden", this.PortalId, false),
                    SitemapExcludePriority = sitemapExcludePriority,
                    SitemapCacheDays = PortalController.GetPortalSettingAsInteger("SitemapCacheDays", this.PortalId, 1)
                };

                var searchEngineUrls = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Google", this._controller.GetSearchEngineSubmissionUrl("google")),
                    new KeyValuePair<string, string>("Bing", this._controller.GetSearchEngineSubmissionUrl("bing")),
                    new KeyValuePair<string, string>("Yahoo!", this._controller.GetSearchEngineSubmissionUrl("yahoo!"))
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = true,
                    Settings = settings,
                    SearchEngineUrls = searchEngineUrls
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/CreateVerification
        /// <summary>
        /// Creates a verification file for specific search engine.
        /// </summary>
        /// <param name="verification">Name of verification.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateVerification(string verification)
        {
            try
            {
                this._controller.CreateVerification(verification);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateSitemapSettings
        /// <summary>
        /// Updates sitemap settings.
        /// </summary>
        /// <param name="request">Data of sitemap settings.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSitemapSettings(SitemapSettingsRequest request)
        {
            try
            {
                PortalController.UpdatePortalSetting(this.PortalId, "SitemapLevelMode", request.SitemapLevelMode.ToString());

                if (request.SitemapMinPriority < 0)
                {
                    request.SitemapMinPriority = 0;
                }
                PortalController.UpdatePortalSetting(this.PortalId, "SitemapMinPriority", request.SitemapMinPriority.ToString(NumberFormatInfo.InvariantInfo));

                PortalController.UpdatePortalSetting(this.PortalId, "SitemapIncludeHidden", request.SitemapIncludeHidden.ToString());

                if (request.SitemapExcludePriority < 0)
                {
                    request.SitemapExcludePriority = 0;
                }
                PortalController.UpdatePortalSetting(this.PortalId, "SitemapExcludePriority", request.SitemapExcludePriority.ToString(NumberFormatInfo.InvariantInfo));

                if (request.SitemapCacheDays == 0)
                {
                    this._controller.ResetCache();
                }

                PortalController.UpdatePortalSetting(this.PortalId, "SitemapCacheDays", request.SitemapCacheDays.ToString());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/ResetCache
        /// <summary>
        /// Resets cache.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ResetCache()
        {
            try
            {
                this._controller.ResetCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetSitemapProviders
        /// <summary>
        /// Gets list of sitemap providers.
        /// </summary>
        /// <param></param>
        /// <returns>Web Server information.</returns>
        [HttpGet]
        public HttpResponseMessage GetSitemapProviders()
        {
            try
            {
                var providers = this._controller.GetSitemapProviders().Select(p => new
                {
                    p.Name,
                    p.Enabled,
                    Priority = p.Priority / 100f,
                    p.OverridePriority
                }).ToList();

                var response = new
                {
                    Success = true,
                    Providers = providers
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateSitemapProvider
        /// <summary>
        /// Updates settings of a sitemap provider.
        /// </summary>
        /// <param name="request">Data of sitemap provider.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSitemapProvider(UpdateSitemapProviderRequest request)
        {
            try
            {
                SitemapProvider editedProvider =
                    this._controller.GetSitemapProviders()
                        .FirstOrDefault(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

                if (editedProvider != null)
                {
                    editedProvider.Enabled = request.Enabled;
                    editedProvider.OverridePriority = request.Priority > -1;
                    if (editedProvider.OverridePriority)
                    {
                        editedProvider.Priority = request.Priority * 100;
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/GetExtensionUrlProviders
        /// <summary>
        /// Gets list of extension url providers.
        /// </summary>
        /// <param></param>
        /// <returns>extension url providers.</returns>
        [HttpGet]
        public HttpResponseMessage GetExtensionUrlProviders()
        {
            try
            {
                var providers = ExtensionUrlProviderController.GetProviders(this.PortalId).Select(p => new
                {
                    p.ExtensionUrlProviderId,
                    p.ProviderName,
                    p.IsActive,
                    SettingUrl = this.NavigationManager.NavigateURL(this.PortalSettings.AdminTabId, "UrlProviderSettings", "Display=settings&popUp=true&ProviderId=" + p.ExtensionUrlProviderId)
                }).ToList();

                var response = new
                {
                    Success = true,
                    Providers = providers
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SEO/UpdateExtensionUrlProviderStatus
        /// <summary>
        /// Enable or disable extension url provider.
        /// </summary>
        /// <param name="request">Data of extension url provider.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateExtensionUrlProviderStatus(UpdateExtensionUrlProviderStatusRequest request)
        {
            try
            {
                if (request.IsActive)
                {
                    ExtensionUrlProviderController.EnableProvider(request.ProviderId, this.PortalId);
                }
                else
                {
                    ExtensionUrlProviderController.DisableProvider(request.ProviderId, this.PortalId);
                }
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Tests the internal URL.
        /// </summary>
        /// <returns>Various forms of the URL and any messages when they exist.</returns>
        /// <example>
        /// GET /API/PersonaBar/SEO/TestUrl?pageId=53&amp;queryString=ab%3Dcd&amp;customPageName=test-page.
        /// </example>
        [HttpGet]
        public HttpResponseMessage TestUrl(int pageId, string queryString, string customPageName)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Urls = this.TestUrlInternal(pageId, queryString, customPageName)
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SEO/TestUrlRewrite
        /// <summary>
        /// Tests the rewritten URL.
        /// </summary>
        /// <returns>Rewitten URL and few other information about the URL ( language, redirection result and reason, messages).</returns>
        /// <example>
        /// GET /API/PersonaBar/SEO/TestUrlRewrite?uri=http%3A%2F%2Fmysite.com%2Ftest-page.
        /// </example>
        [HttpGet]
        public HttpResponseMessage TestUrlRewrite(string uri)
        {
            try
            {
                var response = new
                {
                    Success = true,
                    RewritingResult = this.TestUrlRewritingInternal(uri)
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static bool ValidateRegex(string regexPattern)
        {
            try
            {
                if (Regex.IsMatch("", regexPattern))
                {
                }

                return true;
            }
            catch
            {
                //ignore
            }
            return false;
        }

        private void UpdateRegexSettingsInternal(UpdateRegexSettingsRequest request)
        {
            var settings = new Dictionary<string, string>()
            {
                        { FriendlyUrlSettings.IgnoreRegexSetting, request.IgnoreRegex },
                        { FriendlyUrlSettings.DoNotRewriteRegExSetting, request.DoNotRewriteRegex },
                        { FriendlyUrlSettings.SiteUrlsOnlyRegexSetting, request.UseSiteUrlsRegex },
                        { FriendlyUrlSettings.DoNotRedirectUrlRegexSetting, request.DoNotRedirectRegex },
                        { FriendlyUrlSettings.DoNotRedirectHttpsUrlRegexSetting, request.DoNotRedirectSecureRegex },
                        { FriendlyUrlSettings.PreventLowerCaseUrlRegexSetting, request.ForceLowerCaseRegex },
                        { FriendlyUrlSettings.DoNotUseFriendlyUrlRegexSetting, request.NoFriendlyUrlRegex },
                        { FriendlyUrlSettings.KeepInQueryStringRegexSetting, request.DoNotIncludeInPathRegex },
                        { FriendlyUrlSettings.UrlsWithNoExtensionRegexSetting, request.ValidExtensionlessUrlsRegex },
                        { FriendlyUrlSettings.ValidFriendlyUrlRegexSetting, request.RegexMatch }
            };

            settings.ToList().ForEach((value) =>
            {
                if (this.PortalId == Null.NullInteger)
                {
                    HostController.Instance.Update(value.Key, value.Value, false);
                }
                else
                {
                    PortalController.Instance.UpdatePortalSetting(this.PortalId, value.Key, value.Value, false, Null.NullString, false);
                }
            });
        }

        private void ClearCache()
        {
            if (this.PortalId == Null.NullInteger)
            {
                DataCache.ClearHostCache(false);
            }
            else
            {
                DataCache.ClearPortalCache(this.PortalId, false);
            }
            CacheController.FlushPageIndexFromCache();
            CacheController.FlushFriendlyUrlSettingsFromCache();
        }

        private IEnumerable<string> TestUrlInternal(int pageId, string queryString, string customPageName)
        {
            var provider = new DNNFriendlyUrlProvider();
            var tab = TabController.Instance.GetTab(pageId, this.PortalId, false);
            var pageName = string.IsNullOrEmpty(customPageName) ? Globals.glbDefaultPage : customPageName;
            return PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId).
                Select(alias => provider.FriendlyUrl(
                    tab, "~/Default.aspx?tabId=" + pageId + "&" + queryString, pageName, alias.HTTPAlias));
        }

        private UrlRewritingResult TestUrlRewritingInternal(string uriString)
        {
            var rewritingResult = new UrlRewritingResult();
            try
            {
                var noneText = Localization.GetString("None", Localization.GlobalResourceFile);
                var uri = new Uri(uriString);
                var provider = new AdvancedUrlRewriter();
                var result = new UrlAction(uri.Scheme, uriString, Globals.ApplicationMapPath)
                {
                    RawUrl = uriString
                };
                var httpContext = new HttpContext(HttpContext.Current.Request, new HttpResponse(new StringWriter()));
                provider.ProcessTestRequestWithContext(httpContext, uri, true, result, new FriendlyUrlSettings(this.PortalId));
                rewritingResult.RewritingResult = string.IsNullOrEmpty(result.RewritePath) ? noneText : result.RewritePath;
                rewritingResult.Culture = string.IsNullOrEmpty(result.CultureCode) ? noneText : result.CultureCode;
                var tab = TabController.Instance.GetTab(result.TabId, result.PortalId, false);
                rewritingResult.IdentifiedPage = (tab != null ? tab.TabName : noneText);
                rewritingResult.RedirectionReason = Localization.GetString(result.Reason.ToString());
                rewritingResult.RedirectionResult = result.FinalUrl;
                var messages = new StringBuilder();
                foreach (var message in result.DebugMessages)
                {
                    messages.AppendLine(message);
                }
                rewritingResult.OperationMessages = messages.ToString();
            }
            catch (Exception ex)
            {
                rewritingResult.OperationMessages = ex.Message;
            }
            return rewritingResult;
        }
    }
}
