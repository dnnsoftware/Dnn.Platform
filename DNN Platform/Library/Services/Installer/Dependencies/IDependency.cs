// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System.Xml.XPath;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IDependency Interface defines the contract for a Package Dependency.
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
