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

using DotNetNuke.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Host
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditIPFilters PortalModuleBase is used to manage a login IP filter
    /// for a portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class EditIPFilters : PortalModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (EditIPFilters));

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            lnkCancel.Click += lnkCancel_OnClick;
        }

        private void lnkCancel_OnClick(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ModuleContext.NavigateUrl(ModuleContext.TabId, "", true), true);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                if (!String.IsNullOrEmpty(Request.QueryString["IPFilterID"]))
                {
                    IPFilterInfo editFilter = IPFilterController.Instance.GetIPFilter(Convert.ToInt32(Request.QueryString["IPFilterID"]));
                    txtFirstIP.Text = editFilter.IPAddress;
                    txtSubnet.Text = editFilter.SubnetMask;
                    if (cboType.Items.FindByValue(editFilter.RuleType.ToString()) != null)
                    {
                        cboType.ClearSelection();
                        cboType.Items.FindByValue(editFilter.RuleType.ToString()).Selected = true;
                    }
                }
            }

            cmdSaveFilter.Click += OnSaveFilterClick;
        }



        private void OnSaveFilterClick(object sender, EventArgs e)
        {
            var ipf = new IPFilterInfo();
            ipf.IPAddress = txtFirstIP.Text;
            ipf.SubnetMask = txtSubnet.Text;
            ipf.RuleType = Convert.ToInt32(cboType.SelectedValue);

            if ((ipf.IPAddress == "127.0.0.1" || ipf.IPAddress == "localhost" || ipf.IPAddress == "::1" || ipf.IPAddress=="*" ) && ipf.RuleType == 2)
            {
                Skin.AddModuleMessage(this, Localization.GetString("CannotDeleteLocalhost.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            if (IPFilterController.Instance.IsAllowableDeny(Request.UserHostAddress, ipf) == false)
            {
                Skin.AddModuleMessage(this, Localization.GetString("CannotDeleteIPInUse.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return;
            }

            if (!String.IsNullOrEmpty(Request.QueryString["IPFilterID"]))
            {
                ipf.IPFilterID = Convert.ToInt32(Request.QueryString["IPFilterID"]);
                IPFilterController.Instance.UpdateIPFilter(ipf);
            }
            else
            {
                IPFilterController.Instance.AddIPFilter(ipf);
            }
            Response.Redirect(Globals.NavigateURL(TabId), true);
        }
    }
}