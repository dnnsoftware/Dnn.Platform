using System;
using System.Threading;
using System.Web;
using System.Web.UI;
using Dnn.EditBar.UI.Controllers;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.UI.Skins.EventListeners;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;

namespace Dnn.EditBar.UI.HttpModules
{
    public class EditBarModule : IHttpModule
    {
        private static readonly object LockAppStarted = new object();
        private static bool _hasAppStarted = false;

        public void Init(HttpApplication application)
        {
            if (_hasAppStarted)
            {
                return;
            }
            lock (LockAppStarted)
            {
                if (_hasAppStarted)
                {
                    return;
                }

                ApplicationStart();
                _hasAppStarted = true;
            }
        }

        private void ApplicationStart()
        {
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, OnSkinInit));
        }

        public void Dispose()
        {
        }

        private void OnSkinInit(object sender, SkinEventArgs e)
        {
            var request = e.Skin.Page.Request;
            var isSpecialPageMode = request.QueryString["dnnprintmode"] == "true" || request.QueryString["popUp"] == "true";
            if (isSpecialPageMode || !IsPageEditor() || Globals.IsAdminControl())
            {
                return;
            }

            RegisterEditBarResources(e.Skin.PortalSettings, e.Skin.Page);
        }

        private bool IsPageEditor()
        {
            return HasTabPermission("EDIT,CONTENT,MANAGE") || IsModuleAdmin(PortalSettings.Current);

        }

        public static bool HasTabPermission(string permissionKey)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentPortal = PortalController.Instance.GetCurrentPortalSettings();

            bool isAdminUser = currentPortal.UserInfo.IsSuperUser || PortalSecurity.IsInRole(currentPortal.AdministratorRoleName);
            if (isAdminUser) return true;

            return TabPermissionController.HasTabPermission(permissionKey);
        }

        private bool IsModuleAdmin(PortalSettings portalSettings)
        {
            bool isModuleAdmin = false;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }
            return portalSettings.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }

        private void RegisterEditBarResources(PortalSettings portalSettings, Page page)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            var settings = EditBarController.Instance.GetConfigurations(portalSettings.PortalId);
            var settingsScript = "window.editBarSettings = " + JsonConvert.SerializeObject(settings) + ";";
            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "EditBarSettings", settingsScript, true);

            ClientResourceManager.RegisterScript(page, "~/admin/Dnn.EditBar/scripts/editBarContainer.js");

            ClientResourceManager.RegisterStyleSheet(page, "~/admin/Dnn.EditBar/css/editBarContainer.css");

        }
    }
}
