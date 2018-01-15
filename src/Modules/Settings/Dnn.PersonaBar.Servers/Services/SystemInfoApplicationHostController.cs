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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Application;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Servers.Services
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class SystemInfoApplicationHostController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemInfoApplicationHostController));

        [HttpGet]
        public HttpResponseMessage GetApplicationInfo()
        {
            try
            {
                var friendlyUrlProvider = GetProviderConfiguration("friendlyUrl");
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                   product = DotNetNukeContext.Current.Application.Description,
                   version = DotNetNukeContext.Current.Application.Version.ToString(3),
                   guid = DotNetNuke.Entities.Host.Host.GUID,
                   htmlEditorProvider = GetProviderConfiguration("htmlEditor"),
                   dataProvider = GetProviderConfiguration("data"),
                   cachingProvider = GetProviderConfiguration("caching"),
                   loggingProvider = GetProviderConfiguration("logging"),
                   friendlyUrlProvider,
                   friendlyUrlsEnabled = DotNetNuke.Entities.Host.Host.UseFriendlyUrls.ToString(),
                   friendlyUrlType = GetFriendlyUrlType(friendlyUrlProvider),
                   schedulerMode = DotNetNuke.Entities.Host.Host.SchedulerMode.ToString(),
                   webFarmEnabled = DotNetNuke.Services.Cache.CachingProvider.Instance().IsWebFarm().ToString(),
                   casPermissions = SecurityPolicy.Permissions
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string GetProviderConfiguration(string providerName)
        {
            return ProviderConfiguration.GetProviderConfiguration(providerName).DefaultProvider;
        }

        private static string GetFriendlyUrlType(string friendlyUrlProvider)
        {
            var urlProvider = (Provider) ProviderConfiguration.GetProviderConfiguration("friendlyUrl").Providers[friendlyUrlProvider];
            var urlFormat = urlProvider.Attributes["urlformat"];
            return string.IsNullOrWhiteSpace(urlFormat) ? "SearchFriendly" : FirstCharToUpper(urlFormat);
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return input.First().ToString().ToUpper() + string.Join("", input.Skip(1));
        }

    }
}