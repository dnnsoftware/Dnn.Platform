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
using System.Collections.Generic;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Modules.Admin.Security
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MemberServices UserModuleBase is used to manage a User's services
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	03/03/2006
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class MemberServices : UserModuleBase
    {
        #region Delegates

        public delegate void SubscriptionUpdatedEventHandler(object sender, SubscriptionUpdatedEventArgs e);

        #endregion

		#region "Events"

       public event SubscriptionUpdatedEventHandler SubscriptionUpdated;

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatPrice formats the Fee amount and filters out null-values
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="price">The price to format</param>
        ///	<returns>The correctly formatted price</returns>
        /// <history>
        /// 	[cnurse]	9/13/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private string FormatPrice(float price)
        {
            string formatPrice = Null.NullString;
            try
            {
                if (price != Null.NullSingle)
                {
                    formatPrice = price.ToString("##0.00");
                }
                else
                {
                    formatPrice = "";
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatPrice;
        }

        private void Subscribe(int roleID, bool cancel)
        {
            var objRoles = new RoleController();
            RoleInfo objRole = TestableRoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == roleID);

            if (objRole.IsPublic && objRole.ServiceFee == 0.0)
            {
                objRoles.UpdateUserRole(PortalId, UserInfo.UserID, roleID, cancel);

                //Raise SubscriptionUpdated Event
                OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(cancel, objRole.RoleName));
            }
            else
            {
                if (!cancel)
                {
                    Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + TabId + "&RoleID=" + roleID, true);
                }
                else
                {
                    Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + TabId + "&RoleID=" + roleID + "&cancel=1", true);
                }
            }
        }

        private void UseTrial(int roleID)
        {
            var objRoles = new RoleController();
            RoleInfo objRole = TestableRoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == roleID); ;

            if (objRole.IsPublic && objRole.TrialFee == 0.0)
            {
                objRoles.UpdateUserRole(PortalId, UserInfo.UserID, roleID, false);

                //Raise SubscriptionUpdated Event
                OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(false, objRole.RoleName));
            }
            else
            {
                Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + TabId + "&RoleID=" + roleID, true);
            }
        }

		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatExpiryDate formats the expiry date and filters out null-values
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="expiryDate">The date to format</param>
        ///	<returns>The correctly formatted date</returns>
        /// <history>
        /// 	[cnurse]	9/13/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FormatExpiryDate(DateTime expiryDate)
        {
            string formatExpiryDate = Null.NullString;
            try
            {
                if (!Null.IsNull(expiryDate))
                {
                    if (expiryDate > DateTime.Today)
                    {
                        formatExpiryDate = expiryDate.ToShortDateString();
                    }
                    else
                    {
                        formatExpiryDate = Localization.GetString("Expired", LocalResourceFile);
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatExpiryDate;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatPrice formats the Fee amount and filters out null-values
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="price">The price to format</param>
        ///	<returns>The correctly formatted price</returns>
        /// <history>
        /// 	[cnurse]	01/18/2007 Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FormatPrice(float price, int period, string frequency)
        {
            string formatPrice = Null.NullString;
            try
            {
                switch (frequency)
                {
                    case "N":
                    case "":
                        formatPrice = Localization.GetString("NoFee", LocalResourceFile);
                        break;
                    case "O":
                        formatPrice = FormatPrice(price);
                        break;
                    default:
                        formatPrice = string.Format(Localization.GetString("Fee", LocalResourceFile), FormatPrice(price), period, Localization.GetString("Frequency_" + frequency, LocalResourceFile));
                        break;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatPrice;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatTrial formats the Trial Fee amount and filters out null-values
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="price">The price to format</param>
        ///	<returns>The correctly formatted price</returns>
        /// <history>
        /// 	[cnurse]	03/28/2007 Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FormatTrial(float price, int period, string frequency)
        {
            string formatTrial = Null.NullString;
            try
            {
                switch (frequency)
                {
                    case "N":
                    case "":
                        formatTrial = Localization.GetString("NoFee", LocalResourceFile);
                        break;
                    case "O":
                        formatTrial = FormatPrice(price);
                        break;
                    default:
                        formatTrial = string.Format(Localization.GetString("TrialFee", LocalResourceFile),
                                                     FormatPrice(price),
                                                     period,
                                                     Localization.GetString("Frequency_" + frequency, LocalResourceFile));
                        break;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatTrial;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatURL correctly formats a URL
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<returns>The correctly formatted url</returns>
        /// <history>
        /// 	[cnurse]	9/13/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FormatURL()
        {
            string formatURL = Null.NullString;
            try
            {
                string serverPath = Request.ApplicationPath;
                if (!serverPath.EndsWith("/"))
                {
                    serverPath += "/";
                }
                formatURL = serverPath + "Register.aspx?tabid=" + TabId;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ServiceText gets the Service Text (Cancel or Subscribe)
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="Subscribed">The service state</param>
        ///	<returns>The correctly formatted text</returns>
        /// <history>
        /// 	[cnurse]	9/13/2004	Updated to reflect design changes for Help, 508 support
        ///                       and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string ServiceText(bool subscribed, DateTime expiryDate)
        {
            string serviceText = Null.NullString;
            try
            {
                if (!subscribed)
                {
                    serviceText = Localization.GetString("Subscribe", LocalResourceFile);
                }
                else
                {
                    serviceText = Localization.GetString("Unsubscribe", LocalResourceFile);
                    if (!Null.IsNull(expiryDate))
                    {
                        if (expiryDate < DateTime.Today)
                        {
                            serviceText = Localization.GetString("Renew", LocalResourceFile);
                        }
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return serviceText;
        }

        protected bool ShowSubscribe(int roleID)
        {
            bool showSubscribe = Null.NullBoolean;
            RoleInfo objRole = TestableRoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == roleID); ;
            if (objRole.IsPublic)
            {
                var objPortals = new PortalController();
                PortalInfo objPortal = objPortals.GetPortal(PortalSettings.PortalId);
                if (objRole.ServiceFee == 0.0)
                {
                    showSubscribe = true;
                }
                else if (objPortal != null && !string.IsNullOrEmpty(objPortal.ProcessorUserId))
                {
                    showSubscribe = true;
                }
            }
            return showSubscribe;
        }

        protected bool ShowTrial(int roleID)
        {
            var objRoles = new RoleController();
            bool showTrial = Null.NullBoolean;
            RoleInfo objRole = TestableRoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == roleID); ;
            if (string.IsNullOrEmpty(objRole.TrialFrequency) || objRole.TrialFrequency == "N" || (objRole.IsPublic && objRole.ServiceFee == 0.0))
            {
                showTrial = Null.NullBoolean;
            }
            else if (objRole.IsPublic && objRole.TrialFee == 0.0)
            {
				//Use Trial?
                UserRoleInfo objUserRole = objRoles.GetUserRole(PortalId, UserInfo.UserID, roleID);
                if ((objUserRole == null) || (!objUserRole.IsTrialUsed))
                {
                    showTrial = true;
                }
            }
            return showTrial;
        }

		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/13/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            if (Request.IsAuthenticated)
            {
                RoleController roleController = new RoleController();
                grdServices.DataSource = roleController.GetUserRoles(UserInfo, false);
                grdServices.DataBind();

                //if no service available then hide options
                ServicesRow.Visible = (grdServices.Items.Count > 0);
            }
        }

		#endregion

		#region "Event Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the SubscriptionUpdated Event
        /// </summary>
        /// <history>
        /// 	[cnurse]	01/17/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void OnSubscriptionUpdated(SubscriptionUpdatedEventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            if (SubscriptionUpdated != null)
            {
                SubscriptionUpdated(this, e);
            }
        }

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/13/2006
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdRSVP.Click += cmdRSVP_Click;
            grdServices.ItemCommand += grdServices_ItemCommand;

            try
            {
                lblRSVP.Text = "";

                //If this is the first visit to the page, localize the datalist
                if (Page.IsPostBack == false)
                {
					//Localize the Headers
                    Localization.LocalizeDataGrid(ref grdServices, LocalResourceFile);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRSVP_Click runs when the Subscribe to RSVP Code Roles Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/19/2006  created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdRSVP_Click(object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            //Get the RSVP code
            string code = txtRSVPCode.Text;
            bool rsvpCodeExists = false;
            if (!String.IsNullOrEmpty(code))
            {
				//Get the roles from the Database
                var objRoles = new RoleController();

                //Parse the roles
                foreach (RoleInfo objRole in TestableRoleController.Instance.GetRoles(PortalSettings.PortalId))
                {
                    if (objRole.RSVPCode == code)
                    {
                        objRoles.UpdateUserRole(PortalId, UserInfo.UserID, objRole.RoleID);
                        rsvpCodeExists = true;

                        //Raise SubscriptionUpdated Event
                        OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(false, objRole.RoleName));
                    }
                }
                if (rsvpCodeExists)
                {
                    lblRSVP.Text = Localization.GetString("RSVPSuccess", LocalResourceFile);
                    //Reset RSVP Code field
                    txtRSVPCode.Text = "";
                }
                else
                {
                    lblRSVP.Text = Localization.GetString("RSVPFailure", LocalResourceFile);
                }
            }
            DataBind();
        }

        protected void grdServices_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            string commandName = e.CommandName;
            int roleID = Convert.ToInt32(e.CommandArgument);
            if (commandName == Localization.GetString("Subscribe", LocalResourceFile) || commandName == Localization.GetString("Renew", LocalResourceFile))
            {
				//Subscribe
                Subscribe(roleID, false);
            }
            else if (commandName == Localization.GetString("Unsubscribe", LocalResourceFile))
            {
				//Unsubscribe
                Subscribe(roleID, true);
            }
            else if (commandName == Localization.GetString("Unsubscribe", LocalResourceFile))
            {
				//Unsubscribe
                Subscribe(roleID, true);
            }
            else if (commandName == "UseTrial")
            {
				//Use Trial
                UseTrial(roleID);
            }
			
			//Rebind Grid
            DataBind();
        }
		
		#endregion

        #region Nested type: SubscriptionUpdatedEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The SubscriptionUpdatedEventArgs class provides a customised EventArgs class for
        /// the SubscriptionUpdated Event
        /// </summary>
        /// <history>
        /// 	[cnurse]	01/17/2006  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public class SubscriptionUpdatedEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new SubscriptionUpdatedEventArgs
            /// </summary>
            /// <param name="cancel">Whether this is a subscription cancellation</param>
            /// <history>
            /// 	[cnurse]	01/17/2006  created
            /// </history>
            /// -----------------------------------------------------------------------------
            public SubscriptionUpdatedEventArgs(bool cancel, string roleName)
            {
                Cancel = cancel;
                RoleName = roleName;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets whether this was a cancelation
            /// </summary>
            /// <history>
            /// 	[cnurse]	01/17/2006  created
            /// </history>
            /// -----------------------------------------------------------------------------
            public bool Cancel { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the RoleName that was (un)subscribed to
            /// </summary>
            /// <history>
            /// 	[cnurse]	01/17/2006  created
            /// </history>
            /// -----------------------------------------------------------------------------
            public string RoleName { get; set; }
        }

        #endregion
    }
}