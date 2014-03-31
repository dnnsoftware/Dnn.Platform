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
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Vendors;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditAffiliate PortalModuleBase is used to add/edit an Affiliate
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/21/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class EditAffiliate : PortalModuleBase
    {

        #region Private Members

        private int AffiliateId = -1;
        private int VendorId = -1;

        public new int PortalId
        {
            get
            {
				if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                {
                    return -1;
                }
                return PortalSettings.PortalId;
            }
        }

        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/21/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdCancel.Click += OnCancelClick;
            cmdDelete.Click += OnDeleteClick;
            cmdSend.Click += OnSendClick;
            cmdUpdate.Click += OnUpdateClick;

            if ((Request.QueryString["VendorId"] != null))
            {
                VendorId = Int32.Parse(Request.QueryString["VendorId"]);
            }
			
            if ((Request.QueryString["AffilId"] != null))
            {
                AffiliateId = Int32.Parse(Request.QueryString["AffilId"]);
            }
			
            if (Page.IsPostBack == false)
            {

                var affiliateController = new AffiliateController();
                if (AffiliateId != Null.NullInteger)
                {
                    //Obtain a single row of banner information
                    var affiliate = affiliateController.GetAffiliate(AffiliateId);
                    if (affiliate != null)
                    {
                        StartDatePicker.SelectedDate = Null.IsNull(affiliate.StartDate) ? (DateTime?) null : affiliate.StartDate;
                        EndDatePicker.SelectedDate = Null.IsNull(affiliate.EndDate) ? (DateTime?)null : affiliate.EndDate;

                        txtCPC.Text = affiliate.CPC.ToString("#0.0####");
                        txtCPA.Text = affiliate.CPA.ToString("#0.0####");
                    }
                    else //security violation attempt to access item not related to this Module
                    {
                        Response.Redirect(EditUrl("VendorId", VendorId.ToString()), true);
                    }
                }
                else
                {
                    txtCPC.Text = 0.ToString("#0.0####");
                    txtCPA.Text = 0.ToString("#0.0####");

                    cmdDelete.Visible = false;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdCancel_Click runs when the Cancel Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/21/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnCancelClick(object sender, EventArgs e)
        {
            //Redirect back to the portal home page
            Response.Redirect(EditUrl("VendorId", VendorId.ToString()), true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the Delete Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/21/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnDeleteClick(object sender, EventArgs e)
        {
            if (AffiliateId != -1)
            {
                var objAffiliates = new AffiliateController();
                objAffiliates.DeleteAffiliate(AffiliateId);

                //Redirect back to the portal home page
                Response.Redirect(EditUrl("VendorId", VendorId.ToString()), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdSend_Click runs when the Send Notification Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/21/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnSendClick(object sender, EventArgs e)
        {
            var objVendors = new VendorController();

            var objVendor = objVendors.GetVendor(VendorId, PortalId);
            if (objVendor != null)
            {
                if (!Null.IsNull(objVendor.Email))
                {
                    var custom = new ArrayList
                                     {
                                         objVendor.VendorName,
                                         Globals.GetPortalDomainName(PortalSettings.PortalAlias.HTTPAlias, Request, true) + "/" + Globals.glbDefaultPage + "?AffiliateId=" + VendorId
                                     };
                    var errorMsg = Mail.SendMail(PortalSettings.Email,
                                                    objVendor.Email,
                                                    "",
                                                    Localization.GetSystemMessage(PortalSettings, "EMAIL_AFFILIATE_NOTIFICATION_SUBJECT"),
                                                    Localization.GetSystemMessage(PortalSettings, "EMAIL_AFFILIATE_NOTIFICATION_BODY", Localization.GlobalResourceFile, custom),
                                                    "",
                                                    "",
                                                    "",
                                                    "",
                                                    "",
                                                    "");
                    string strMessage;
                    if (String.IsNullOrEmpty(errorMsg))
                    {
                        strMessage = Localization.GetString("NotificationSuccess", LocalResourceFile);
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                    else
                    {
                        strMessage = Localization.GetString("NotificationFailure", LocalResourceFile);
                        strMessage = string.Format(strMessage, errorMsg);
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	9/21/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void OnUpdateClick(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                var objAffiliate = new AffiliateInfo
                                       {
                                           AffiliateId = AffiliateId,
                                           VendorId = VendorId,
                                           StartDate =  StartDatePicker.SelectedDate.HasValue ? StartDatePicker.SelectedDate.Value : Null.NullDate,
                                           EndDate = EndDatePicker.SelectedDate.HasValue ? EndDatePicker.SelectedDate.Value : Null.NullDate,
                                           CPC = double.Parse(txtCPC.Text),
                                           CPA = double.Parse(txtCPA.Text)
                                       };
                var objAffiliates = new AffiliateController();

                if (AffiliateId == -1)
                {
                    objAffiliates.AddAffiliate(objAffiliate);
                }
                else
                {
                    objAffiliates.UpdateAffiliate(objAffiliate);
                }
				
                //Redirect back to the portal home page
                Response.Redirect(EditUrl("VendorId", VendorId.ToString()), true);
            }
        }

        #endregion

    }
}