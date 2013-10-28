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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls.Config;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class RewriteController
    {
        internal const int SiteRootRewrite = -3;
        internal const int AllTabsRewrite = -1;

        #region Private Methods

        private static string AddQueryStringToRewritePath(string rewritePath, string queryString)
        {
            //now add back querystring if they existed
            if (queryString != "")
            {
                bool rewritePathHasQuery = rewritePath.IndexOf("?", StringComparison.Ordinal) != -1;
                if (queryString.StartsWith("?"))
                {
                    queryString = queryString.Substring(1);
                }
                string[] parms = queryString.Split('&');
                // iterate through the array of parameters
                for (int i = 0; i < parms.Length; i++)
                {
                    bool hasValue = false;
                    // get parameter name
                    string parmName = parms[i];
                    //check if parm name contains a value as well
                    if (parmName.IndexOf("=", StringComparison.Ordinal) != -1)
                    {
                        //snip off the =value part of the parm
                        parmName = parmName.Substring(0, parmName.IndexOf("=", StringComparison.Ordinal));
                        hasValue = true;
                    }
                    //597 : do a compare with the '=' on the end to only
                    //compare the entire parmname, not just the start
                    string comparePath1 = "?" + parmName.ToLower();
                    string comparePath2 = "&" + parmName.ToLower();
                    if (hasValue)
                    {
                        comparePath1 += "=";
                        comparePath2 += "=";
                    }
                    // check if parameter already exists in the rewrite path
                    // we only want to add the querystring back on if the 
                    // query string keys were not already shown in the friendly
                    // url path
                    if (!rewritePath.ToLower().Contains(comparePath1)
                        && !rewritePath.ToLower().Contains(comparePath2))
                    {
                        //622 : remove encoding from querystring paths
                        //699 : reverses 622 because works from Request.QUeryString instead of Request.Url.Query
                        //string queryStringPiece = System.Web.HttpUtility.UrlDecode(parms[i]);
                        string queryStringPiece = parms[i];
                        //no decoding - querystring passes through rewriting process untouched
                        // add parameter to SendTo value
                        if (rewritePathHasQuery)
                        {
                            rewritePath = rewritePath + "&" + queryStringPiece;
                        }
                        else
                        {
                            rewritePath = rewritePath + "?" + queryStringPiece;
                            rewritePathHasQuery = true;
                        }
                    }
                }
            }
            return rewritePath;
        }

        private static string CheckIfPortalAlias(string url, NameValueCollection querystringCol, UrlAction result, FriendlyUrlSettings settings, SharedDictionary<string, string> tabDict)
        {
            string newUrl = url;
            bool reWritten = false;

            string defaultPage = Globals.glbDefaultPage.ToLower();
            string portalAliasUrl = url.ToLower().Replace("/" + defaultPage, "");
            //if there is a straight match on a portal alias, it's the home page for that portal requested 
            var portalAlias = PortalAliasController.GetPortalAliasInfo(portalAliasUrl);
            if (portalAlias != null)
            {
                //special case : sometimes, some servers issue root/default.aspx when root/ was requested, sometimes not.  It depends
                //on other server software installed (apparently)
                //so check the raw Url and the url, and see if they are the same except for the /default.aspx
                string rawUrl = result.RawUrl;
                if (url.ToLower().EndsWith(rawUrl + defaultPage.ToLower()))
                {
                    //special case - change the url to be equal to the raw Url
                    url = url.Substring(0, url.Length - defaultPage.Length);
                }

                if (settings.RedirectDefaultPage
                    && url.ToLower().EndsWith("/" + defaultPage)
                    && result.RedirectAllowed)
                {
                    result.Reason = RedirectReason.Site_Root_Home;
                    result.FinalUrl = Globals.AddHTTP(portalAliasUrl + "/");
                    result.Action = ActionType.Redirect301;
                }
                else
                {
                    //special case -> look in the tabdict for a blank intercept
                    //735 : switch to custom method for getting portal
                    PortalInfo portal = CacheController.GetPortal(portalAlias.PortalID, true);
                    if (portal.HomeTabId == -1)
                    {
                        string tabKey = url;
                        if (tabKey.EndsWith("/"))
                        {
                            tabKey = tabKey.TrimEnd('/');
                        }
                        tabKey += "::";
                        using (tabDict.GetReadLock())
                        {
                            if (tabDict.ContainsKey(tabKey))
                            {
                                newUrl = tabDict[tabKey];
                                reWritten = true;
                            }
                        }
                        //if no home tab, but matched a portal alias, and no trailing /default.aspx
                        //and no 'newUrl' value because not rewritten, then append the /default.aspx 
                        //and ask for a rewrite on that one.
                        //DNNDEV-27291
                        if (reWritten == false)
                        {
                            //Need to determine if this is a child alias
                            newUrl = "/" + Globals.glbDefaultPage;
                            reWritten = true;
                        }
                    }
                    else
                    {
                        //set rewrite to home page of site
                        //760: check for portal alias specific culture before choosing home tabid
                        bool checkForCustomAlias = false;
                        bool customTabAlias = false;
                        //check for culture-specific aliases
                        string culture = null;
                        var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
                        //if there are chosen portal aliases, check to see if the found alias is one of them
                        //if not, then will check for a custom alias per tab
                        if (primaryAliases.ContainsAlias(portal.PortalID, portalAlias.HTTPAlias) == false)
                        {
                            checkForCustomAlias = true;
                        }
                        else
                        {
                            //check for a specific culture for the alias
                            culture = primaryAliases.GetCultureByPortalIdAndAlias(portal.PortalID, portalAlias.HTTPAlias);
                        }
                        if (checkForCustomAlias)
                        {
                            //ok, this isnt' a chosen portal alias, check the list of custom aliases
                            List<string> customAliasesForTabs = TabIndexController.GetCustomPortalAliases(settings);
                            if (customAliasesForTabs != null && customAliasesForTabs.Contains(portalAlias.HTTPAlias.ToLower()))
                            {
                                //ok, the alias is used as a custom tab, so now look in the dictionary to see if it's used a 'root' context
                                string tabKey = url.ToLower();
                                if (tabKey.EndsWith("/"))
                                {
                                    tabKey = tabKey.TrimEnd('/');
                                }
                                if (tabKey.EndsWith("/default.aspx"))
                                {
                                    tabKey = tabKey.Substring(0, tabKey.Length - 13); //13 = "/default.aspx".length
                                }
                                tabKey += "::";
                                using (tabDict.GetReadLock())
                                {
                                    if (tabDict.ContainsKey(tabKey))
                                    {
                                        newUrl = tabDict[tabKey];
                                        reWritten = true;
                                        customTabAlias = true; //this alias is used as the alias for a custom tab
                                    }
                                }
                            }
                        }
                        if (customTabAlias == false)
                        {
                            int tabId;
                            if (!String.IsNullOrEmpty(querystringCol["TabId"]))
                            {
                                tabId = Convert.ToInt32(querystringCol["TabId"]);
                                result.Action = ActionType.CheckFor301;
                            }
                            else
                            {
                                tabId = portal.HomeTabId;
                                //not a custom alias for a specific tab, so it must be the home page for the portal we identified
                                if (culture == null)
                                {
                                    culture = portal.DefaultLanguage; //set culture to default if not found specifically
                                }
                                else
                                {
                                    //if there is a specific culture for this alias, and it's different to the default language, then
                                    //go check for a specific culture home page (5.5 and later)
                                    tabId = TabPathHelper.GetHomePageTabIdForCulture(portal.DefaultLanguage,
                                                                                         portal.PortalID,
                                                                                         culture,
                                                                                         tabId);
                                }
                            }
                            //see if there is a skin for the alias/culture combination
                            string skin = TabPathHelper.GetTabAliasSkinForTabAndAlias(portalAlias.PortalID,
                                                                                      portalAlias.HTTPAlias, culture);
                            if (string.IsNullOrEmpty(skin) == false)
                            {
                                newUrl = Globals.glbDefaultPage + TabIndexController.CreateRewritePath(tabId, "", "skinSrc=" + skin);
                            }
                            else
                            {
                                newUrl = Globals.glbDefaultPage + TabIndexController.CreateRewritePath(tabId, "");
                            }
                            if (culture != portal.DefaultLanguage)
                            {
                                AddLanguageCodeToRewritePath(ref newUrl, culture);
                            }
                            //add on language specified by current portal alias
                            reWritten = true;
                        }
                    }
                }

                if (reWritten)
                {
                    //check for replaced to site root from /default.aspx 
                    // 838  set redirect reason and action from result
                    SetRewriteParameters(ref result, newUrl);
                    ActionType action;
                    RedirectReason reason;
                    string resultingUrl;
                    RedirectTokens.DetermineRedirectReasonAndAction(newUrl, result, true, settings, out resultingUrl, out reason, out action);
                    newUrl = resultingUrl;
                    result.Action = action;
                    result.Reason = reason;
                }
            }
            return newUrl;
        }

        private static bool CheckSpecialCase(string tabKeyVal, SharedDictionary<string, string> tabDict)
        {
            bool found = false;
            int pathStart = tabKeyVal.LastIndexOf("::", StringComparison.Ordinal); //look for portal alias separator
            int lastPath = tabKeyVal.LastIndexOf('/');
            //get any path separator in the tab path portion
            if (pathStart > lastPath)
            {
                lastPath = pathStart;
            }
            if (lastPath >= 0)
            {
                int defaultStart = tabKeyVal.ToLower().IndexOf("default", lastPath, StringComparison.Ordinal);
                //no .aspx on the end anymore
                if (defaultStart > 0 && defaultStart > lastPath)
                //there is a default in the path, and it's not the entire path (ie pagnamedefault and not default)
                {
                    tabKeyVal = tabKeyVal.Substring(0, defaultStart);
                    //get rid of the default.aspx part
                    using (tabDict.GetReadLock())
                    {
                        found = tabDict.ContainsKey(tabKeyVal);
                        //lookup the tabpath in the tab dictionary again
                    }
                }
            }
            return found;
        }

        private static UserInfo GetUser(int portalId, string vanityUrl)
        {
            string cacheKey = string.Format(CacheController.VanityUrlLookupKey, portalId);
            var vanityUrlLookupDictionary = CBO.GetCachedObject<Dictionary<string, UserInfo>>(new CacheItemArgs(cacheKey, 20, CacheItemPriority.High, portalId), 
                                                                        c => new Dictionary<string, UserInfo>());

            if (!vanityUrlLookupDictionary.ContainsKey(vanityUrl))
            {
                vanityUrlLookupDictionary[vanityUrl] = UserController.GetUserByVanityUrl(portalId, vanityUrl);
            }

            return vanityUrlLookupDictionary[vanityUrl];
        }


        private static bool CheckTabPath(string tabKeyVal, UrlAction result, FriendlyUrlSettings settings, SharedDictionary<string, string> tabDict, ref string newUrl)
        {
            bool found;
            string userParam = String.Empty;
            string tabLookUpKey = tabKeyVal;
            using (tabDict.GetReadLock())
            {
                found = tabDict.ContainsKey(tabLookUpKey); //lookup the tabpath in the tab dictionary
            }

            //special case, if no extensions and the last part of the tabKeyVal contains default.aspx, then
            //split off the default.aspx part and try again - compensating for gemini issue http://support.dotnetnuke.com/issue/ViewIssue.aspx?id=8651&PROJID=39
            if (!found && settings.PageExtensionUsageType != PageExtensionUsageType.AlwaysUse)
            {
                found = CheckSpecialCase(tabLookUpKey, tabDict);
            }

            //Check for VanityUrl
            var doNotRedirectRegex = new Regex(settings.DoNotRedirectRegex);
            if (!found && !AdvancedUrlRewriter.ServiceApi.IsMatch(result.RawUrl) && !doNotRedirectRegex.IsMatch(result.RawUrl))
            {
                string[] urlParams = tabLookUpKey.Split(new[] { "::" }, StringSplitOptions.None);
                if (urlParams.Length > 1)
                {
                    //Extract the first Url parameter
                    string tabPath = urlParams[1];

                    var urlSegments = tabPath.Split('/');

                    string prefix = urlSegments[0];

                    if (prefix == settings.VanityUrlPrefix && urlSegments.Length > 1)
                    {
                        string vanityUrl = urlSegments[1];

                        //check if its a vanityUrl
                        var user = GetUser(PortalController.GetEffectivePortalId(result.PortalId), vanityUrl);
                        if (user != null)
                        {
                            userParam = "UserId=" + user.UserID.ToString();

                            //Get the User profile Tab
                            var portal = new PortalController().GetPortal(result.PortalId);
                            var profilePage = new TabController().GetTab(portal.UserTabId, result.PortalId, false);

                            FriendlyUrlOptions options = UrlRewriterUtils.GetOptionsFromSettings(settings);
                            string profilePagePath = TabPathHelper.GetFriendlyUrlTabPath(profilePage, options, Guid.NewGuid());

                            //modify lookup key;
                            tabLookUpKey = tabLookUpKey.Replace("::" + String.Format("{0}/{1}", settings.VanityUrlPrefix, vanityUrl), "::" + profilePagePath.TrimStart('/').ToLowerInvariant());

                            using (tabDict.GetReadLock())
                            {
                                found = tabDict.ContainsKey(tabLookUpKey); //lookup the tabpath in the tab dictionary
                            }
                        }
                    }
                }
            }

            if (found)
            {
                using (tabDict.GetReadLock())
                {
                    //determine what the rewritten URl will be 
                    newUrl = tabDict[tabLookUpKey];
                }
                if (!String.IsNullOrEmpty(userParam))
                {
                    newUrl = newUrl + "&" + userParam;
                }
                //if this is a match on the trigger dictionary rebuild,
                //then temporarily store this value in case it's a page name change
                //677 : only match if is on actual tabKeyVal match, to prevent site root redirects
                //statements were moved into this 'if' statement
                result.dictVal = newUrl;
                result.dictKey = tabKeyVal;
            }
            return found;
        }

        private static string CheckLanguageMatch(ref string url, UrlAction result)
        {
            //ok now scan for the language modifier 
            Match langMatch = Regex.Match(url, "/language/(?<code>.[^/]+)(?:/|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            //searches for a string like language/en-US/ in the url
            string langParms = "";
            if (langMatch.Success)
            {
                //OK there is a language modifier in the path
                //we want to shift this so it is back on the end where it belongs
                langParms = langMatch.Value.TrimEnd('/'); //in the format of /language/en-US only
                //it doesn't matter if you get /home.aspx/language/en-US in the url field because the .aspx gets 
                //removed when matching with the tab dictionary
                url = url.Replace(langParms, "") + langParms;
                result.CultureCode = langMatch.Groups["code"].Value; //get the culture code in the requested url

                var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
                if (primaryAliases.Count > 0)
                {
                    string aliasCulture = primaryAliases.GetCultureByPortalIdAndAlias(result.PortalId, result.HttpAlias);
                    if (aliasCulture != null)
                    {
                        //want to do a 301 check because this potentially has a duplicate of portal alias and culture code in the url, which 
                        //is not the best combination
                        if (result.Action == ActionType.Continue)
                        {
                            result.Action = ActionType.CheckFor301;
                        }
                    }
                }
            }
            return langParms;
        }

        private static string ReplaceDefaultPage(string newUrl, string requestUrl, IEnumerable<string> list)
        {
            string url = newUrl; //fall back case: we don't change anything
            //iterate the list and replace in the url is a match is found
            foreach (string requestPage in list)
            {
                if (requestUrl.ToLower().Contains(requestPage))
                {
                    url = newUrl.Replace(Globals.glbDefaultPage, requestPage);
                    break;
                }
            }
            return url;
        }

        private static string RewriteParametersFromModuleProvider(string newUrl,
                                                                    string tabKeyVal,
                                                                    string[] urlParms,
                                                                    bool isSiteRootMatch,
                                                                    UrlAction result,
                                                                    FriendlyUrlSettings settings,
                                                                    out bool rewriteParms,
                                                                    out bool newAction,
                                                                    ref List<string> messages,
                                                                    Guid parentTraceId)
        {
            string rewrittenUrl;
            rewriteParms = ExtensionUrlProviderController.TransformFriendlyUrlPath(newUrl, 
                                                                                    tabKeyVal, 
                                                                                    urlParms,
                                                                                    isSiteRootMatch, 
                                                                                    ref result, 
                                                                                    settings,
                                                                                    out rewrittenUrl, 
                                                                                    out newAction,
                                                                                    ref messages, 
                                                                                    parentTraceId);
            if (rewriteParms)
            {
                result.CustomParmRewrite = true;
            }
            return rewrittenUrl;
        }

        private static void SetRewriteParameters(ref UrlAction result, string rewritePath)
        {
            //split out found replaced and store tabid, rulePortalId and do301 if found
            result.RewritePath = rewritePath;
            MatchCollection qsItems = Regex.Matches(rewritePath, @"(?:\&|\?)(?:(?<key>.[^\=\&]*)\=(?<val>.[^\=\&]*))");
            foreach (Match itemMatch in qsItems)
            {
                string val = itemMatch.Groups["val"].Value;
                string key = itemMatch.Groups["key"].Value;
                switch (key.ToLower())
                {
                    case "tabid":
                        int tabidtemp;
                        if (Int32.TryParse(val, out tabidtemp))
                        {
                            result.TabId = tabidtemp;
                        }
                        break;
                    case "portalid":
                        int pid;
                        if (Int32.TryParse(val, out pid))
                        {
                            result.PortalId = pid;
                        }
                        break;
                    case "language":
                        result.CultureCode = val;
                        break;
                    case "ctl":
                        //786: force redirect for ctl/terms or ctl/privacy
                        RequestRedirectOnBuiltInUrl(val, rewritePath, result);
                        break;
                }
            }
            //remove the application path
            result.RewritePath = result.RewritePath.Replace(result.ApplicationPath + "/", "");
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// appends a language/culture code value if it is not already present in the rewrite path
        /// </summary>
        /// <param name="rewritePath"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        internal static bool AddLanguageCodeToRewritePath(ref string rewritePath, string cultureCode)
        {
            bool changed = false;
            //758 : check for any language identifier in the Url before adding a new one, not just the same one
            if (!string.IsNullOrEmpty(cultureCode) && !rewritePath.ToLower().Contains("language="))
            {
                changed = true;
                if (rewritePath.Contains("?"))
                {
                    rewritePath += "&language=" + cultureCode;
                }
                else
                {
                    rewritePath += "?language=" + cultureCode;
                }
            }
            return changed;
        }

        /// <summary>
        /// appends a skin value to the rewrite path, as long as there is no existing skin in the path
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="rewritePath">The current rewrite path</param>
        /// <param name="skin">The selected skin</param>
        /// <param name="tabId"></param>
        /// <param name="message"></param>
        /// <remarks>852 : Add skin src to rewrite path for specific aliases</remarks>
        internal static bool AddSkinToRewritePath(int tabId, int portalId, ref string rewritePath, string skin, out string message)
        {
            bool changed = false;
            message = null;
            TabInfo tab = null;
            if (tabId > 0 && portalId > -1)
            {
                var tc = new TabController();
                tab = tc.GetTab(tabId, portalId, false);
            }
            //don't overwrite specific skin at tab level for rewritten Urls
            if (tab == null || string.IsNullOrEmpty(tab.SkinSrc))
            {
                if (!string.IsNullOrEmpty(skin) && skin != "default" && !rewritePath.ToLower().Contains("skinsrc="))
                {
                    message = "Added SkinSrc : " + skin;
                    changed = true;
                    rewritePath += rewritePath.Contains("?") ? "&SkinSrc=" + skin.Replace(".ascx", "") : "?SkinSrc=" + skin.Replace(".ascx", "");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(tab.SkinSrc))
                {
                    message = "Tab " + tab.TabID.ToString() + " has skin specified : " + tab.SkinSrc;
                    if (skin != tab.SkinSrc)
                    {
                        message += " - " + skin + " not applied due to tab specific skin";
                    }
                }
            }
            return changed;
        }

        /// <summary>
        /// Checks for exclusions on Rewriting the path, based on a regex pattern
        /// </summary>
        /// <param name="result"></param>
        /// <param name="requestedPath"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static bool CanRewriteRequest(UrlAction result, string requestedPath, FriendlyUrlSettings settings)
        {
            bool retVal;
            try
            {
                //var uri = new Uri(requestedPath);
                //if (uri.PathAndQuery.ToLowerInvariant().StartsWith("/default.aspx"))
                //{
                //    retVal = false;
                //    result.CanRewrite = StateBoolean.True;
                //    result.RewritePath = uri.PathAndQuery.Substring(1);
                //}
                //else
                //{
                    if (String.IsNullOrEmpty(settings.DoNotRewriteRegex) ||
                        (!Regex.IsMatch(requestedPath, settings.DoNotRewriteRegex,
                            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))
                    {
                        retVal = true;
                        result.CanRewrite = StateBoolean.True;
                    }
                    else
                    {
                        retVal = false;
                        result.CanRewrite = StateBoolean.False;
                    }
                //}
            }
            catch (Exception ex)
            {
                retVal = true; //always rewrite if an error in the regex 

                UrlRewriterUtils.LogExceptionInRequest(ex, "NotSet", result);
                result.Ex = ex;
            }
            return retVal;
        }

        internal static string CleanExtension(string value, string extension, out bool replaced)
        {
            return CleanExtension(value, extension, "", out replaced);
        }

        internal static string CleanExtension(string value, FriendlyUrlSettings settings, string langParms, out bool replaced)
        {
            return CleanExtension(value, settings.PageExtension, langParms, out replaced);
        }

        internal static string CleanExtension(string value, FriendlyUrlSettings settings, out bool replaced)
        {
            return CleanExtension(value, settings.PageExtension, "", out replaced);
        }

        internal static string CleanExtension(string value, string extension, string langParms, out bool replaced)
        {
            string result = value;
            string ext = extension.ToLower();
            replaced = false;
            if (result.ToLower().EndsWith(ext) && ext != "")
            {
                result = result.Substring(0, result.Length - ext.Length);
                replaced = true;
            }
            else
            {
                if (result.ToLower().EndsWith(".aspx"))
                {
                    result = result.Substring(0, result.Length - 5);
                    replaced = true;
                }
                else
                {
                    //object not set errors when language parameters used
                    if (string.IsNullOrEmpty(langParms) == false)
                    {
                        //safely remove .aspx from the language path without doing a full .aspx -> "" replace on the entire path
                        if (string.IsNullOrEmpty(result) == false &&
                            result.ToLower().EndsWith(".aspx" + langParms.ToLower()))
                        {
                            result = result.Substring(0, result.Length - (5 + langParms.Length)) + langParms;
                            replaced = true;
                        }
                    }
                    else if (result.EndsWith("/"))
                    {
                        result = result.Substring(0, result.Length - 1);
                        replaced = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns either the rewritten path (if a rewrite used) or the requested path (if no rewrite used)
        /// </summary>
        /// <param name="result"></param>
        /// <param name="requestUri"></param>
        /// <returns>Url suitable for input into friendly url generation routine</returns>
        internal static string GetRewriteOrRequestedPath(UrlAction result, Uri requestUri)
        {
            var pathOnly = result.RewritePath;
            if (result.DoRewrite == false)
            {
                //if no rewrite, then the path should have been a non-friendly path, and therefore can be passed in to get the friendly Url
                pathOnly = requestUri.Authority + requestUri.PathAndQuery;
                int aliasEnd =
                    pathOnly.IndexOf(result.PortalAlias.HTTPAlias, StringComparison.InvariantCultureIgnoreCase) +
                    result.PortalAlias.HTTPAlias.Length;
                if (aliasEnd > -1)
                {
                    pathOnly = pathOnly.Substring(aliasEnd);
                }
                pathOnly = HttpUtility.UrlDecode(pathOnly, Encoding.UTF8);
            }
            return pathOnly;
        }

        internal static string GetTabFromDictionary(string url, NameValueCollection querystringCol, FriendlyUrlSettings settings, UrlAction result, Guid parentTraceId)
        {
            //retrive the tab dictionary from the cache and get the path depth values 
            int maxAliasPathDepth;
            int maxTabPathDepth;
            int minAliasPathDepth;
            int minTabPathDepth;
            bool triedFixingSubdomain = false;
            int curAliasPathDepth = 0;

            var tabDict = TabIndexController.FetchTabDictionary(result.PortalId,
                                                                        out minTabPathDepth,
                                                                        out maxTabPathDepth,
                                                                        out minAliasPathDepth,
                                                                        out maxAliasPathDepth,
                                                                        settings,
                                                                        false,
                                                                        result.BypassCachedDictionary,
                                                                        parentTraceId);

            //clean up and prepare the url for scanning
            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }

            //ok now scan for the language modifier 
            string langParms = CheckLanguageMatch(ref url, result);

            //clean off the extension before checking the url matches, but 
            //remember if an extension existed
            bool hadExtension;
            string cleanUrl = CleanExtension(url, settings, langParms, out hadExtension);
            string[] splitUrl = cleanUrl.Split('/');

            //initialise logic switches 
            bool reWritten = false;
            bool finished = false;

            string newUrl = CheckIfPortalAlias(url, querystringCol, result, settings, tabDict);
            if (newUrl != url)
            {
                finished = true;
                reWritten = true;
            }

            //start looping through the url segments looking for a match in the tab dictionary 
            while (finished == false)
            {
                //first, try forming up a key based on alias/tabpath 
                int lastIndex = splitUrl.GetUpperBound(0);
                int arraySize = lastIndex + 1;
                int totalDepth = maxAliasPathDepth + 1 + maxTabPathDepth + 1;

                //the maximum depth of segments of a valid url 
                for (int i = lastIndex; i >= 0; i += -1)
                {
                    //only start checking the url when it is in the range of the min->max number of segments 
                    if ((i > minAliasPathDepth & i <= totalDepth))
                    {
                        //join all the tab path sections together 
                        //flag to remember if the incoming path had a .aspx or other pageAndExtension on it 
                        int tabPathStart = curAliasPathDepth + 1;
                        int tabPathLength = i - curAliasPathDepth;
                        if ((tabPathStart + tabPathLength) <= arraySize)
                        {
                            string tabPath = "";
                            if ((tabPathLength > -1))
                            {
                                tabPath = string.Join("/", splitUrl, tabPathStart, tabPathLength);
                            }
                            string aliasPath;
                            if ((curAliasPathDepth <= lastIndex))
                            {
                                aliasPath = string.Join("/", splitUrl, 0, curAliasPathDepth + 1);
                            }
                            else
                            {
                                finished = true;
                                break;
                            }
                            int parmsSize = lastIndex - i;
                            int parmStart = i + 1; //determine if any parameters on this value

                            //make up the index that is looked for in the Tab Dictionary
                            string urlPart = aliasPath + "::" + tabPath;
                            //the :: allows separation of pagename and portal alias

                            string tabKeyVal = urlPart.ToLower(); //force lower case lookup, all keys are lower case

                            //Try with querystring first If last Index
                            bool found = false;
                            if (querystringCol.Count > 0)
                            {
                                found = CheckTabPath(tabKeyVal.Replace(" ", settings.SpaceEncodingValue) + "?" + querystringCol.ToString().Split('&')[0].ToLowerInvariant(), result, settings, tabDict, ref newUrl);
                            }
                            if (!found)
                            {
                                found = CheckTabPath(tabKeyVal.Replace(" ", settings.SpaceEncodingValue), result, settings, tabDict, ref newUrl);
                            }

                            bool isSiteRootMatch = false;
                            if (!found && tabPathLength == 1)
                            {
                                //look for special case where the site root has a * value
                                string siteRootLookup = aliasPath + "::" + "*";
                                using (tabDict.GetReadLock())
                                {
                                    found = tabDict.ContainsKey(siteRootLookup);
                                    if (found)
                                    {
                                        isSiteRootMatch = true;
                                        newUrl = tabDict[siteRootLookup];
                                        parmsSize++; //re-increase the parameter size
                                        parmStart--; //shift the point of the parms starting back one
                                    }
                                }
                            }

                            if (found)
                            {
                                if (settings.ProcessRequestList != null)
                                {
                                    newUrl = ReplaceDefaultPage(newUrl, url, settings.ProcessRequestList);
                                }

                                //look for a plain match on the default.aspx page which indicates no more rewriting needs to be done
                                if (!isSiteRootMatch && (newUrl == Globals.glbDefaultPage || newUrl == Globals.glbDefaultPage + "[UseBase]"))
                                {
                                    //the [UseBase] moniker is a shortcut hack.  It's used to recognise pages which have been excluded 
                                    //from using Friendly Urls.  The request will go on to be processed by the dnn siteurls.config processing.
                                    //this stops the loop and exits the function
                                    newUrl = newUrl.Replace("[UseBase]", ""); //get rid of usebase hack pattern
                                    SetRewriteParameters(ref result, newUrl); //set result 
                                    finished = true;
                                }
                                else
                                {
                                    //708 : move result rewrite set so that downstream functions know the tabid
                                    SetRewriteParameters(ref result, newUrl);
                                    //found the correct rewrite page, now investigate whether there 
                                    //is part of the url path that needs to be converted to tab id's 
                                    //  Multi Language Urls not being rewritten 
                                    if (parmsSize > 0)
                                    {
                                        bool rewriteParms = false;

                                        //determine the url action and reason from embedded rewriting tokens
                                        ActionType action;
                                        RedirectReason reason;
                                        string resultingUrl;
                                        RedirectTokens.DetermineRedirectReasonAndAction(newUrl,
                                                                                        result,
                                                                                        true,
                                                                                        settings,
                                                                                        out resultingUrl,
                                                                                        out reason,
                                                                                        out action);
                                        newUrl = resultingUrl;
                                        result.Action = action;
                                        result.Reason = reason;

                                        //copy the parms into a separate array 
                                        var urlParms = new string[parmsSize];
                                        Array.ConstrainedCopy(splitUrl, parmStart, urlParms, 0, parmsSize);

                                        if (!isSiteRootMatch && result.Reason == RedirectReason.User_Profile_Url)
                                        {
                                            result.Reason = RedirectReason.Not_Redirected;
                                            newUrl = RedirectTokens.RemoveAnyRedirectTokensAndReasons(newUrl);
                                        }

                                        //738 : check for custom module providers
                                        //894 : allow disable of custom url providers functionality
                                        if (!rewriteParms && settings.EnableCustomProviders)
                                        {
                                            bool newAction;
                                            //newAction tracks whether or not a new 'action' (ie 301, 404, etc) has been requested.
                                            //call the module friendly url providers. Note that the rewriteParms value will be changed if there is a rewrite.
                                            List<string> messages = result.DebugMessages;
                                            newUrl = RewriteParametersFromModuleProvider(newUrl,
                                                                                        tabKeyVal,
                                                                                        urlParms,
                                                                                        isSiteRootMatch,
                                                                                        result,
                                                                                        settings,
                                                                                        out rewriteParms,
                                                                                        out newAction,
                                                                                        ref messages,
                                                                                        parentTraceId);
                                            result.DebugMessages = messages;
                                            if (newAction)
                                            {
                                                finished = true;
                                            }
                                        }
                                        //do a rewrite on the parameters from the stored parameter regex rewrites 
                                        if (!rewriteParms)
                                        {
                                            newUrl = RewriteParameters(newUrl,
                                                                        tabKeyVal,
                                                                        urlParms,
                                                                        isSiteRootMatch,
                                                                        result,
                                                                        out rewriteParms,
                                                                        parentTraceId);
                                        }
                                        if (rewriteParms && isSiteRootMatch)
                                        {
                                            //set rewrite parameters to take tabid for site root matches
                                            SetRewriteParameters(ref result, newUrl);
                                        }

                                        //if the parms weren't rewritten by means of a regex, then process them normally
                                        if (!rewriteParms && !isSiteRootMatch)
                                        //can only try other matches if it wasn't a site root match
                                        {
                                            //put those parms on the back of the url as a query string 
                                            string cultureCode;
                                            newUrl = RewriteParameters(newUrl,
                                                                        tabKeyVal,
                                                                        urlParms,
                                                                        result,
                                                                        langParms,
                                                                        settings,
                                                                        out cultureCode);
                                            if (cultureCode != null) //set culture code if not already set
                                            {
                                                result.CultureCode = cultureCode;
                                            }
                                        }
                                        //now check if the request involved a page pageAndExtension, (.aspx) and shouldn't have 
                                        if (!finished)
                                        {
                                            //944 : don't switch to 301 redirect if action already set to 404
                                            if ((settings.PageExtensionUsageType == PageExtensionUsageType.Never
                                                 || settings.PageExtensionUsageType == PageExtensionUsageType.PageOnly) &
                                                hadExtension)
                                            {
                                                //948 : use new 'no downgrade' method
                                                result.SetActionWithNoDowngrade(ActionType.CheckFor301);
                                            }
                                            else
                                                //866 : redirect back from no extension to extension if it didn't have one
                                                if (settings.PageExtensionUsageType != PageExtensionUsageType.Never &&
                                                    hadExtension == false)
                                                {
                                                    //948 : use new 'no downgrade' method
                                                    result.SetActionWithNoDowngrade(ActionType.CheckFor301);
                                                }
                                        }

                                        if (isSiteRootMatch && !finished)
                                        //when it was a site root match, this must be matched with a custom parameter regex
                                        {
                                            //only finished if the parms were rewritten by means of a regex rewrite
                                            reWritten = rewriteParms;
                                            finished = rewriteParms;
                                        }
                                        else
                                        {
                                            //rewriting done
                                            reWritten = true;
                                            finished = true;
                                        }
                                    }
                                    else
                                    {
                                        //determine the url action and redirect reason from embedded tokens in the url rewrite path
                                        string resultUrl;
                                        RedirectReason reason;
                                        ActionType action;
                                        //add back language parameters if they were there
                                        if (string.IsNullOrEmpty(langParms) == false)
                                        {
                                            string[] parms = langParms.Split('/');
                                            if (parms.GetUpperBound(0) >= 1)
                                            {
                                                if (parms[0] == "" && parms.GetUpperBound(0) > 1)
                                                {
                                                    newUrl += "&" + parms[1] + "=" + parms[2];
                                                }
                                                else
                                                {
                                                    newUrl += "&" + parms[0] + "=" + parms[1];
                                                }
                                            }
                                        }
                                        RedirectTokens.DetermineRedirectReasonAndAction(newUrl, result, false, settings,
                                                                                        out resultUrl, out reason,
                                                                                        out action);
                                        newUrl = resultUrl;
                                        result.Reason = reason;
                                        result.Action = action;

                                        if (settings.EnableCustomProviders && ExtensionUrlProviderController.CheckForAlwaysCallProviders(result.PortalId,
                                                                                                       result.TabId,
                                                                                                       settings,
                                                                                                       parentTraceId))
                                        {
                                            bool newAction;
                                            //newAction tracks whether or not a new 'action' (ie 301, 404, etc) has been requested.
                                            //call the module friendly url providers. Note that the rewriteParms value will be changed if there is a rewrite.
                                            string[] urlParms = (new List<string>()).ToArray(); //empty parm array
                                            if (string.IsNullOrEmpty(langParms) == false)
                                            {
                                                urlParms = langParms.Split('/');
                                                //split the lang parms into the url Parms
                                            }
                                            bool rewriteParms;
                                            List<string> messages = result.DebugMessages;
                                            newUrl = RewriteParametersFromModuleProvider(newUrl,
                                                                                        tabKeyVal,
                                                                                        urlParms,
                                                                                        isSiteRootMatch,
                                                                                        result,
                                                                                        settings,
                                                                                        out rewriteParms,
                                                                                        out newAction,
                                                                                        ref messages,
                                                                                        parentTraceId);
                                            result.DebugMessages = messages;
                                        }

                                        //this is a page only, no parameters to deal with 
                                        //944 : don't downgrade to redirect if the current action is a 404 (see 948 for new solution to 944)
                                        if (settings.PageExtensionUsageType == PageExtensionUsageType.Never & hadExtension)
                                        {
                                            //potentially a 301 replaced because shouldn't be using page extensions 
                                            //948 : check to prevent action downgrade, in case already set to redirect
                                            result.SetActionWithNoDowngrade(ActionType.CheckFor301);
                                        }
                                        else
                                        {
                                            //866 : redirect back from no extension to extension if it didn't have one
                                            if (settings.PageExtensionUsageType != PageExtensionUsageType.Never &&
                                                hadExtension == false)
                                            {
                                                result.SetActionWithNoDowngrade(ActionType.CheckFor301);
                                            }
                                        }
                                        //rewriting done
                                        reWritten = true;
                                        finished = true;
                                    }
                                }
                                if (finished)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                //next, try forming up a key based on alias1/alias2/tabpath 
                if (!finished)
                {
                    curAliasPathDepth += 1;
                    //gone too deep 
                    if ((curAliasPathDepth > maxAliasPathDepth) & (reWritten == false))
                    {
                        // no hope of finding it then 
                        if (triedFixingSubdomain == false && false)
                        {
                            //resplit the new url 
                            splitUrl = newUrl.Split(Convert.ToChar("/"));
                            curAliasPathDepth = minAliasPathDepth;
                            if (result.RedirectAllowed)
                            {
                                result.Action = ActionType.Redirect301;
                            }
                            //this should be redirected 
                            triedFixingSubdomain = true;
                        }
                        else
                        {
                            if (!AdvancedUrlRewriter.ServiceApi.IsMatch(url) && result.RedirectAllowed)
                            {
                                //nothing left to try 
                                result.Action = (settings.DeletedTabHandlingType == DeletedTabHandlingType.Do404Error)
                                        ? ActionType.Output404
                                        : ActionType.Redirect301;
                                if (result.Action == ActionType.Redirect301)
                                {
                                    result.Reason = RedirectReason.Deleted_Page;
                                    result.DoRewrite = true;
                                    result.FinalUrl = Globals.AddHTTP(result.PortalAlias.HTTPAlias + "/");
                                    reWritten = true;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            result.FriendlyRewrite = reWritten;
            result.DoRewrite = reWritten;
            return newUrl;
        }

        internal static void GetUrlWithQuerystring(HttpRequest request, Uri requestUri, out string fullUrl, out string querystring)
        {
            //699: incorrect encoding in absoluteUri.ToString()
            string urlWoutQuery = requestUri.AbsoluteUri;
            if (requestUri.Query != "")
            {
                urlWoutQuery = urlWoutQuery.Replace(requestUri.Query, "");
                // replace the querystring on the reuqest absolute Uri
            }
            //926 : do not use querystring.toString() because of encoding errors that restul,
            //the encoding type is changed from utf-8 to latin-1
            querystring = requestUri.Query;
            //get results
            fullUrl = urlWoutQuery;
            if (fullUrl.EndsWith("/_noext.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                fullUrl = fullUrl.Replace("_noext.aspx", "");
                //replace this marker pattern so it looks as though it isn't there *(leave on trailing slash)  
            }
            if (querystring != "")
            {
                //set up the querystring and the fullUrl to include the querystring
                if (querystring.StartsWith("?") == false)
                {
                    querystring = "?" + querystring;
                }
                fullUrl += querystring;
            }
        }

        /// <summary>
        /// Identifies a request for a physical file on the system
        /// </summary>
        /// <param name="physicalPath">The Physical Path propery of the request</param>
        /// <param name="fullUrl"></param>
        /// <param name="queryStringCol"></param>
        /// <param name="result"></param>
        /// <param name="useFriendlyUrls"></param>
        /// <param name="settings"></param>
        /// <param name="isPhysicalResource"></param>
        /// <param name="checkFurtherForRewrite"></param>
        /// <param name="parentTraceId"></param>
        /// <returns>true if a physical path, false if not</returns>
        internal static void IdentifyByPhysicalResource(string physicalPath,
                                                        string fullUrl,
                                                        NameValueCollection queryStringCol,
                                                        ref UrlAction result,
                                                        bool useFriendlyUrls,
                                                        FriendlyUrlSettings settings,
                                                        out bool isPhysicalResource,
                                                        out bool checkFurtherForRewrite,
                                                        Guid parentTraceId)
        {
            isPhysicalResource = false;
            checkFurtherForRewrite = true;
            if (File.Exists(physicalPath) && physicalPath.EndsWith("\\_noext.aspx") == false)
            {
                //resource found
                string appPath = Globals.ApplicationMapPath + "\\default.aspx";
                bool isDefaultAspxPath = false;
                if (String.Compare(physicalPath, appPath, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    string aliasQs;
                    if (AdvancedUrlRewriter.CheckForChildPortalRootUrl(fullUrl, result, out aliasQs))
                    {
                        result.DebugMessages.Add("Child Portal Alias Root Identified");
                    }
                    else
                    {
                        //it's not the default.aspx path or a child alias request, so we haven't identifed the resource
                        isPhysicalResource = true;
                        checkFurtherForRewrite = false;
                        result.DebugMessages.Add("Resource Identified No Rewrite Used");
                    }
                }
                else
                {
                    isDefaultAspxPath = true;
                }

                if (isDefaultAspxPath) //it is the default aspx path
                {
                    //check to see if it is a /default.aspx?key=value url (not just default.aspx, nor a child alias)
                    if ((queryStringCol != null && queryStringCol.Count > 0))
                    {
                        //when there is a query string supplied, we don't need to rewrite
                        if (useFriendlyUrls)
                        {
                            //using friendly URls, so just nab the Tab id from the querystring
                            if (queryStringCol["tabId"] != null)
                            {
                                if (result.RedirectAllowed)
                                {
                                    result.Action = ActionType.CheckFor301;
                                    result.Reason = RedirectReason.Unfriendly_Url_TabId;
                                }
                            }
                        }
                        result.DoRewrite = false;
                        checkFurtherForRewrite = false;
                        //no more checking for rewrites, we have our physical file and our query string
                        //the default.aspx is still a physical resource, but we want to do the rest of the processing
                    }
                    else
                    {
                        //could be either default.aspx with no querystring, or a child portal alias root
                        //if 301 redirects are on, then we want to rewrite and replace this Url to include information like language modifiers
                        //so return false to indicate that the physical resource couldn't be identified
                        isPhysicalResource = !useFriendlyUrls;
                    }
                }
            }

            //mark as physical request
            result.IsPhysicalResource = isPhysicalResource;
        }

        internal static bool IdentifyByRegEx(string absoluteUri,
                                                string queryString,
                                                string applicationPath,
                                                ref UrlAction result,
                                                FriendlyUrlSettings settings,
                                                Guid parentTraceId)
        {
            var doRewrite = false;

            var rewriterConfig = RewriterConfiguration.GetConfig();

            if (rewriterConfig != null)
            {
                var url = absoluteUri; //get local copy because it gets hacked around
                // Remove querystring if exists.. 
                if (queryString != "")
                {
                    url = url.Replace(queryString, "");
                }

                var rules = RewriterConfiguration.GetConfig().Rules;
                if (rules == null)
                {
                    throw new NullReferenceException("DotNetNuke.HttpModules.Config.RewriterRuleCollection is null");
                }
                for (var i = 0; i <= rules.Count - 1; i++)
                {
                    //iterate the Config Rules looking for a match
                    var lookFor = "^" + RewriterUtils.ResolveUrl(applicationPath, rules[i].LookFor) + "$";
                    var re = new Regex(lookFor, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    if (re.IsMatch(url))
                    {
                        var sendTo = rules[i].SendTo;
                        //get a new rewritePath location 

                        var rewritePath = RewriterUtils.ResolveUrl(applicationPath, re.Replace(url, sendTo)); //no rewrite path unless we match by regex the intended tab
                        var sesMatch = re.Match(url);
                        var sesUrlParams = sesMatch.Groups[2].Value;
                        //a match by regex means it's probably not a 'friendly' Url, so assume at this stage that this request will end up a 301
                        if (settings.UrlFormat == "advanced" && settings.RedirectUnfriendly)
                        {
                            result.Action = ActionType.CheckFor301;
                        }
                        //if a match is found here, there is the potential for a 'friendlier' url 
                        if ((sesUrlParams.Trim().Length > 0))
                        {
                            sesUrlParams = sesUrlParams.Replace("\\", "/");
                            var urlParams = sesUrlParams.Split('/');
                            for (var x = 1; x <= urlParams.Length - 1; x++)
                            {
                                if (urlParams[x].Trim().Length > 0 &&
                                    urlParams[x].ToLower() != Globals.glbDefaultPage.ToLower())
                                {
                                    rewritePath = rewritePath + "&" + urlParams[x].Replace(".aspx", "").Trim() + "=";
                                    if ((x < (urlParams.Length - 1)))
                                    {
                                        x += 1;
                                        if ((urlParams[x].Trim() != ""))
                                        {
                                            rewritePath = rewritePath + urlParams[x].Replace(".aspx", "");
                                        }
                                    }
                                }
                            }
                        }
                        //add back the query string if it was there
                        rewritePath = AddQueryStringToRewritePath(rewritePath, queryString);

                        //832 : check for leading ~ - if not there, then redirect
                        if (sendTo.StartsWith("~"))
                        {
                            doRewrite = true;
                            SetRewriteParameters(ref result, rewritePath);
                            RedirectTokens.SetRedirectReasonAndAction(ref result, settings);
                            result.DoRewrite = true;
                        }
                        else
                        {
                            //we'll assume it's a 301 instead of a 302
                            result.Action = ActionType.Redirect301;
                            result.DoRewrite = false;
                            result.Reason = RedirectReason.SiteUrls_Config_Rule;
                            result.FinalUrl = rewritePath;
                        }
                        break; //exit loop, match found
                    }
                }            
            }

            return doRewrite;
        }

        internal static bool IdentifyByTabPathEx(string absoluteUri,
                                                    string queryString,
                                                    UrlAction result,
                                                    NameValueCollection queryStringCol,
                                                    FriendlyUrlSettings settings,
                                                    Guid parentTraceId)
        {
            string scheme = result.Scheme;
            if (absoluteUri.ToLower().StartsWith(scheme))
            {
                absoluteUri = absoluteUri.Substring(scheme.Length);
            }
            // Remove QueryString if it exists in the Url value 
            if ((queryString != ""))
            {
                absoluteUri = absoluteUri.Replace(queryString, "");
            }
            absoluteUri = HttpUtility.UrlDecode(absoluteUri); //decode the incoming request
            string rewritePath = GetTabFromDictionary(absoluteUri, queryStringCol, settings, result, parentTraceId);
            //put the query string back on the end
            rewritePath = AddQueryStringToRewritePath(rewritePath, queryString);
            //810 : if a culture code is not specified in the rewrite path
            if (result.DoRewrite && settings.ForcePortalDefaultLanguage && result.PortalId >= 0 &&
                rewritePath.Contains("language=") == false)
            {
                //add the portal default language to the rewrite path
                PortalInfo portal = CacheController.GetPortal(result.PortalId, false);
                if (portal != null && !string.IsNullOrEmpty(portal.DefaultLanguage))
                {
                    AddLanguageCodeToRewritePath(ref rewritePath, portal.DefaultLanguage);
                    result.CultureCode = portal.DefaultLanguage;
                    result.DebugMessages.Add("Portal Default Language code " + portal.DefaultLanguage + " added");
                }
            }
            //set the rewrite path
            result.RewritePath = rewritePath;
            return result.DoRewrite;
        }

        internal static bool IdentifyByTabQueryString(Uri requestUri, NameValueCollection queryStringCol, bool useFriendlyUrls, UrlAction result)
        {
            const bool doRewrite = false;
            string requestedPath = requestUri.LocalPath;

            if (useFriendlyUrls)
            {
                //using friendly URls, so just nab the Tab id from the querystring
                if (result.RedirectAllowed && requestedPath.EndsWith(Globals.glbDefaultPage, StringComparison.OrdinalIgnoreCase)
                        && (queryStringCol["tabId"] != null))
                {
                    result.Action = ActionType.CheckFor301;
                    result.Reason = RedirectReason.Unfriendly_Url_TabId;
                }
            }
            result.DoRewrite = doRewrite;
            return doRewrite;
        }

        /// <summary>
        /// Replaces the core IsAdminTab call which was decommissioned for DNN 5.0
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabPath">The path of the tab //admin//someothername</param>
        /// <param name="settings"></param>
        /// /// <remarks>Duplicated in UrlMasterController.cs</remarks>
        /// <returns></returns>
        internal static bool IsAdminTab(int portalId, string tabPath, FriendlyUrlSettings settings)
        {
            //fallback position - all portals match 'Admin'
            const string adminPageName = "Admin";
            //we should be checking that the tab path matches //Admin//pagename or //admin
            //in this way we should avoid partial matches (ie //Administrators
            if (tabPath.StartsWith("//" + adminPageName + "//", StringComparison.CurrentCultureIgnoreCase)
                || String.Compare(tabPath, "//" + adminPageName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if the tab is excluded from FriendlyUrl Processing
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="settings"></param>
        /// <param name="rewriting">If true, we are checking for rewriting purposes, if false, we are checking for friendly Url Generating.</param>
        /// <returns></returns>
        internal static bool IsExcludedFromFriendlyUrls(TabInfo tab, FriendlyUrlSettings settings, bool rewriting)
        {
            bool exclude = false;
            string tabPath = (tab.TabPath.Replace("//", "/") + ";").ToLower();
            //553 change for dnn 5.0 isAdminTab no longer returns true in any case, so
            //check custom admin tab path header
            //811: allow for admin tabs to be friendly
            if (settings.FriendlyAdminHostUrls == false && IsAdminTab(tab.PortalID, tab.TabPath, settings))
            {
                exclude = true;
            }

            if (!exclude && settings.UseBaseFriendlyUrls != null)
            {
                exclude = settings.UseBaseFriendlyUrls.ToLower().Contains(tabPath);
            }

            return exclude;
        }

        /// <summary>
        /// Checks for a current parameter belonging to one of the built in 'ctl' values
        /// </summary>
        /// <param name="urlParm"></param>
        /// <param name="rewritePath"></param>
        /// <param name="result"></param>
        /// <remarks>Sets the Action parameter of the Result to 'CheckFor301' if suspected. Actual redirect taken care of by friendly url redirection logic</remarks>
        internal static void RequestRedirectOnBuiltInUrl(string urlParm, string rewritePath, UrlAction result)
        {
            //on the lookout for items to potentially redirect
            if ("terms|privacy|login|register".Contains(urlParm.ToLower()))
            {
                //likely that this should be redirected, because we don't want ctl/terms, ctl/register, etc
                result.Reason = RedirectReason.Built_In_Url;
                result.Action = ActionType.CheckFor301;
                result.DebugMessages.Add("Built-in Url found: " + rewritePath);
            }
        }

        /// <summary>
        /// converts an array of Url path sections into the rewritten string of parameters for the requested Url
        /// </summary>
        /// <param name="newUrl">The current candidate for the rewritten tab path, as found in the tab dictionary</param>
        /// <param name="tabKeyVal">The tabKey value which was used to find the current newUrl value</param>
        /// <param name="urlParms">The Url path (after the tab name) converted to an array</param>
        /// <param name="result">The UrlAction parameter keeping track of the values</param>
        /// <param name="langParms">The raw language/xx-XX values from the requested Url</param>
        /// <param name="settings">The current friendly url settings</param>
        /// <param name="cultureCode">an out parameter identifying if a culture code was determined during the process.</param>
        /// <returns></returns>
        internal static string RewriteParameters(string newUrl,
                                                    string tabKeyVal,
                                                    string[] urlParms,
                                                    UrlAction result,
                                                    string langParms,
                                                    FriendlyUrlSettings settings,
                                                    out string cultureCode)
        {
            cultureCode = null; //culture code is assigned from the langParms value, if it exists
            if (urlParms != null)
            {
                //determine page extension value and usage
                string pageExtension = settings.PageExtension;

                var parmString = new StringBuilder();
                bool valueField = false;
                bool stripLoneParm = false;
                int lastParmToProcessTo;

                string userIdParm = null;
                PortalInfo thisPortal = new PortalController().GetPortal(result.PortalId);

                //check if there is more than one parm, and keep the value of the primary (first) parm
                if (thisPortal.UserTabId == result.TabId || thisPortal.UserTabId == -1)
                {
                    //719 : shift to only remove last parm on pages with 'all' match
                    stripLoneParm = true; //710 : don't put in username into rewritten parameters 
                    userIdParm = "UserId";
                }

                //recheck firstParmLast - just because it is set to be that way in the config doesn't 
                //mean that the url will come in that way. 
                //first strip out any language parameters
                if (langParms != null)
                {
                    string[] langValues = langParms.TrimStart('/').Split('/');
                    if (langValues.GetUpperBound(0) == 1)
                    {
                        int pos1 = -1, pos2 = -1;
                        for (int i = 0; i < urlParms.GetUpperBound(0); i++)
                        {
                            //match this part of the urlParms with the language parms
                            if (urlParms[i] == langValues[0] && urlParms[i + 1] == langValues[1])
                            {
                                pos1 = i;
                                pos2 = i + 1;
                                break;
                            }
                        }
                        if (pos1 > -1 && pos2 > -1)
                        {
                            //this hacky operation removes the language urls from the array
                            var temp = new List<string>(urlParms);
                            temp.RemoveAt(pos2);
                            temp.RemoveAt(pos1);
                            urlParms = temp.ToArray();
                            //656 : don't allow forced lower case of the culture identifier - always convert the case to aa-AA to match the standard
                            string cultureId = langValues[1];
                            Match cultureMatch = Regex.Match(cultureId, "([a-z]{2})-([a-z]{2})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            if (cultureMatch.Success)
                            {
                                cultureId = cultureMatch.Groups[1].Value + "-" +
                                            cultureMatch.Groups[2].ToString().ToUpper();
                            }
                            //set procedure level culture code, which indicates a language was found in the path
                            cultureCode = cultureId;
                        }
                    }
                }

                lastParmToProcessTo = urlParms.GetUpperBound(0);

                //build up the parameters rewrite string by iterating through the key/value pairs in the Url
                //and turn them into &key=value pairs.
                string keyName = null;
                bool skip = false;
                bool isUserParm = false;
                for (int i = 0; i <= lastParmToProcessTo; i++)
                {
                    string thisParm = urlParms[i];
                    //here's the thing - we either take the last one and put it at the start, or just go two-by-two 
                    if (thisParm.ToLower() != Globals.glbDefaultPage.ToLower())
                    {
                        if (thisParm.ToLower() == "tabid")
                        {
                            skip = true;
                            //discovering the tabid in the list of parameters means that 
                            //it was likely a request for an old-style tab url that 
                            //found a retVal due to match in the tab path.  
                            //while this may mean a 301, we definitely don't want to force a 301,
                            //just investigate it and let the 301 redirect logic work it out
                            //we also want to skip the next parameter, because it is the tabid value
                            if (result.Reason != RedirectReason.Custom_Redirect)
                            {
                                //only reason not to let this one through and count on the 
                                //friendly url checking code is if it was a custom redirect set up
                                result.Reason = RedirectReason.Not_Redirected;
                                result.Action = ActionType.CheckFor301;
                                //set the value field back to false, because, even if the parameter handling is
                                //first parm last, this was an old style URl desitned to be redirected.
                                //and we would expect old-style urls to have the correct parameter order
                                //note this assumes tabid is the first parm in the list.
                                valueField = false;
                            }
                        }
                        else if (!skip)
                        {
                            bool extReplaced;
                            string urlParm = CleanExtension(thisParm, pageExtension, out extReplaced);

                            if (extReplaced && pageExtension == "") //replacing a .aspx extension
                            {
                                result.Action = ActionType.CheckFor301;
                            }
                            if (valueField)
                            {
                                //this parameter is going into the value half of a &key=value pair
                                parmString.Append("=");
                                parmString.Append(urlParm);
                                valueField = false;
                                if (isUserParm)
                                {
                                    int userIdVal;
                                    int.TryParse(urlParm, out userIdVal);
                                    isUserParm = false;
                                }
                                //786 : redirect ctl/terms etc
                                if (keyName != null && keyName.ToLower() == "ctl")
                                {
                                    RequestRedirectOnBuiltInUrl(urlParm, parmString.ToString(), result);
                                }
                            }
                            else
                            {
                                //this parameter is going into the key half of a &key=value pair
                                keyName = urlParm;
                                parmString.Append("&");
                                parmString.Append(urlParm);
                                valueField = true;
                                //if we are looking for a userid parameter in this querystring, check for a match
                                if (userIdParm != null)
                                {
                                    if (String.Compare(keyName, userIdParm, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        isUserParm = true;
                                    }
                                }
                            }
                        }
                        else if (skip)
                        {
                            skip = false;
                        }
                    }
                }

                //add back language parameters if they were found
                AddLanguageCodeToRewritePath(ref newUrl, cultureCode);
                //add on the parameter string
                newUrl += parmString.ToString();

                if (stripLoneParm)
                {
                    newUrl = Regex.Replace(newUrl, @"&[^=]+(?:&|$)", "&");
                    if (newUrl.EndsWith("&"))
                    {
                        newUrl = newUrl.Substring(0, newUrl.Length - 1);
                    }
                }
                //chop the last char off if it is an empty parameter 
                if ((newUrl[newUrl.Length - 1] == '&'))
                {
                    newUrl = newUrl.Substring(0, newUrl.Length - 1);
                }
            }
            return newUrl;
        }

        /// <summary>
        /// Scans the collection of Rewrite Parameter rules, and rewrites the parameters if a match is found
        /// </summary>
        /// <param name="newUrl"></param>
        /// <param name="tabKeyVal"></param>
        /// <param name="urlParms"></param>
        /// <param name="isSiteRoot"></param>
        /// <param name="urlAction"></param>
        /// <param name="rewriteParms"></param>
        /// <param name="parentTraceId"></param>
        /// <returns>The new Url with the parameters rewritten onto the end of hte old Url</returns>
        internal static string RewriteParameters(string newUrl,
                                                        string tabKeyVal,
                                                        string[] urlParms,
                                                        bool isSiteRoot,
                                                        UrlAction urlAction,
                                                        out bool rewriteParms,
                                                        Guid parentTraceId)
        {
            string result = newUrl;
            rewriteParms = false;
            //get the actions from the cache
            var messages = new List<string>();
            Dictionary<int, SharedList<ParameterRewriteAction>> rewriteActions = CacheController.GetParameterRewrites(urlAction.PortalId,
                                                                                            ref messages, parentTraceId);
            if (messages == null)
            {
                messages = new List<string>();
            }
            try
            {
                if (rewriteActions != null && rewriteActions.Count > 0)
                {
                    SharedList<ParameterRewriteAction> tabRewrites = null;
                    var tabIdRegex = new Regex(@"(?:\?|\&)tabid\=(?<tabid>[\d]+)",
                                               RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    Match tabMatch = tabIdRegex.Match(newUrl);
                    if (tabMatch.Success)
                    {
                        string rawTabId = tabMatch.Groups["tabid"].Value;
                        int tabId;
                        if (Int32.TryParse(rawTabId, out tabId))
                        {
                            if (rewriteActions.ContainsKey(tabId))
                            {
                                //find the right set of rewrite actions for this tab
                                tabRewrites = rewriteActions[tabId];
                            }
                        }
                    }

                    if (rewriteActions.ContainsKey(AllTabsRewrite)) //-1 means 'all tabs' - rewriting across all tabs
                    {
                        //initialise to empty collection if there are no specific tab rewrites
                        if (tabRewrites == null)
                        {
                            tabRewrites = new SharedList<ParameterRewriteAction>();
                        }
                        //add in the all rewrites
                        SharedList<ParameterRewriteAction> allRewrites = rewriteActions[AllTabsRewrite];
                        foreach (ParameterRewriteAction rewrite in allRewrites)
                        {
                            tabRewrites.Add(rewrite); //add the 'all' range to the tab range
                        }
                    }
                    if (isSiteRoot && rewriteActions.ContainsKey(SiteRootRewrite))
                    {
                        //initialise to empty collection if there are no specific tab rewrites
                        if (tabRewrites == null)
                        {
                            tabRewrites = new SharedList<ParameterRewriteAction>();
                        }
                        SharedList<ParameterRewriteAction> siteRootRewrites = rewriteActions[SiteRootRewrite];
                        foreach (ParameterRewriteAction rewrite in siteRootRewrites)
                        {
                            tabRewrites.Add(rewrite); //add the site root rewrites to the collection
                        }
                    }
                    //get the parms as a string
                    string parms = string.Join("/", urlParms);

                    if (tabRewrites != null && tabRewrites.Count > 0)
                    {
                        //process each one until a match is found
                        foreach (ParameterRewriteAction rewrite in tabRewrites)
                        {
                            string lookFor = rewrite.LookFor;
                            //debugInfo += " lookFor:" + lookFor;
                            var parmRegex = new Regex(lookFor, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            //check the match, if a match found, do the replacement
                            if (parmRegex.IsMatch(parms))
                            {
                                //rewrite the parameter
                                string sendTo = rewrite.RewriteTo;
                                string parmsOriginal = parms;
                                //replace hte parameter with the rewrite string
                                parms = parmRegex.Replace(parms, sendTo);
                                messages.Add(rewrite.Name + " rewrite match (" + parmsOriginal + "), replaced to : " + parms);
                                //makes sure the newUrl has got a trailing ampersand or a ? to start the query string
                                if (newUrl.Contains("?"))
                                {
                                    if (newUrl.EndsWith("&") == false)
                                    {
                                        newUrl += "&";
                                    }
                                }
                                else //need to start the querystring off (592: allow for custom rewrites on site root)
                                {
                                    newUrl += "?";
                                }

                                //makes sure the new parms string hasn't got a starting ampersand
                                if (parms.StartsWith("&"))
                                {
                                    parms = parms.Substring(1);
                                }
                                //parameters are added to the back fo the newUrl
                                newUrl += parms;
                                //it's a rewrite, all right
                                rewriteParms = true;
                                result = newUrl;
                                urlAction.CustomParmRewrite = true;
                                break;
                            }
                            messages.Add(rewrite.Name + " rewrite not matched (" + parms + ")");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
                string error = "Exception: " + ex.Message + "\n" + ex.StackTrace;
                messages.Add(error);
            }
            finally
            {
                //post messages to debug output
                urlAction.DebugMessages.AddRange(messages);
            }

            return result;
        }

        #endregion
    }
}