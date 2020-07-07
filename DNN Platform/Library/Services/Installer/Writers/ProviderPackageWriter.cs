// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProviderPackageWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProviderPackageWriter : PackageWriterBase
    {
        public ProviderPackageWriter(PackageInfo package)
            : base(package)
        {
            XmlDocument configDoc = Config.Load();
            XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode("/configuration/dotnetnuke/*/providers/add[@name='" + package.Name + "']");
            string providerPath = Null.NullString;
            if (providerNavigator != null)
            {
                providerPath = Util.ReadAttribute(providerNavigator, "providerPath");
            }

            if (!string.IsNullOrEmpty(providerPath))
            {
                this.BasePath = providerPath.Replace("~/", string.Empty).Replace("/", "\\");
            }
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            base.GetFiles(includeSource, false);
        }
    }
}
