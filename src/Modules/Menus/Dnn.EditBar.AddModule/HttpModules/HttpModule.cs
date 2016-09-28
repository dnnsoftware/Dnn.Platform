using System;
using System.Web;
using System.Web.UI;
using Dnn.EditBar.AddModule.ContentEditor;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Skins.EventListeners;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Newtonsoft.Json;

namespace Dnn.EditBar.AddModule.HttpModules
{
    public class HttpModule : IHttpModule
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

        public static void OnSkinInit(object sender, SkinEventArgs e)
        {
            try
            {
                if (DotNetNukeContext.Current.Application.SKU != "DNN")
                {
                    return;
                }

                if (ContentEditorManager.GetCurrent(e.Skin.Page) == null && !Globals.IsAdminControl())
                {
                    if (PortalSettings.Current.UserId > 0)
                    {
                        e.Skin.Page.Form.Controls.Add(new ContentEditorManager { Skin = e.Skin });
                        //e.Skin.Page.Form.Controls.Add(new QuickAddModuleManager());
                    }
                }

                ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

    }
}
