// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Xml.XPath;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Installer.Dependencies
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DependencyBase is a base class for Installer Dependencies
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class DependencyBase : IDependency
    {
        #region IDependency Members

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

        #endregion
    }
}
