// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics
{
    using System;

    using DNN.Connectors.GoogleTagManager;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Analytics.Config;

    /// <inheritdoc />
    public class GoogleTagManagerEngine : AnalyticsEngineBase
    {
        /// <summary>The token replaced by the configured ID in the script templates.</summary>
        private const string IdToken = "[GTM_ID]";

        /// <summary>
        /// The Google tag (gtag.js) snippet used for non-GTM IDs (G-, AW-, DC-, GT-),
        /// see https://support.google.com/tagmanager/answer/17231523.
        /// </summary>
        private const string GoogleTagScriptTemplate = @"
          <!-- Google tag (gtag.js) -->
          <script async src=""https://www.googletagmanager.com/gtag/js?id=" + IdToken + @"""></script>
          <script>
            window.dataLayer = window.dataLayer || [];
            function gtag(){dataLayer.push(arguments);}
            gtag('js', new Date());

            gtag('config', '" + IdToken + @"');
          </script>
          <!-- End Google tag (gtag.js) -->
        ";

        /// <inheritdoc />
        public override string EngineName
        {
            get
            {
                return "GoogleTagManager";
            }
        }

        /// <inheritdoc />
        public override string RenderScript(string scriptTemplate)
        {
            AnalyticsConfiguration config = this.GetConfig();

            if (config == null)
            {
                return string.Empty;
            }

            var trackingId = string.Empty;
            var urlParameter = string.Empty;
            var trackForAdmin = true;

            foreach (AnalyticsSetting setting in config.Settings)
            {
                switch (setting.SettingName.ToLowerInvariant())
                {
                    case "gtmid":
                        trackingId = setting.SettingValue;
                        break;
                    case "trackforadmin":
                        if (!bool.TryParse(setting.SettingValue, out trackForAdmin))
                        {
                            trackForAdmin = true;
                        }

                        break;
                }
            }

            trackingId = GoogleTagId.Normalize(trackingId);
            var idType = GoogleTagId.GetIdType(trackingId);
            if (idType == GoogleTagIdType.Invalid)
            {
                return string.Empty;
            }

            // check whether setting to not track traffic if current user is host user or website administrator.
            if (!trackForAdmin &&
                (UserController.Instance.GetCurrentUserInfo().IsSuperUser
                 ||
                 (PortalSettings.Current != null &&
                  UserController.Instance.GetCurrentUserInfo().IsInRole(PortalSettings.Current.AdministratorRoleName))))
            {
                return string.Empty;
            }

            if (idType == GoogleTagIdType.GoogleTag)
            {
                // Google no longer supports loading G-, AW- or DC- IDs through gtm.js, so the
                // container snippets (which may already be in SiteAnalytics.config) are swapped
                // for the gtag.js snippet, and the noscript iframe (GTM containers only) is dropped.
                if (IsContainerNoScriptTemplate(scriptTemplate))
                {
                    return string.Empty;
                }

                if (IsContainerScriptTemplate(scriptTemplate))
                {
                    scriptTemplate = GoogleTagScriptTemplate;
                }
            }

            return scriptTemplate.Replace(IdToken, trackingId);
        }

        private static bool IsContainerScriptTemplate(string scriptTemplate)
        {
            return scriptTemplate.IndexOf("googletagmanager.com/gtm.js", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsContainerNoScriptTemplate(string scriptTemplate)
        {
            return scriptTemplate.IndexOf("googletagmanager.com/ns.html", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
