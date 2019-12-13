// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Configuration;

namespace DotNetNuke.Web.ConfigSection
{
    public class AuthServicesConfiguration : ConfigurationSection
    {
        public static AuthServicesConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("dotnetnuke/authServices") as AuthServicesConfiguration;
        }

        [ConfigurationProperty("messageHandlers", IsRequired = true)]
        public MessageHandlersCollection MessageHandlers => this["messageHandlers"] as MessageHandlersCollection;
    }
}
