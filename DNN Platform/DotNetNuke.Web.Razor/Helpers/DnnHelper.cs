#region Usings

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Modules;
using System;

#endregion

namespace DotNetNuke.Web.Razor.Helpers
{
    [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
    public class DnnHelper
    {
        private readonly ModuleInstanceContext _context;

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public DnnHelper(ModuleInstanceContext context)
        {
            _context = context;
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public ModuleInfo Module
        {
            get
            {
                return _context.Configuration;
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public TabInfo Tab
        {
            get
            {
                return _context.PortalSettings.ActiveTab;
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public PortalSettings Portal
        {
            get
            {
                return _context.PortalSettings;
            }
        }

        [Obsolete("Deprecated in 9.3.2, will be removed in 11.0.0, use Razor Pages instead")]
        public UserInfo User
        {
            get
            {
                return _context.PortalSettings.UserInfo;
            }
        }
    }
}
