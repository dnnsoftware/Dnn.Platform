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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Modules.Dashboard.Components;
using DotNetNuke.Security;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;
using Reflection = DotNetNuke.Framework.Reflection;

#endregion

namespace DotNetNuke.Modules.Admin.Dashboard
{
    public partial class Dashboard : PortalModuleBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            jQuery.RequestDnnPluginsRegistration();

            cmdInstall.NavigateUrl = Util.InstallURL(TabId, "DashboardControl");
            cmdManage.NavigateUrl = EditUrl("DashboardControls");
            cmdExport.NavigateUrl = EditUrl("Export");


            //string dashboardJs = ResolveUrl("~/resources/dashboard/jquery.dashboard.js");
            //Page.ClientScript.RegisterClientScriptInclude("DashboardJS", dashboardJs);
            ClientAPI.RegisterClientVariable(Page, "dashboardBaseUrl", ControlPath, false);
            ClientAPI.RegisterClientVariable(Page, "appBaseUrl", Globals.ApplicationPath, false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }

            rptControls.ItemDataBound += rptControls_ItemDataBound;

            //Get enabled Dashboard Controls 
            List<DashboardControl> controls = DashboardController.GetDashboardControls(true);

            //Bind to tab list 
            rptTabs.DataSource = controls;
            rptTabs.DataBind();

            //Bind to control list 
            rptControls.DataSource = controls;
            rptControls.DataBind();
        }

        protected void rptControls_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var control = e.Item.DataItem as DashboardControl;
                if (control != null)
                {
                    Control dashboardControl;
                    if (control.DashboardControlSrc.ToLowerInvariant().EndsWith("ascx"))
                    {
						//load from a user control on the file system 
                        dashboardControl = LoadControl("~/" + control.DashboardControlSrc);
                    }
                    else
                    {
						//load from a typename in an assembly ( ie. server control ) 
                        dashboardControl = LoadControl(Reflection.CreateType(control.DashboardControlSrc), null);
                    }
                    dashboardControl.ID = Path.GetFileNameWithoutExtension(control.DashboardControlSrc);
                    var placeHolder = (PlaceHolder) e.Item.FindControl("phControl");
                    placeHolder.Controls.Add(dashboardControl);
                }
            }
        }
    }
}