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
using System.Text.RegularExpressions;
using System.Threading;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Vendors;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
    public partial class BannerClickThrough : PageBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
			
			//exit without incrementing count if page is indexed by crawler
            if (Request.Browser.Crawler)
            {
                return;
            }
            try
            {
                if ((Request.QueryString["vendorid"] != null) && (Request.QueryString["bannerid"] != null))
                {
                    int intVendorId = -1;
                    if (Regex.IsMatch(Request.QueryString["vendorid"], "^\\d+$"))
                    {
                        intVendorId = int.Parse(Request.QueryString["vendorid"]);
                    }
                    int intBannerId = -1;
                    if (Regex.IsMatch(Request.QueryString["bannerid"], "^\\d+$"))
                    {
                        intBannerId = int.Parse(Request.QueryString["bannerid"]);
                    }
                    int intPortalId = -1;
                    if ((Request.QueryString["portalid"] != null))
                    {
                        if (Regex.IsMatch(Request.QueryString["portalid"], "^\\d+$"))
                        {
                            intPortalId = int.Parse(Request.QueryString["portalid"]);
                        }
                    }
                    else
                    {
                        intPortalId = Globals.GetPortalSettings().PortalId;
                    }
                    if (intBannerId != -1 && intVendorId != -1 && intPortalId != -1)
                    {
                        string strURL = "~/" + Globals.glbDefaultPage;

                        var objBanners = new BannerController();
                        BannerInfo objBanner = objBanners.GetBanner(intBannerId);
                        if (objBanner != null)
                        {
                            if (objBanners.IsBannerActive(objBanner))
                            {
                                if (!Null.IsNull(objBanner.URL))
                                {
                                    strURL = Globals.LinkClick(objBanner.URL, -1, -1, false);
                                }
                                else
                                {
                                    var objVendors = new VendorController();
                                    VendorInfo objVendor = objVendors.GetVendor(objBanner.VendorId, intPortalId);
                                    if (objVendor == null)
                                    {
                                        objVendor = objVendors.GetVendor(objBanner.VendorId, Null.NullInteger);
                                    }
                                    if (objVendor != null)
                                    {
                                        if (!String.IsNullOrEmpty(objVendor.Website))
                                        {
                                            strURL = Globals.AddHTTP(objVendor.Website);
                                        }
                                    }
                                }
                                objBanners.UpdateBannerClickThrough(intBannerId, intVendorId);
                            }
                        }
                        else if (Request.UrlReferrer != null)
                        {
                            strURL = Request.UrlReferrer.ToString();
                        }
                        Response.Redirect(strURL, true);
                    }
                }
            }
            catch(ThreadAbortException)
            {
                //ignore the abort thread expcetion.
            }
            catch (Exception exc) //Page failed to load
            {
                Exceptions.ProcessPageLoadException(exc);
            }
        }
    }
}