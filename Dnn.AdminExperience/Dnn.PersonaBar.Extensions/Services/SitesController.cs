// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Sites.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Sites.Services.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SitesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SitesController));
        private readonly Components.SitesController _controller = new Components.SitesController();

        /// <summary>
        /// Gets list of portals.
        /// </summary>
        /// <param name="portalGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>List of portals.</returns>
        /// <example>
        /// GET /api/personabar/sites/GetPortals?portalGroupId=-1&amp;filter=mysite&amp;pageIndex=0&amp;pageSize=10.
        /// </example>
        [HttpGet]
        public HttpResponseMessage GetPortals(int portalGroupId, string filter, int pageIndex, int pageSize)
        {
            try
            {
                var totalRecords = 0;
                IEnumerable<PortalInfo> portals;

                if (portalGroupId > Null.NullInteger)
                {
                    portals = PortalGroupController.Instance.GetPortalsByGroup(portalGroupId)
                                .Where(p => string.IsNullOrEmpty(filter)
                                                || p.PortalName.ToLowerInvariant().Contains(filter.ToLowerInvariant()))
                                .ToList();
                    totalRecords = portals.Count();
                    portals = portals.Skip(pageIndex * pageSize).Take(pageSize);
                }
                else
                {
                    portals = PortalController.GetPortalsByName($"%{filter}%", pageIndex, pageSize,
                                ref totalRecords).Cast<PortalInfo>();
                }
                var response = new
                {
                    Results = portals.Select(this.GetPortalDto).ToList(),
                    TotalResults = totalRecords
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/CreatePortal
        /// <summary>
        /// Adds a portal.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreatePortal(CreatePortalRequest request)
        {
            try
            {
                var errors = new List<string>();
                var portalId = this._controller.CreatePortal(errors, this.GetDomainName(), this.GetAbsoluteServerPath(),
                    request.SiteTemplate, request.SiteName,
                    request.SiteAlias, request.SiteDescription, request.SiteKeywords,
                    request.IsChildSite, request.HomeDirectory, request.SiteGroupId, request.UseCurrentUserAsAdmin,
                    request.Firstname, request.Lastname, request.Username, request.Email, request.Password,
                    request.PasswordConfirm, request.Question, request.Answer);
                if (portalId < 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Join("<br/>", errors));
                }
                var portal = PortalController.Instance.GetPortal(portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Portal = this.GetPortalDto(portal),
                    ErrorMessage = errors
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/DeletePortal
        /// <summary>
        /// Deletes a portal.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePortal(int portalId)
        {
            try
            {
                var portal = PortalController.Instance.GetPortal(portalId);
                if (portal != null)
                {
                    if (portal.PortalID != this.PortalSettings.PortalId &&
                        !PortalController.IsMemberOfPortalGroup(portal.PortalID))
                    {
                        var strMessage = PortalController.DeletePortal(portal, this.GetAbsoluteServerPath());
                        if (string.IsNullOrEmpty(strMessage))
                        {
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
                        }
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, strMessage);
                    }
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                        Localization.GetString("PortalDeletionDenied", Components.Constants.LocalResourcesFile));
                }
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    Localization.GetString("PortalNotFound", Components.Constants.LocalResourcesFile));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/ExportPortalTemplate
        /// <summary>
        /// Exports portal template.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ExportPortalTemplate(ExportTemplateRequest request)
        {
            try
            {
                bool success;
                var message = this._controller.ExportPortalTemplate(request, this.UserInfo, out success);

                if (!success)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                }
                var template = this._controller.GetPortalTemplates().First(x => x.Name == request.FileName);
                var templateItem = this._controller.CreateListItem(template);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Message = message,
                    Template = new
                    {
                        Name = templateItem.Text,
                        templateItem.Value
                    }
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sites/GetPortalLocales
        /// <summary>
        /// Gets list of portal locales.
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>List of portal locales.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalLocales(int portalId)
        {
            try
            {
                var locales = LocaleController.Instance.GetLocales(portalId).Values;
                var response = new
                {
                    Success = true,
                    Results = locales.Select(l => new
                    {
                        l.Code,
                        l.EnglishName,
                        l.LanguageId
                    }),
                    TotalResults = locales.Count
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/DeleteExpiredPortals
        /// <summary>
        /// Deletes expired portals.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteExpiredPortals()
        {
            try
            {
                PortalController.DeleteExpiredPortals(this.GetAbsoluteServerPath());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sites/GetPortalTemplates
        /// <summary>
        /// Gets list of portal templates.
        /// </summary>
        /// <param></param>
        /// <returns>List of portal templates.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTemplates()
        {
            try
            {
                var defaultTemplate = this._controller.GetDefaultTemplate();
                var temps = new List<Tuple<string, string>>();
                var templates = this._controller.GetPortalTemplates();
                foreach (var template in templates)
                {
                    var item = this._controller.CreateListItem(template);
                    temps.Add(new Tuple<string, string>(item.Text, item.Value));
                    if (item.Value.StartsWith(defaultTemplate))
                        defaultTemplate = item.Value;
                }

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Templates = temps.Select(t => new
                        {
                            Name = t.Item1,
                            Value = t.Item2
                        }),
                        DefaultTemplate = defaultTemplate
                    },
                    TotalResults = temps.Count
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sites/RequiresQuestionAndAnswer
        /// <summary>
        /// Gets whether a Question/Answer is required for Password retrieval.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage RequiresQuestionAndAnswer()
        {
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    MembershipProviderConfig.RequiresQuestionAndAnswer
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string GetAbsoluteServerPath()
        {
            var httpContext = this.Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            var strServerPath = string.Empty;
            if (httpContext != null)
                strServerPath = httpContext.Request.MapPath(httpContext.Request.ApplicationPath);
            if (!strServerPath.EndsWith("\\"))
            {
                strServerPath += "\\";
            }
            return strServerPath;
        }

        private string GetDomainName()
        {
            var httpContext = this.Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            return httpContext != null ? Globals.GetDomainName(httpContext.Request, true) : string.Empty;
        }

        private object GetPortalDto(PortalInfo portal)
        {
            string contentLocalizable;
            var portalDto = new
            {
                portal.PortalID,
                portal.PortalName,
                PortalAliases = this._controller.FormatPortalAliases(portal.PortalID),
                portal.Users,
                portal.Pages,
                portal.HostSpace,
                portal.HostFee,
                portal.DefaultLanguage,
                ExpiryDate = this._controller.FormatExpiryDate(portal.ExpiryDate),
                portal.LastModifiedOnDate,
                contentLocalizable =
                    PortalController.Instance.GetPortalSettings(portal.PortalID)
                        .TryGetValue("ContentLocalizationEnabled", out contentLocalizable) &&
                    Convert.ToBoolean(contentLocalizable),
                allowDelete =
                    (portal.PortalID != this.PortalSettings.PortalId &&
                     !PortalController.IsMemberOfPortalGroup(portal.PortalID))
            };
            return portalDto;
        }
    }
}
