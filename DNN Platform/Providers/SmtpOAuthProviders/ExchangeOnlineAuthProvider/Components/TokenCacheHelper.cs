// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web;
    using Microsoft.Identity.Client;

    /// <summary>
    /// The token cache helper.
    /// </summary>
    public class TokenCacheHelper
    {
        private int portalId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCacheHelper"/> class.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public TokenCacheHelper(int portalId)
        {
            this.portalId = portalId;
        }

        /// <summary>
        /// Enable the token cache.
        /// </summary>
        /// <param name="tokenCache">The token cache instance.</param>
        public void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(this.BeforeAccessNotification);
            tokenCache.SetAfterAccess(this.AfterAccessNotification);
        }

        private static IServiceProvider GetServiceProvider()
        {
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ??
                DependencyInjectionInitialize.BuildServiceProvider();
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            var authentication = this.GetAuthenticationData(this.portalId);
            if (!string.IsNullOrEmpty(authentication))
            {
                args.TokenCache.DeserializeMsalV3(Encoding.UTF8.GetBytes(authentication));
            }
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (args.HasStateChanged)
            {
                byte[] data = args.TokenCache.SerializeMsalV3();
                this.UpdateAuthenticationData(this.portalId, data);
            }
        }

        private string GetAuthenticationData(int portalId)
        {
            if (portalId == Null.NullInteger)
            {
                var hostSettingsService = (IHostSettingsService)GetServiceProvider().GetService(typeof(IHostSettingsService));
                return hostSettingsService.GetEncryptedString(Constants.AuthenticationSettingName, Config.GetDecryptionkey());
            }

            return PortalController.GetEncryptedString(Constants.AuthenticationSettingName, portalId, Config.GetDecryptionkey());
        }

        private void UpdateAuthenticationData(int portalId, byte[] data)
        {
            var settingValue = Encoding.UTF8.GetString(data);
            if (portalId == Null.NullInteger)
            {
                var hostSettingsService = (IHostSettingsService)GetServiceProvider().GetService(typeof(IHostSettingsService));
                hostSettingsService.UpdateEncryptedString(Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
            }
            else
            {
                PortalController.UpdateEncryptedString(portalId, Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
            }
        }
    }
}
