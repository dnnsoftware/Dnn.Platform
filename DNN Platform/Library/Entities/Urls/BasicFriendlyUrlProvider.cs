#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Entities.Urls
{
    internal class BasicFriendlyUrlProvider : FriendlyUrlProviderBase
    {
        private const string RegexMatchExpression = "[^a-zA-Z0-9 ]";
        private readonly string _fileExtension;
        private readonly bool _includePageName;
        private readonly string _regexMatch;

        private static readonly Regex FriendlyPathRx = new Regex("(.[^\\\\?]*)\\\\?(.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex DefaultPageRx = new Regex(Globals.glbDefaultPage, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        internal BasicFriendlyUrlProvider(NameValueCollection attributes)
            : base(attributes)
        {
            //Read the attributes for this provider
            _includePageName = String.IsNullOrEmpty(attributes["includePageName"]) || bool.Parse(attributes["includePageName"]);
            _regexMatch = !String.IsNullOrEmpty(attributes["regexMatch"]) ? attributes["regexMatch"] : RegexMatchExpression;
            _fileExtension = !String.IsNullOrEmpty(attributes["fileExtension"]) ? attributes["fileExtension"] : ".aspx";
        }

        #region Public Properties

        public string FileExtension
        {
            get { return _fileExtension; }
        }

        public bool IncludePageName
        {
            get { return _includePageName; }
        }

        public string RegexMatch
        {
            get { return _regexMatch; }
        }

        #endregion

        #region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddPage adds the page to the friendly url
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The page name.</param>
        /// <returns>The formatted url</returns>
        private string AddPage(string path, string pageName)
        {
            string friendlyPath = path;
            if ((friendlyPath.EndsWith("/")))
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
        /// GetFriendlyAlias gets the Alias root of the friendly url
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="path">The path to format.</param>
        /// <param name="portalAlias">The portal alias of the site.</param>
        /// <param name="isPagePath">Whether is a relative page path.</param>
        /// <returns>The formatted url</returns>
        private string GetFriendlyAlias(string path, string portalAlias, bool isPagePath)
        {
            string friendlyPath = path;
            string matchString = "";
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
                    if ((String.IsNullOrEmpty(matchString)))
                    {
                        //Manage the special case where original url contains the alias as
                        //http://www.domain.com/Default.aspx?alias=www.domain.com/child"
                        Match portalMatch = Regex.Match(originalUrl, "^?alias=" + portalAlias, RegexOptions.IgnoreCase);
                        if (!ReferenceEquals(portalMatch, Match.Empty))
                        {
                            matchString = httpAlias;
                        }
                    }

                    if ((String.IsNullOrEmpty(matchString)))
                    {
                        //Manage the special case of child portals 
                        //http://www.domain.com/child/default.aspx
                        string tempurl = HttpContext.Current.Request.Url.Host + Globals.ResolveUrl(friendlyPath);
                        if (!tempurl.Contains(portalAlias))
                        {
                            matchString = httpAlias;
                        }
                    }

                    if ((String.IsNullOrEmpty(matchString)))
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
            if ((!String.IsNullOrEmpty(matchString)))
            {
                if ((path.IndexOf("~", StringComparison.Ordinal) != -1))
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
        /// GetFriendlyQueryString gets the Querystring part of the friendly url
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="tab">The tab whose url is being formatted.</param>
        /// <param name="path">The path to format.</param>
        /// <param name="pageName">The Page name.</param>
        /// <returns>The formatted url</returns>
        private string GetFriendlyQueryString(TabInfo tab, string path, string pageName)
        {
            string friendlyPath = path;
            Match queryStringMatch = FriendlyPathRx.Match(friendlyPath);
            string queryStringSpecialChars = "";
            if (!ReferenceEquals(queryStringMatch, Match.Empty))
            {
                friendlyPath = queryStringMatch.Groups[1].Value;
                friendlyPath = DefaultPageRx.Replace(friendlyPath, "");
                string queryString = queryStringMatch.Groups[2].Value.Replace("&amp;", "&");
                if ((queryString.StartsWith("?")))
                {
                    queryString = queryString.TrimStart(Convert.ToChar("?"));
                }
                string[] nameValuePairs = queryString.Split(Convert.ToChar("&"));
                for (int i = 0; i <= nameValuePairs.Length - 1; i++)
                {
                    string pathToAppend = "";
                    string[] pair = nameValuePairs[i].Split(Convert.ToChar("="));

                    //Add name part of name/value pair
                    if ((friendlyPath.EndsWith("/")))
                    {
                        pathToAppend = pathToAppend + pair[0];
                    }
                    else
                    {
                        pathToAppend = pathToAppend + "/" + pair[0];
                    }
                    if ((pair.Length > 1))
                    {
                        if ((!String.IsNullOrEmpty(pair[1])))
                        {
                            if ((Regex.IsMatch(pair[1], _regexMatch) == false))
                            {
                                //Contains Non-AlphaNumeric Characters
                                if ((pair[0].ToLower() == "tabid"))
                                {
                                    if (Globals.NumberMatchRegex.IsMatch(pair[1]))
                                    {
                                        if (tab != null)
                                        {
                                            int tabId = Convert.ToInt32(pair[1]);
                                            if ((tab.TabID == tabId))
                                            {
                                                if ((string.IsNullOrEmpty(tab.TabPath) == false) && IncludePageName)
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
                                //Rewrite into URL, contains only alphanumeric and the % or space
                                if (String.IsNullOrEmpty(queryStringSpecialChars))
                                {
                                    queryStringSpecialChars = pair[0] + "=" + pair[1];
                                }
                                else
                                {
                                    queryStringSpecialChars = queryStringSpecialChars + "&" + pair[0] + "=" + pair[1];
                                }
                                pathToAppend = "";
                            }
                        }
                        else
                        {
                            pathToAppend = pathToAppend + "/" + HttpUtility.UrlPathEncode((' ').ToString());
                        }
                    }
                    friendlyPath = friendlyPath + pathToAppend;
                }
            }
            if ((!String.IsNullOrEmpty(queryStringSpecialChars)))
            {
                return AddPage(friendlyPath, pageName) + "?" + queryStringSpecialChars;
            }
            return AddPage(friendlyPath, pageName);
        }

        private Dictionary<string, string> GetQueryStringDictionary(string path)
        {
            string[] parts = path.Split('?');
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if ((parts.Length == 2))
            {
                foreach (string part in parts[1].Split('&'))
                {
                    string[] keyvalue = part.Split('=');
                    if ((keyvalue.Length == 2))
                    {
                        results[keyvalue[0]] = keyvalue[1];
                    }
                }
            }

            return results;
        }

        #endregion

        #region Internal Methods

        internal override string FriendlyUrl(TabInfo tab, string path)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return FriendlyUrl(tab, path, Globals.glbDefaultPage, _portalSettings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            PortalSettings _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            return FriendlyUrl(tab, path, pageName, _portalSettings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return FriendlyUrl(tab, path, pageName, settings.PortalAlias.HTTPAlias, settings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return FriendlyUrl(tab, path, pageName, portalAlias, null);
        }

        private string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias, PortalSettings portalSettings)
        {
            string friendlyPath = path;
            bool isPagePath = (tab != null);

            if ((UrlFormat == UrlFormatType.HumanFriendly))
            {
                if ((tab != null))
                {
                    Dictionary<string, string> queryStringDic = GetQueryStringDictionary(path);
                    if ((queryStringDic.Count == 0 || (queryStringDic.Count == 1 && queryStringDic.ContainsKey("tabid"))))
                    {
                        friendlyPath = GetFriendlyAlias("~/" + tab.TabPath.Replace("//", "/").TrimStart('/') + ".aspx", portalAlias, true);
                    }
                    else if ((queryStringDic.Count == 2 && queryStringDic.ContainsKey("tabid") && queryStringDic.ContainsKey("language")))
                    {
                        if (!tab.IsNeutralCulture)
                        {
                            friendlyPath = GetFriendlyAlias("~/" + tab.CultureCode + "/" + tab.TabPath.Replace("//", "/").TrimStart('/') + ".aspx", 
                                                portalAlias, 
                                                true)
                                                .ToLower();
                        }
                        else
                        {
                            friendlyPath = GetFriendlyAlias("~/" + queryStringDic["language"] + "/" + tab.TabPath.Replace("//", "/").TrimStart('/') + ".aspx", 
                                                portalAlias, 
                                                true)
                                            .ToLower();
                        }
                    }
                    else
                    {
                        if (queryStringDic.ContainsKey("ctl") && !queryStringDic.ContainsKey("language"))
                        {
                            switch (queryStringDic["ctl"].ToLowerInvariant())
                            {
                                case "terms":
                                    friendlyPath = GetFriendlyAlias("~/terms.aspx", portalAlias, true);
                                    break;
                                case "privacy":
                                    friendlyPath = GetFriendlyAlias("~/privacy.aspx", portalAlias, true);
                                    break;
                                case "login":
                                    friendlyPath = (queryStringDic.ContainsKey("returnurl")) 
                                                    ? GetFriendlyAlias("~/login.aspx?ReturnUrl=" + queryStringDic["returnurl"], portalAlias, true) 
                                                    : GetFriendlyAlias("~/login.aspx", portalAlias, true);
                                    break;
                                case "register":
                                    friendlyPath = (queryStringDic.ContainsKey("returnurl")) 
                                                    ? GetFriendlyAlias("~/register.aspx?returnurl=" + queryStringDic["returnurl"], portalAlias, true) 
                                                    : GetFriendlyAlias("~/register.aspx", portalAlias, true);
                                    break;
                                default:
                                    //Return Search engine friendly version
                                    return GetFriendlyQueryString(tab, GetFriendlyAlias(path, portalAlias, true), pageName);
                            }
                        }
                        else
                        {
                            //Return Search engine friendly version
                            return GetFriendlyQueryString(tab, GetFriendlyAlias(path, portalAlias, true), pageName);
                        }
                    }
                }
            }
            else
            {
                //Return Search engine friendly version
                friendlyPath = GetFriendlyQueryString(tab, GetFriendlyAlias(path, portalAlias, isPagePath), pageName);
            }

            friendlyPath = CheckPathLength(Globals.ResolveUrl(friendlyPath), path);

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

        #endregion
    }
}