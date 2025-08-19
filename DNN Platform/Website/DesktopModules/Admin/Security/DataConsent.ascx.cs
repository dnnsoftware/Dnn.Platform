// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which handles a user's consent to the site's usage of their data.</summary>
    public partial class DataConsent : UserModuleBase
    {
        private readonly INavigationManager navigationManager;

        /// <summary>Initializes a new instance of the <see cref="DataConsent"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public DataConsent()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DataConsent"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public DataConsent(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager ?? Common.Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        /// <summary>A function which handles the <see cref="DataConsent.DataConsentCompleted"/> event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void DataConsentEventHandler(object sender, DataConsentEventArgs e);

        /// <summary>An event which is triggered when the user takes a data consent action.</summary>
        public event DataConsentEventHandler DataConsentCompleted;

        /// <summary>The options a user can take from the data consent screen.</summary>
        public enum DataConsentStatus
        {
            /// <summary>Consented.</summary>
            Consented = 0,

            /// <summary>Cancelled.</summary>
            Cancelled = 1,

            /// <summary>Removed account.</summary>
            RemovedAccount = 2,

            /// <summary>Failed to remove account.</summary>
            FailedToRemoveAccount = 3,
        }

        /// <summary>Gets the confirmation text to display when a user chooses for their account to be deleted.</summary>
        public string DeleteMeConfirmString
        {
            get
            {
                switch (this.PortalSettings.DataConsentUserDeleteAction)
                {
                    case PortalSettings.UserDeleteAction.Manual:
                        return this.LocalizeText("ManualDelete.Confirm");
                    case PortalSettings.UserDeleteAction.DelayedHardDelete:
                        return this.LocalizeText("DelayedHardDelete.Confirm");
                    case PortalSettings.UserDeleteAction.HardDelete:
                        return this.LocalizeText("HardDelete.Confirm");
                }

                return string.Empty;
            }
        }

        /// <summary>Gets the HTML to display with the statement agreeing to the terms and privacy policies.</summary>
        protected IHtmlString DataConsentHtml
        {
            get
            {
                var termsUrl = this.PortalSettings.TermsTabId == Null.NullInteger
                    ? this.navigationManager.NavigateURL(this.TabId, "Terms")
                    : this.navigationManager.NavigateURL(this.PortalSettings.TermsTabId);
                var privacyUrl = this.PortalSettings.PrivacyTabId == Null.NullInteger
                    ? this.navigationManager.NavigateURL(this.TabId, "Privacy")
                    : this.navigationManager.NavigateURL(this.PortalSettings.PrivacyTabId);
                var dataConsentHtml = string.Format(
                    Localization.GetString("DataConsent"),
                    termsUrl,
                    privacyUrl);
                return new HtmlString(dataConsentHtml);
            }
        }

        /// <summary>Called when any data consent action is completed.</summary>
        /// <param name="e">The details of the action.</param>
        public void OnDataConsentComplete(DataConsentEventArgs e)
        {
            this.DataConsentCompleted?.Invoke(this, e);
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.cmdCancel.Click += this.CmdCancel_Click;
            this.cmdSubmit.Click += this.CmdSubmit_Click;
            this.cmdDeleteMe.Click += this.CmdDeleteMe_Click;
            this.cmdDeleteMe.Visible = this.PortalSettings.DataConsentUserDeleteAction != PortalSettings.UserDeleteAction.Off;
            if (!this.Page.IsPostBack)
            {
                this.chkAgree.Checked = false;
                this.cmdSubmit.Enabled = false;
                this.pnlNoAgreement.Visible = false;
            }

            this.chkAgree.Attributes.Add(
                "onclick",
                $"document.getElementById({HttpUtility.JavaScriptStringEncode(this.cmdSubmit.ClientID, addDoubleQuotes: true)}).disabled = !this.checked;");
            this.cmdDeleteMe.Attributes.Add(
                "onclick",
                $"if (!confirm({HttpUtility.JavaScriptStringEncode(this.DeleteMeConfirmString, addDoubleQuotes: true)})) this.preventDefault();");
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            this.OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Cancelled));
        }

        private void CmdSubmit_Click(object sender, EventArgs e)
        {
            if (this.chkAgree.Checked)
            {
                UserController.UserAgreedToTerms(this.User);
                this.User.HasAgreedToTerms = true;
                this.OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Consented));
            }
        }

        private void CmdDeleteMe_Click(object sender, EventArgs e)
        {
            var success = false;
            switch (this.PortalSettings.DataConsentUserDeleteAction)
            {
                case PortalSettings.UserDeleteAction.Manual:
                    this.User.Membership.Approved = false;
                    UserController.UpdateUser(this.PortalSettings.PortalId, this.User);
                    UserController.UserRequestsRemoval(this.User, true);
                    success = true;
                    break;
                case PortalSettings.UserDeleteAction.DelayedHardDelete:
                    var user = this.User;
                    success = UserController.DeleteUser(ref user, true, false);
                    UserController.UserRequestsRemoval(this.User, true);
                    break;
                case PortalSettings.UserDeleteAction.HardDelete:
                    success = UserController.RemoveUser(this.User);
                    break;
            }

            if (success)
            {
                PortalSecurity.Instance.SignOut();
                this.OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.RemovedAccount));
            }
            else
            {
                this.OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.FailedToRemoveAccount));
            }
        }

        /// <summary>The DataConsentEventArgs class provides a customised EventArgs class for the <see cref="DataConsent.DataConsentCompleted"/> Event.</summary>
        public class DataConsentEventArgs
        {
            /// <summary>Initializes a new instance of the <see cref="DataConsentEventArgs"/> class.</summary>
            /// <param name="status">The Data Consent Status.</param>
            public DataConsentEventArgs(DataConsentStatus status)
            {
                this.Status = status;
            }

            /// <summary>Gets or sets the Update Status.</summary>
            public DataConsentStatus Status { get; set; }
        }
    }
}
