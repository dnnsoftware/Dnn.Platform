﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using DotNetNuke.Entities.Modules.Settings;

    /// <summary>
    /// Settings class for storing and retrieving API token settings.
    /// </summary>
    public class ApiTokenSettings
    {
        /// <summary>
        /// Gets or sets the maximum timespan of a token.
        /// </summary>
        [HostSetting(Prefix = "ApiTokens_")]
        public int MaximumTimespan { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum timespan measure of a token. Can be one of the following: y, M, w, d.
        /// </summary>
        [HostSetting(Prefix = "ApiTokens_")]
        public string MaximumTimespanMeasure { get; set; } = "y";

        /// <summary>
        /// Gets or sets a value indicating whether API tokens are allowed.
        /// </summary>
        [PortalSetting(Prefix = "ApiTokens_")]
        public bool AllowApiTokens { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether API tokens are enabled.
        /// </summary>
        public bool ApiTokensEnabled => ApiTokenAuthMessageHandler.IsEnabled;

        /// <summary>
        /// Gets the settings defined for the API tokens.
        /// </summary>
        /// <param name="portalId">The ID of the portal for the settings.</param>
        /// <returns>The API token settings for the given portal ID.</returns>
        public static ApiTokenSettings GetSettings(int portalId)
        {
            var repo = new ApiTokenSettingsRepository();
            return repo.GetSettings(portalId);
        }

        /// <summary>
        /// Saves the updated settings for the API tokens.
        /// </summary>
        /// <param name="portalId">The ID of the portal for the settings.</param>
        public void SaveSettings(int portalId)
        {
            var repo = new ApiTokenSettingsRepository();
            repo.SaveSettings(portalId, this);
        }
    }
}
