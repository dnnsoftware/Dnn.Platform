using System;
using System.Web;
using System.Web.UI;
using Dnn.EditBar.UI.Controllers;
using DotNetNuke.Application;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
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
            RegisterEditBarResources(e.Skin.PortalSettings, e.Skin.Page);
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
