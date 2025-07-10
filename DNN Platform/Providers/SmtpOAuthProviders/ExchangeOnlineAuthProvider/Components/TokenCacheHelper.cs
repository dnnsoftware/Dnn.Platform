// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components;

using System;
using System.Text;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

/// <summary>The token cache helper.</summary>
public class TokenCacheHelper
{
    private readonly IHostSettingsService hostSettingsService;
    private readonly IPortalController portalController;
    private readonly IHostSettings hostSettings;
    private readonly int portalId;

    /// <summary>Initializes a new instance of the <see cref="TokenCacheHelper"/> class.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public TokenCacheHelper(int portalId, IHostSettingsService hostSettingsService)
        : this(portalId, hostSettingsService, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TokenCacheHelper"/> class.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    public TokenCacheHelper(int portalId, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController)
    {
        this.portalId = portalId;
        this.hostSettingsService = hostSettingsService;
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
    }

    /// <summary>Enable the token cache.</summary>
    /// <param name="tokenCache">The token cache instance.</param>
    public void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(this.BeforeAccessNotification);
        tokenCache.SetAfterAccess(this.AfterAccessNotification);
    }

    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        var authentication = this.GetAuthenticationData();
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
            this.UpdateAuthenticationData(data);
        }
    }

    private string GetAuthenticationData()
    {
        if (this.portalId == Null.NullInteger)
        {
            return this.hostSettingsService.GetEncryptedString(Constants.AuthenticationSettingName, Config.GetDecryptionkey());
        }

        return PortalController.GetEncryptedString(this.hostSettings, this.portalController, Constants.AuthenticationSettingName, this.portalId, Config.GetDecryptionkey());
    }

    private void UpdateAuthenticationData(byte[] data)
    {
        var settingValue = Encoding.UTF8.GetString(data);
        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.UpdateEncryptedString(Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
        else
        {
            PortalController.UpdateEncryptedString(this.hostSettings, this.portalId, Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
    }
}
