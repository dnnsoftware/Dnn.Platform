using System;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Services.GettingStarted
{
    public class GettingStartedController
    {

        public string UserManualUrl
        {
            get { return "http://www.dnnsoftware.com/Community/Download/Manuals"; }
        }

        public string ContentUrl
        {
            get
            {
                var result = "";
                var tabcontroller = new TabController();
                var tab = tabcontroller.GetTab(GettingStartedTabId, PortalController.GetCurrentPortalSettings().PortalId, false);
                var modulecontroller = new ModuleController();
                var modules = modulecontroller.GetTabModules(tab.TabID).Values;

                if (modules.Count > 0)
                {
                    var pmb = new PortalModuleBase();
                    result = pmb.EditUrl(tab.TabID, "", false, "mid=" + modules.ElementAt(0).ModuleID, "popUp=true", "ReturnUrl=" + HttpUtility.UrlEncode(Globals.NavigateURL()));
                }
                else
                {
                    result = Globals.NavigateURL(tab.TabID);
                }

                return result;
            }
        }

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