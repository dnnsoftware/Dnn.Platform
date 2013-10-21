#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class AdvancedFriendlyUrlProvider : FriendlyUrlProviderBase
    {

        #region Internal Properties

        private FriendlyUrlSettings GetSettings(int portalId)
        {
                return new FriendlyUrlSettings(portalId) { UrlFormat = "Advanced" };          
        }

        #endregion

        #region Constructor

        internal AdvancedFriendlyUrlProvider(NameValueCollection attributes)
            : base(attributes)
        {
        }

        #endregion

        #region Overriden methods

        internal override string FriendlyUrl(TabInfo tab, string path)
        {
            return FriendlyUrl(tab, path, Globals.glbDefaultPage, PortalController.GetCurrentPortalSettings());
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            return FriendlyUrl(tab, path, pageName, PortalController.GetCurrentPortalSettings());
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings portalSettings)
        {
            if (portalSettings == null)
            {
                throw new ArgumentNullException("portalSettings");
            }
            return FriendlyUrlInternal(tab, path, pageName, String.Empty, portalSettings);
        }

        internal override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            return FriendlyUrlInternal(tab, path, pageName, portalAlias, null);
        }

        #endregion

        #region Addvanced Friendly Url Methods

        /// <summary>
        /// Return a FriendlyUrl for the supplied Tab, but don't improve it past the standard DNN Friendly Url version 
        /// </summary>
        internal static string BaseFriendlyUrl(TabInfo tab, string path, string pageName, string httpAlias, FriendlyUrlSettings settings)
        {
            bool cultureSpecificAlias;
            
            //Call GetFriendlyAlias to get the Alias part of the url 
            string friendlyPath = GetFriendlyAlias(path, 
                                                    ref httpAlias, 
                                                    tab.PortalID, 
                                                    settings, 
                                                    null, 
                                                    out cultureSpecificAlias);
            
            //Call GetFriendlyQueryString to get the QueryString part of the url 
            friendlyPath = GetFriendlyQueryString(tab, friendlyPath, pageName, settings);
            
            return friendlyPath;
        }

        /// <summary>
        /// Return a full-improved Friendly Url for the supplied tab
        /// </summary>
        /// <param name="tab">The current page</param>
        /// <param name="path">The non-friendly path to the page</param>
        /// <param name="pageName">The name of the page</param>
        /// <param name="httpAlias">The current portal alias to use</param>
        /// <param name="ignoreCustomRedirects">If true, then the Friendly Url will be constructed without using any custom redirects</param>
        /// <param name="settings">The current Friendly Url Settings to use</param>
        /// <param name="parentTraceId"></param>
        /// <returns></returns>
        public static string ImprovedFriendlyUrl(TabInfo tab, 
                                                        string path, 
                                                        string pageName, 
                                                        string httpAlias,
                                                        bool ignoreCustomRedirects, 
                                                        FriendlyUrlSettings settings,
                                                        Guid parentTraceId)
        {
            List<string> messages;
            return ImprovedFriendlyUrlWithMessages(tab, 
                                                    path, 
                                                    pageName, 
                                                    httpAlias, 
                                                    ignoreCustomRedirects, 
                                                    settings,
                                                    out messages, 
                                                    parentTraceId);
        }

        internal static string ImprovedFriendlyUrlWithMessages(TabInfo tab, 
                                                                    string path, 
                                                                    string pageName, 
                                                                    string httpAlias,
                                                                    bool ignoreCustomRedirects, 
                                                                    FriendlyUrlSettings settings,
                                                                    out List<string> messages, 
                                                                    Guid parentTraceId)
        {
            messages = new List<string>();
            bool cultureSpecificAlias;

            //Call GetFriendlyAlias to get the Alias part of the url
            string friendlyPath = GetFriendlyAlias(path, 
                                                   ref httpAlias, 
                                                   tab.PortalID, 
                                                   settings,
                                                   new PortalSettings(tab.PortalID), 
                                                   out cultureSpecificAlias);

            //Call GetFriendlyQueryString to get the QueryString part of the url 
            friendlyPath = GetFriendlyQueryString(tab, friendlyPath, pageName, settings);

            //ImproveFriendlyUrl will attempt to remove tabid/nn and other information from the Url 
            //700 : avoid null alias if a tab.portalid / httpAlias mismatch
            PortalAliasInfo alias = GetAliasForPortal(httpAlias, tab.PortalID, ref messages);
            if (alias != null)
            {
                var portalSettings = new PortalSettings(tab.TabID, alias);
                friendlyPath = ImproveFriendlyUrlWithMessages(tab, 
                                                                friendlyPath, 
                                                                pageName, 
                                                                portalSettings,
                                                                ignoreCustomRedirects, 
                                                                settings, 
                                                                ref messages,
                                                                cultureSpecificAlias, 
                                                                parentTraceId);

                friendlyPath = ForceLowerCaseIfAllowed(tab, friendlyPath, settings);
            }
            OutputFriendlyUrlMessages(tab, path, "base/messages", messages, friendlyPath, settings);
            return friendlyPath;
        }
        #endregion

        #region Private Methods

        private static string AddPage(string path, string pageName)
        {
            string friendlyPath = path;
            if (friendlyPath.EndsWith(pageName + "/"))
            {
                friendlyPath = friendlyPath.TrimEnd('/');
            }
            else if (friendlyPath.EndsWith(pageName) == false)
            {
                if (friendlyPath.EndsWith("/"))
                {
                    friendlyPath = friendlyPath + pageName;
                }
                else
                {
                    friendlyPath = friendlyPath + "/" + pageName;
                }
            }
            return friendlyPath;
        }

        private static void CheckAndUpdatePortalSettingsForNewAlias(PortalSettings portalSettings, bool cultureSpecificAlias, string portalAlias)
        {
            if (cultureSpecificAlias && portalAlias != portalSettings.PortalAlias.HTTPAlias)
            {
                //was a change in alias, need to update the portalSettings with a new portal alias
                var pac = new PortalAliasController();
                PortalAliasInfo pa = pac.GetPortalAlias(portalAlias, portalSettings.PortalId);
                if (pa != null)
                {
                    portalSettings.PortalAlias = pa;
                }
            }
        }

        private static string CheckForDebug(HttpRequest request)
        {
            string debugValue = "";
            const string debugToken = "_fugdebug";
            //798 : change reference to debug parameters
            if (request != null)
            {
                debugValue = (request.Params.Get("HTTP_" + debugToken.ToUpper()));
            }
            if (debugValue == null)
            {
                debugValue = "false";
            }
            return debugValue.ToLower();
        }

        private static string CreateFriendlyUrl(string portalAlias,
                                                    string newTabPath,
                                                    string newPath,
                                                    string pageAndExtension,
                                                    string newPageName,
                                                    string qs,
                                                    string langParms,
                                                    ref List<string> messages,
                                                    bool builtInUrl,
                                                    bool changeToSiteRoot,
                                                    bool dropLangParms,
                                                    bool isHomePage)
        {
            var finalPathBuilder = new StringBuilder();

            if (changeToSiteRoot) //no page path if changing to site root because of parameter replacement rule (593)
            {
                if (newPath.StartsWith("/"))
                {
                    newPath = newPath.Substring(1);
                }

                finalPathBuilder.Append(Globals.AddHTTP(portalAlias + "/"));
                //don't show lang parms for site-root set values
                finalPathBuilder.Append(newPath);
                finalPathBuilder.Append(newPageName);
                finalPathBuilder.Append(pageAndExtension);
                finalPathBuilder.Append(qs);
                messages.Add("Using change to site root rule to remove path path: " + newTabPath);
            }
            else
            {
                finalPathBuilder.Append(dropLangParms == false
                                            ? Globals.AddHTTP(portalAlias + langParms + "/")
                                            : Globals.AddHTTP(portalAlias + "/"));

                //685 : When final value no longer includes a path, and it's for the home page, don't use the path
                // this happens when items are removed from 'newPath' and shifted to 'qs' because of the 'doNotINcludeInPath' regex
                // It's true this could be solved by taking the items from the path earlier in the chain (in the original routine that takes them
                // from the querystring and includes them in the path), but the intention is to leave the primary friendly url generation similar
                // to the original DNN logic, and have all the 'improved' logic in this routine.
                if (newPath == "" //no path (no querystring when rewritten)
                    && (isHomePage && newTabPath == "/") //is the home page, and we're not using 'home' for it
                    && (langParms == "" || dropLangParms)
                    //doesn't have any language parms, or we're intentionally getting rid of them
                    && !builtInUrl) //builtin Url == login, terms, privacy, register 
                {
                    //Url is home page, and there's no friendly path to add, so we don't need the home page path (ie, /home is unneeded, just use the site root)
                    if (newPageName.Length == 0 && pageAndExtension.StartsWith("."))
                    {
                        //when the newPageName isn't specified, and the pageAndExtension is just an extension
                        //just add the querystring on the end
                        finalPathBuilder.Append(qs);
                    }
                    else
                    {
                        finalPathBuilder.Append(newPageName);
                        finalPathBuilder.Append(pageAndExtension);
                        finalPathBuilder.Append(qs);
                    }
                }
                else
                //this is the normal case 
                {
                    //finalPath += newTabPath.TrimStart('/') + newPath + newPageName + pageAndExtension + qs;
                    finalPathBuilder.Append(newTabPath.TrimStart('/'));
                    finalPathBuilder.Append(newPath);
                    finalPathBuilder.Append(newPageName);
                    finalPathBuilder.Append(pageAndExtension);
                    finalPathBuilder.Append(qs);
                }
            }
            return finalPathBuilder.ToString();
        }

        private static string DetermineExtension(bool isHomePage, string pageName, FriendlyUrlSettings settings)
        {
            string extension = "";
            if (!isHomePage) //no pageAndExtension for the home page when no query string specified
            {
                //the ending of the url depends on the current page pageAndExtension settings
                if (settings.PageExtensionUsageType == PageExtensionUsageType.AlwaysUse
                    || settings.PageExtensionUsageType == PageExtensionUsageType.PageOnly)
                {
                    //check whether a 'custom' (other than default.aspx) page was supplied, and insert that as the pageAndExtension 
                    if (String.Compare(pageName, Globals.glbDefaultPage, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        extension = "/" + pageName.Replace(".aspx", settings.PageExtension);
                    }
                    else
                    {
                        extension = settings.PageExtension;
                        //default page pageAndExtension 
                    }
                }
                else
                {
                    if (String.Compare(pageName, Globals.glbDefaultPage, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        //get rid of the .aspx on the page if it was there 
                        extension = "/" + pageName.Replace(".aspx", ""); // +"/"; //610 : don't always end with /
                    }
                    else
                    {
                        //no pageAndExtension 
                        extension = ""; //610 dont always end with "/";
                    }
                }
            }
            return extension;
        }

        private static string DeterminePageNameAndExtension(ref string pageName, FriendlyUrlSettings settings)
        {
            string pageAndExtension;
            if (settings.PageExtensionUsageType == PageExtensionUsageType.Never
                || settings.PageExtensionUsageType == PageExtensionUsageType.PageOnly)
            {
                if (pageName != Globals.glbDefaultPage)
                {
                    if (pageName == "Logoff.aspx")
                    {
                        pageAndExtension = "/" + pageName;
                    }
                    else if (settings.ProcessRequestList != null &&
                             settings.ProcessRequestList.Contains(pageName.ToLower()))
                    {
                        pageAndExtension = "/" + pageName;
                    }
                    else
                    {
                        pageAndExtension = "/" + pageName.Replace(".aspx", ""); //610 + "/";
                        //get rid of the .aspx on the page if it was there 
                    }
                }
                else
                {
                    pageAndExtension = ""; //610 don't end with "/"; 
                    //no pageAndExtension 
                }
            }
            else
            {
                if (pageName != Globals.glbDefaultPage)
                {
                    if (pageName == "Logoff.aspx")
                    {
                        pageAndExtension = "/" + pageName;
                    }
                    else if (settings.ProcessRequestList != null &&
                             settings.ProcessRequestList.Contains(pageName.ToLower()))
                    {
                        pageAndExtension = "/" + pageName;
                    }
                    else
                    {
                        pageAndExtension = "/" + pageName.Replace(".aspx", settings.PageExtension);
                    }
                }
                else
                {
                    pageAndExtension = settings.PageExtension;
                    //default page extension 
                }
            }
            return pageAndExtension;
        }

        private string FriendlyUrlInternal(TabInfo tab, string path, string pageName, string portalAlias, PortalSettings portalSettings)
        {
            Guid parentTraceId = Guid.Empty;
            int portalId = (portalSettings != null) ? portalSettings.PortalId : tab.PortalID;
            bool cultureSpecificAlias;
            var localSettings = GetSettings(portalId);

            //Call GetFriendlyAlias to get the Alias part of the url
            if (String.IsNullOrEmpty(portalAlias) && portalSettings != null)
            {
                portalAlias = portalSettings.PortalAlias.HTTPAlias;
            }
            string friendlyPath = GetFriendlyAlias(path,
                                                    ref portalAlias,
                                                    portalId,
                                                    localSettings,
                                                    portalSettings,
                                                    out cultureSpecificAlias);

            if (portalSettings != null)
            {
                CheckAndUpdatePortalSettingsForNewAlias(portalSettings, cultureSpecificAlias, portalAlias);
            }

            if (tab == null && path == "~/" && String.Compare(pageName, Globals.glbDefaultPage, StringComparison.OrdinalIgnoreCase) == 0)
            {
                //this is a request for the site root for he dnn logo skin object (642)
                //do nothing, the friendly alias is already correct - we don't want to append 'default.aspx' on the end
            }
            else
            {
                //Get friendly path gets the standard dnn-style friendly path 
                friendlyPath = GetFriendlyQueryString(tab, friendlyPath, pageName, localSettings);

                if (portalSettings == null)
                {
                    var pac = new PortalAliasController();
                    PortalAliasInfo alias = pac.GetPortalAlias(portalAlias, tab.PortalID);

                    portalSettings = new PortalSettings(tab.TabID, alias);
                }
                //ImproveFriendlyUrl will attempt to remove tabid/nn and other information from the Url 
                friendlyPath = ImproveFriendlyUrl(tab,
                                                    friendlyPath,
                                                    pageName,
                                                    portalSettings,
                                                    false,
                                                    cultureSpecificAlias,
                                                    localSettings,
                                                    parentTraceId);
            }
            //set it to lower case if so allowed by settings
            friendlyPath = ForceLowerCaseIfAllowed(tab, friendlyPath, localSettings);

            return friendlyPath;
        }

        private static PortalAliasInfo GetAliasForPortal(string httpAlias, int portalId, ref List<string> messages)
        {
            //if no match found, then call database to find (don't rely on cache for this one, because it is an exception event, not an expected event)
            var pac = new PortalAliasController();
            PortalAliasInfo alias = pac.GetPortalAlias(httpAlias, portalId);
            if (alias == null)
            {
                //no match between alias and portal id
                var aliasArray = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();
                if (aliasArray.Count > 0)
                {
                    alias = aliasArray[0]; //nab the first one here
                    messages.Add("Portal Id " + portalId.ToString() + " does not match http alias " + httpAlias +
                                 " - " + alias.HTTPAlias + " was used instead");
                }
                else
                {
                    messages.Add("Portal Id " + portalId.ToString() +
                                 " does not match http alias and no usable alias could be found");
                }
            }
            return alias;
        }

        internal static string GetCultureOfPath(string path)
        {
            string code = "";
            const string codeRegexPattern = @"(?:\&|\?)language=(?<cc>[A-Za-z]{2}-[A-Za-z]{2})";
            MatchCollection matches = Regex.Matches(path, codeRegexPattern);
            if (matches.Count > 0)
            {
                foreach (Match langMatch in matches)
                {
                    if (langMatch.Success)
                    {
                        Group langGroup = langMatch.Groups["cc"];
                        if (langGroup.Success)
                        {
                            code = langGroup.Value;
                            break;
                        }
                    }
                }
            }
            return code;
        }

        private static string GetCultureOfSettings(PortalSettings portalSettings)
        {
            //note! should be replaced with compiled call to portalSettings.CultureCode property when base supported version is increased.
            string cultureCode = "";
            PropertyInfo cultureCodePi = portalSettings.GetType().GetProperty("CultureCode");
            if (cultureCodePi != null)
            {
                cultureCode = (string)cultureCodePi.GetValue(portalSettings, null);
            }
            return cultureCode;
        }

        private static string GetFriendlyAlias(string path,
                                                ref string httpAlias,
                                                int portalId,
                                                FriendlyUrlSettings settings,
                                                PortalSettings portalSettings,
                                                out bool cultureSpecificAlias)
        {
            cultureSpecificAlias = false;
            string friendlyPath = path;
            bool done = false;
            string httpAliasFull = null;
            //this regex identifies if the correct http://portalAlias already is in the path
            var portalMatchRegex = new Regex("^http[s]*://" + httpAlias, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            string cultureCode = GetCultureOfPath(path);
            if (portalSettings != null)
            {
                if (portalMatchRegex.IsMatch(path) == false || !String.IsNullOrEmpty(cultureCode))
                {
                    //get the portal alias mapping for this portal, which tells whether to enforce a primary portal alias or not
                    PortalSettings.PortalAliasMapping aliasMapping = PortalSettings.GetPortalAliasMappingMode(portalId);
                    //check to see if we should be specifying this based on the culture code
                    var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();

                    //799: if there was no culture code in the path and the portal settings culture was supplied, then use that
                    //essentially we are ignoring the portal alias of the portal settings object and driving the alias from the supplied portal settings culture code
                    //this is because the calling module cannot be guaranteed to be supplying the right culture Code / portal alias combination.
                    if (string.IsNullOrEmpty(cultureCode))
                    {
                        cultureCode = GetCultureOfSettings(portalSettings);
                        //lookup the culture code of the portal settings object
                    }
                    var primaryAlias = primaryAliases.GetAliasByPortalIdAndSettings(portalId, null, cultureCode, settings);

                    if (primaryAlias != null)
                    {
                        if ((aliasMapping == PortalSettings.PortalAliasMapping.Redirect || !String.IsNullOrEmpty(primaryAlias.CultureCode))
                            &&  String.Compare(primaryAlias.HTTPAlias, httpAlias, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            //found the primary alias alias, and it's different from the supplied portal alias
                            //and the site is using a redirect portal alias mapping
                            //substitute in the primary Alias for the supplied alias
                            friendlyPath = friendlyPath.Replace(Globals.AddHTTP(httpAlias), String.Empty);
                            httpAlias = primaryAlias.HTTPAlias;
                            if (!string.IsNullOrEmpty(primaryAlias.CultureCode))
                            {
                                cultureSpecificAlias = true;
                            }
                            else if (primaryAlias.CultureCode == "")
                            {
                                cultureSpecificAlias = true;
                                //770: hacking this so that a change is forced to portal settings when a non-chosen alias is requested.
                            }
                        }
                        else
                        {
                            //check to see if we matched for this alias - if so, it's a culture specific alias
                            if (String.Compare(primaryAlias.HTTPAlias, httpAlias, StringComparison.OrdinalIgnoreCase) == 0 &&
                                !string.IsNullOrEmpty(primaryAlias.CultureCode))
                            {
                                cultureSpecificAlias = true;
                            }
                        }

                        //852 : check to see if the skinSrc is explicityl specified, which we don't want to duplicate if the alias also specifies this
                        if (path.ToLower().Contains("skinsrc=") && primaryAliases.ContainsSpecificSkins())
                        {
                            //path has a skin specified and (at least one) alias has a skin specified
                            string[] parms = path.Split('&');
                            foreach (string parmPair in parms)
                            {
                                if (parmPair.ToLower().Contains("skinsrc="))
                                {
                                    //splits the key/value pair into a two-element array
                                    string[] keyValue = parmPair.Split('=');
                                    //check to see if it was a full skinsrc=xyz pair
                                    if (keyValue.GetUpperBound(0) >= 1)
                                    {
                                        friendlyPath = path.Replace("&" + parmPair, "");
                                    }
                                }
                            }
                        }
                    }

                    //the portal alias is not in the path already, so we need to get it in there
                    if (friendlyPath.StartsWith("~/"))
                    {
                        friendlyPath = friendlyPath.Substring(1);
                    }
                    if (friendlyPath.StartsWith("/") == false)
                    {
                        friendlyPath = "/" + friendlyPath;
                    }
                    //should now have a standard /path/path/page.aspx?key=value style of Url
                    httpAliasFull = Globals.AddHTTP(httpAlias);

                    if (HttpContext.Current != null && HttpContext.Current.Items["UrlRewrite:OriginalUrl"] != null)
                    {
                        string originalUrl = HttpContext.Current.Items["UrlRewrite:OriginalUrl"].ToString();
                        //confirming this portal was the original one requested, making all the generated aliases
                        //for the same portal
                        Match portalMatch = Regex.Match(originalUrl, "^" + httpAliasFull,
                                                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        if (portalMatch.Success == false)
                        {
                            //Manage the special case where original url contains the alias as 
                            //http://www.domain.com/Default.aspx?alias=www.domain.com/child" 
                            portalMatch = Regex.Match(originalUrl, "^?alias=" + httpAlias, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            if (portalMatch.Success)
                            {
                                friendlyPath = Globals.ResolveUrl(friendlyPath);
                                done = true;
                            }
                        }
                    }
                }
            }
            else
            {
                done = true; // well, the friendly path passed in had the right portal alias
            }

            if (!done)
            {
                if (httpAliasFull == null)
                {
                    httpAliasFull = Globals.AddHTTP(httpAlias);
                }
                friendlyPath = httpAliasFull + friendlyPath;
            }
            return friendlyPath;
        }

        private static string GetFriendlyQueryString(TabInfo tab, string path, string pageName, FriendlyUrlSettings settings)
        {
            string friendlyPath = path;
            Match queryStringMatch = Regex.Match(friendlyPath, "(.[^\\\\?]*)\\\\?(.*)",
                                                 RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            string queryStringSpecialChars = "";
            const string defaultPageName = Globals.glbDefaultPage;
            if (!ReferenceEquals(queryStringMatch, Match.Empty))
            {
                friendlyPath = queryStringMatch.Groups[1].Value;
                friendlyPath = Regex.Replace(friendlyPath, defaultPageName, "",
                                             RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (String.Compare(pageName, defaultPageName, StringComparison.OrdinalIgnoreCase) != 0)
                //take out the end page name, it will get re-added
                {
                    friendlyPath = Regex.Replace(friendlyPath, pageName, "",
                                                 RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                string queryString = queryStringMatch.Groups[2].Value.Replace("&amp;", "&");
                if ((queryString.StartsWith("?")))
                {
                    queryString = queryString.TrimStart('?');
                }

                string[] nameValuePairs = queryString.Split('&');
                for (int i = 0; i <= nameValuePairs.Length - 1; i++)
                {
                    string pathToAppend = "";
                    string[] pair = nameValuePairs[i].Split('=');

                    var illegalPageNames = new[]{"con", "aux", "nul", "prn"};

                    if (!illegalPageNames.Contains(pair[0].ToLowerInvariant()) && (pair.Length == 1 || !illegalPageNames.Contains(pair[1].ToLowerInvariant())))
                    {
                        //Add name part of name/value pair 
                        if (friendlyPath.EndsWith("/"))
                        {
                            if (pair[0].ToLower() == "tabid") //always lowercase the tabid part of the path
                            {
                                pathToAppend = pathToAppend + pair[0].ToLower();
                            }
                            else
                            {
                                pathToAppend = pathToAppend + pair[0];
                            }
                        }
                        else
                        {
                            pathToAppend = pathToAppend + "/" + pair[0];
                        }

                        if (pair.Length > 1)
                        {
                            if (pair[1].Length > 0)
                            {
                                if (Regex.IsMatch(pair[1], settings.RegexMatch) == false)
                                {
                                    // Contains Non-AlphaNumeric Characters 
                                    if (pair[0].ToLower() == "tabid")
                                    {
                                        int tabId;
                                        if (Int32.TryParse(pair[1], out tabId))
                                        {
                                            if (tab != null && tab.TabID == tabId)
                                            {
                                                if (tab.TabPath != Null.NullString && settings.IncludePageName)
                                                {
                                                    if (pathToAppend.StartsWith("/") == false)
                                                    {
                                                        pathToAppend = "/" + pathToAppend;
                                                    }
                                                    pathToAppend = tab.TabPath.Replace("//", "/").TrimStart('/').TrimEnd('/') + pathToAppend;
                                                }
                                            }
                                        }
                                    }
                                    if (pair[1].Contains(" "))
                                    {
                                        if (tab != null && (tab.IsSuperTab || RewriteController.IsAdminTab(tab.PortalID, tab.TabPath, settings)))
                                        {
                                            //741 : check admin paths to make sure they aren't using + encoding 
                                            pathToAppend = pathToAppend + "/" + pair[1].Replace(" ", "%20");
                                        }
                                        else
                                        {
                                            pathToAppend = pathToAppend + "/" +
                                                           pair[1].Replace(" ", settings.SpaceEncodingValue);
                                            //625 : replace space with specified url encoding value 
                                        }
                                    }
                                    else
                                    {
                                        pathToAppend = pathToAppend + "/" + pair[1];
                                    }
                                }
                                else
                                {
                                    var valueBuilder = new StringBuilder(pair.GetUpperBound(0));
                                    string key = pair[0];
                                    //string value = pair[1];
                                    valueBuilder.Append(pair[1]);
                                    //if the querystring has been decoded and has a '=' in it, the value will get split more times.  Put those back together with a loop.
                                    if (pair.GetUpperBound(0) > 1)
                                    {
                                        for (int j = 2; j <= pair.GetUpperBound(0); j++)
                                        {
                                            valueBuilder.Append("=");
                                            valueBuilder.Append(pair[j]);
                                        }
                                    }
                                    // Rewrite into URL, contains only alphanumeric and the % or space 
                                    if (queryStringSpecialChars.Length == 0)
                                    {
                                        queryStringSpecialChars = key + "=" + valueBuilder;
                                    }
                                    else
                                    {
                                        queryStringSpecialChars = queryStringSpecialChars + "&" + key + "=" + valueBuilder;
                                    }
                                    pathToAppend = "";
                                }
                            }
                            else
                            {
                                pathToAppend = pathToAppend + "/" + settings.SpaceEncodingValue;
                                //625 : replace with specified space encoding value
                            }
                        }
                    }
                    else
                    {
                        if (pair.Length == 2)
                        {
                            if (queryStringSpecialChars.Length == 0)
                            {
                                queryStringSpecialChars = pair[0] + "=" + pair[1];
                            }
                            else
                            {
                                queryStringSpecialChars = queryStringSpecialChars + "&" + pair[0] + "=" + pair[1];
                            }
                        }
                    }

                    friendlyPath = friendlyPath + pathToAppend;
                }
            }

            if ((queryStringSpecialChars.Length > 0))
            {
                return AddPage(friendlyPath, pageName) + "?" + queryStringSpecialChars;
            }
            return AddPage(friendlyPath, pageName);
        }

        private static string ImproveFriendlyUrl(TabInfo tab,
                                                    string friendlyPath,
                                                    string pageName,
                                                    PortalSettings portalSettings,
                                                    bool ignoreCustomRedirects,
                                                    bool cultureSpecificAlias,
                                                    FriendlyUrlSettings settings,
                                                    Guid parentTraceId)
        {
            //older overload does not include informational/debug messages on return
            List<string> messages = null;
            string url = ImproveFriendlyUrlWithMessages(tab,
                                                        friendlyPath,
                                                        pageName,
                                                        portalSettings,
                                                        ignoreCustomRedirects,
                                                        settings,
                                                        ref messages,
                                                        cultureSpecificAlias,
                                                        parentTraceId);

            OutputFriendlyUrlMessages(tab, friendlyPath, "base/private", messages, url, settings);

            return url;
        }

        private static string ImproveFriendlyUrlWithMessages(TabInfo tab,
                                                        string friendlyPath,
                                                        string pageName,
                                                        PortalSettings portalSettings,
                                                        bool ignoreCustomRedirects,
                                                        FriendlyUrlSettings settings,
                                                        ref List<string> messages,
                                                        bool cultureSpecificAlias,
                                                        Guid parentTraceId)
        {
            if (messages == null)
            {
                messages = new List<string>();
            }
            string httpAlias = portalSettings.PortalAlias.HTTPAlias;
            //invalid call just return the path
            if (tab == null)
            {
                return friendlyPath;
            }

            //no improved friendly urls for super tabs, admin tabs
            if ((tab.IsSuperTab || RewriteController.IsAdminTab(tab.PortalID, tab.TabPath, settings)) && settings.FriendlyAdminHostUrls == false) //811 : allow for friendly admin/host urls
            {
                return friendlyPath;
            }

            //655 : no friendly url if matched with regex
            if (!string.IsNullOrEmpty(settings.NoFriendlyUrlRegex))
            {
                if (Regex.IsMatch(friendlyPath, settings.NoFriendlyUrlRegex,
                                  RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    return friendlyPath;
                }
            }

            string result = friendlyPath;
            //821 : new 'CustomOnly' setting which allows keeping base Urls but also using Custom Urls.  Basically keeps search friendly 
            //but allows for customised urls and redirects
            bool customOnly = settings.UrlFormat.ToLower() == "customonly";
            FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
            //determine if an improved friendly Url is wanted at all
            if ((settings.UrlFormat.ToLower() == "advanced" || customOnly) && !RewriteController.IsExcludedFromFriendlyUrls(tab, settings, false))
            {
                string newTabPath;
                string customHttpAlias;
                bool isHomePage = TabPathHelper.IsTabHomePage(tab, portalSettings);
                //do a regex check on the base friendly Path, to see if there is parameters on the end or not
                var tabOnlyRegex = new Regex("[^?]*/tabId/(?<tabid>\\d+)/" + pageName + @"($|\?(?<qs>.+$))",
                                             RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                if (tabOnlyRegex.IsMatch(friendlyPath))
                {
                    MatchCollection tabOnlyMatches = tabOnlyRegex.Matches(friendlyPath);
                    string qs = "";
                    if (tabOnlyMatches.Count > 0)
                    {
                        Match rgxMatch = tabOnlyMatches[0];
                        if (rgxMatch.Groups["qs"] != null && rgxMatch.Groups["qs"].Success)
                        {
                            qs = "?" + rgxMatch.Groups["qs"].Value;
                        }
                    }
                    bool dropLangParms;
                    //736: observe the culture code of the tab
                    string cultureCode = tab.CultureCode;
                    bool isDefaultLanguage = false;
                    if (cultureCode == "")
                    {
                        cultureCode = portalSettings.DefaultLanguage;
                        isDefaultLanguage = true;
                        //751 when using the default culture code, the redirect doesn't need an explicit language
                    }
                    bool isCustomUrl;
                    newTabPath = TabPathHelper.GetTabPath(tab, 
                                                            settings, 
                                                            options, 
                                                            ignoreCustomRedirects, 
                                                            true,
                                                            isHomePage, 
                                                            cultureCode, 
                                                            isDefaultLanguage, 
                                                            false,
                                                            out dropLangParms, 
                                                            out customHttpAlias, 
                                                            out isCustomUrl,
                                                            parentTraceId);
                    //770 : custom http alias found, merge into overall result
                    if (!string.IsNullOrEmpty(customHttpAlias))
                    {
                        httpAlias = customHttpAlias;
                        messages.Add("Uses page-specific alias: " + customHttpAlias);
                    }

                    //it is a straight page.aspx reference, with just the tabid to specify parameters, so get the extension that should be used (no pagename is used by design)
                    string extension = customHttpAlias != null && newTabPath == ""
                                           ? ""
                                           : DetermineExtension(isHomePage, pageName, settings);

                    if (customOnly && isCustomUrl || customOnly == false)
                    {
                        result = Globals.AddHTTP(httpAlias + "/" + newTabPath.TrimStart('/') + extension) + qs;
                    }
                }
                else
                {
                    //When the home page is requested with a querystring value, the path for the home page is included.  This is because path items without the home page 
                    //qualifier are incorrectly checked for as dnn pages, and will result in a 404.  Ie domain.com/key/value will fail looking for a DNN path called 'key/value'.
                    //This gets around the problem because it places the path aas /home/key/value - which correctly identifies the tab as '/Home' and the key/value parameters as
                    // &key=value.

                    //there are parameters on the base friendly url path, so split them off and process separately
                    //this regex splits the incoming friendly path pagename/tabid/56/default.aspx into the non-tabid path, and individual parms for each /parm/ in the friendly path 
                    //550 : add in \. to allow '.' in the parameter path.
                    //667 : allow non-word characters (specifically %) in the path
                    var rgx = new Regex(@"[^?]*(?<tabs>/tabId/(?<tabid>\d+))(?<path>(?<parms>(?:(?:/[^/?]+){1})+))(?:/" + pageName + @")(?:$|\?(?<qs>.+$))", 
                                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); 
                    MatchCollection matches = rgx.Matches(friendlyPath);
                    if (matches.Count > 0)
                    {
                        //it is a friendly url with other parameters in it 
                        //format it up with the parameters in the end, keeping the same page name 
                        //find the first param name in the params of the Url (the first path piece after the tabid/nn/ value) 
                        Match rgxMatch = matches[0];
                        bool hasParms = false;
                        string cultureCode = null;
                        string defaultCode = null;
                        string newPageName = "";
                        string newPath = "";
                        string qs = "";
                        string langParms = "";
                        //check we matched on some parms in the path 
                        if (rgxMatch.Groups["parms"] != null && rgxMatch.Groups["parms"].Success)
                        {
                            if (rgxMatch.Groups["parms"].Captures.Count > 0)
                            {
                                hasParms = true;
                                newPath = rgxMatch.Groups["path"].Value;
                                qs = rgxMatch.Groups["qs"].Value;
                                Match langMatch = Regex.Match(newPath, "/language/(?<code>.[^/]+)(?:/|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                                if (langMatch.Success)
                                {
                                    //a language specifier parameter is in the string
                                    //this is always the last parameter in the list, which
                                    //gives nasty pagename results, so shift the language query to before the page name
                                    // ie home/tabid/66/language/en-us/default.aspx will give home/language/en-us.aspx
                                    // we really want language/en-us/home.aspx
                                    langParms = langMatch.Value;
                                    langParms = langParms.TrimEnd('/');
                                    newPath = newPath.Replace(langParms, "");
                                    cultureCode = langMatch.Groups["code"].Value;
                                    //obtain the culture code for this language 
                                    defaultCode = portalSettings.DefaultLanguage;
                                }
                                else
                                {
                                    //no language settings, use default
                                    cultureCode = portalSettings.DefaultLanguage;
                                    defaultCode = cultureCode;
                                }
                            }
                        }
                        bool dropLangParms;
                        //determine if we allow the home page to be shown as the site root.  This is if :
                        //cultureCode = default culture, newPath is blank after removing languageParms
                        //allow site root for home page if it is the default culture and no other path items, or there is a specific alias for this culture and there are no other path items
                        bool homePageSiteRoot = (cultureCode == defaultCode && newPath == "") || (cultureSpecificAlias && newPath == "");
                        bool hasPath = (newPath != "");

                        //871 : case insensitive comparison for culture
                        bool isDefaultLanguage = (String.Compare(cultureCode, defaultCode, StringComparison.OrdinalIgnoreCase) == 0);
                        bool isCustomUrl;
                        newTabPath = TabPathHelper.GetTabPath(tab,
                                                                settings,
                                                                options,
                                                                ignoreCustomRedirects,
                                                                homePageSiteRoot,
                                                                isHomePage,
                                                                cultureCode,
                                                                isDefaultLanguage,
                                                                hasPath,
                                                                out dropLangParms,
                                                                out customHttpAlias,
                                                                out isCustomUrl,
                                                                parentTraceId);
                        if (hasParms)
                        {
                            bool changeToSiteRoot = false;

                            //remove any parts of the path excluded by regex
                            RemoveExcludedPartsOfPath(settings, ref newPath, ref qs);
                            if (newPath == "" && isHomePage)
                            {
                                //956 : recheck the test for the homePagebeing a site root Url
                                homePageSiteRoot = (cultureCode == defaultCode && newPath == "") || (cultureSpecificAlias && newPath == "");
                                hasPath = (newPath != "");
                                if (homePageSiteRoot)
                                {
                                    //special case - if the newPath is empty after removing the parameters, and it is the home page, and the home page is only to show the site root
                                    //then re-get the home page Url with no params (hasPath=false)
                                    //done like this to re-run all rules relating the url
                                    newTabPath = TabPathHelper.GetTabPath(tab, 
                                                                            settings, 
                                                                            options, 
                                                                            ignoreCustomRedirects,
                                                                            homePageSiteRoot, 
                                                                            isHomePage, 
                                                                            cultureCode,
                                                                            isDefaultLanguage, 
                                                                            hasPath, 
                                                                            out dropLangParms,
                                                                            out customHttpAlias, 
                                                                            out isCustomUrl,
                                                                            parentTraceId);
                                }
                            }

                            //check for parameter regex replacement 
                            string changedPath = String.Empty;
                            bool allowOtherParameters = false;
                            if (FriendlyUrlPathController.CheckUserProfileReplacement(newPath,
                                                                                        tab,
                                                                                        portalSettings,
                                                                                        settings,
                                                                                        options,
                                                                                        out changedPath,
                                                                                        out changeToSiteRoot,
                                                                                        out allowOtherParameters,
                                                                                        ref messages,
                                                                                        parentTraceId))
                            {
                                messages.Add("User Profile Replacement: Old Path=" + newPath + "; Changed Path=" + changedPath);
                                newPath = changedPath;
                            }

                            //transform is ctl/privacy, ctl/login etc
                            bool builtInUrl = TransformStandardPath(ref newPath, ref newTabPath);

                            //770 : custom http alias found, merge into overall result (except if builtin url, which don't get to use custom aliases)
                            //820 : allow for custom http aliases for builtin urls as well (reverses 770).  Otherwise, can't log in to other aliases.
                            if (!string.IsNullOrEmpty(customHttpAlias))
                            {
                                httpAlias = customHttpAlias;
                                messages.Add("Uses page-specific alias: " + customHttpAlias);
                            }

                            if (allowOtherParameters)
                            {
                                //738 : check for transformation by module specific provider
                                //894 : allow module providers to be disabled
                                bool customModuleUrl = false;
                                if (settings.EnableCustomProviders)
                                {
                                    customModuleUrl = ExtensionUrlProviderController.GetUrlFromExtensionUrlProviders(portalSettings.PortalId, 
                                                                tab, 
                                                                settings, 
                                                                newPath, 
                                                                cultureCode, 
                                                                ref pageName,
                                                                out changedPath, 
                                                                out changeToSiteRoot, 
                                                                ref messages, 
                                                                parentTraceId);
                                }
                                //when no custom module Urls, check for any regex replacements by way of the friendlyurlparms.config file
                                if (!customModuleUrl)
                                {
                                    if (FriendlyUrlPathController.CheckParameterRegexReplacement(newPath, 
                                                                                                    tab, 
                                                                                                    settings,
                                                                                                    portalSettings.PortalId,
                                                                                                    out changedPath,
                                                                                                    ref messages,
                                                                                                    out changeToSiteRoot,
                                                                                                    parentTraceId))
                                    {
                                        newPath = changedPath;
                                    }
                                }
                                else
                                {
                                    //update path value with custom Url returned from module provider(s)
                                    newPath = changedPath;
                                }
                            }

                            //start constructing the url
                            //get the page and extension
                            //770 : when using a custom http alias, and there is no Url for that path, there's no extension regardless of settings
                            //because it's treated like a site root (quasi home page if you like)
                            string pageAndExtension = customHttpAlias != null && newTabPath == ""
                                                          ? ""
                                                          : DeterminePageNameAndExtension(ref pageName, settings);
                            //prepend querystring qualifier if necessary                            
                            qs = !string.IsNullOrEmpty(qs) ? "?" + qs : "";

                            //string it all together
                            if (!dropLangParms)
                            {
                                //871 : case insensitive culture comparisons
                                //drop the language parameters when the defaultCode is the cultureCode for this Url, or the portal alias defines the culture code
                                dropLangParms = isDefaultLanguage || cultureSpecificAlias;
                                // (defaultCode.ToLower() == cultureCode.ToLower()) || cultureSpecificAlias; 
                            }
                            string finalPath = CreateFriendlyUrl(httpAlias, 
                                                                    newTabPath, 
                                                                    newPath, 
                                                                    pageAndExtension,
                                                                    newPageName, 
                                                                    qs, 
                                                                    langParms, 
                                                                    ref messages,
                                                                    builtInUrl, 
                                                                    changeToSiteRoot,
                                                                    dropLangParms, 
                                                                    isHomePage);

                            //702: look for _aumdebug=true|false and remove if so - never want it part of the output friendly url path
                            finalPath = Regex.Replace(finalPath, "/_aumdebug/(?:true|false)", "");

                            //'and we're done! 
                            if (customOnly && isCustomUrl || customOnly == false || builtInUrl)
                            {
                                result = Globals.AddHTTP(finalPath);
                            }
                        }
                    }
                    else
                    {
                        var re = new Regex("[^?]*/tabId/(\\d+)/ctl/([A-Z][a-z]+)/" + pageName + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        if ((re.IsMatch(friendlyPath)))
                        {
                            Match sesMatch = re.Match(friendlyPath);
                            if ((sesMatch.Groups.Count > 2))
                            {
                                switch (sesMatch.Groups[2].Value.ToLower())
                                {
                                    case "terms":
                                        result = Globals.AddHTTP(httpAlias + "/" + sesMatch.Groups[2].Value + ".aspx");
                                        break;
                                    case "privacy":
                                        result = Globals.AddHTTP(httpAlias + "/" + sesMatch.Groups[2].Value + ".aspx");
                                        break;
                                    case "login":
                                        result = Globals.AddHTTP(httpAlias + "/" + sesMatch.Groups[2].Value + ".aspx");
                                        break;
                                    case "register":
                                        result = Globals.AddHTTP(httpAlias + "/" + sesMatch.Groups[2].Value + ".aspx");
                                        break;
                                    default:
                                        result = friendlyPath;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static void OutputFriendlyUrlMessages(TabInfo tab, 
                                                        string path, 
                                                        string method, 
                                                        List<string> messages,
                                                        string resultUrl, 
                                                        FriendlyUrlSettings settings)
        {
            if (settings != null && settings.AllowDebugCode && HttpContext.Current != null)
            {
                HttpRequest request = HttpContext.Current.Request;
                string debugCheck = CheckForDebug(request);
                if (debugCheck != "" && debugCheck != "false")
                {
                    int id = DateTime.Now.Millisecond;
                    //append the friendly url headers
                    HttpResponse response = HttpContext.Current.Response;
                    string msgId = id.ToString("000");
                    int tabId = -1;
                    string tabName = "null";
                    if (tab != null)
                    {
                        tabId = tab.TabID;
                        tabName = tab.TabName;
                    }
                    msgId += "-" + tabId.ToString("000");

                    if (messages != null && messages.Count > 0)
                    {
                        response.AppendHeader("X-Friendly-Url-" + msgId + ".00",
                                              "Messages for Tab " + tabId.ToString() + ", " + tabName + ", " + path + " calltype:" + method);

                        int i = 1;
                        foreach (string msg in messages)
                        {
                            response.AppendHeader("X-Friendly-Url-" + msgId + "." + i.ToString("00"), msg);
                            i++;
                        }
                        if (resultUrl != null)
                        {
                            response.AppendHeader("X-Friendly-Url-" + msgId + ".99",
                                                  "Path : " + path + " Generated Url : " + resultUrl);
                        }
                    }
                    else
                    {
                        if (debugCheck == "all")
                        {
                            response.AppendHeader("X-Friendly-Url-" + msgId + ".00",
                                                  "Path : " + path + " Generated Url: " + resultUrl);
                        }
                    }
                }
            }
        }


        private static void RemoveExcludedPartsOfPath(FriendlyUrlSettings settings, ref string newPath, ref string qs)
        {
            if (!string.IsNullOrEmpty(newPath)) //if no path, no point in checking
            {
                //661 : separate out querystring items if regex match is made
                //string path = rgxMatch.Groups["path"].Value; //work with newPath, not path, in case of language parms
                if (!string.IsNullOrEmpty(settings.DoNotIncludeInPathRegex))
                {
                    //inspect each path for an item which should be in the querystring, not in the path
                    var notInPath = new Regex(settings.DoNotIncludeInPathRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    if (notInPath.IsMatch(newPath))
                    {
                        MatchCollection pathMatches = notInPath.Matches(newPath);
                        //remove the part from the path
                        string removedPath = notInPath.Replace(newPath, "");
                        //new replace the removedPath in the main 'newPath' value
                        newPath = newPath.Replace(newPath, removedPath);
                        //this prior line replaces the original path (ie key/value/k2/v2) in the overall page path (ie tabid/56/key/value/k2/v2) with 
                        //the original path less the items identified in the regex (ie, remove k2/v2) : leaving tabid/56/key/value
                        //then, those identified path items are returned to the querystring ?k2=v2
                        var qsValueBuilder = new StringBuilder();
                        foreach (Match pathMatch in pathMatches)
                        {
                            if (pathMatch.Value != "")
                            {
                                //return to querystring
                                string[] parts = pathMatch.Value.Split('/'); //split up the matching path
                                bool even = true;
                                foreach (string part in parts)
                                {
                                    if (part != "") //ignore empty parts, generally caused by the split function
                                    {
                                        if (even) //if even, forms the key part of a querystring key/value pair
                                        {
                                            if (qsValueBuilder.Length == 0) //not yet started
                                            {
                                                qsValueBuilder.Append(part);
                                            }
                                            else
                                            {
                                                qsValueBuilder.Append("&");
                                                qsValueBuilder.Append(part);
                                            }
                                        }
                                        else
                                        {
                                            //if odd, forms the value part
                                            qsValueBuilder.Append("=");
                                            qsValueBuilder.Append(part);
                                        }
                                        even = !even;
                                    }
                                }
                            }
                        }
                        //now stick the querystring bit back onto the querystring part of this procedure
                        if (string.IsNullOrEmpty(qs))
                        {
                            qs = qsValueBuilder.ToString();
                        }
                        else
                        {
                            qsValueBuilder.Insert(0, "&"); //start with & character
                            //910: append rather than replace
                            qs += qsValueBuilder.ToString();
                        }
                    }
                }
            }
        }

        private static bool TransformStandardPath(ref string newPath, ref string newTabPath)
        {
            //615 : output simple urls for login, privacy, register and terms urls
            bool builtInUrl = false;
            if (newPath != "/" && newPath != "" && "/ctl/privacy|/ctl/login|/ctl/register|/ctl/terms".Contains(newPath.ToLower()))
            {
                builtInUrl = true;
                switch (newPath.ToLower())
                {
                    case "/ctl/privacy":
                        newTabPath = "Privacy";
                        newPath = newPath.ToLower().Replace("/ctl/privacy", "");
                        break;
                    case "/ctl/login":
                        newTabPath = "Login";
                        newPath = newPath.ToLower().Replace("/ctl/login", "");
                        break;
                    case "/ctl/register":
                        newTabPath = "Register";
                        newPath = newPath.ToLower().Replace("/ctl/register", "");
                        break;
                    case "/ctl/terms":
                        newTabPath = "Terms";
                        newPath = newPath.ToLower().Replace("/ctl/terms", "");
                        break;
                }
            }
            return builtInUrl;
        }

        #endregion

        #region Internal Methods

        internal static string ForceLowerCaseIfAllowed(TabInfo tab, string url, FriendlyUrlSettings settings)
        {
            //606 : include regex to stop lower case in certain circumstances
            //840 : change to re-introduce lower case restrictions on admin / host tabs
            if (tab != null)
            {
                if (!RewriteController.IsAdminTab(tab.PortalID, tab.TabPath, settings))
                {
                    bool forceLowerCase = (settings.ForceLowerCase);
                    if (forceLowerCase)
                    {
                        if (!string.IsNullOrEmpty(settings.ForceLowerCaseRegex))
                        {
                            forceLowerCase = !Regex.IsMatch(url, settings.ForceLowerCaseRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        }
                    }
                    if (forceLowerCase)
                    {
                        //don't force lower case for Urls excluded from being 'friendly'
                        forceLowerCase = (!RewriteController.IsExcludedFromFriendlyUrls(tab, settings, false));
                    }

                    if (forceLowerCase)
                    {
                        url = url.ToLower();
                    }
                }
            }
            return url;
        }

        #endregion
    }
}