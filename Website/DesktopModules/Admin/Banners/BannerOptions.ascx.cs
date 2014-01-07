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
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Vendors;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
    public partial class BannerOptions : PortalModuleBase
    {
        #region Private Properties

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(Request.Params["ReturnURL"]) ?? Globals.NavigateURL();
            }
        }

        #endregion

        /// <summary>
		/// The Page_Load event handler on this User Control is used to
        /// obtain a DataReader of banner information from the Banners
        /// table, and then databind the results to a templated DataList
        /// server control.  It uses the DotNetNuke.BannerDB()
        /// data component to encapsulate all data functionality.
		/// </summary>
		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdUpdate.Click += cmdUpdate_Click;
            cmdCancel.Click += cmdCancel_Click;
            DNNTxtBannerGroup.PopulateOnDemand += DNNTxtBannerGroup_PopulateOnDemand;

            try
            {
                if (!Page.IsPostBack)
                {
					//Obtain banner information from the Banners table and bind to the list control
                    var objBannerTypes = new BannerTypeController();

                    cboType.DataSource = objBannerTypes.GetBannerTypes();
                    cboType.DataBind();
                    cboType.Items.Insert(0, new ListItem(Localization.GetString("AllTypes", LocalResourceFile), "-1"));

                    if (ModuleId > 0)
                    {
                        //Get settings from the database
                        Hashtable settings = new ModuleController().GetModuleSettings(ModuleId);

                        if (optSource.Items.FindByValue(Convert.ToString(settings["bannersource"])) != null)
                        {
                            optSource.Items.FindByValue(Convert.ToString(settings["bannersource"])).Selected = true;
                        }
                        else
                        {
                            optSource.Items.FindByValue("L").Selected = true;
                        }
                        if (cboType.Items.FindByValue(Convert.ToString(settings["bannertype"])) != null)
                        {
                            cboType.Items.FindByValue(Convert.ToString(settings["bannertype"])).Selected = true;
                        }
                        if (!String.IsNullOrEmpty(Convert.ToString(settings["bannergroup"])))
                        {
                            DNNTxtBannerGroup.Text = Convert.ToString(settings["bannergroup"]);
                        }
                        if (optOrientation.Items.FindByValue(Convert.ToString(settings["orientation"])) != null)
                        {
                            optOrientation.Items.FindByValue(Convert.ToString(settings["orientation"])).Selected = true;
                        }
                        else
                        {
                            optOrientation.Items.FindByValue("V").Selected = true;
                        }
                        if (!String.IsNullOrEmpty(Convert.ToString(settings["bannercount"])))
                        {
                            txtCount.Text = Convert.ToString(settings["bannercount"]);
                        }
                        else
                        {
                            txtCount.Text = "1";
                        }
                        if (!String.IsNullOrEmpty(Convert.ToString(settings["border"])))
                        {
                            txtBorder.Text = Convert.ToString(settings["border"]);
                        }
                        else
                        {
                            txtBorder.Text = "0";
                        }
                        if (!String.IsNullOrEmpty(Convert.ToString(settings["padding"])))
                        {
                            txtPadding.Text = Convert.ToString(settings["padding"]);
                        }
                        else
                        {
                            txtPadding.Text = "4";
                        }
                        txtBorderColor.Text = Convert.ToString(settings["bordercolor"]);
                        txtRowHeight.Text = Convert.ToString(settings["rowheight"]);
                        txtColWidth.Text = Convert.ToString(settings["colwidth"]);
                        txtBannerClickThroughURL.Text = Convert.ToString(settings["bannerclickthroughurl"]);
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
					//Update settings in the database
                    var objModules = new ModuleController();

                    if (optSource.SelectedItem != null)
                    {
                        objModules.UpdateModuleSetting(ModuleId, "bannersource", optSource.SelectedItem.Value);
                    }
                    if (cboType.SelectedItem != null)
                    {
                        objModules.UpdateModuleSetting(ModuleId, "bannertype", cboType.SelectedItem.Value);
                    }
                    objModules.UpdateModuleSetting(ModuleId, "bannergroup", DNNTxtBannerGroup.Text);
                    if (optOrientation.SelectedItem != null)
                    {
                        objModules.UpdateModuleSetting(ModuleId, "orientation", optOrientation.SelectedItem.Value);
                    }
                    objModules.UpdateModuleSetting(ModuleId, "bannercount", txtCount.Text);
                    objModules.UpdateModuleSetting(ModuleId, "border", txtBorder.Text);
                    objModules.UpdateModuleSetting(ModuleId, "bordercolor", txtBorderColor.Text);
                    objModules.UpdateModuleSetting(ModuleId, "rowheight", txtRowHeight.Text);
                    objModules.UpdateModuleSetting(ModuleId, "colwidth", txtColWidth.Text);
                    objModules.UpdateModuleSetting(ModuleId, "padding", txtPadding.Text);
                    objModules.UpdateModuleSetting(ModuleId, "bannerclickthroughurl", txtBannerClickThroughURL.Text);

                    //Redirect back to the portal home page
                    Response.Redirect(ReturnURL, true);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(ReturnURL, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DNNTxtBannerGroup_PopulateOnDemand runs when something is entered on the
        /// BannerGroup field
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[vmasanas]	9/29/2006	Implement a callback to display current groups
        ///  to user so the BannerGroup can be easily selected
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void DNNTxtBannerGroup_PopulateOnDemand(object source, DNNTextSuggestEventArgs e)
        {
            DataTable dt;
            DNNNode objNode;

            var objBanners = new BannerController();
            dt = objBanners.GetBannerGroups(PortalId);
            DataRow[] dr;
            dt.CaseSensitive = false;
            dr = dt.Select("GroupName like '" + e.Text + "%'");
            foreach (DataRow d in dr)
            {
                objNode = new DNNNode(d["GroupName"].ToString());
                objNode.ID = e.Nodes.Count.ToString();
                e.Nodes.Add(objNode);
            }
        }
    }
}