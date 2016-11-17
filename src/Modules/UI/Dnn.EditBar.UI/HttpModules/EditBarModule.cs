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
            if (DotNetNukeContext.Current.Application.SKU != "DNN")
            {
                return;
            }

            var request = e.Skin.Page.Request;
            var isSpecialPageMode = request.QueryString["dnnprintmode"] == "true" || request.QueryString["popUp"] == "true";
            if (isSpecialPageMode 
                    || Globals.IsAdminControl())
            {
                return;
            }

            if (ContentEditorManager.GetCurrent(e.Skin.Page) == null && !Globals.IsAdminControl())
            {
                if (PortalSettings.Current.UserId > 0)
                {
                    e.Skin.Page.Form.Controls.Add(new ContentEditorManager { Skin = e.Skin });
                }
            }
        }
    }
}
