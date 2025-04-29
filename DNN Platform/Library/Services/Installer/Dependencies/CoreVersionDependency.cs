// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies;

using System;
using System.Reflection;
using System.Xml.XPath;

/// <summary>The CoreVersionDependency determines whether the CoreVersion is correct.</summary>
public class CoreVersionDependency : DependencyBase
{
    private Version minVersion;

    /// <inheritdoc/>
    public override string ErrorMessage
    {
        get
        {
            return string.Format(Util.INSTALL_Compatibility, this.minVersion);
        }
    }

    /// <inheritdoc/>
    public override bool IsValid
    {
        get
        {
            bool isValid = true;
            if (Assembly.GetExecutingAssembly().GetName().Version < this.minVersion)
            {
                isValid = false;
            }

            return isValid;
        }
    }

    /// <inheritdoc/>
    public override void ReadManifest(XPathNavigator dependencyNav)
    {
        this.minVersion = new Version(dependencyNav.Value);
    }
}
