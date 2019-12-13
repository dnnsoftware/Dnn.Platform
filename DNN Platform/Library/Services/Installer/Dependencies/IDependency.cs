// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml.XPath;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IDependency Interface defines the contract for a Package Dependency
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public interface IDependency
    {
        string ErrorMessage { get; }
        bool IsValid { get; }

        void ReadManifest(XPathNavigator dependencyNav);
    }
}
