// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Upgrade;

    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ServerSummaryController : PersonaBarApiController
    {
        private const string CriticalUpdateHash = "e67b666fb40c4f304a41d1706d455c09017b7bcf4ec1e411450ebfcd2c8f12d0";
        private const string NormalUpdateHash = "df29e1cda367bb8fa8534b5fb2415406100252dec057138b8d63cbadb44fb8e7";

        private enum UpdateType
        {
            None = 0,
            Normal = 1,
            Critical = 2,
        }

        /// <summary>
        /// Return server info.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetServerInfo()
        {
            try
            {
                var isHost = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
                var response = new
                {
                    ProductName = DotNetNukeContext.Current.Application.Description,
                    ProductVersion = "v. " + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, true),
                    FrameworkVersion = isHost ? Globals.NETFrameworkVersion.ToString(2) : string.Empty,
                    ServerName = isHost ? Globals.ServerName : string.Empty,
                    LicenseVisible = isHost && this.GetVisibleSetting("LicenseVisible"),
                    DocCenterVisible = this.GetVisibleSetting("DocCenterVisible"),
                    UpdateUrl = this.UpdateUrl(),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUpdateLink()
        {
            var url = this.UpdateUrl();
            var updateType = url == string.Empty ? UpdateType.None : UpdateType.Normal;
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Url = url, Type = updateType });
        }

        private bool GetVisibleSetting(string settingName)
        {
            var portalSettings = PortalController.Instance.GetPortalSettings(this.PortalId);
            return !portalSettings.ContainsKey(settingName)
                   || string.IsNullOrEmpty(portalSettings[settingName])
                   || portalSettings[settingName] == "true";
        }

        private string UpdateUrl()
        {
            if (HttpContext.Current == null || !Host.CheckUpgrade || !this.UserInfo.IsSuperUser)
            {
                return string.Empty;
            }

            return CBO.GetCachedObject<string>(new CacheItemArgs("DnnUpdateUrl"), this.RetrieveUpdateUrl);
        }

        private string RetrieveUpdateUrl(CacheItemArgs args)
        {
            try
            {
                var latestReleases = Globals.GetJsonObject<List<DTO.GithubLatestReleaseDTO>>("https://api.github.com/repos/dnnsoftware/dnn.platform/releases?per_page=5");
                if (latestReleases != null)
                {
                    foreach (var release in latestReleases)
                    {
                        if (!release.Draft && !release.PreRelease)
                        {
                            var m = Regex.Match(release.TagName, @"(\d+\.\d+\.\d+)$");
                            if (m.Success)
                            {
                                var latestVersion = new Version(m.Groups[1].Value);
                                if (latestVersion.CompareTo(DotNetNukeContext.Current.Application.Version) > 0)
                                {
                                    return release.Url;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return string.Empty;
        }
    }
}
