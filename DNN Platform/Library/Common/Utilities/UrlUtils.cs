// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides utilities for dealing with DNN's URLs. Consider using <see cref="System.Uri"/> if applicable.</summary>
    public static partial class UrlUtils
    {
        private static INavigationManager NavigationManager => Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        /// <summary>Combines two URLs, trimming any slashes between them.</summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="relativeUrl">The URL to add to the base URL.</param>
        /// <returns>A new URL that combines <paramref name="baseUrl"/> and <paramref name="relativeUrl"/>.</returns>
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

            baseUrl = baseUrl.TrimEnd('/', '\\');
            relativeUrl = relativeUrl.TrimStart('/', '\\');
            return $"{baseUrl}/{relativeUrl}";
        }

        /// <summary>Decodes a base64 encoded value generated via <see cref="EncodeParameter"/>.</summary>
        /// <param name="value">The encoded value.</param>
        /// <returns>The decoded value.</returns>
        public static string DecodeParameter(string value)
        {
            value = value.Replace('-', '+').Replace('_', '/').Replace('$', '=');
            byte[] arrBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(arrBytes);
        }

        /// <summary>Decrypts an encrypted value generated via <see cref="EncryptParameter(string)"/>. Decrypted using the current portal's <see cref="IPortalSettings.GUID"/>.</summary>
        /// <param name="value">The encrypted value.</param>
        /// <returns>The decrypted value.</returns>
        public static string DecryptParameter(string value)
        {
            return DecryptParameter(value, PortalController.Instance.GetCurrentSettings().GUID.ToString());
        }

        /// <summary>Decrypts an encrypted value generated via <see cref="EncryptParameter(string,string)"/>.</summary>
        /// <param name="value">The encrypted value.</param>
        /// <param name="encryptionKey">The key used to encrypt the value.</param>
        /// <returns>The decrypted value.</returns>
        public static string DecryptParameter(string value, string encryptionKey)
        {
            // [DNN-8257] - Can't do URLEncode/URLDecode as it introduces issues on decryption (with / = %2f), so we use a modified Base64
            var toDecrypt = new StringBuilder(value);
            toDecrypt.Replace('_', '/');
            toDecrypt.Replace('-', '+');
            toDecrypt.Replace("%3d", "=");
            return PortalSecurity.Instance.Decrypt(encryptionKey, toDecrypt.ToString());
        }

        /// <summary>Encodes a value (using base64) for placing in a URL.</summary>
        /// <param name="value">The value to encode.</param>
        /// <returns>The encoded value.</returns>
        public static string EncodeParameter(string value)
        {
            byte[] arrBytes = Encoding.UTF8.GetBytes(value);
            var toEncode = new StringBuilder(Convert.ToBase64String(arrBytes));
            toEncode.Replace('+', '-');
            toEncode.Replace('/', '_');
            toEncode.Replace('=', '$');
            return toEncode.ToString();
        }

        /// <summary>Encrypt a parameter for placing in a URL. Encrypted using the current portal's <see cref="IPortalSettings.GUID"/>.</summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns>The encrypted value.</returns>
        public static string EncryptParameter(string value)
        {
            return EncryptParameter(value, PortalController.Instance.GetCurrentSettings().GUID.ToString());
        }

        /// <summary>Encrypt a parameter for placing in a URL.</summary>
        /// <param name="value">The value to encrypt.</param>
        /// <param name="encryptionKey">The key to use when encrypting the value. This key must be used to decrypt the value.</param>
        /// <returns>The encrypted value.</returns>
        public static string EncryptParameter(string value, string encryptionKey)
        {
            var encryptedValue = PortalSecurity.Instance.Encrypt(encryptionKey, value);
            var parameterValue = new StringBuilder(encryptedValue);

            // [DNN-8257] - Can't do URLEncode/URLDecode as it introduces issues on decryption (with / = %2f), so we use a modified Base64
            parameterValue.Replace('/', '_');
            parameterValue.Replace('+', '-');
            parameterValue.Replace("=", "%3d");
            return parameterValue.ToString();
        }

        /// <summary>Gets the name from a query string pair.</summary>
        /// <param name="pair">The pair, e.g. <c>"name=value"</c>.</param>
        /// <returns>The name.</returns>
        public static string GetParameterName(string pair)
        {
            var length = pair.IndexOf('=');
            if (length == -1)
            {
                length = pair.Length;
            }

            return pair.Substring(0, length);
        }

        /// <summary>Gets the value from a query string pair.</summary>
        /// <param name="pair">The pair, e.g. <c>"name=value"</c>.</param>
        /// <returns>The value.</returns>
        public static string GetParameterValue(string pair)
        {
            var start = pair.IndexOf('=') + 1;
            if (start == 0)
            {
                return string.Empty;
            }

            return pair.Substring(start);
        }

        /// <summary>
        /// getQSParamsForNavigateURL builds up a new querystring. This is necessary
        /// in order to prep for navigateUrl.
        /// we don't ever want a tabid, a ctl and a language parameter in the qs
        /// either, the portalid param is not allowed when the tab is a supertab
        /// (because NavigateUrl adds the portalId param to the qs).
        /// </summary>
        /// <returns>The query-string parameters collection in <c>"key=value"</c> format.</returns>
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
            return IsSecureConnectionOrSslOffload(new HttpRequestWrapper(request));
        }

        /// <summary>
        /// check if connection is HTTPS
        /// or is HTTP but with ssloffload enabled on a secure page.
        /// </summary>
        /// <param name="request">current request.</param>
        /// <returns>true if HTTPS or if HTTP with an SSL offload header value, false otherwise.</returns>
        public static bool IsSecureConnectionOrSslOffload(HttpRequestBase request)
        {
            return request.IsSecureConnection || IsSslOffloadEnabled(request);
        }

        public static bool IsSslOffloadEnabled(HttpRequest request)
        {
            return IsSslOffloadEnabled(new HttpRequestWrapper(request));
        }

        public static bool IsSslOffloadEnabled(HttpRequestBase request)
        {
            var sslOffloadHeader = HostController.Instance.GetString("SSLOffloadHeader", string.Empty);

            // if the sslOffloadHeader variable has been set check to see if a request header with that type exists
            if (string.IsNullOrEmpty(sslOffloadHeader))
            {
                return false;
            }

            var sslOffloadValue = string.Empty;
            if (sslOffloadHeader.Contains(":"))
            {
                var settingParts = sslOffloadHeader.Split(':');
                sslOffloadHeader = settingParts[0];
                sslOffloadValue = settingParts[1];
            }

            var sslOffload = request.Headers[sslOffloadHeader];
            return !string.IsNullOrEmpty(sslOffload) && (string.IsNullOrWhiteSpace(sslOffloadValue) || sslOffloadValue == sslOffload);
        }

        public static void OpenNewWindow(Page page, Type type, string url)
        {
            page.ClientScript.RegisterStartupScript(
                type,
                "DotNetNuke.NewWindow",
                $"<script>window.open({HttpUtility.JavaScriptStringEncode(url, addDoubleQuotes: true)},'new')</script>");
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

            var delimiter = popUpUrl.Contains("?") ? '&' : '?';

            // create the querystring for use on a Response.Redirect
            if (responseRedirect)
            {
                popUpUrl = $"{popUpUrl}{delimiter}popUp=true";
            }
            else
            {
                if (!popUpUrl.Contains("dnnModal.show"))
                {
                    closingUrl = (closingUrl != Null.NullString) ? closingUrl : string.Empty;
                    popUpUrl =
                        $"javascript:dnnModal.show('{HttpUtility.JavaScriptStringEncode(popUpUrl)}{delimiter}popUp=true',/*showReturn*/{onClickEvent.ToString().ToLowerInvariant()},{windowHeight},{windowWidth},{refresh.ToString().ToLower()},'{HttpUtility.JavaScriptStringEncode(closingUrl)}')";
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

        /// <summary>Creates a URL (or script) to close a pop-up.</summary>
        /// <param name="refresh">Whether to refresh the page when the pop-up is closed.</param>
        /// <param name="url">The URL.</param>
        /// <param name="onClickEvent">Whether to generate a script for an onClick event (rather than a URL with a <c>javascript:</c> protocol).</param>
        /// <returns>The URL or script.</returns>
        public static string ClosePopUp(bool refresh, string url, bool onClickEvent)
        {
            var protocol = onClickEvent ? string.Empty : "javascript:";
            var refreshBool = refresh.ToString().ToLowerInvariant();
            var urlString = HttpUtility.JavaScriptStringEncode(url);
            return $"{protocol}dnnModal.closePopUp({refreshBool}, '{urlString}')";
        }

        /// <summary>Replaces a query string parameter's value in a URL.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="param">The parameter name.</param>
        /// <param name="newValue">The parameter value.</param>
        /// <returns>The updated URL.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string ReplaceQSParam(string url, string param, string newValue)
            => ReplaceQSParam(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), url, param, newValue);

        /// <summary>Replaces a query string parameter's value in a URL.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="url">The URL.</param>
        /// <param name="param">The parameter name.</param>
        /// <param name="newValue">The parameter value.</param>
        /// <returns>The updated URL.</returns>
        public static string ReplaceQSParam(IHostSettings hostSettings, string url, string param, string newValue)
        {
            if (hostSettings.UseFriendlyUrls)
            {
                var escapedReplacementValue = newValue.Replace("$1", "$$1").Replace("$2", "$$2").Replace("$3", "$$3").Replace("$4", "$$4");
                return Regex.Replace(url, $@"(.*)({Regex.Escape(param)}/)([^/]+)(/.*)", $"$1$2{escapedReplacementValue}$4", RegexOptions.IgnoreCase);
            }
            else
            {
                var escapedReplacementValue = newValue.Replace("$1", "$$1").Replace("$2", "$$2").Replace("$3", "$$3").Replace("$4", "$$4").Replace("$5", "$$5");
                return Regex.Replace(url, $@"(.*)(&|\?)({Regex.Escape(param)}=)([^&\?]+)(.*)", $"$1$2$3{escapedReplacementValue}$5", RegexOptions.IgnoreCase);
            }
        }

        /// <summary>Removes the query string parameter with the given name from the URL.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="param">The parameter name.</param>
        /// <returns>The updated URL.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string StripQSParam(string url, string param)
            => StripQSParam(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), url, param);

        /// <summary>Removes the query string parameter with the given name from the URL.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="url">The URL.</param>
        /// <param name="param">The parameter name.</param>
        /// <returns>The updated URL.</returns>
        public static string StripQSParam(IHostSettings hostSettings, string url, string param)
        {
            if (hostSettings.UseFriendlyUrls)
            {
                return Regex.Replace(url, $"(.*)({Regex.Escape(param)}/[^/]+/)(.*)", "$1$3", RegexOptions.IgnoreCase);
            }
            else
            {
                return Regex.Replace(url, $@"(.*)(&|\?)({Regex.Escape(param)}=)([^&\?]+)([&\?])?(.*)", "$1$2$6", RegexOptions.IgnoreCase).Replace("(.*)([&\\?]$)", "$1");
            }
        }

        /// <summary>Determines whether a <paramref name="url"/> is valid as a return URL.</summary>
        /// <param name="url">The URL string.</param>
        /// <returns>The normalized return URL or <see cref="string.Empty"/>.</returns>
        public static string ValidReturnUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    // DNN-10193: returns the same value as passed in rather than empty string
                    return url;
                }

                url = url.Replace('\\', '/');
                if (url.IndexOf("data:", StringComparison.OrdinalIgnoreCase) > -1)
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
                if (urlWithNoQuery.IndexOf('?') > -1)
                {
                    urlWithNoQuery = urlWithNoQuery.Substring(0, urlWithNoQuery.IndexOf("?", StringComparison.InvariantCultureIgnoreCase));
                }

                if (urlWithNoQuery.IndexOf(':') > -1)
                {
                    var portalSettings = PortalSettings.Current;
                    var aliasWithHttp = Globals.AddHTTP(((IPortalAliasInfo)portalSettings.PortalAlias).HttpAlias);
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

                while (url.StartsWith("///", StringComparison.Ordinal))
                {
                    url = url.Substring(1);
                }

                if (url.StartsWith("//", StringComparison.Ordinal))
                {
                    var urlWithNoProtocol = url.Substring(2);
                    var portalSettings = PortalSettings.Current;

                    // note: this can redirect from parent to child and vice versa
                    if (!urlWithNoProtocol.StartsWith(((IPortalAliasInfo)portalSettings.PortalAlias).HttpAlias + "/", StringComparison.InvariantCultureIgnoreCase))
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

        /// <summary>Determines whether the current page is being shown in a pop-up.</summary>
        /// <returns><see langword="true"/> if the current page is in a pop-up, otherwise <see langword="false"/>.</returns>
        public static bool InPopUp()
        {
            return HttpContext.Current != null && IsPopUp(HttpContext.Current.Request.Url.ToString());
        }

        /// <summary>Determines whether the given URL is for a page being shown in a pop-up.</summary>
        /// <param name="url">The URL.</param>
        /// <returns><see langword="true"/> if the URL is for a page in a pop-up, otherwise <see langword="false"/>.</returns>
        public static bool IsPopUp(string url)
        {
            return url.IndexOf("popUp=true", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Redirect current response to 404 error page or output 404 content if error page not defined.</summary>
        /// <param name="response">The response.</param>
        /// <param name="portalSetting">The portal settings.</param>
        public static void Handle404Exception(HttpResponse response, PortalSettings portalSetting)
        {
            if (portalSetting?.ErrorPage404 > Null.NullInteger)
            {
                response.Redirect(NavigationManager.NavigateURL(portalSetting.ErrorPage404, string.Empty, "status=404"));
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
