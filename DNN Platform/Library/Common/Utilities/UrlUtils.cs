// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Skins;
    using Microsoft.Extensions.DependencyInjection;

    public class UrlUtils
    {
        private static readonly INavigationManager _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();

        public static string Combine(string baseUrl, string relativeUrl)
        {
            if (baseUrl.Length == 0)
            {
                return relativeUrl;
            }

            if (relativeUrl.Length == 0)
            {
                return baseUrl;
            }

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
            var objSecurity = PortalSecurity.Instance;

            // [DNN-8257] - Can't do URLEncode/URLDecode as it introduces issues on decryption (with / = %2f), so we use a modifed Base64
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
            var objSecurity = PortalSecurity.Instance;
            var parameterValue = new StringBuilder(objSecurity.Encrypt(encryptionKey, value));

            // [DNN-8257] - Can't do URLEncode/URLDecode as it introduces issues on decryption (with / = %2f), so we use a modifed Base64
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

            return string.Empty;
        }

        /// <summary>
        /// getQSParamsForNavigateURL builds up a new querystring. This is necessary
        /// in order to prep for navigateUrl.
        /// we don't ever want a tabid, a ctl and a language parameter in the qs
        /// either, the portalid param is not allowed when the tab is a supertab
        /// (because NavigateUrl adds the portalId param to the qs).
        /// </summary>
        /// <returns></returns>
        public static string[] GetQSParamsForNavigateURL()
        {
            string returnValue = string.Empty;
            var coll = HttpContext.Current.Request.QueryString;
            string[] keys = coll.AllKeys;
            for (var i = 0; i <= keys.GetUpperBound(0); i++)
            {
                if (keys[i] != null)
                {
                    switch (keys[i].ToLowerInvariant())
                    {
                        case "tabid":
                        case "ctl":
                        case "language":
                            // skip parameter
                            break;
                        default:
                            if (keys[i].Equals("portalid", StringComparison.InvariantCultureIgnoreCase) && Globals.GetPortalSettings().ActiveTab.IsSuperTab)
                            {
                                // skip parameter
                                // navigateURL adds portalid to querystring if tab is superTab
                            }
                            else
                            {
                                string[] values = coll.GetValues(i);
                                for (int j = 0; j <= values.GetUpperBound(0); j++)
                                {
                                    if (!string.IsNullOrEmpty(returnValue))
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

            // return the new querystring as a string array
            return returnValue.Split('&');
        }

        /// <summary>
        /// check if connection is HTTPS
        /// or is HTTP but with ssloffload enabled on a secure page.
        /// </summary>
        /// <param name="request">current request.</param>
        /// <returns>true if HTTPS or if HTTP with an SSL offload header value, false otherwise.</returns>
        public static bool IsSecureConnectionOrSslOffload(HttpRequest request)
        {
            return request.IsSecureConnection || IsSslOffloadEnabled(request);
        }

        public static bool IsSslOffloadEnabled(HttpRequest request)
        {
            var ssloffloadheader = HostController.Instance.GetString("SSLOffloadHeader", string.Empty);

            // if the ssloffloadheader variable has been set check to see if a request header with that type exists
            if (!string.IsNullOrEmpty(ssloffloadheader))
            {
                var ssloffloadValue = string.Empty;
                if (ssloffloadheader.Contains(":"))
                {
                    var settingParts = ssloffloadheader.Split(':');
                    ssloffloadheader = settingParts[0];
                    ssloffloadValue = settingParts[1];
                }

                string ssloffload = request.Headers[ssloffloadheader];
                if (!string.IsNullOrEmpty(ssloffload) && (string.IsNullOrWhiteSpace(ssloffloadValue) || ssloffloadValue == ssloffload))
                {
                    return true;
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

            // ensure delimiters are not used
            if (!popUpUrl.Contains("dnnModal.show"))
            {
                popUpUrl = Uri.EscapeUriString(url);
                popUpUrl = popUpUrl.Replace("\"", string.Empty);
                popUpUrl = popUpUrl.Replace("'", string.Empty);
            }

            var delimiter = popUpUrl.Contains("?") ? "&" : "?";
            var popUpScriptFormat = string.Empty;

            // create a the querystring for use on a Response.Redirect
            if (responseRedirect)
            {
                popUpScriptFormat = "{0}{1}popUp=true";
                popUpUrl = string.Format(popUpScriptFormat, popUpUrl, delimiter);
            }
            else
            {
                if (!popUpUrl.Contains("dnnModal.show"))
                {
                    popUpScriptFormat = "dnnModal.show('{0}{1}popUp=true',/*showReturn*/{2},{3},{4},{5},'{6}')";
                    closingUrl = (closingUrl != Null.NullString) ? closingUrl : string.Empty;
                    popUpUrl = "javascript:" + string.Format(popUpScriptFormat, popUpUrl, delimiter, onClickEvent.ToString().ToLowerInvariant(), windowHeight, windowWidth, refresh.ToString().ToLower(), closingUrl);
                }
                else
                {
                    // Determines if the resulting JS call should return or not.
                    if (popUpUrl.Contains("/*showReturn*/false"))
                    {
                        popUpUrl = popUpUrl.Replace("/*showReturn*/false", "/*showReturn*/" + onClickEvent.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        popUpUrl = popUpUrl.Replace("/*showReturn*/true", "/*showReturn*/" + onClickEvent.ToString().ToLowerInvariant());
                    }
                }
            }

            // Removes the javascript txt for onClick scripts
            if (onClickEvent && popUpUrl.StartsWith("javascript:"))
            {
                popUpUrl = popUpUrl.Replace("javascript:", string.Empty);
            }

            return popUpUrl;
        }

        public static string ClosePopUp(bool refresh, string url, bool onClickEvent)
        {
            var closePopUpStr = "dnnModal.closePopUp({0}, {1})";
            closePopUpStr = "javascript:" + string.Format(closePopUpStr, refresh.ToString().ToLowerInvariant(), "'" + url + "'");

            // Removes the javascript txt for onClick scripts)
            if (onClickEvent && closePopUpStr.StartsWith("javascript:"))
            {
                closePopUpStr = closePopUpStr.Replace("javascript:", string.Empty);
            }

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
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    // DNN-10193: returns the same value as passed in rather than empty string
                    return url;
                }

                url = url.Replace("\\", "/");
                if (url.ToLowerInvariant().Contains("data:"))
                {
                    return string.Empty;
                }

                // clean the return url to avoid possible XSS attack.
                var cleanUrl = PortalSecurity.Instance.InputFilter(url, PortalSecurity.FilterFlag.NoScripting);
                if (url != cleanUrl)
                {
                    return string.Empty;
                }

                // redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
                var urlWithNoQuery = url;
                if (urlWithNoQuery.Contains("?"))
                {
                    urlWithNoQuery = urlWithNoQuery.Substring(0, urlWithNoQuery.IndexOf("?", StringComparison.InvariantCultureIgnoreCase));
                }

                if (urlWithNoQuery.Contains("://"))
                {
                    var portalSettings = PortalSettings.Current;
                    var aliasWithHttp = Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias);
                    var uri1 = new Uri(url);
                    var uri2 = new Uri(aliasWithHttp);

                    // protocol switching (HTTP <=> HTTPS) is allowed by not being checked here
                    if (!string.Equals(uri1.DnsSafeHost, uri2.DnsSafeHost, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return string.Empty;
                    }

                    // this check is mainly for child portals
                    if (!uri1.AbsolutePath.StartsWith(uri2.AbsolutePath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return string.Empty;
                    }
                }

                while (url.StartsWith("///"))
                {
                    url = url.Substring(1);
                }

                if (url.StartsWith("//"))
                {
                    var urlWithNoProtocol = url.Substring(2);
                    var portalSettings = PortalSettings.Current;

                    // note: this can redirict from parent to childe and vice versa
                    if (!urlWithNoProtocol.StartsWith(portalSettings.PortalAlias.HTTPAlias + "/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return string.Empty;
                    }
                }

                return url;
            }
            catch (UriFormatException)
            {
                return string.Empty;
            }
        }

        // Whether current page is show in popup.
        public static bool InPopUp()
        {
            return HttpContext.Current != null && HttpContext.Current.Request.Url.ToString().IndexOf("popUp=true", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsPopUp(string url)
        {
            return url.IndexOf("popUp=true", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Redirect current response to 404 error page or output 404 content if error page not defined.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="portalSetting"></param>
        public static void Handle404Exception(HttpResponse response, PortalSettings portalSetting)
        {
            if (portalSetting?.ErrorPage404 > Null.NullInteger)
            {
                response.Redirect(_navigationManager.NavigateURL(portalSetting.ErrorPage404, string.Empty, "status=404"));
            }
            else
            {
                response.ClearContent();
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 404;
                response.Status = "404 Not Found";
                response.Write("404 Not Found");
                response.End();
            }
        }
    }
}
