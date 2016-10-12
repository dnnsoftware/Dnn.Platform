#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Sitemap.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Sitemap;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Sitemap.Services
{
    [ServiceScope(Scope = ServiceScope.Admin, Identifier = "Sitemap")]
    public class SitemapController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SitemapController));
        private Components.SitemapController _controller = new Components.SitemapController();

        /// GET: api/Sitemap/GetProviders
        /// <summary>
        /// Gets list of sitemap providers
        /// </summary>
        /// <param></param>
        /// <returns>Web Server information</returns>
        [HttpGet]
        public HttpResponseMessage GetProviders()
        {
            try
            {
                var providers = _controller.GetProviders().Select(p => new 
                {
                    p.Name,
                    p.Description,
                    p.Enabled,
                    p.Priority,
                    p.OverridePriority
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = providers,
                    TotalResults = providers.Count()
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sitemap/UpdateProvider
        /// <summary>
        /// Updates settings of a sitemap provider
        /// </summary>
        /// <param name="providerDto">Data of sitemap provider</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateProvider(SitemapProviderDto providerDto)
        {
            try
            {
                SitemapProvider editedProvider =
                    _controller.GetProviders()
                        .FirstOrDefault(p => p.Name.Equals(providerDto.Name, StringComparison.InvariantCultureIgnoreCase));

                if (editedProvider != null)
                {
                    editedProvider.Enabled = providerDto.Enabled;
                    editedProvider.OverridePriority = providerDto.OverridePriority;
                    editedProvider.Priority = providerDto.Priority;
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sitemap/GetSettings
        /// <summary>
        /// Gets sitemap settings
        /// </summary>
        /// <param></param>
        /// <returns>Data of sitemap settings</returns>
        [HttpGet]
        public HttpResponseMessage GetSettings()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                var settings = new
                {
                    SitemapLevelMode =
                        bool.Parse(PortalController.GetPortalSetting("SitemapLevelMode", portalId,
                            "False")),
                    SitemapMinPriority =
                        float.Parse(
                            PortalController.GetPortalSetting("SitemapMinPriority", portalId, "0.1"),
                            NumberFormatInfo.InvariantInfo),
                    SitemapIncludeHidden =
                        bool.Parse(PortalController.GetPortalSetting("SitemapIncludeHidden", portalId,
                            "False")),
                    SitemapExcludePriority =
                        float.Parse(
                            PortalController.GetPortalSetting("SitemapExcludePriority", portalId, "0.1"),
                            NumberFormatInfo.InvariantInfo),
                    SitemapCacheDays =
                        int.Parse(PortalController.GetPortalSetting("SitemapCacheDays", portalId, "1"))
                };
                return Request.CreateResponse(HttpStatusCode.OK, settings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sitemap/UpdateSettings
        /// <summary>
        /// Updates sitemap settings
        /// </summary>
        /// <param name="settingsDto">Data of sitemap settings</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSettings(SitemapSettingsDto settingsDto)
        {
            try
            {
                int portalId = settingsDto.PortalId;
                PortalController.UpdatePortalSetting(portalId, "SitemapLevelMode", settingsDto.SitemapLevelMode.ToString());

                if (settingsDto.SitemapMinPriority < 0)
                {
                    settingsDto.SitemapMinPriority = 0;
                }
                PortalController.UpdatePortalSetting(portalId, "SitemapMinPriority", settingsDto.SitemapMinPriority.ToString(NumberFormatInfo.InvariantInfo));

                PortalController.UpdatePortalSetting(portalId, "SitemapIncludeHidden", settingsDto.SitemapIncludeHidden.ToString());

                if (settingsDto.SitemapExcludePriority < 0)
                {
                    settingsDto.SitemapExcludePriority = 0;
                }
                PortalController.UpdatePortalSetting(portalId, "SitemapExcludePriority", settingsDto.SitemapExcludePriority.ToString(NumberFormatInfo.InvariantInfo));

                if (settingsDto.SitemapCacheDays == 0)
                {
                    _controller.ResetCache();
                }

                PortalController.UpdatePortalSetting(portalId, "SitemapCacheDays", settingsDto.SitemapCacheDays.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sitemap/ResetCache
        /// <summary>
        /// Resets cache
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ResetCache()
        {
            try
            {
                _controller.ResetCache();
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sitemap/GetSearchEngineList
        /// <summary>
        /// Gets list of predefined search engines
        /// </summary>
        /// <param></param>
        /// <returns>List of search engines</returns>
        [HttpGet]
        public HttpResponseMessage GetSearchEngineList()
        {
            string[] searchEngines = new string[3] { "Google", "Bing", "Yahoo!" };
            var response = new
            {
                Success = true,
                Results = searchEngines,
                TotalResults = searchEngines.Count()
            };

            return Request.CreateResponse(HttpStatusCode.OK, response);
            
        }

        /// GET: api/Sitemap/GetSearchEngineSubmissionUrl
        /// <summary>
        /// Gets the submission url of specific search engine
        /// </summary>
        /// <param name="searchEngine">Name of a search engine</param>
        /// <returns>Submission Url of specific search engine</returns>
        [HttpGet]
        public HttpResponseMessage GetSearchEngineSubmissionUrl(string searchEngine)
        {
            try
            {
                var submissionUrl = _controller.GetSearchEngineSubmissionUrl(searchEngine);
                return Request.CreateResponse(HttpStatusCode.OK, submissionUrl);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sitemap/CreateVerification
        /// <summary>
        /// Creates a verification file for specific search engine
        /// </summary>
        /// <param name="searchEngine">Name of verification</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateVerification(string verification)
        {
            try
            {
                _controller.CreateVerification(verification);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
