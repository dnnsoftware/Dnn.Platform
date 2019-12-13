// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Reflection;
using System.Xml.XPath;

using DotNetNuke.Application;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CoreVersionDependency determines whether the CoreVersion is correct
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
                return string.Format(Util.INSTALL_Compatibility,minVersion);
            }
        }

        public override bool IsValid
        {
            get
            {
                bool _IsValid = true;
                if (Assembly.GetExecutingAssembly().GetName().Version < minVersion)
                {
                    _IsValid = false;
                }
                return _IsValid;
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            minVersion = new Version(dependencyNav.Value);
        }
    }
}
