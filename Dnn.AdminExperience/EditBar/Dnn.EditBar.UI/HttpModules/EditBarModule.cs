// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.HttpModules
{
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

                this.ApplicationStart();
                _hasAppStarted = true;
            }
        }

        public void Dispose()
        {
        }

        private void ApplicationStart()
        {
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, this.OnSkinInit));
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
