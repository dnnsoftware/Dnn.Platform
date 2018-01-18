#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.Services.Cache;
using System.Web.Caching;

namespace Dnn.PersonaBar.UI.Services
{
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ServerSummaryController : PersonaBarApiController
    {
        private const string CriticalUpdateHash = "e67b666fb40c4f304a41d1706d455c09017b7bcf4ec1e411450ebfcd2c8f12d0";
        private const string NormalUpdateHash = "df29e1cda367bb8fa8534b5fb2415406100252dec057138b8d63cbadb44fb8e7";

        enum UpdateType
        {
            None,
            Normal,
            Critical
        }

        #region Public API methods

        /// <summary>
        /// Return server info.
        /// </summary>
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
                    LicenseVisible = isHost && GetVisibleSetting("LicenseVisible"),
                    DocCenterVisible = GetVisibleSetting("DocCenterVisible"),
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUpdateLink()
        {
            UpdateType updateType;
            var url = NeedUpdate(out updateType) ? Upgrade.UpgradeRedirect() : string.Empty;

            return Request.CreateResponse(HttpStatusCode.OK, new {Url = url, Type = updateType});
        }

        private bool GetVisibleSetting(string settingName)
        {
            var portalSettings = PortalController.Instance.GetPortalSettings(PortalId);
            return !portalSettings.ContainsKey(settingName)
                   || string.IsNullOrEmpty(portalSettings[settingName])
                   || portalSettings[settingName] == "true";
        }

        private bool NeedUpdate(out UpdateType updateType)
        {
            updateType = UpdateType.None;
            
            if (HttpContext.Current == null || !Host.CheckUpgrade || !UserInfo.IsSuperUser)
            {
                return false;
            }

            var version = DotNetNukeContext.Current.Application.Version;
            var request = HttpContext.Current.Request;

            var imageUrl = Upgrade.UpgradeIndicator(version, request.IsLocal, request.IsSecureConnection); ;
            imageUrl = Globals.AddHTTP(imageUrl.TrimStart('/'));

            try
            {
                string hash;
                const string cacheKey = "UpdateServiceUrlCacheKey";
                var cachedData = DataCache.GetCache(cacheKey) as string;
                if (cachedData != null)
                {
                    hash = cachedData;
                }
                else
                {
                    var webRequest = WebRequest.CreateHttp(imageUrl);
                    webRequest.Timeout = Host.WebRequestTimeout;
                    webRequest.UserAgent = request.UserAgent;
                    webRequest.Referer = request.RawUrl;

                    using (var stream = ((HttpWebResponse) webRequest.GetResponse()).GetResponseStream())
                    {
                        if (stream == null)
                        {
                            return false;
                        }
                        using (var sha256 = SHA256.Create())
                        {
                            hash =
                                BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                            DataCache.SetCache(cacheKey, hash, (DNNCacheDependency) null,
                                Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1), CacheItemPriority.Normal, null);
                        }
                    }
                }
                switch (hash)
                {
                    case NormalUpdateHash:
                        updateType = UpdateType.Normal;
                        return true;
                    case CriticalUpdateHash:
                        updateType = UpdateType.Critical;
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
