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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Vendors;
using DotNetNuke.UI;
using DotNetNuke.UI.Skins.Controls;

using Globals = DotNetNuke.Common.Globals;
using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditVendors PortalModuleBase is used to add/edit a Vendor
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class EditVendors : PortalModuleBase
    {

        public int VendorID = -1;

        #region Private Methods

        /// <summary>
        /// Return url redirects to the previous page, with or without filter info
        /// </summary>
        /// <param name="filter"></param>
        /// <history>
        /// 	[erikvb]	10/18/2007
        /// </history>
        private void ReturnUrl(string filter)
        {
            Response.Redirect(string.IsNullOrEmpty(filter.Trim()) ? Globals.NavigateURL() : Globals.NavigateURL(TabId, Null.NullString, "filter=" + filter), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddModuleMessage adds a module message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="type">The type of message</param>
        /// <history>
        /// 	[cnurse]	08/24/2006
        /// </history>
        /// -----------------------------------------------------------------------------
        private void AddModuleMessage(string message, ModuleMessage.ModuleMessageType type)
        {
            UI.Skins.Skin.AddModuleMessage(this, Localization.GetString(message, LocalResourceFile), type);
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += OnCancelClick;
            cmdDelete.Click += OnDeleteClick;
            cmdUpdate.Click += OnUpdateClick;

            try
            {
                var blnBanner = false;
                var blnSignup = false;

                if ((Request.QueryString["VendorID"] != null))
                {
                    VendorID = Int32.Parse(Request.QueryString["VendorID"]);
                }
                if (Request.QueryString["ctl"] != null && VendorID == -1)
                {
                    blnSignup = true;
                }
                if (Request.QueryString["banner"] != null)
                {
                    blnBanner = true;
                }
                if (Page.IsPostBack == false)
                {
                    ctlLogo.FileFilter = Globals.glbImageFileTypes;

                    addresssVendor.ModuleId = ModuleId;
                    addresssVendor.StartTabIndex = 4;

                    var objVendors = new VendorController();
                    if (VendorID != -1)
                    {
                        VendorInfo objVendor;
						if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID) && UserInfo.IsSuperUser)
                        {
							//Get Host Vendor
                            objVendor = objVendors.GetVendor(VendorID, Null.NullInteger);
                        }
                        else
                        {
							//Get Portal Vendor
                            objVendor = objVendors.GetVendor(VendorID, PortalId);
                        }
                        if (objVendor != null)
                        {
                            txtVendorName.Text = objVendor.VendorName;
                            txtFirstName.Text = objVendor.FirstName;
                            txtLastName.Text = objVendor.LastName;
                            ctlLogo.Url = objVendor.LogoFile;
                            addresssVendor.Unit = objVendor.Unit;
                            addresssVendor.Street = objVendor.Street;
                            addresssVendor.City = objVendor.City;
                            addresssVendor.Region = objVendor.Region;
                            addresssVendor.Country = objVendor.Country;
                            addresssVendor.Postal = objVendor.PostalCode;
                            addresssVendor.Telephone = objVendor.Telephone;
                            addresssVendor.Fax = objVendor.Fax;
                            addresssVendor.Cell = objVendor.Cell;
                            txtEmail.Text = objVendor.Email;
                            txtWebsite.Text = objVendor.Website;
                            chkAuthorized.Checked = objVendor.Authorized;
                            txtKeyWords.Text = objVendor.KeyWords;

                            ctlAudit.CreatedByUser = objVendor.CreatedByUser;
                            ctlAudit.CreatedDate = objVendor.CreatedDate.ToString();
                        }

                        //use dispatch method to load modules
                        var banners = ControlUtilities.LoadControl<Banners>(this, TemplateSourceDirectory.Remove(0, Globals.ApplicationPath.Length) + "/Banners.ascx");
                        banners.ID = "/Banners.ascx";
                        banners.VendorID = VendorID;
                        banners.ModuleConfiguration = ModuleConfiguration;
                        divBanners.Controls.Add(banners);


                        var affiliates = ControlUtilities.LoadControl<Affiliates>(this, TemplateSourceDirectory.Remove(0, Globals.ApplicationPath.Length) + "/Affiliates.ascx");
                        affiliates.ID = "/Affiliates.ascx";
                        affiliates.VendorID = VendorID;
                        affiliates.ModuleConfiguration = ModuleConfiguration;
                        divAffiliates.Controls.Add(affiliates);
                    }
                    else
                    {
                        chkAuthorized.Checked = true;
                        ctlAudit.Visible = false;
                        cmdDelete.Visible = false;
                        pnlBanners.Visible = false;
                        pnlAffiliates.Visible = false;
                    }
                    if (blnSignup || blnBanner)
                    {
                        rowVendor1.Visible = false;
                        rowVendor2.Visible = false;
                        pnlVendor.Visible = false;
                        cmdDelete.Visible = false;
                        ctlAudit.Visible = false;
                        if (blnBanner)
                        {
                            cmdUpdate.Visible = false;
                        }
                        else
                        {
                            cmdUpdate.Text = "Signup";
                        }
                    }
                    else
                    {
                        TabInfo objTab = TabController.Instance.GetTabByName("Vendors", Globals.IsHostTab(PortalSettings.ActiveTab.TabID) ? Null.NullInteger : PortalId);
                        if (objTab != null)
                        {
                            ViewState["filter"] = Request.QueryString["filter"];
                        }
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the Cancel button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnCancelClick(object sender, EventArgs e)
        {
            try
            {
                ReturnUrl(Convert.ToString(ViewState["filter"]));
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the Delete button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnDeleteClick(object sender, EventArgs e)
        {
            try
            {
                if (VendorID != -1)
                {
                    var objVendors = new VendorController();
                    objVendors.DeleteVendor(VendorID);
                }
                Response.Redirect(Globals.NavigateURL());
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    int portalID;
					if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                    {
                        portalID = -1;
                    }
                    else
                    {
                        portalID = PortalId;
                    }
                    var objVendors = new VendorController();
                    var objVendor = new VendorInfo
                                        {
                                            PortalId = portalID,
                                            VendorId = VendorID,
                                            VendorName = txtVendorName.Text,
                                            Unit = addresssVendor.Unit,
                                            Street = addresssVendor.Street,
                                            City = addresssVendor.City,
                                            Region = addresssVendor.Region,
                                            Country = addresssVendor.Country,
                                            PostalCode = addresssVendor.Postal,
                                            Telephone = addresssVendor.Telephone,
                                            Fax = addresssVendor.Fax,
                                            Cell = addresssVendor.Cell,
                                            Email = txtEmail.Text,
                                            Website = txtWebsite.Text,
                                            FirstName = txtFirstName.Text,
                                            LastName = txtLastName.Text,
                                            UserName = UserInfo.UserID.ToString(),
                                            LogoFile = ctlLogo.Url,
                                            KeyWords = txtKeyWords.Text,
                                            Authorized = chkAuthorized.Checked
                                        };
                    if (VendorID == -1)
                    {
                        try
                        {
                            VendorID = objVendors.AddVendor(objVendor);
                        }
                        catch
                        {
                            AddModuleMessage("ErrorAddVendor", ModuleMessage.ModuleMessageType.RedError);
                            return;
                        }
                    }
                    else
                    {
                        VendorInfo vendorCheck = objVendors.GetVendor(VendorID, portalID);
                        if (vendorCheck != null)
                        {
                            objVendors.UpdateVendor(objVendor);
                        }
                        else
                        {
                            Response.Redirect(Globals.NavigateURL());
                        }
                    }

                    if (cmdUpdate.Text == "Signup")
                    {
                        var custom = new ArrayList();
                        custom.Add(DateTime.Now.ToString());
                        custom.Add(txtVendorName.Text);
                        custom.Add(txtFirstName.Text);
                        custom.Add(txtLastName.Text);
                        custom.Add(addresssVendor.Unit);
                        custom.Add(addresssVendor.Street);
                        custom.Add(addresssVendor.City);
                        custom.Add(addresssVendor.Region);
                        custom.Add(addresssVendor.Country);
                        custom.Add(addresssVendor.Postal);
                        custom.Add(addresssVendor.Telephone);
                        custom.Add(addresssVendor.Fax);
                        custom.Add(addresssVendor.Cell);
                        custom.Add(txtEmail.Text);
                        custom.Add(txtWebsite.Text);
                        //send email to Admin
                        Mail.SendEmail(PortalSettings.Email,
                                       PortalSettings.Email,
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_ADMINISTRATOR_SUBJECT"),
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_ADMINISTRATOR_BODY", Localization.GlobalResourceFile, custom));


                        //send email to vendor
                        custom.Clear();
                        custom.Add(txtFirstName.Text);
                        custom.Add(txtLastName.Text);
                        custom.Add(txtVendorName.Text);

                        Mail.SendEmail(PortalSettings.Email,
                                       txtEmail.Text,
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_SUBJECT"),
                                       Localization.GetSystemMessage(PortalSettings, "EMAIL_VENDOR_REGISTRATION_BODY", Localization.GlobalResourceFile, custom));


                        ReturnUrl(txtVendorName.Text.Substring(0, 1));
                    }
                    else
                    {
                        ReturnUrl(Convert.ToString(ViewState["filter"]));
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

    }
}