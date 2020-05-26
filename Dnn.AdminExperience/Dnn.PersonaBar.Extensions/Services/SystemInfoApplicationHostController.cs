// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
