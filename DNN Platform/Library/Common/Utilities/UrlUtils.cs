#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.UI;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Common.Utilities
{
    public class UrlUtils
    {
        public static string Combine(string baseUrl, string relativeUrl)
        {
            if (baseUrl.Length == 0)
                return relativeUrl;
            if (relativeUrl.Length == 0)
                return baseUrl;
            return string.Format("{0}/{1}", baseUrl.TrimEnd(new[] { '/', '\\' }), relativeUrl.TrimStart(new[] { '/', '\\' }));
        }

        public static string DecodeParameter(string value)
        {
            value = value.Replace("-", "+").Replace("_", "/").Replace("$", "=");
            byte[] arrBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(arrBytes);
        }

        public static string DecryptParameter(string value)
        {
            return DecryptParameter(value, PortalController.Instance.GetCurrentPortalSettings().GUID.ToString());
        }

        public static string DecryptParameter(string value, string encryptionKey)
        {
            var objSecurity = new PortalSecurity();
            //[DNN-8257] - Can't do URLEncode/URLDecode as it introduces issues on decryption (with / = %2f), so we use a modifed Base64
            var toDecrypt = new StringBuilder(value);
            toDecrypt.Replace("_", "/");
            toDecrypt.Replace("-", "+");
            toDecrypt.Replace("%3d", "=");
            return objSecurity.Decrypt(encryptionKey, toDecrypt.ToString());
        }

        public static string EncodeParameter(string value)
        {
            byte[] arrBytes = Encoding.UTF8.GetBytes(value);
            var toEncode = new StringBuilder(Convert.ToBase64String(arrBytes));
            toEncode.Replace("+", "-");
            toEncode.Replace("/", "_");
            toEncode.Replace("=", "$");
            return toEncode.ToString();
        }

        public static string EncryptParameter(string value)
        {
            return EncryptParameter(value, PortalController.Instance.GetCurrentPortalSettings().GUID.ToString());
        }

        public static string EncryptParameter(string value, string encryptionKey)
        {
            var objSecurity = new PortalSecurity();
            var parameterValue = new StringBuilder(objSecurity.Encrypt(encryptionKey, value));

            //[DNN-8257] - Can't do URLEncode/URLDecode as it introduces issues on decryption (with / = %2f), so we use a modifed Base64
            parameterValue.Replace("/", "_");
            parameterValue.Replace("+", "-");
            parameterValue.Replace("=", "%3d");
            return parameterValue.ToString();
        }

        public static string GetParameterName(string pair)
        {
            string[] nameValues = pair.Split('=');
            return nameValues[0];
        }

        public static string GetParameterValue(string pair)
        {
            string[] nameValues = pair.Split('=');
            if (nameValues.Length > 1)
            {
                return nameValues[1];
            }
            return "";
        }

        /// <summary>
        /// getQSParamsForNavigateURL builds up a new querystring. This is necessary
        /// in order to prep for navigateUrl.
        /// we don't ever want a tabid, a ctl and a language parameter in the qs
        /// either, the portalid param is not allowed when the tab is a supertab
        /// (because NavigateUrl adds the portalId param to the qs)
        /// </summary>
        /// <history>
        ///     [erikvb]   20070814    added
        /// </history>
        public static string[] GetQSParamsForNavigateURL()
        {
            string returnValue = "";
            var coll = HttpContext.Current.Request.QueryString;
            string[] keys = coll.AllKeys;
            for (var i = 0; i <= keys.GetUpperBound(0); i++)
            {
                if (keys[i] != null)
                {
                    switch (keys[i].ToLower())
                    {
                        case "tabid":
                        case "ctl":
                        case "language":
                            //skip parameter
                            break;
                        default:
                            if ((keys[i].ToLower() == "portalid") && Globals.GetPortalSettings().ActiveTab.IsSuperTab)
                            {
                                //skip parameter
                                //navigateURL adds portalid to querystring if tab is superTab

                            }
                            else
                            {
                                string[] values = coll.GetValues(i);
                                for (int j = 0; j <= values.GetUpperBound(0); j++)
                                {
                                    if (!String.IsNullOrEmpty(returnValue))
                                    {
                                        returnValue += "&";
                                    }
                                    returnValue += keys[i] + "=" + values[j];
                                }
                            }
                            break;
                    }
                }
            }
            //return the new querystring as a string array
            return returnValue.Split('&');

        }

        /// <summary>
        /// check if connection is HTTPS
        /// or is HTTP but with ssloffload enabled on a secure page
        /// </summary>
        /// <param name="request">current request</param>
        /// <returns>true if HTTPS or if HTTP with an SSL offload header value, false otherwise</returns>
        public static bool IsSecureConnectionOrSslOffload(HttpRequest request)
        {
            if (request.IsSecureConnection)
            {
                return true;
            }
            string ssloffloadheader = HostController.Instance.GetString("SSLOffloadHeader", "");
            //if the ssloffloadheader variable has been set check to see if a request header with that type exists
            if (!string.IsNullOrEmpty(ssloffloadheader))
            {
                string ssloffload = request.Headers[ssloffloadheader];
                if (!string.IsNullOrEmpty(ssloffload))
                {
                    PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                    if (portalSettings.ActiveTab.IsSecure)
                    {
                        return true;
                    }
                    
                }
            }
            return false;
        }

        public static void OpenNewWindow(Page page, Type type, string url)
        {
            page.ClientScript.RegisterStartupScript(type, "DotNetNuke.NewWindow", string.Format("<script>window.open('{0}','new')</script>", url));
        }

        public static string PopUpUrl(string url, Control control, PortalSettings portalSettings, bool onClickEvent, bool responseRedirect)
        {
            return PopUpUrl(url, control, portalSettings, onClickEvent, responseRedirect, 550, 950);
        }       

        public static string PopUpUrl(string url, Control control, PortalSettings portalSettings, bool onClickEvent, bool responseRedirect, int windowHeight, int windowWidth)
        {
            return PopUpUrl(url, control, portalSettings, onClickEvent, responseRedirect, windowHeight, windowWidth, true, string.Empty);
        }

        public static string PopUpUrl(string url, PortalSettings portalSettings, bool onClickEvent, bool responseRedirect, int windowHeight, int windowWidth)
        {
            return PopUpUrl(url, null, portalSettings, onClickEvent, responseRedirect, windowHeight, windowWidth, true, string.Empty);
        }

        public static string PopUpUrl(string url, Control control, PortalSettings portalSettings, bool onClickEvent, bool responseRedirect, int windowHeight, int windowWidth, bool refresh, string closingUrl)
        {
	        
            if (UrlUtils.IsSecureConnectionOrSslOffload(HttpContext.Current.Request))
            {
                url = url.Replace("http://", "https://");
            }
            var popUpUrl = url;
            //ensure delimiters are not used
	        if (!popUpUrl.Contains("dnnModal.show"))
	        {
				popUpUrl = Uri.EscapeUriString(url);
		        popUpUrl = popUpUrl.Replace("\"", "");
		        popUpUrl = popUpUrl.Replace("'", "");
	        }

            var delimiter = popUpUrl.Contains("?") ? "&" : "?";
            var popUpScriptFormat = String.Empty;

            //create a the querystring for use on a Response.Redirect
            if (responseRedirect)
            {
                popUpScriptFormat = "{0}{1}popUp=true";
                popUpUrl = String.Format(popUpScriptFormat, popUpUrl, delimiter);
            }
            else
            {
                if (!popUpUrl.Contains("dnnModal.show"))
                {
                    popUpScriptFormat = "dnnModal.show('{0}{1}popUp=true',/*showReturn*/{2},{3},{4},{5},'{6}')";
                    closingUrl = (closingUrl != Null.NullString) ? closingUrl : String.Empty;
                    popUpUrl = "javascript:" + String.Format(popUpScriptFormat, popUpUrl, delimiter, onClickEvent.ToString().ToLower(), windowHeight, windowWidth, refresh.ToString().ToLower(), closingUrl);
                }
                else
                {
                    //Determines if the resulting JS call should return or not.
                    if (popUpUrl.Contains("/*showReturn*/false"))
                    {
                        popUpUrl = popUpUrl.Replace("/*showReturn*/false", "/*showReturn*/" + onClickEvent.ToString().ToLower());
                    }
                    else
                    {
                        popUpUrl = popUpUrl.Replace("/*showReturn*/true", "/*showReturn*/" + onClickEvent.ToString().ToLower());
                    }
                }
            }

            // Removes the javascript txt for onClick scripts
            if (onClickEvent && popUpUrl.StartsWith("javascript:")) popUpUrl = popUpUrl.Replace("javascript:", "");

            return popUpUrl;
        }

        public static string ClosePopUp(Boolean refresh, string url, bool onClickEvent)
        {
            var closePopUpStr = "dnnModal.closePopUp({0}, {1})";
            closePopUpStr = "javascript:" + string.Format(closePopUpStr, refresh.ToString().ToLower(), "'" + url + "'");

            // Removes the javascript txt for onClick scripts)
            if (onClickEvent && closePopUpStr.StartsWith("javascript:")) closePopUpStr = closePopUpStr.Replace("javascript:", "");

            return closePopUpStr;
        }

        public static string ReplaceQSParam(string url, string param, string newValue)
        {
            if (Host.UseFriendlyUrls)
            {
                return Regex.Replace(url, "(.*)(" + param + "/)([^/]+)(/.*)", "$1$2" + newValue + "$4", RegexOptions.IgnoreCase);
            }
            else
            {
                return Regex.Replace(url, "(.*)(&|\\?)(" + param + "=)([^&\\?]+)(.*)", "$1$2$3" + newValue + "$5", RegexOptions.IgnoreCase);
            }
        }

        public static string StripQSParam(string url, string param)
        {
            if (Host.UseFriendlyUrls)
            {
                return Regex.Replace(url, "(.*)(" + param + "/[^/]+/)(.*)", "$1$3", RegexOptions.IgnoreCase);
            }
            else
            {
                return Regex.Replace(url, "(.*)(&|\\?)(" + param + "=)([^&\\?]+)([&\\?])?(.*)", "$1$2$6", RegexOptions.IgnoreCase).Replace("(.*)([&\\?]$)", "$1");
            }
        }

        public static string ValidReturnUrl(string url)
        {
            //redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
            if (url != null && url.Contains("://"))
            {
                url = "";
            }
            return url;
        }

        //Whether current page is show in popup.
        public static bool InPopUp()
        {
            return HttpContext.Current != null && HttpContext.Current.Request.Url.ToString().Contains("popUp=true");
        }
    }
}
