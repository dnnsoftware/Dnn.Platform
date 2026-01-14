// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>
    /// Provides extension methods for ApiTokens.
    /// </summary>
    public static class ApiTokenExtensions
    {
        /// <summary>
        /// Converts ApiToken properties to LogProperties in order to log them.
        /// </summary>
        /// <param name="apiToken">The ApiToken to convert to log props.</param>
        /// <returns>A LogProperties instance with ApiToken properties as LogDetailInfo.</returns>
        public static ILogProperties ToLogProps(this ApiToken apiToken)
        {
            var lp = new LogProperties
              {
                new LogDetailInfo("ApiTokenId", apiToken.ApiTokenId.ToString(CultureInfo.InvariantCulture)),
                new LogDetailInfo("TokenName", apiToken.TokenName),
                new LogDetailInfo("ExpiresOn", apiToken.ExpiresOn.ToString(CultureInfo.InvariantCulture)),
                new LogDetailInfo("Created By", apiToken.CreatedByUsername),
                new LogDetailInfo("Created On", apiToken.CreatedOnDate.ToString(CultureInfo.InvariantCulture)),
                new LogDetailInfo("Scope", apiToken.Scope.ToString()),
                new LogDetailInfo("Portal", apiToken.PortalName),
                new LogDetailInfo("Keys", apiToken.Keys),
                new LogDetailInfo("Revoked By", apiToken.RevokedByUsername),
                new LogDetailInfo("Revoked On", apiToken.RevokedOnDate?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
                new LogDetailInfo("Last Used On", apiToken.LastUsedOnDate?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
              };
            return lp;
        }
    }
}
