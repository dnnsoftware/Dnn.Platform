#region Usings

using System.ComponentModel;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserControlBase class defines a custom base class inherited by all
    /// user controls within the Portal.
    /// </summary>
    public class UserControlBase : UserControl
    {
        public bool IsHostMenu
        {
            get
            {
            	return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }
    }
}
