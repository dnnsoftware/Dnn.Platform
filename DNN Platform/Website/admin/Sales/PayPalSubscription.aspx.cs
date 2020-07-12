// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Sales
{
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

    public partial class PayPalSubscription : PageBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PayPalSubscription));

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                UserInfo objUserInfo = null;
                int intUserID = -1;
                if (this.Request.IsAuthenticated)
                {
                    objUserInfo = UserController.Instance.GetCurrentUserInfo();
                    if (objUserInfo != null)
                    {
                        intUserID = objUserInfo.UserID;
                    }
                }

                int intRoleId = -1;
                if (this.Request.QueryString["roleid"] != null)
                {
                    intRoleId = int.Parse(this.Request.QueryString["roleid"]);
                }

                string strProcessorUserId = string.Empty;
                PortalInfo objPortalInfo = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                if (objPortalInfo != null)
                {
                    strProcessorUserId = objPortalInfo.ProcessorUserId;
                }

                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(this.PortalSettings.PortalId);
                string strPayPalURL;
                if (intUserID != -1 && intRoleId != -1 && !string.IsNullOrEmpty(strProcessorUserId))
                {
                    // Sandbox mode
                    if (settings.ContainsKey("paypalsandbox") && !string.IsNullOrEmpty(settings["paypalsandbox"]) && settings["paypalsandbox"].ToLowerInvariant() == "true")
                    {
                        strPayPalURL = "https://www.sandbox.paypal.com/cgi-bin/webscr?";
                    }
                    else
                    {
                        strPayPalURL = "https://www.paypal.com/cgi-bin/webscr?";
                    }

                    if (this.Request.QueryString["cancel"] != null)
                    {
                        // build the cancellation PayPal URL
                        strPayPalURL += "cmd=_subscr-find&alias=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                    }
                    else
                    {
                        strPayPalURL += "cmd=_ext-enter";
                        RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == intRoleId);
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

                            // explicitely format numbers using en-US so numbers are correctly built
                            var enFormat = new CultureInfo("en-US");
                            string strService = string.Format(enFormat.NumberFormat, "{0:#####0.00}", objRole.ServiceFee);
                            string strTrial = string.Format(enFormat.NumberFormat, "{0:#####0.00}", objRole.TrialFee);
                            if (objRole.BillingFrequency == "O" || objRole.TrialFrequency == "O")
                            {
                                // build the payment PayPal URL
                                strPayPalURL += "&redirect_cmd=_xclick&business=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                                strPayPalURL += "&item_name=" +
                                                Globals.HTTPPOSTEncode(this.PortalSettings.PortalName + " - " + objRole.RoleName + " ( " + objRole.ServiceFee.ToString("#.##") + " " +
                                                                       this.PortalSettings.Currency + " )");
                                strPayPalURL += "&item_number=" + Globals.HTTPPOSTEncode(intRoleId.ToString());
                                strPayPalURL += "&no_shipping=1&no_note=1";
                                strPayPalURL += "&quantity=1";
                                strPayPalURL += "&amount=" + Globals.HTTPPOSTEncode(strService);
                                strPayPalURL += "&currency_code=" + Globals.HTTPPOSTEncode(this.PortalSettings.Currency);
                            }
                            else // recurring payments
                            {
                                // build the subscription PayPal URL
                                strPayPalURL += "&redirect_cmd=_xclick-subscriptions&business=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                                strPayPalURL += "&item_name=" +
                                                Globals.HTTPPOSTEncode(this.PortalSettings.PortalName + " - " + objRole.RoleName + " ( " + objRole.ServiceFee.ToString("#.##") + " " +
                                                                       this.PortalSettings.Currency + " every " + intBillingPeriod + " " + this.GetBillingFrequencyText(objRole.BillingFrequency) + " )");
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
                                strPayPalURL += "&currency_code=" + Globals.HTTPPOSTEncode(this.PortalSettings.Currency);
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
                                                Globals.HTTPPOSTEncode(Convert.ToString(!string.IsNullOrEmpty(objUserInfo.Profile.Unit) ? objUserInfo.Profile.Unit + " " : string.Empty) +
                                                                       objUserInfo.Profile.Street);
                                strPayPalURL += "&city=" + Globals.HTTPPOSTEncode(objUserInfo.Profile.City);
                                strPayPalURL += "&state=" + Globals.HTTPPOSTEncode(colList.Value);
                                strPayPalURL += "&zip=" + Globals.HTTPPOSTEncode(objUserInfo.Profile.PostalCode);
                            }
                        }
                        catch (Exception ex)
                        {
                            // issue getting user address
                            Logger.Error(ex);
                        }

                        // Return URL
                        if (settings.ContainsKey("paypalsubscriptionreturn") && !string.IsNullOrEmpty(settings["paypalsubscriptionreturn"]))
                        {
                            strPayPalURL += "&return=" + Globals.HTTPPOSTEncode(settings["paypalsubscriptionreturn"]);
                        }
                        else
                        {
                            strPayPalURL += "&return=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(this.Request)));
                        }

                        // Cancellation URL
                        if (settings.ContainsKey("paypalsubscriptioncancelreturn") && !string.IsNullOrEmpty(settings["paypalsubscriptioncancelreturn"]))
                        {
                            strPayPalURL += "&cancel_return=" + Globals.HTTPPOSTEncode(settings["paypalsubscriptioncancelreturn"]);
                        }
                        else
                        {
                            strPayPalURL += "&cancel_return=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(this.Request)));
                        }

                        // Instant Payment Notification URL
                        if (settings.ContainsKey("paypalsubscriptionnotifyurl") && !string.IsNullOrEmpty(settings["paypalsubscriptionnotifyurl"]))
                        {
                            strPayPalURL += "&notify_url=" + Globals.HTTPPOSTEncode(settings["paypalsubscriptionnotifyurl"]);
                        }
                        else
                        {
                            strPayPalURL += "&notify_url=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(this.Request)) + "/admin/Sales/PayPalIPN.aspx");
                        }

                        strPayPalURL += "&sra=1"; // reattempt on failure
                    }

                    // redirect to PayPal
                    this.Response.Redirect(strPayPalURL, true);
                }
                else
                {
                    if (settings.ContainsKey("paypalsubscriptioncancelreturn") && !string.IsNullOrEmpty(settings["paypalsubscriptioncancelreturn"]))
                    {
                        strPayPalURL = settings["paypalsubscriptioncancelreturn"];
                    }
                    else
                    {
                        strPayPalURL = Globals.AddHTTP(Globals.GetDomainName(this.Request));
                    }

                    // redirect to PayPal
                    this.Response.Redirect(strPayPalURL, true);
                }
            }
            catch (Exception exc) // Page failed to load
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }

        private void InitializeComponent()
        {
        }

        /// <summary>
        /// This methods return the text description of the Frequency value.
        /// </summary>
        /// <param name="value">value of the Frequency.</param>
        /// <returns>text of the Frequency.</returns>
        private string GetBillingFrequencyText(string value)
        {
            var ctlEntry = new ListController();
            ListEntryInfo entry = ctlEntry.GetListEntryInfo("Frequency", value);
            return entry.Text;
        }
    }
}
