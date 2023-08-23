// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components
{
    /// <summary>Azure connector constants.</summary>
    internal class Constants
    {
        /// <summary>
        /// The provider name.
        /// </summary>
        public const string Name = "ExchangeOnline";

        /// <summary>
        /// The tenant id setting name.
        /// </summary>
        public const string TenantIdSettingName = "msauth_tenantId";

        /// <summary>
        /// The client id setting name.
        /// </summary>
        public const string ClientIdSettingName = "msauth_clientId";

        /// <summary>
        /// The client secret setting name.
        /// </summary>
        public const string ClientSecretSettingName = "msauth_clientSecret";

        /// <summary>
        /// The access token setting name.
        /// </summary>
        public const string AuthenticationSettingName = "msauth_authentication";

        /// <summary>
        /// The authorize callback url.
        /// </summary>
        public const string CallbackUrl = "{0}/Providers/SmtpOAuthProviders/ExchangeOnline/Authorize.aspx?state=portal_{1}_";

        /// <summary>
        /// The local resources file path.
        /// </summary>
        public const string LocalResourcesFile = "~/Providers/SmtpOAuthProviders/ExchangeOnline/App_LocalResources/SharedResources.resx";

        /// <summary>
        /// The default azure instance.
        /// </summary>
        public const string AzureInstance = "https://login.microsoftonline.com/";
    }
}
