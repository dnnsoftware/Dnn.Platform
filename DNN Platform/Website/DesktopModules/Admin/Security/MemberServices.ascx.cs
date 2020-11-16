// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Security
{
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

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MemberServices UserModuleBase is used to manage a User's services.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class MemberServices : UserModuleBase
    {
        public delegate void SubscriptionUpdatedEventHandler(object sender, SubscriptionUpdatedEventArgs e);

        public event SubscriptionUpdatedEventHandler SubscriptionUpdated;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            if (this.Request.IsAuthenticated)
            {
                Localization.LocalizeDataGrid(ref this.grdServices, this.LocalResourceFile);
                this.grdServices.DataSource = RoleController.Instance.GetUserRoles(this.UserInfo, false);
                this.grdServices.DataBind();

                // if no service available then hide options
                this.ServicesRow.Visible = this.grdServices.Items.Count > 0;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the SubscriptionUpdated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnSubscriptionUpdated(SubscriptionUpdatedEventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.SubscriptionUpdated != null)
            {
                this.SubscriptionUpdated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatExpiryDate formats the expiry date and filters out null-values.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="expiryDate">The date to format.</param>
        ///     <returns>The correctly formatted date.</returns>
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
                        formatExpiryDate = Localization.GetString("Expired", this.LocalResourceFile);
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return formatExpiryDate;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatPrice formats the Fee amount and filters out null-values.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="price">The price to format.</param>
        /// <param name="period">Period of price.</param>
        /// <param name="frequency">Frenquency of price.</param>
        ///     <returns>The correctly formatted price.</returns>
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
                        formatPrice = Localization.GetString("NoFee", this.LocalResourceFile);
                        break;
                    case "O":
                        formatPrice = this.FormatPrice(price);
                        break;
                    default:
                        formatPrice = string.Format(Localization.GetString("Fee", this.LocalResourceFile), this.FormatPrice(price), period, Localization.GetString("Frequency_" + frequency, this.LocalResourceFile));
                        break;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return formatPrice;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatTrial formats the Trial Fee amount and filters out null-values.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="price">The price to format.</param>
        /// <param name="period">Period of price.</param>
        /// <param name="frequency">Frenquency of price.</param>
        ///     <returns>The correctly formatted price.</returns>
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
                        formatTrial = Localization.GetString("NoFee", this.LocalResourceFile);
                        break;
                    case "O":
                        formatTrial = this.FormatPrice(price);
                        break;
                    default:
                        formatTrial = string.Format(
                            Localization.GetString("TrialFee", this.LocalResourceFile),
                            this.FormatPrice(price),
                            period,
                            Localization.GetString("Frequency_" + frequency, this.LocalResourceFile));
                        break;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return formatTrial;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatURL correctly formats a URL.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <returns>The correctly formatted url.</returns>
        /// -----------------------------------------------------------------------------
        protected string FormatURL()
        {
            string formatURL = Null.NullString;
            try
            {
                string serverPath = this.Request.ApplicationPath;
                if (!serverPath.EndsWith("/"))
                {
                    serverPath += "/";
                }

                formatURL = serverPath + "Register.aspx?tabid=" + this.TabId;
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return formatURL;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ServiceText gets the Service Text (Cancel or Subscribe).
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="subscribed">The service state.</param>
        ///     <param name="expiryDate">The service expiry date.</param>
        ///     <returns>The correctly formatted text.</returns>
        /// -----------------------------------------------------------------------------
        protected string ServiceText(bool subscribed, DateTime expiryDate)
        {
            string serviceText = Null.NullString;
            try
            {
                if (!subscribed)
                {
                    serviceText = Localization.GetString("Subscribe", this.LocalResourceFile);
                }
                else
                {
                    serviceText = Localization.GetString("Unsubscribe", this.LocalResourceFile);
                    if (!Null.IsNull(expiryDate))
                    {
                        if (expiryDate < DateTime.Today)
                        {
                            serviceText = Localization.GetString("Renew", this.LocalResourceFile);
                        }
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return serviceText;
        }

        protected bool ShowSubscribe(int roleID)
        {
            bool showSubscribe = Null.NullBoolean;
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);
            if (objRole.IsPublic)
            {
                PortalInfo objPortal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
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
            bool showTrial = Null.NullBoolean;
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);
            if (string.IsNullOrEmpty(objRole.TrialFrequency) || objRole.TrialFrequency == "N" || (objRole.IsPublic && objRole.ServiceFee == 0.0))
            {
                showTrial = Null.NullBoolean;
            }
            else if (objRole.IsPublic && objRole.TrialFee == 0.0)
            {
                // Use Trial?
                UserRoleInfo objUserRole = RoleController.Instance.GetUserRole(this.PortalId, this.UserInfo.UserID, roleID);
                if ((objUserRole == null) || (!objUserRole.IsTrialUsed))
                {
                    showTrial = true;
                }
            }

            return showTrial;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdRSVP.Click += this.cmdRSVP_Click;
            this.grdServices.ItemCommand += this.grdServices_ItemCommand;
            this.lblRSVP.Text = string.Empty;
        }

        protected void grdServices_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            string commandName = e.CommandName;
            int roleID = Convert.ToInt32(e.CommandArgument);
            if (commandName == Localization.GetString("Subscribe", this.LocalResourceFile) || commandName == Localization.GetString("Renew", this.LocalResourceFile))
            {
                // Subscribe
                this.Subscribe(roleID, false);
            }
            else if (commandName == Localization.GetString("Unsubscribe", this.LocalResourceFile))
            {
                // Unsubscribe
                this.Subscribe(roleID, true);
            }
            else if (commandName == "UseTrial")
            {
                // Use Trial
                this.UseTrial(roleID);
            }

            // Rebind Grid
            this.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FormatPrice formats the Fee amount and filters out null-values.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="price">The price to format.</param>
        ///     <returns>The correctly formatted price.</returns>
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
                    formatPrice = string.Empty;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return formatPrice;
        }

        private void Subscribe(int roleID, bool cancel)
        {
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);

            if (objRole.IsPublic && objRole.ServiceFee == 0.0)
            {
                RoleController.Instance.UpdateUserRole(this.PortalId, this.UserInfo.UserID, roleID, RoleStatus.Approved, false, cancel);

                // Raise SubscriptionUpdated Event
                this.OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(cancel, objRole.RoleName));
            }
            else
            {
                if (!cancel)
                {
                    this.Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + this.TabId + "&RoleID=" + roleID, true);
                }
                else
                {
                    this.Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + this.TabId + "&RoleID=" + roleID + "&cancel=1", true);
                }
            }
        }

        private void UseTrial(int roleID)
        {
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);

            if (objRole.IsPublic && objRole.TrialFee == 0.0)
            {
                RoleController.Instance.UpdateUserRole(this.PortalId, this.UserInfo.UserID, roleID, RoleStatus.Approved, false, false);

                // Raise SubscriptionUpdated Event
                this.OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(false, objRole.RoleName));
            }
            else
            {
                this.Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + this.TabId + "&RoleID=" + roleID, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRSVP_Click runs when the Subscribe to RSVP Code Roles Button is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdRSVP_Click(object sender, EventArgs e)
        {
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            // Get the RSVP code
            string code = this.txtRSVPCode.Text;
            bool rsvpCodeExists = false;
            if (!string.IsNullOrEmpty(code))
            {
                // Parse the roles
                foreach (RoleInfo objRole in RoleController.Instance.GetRoles(this.PortalSettings.PortalId))
                {
                    if (objRole.RSVPCode == code)
                    {
                        RoleController.Instance.UpdateUserRole(this.PortalId, this.UserInfo.UserID, objRole.RoleID, RoleStatus.Approved, false, false);
                        rsvpCodeExists = true;

                        // Raise SubscriptionUpdated Event
                        this.OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(false, objRole.RoleName));
                    }
                }

                if (rsvpCodeExists)
                {
                    this.lblRSVP.Text = Localization.GetString("RSVPSuccess", this.LocalResourceFile);

                    // Reset RSVP Code field
                    this.txtRSVPCode.Text = string.Empty;
                }
                else
                {
                    this.lblRSVP.Text = Localization.GetString("RSVPFailure", this.LocalResourceFile);
                }
            }

            this.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The SubscriptionUpdatedEventArgs class provides a customised EventArgs class for
        /// the SubscriptionUpdated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class SubscriptionUpdatedEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="SubscriptionUpdatedEventArgs"/> class.
            /// Constructs a new SubscriptionUpdatedEventArgs.
            /// </summary>
            /// <param name="cancel">Whether this is a subscription cancellation.</param>
            /// <param name="roleName">The role name of subscription role.</param>
            /// -----------------------------------------------------------------------------
            public SubscriptionUpdatedEventArgs(bool cancel, string roleName)
            {
                this.Cancel = cancel;
                this.RoleName = roleName;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets a value indicating whether gets and sets whether this was a cancelation.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public bool Cancel { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the RoleName that was (un)subscribed to.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public string RoleName { get; set; }
        }
    }
}
