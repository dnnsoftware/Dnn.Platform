#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Security;
using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    public partial class Store : PortalModuleBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            cmdSave.Click += OnSaveClick;
            cmdCancel.Click += OnCancelClick;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                PortalSecurity ps = new PortalSecurity();
                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(ModuleContext.PortalId);
                if (settings.ContainsKey("Store_Username"))
                { txtUsername.Text = ps.DecryptString(settings["Store_Username"], Config.GetDecryptionkey()); }

                if (settings.ContainsKey("Store_Username"))
                { txtPassword.Text = ps.DecryptString(settings["Store_Password"], Config.GetDecryptionkey()); }
            }

       }
        private void OnSaveClick(object sender, EventArgs e)
        {
            PortalSecurity ps = new PortalSecurity();
            PortalController.UpdatePortalSetting(PortalId, "Store_Username", ps.EncryptString(txtUsername.Text, Config.GetDecryptionkey()));
            PortalController.UpdatePortalSetting(PortalId, "Store_Password", ps.EncryptString(txtPassword.Text, Config.GetDecryptionkey()));
            Response.Redirect(Globals.NavigateURL());
        }

        protected void OnCancelClick(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL());
        }

    }
}