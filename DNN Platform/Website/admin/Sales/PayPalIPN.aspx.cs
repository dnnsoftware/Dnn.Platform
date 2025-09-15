// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Sales
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A page which received messages from PayPal.</summary>
    [DnnDeprecated(10, 0, 2, "No replacement")]
    public partial class PayPalIPN : PageBase
    {
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="PayPalIPN"/> class.</summary>
        public PayPalIPN()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PayPalIPN"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        public PayPalIPN(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IHostSettingsService hostSettingsService)
            : base(portalController, appStatus, hostSettings)
        {
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                bool blnValid = true;

                // string strTransactionID;
                int intRoleID = 0;
                int intPortalID = this.PortalSettings.PortalId;
                int intUserID = 0;

                // string strDescription;
                double dblAmount = 0;

                // string strEmail;
                bool blnCancel = false;
                string strPayPalID = Null.NullString;
                string strPost = "cmd=_notify-validate";
                foreach (string strName in this.Request.Form)
                {
                    string strValue = this.Request.Form[strName];
                    switch (strName)
                    {
                        case "txn_type": // get the transaction type
                            string strTransactionType = strValue;
                            switch (strTransactionType)
                            {
                                case "subscr_signup":
                                case "subscr_payment":
                                case "web_accept":
                                    break;
                                case "subscr_cancel":
                                    blnCancel = true;
                                    break;
                                default:
                                    blnValid = false;
                                    break;
                            }

                            break;
                        case "payment_status": // verify the status
                            if (strValue != "Completed")
                            {
                                blnValid = false;
                            }

                            break;
                        case "txn_id": // verify the transaction id for duplicates
                                       //                            strTransactionID = strValue;
                            break;
                        case "receiver_email": // verify the PayPalId
                            strPayPalID = strValue;
                            break;
                        case "mc_gross": // verify the price
                            dblAmount = double.Parse(strValue);
                            break;
                        case "item_number": // get the RoleID
                            intRoleID = int.Parse(strValue);

                            // RoleInfo objRole = objRoles.GetRole(intRoleID, intPortalID);
                            break;
                        case "item_name": // get the product description
                                          //                            strDescription = strValue;
                            break;
                        case "custom": // get the UserID
                            intUserID = int.Parse(strValue);
                            break;
                        case "email": // get the email
                                      //                            strEmail = strValue;
                            break;
                    }

                    // reconstruct post for postback validation
                    strPost += string.Format("&{0}={1}", Globals.HTTPPOSTEncode(strName), Globals.HTTPPOSTEncode(strValue));
                }

                // postback to verify the source
                if (blnValid)
                {
                    Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(this.PortalSettings.PortalId);
                    string strPayPalURL;

                    // Sandbox mode
                    if (settings.ContainsKey("paypalsandbox") && !string.IsNullOrEmpty(settings["paypalsandbox"]) && settings["paypalsandbox"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        strPayPalURL = "https://www.sandbox.paypal.com/cgi-bin/webscr?";
                    }
                    else
                    {
                        strPayPalURL = "https://www.paypal.com/cgi-bin/webscr?";
                    }

                    var objRequest = Globals.GetExternalRequest(strPayPalURL);
                    objRequest.Method = "POST";
                    objRequest.ContentLength = strPost.Length;
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    using (var objStream = new StreamWriter(objRequest.GetRequestStream()))
                    {
                        objStream.Write(strPost);
                    }

                    string strResponse;
                    using (var objResponse = (HttpWebResponse)objRequest.GetResponse())
                    {
                        using (var sr = new StreamReader(objResponse.GetResponseStream()))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }

                    switch (strResponse)
                    {
                        case "VERIFIED":
                            break;
                        default:
                            // possible fraud
                            blnValid = false;
                            break;
                    }
                }

                if (blnValid)
                {
                    int intAdministratorRoleId = 0;
                    string strProcessorID = Null.NullString;
                    PortalInfo objPortalInfo = PortalController.Instance.GetPortal(intPortalID);
                    if (objPortalInfo != null)
                    {
                        intAdministratorRoleId = objPortalInfo.AdministratorRoleId;
                        strProcessorID = objPortalInfo.ProcessorUserId.ToLowerInvariant();
                    }

                    if (intRoleID == intAdministratorRoleId)
                    {
                        // admin portal renewal
                        strProcessorID = this.hostSettingsService.GetString("ProcessorUserId").ToLowerInvariant();
                        float portalPrice = objPortalInfo.HostFee;
                        if ((portalPrice.ToString() == dblAmount.ToString()) && (HttpUtility.UrlDecode(strPayPalID.ToLowerInvariant()) == strProcessorID))
                        {
                            PortalController.Instance.UpdatePortalExpiry(intPortalID, PortalController.GetActivePortalLanguage(intPortalID));
                        }
                        else
                        {
                            var log = new LogInfo
                            {
                                LogPortalID = intPortalID,
                                LogPortalName = this.PortalSettings.PortalName,
                                LogUserID = intUserID,
                                LogTypeKey = EventLogController.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD.ToString(),
                            };
                            LogController.Instance.AddLog(log);
                        }
                    }
                    else
                    {
                        // user subscription
                        RoleInfo objRoleInfo = RoleController.Instance.GetRole(intPortalID, r => r.RoleID == intRoleID);
                        float rolePrice = objRoleInfo.ServiceFee;
                        float trialPrice = objRoleInfo.TrialFee;
                        if ((rolePrice.ToString() == dblAmount.ToString() || trialPrice.ToString() == dblAmount.ToString()) && (HttpUtility.UrlDecode(strPayPalID.ToLowerInvariant()) == strProcessorID))
                        {
                            RoleController.Instance.UpdateUserRole(intPortalID, intUserID, intRoleID, RoleStatus.Approved, false, blnCancel);
                        }
                        else
                        {
                            var log = new LogInfo
                            {
                                LogPortalID = intPortalID,
                                LogPortalName = this.PortalSettings.PortalName,
                                LogUserID = intUserID,
                                LogTypeKey = EventLogController.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD.ToString(),
                            };
                            LogController.Instance.AddLog(log);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }

        private void InitializeComponent()
        {
        }
    }
}
