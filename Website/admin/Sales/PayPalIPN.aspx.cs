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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Modules.Admin.Sales
{
    public partial class PayPalIPN : PageBase
    {
        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                bool blnValid = true;
//                string strTransactionID;
                int intRoleID = 0;
                int intPortalID = PortalSettings.PortalId;
                int intUserID = 0;
//                string strDescription;
                double dblAmount = 0;
//                string strEmail;
                bool blnCancel = false;
                string strPayPalID = Null.NullString;
                string strPost = "cmd=_notify-validate";
                foreach (string strName in Request.Form)
                {
                    string strValue = Request.Form[strName];
                    switch (strName)
                    {
                        case "txn_type": //get the transaction type
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
                        case "payment_status": //verify the status
                            if (strValue != "Completed")
                            {
                                blnValid = false;
                            }
                            break;
                        case "txn_id": //verify the transaction id for duplicates
//                            strTransactionID = strValue;
                            break;
                        case "receiver_email": //verify the PayPalId
                            strPayPalID = strValue;
                            break;
                        case "mc_gross": // verify the price
                            dblAmount = double.Parse(strValue);
                            break;
                        case "item_number": //get the RoleID
                            intRoleID = Int32.Parse(strValue);
                            //RoleInfo objRole = objRoles.GetRole(intRoleID, intPortalID);
                            break;
                        case "item_name": //get the product description
//                            strDescription = strValue;
                            break;
                        case "custom": //get the UserID
                            intUserID = Int32.Parse(strValue);
                            break;
                        case "email": //get the email
//                            strEmail = strValue;
                            break;
                    }
                    
					//reconstruct post for postback validation
					strPost += string.Format("&{0}={1}", Globals.HTTPPOSTEncode(strName), Globals.HTTPPOSTEncode(strValue));
                }
                
				//postback to verify the source
				if (blnValid)
                {
                    Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(PortalSettings.PortalId);
                    string strPayPalURL;

                    // Sandbox mode
                    if (settings.ContainsKey("paypalsandbox") && !String.IsNullOrEmpty(settings["paypalsandbox"]) && settings["paypalsandbox"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
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
                    using (var objResponse = (HttpWebResponse) objRequest.GetResponse())
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
                            //possible fraud
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
                        strProcessorID = objPortalInfo.ProcessorUserId.ToLower();
                    }

                    if (intRoleID == intAdministratorRoleId)
                    {
						//admin portal renewal
                        strProcessorID = Host.ProcessorUserId.ToLower();
                        float portalPrice = objPortalInfo.HostFee;
                        if ((portalPrice.ToString() == dblAmount.ToString()) && (HttpUtility.UrlDecode(strPayPalID.ToLower()) == strProcessorID))
                        {
                            PortalController.Instance.UpdatePortalExpiry(intPortalID, PortalController.GetActivePortalLanguage(intPortalID));
                        }
                        else
                        {
                            var log = new LogInfo
                            {
                                LogPortalID = intPortalID,
                                LogPortalName = PortalSettings.PortalName,
                                LogUserID = intUserID,
                                LogTypeKey = EventLogController.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD.ToString()
                            };
                            LogController.Instance.AddLog(log);
                        }
                    }
                    else
                    {
						//user subscription
                        RoleInfo objRoleInfo = RoleController.Instance.GetRole(intPortalID, r => r.RoleID == intRoleID);
                        float rolePrice = objRoleInfo.ServiceFee;
                        float trialPrice = objRoleInfo.TrialFee;
                        if ((rolePrice.ToString() == dblAmount.ToString() || trialPrice.ToString() == dblAmount.ToString()) && (HttpUtility.UrlDecode(strPayPalID.ToLower()) == strProcessorID))
                        {
                            RoleController.Instance.UpdateUserRole(intPortalID, intUserID, intRoleID, RoleStatus.Approved, false, blnCancel);
                        }
                        else
                        {
                            var log = new LogInfo
                            {
                                LogPortalID = intPortalID,
                                LogPortalName = PortalSettings.PortalName,
                                LogUserID = intUserID,
                                LogTypeKey = EventLogController.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD.ToString()
                            };
                            LogController.Instance.AddLog(log);
                        }
                    }
                }
            }
            catch (Exception exc) //Page failed to load
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }
    }
}