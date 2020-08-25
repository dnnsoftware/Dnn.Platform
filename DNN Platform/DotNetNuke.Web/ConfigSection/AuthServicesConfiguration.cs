// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.ConfigSection
{
    using System.Configuration;

    public class AuthServicesConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("messageHandlers", IsRequired = true)]
        public MessageHandlersCollection MessageHandlers => this["messageHandlers"] as MessageHandlersCollection;

        public static AuthServicesConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("dotnetnuke/authServices") as AuthServicesConfiguration;
        }
    }
}
