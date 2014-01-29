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
using System.IO;
using System.Net;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Modules.Admin.Sales
{
    public partial class Purchase : PortalModuleBase
    {
        private int RoleID = -1;

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

            cmdPurchase.Click += cmdPurchase_Click;
            cmdCancel.Click += cmdCancel_Click;

            try
            {
                double dblTotal;
                string strCurrency;

                if ((Request.QueryString["RoleID"] != null))
                {
                    RoleID = Int32.Parse(Request.QueryString["RoleID"]);
                }
                if (Page.IsPostBack == false)
                {
                    if (RoleID != -1)
                    {
                        RoleInfo objRole = TestableRoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == RoleID);

                        if (objRole.RoleID != -1)
                        {
                            lblServiceName.Text = objRole.RoleName;
                            if (!Null.IsNull(objRole.Description))
                            {
                                lblDescription.Text = objRole.Description;
                            }
                            if (RoleID == PortalSettings.AdministratorRoleId)
                            {
                                if (!Null.IsNull(PortalSettings.HostFee))
                                {
                                    lblFee.Text = PortalSettings.HostFee.ToString("#,##0.00");
                                }
                            }
                            else
                            {
                                if (!Null.IsNull(objRole.ServiceFee))
                                {
                                    lblFee.Text = objRole.ServiceFee.ToString("#,##0.00");
                                }
                            }
                            if (!Null.IsNull(objRole.BillingFrequency))
                            {
                                var ctlEntry = new ListController();
                                ListEntryInfo entry = ctlEntry.GetListEntryInfo("Frequency", objRole.BillingFrequency);
                                lblFrequency.Text = entry.Text;
                            }
                            txtUnits.Text = "1";
                            if (objRole.BillingFrequency == "O") //one-time fee
                            {
                                txtUnits.Enabled = false;
                            }
                        }
                        else //security violation attempt to access item not related to this Module
                        {
                            Response.Redirect(Globals.NavigateURL(), true);
                        }
                    }
					
                    //Store URL Referrer to return to portal
                    if (Request.UrlReferrer != null)
                    {
                        ViewState["UrlReferrer"] = Convert.ToString(Request.UrlReferrer);
                    }
                    else
                    {
                        ViewState["UrlReferrer"] = "";
                    }
                }
                if (RoleID == PortalSettings.AdministratorRoleId)
                {
                    strCurrency = Host.HostCurrency;
                }
                else
                {
                    strCurrency = PortalSettings.Currency;
                }
                dblTotal = Convert.ToDouble(lblFee.Text)*Convert.ToDouble(txtUnits.Text);
                lblTotal.Text = dblTotal.ToString("#.##");

                lblFeeCurrency.Text = strCurrency;
                lblTotalCurrency.Text = strCurrency;
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdPurchase_Click(Object sender, EventArgs e)
        {
            try
            {
                string strPaymentProcessor = "";
                string strProcessorUserId = "";
                string strProcessorPassword = "";

                if (Page.IsValid)
                {
                    var objPortalController = new PortalController();
                    PortalInfo objPortalInfo = objPortalController.GetPortal(PortalSettings.PortalId);
                    if (objPortalInfo != null)
                    {
                        strPaymentProcessor = objPortalInfo.PaymentProcessor;
                        strProcessorUserId = objPortalInfo.ProcessorUserId;
                        strProcessorPassword = objPortalInfo.ProcessorPassword;
                    }
                    if (strPaymentProcessor == "PayPal")
                    {
						//build secure PayPal URL
                        string strPayPalURL = "";
                        strPayPalURL = "https://www.paypal.com/xclick/business=" + Globals.HTTPPOSTEncode(strProcessorUserId);
                        strPayPalURL = strPayPalURL + "&item_name=" +
                                       Globals.HTTPPOSTEncode(PortalSettings.PortalName + " - " + lblDescription.Text + " ( " + txtUnits.Text + " units @ " + lblFee.Text + " " + lblFeeCurrency.Text +
                                                              " per " + lblFrequency.Text + " )");
                        strPayPalURL = strPayPalURL + "&item_number=" + Globals.HTTPPOSTEncode(Convert.ToString(RoleID));
                        strPayPalURL = strPayPalURL + "&quantity=1";
                        strPayPalURL = strPayPalURL + "&custom=" + Globals.HTTPPOSTEncode(UserInfo.UserID.ToString());
                        strPayPalURL = strPayPalURL + "&amount=" + Globals.HTTPPOSTEncode(lblTotal.Text);
                        strPayPalURL = strPayPalURL + "&currency_code=" + Globals.HTTPPOSTEncode(lblTotalCurrency.Text);
                        strPayPalURL = strPayPalURL + "&return=" + Globals.HTTPPOSTEncode("http://" + Globals.GetDomainName(Request));
                        strPayPalURL = strPayPalURL + "&cancel_return=" + Globals.HTTPPOSTEncode("http://" + Globals.GetDomainName(Request));
                        strPayPalURL = strPayPalURL + "&notify_url=" + Globals.HTTPPOSTEncode("http://" + Globals.GetDomainName(Request) + "/admin/Sales/PayPalIPN.aspx");
                        strPayPalURL = strPayPalURL + "&undefined_quantity=&no_note=1&no_shipping=1";

                        //redirect to PayPal
                        Response.Redirect(strPayPalURL, true);
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void cmdCancel_Click(Object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Convert.ToString(ViewState["UrlReferrer"]), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private double ConvertCurrency(string Amount, string FromCurrency, string ToCurrency)
        {
            string strPost = "Amount=" + Amount + "&From=" + FromCurrency + "&To=" + ToCurrency;
            StreamWriter objStream;
            double retValue = 0;
            try
            {
                var objRequest = Globals.GetExternalRequest("http://www.xe.com/ucc/convert.cgi");
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";

                objStream = new StreamWriter(objRequest.GetRequestStream());
                objStream.Write(strPost);
                objStream.Close();

                var objResponse = (HttpWebResponse) objRequest.GetResponse();
                StreamReader sr;
                sr = new StreamReader(objResponse.GetResponseStream());
                string strResponse = sr.ReadToEnd();
                sr.Close();

                int intPos1 = strResponse.IndexOf(ToCurrency + "</B>");
                int intPos2 = strResponse.LastIndexOf("<B>", intPos1);

                retValue = Convert.ToDouble(strResponse.Substring(intPos2 + 3, (intPos1 - intPos2) - 4));
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return retValue;
        }
    }
}