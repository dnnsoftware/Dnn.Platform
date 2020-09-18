// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System;
    using System.Configuration;
    using System.Xml;

    [Obsolete("This class is obsolete.  It is no longer used to load provider configurations, as there are medium trust issues. Scheduled removal in v11.0.0.")]
    internal class ProviderConfigurationHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object context, XmlNode node)
        {
            var objProviderConfiguration = new ProviderConfiguration();
            objProviderConfiguration.LoadValuesFromConfigurationXml(node);
            return objProviderConfiguration;
        }
    }
}
