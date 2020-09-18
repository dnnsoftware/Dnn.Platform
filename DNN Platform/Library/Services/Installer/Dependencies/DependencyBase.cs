// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DependencyBase is a base class for Installer Dependencies.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class DependencyBase : IDependency
    {
        public virtual string ErrorMessage
        {
            get
            {
                return Null.NullString;
            }
        }

        public virtual bool IsValid
        {
            get
            {
                return true;
            }
        }

        public virtual void ReadManifest(XPathNavigator dependencyNav)
        {
        }
    }
}
