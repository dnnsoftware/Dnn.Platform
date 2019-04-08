#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Log.EventLog;
using System;
using System.Web.UI;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    public partial class DataConsent : UserModuleBase
    {
        public string DeleteMeConfirmString
        {
            get
            {
                switch (PortalSettings.GdprUserDeleteAction)
                {
                    case PortalSettings.UserDeleteAction.DelayedHardDelete:
                        return LocalizeString("DelayedHardDelete.Confirm");
                    case PortalSettings.UserDeleteAction.HardDelete:
                        return LocalizeString("HardDelete.Confirm");
                    case PortalSettings.UserDeleteAction.Anonymize:
                        return LocalizeString("Anonymize.Confirm");
                }
                return "";
            }
        }

        #region Delegate and event

        public delegate void DataConsentEventHandler(object sender, DataConsentEventArgs e);
        public event DataConsentEventHandler DataConsentCompleted;
        public void OnDataConsentComplete(DataConsentEventArgs e)
        {
            DataConsentCompleted?.Invoke(this, e);
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdCancel.Click += cmdCancel_Click;
            cmdSubmit.Click += cmdSubmit_Click;
            cmdDeleteMe.Click += cmdDeleteMe_Click;
            if (!Page.IsPostBack)
            {
                chkAgree.Checked = false;
                cmdSubmit.Enabled = false;
                pnlNoAgreement.Visible = false;
            }
        }
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Cancelled));
        }

        private void cmdSubmit_Click(object sender, EventArgs e)
        {
            if (chkAgree.Checked)
            {
                UserController.UserAgreedToTerms(User);
                User.HasAgreedToTerms = true;
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.Consented));
            }
        }

        private void cmdDeleteMe_Click(object sender, EventArgs e)
        {
            var success = false;
            switch (PortalSettings.GdprUserDeleteAction)
            {
                case PortalSettings.UserDeleteAction.DelayedHardDelete:
                    var user = User;
                    success = UserController.DeleteUser(ref user, true, false);
                    break;
                case PortalSettings.UserDeleteAction.HardDelete:
                    success = UserController.RemoveUser(User);
                    break;
                case PortalSettings.UserDeleteAction.Anonymize:
                    UserController.AnonymizeUser(User);
                    success = true;
                    break;
            }
            if (success)
            {
                PortalSecurity.Instance.SignOut();
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.RemovedAccount));
            }
            else
            {
                OnDataConsentComplete(new DataConsentEventArgs(DataConsentStatus.FailedToRemoveAccount));
            }
        }

        private void LogSuccess()
        {
            LogResult(string.Empty);
        }

        private void LogFailure(string reason)
        {
            LogResult(reason);
        }

        private void LogResult(string message)
        {
            var portalSecurity = PortalSecurity.Instance;

            var log = new LogInfo
            {
                LogPortalID = PortalSettings.PortalId,
                LogPortalName = PortalSettings.PortalName,
                LogUserID = UserId,
                LogUserName = portalSecurity.InputFilter(User.Username, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup)
            };

            if (string.IsNullOrEmpty(message))
            {
                log.LogTypeKey = "DATA_CONSENT_SUCCESS";
            }
            else
            {
                log.LogTypeKey = "DATA_CONSENT_FAILURE";
                log.LogProperties.Add(new LogDetailInfo("Cause", message));
            }

            LogController.Instance.AddLog(log);
        }

        #region DataConsentEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DataConsentEventArgs class provides a customised EventArgs class for
        /// the DataConsent Event
        /// </summary>
        public class DataConsentEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new DataConsentEventArgs
            /// </summary>
            /// <param name="status">The Data Consent Status</param>
            public DataConsentEventArgs(DataConsentStatus status)
            {
                Status = status;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the Update Status
            /// </summary>
            public DataConsentStatus Status { get; set; }
        }

        public enum DataConsentStatus
        {
            Consented,
            Cancelled,
            RemovedAccount,
            FailedToRemoveAccount
        }

        #endregion

    }
}