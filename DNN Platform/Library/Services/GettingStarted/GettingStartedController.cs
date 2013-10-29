using System;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.GettingStarted
{
    public class GettingStartedController
    {
        public bool ShowOnStartup
        {
            get
            {
                var result = false;
                if (GettingStartedTabId > -1)
                {
                    if (!IsPage(GettingStartedTabId) && PortalController.GetCurrentPortalSettings().UserInfo.IsSuperUser && Host.EnableGettingStartedPage)
                    {
                        var settings = PortalController.GetCurrentPortalSettings();
                        result =
                            HostController.Instance.GetBoolean(String.Format("GettingStarted_Display_{0}", settings.UserId), true) &&
                            !HostController.Instance.GetBoolean(String.Format("GettingStarted_Hide_{0}", settings.UserId), false);
                    }
                }
                return result;
            }
        }

        private static int GettingStartedTabId
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("GettingStartedTabId", PortalController.GetCurrentPortalSettings().PortalId, -1);
            }
        }

        private static bool IsPage(int tabId)
        {
            return (PortalController.GetCurrentPortalSettings().ActiveTab.TabID == tabId);
        }

    }
}
