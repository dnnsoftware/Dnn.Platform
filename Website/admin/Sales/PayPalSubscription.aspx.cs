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
using System.Globalization;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Modules.Admin.Sales
{
    public partial class PayPalSubscription : PageBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (PayPalSubscription));
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
                UserInfo objUserInfo = null;
                int intUserID = -1;
                if (Request.IsAuthenticated)
                {
                    objUserInfo = UserController.Instance.GetCurrentUserInfo();
                    if (objUserInfo != null)
                    {
                        intUserID = objUserInfo.UserID;
                    }
                }
                int intRoleId = -1;
                if (Request.QueryString["roleid"] != null)
                {
                    intRoleId = int.Parse(Request.QueryString["roleid"]);
                }
                string strProcessorUserId = "";
                PortalInfo objPortalInfo = PortalController.Instance.GetPortal(PortalSettings.PortalId);
                if (objPortalInfo != null)
                {
                    strProcessorUserId = objPortalInfo.ProcessorUserId;
                }
                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(PortalSettings.PortalId);
                string strPayPalURL;
                if (intUserID != -1 && intRoleId != -1 && !String.IsNullOrEmpty(strProcessorUserId))
                {
                    // Sandbox mode
                    if (settings.ContainsKey("paypalsandbox") && !String.IsNullOrEmpty(settings["paypalsandbox"]) && settings["paypalsandbox"].ToLower() == "true")
                    {
                        strPayPalURL = "https://www.sandbox.paypal.com/cgi-bin/webscr?";
                    }
                    else
                    {
                        strPayPalURL = "https://www.paypal.com/cgi-bin/webscr?";
                    }

                    if (Request.QueryString["cancel"] != null)
                    {
						//build the cancellation PayPal URL
                        strPayPalURL += "cmd=_subscr-find&alias=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                    }
                    else
                    {
                        strPayPalURL += "cmd=_ext-enter";
                        RoleInfo objRole = RoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == intRoleId);
                        if (objRole.RoleID != -1)
                        {
                            int intTrialPeriod = 1;
                            if (objRole.TrialPeriod != 0)
                            {
                                intTrialPeriod = objRole.TrialPeriod;
                            }
                            int intBillingPeriod = 1;
                            if (objRole.BillingPeriod != 0)
                            {
                                intBillingPeriod = objRole.BillingPeriod;
                            }
							
							//explicitely format numbers using en-US so numbers are correctly built
                            var enFormat = new CultureInfo("en-US");
                            string strService = string.Format(enFormat.NumberFormat, "{0:#####0.00}", objRole.ServiceFee);
                            string strTrial = string.Format(enFormat.NumberFormat, "{0:#####0.00}", objRole.TrialFee);
                            if (objRole.BillingFrequency == "O" || objRole.TrialFrequency == "O")
                            {
								//build the payment PayPal URL
                                strPayPalURL += "&redirect_cmd=_xclick&business=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                                strPayPalURL += "&item_name=" +
                                                Globals.HTTPPOSTEncode(PortalSettings.PortalName + " - " + objRole.RoleName + " ( " + objRole.ServiceFee.ToString("#.##") + " " +
                                                                       PortalSettings.Currency + " )");
                                strPayPalURL += "&item_number=" + Globals.HTTPPOSTEncode(intRoleId.ToString());
                                strPayPalURL += "&no_shipping=1&no_note=1";
                                strPayPalURL += "&quantity=1";
                                strPayPalURL += "&amount=" + Globals.HTTPPOSTEncode(strService);
                                strPayPalURL += "&currency_code=" + Globals.HTTPPOSTEncode(PortalSettings.Currency);
                            }
                            else //recurring payments
                            {
								//build the subscription PayPal URL
                                strPayPalURL += "&redirect_cmd=_xclick-subscriptions&business=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                                strPayPalURL += "&item_name=" +
                                                Globals.HTTPPOSTEncode(PortalSettings.PortalName + " - " + objRole.RoleName + " ( " + objRole.ServiceFee.ToString("#.##") + " " +
                                                                       PortalSettings.Currency + " every " + intBillingPeriod + " " + GetBillingFrequencyText(objRole.BillingFrequency) + " )");
                                strPayPalURL += "&item_number=" + Globals.HTTPPOSTEncode(intRoleId.ToString());
                                strPayPalURL += "&no_shipping=1&no_note=1";
                                if (objRole.TrialFrequency != "N")
                                {
                                    strPayPalURL += "&a1=" + Globals.HTTPPOSTEncode(strTrial);
                                    strPayPalURL += "&p1=" + Globals.HTTPPOSTEncode(intTrialPeriod.ToString());
                                    strPayPalURL += "&t1=" + Globals.HTTPPOSTEncode(objRole.TrialFrequency);
                                }
                                strPayPalURL += "&a3=" + Globals.HTTPPOSTEncode(strService);
                                strPayPalURL += "&p3=" + Globals.HTTPPOSTEncode(intBillingPeriod.ToString());
                                strPayPalURL += "&t3=" + Globals.HTTPPOSTEncode(objRole.BillingFrequency);
                                strPayPalURL += "&src=1";
                                strPayPalURL += "&currency_code=" + Globals.HTTPPOSTEncode(PortalSettings.Currency);
                            }
                        }
                        var ctlList = new ListController();

                        strPayPalURL += "&custom=" + Globals.HTTPPOSTEncode(intUserID.ToString());
                        strPayPalURL += "&first_name=" + Globals.HTTPPOSTEncode(objUserInfo.Profile.FirstName);
                        strPayPalURL += "&last_name=" + Globals.HTTPPOSTEncode(objUserInfo.Profile.LastName);
                        try
                        {
                            if (objUserInfo.Profile.Country == "United States")
                            {
                                ListEntryInfo colList = ctlList.GetListEntryInfo("Region", objUserInfo.Profile.Region);
                                strPayPalURL += "&address1=" +
                                                Globals.HTTPPOSTEncode(Convert.ToString(!String.IsNullOrEmpty(objUserInfo.Profile.Unit) ? objUserInfo.Profile.Unit + " " : "") +
                                                                       objUserInfo.Profile.Street);
                                strPayPalURL += "&city=" + Globals.HTTPPOSTEncode(objUserInfo.Profile.City);
                                strPayPalURL += "&state=" + Globals.HTTPPOSTEncode(colList.Value);
                                strPayPalURL += "&zip=" + Globals.HTTPPOSTEncode(objUserInfo.Profile.PostalCode);
                            }
                        }
						catch (Exception ex)
						{
							//issue getting user address
							Logger.Error(ex);
						}
						
                        //Return URL
                        if (settings.ContainsKey("paypalsubscriptionreturn") && !string.IsNullOrEmpty(settings["paypalsubscriptionreturn"]))
                        {
                            strPayPalURL += "&return=" + Globals.HTTPPOSTEncode(settings["paypalsubscriptionreturn"]);
                        }
                        else
                        {
                            strPayPalURL += "&return=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(Request)));
                        }
						
                        //Cancellation URL
                        if (settings.ContainsKey("paypalsubscriptioncancelreturn") && !string.IsNullOrEmpty(settings["paypalsubscriptioncancelreturn"]))
                        {
                            strPayPalURL += "&cancel_return=" + Globals.HTTPPOSTEncode(settings["paypalsubscriptioncancelreturn"]);
                        }
                        else
                        {
                            strPayPalURL += "&cancel_return=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(Request)));
                        }
						
                        //Instant Payment Notification URL
                        if (settings.ContainsKey("paypalsubscriptionnotifyurl") && !string.IsNullOrEmpty(settings["paypalsubscriptionnotifyurl"]))
                        {
                            strPayPalURL += "&notify_url=" + Globals.HTTPPOSTEncode(settings["paypalsubscriptionnotifyurl"]);
                        }
                        else
                        {
                            strPayPalURL += "&notify_url=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(Request)) + "/admin/Sales/PayPalIPN.aspx");
                        }
                        strPayPalURL += "&sra=1"; //reattempt on failure
                    }
					
					//redirect to PayPal
                    Response.Redirect(strPayPalURL, true);
                }
                else
                {
                    if ((settings.ContainsKey("paypalsubscriptioncancelreturn") && !string.IsNullOrEmpty(settings["paypalsubscriptioncancelreturn"])))
                    {
                        strPayPalURL = settings["paypalsubscriptioncancelreturn"];
                    }
                    else
                    {
                        strPayPalURL = Globals.AddHTTP(Globals.GetDomainName(Request));
                    }
					
					//redirect to PayPal
                    Response.Redirect(strPayPalURL, true);
                }
            }
            catch (Exception exc) //Page failed to load
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }

        /// <summary>
        /// This methods return the text description of the Frequency value
        /// </summary>
        /// <param name="value">value of the Frequency</param>
        /// <returns>text of the Frequency</returns>
        private string GetBillingFrequencyText(string value)
        {
            var ctlEntry = new ListController();
            ListEntryInfo entry = ctlEntry.GetListEntryInfo("Frequency", value);
            return entry.Text;
        }
    }
}