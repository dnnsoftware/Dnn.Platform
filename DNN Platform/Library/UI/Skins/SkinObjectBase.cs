

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.ComponentModel;
using System.Web.UI;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.UI.Skins
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinObject class defines a custom base class inherited by all
    /// skin and container objects within the Portal.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class SkinObjectBase : UserControl, ISkinControl
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the portal Settings for this Skin Control
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether we are in Admin Mode
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool AdminMode
        {
            get
            {
                return TabPermissionController.CanAdminPage();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the associated ModuleControl for this SkinControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public IModuleControl ModuleControl { get; set; }
    }
}
