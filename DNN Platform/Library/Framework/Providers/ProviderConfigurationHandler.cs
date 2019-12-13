// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Configuration;
using System.Xml;

#endregion

namespace DotNetNuke.Framework.Providers
{
    [Obsolete("This class is obsolete.  It is no longer used to load provider configurations, as there are medium trust issues. Scheduled removal in v11.0.0.")]
    internal class ProviderConfigurationHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public virtual object Create(object parent, object context, XmlNode node)
        {
            var objProviderConfiguration = new ProviderConfiguration();
            objProviderConfiguration.LoadValuesFromConfigurationXml(node);
            return objProviderConfiguration;
        }

        #endregion
    }
}
