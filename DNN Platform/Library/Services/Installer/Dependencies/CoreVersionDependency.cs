// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System;
    using System.Reflection;
    using System.Xml.XPath;

    using DotNetNuke.Application;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CoreVersionDependency determines whether the CoreVersion is correct.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class CoreVersionDependency : DependencyBase
    {
        private Version minVersion;

        public override string ErrorMessage
        {
            get
            {
                return string.Format(Util.INSTALL_Compatibility, this.minVersion);
            }
        }

        public override bool IsValid
        {
            get
            {
                bool _IsValid = true;
                if (Assembly.GetExecutingAssembly().GetName().Version < this.minVersion)
                {
                    _IsValid = false;
                }

                return _IsValid;
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            this.minVersion = new Version(dependencyNav.Value);
        }
    }
}
