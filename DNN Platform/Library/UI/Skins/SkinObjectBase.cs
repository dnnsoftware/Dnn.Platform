#region Usings

using System.ComponentModel;
using System.Web.UI;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.UI.Modules;

#endregion

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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        #region ISkinControl Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the associated ModuleControl for this SkinControl
        /// </summary>
        /// -----------------------------------------------------------------------------
        public IModuleControl ModuleControl { get; set; }

        #endregion
    }
}
