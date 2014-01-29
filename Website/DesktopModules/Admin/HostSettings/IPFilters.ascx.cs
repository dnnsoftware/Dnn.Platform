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
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common.Utils;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Host
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The IPFilters PortalModuleBase is used to edit the login IP filters
    /// for the application.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class IPFilters : PortalModuleBase
    {
        #region Private methods

        protected string GetEditUrl(string ipfilterId)
        {
            string editUrl = ModuleContext.EditUrl("IPFilterID", ipfilterId, "EditIPFilters");
            if (ModuleContext.PortalSettings.EnablePopUps)
            {
                editUrl = UrlUtils.PopUpUrl(editUrl, this, ModuleContext.PortalSettings, false, false);
            }
            return editUrl;
        }

        protected string ConvertCIDR(object ip, object subnet)
        {
            return NetworkUtils.FormatAsCidr(ip.ToString(), subnet.ToString());
        }

        protected string ConvertType(object ipFilterType)
        {
            int filterType = Convert.ToInt32(ipFilterType);
            switch (filterType)
            {
                case 1:
                    return ResolveUrl("images/checkmark.png");
                    break;
                case 2:
                    return ResolveUrl("images/block.png");
                    break;
            }
            return String.Empty;
        }

        protected string ConvertTypeAlt(object ipFilterType)
        {
            int filterType = Convert.ToInt32(ipFilterType);
            switch (filterType)
            {
                case 1:
                    return Localization.GetString("AllowIP.Text", LocalResourceFile);
                    break;
                case 2:
                    return Localization.GetString("DenyIP.Text", LocalResourceFile);
                    break;
            }
            return String.Empty;
        }

        private void BindFilters()
        {
            grdFilters.DataSource = IPFilterController.Instance.GetIPFilters();
            grdFilters.DataBind();
        }

        protected void DeleteFilter(object sender, EventArgs e)
        {
            //Get the index of the row to delete
            var btnDel = ((LinkButton) (sender));
            int removedIpf = Convert.ToInt32(btnDel.CommandArgument);

            IList<IPFilterInfo> currentRules = IPFilterController.Instance.GetIPFilters();

            List<IPFilterInfo> currentWithDeleteRemoved = (from p in currentRules where p.IPFilterID != removedIpf select p).ToList();

            if (IPFilterController.Instance.CanIPStillAccess(Request.UserHostAddress, currentWithDeleteRemoved) == false)
            {
                Skin.AddModuleMessage(this, Localization.GetString("CannotDelete.Text", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
            else
            {
                var ipf = new IPFilterInfo();
                ipf.IPFilterID = removedIpf;
                IPFilterController.Instance.DeleteIPFilter(ipf);
                BindFilters();
            }
        }

        private void CheckSecurity()
        {

            if (!UserInfo.IsSuperUser)
            {

                Response.Redirect(Globals.NavigateURL("Access Denied"), true);

            }

        }
        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CheckSecurity();

            cmdAddFilter.NavigateUrl = EditUrl("EditIPFilters");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                Localization.LocalizeDataGrid(ref grdFilters, LocalResourceFile);
                BindFilters();
            }
        }
    }

    #endregion
}