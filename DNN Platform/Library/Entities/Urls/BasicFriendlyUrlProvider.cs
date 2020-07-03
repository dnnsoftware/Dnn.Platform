// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    internal class BasicFriendlyUrlProvider : FriendlyUrlProviderBase
    {
        private const string RegexMatchExpression = "[^a-zA-Z0-9 ]";
        private static readonly Regex FriendlyPathRx = new Regex("(.[^\\\\?]*)\\\\?(.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex DefaultPageRx = new Regex(Globals.glbDefaultPage, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly string _fileExtension;
        private readonly bool _includePageName;
        private readonly string _regexMatch;

        internal BasicFriendlyUrlProvider(NameValueCollection attributes)
            : base(attributes)
        {
            // Read the attributes for this provider
            this._includePageName = string.IsNullOrEmpty(attributes["includePageName"]) || bool.Parse(attributes["includePageName"]);
            this._regexMatch = !string.IsNullOrEmpty(attributes["regexMatch"]) ? attributes["regexMatch"] : RegexMatchExpression;
            this._fileExtension = !string.IsNullOrEmpty(attributes["fileExtension"]) ? attributes["fileExtension"] : ".aspx";
        }

        public string FileExtension
        {
            get { return this._fileExtension; }
        }

        public bool IncludePageName
        {
            get { return this._includePageName; }
        }

        public string RegexMatch
        {
            get { return this._regexMatch; }
        }

        internal override string FriendlyUrl(TabInfo tab, string path)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return this.FriendlyUrl(tab, path, Globals.glbDefaultPage, _portalSettings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return this.FriendlyUrl(tab, path, pageName, _portalSettings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings)
        {
            return this.FriendlyUrl(tab, path, pageName, ((PortalSettings)settings)?.PortalAlias.HTTPAlias, settings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return this.FriendlyUrl(tab, path, pageName, portalAlias, null);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPage adds the page to the friendly url.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page name.</param>
        /// <returns>The formatted url.</returns>
        private string AddPage(string path, string pageName)
        {
            string friendlyPath = path;
            if (friendlyPath.EndsWith("/"))
            {
                friendlyPath = friendlyPath + pageName;
            }
            else
            {
                friendlyPath = friendlyPath + "/" + pageName;
            }

            return friendlyPath;
        }

        private string CheckPathLength(string friendlyPath, string originalpath)
        {
            if (friendlyPath.Length >= 260)
            {
                return Globals.ResolveUrl(originalpath);
            }

            return friendlyPath;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriendlyAlias gets the Alias root of the friendly url.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="path">The path to format.</param>
        /// <param name="portalAlias">The portal alias of the site.</param>
        /// <param name="isPagePath">Whether is a relative page path.</param>
        /// <returns>The formatted url.</returns>
        private string GetFriendlyAlias(string path, string portalAlias, bool isPagePath)
        {
            string friendlyPath = path;
            string matchString = string.Empty;
            if (string.IsNullOrEmpty(portalAlias) == false)
            {
                string httpAlias = Globals.AddHTTP(portalAlias).ToLowerInvariant();
                if (HttpContext.Current?.Items["UrlRewrite:OriginalUrl"] != null)
                {
                    string originalUrl =
                        HttpContext.Current.Items["UrlRewrite:OriginalUrl"].ToString().ToLowerInvariant();
                    httpAlias = Globals.AddPort(httpAlias, originalUrl);
                    if (originalUrl.StartsWith(httpAlias))
                    {
                        matchString = httpAlias;
                    }

                    if (string.IsNullOrEmpty(matchString))
                    {
                        // Manage the special case where original url contains the alias as
                        // http://www.domain.com/Default.aspx?alias=www.domain.com/child"
                        Match portalMatch = Regex.Match(originalUrl, "^?alias=" + portalAlias, RegexOptions.IgnoreCase);
                        if (!ReferenceEquals(portalMatch, Match.Empty))
                        {
                            matchString = httpAlias;
                        }
                    }

                    if (string.IsNullOrEmpty(matchString))
                    {
                        // Manage the special case of child portals
                        // http://www.domain.com/child/default.aspx
                        string tempurl = HttpContext.Current.Request.Url.Host + Globals.ResolveUrl(friendlyPath);
                        if (!tempurl.Contains(portalAlias))
                        {
                            matchString = httpAlias;
                        }
                    }

                    if (string.IsNullOrEmpty(matchString))
                    {
                        // manage the case where the current hostname is www.domain.com and the portalalias is domain.com
                        // (this occurs when www.domain.com is not listed as portal alias for the portal, but domain.com is)
                        string wwwHttpAlias = Globals.AddHTTP("www." + portalAlias);
                        if (originalUrl.StartsWith(wwwHttpAlias))
                        {
                            matchString = wwwHttpAlias;
                        }
                    }
                }
                else
                {
                    matchString = httpAlias;
                }
            }

            if (!string.IsNullOrEmpty(matchString))
            {
                if (path.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    friendlyPath = friendlyPath.Replace(matchString.EndsWith("/") ? "~/" : "~", matchString);
                }
                else
                {
                    friendlyPath = matchString + friendlyPath;
                }
            }
            else
            {
                friendlyPath = Globals.ResolveUrl(friendlyPath);
            }

            if (friendlyPath.StartsWith("//") && isPagePath)
            {
                friendlyPath = friendlyPath.Substring(1);
            }

            return friendlyPath;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetFriendlyQueryString gets the Querystring part of the friendly url.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="tab">The tab whose url is being formatted.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The Page name.</param>
        /// <returns>The formatted url.</returns>
        private string GetFriendlyQueryString(TabInfo tab, string path, string pageName)
        {
            string friendlyPath = path;
            Match queryStringMatch = FriendlyPathRx.Match(friendlyPath);
            string queryStringSpecialChars = string.Empty;
            if (!ReferenceEquals(queryStringMatch, Match.Empty))
            {
                friendlyPath = queryStringMatch.Groups[1].Value;
                friendlyPath = DefaultPageRx.Replace(friendlyPath, string.Empty);
                string queryString = queryStringMatch.Groups[2].Value.Replace("&amp;", "&");
                if (queryString.StartsWith("?"))
                {
                    queryString = queryString.TrimStart(Convert.ToChar("?"));
                }

                string[] nameValuePairs = queryString.Split(Convert.ToChar("&"));
                for (int i = 0; i <= nameValuePairs.Length - 1; i++)
                {
                    string pathToAppend = string.Empty;
                    string[] pair = nameValuePairs[i].Split(Convert.ToChar("="));

                    // Add name part of name/value pair
                    if (friendlyPath.EndsWith("/"))
                    {
                        pathToAppend = pathToAppend + pair[0];
                    }
                    else
                    {
                        pathToAppend = pathToAppend + "/" + pair[0];
                    }

                    if (pair.Length > 1)
                    {
                        if (!string.IsNullOrEmpty(pair[1]))
                        {
                            if (Regex.IsMatch(pair[1], this._regexMatch) == false)
                            {
                                // Contains Non-AlphaNumeric Characters
                                if (pair[0].ToLowerInvariant() == "tabid")
                                {
                                    if (Globals.NumberMatchRegex.IsMatch(pair[1]))
                                    {
                                        if (tab != null)
                                        {
                                            int tabId = Convert.ToInt32(pair[1]);
                                            if (tab.TabID == tabId)
                                            {
                                                if ((string.IsNullOrEmpty(tab.TabPath) == false) && this.IncludePageName)
                                                {
                                                    pathToAppend = tab.TabPath.Replace("//", "/").TrimStart('/') + "/" + pathToAppend;
                                                }
                                            }
                                        }
                                    }
                                }

                                pathToAppend = pathToAppend + "/" + HttpUtility.UrlPathEncode(pair[1]);
                            }
                            else
                            {
                                // Rewrite into URL, contains only alphanumeric and the % or space
                                if (string.IsNullOrEmpty(queryStringSpecialChars))
                                {
                                    queryStringSpecialChars = pair[0] + "=" + pair[1];
                                }
                                else
                                {
                                    queryStringSpecialChars = queryStringSpecialChars + "&" + pair[0] + "=" + pair[1];
                                }

                                pathToAppend = string.Empty;
                            }
                        }
                        else
                        {
                            pathToAppend = pathToAppend + "/" + HttpUtility.UrlPathEncode(' '.ToString());
                        }
                    }

                    friendlyPath = friendlyPath + pathToAppend;
                }
            }

            if (!string.IsNullOrEmpty(queryStringSpecialChars))
            {
                return this.AddPage(friendlyPath, pageName) + "?" + queryStringSpecialChars;
            }

            return this.AddPage(friendlyPath, pageName);
        }

        private Dictionary<string, string> GetQueryStringDictionary(string path)
        {
            string[] parts = path.Split('?');
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (parts.Length == 2)
            {
                foreach (string part in parts[1].Split('&'))
                {
                    string[] keyvalue = part.Split('=');
                    if (keyvalue.Length == 2)
                    {
                        results[keyvalue[0]] = keyvalue[1];
                    }
                }
            }

            return results;
        }

        private string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias, IPortalSettings portalSettings)
        {
            string friendlyPath = path;
            bool isPagePath = tab != null;

            if (this.UrlFormat == UrlFormatType.HumanFriendly)
            {
                if (tab != null)
                {
                    Dictionary<string, string> queryStringDic = this.GetQueryStringDictionary(path);
                    if (queryStringDic.Count == 0 || (queryStringDic.Count == 1 && queryStringDic.ContainsKey("tabid")))
                    {
                        friendlyPath = this.GetFriendlyAlias("~/" + tab.TabPath.Replace("//", "/").TrimStart('/') + ".aspx", portalAlias, true);
                    }
                    else if (queryStringDic.Count == 2 && queryStringDic.ContainsKey("tabid") && queryStringDic.ContainsKey("language"))
                    {
                        if (!tab.IsNeutralCulture)
                        {
                            friendlyPath = this.GetFriendlyAlias(
                                "~/" + tab.CultureCode + "/" + tab.TabPath.Replace("//", "/").TrimStart('/') + ".aspx",
                                portalAlias,
                                true)
                                                .ToLowerInvariant();
                        }
                        else
                        {
                            friendlyPath = this.GetFriendlyAlias(
                                "~/" + queryStringDic["language"] + "/" + tab.TabPath.Replace("//", "/").TrimStart('/') + ".aspx",
                                portalAlias,
                                true)
                                            .ToLowerInvariant();
                        }
                    }
                    else
                    {
                        if (queryStringDic.ContainsKey("ctl") && !queryStringDic.ContainsKey("language"))
                        {
                            switch (queryStringDic["ctl"].ToLowerInvariant())
                            {
                                case "terms":
                                    friendlyPath = this.GetFriendlyAlias("~/terms.aspx", portalAlias, true);
                                    break;
                                case "privacy":
                                    friendlyPath = this.GetFriendlyAlias("~/privacy.aspx", portalAlias, true);
                                    break;
                                case "login":
                                    friendlyPath = queryStringDic.ContainsKey("returnurl")
                                                    ? this.GetFriendlyAlias("~/login.aspx?ReturnUrl=" + queryStringDic["returnurl"], portalAlias, true)
                                                    : this.GetFriendlyAlias("~/login.aspx", portalAlias, true);
                                    break;
                                case "register":
                                    friendlyPath = queryStringDic.ContainsKey("returnurl")
                                                    ? this.GetFriendlyAlias("~/register.aspx?returnurl=" + queryStringDic["returnurl"], portalAlias, true)
                                                    : this.GetFriendlyAlias("~/register.aspx", portalAlias, true);
                                    break;
                                default:
                                    // Return Search engine friendly version
                                    return this.GetFriendlyQueryString(tab, this.GetFriendlyAlias(path, portalAlias, true), pageName);
                            }
                        }
                        else
                        {
                            // Return Search engine friendly version
                            return this.GetFriendlyQueryString(tab, this.GetFriendlyAlias(path, portalAlias, true), pageName);
                        }
                    }
                }
            }
            else
            {
                // Return Search engine friendly version
                friendlyPath = this.GetFriendlyQueryString(tab, this.GetFriendlyAlias(path, portalAlias, isPagePath), pageName);
            }

            friendlyPath = this.CheckPathLength(Globals.ResolveUrl(friendlyPath), path);

            // Replace http:// by https:// if SSL is enabled and site is marked as secure
            // (i.e. requests to http://... will be redirected to https://...)
            portalSettings = portalSettings ?? PortalController.Instance.GetCurrentPortalSettings();
            if (tab != null && portalSettings != null && portalSettings.SSLEnabled && tab.IsSecure &&
                friendlyPath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                friendlyPath = "https://" + friendlyPath.Substring("http://".Length);

                // If portal's "SSL URL" setting is defined: Use "SSL URL" instaed of current portal alias
                var sslUrl = portalSettings.SSLURL;
                if (!string.IsNullOrEmpty(sslUrl))
                {
                    friendlyPath = friendlyPath.Replace("https://" + portalAlias, "https://" + sslUrl);
                }
            }

            return friendlyPath;
        }
    }
}
