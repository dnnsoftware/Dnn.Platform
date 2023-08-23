// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components
{
    /// <summary>Azure connector constants.</summary>
    internal class Constants
    {
        /// <summary>
        /// The provider name.
        /// </summary>
        public const string Name = "GoogleMail";

        /// <summary>
        /// The client id setting name.
        /// </summary>
        public const string ClientIdSettingName = "gmailauth_clientId";

        /// <summary>
        /// The client secret setting name.
        /// </summary>
        public const string ClientSecretSettingName = "gmailauth_clientSecret";

        /// <summary>
        /// The account email setting name.
        /// </summary>
        public const string AccountEmailSettingName = "gmailauth_username";

        /// <summary>
        /// The data store setting name.
        /// </summary>
        public const string DataStoreSettingName = "gmailauth_{0}_datastore";

        /// <summary>
        /// The authorize callback url.
        /// </summary>
        public const string CallbackUrl = "{0}/Providers/SmtpOAuthProviders/GoogleMail/Authorize.aspx?state=portal_{1}_";

        /// <summary>
        /// The local resources file path.
        /// </summary>
        public const string LocalResourcesFile = "~/Providers/SmtpOAuthProviders/GoogleMail/App_LocalResources/SharedResources.resx";
    }
}
