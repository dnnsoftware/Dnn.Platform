// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PermissionsDependency determines whether the DotNetNuke site has the
    /// corretc permissions.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PermissionsDependency : DependencyBase
    {
        private string Permission = Null.NullString;
        private string Permissions;

        public override string ErrorMessage
        {
            get
            {
                return Util.INSTALL_Permissions + " - " + Localization.GetString(this.Permission, Localization.GlobalResourceFile);
            }
        }

        public override bool IsValid
        {
            get
            {
                return SecurityPolicy.HasPermissions(this.Permissions, ref this.Permission);
            }
        }

        public override void ReadManifest(XPathNavigator dependencyNav)
        {
            this.Permissions = dependencyNav.Value;
        }
    }
}
