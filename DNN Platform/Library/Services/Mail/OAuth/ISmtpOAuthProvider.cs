// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using MailKit.Net.Smtp;

    /// <summary>
    /// Smtp Oauth provider interface.
    /// </summary>
    public interface ISmtpOAuthProvider
    {
        /// <summary>
        /// Gets provider name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the localized name.
        /// </summary>
        string LocalizedName { get; }

        /// <summary>
        /// Whether the provider completed the authorize process.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>status.</returns>
        bool IsAuthorized(int portalId);

        /// <summary>
        /// Get the authorize url.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The authorize url.</returns>
        string GetAuthorizeUrl(int portalId);

        /// <summary>
        /// Get the provider parameters.
        /// </summary>
        /// <param name="portalId">the portal id of the setting, pass Null.NullInteger if it's a global setting.</param>
        /// <returns>parameters list.</returns>
        IList<SmtpOAuthSetting> GetSettings(int portalId);

        /// <summary>
        /// update provider settings.
        /// </summary>
        /// <param name="portalId">the portal id of the setting, pass Null.NullInteger if it's a global setting.</param>
        /// <param name="settings">the settings.</param>
        /// <param name="errorMessages">the errors.</param>
        /// <returns>Whether update the settings successfully.</returns>
        bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages);

        /// <summary>
        /// Authorize the smtp client.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="smtpClient">The smtp client.</param>
        void Authorize(int portalId, ISmtpClient smtpClient);
    }
}
