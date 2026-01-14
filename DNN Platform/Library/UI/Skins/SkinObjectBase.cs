// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.UI.Modules;

    /// <summary>
    /// The SkinObject class defines a custom base class inherited by all
    /// skin and container objects within the Portal.
    /// </summary>
    public class SkinObjectBase : UserControl, ISkinControl
    {
        /// <summary>Gets the portal Settings for this Skin Control.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();

        /// <summary>Gets a value indicating whether we are in Admin Mode.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool AdminMode => TabPermissionController.CanAdminPage();

        /// <summary>Gets or sets the associated ModuleControl for this SkinControl.</summary>
        public IModuleControl ModuleControl { get; set; }
    }
}
