// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Settings class for storing and retrieving API token settings.</summary>
    public partial class ApiTokenSettings
    {
        /// <summary>Gets or sets the timespan of a token for end users.</summary>
        [PortalSetting(Prefix = "ApiTokens_")]
        public ApiTokenTimespan UserTokenTimespan { get; set; } = ApiTokenTimespan.Days30;

        /// <summary>Gets or sets the maximum timespan of a token for site admins.</summary>
        [HostSetting(Prefix = "ApiTokens_")]
        public ApiTokenTimespan MaximumSiteTimespan { get; set; } = ApiTokenTimespan.Years1;

        /// <summary>Gets or sets a value indicating whether API tokens are allowed.</summary>
        [PortalSetting(Prefix = "ApiTokens_")]
        public bool AllowApiTokens { get; set; } = false;

        /// <summary>Gets a value indicating whether API tokens are enabled.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool ApiTokensEnabled => ApiTokenAuthMessageHandler.IsEnabled;

        /// <summary>Gets the settings defined for the API tokens.</summary>
        /// <param name="portalId">The ID of the portal for the settings.</param>
        /// <returns>The API token settings for the given portal ID.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking ApiTokenSettingsRepository")]
        public static partial ApiTokenSettings GetSettings(int portalId)
            => GetSettings(Globals.GetCurrentServiceProvider().GetRequiredService<ApiTokenSettingsRepository>(), portalId);

        /// <summary>Gets the settings defined for the API tokens.</summary>
        /// <param name="settingsRepository">The settings repository.</param>
        /// <param name="portalId">The ID of the portal for the settings.</param>
        /// <returns>The API token settings for the given portal ID.</returns>
        public static ApiTokenSettings GetSettings(ApiTokenSettingsRepository settingsRepository, int portalId)
        {
            return settingsRepository.GetSettings(portalId);
        }

        /// <summary>Saves the updated settings for the API tokens.</summary>
        /// <param name="portalId">The ID of the portal for the settings.</param>
        [DnnDeprecated(10, 2, 4, "Use overload taking ApiTokenSettingsRepository")]
        public partial void SaveSettings(int portalId)
            => this.SaveSettings(Globals.GetCurrentServiceProvider().GetRequiredService<ApiTokenSettingsRepository>(), portalId);

        /// <summary>Saves the updated settings for the API tokens.</summary>
        /// <param name="settingsRepository">The settings repository.</param>
        /// <param name="portalId">The ID of the portal for the settings.</param>
        public void SaveSettings(ApiTokenSettingsRepository settingsRepository, int portalId)
        {
            settingsRepository.SaveSettings(portalId, this);
        }
    }
}
