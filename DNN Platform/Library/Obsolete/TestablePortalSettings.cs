using System;
using System.ComponentModel;

using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;


namespace DotNetNuke.Entities.Portals.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Deprecated in DotNetNuke 7.3.0. Use PortalController.Instance.GetCurrentPortalSettings to get a mockable PortalSettings. Scheduled removal in v10.0.0.")]
    public class TestablePortalSettings : ComponentBase<IPortalSettings, TestablePortalSettings>, IPortalSettings
    {
        public string AdministratorRoleName
        {
            get { return PortalSettings.Current.AdministratorRoleName; }
        }
    }
}
