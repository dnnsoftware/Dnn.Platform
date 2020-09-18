// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Sales
{
    using System;
    using System.IO;
    using System.Net;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    using Host = DotNetNuke.Entities.Host.Host;

    public partial class Purchase : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;
        private int RoleID = -1;

        public Purchase()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdPurchase.Click += this.cmdPurchase_Click;
            this.cmdCancel.Click += this.cmdCancel_Click;

            try
            {
                double dblTotal;
                string strCurrency;

                if (this.Request.QueryString["RoleID"] != null)
                {
                    this.RoleID = int.Parse(this.Request.QueryString["RoleID"]);
                }

                if (this.Page.IsPostBack == false)
                {
                    if (this.RoleID != -1)
                    {
                        RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == this.RoleID);

                        if (objRole.RoleID != -1)
                        {
                            this.lblServiceName.Text = objRole.RoleName;
                            if (!Null.IsNull(objRole.Description))
                            {
                                this.lblDescription.Text = objRole.Description;
                            }

                            if (this.RoleID == this.PortalSettings.AdministratorRoleId)
                            {
                                if (!Null.IsNull(this.PortalSettings.HostFee))
                                {
                                    this.lblFee.Text = this.PortalSettings.HostFee.ToString("#,##0.00");
                                }
                            }
                            else
                            {
                                if (!Null.IsNull(objRole.ServiceFee))
                                {
                                    this.lblFee.Text = objRole.ServiceFee.ToString("#,##0.00");
                                }
                            }

                            if (!Null.IsNull(objRole.BillingFrequency))
                            {
                                var ctlEntry = new ListController();
                                ListEntryInfo entry = ctlEntry.GetListEntryInfo("Frequency", objRole.BillingFrequency);
                                this.lblFrequency.Text = entry.Text;
                            }

                            this.txtUnits.Text = "1";
                            if (objRole.BillingFrequency == "O") // one-time fee
                            {
                                this.txtUnits.Enabled = false;
                            }
                        }
                        else // security violation attempt to access item not related to this Module
                        {
                            this.Response.Redirect(this._navigationManager.NavigateURL(), true);
                        }
                    }

                    // Store URL Referrer to return to portal
                    if (this.Request.UrlReferrer != null)
                    {
                        this.ViewState["UrlReferrer"] = Convert.ToString(this.Request.UrlReferrer);
                    }
                    else
                    {
                        this.ViewState["UrlReferrer"] = string.Empty;
                    }
                }

                if (this.RoleID == this.PortalSettings.AdministratorRoleId)
                {
                    strCurrency = Host.HostCurrency;
                }
                else
                {
                    strCurrency = this.PortalSettings.Currency;
                }

                dblTotal = Convert.ToDouble(this.lblFee.Text) * Convert.ToDouble(this.txtUnits.Text);
                this.lblTotal.Text = dblTotal.ToString("#.##");

                this.lblFeeCurrency.Text = strCurrency;
                this.lblTotalCurrency.Text = strCurrency;
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void InitializeComponent()
        {
        }

        private void cmdPurchase_Click(object sender, EventArgs e)
        {
            try
            {
                string strPaymentProcessor = string.Empty;
                string strProcessorUserId = string.Empty;
                string strProcessorPassword = string.Empty;

                if (this.Page.IsValid)
                {
                    PortalInfo objPortalInfo = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                    if (objPortalInfo != null)
                    {
                        strPaymentProcessor = objPortalInfo.PaymentProcessor;
                        strProcessorUserId = objPortalInfo.ProcessorUserId;
                        strProcessorPassword = objPortalInfo.ProcessorPassword;
                    }

                    if (strPaymentProcessor == "PayPal")
                    {
                        // build secure PayPal URL
                        string strPayPalURL = string.Empty;
                        strPayPalURL = "https://www.paypal.com/xclick/business=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                        strPayPalURL = strPayPalURL + "&item_name=" +
                                       Globals.HTTPPOSTEncode(this.PortalSettings.PortalName + " - " + this.lblDescription.Text + " ( " + this.txtUnits.Text + " units @ " + this.lblFee.Text + " " + this.lblFeeCurrency.Text +
                                                              " per " + this.lblFrequency.Text + " )");
                        strPayPalURL = strPayPalURL + "&item_number=" + Globals.HTTPPOSTEncode(Convert.ToString(this.RoleID));
                        strPayPalURL = strPayPalURL + "&quantity=1";
                        strPayPalURL = strPayPalURL + "&custom=" + Globals.HTTPPOSTEncode(this.UserInfo.UserID.ToString());
                        strPayPalURL = strPayPalURL + "&amount=" + Globals.HTTPPOSTEncode(this.lblTotal.Text);
                        strPayPalURL = strPayPalURL + "&currency_code=" + Globals.HTTPPOSTEncode(this.lblTotalCurrency.Text);
                        strPayPalURL = strPayPalURL + "&return=" + Globals.HTTPPOSTEncode("http://" + Globals.GetDomainName(this.Request));
                        strPayPalURL = strPayPalURL + "&cancel_return=" + Globals.HTTPPOSTEncode("http://" + Globals.GetDomainName(this.Request));
                        strPayPalURL = strPayPalURL + "&notify_url=" + Globals.HTTPPOSTEncode("http://" + Globals.GetDomainName(this.Request) + "/admin/Sales/PayPalIPN.aspx");
                        strPayPalURL = strPayPalURL + "&undefined_quantity=&no_note=1&no_shipping=1";

                        // redirect to PayPal
                        this.Response.Redirect(strPayPalURL, true);
                    }
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(Convert.ToString(this.ViewState["UrlReferrer"]), true);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private double ConvertCurrency(string Amount, string FromCurrency, string ToCurrency)
        {
            string strPost = "Amount=" + Amount + "&From=" + FromCurrency + "&To=" + ToCurrency;
            double retValue = 0;
            try
            {
                var objRequest = Globals.GetExternalRequest("http://www.xe.com/ucc/convert.cgi");
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";

                using (var objStream = new StreamWriter(objRequest.GetRequestStream()))
                {
                    objStream.Write(strPost);
                    objStream.Close();
                }

                var objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (var sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    string strResponse = sr.ReadToEnd();
                    int intPos1 = strResponse.IndexOf(ToCurrency + "</B>");
                    int intPos2 = strResponse.LastIndexOf("<B>", intPos1);

                    retValue = Convert.ToDouble(strResponse.Substring(intPos2 + 3, (intPos1 - intPos2) - 4));
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return retValue;
        }
    }
}
