// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.ConfigSection
{
    using System.Configuration;

    /// <summary>A <see cref="ConfigurationSection"/> for auth services.</summary>
    public class AuthServicesConfiguration : ConfigurationSection
    {
        /// <summary>Gets the message handlers.</summary>
        [ConfigurationProperty("messageHandlers", IsRequired = true)]
        public MessageHandlersCollection MessageHandlers => this["messageHandlers"] as MessageHandlersCollection;

        /// <summary>Gets the auth services config.</summary>
        /// <returns>The configuration section, or <see langword="null"/> if it doesn't exist.</returns>
        public static AuthServicesConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("dotnetnuke/authServices") as AuthServicesConfiguration;
        }
    }
}
