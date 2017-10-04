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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Entities.Urls
{
    internal class RedirectController
    {
        /// <summary>
        /// Cancels a redirect
        /// </summary>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <param name="settings"></param>
        /// <param name="message"></param>
        internal static void CancelRedirect(ref UrlAction result, HttpContext context, FriendlyUrlSettings settings, string message)
        {
            result.Action = ActionType.Continue;
            result.Reason = RedirectReason.Not_Redirected;
            result.FinalUrl = null;
            //clean the path for the rewrite 
            NameValueCollection queryString = null;
            if (context != null)
            {
                queryString = context.Request.QueryString;
            }
            result.RewritePath = RedirectTokens.RemoveAnyRedirectTokens(result.RewritePath, queryString);
            //redo the rewrite to fix up the problem.  The user has ticked 'permanent redirect' but hasn't supplied a forwarding Url
            if (context != null)
            //if no context supplied, means no rewrite was required because querystring didn't contain do301 action
            {
                //RewriterUtils.RewriteUrl(context, result.RewritePath, settings.RebaseClientPath);
                RewriterUtils.RewriteUrl(context, result.RewritePath);
            }
            result.DebugMessages.Add(message);
        }

        /// <summary>
        /// Checks for a redirect based on a module friendly url provider rule
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="result"></param>
        /// <param name="queryStringCol"></param>
        /// <param name="settings"></param>
        /// <param name="parentTraceId"></param>
        /// <returns></returns>
        internal static bool CheckForModuleProviderRedirect(Uri requestUri, 
                                                                ref UrlAction result,
                                                                NameValueCollection queryStringCol,
                                                                FriendlyUrlSettings settings, 
                                                                Guid parentTraceId)
        {
            var messages = new List<string>();
            string location;
            bool redirected = ExtensionUrlProviderController.CheckForRedirect(requestUri, 
                                                                                result, 
                                                                                queryStringCol, 
                                                                                settings,
                                                                                out location, 
                                                                                ref messages, 
                                                                                parentTraceId);
            if (messages != null)
            {
                result.DebugMessages.AddRange(messages);
            }
            if (redirected)
            {
                result.FinalUrl = location;
                result.Action = ActionType.Redirect301;
                result.Reason = RedirectReason.Custom_Redirect;
            }
            return redirected;
        }

        internal static bool CheckForParameterRedirect(Uri requestUri, 
                                                            ref UrlAction result,
                                                            NameValueCollection queryStringCol, 
                                                            FriendlyUrlSettings settings)
        {
            //check for parameter replaced works by inspecting the parameters on a rewritten request, comparing 
            //them agains the list of regex expressions on the friendlyurls.config file, and redirecting to the same page 
            //but with new parameters, if there was a match
            bool redirect = false;
            //get the redirect actions for this portal
            var messages = new List<string>();
            Dictionary<int, List<ParameterRedirectAction>> redirectActions = CacheController.GetParameterRedirects(settings, result.PortalId, ref messages);
            if (redirectActions != null && redirectActions.Count > 0)
            {
                try
                {
                    #region trycatch block

                    string rewrittenUrl = result.RewritePath ?? result.RawUrl;

                    List<ParameterRedirectAction> parmRedirects = null;
                    //find the matching redirects for the tabid
                    int tabId = result.TabId;
                    if (tabId > -1)
                    {
                        if (redirectActions.ContainsKey(tabId))
                        {
                            //find the right set of replaced actions for this tab
                            parmRedirects = redirectActions[tabId];
                        }
                    }
                    //check for 'all tabs' redirections
                    if (redirectActions.ContainsKey(-1)) //-1 means 'all tabs' - rewriting across all tabs
                    {
                        //initialise to empty collection if there are no specific tab redirects
                        if (parmRedirects == null)
                        {
                            parmRedirects = new List<ParameterRedirectAction>();
                        }
                        //add in the all redirects
                        List<ParameterRedirectAction> allRedirects = redirectActions[-1];
                        parmRedirects.AddRange(allRedirects); //add the 'all' range to the tab range
                        tabId = result.TabId;
                    }
                    if (redirectActions.ContainsKey(-2) && result.OriginalPath.ToLowerInvariant().Contains("default.aspx"))
                    {
                        //for the default.aspx page
                        if (parmRedirects == null)
                        {
                            parmRedirects = new List<ParameterRedirectAction>();
                        }
                        List<ParameterRedirectAction> defaultRedirects = redirectActions[-2];
                        parmRedirects.AddRange(defaultRedirects); //add the default.aspx redirects to the list
                        tabId = result.TabId;
                    }
                    //726 : allow for site-root redirects, ie redirects where no page match
                    if (redirectActions.ContainsKey(-3))
                    {
                        //request is for site root
                        if (parmRedirects == null)
                        {
                            parmRedirects = new List<ParameterRedirectAction>();
                        }
                        List<ParameterRedirectAction> siteRootRedirects = redirectActions[-3];
                        parmRedirects.AddRange(siteRootRedirects); //add the site root redirects to the collection
                    }
                    //OK what we have now is a list of redirects for the currently requested tab (either because it was specified by tab id,
                    // or because there is a replaced for 'all tabs'

                    if (parmRedirects != null && parmRedirects.Count > 0 && rewrittenUrl != null)
                    {
                        foreach (ParameterRedirectAction parmRedirect in parmRedirects)
                        {
                            //regex test each replaced to see if there is a match between the parameter string
                            //and the parmRedirect
                            string compareWith = rewrittenUrl;
                            var redirectRegex = RegexUtils.GetCachedRegex(parmRedirect.LookFor,
                                                          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            Match regexMatch = redirectRegex.Match(compareWith);
                            bool success = regexMatch.Success;
                            bool siteRootTried = false;
                            //if no match, but there is a site root redirect to try
                            if (!success && parmRedirect.TabId == -3)
                            {
                                siteRootTried = true;
                                compareWith = result.OriginalPathNoAlias;
                                regexMatch = redirectRegex.Match(compareWith);
                                success = regexMatch.Success;
                            }
                            if (!success)
                            {
                                result.DebugMessages.Add(parmRedirect.Name + " redirect not matched (" + rewrittenUrl +
                                                         ")");
                                if (siteRootTried)
                                {
                                    result.DebugMessages.Add(parmRedirect.Name + " redirect not matched [site root] (" +
                                                             result.OriginalPathNoAlias + ")");
                                }
                            }
                            else
                            {
                                //success! there was a match in the parameters
                                string parms = redirectRegex.Replace(compareWith, parmRedirect.RedirectTo);
                                if (siteRootTried)
                                {
                                    result.DebugMessages.Add(parmRedirect.Name + " redirect matched [site root] with (" +
                                                             result.OriginalPathNoAlias + "), replaced with " + parms);
                                }
                                else
                                {
                                    result.DebugMessages.Add(parmRedirect.Name + " redirect matched with (" +
                                                             compareWith + "), replaced with " + parms);
                                }
                                string finalUrl = "";
                                //now we need to generate the friendly Url

                                //first check to see if the parameter replacement string has a destination tabid specified
                                if (parms.ToLower().Contains("tabid/"))
                                {
                                    //if so, using a feature whereby the dest tabid can be changed within the parameters, which will 
                                    //redirect the page as well as redirecting the parameter values
                                    string[] parmParts = parms.Split('/');
                                    bool tabIdNext = false;
                                    foreach (string parmPart in parmParts)
                                    {
                                        if (tabIdNext)
                                        {
                                            //changes the tabid of page, effects a page redirect along with a parameter redirect
                                            Int32.TryParse(parmPart, out tabId);
                                            parms = parms.Replace("tabid/" + tabId.ToString(), "");
                                            //remove the tabid/xx from the path
                                            break; //that's it, we're finished
                                        }
                                        if (parmPart.ToLower() == "tabid")
                                        {
                                            tabIdNext = true;
                                        }
                                    }
                                }
                                else if (tabId == -1)
                                {
                                    //find the home tabid for this portal
                                    //735 : switch to custom method for getting portal
                                    PortalInfo portal = CacheController.GetPortal(result.PortalId, true);
                                    tabId = portal.HomeTabId;
                                }
                                if (parmRedirect.ChangeToSiteRoot)
                                {
                                    //when change to siteroot requested, new path goes directly off the portal alias
                                    //so set the finalUrl as the poratl alias
                                    finalUrl = result.Scheme + result.HttpAlias + "/";
                                }
                                else
                                {
                                    //if the tabid has been supplied, do a friendly url provider lookup to get the correct format for the tab url
                                    if (tabId > -1)
                                    {
                                        TabInfo tab = TabController.Instance.GetTab(tabId, result.PortalId, false);
                                        if (tab != null)
                                        {
                                            string path = Globals.glbDefaultPage + TabIndexController.CreateRewritePath(tab.TabID, "");
                                            string friendlyUrlNoParms = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab, 
                                                                                                path,
                                                                                                Globals.glbDefaultPage,
                                                                                                result.HttpAlias, 
                                                                                                false,
                                                                                                settings, 
                                                                                                Guid.Empty);
                                            if (friendlyUrlNoParms.EndsWith("/") == false)
                                            {
                                                friendlyUrlNoParms += "/";
                                            }
                                            finalUrl = friendlyUrlNoParms;
                                        }
                                        if (tab == null)
                                        {
                                            result.DebugMessages.Add(parmRedirect.Name +
                                                                     " tabId in redirect rule (tabId:" +
                                                                     tabId.ToString() + ", portalId:" +
                                                                     result.PortalId.ToString() +
                                                                     " ), tab was not found");
                                        }
                                        else
                                        {
                                            result.DebugMessages.Add(parmRedirect.Name +
                                                                     " tabId in redirect rule (tabId:" +
                                                                     tabId.ToString() + ", portalId:" +
                                                                     result.PortalId.ToString() + " ), tab found : " +
                                                                     tab.TabName);
                                        }
                                    }
                                }
                                if (parms.StartsWith("//"))
                                {
                                    parms = parms.Substring(2);
                                }
                                if (parms.StartsWith("/"))
                                {
                                    parms = parms.Substring(1);
                                }

                                if (settings.PageExtensionUsageType != PageExtensionUsageType.Never)
                                {
                                    if (parms.EndsWith("/"))
                                    {
                                        parms = parms.TrimEnd('/');
                                    }
                                    if (parms.Length > 0)
                                    {
                                        //we are adding more parms onto the end, so remove the page extension
                                        //from the parameter list
                                        //946 : exception when settings.PageExtension value is empty
                                        parms += settings.PageExtension;
                                        //816: if page extension is /, then don't do this
                                        if (settings.PageExtension != "/" &&
                                            string.IsNullOrEmpty(settings.PageExtension) == false)
                                        {
                                            finalUrl = finalUrl.Replace(settings.PageExtension, "");
                                        }
                                    }
                                    else
                                    {
                                        //we are removing all the parms altogether, so
                                        //the url needs to end in the page extension only
                                        //816: if page extension is /, then don't do this
                                        if (settings.PageExtension != "/" &&
                                            string.IsNullOrEmpty(settings.PageExtension) == false)
                                        {
                                            finalUrl = finalUrl.Replace(settings.PageExtension + "/",
                                                                        settings.PageExtension);
                                        }
                                    }
                                }
                                //put the replaced parms back on the end
                                finalUrl += parms;

                                //set the final url
                                result.FinalUrl = finalUrl;
                                result.Reason = RedirectReason.Custom_Redirect;
                                switch (parmRedirect.Action)
                                {
                                    case "301":
                                        result.Action = ActionType.Redirect301;
                                        break;
                                    case "302":
                                        result.Action = ActionType.Redirect302;
                                        break;
                                    case "404":
                                        result.Action = ActionType.Output404;
                                        break;
                                }
                                redirect = true;
                                break;
                            }
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
                finally
                {
                    if (messages.Count > 0)
                    {
                        result.DebugMessages.AddRange(messages);
                    }
                }
            }
            return redirect;
        }

        /// <summary>
        /// Gets a redirect Url for when the tab has a specified external Url that is of type 'TabType.Tab'.  This covers both
        /// 'PermanentRedirect' and 'ExternalUrl' scenarios, where the settings are to redirect the value.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="settings"></param>
        /// <param name="cleanPath"></param>
        /// <param name="result"></param>
        /// <param name="permRedirect"></param>
        /// <param name="parentTraceId"></param>
        /// <returns></returns>
        /// <remarks>823 : Moved from CheckForRedirects to allow call earlier in pipeline</remarks>
        internal static string GetTabRedirectUrl(TabInfo tab, 
                                                    FriendlyUrlSettings settings, 
                                                    string cleanPath,
                                                    UrlAction result, 
                                                    out bool permRedirect, 
                                                    Guid parentTraceId)
        {
            string bestFriendlyUrl = null;
            //592 : check for permanent redirect
            permRedirect = tab.PermanentRedirect;
            int redirectTabId;
            if (int.TryParse(tab.Url, out redirectTabId))
            {
                //ok, redirecting to a new tab, specified by the tabid in the Url field
                TabInfo redirectTab = TabController.Instance.GetTab(redirectTabId, tab.PortalID, false);
                if (redirectTab != null)
                {
                    //now get the friendly url for that tab
                    bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(redirectTab, 
                                                                                        cleanPath,
                                                                                        Globals.glbDefaultPage,
                                                                                        result.HttpAlias, 
                                                                                        false, 
                                                                                        settings,
                                                                                        parentTraceId);
                }
            }
            else
            {
                //use the url, as specified in the dnn tab url property
                //776 : when no url, don't redirect 
                if (tab.Url != "")
                {
                    bestFriendlyUrl = tab.Url;
                }
                else
                {
                    //776: no actual Url for the 'perm redirect'.
                    //this is a mistake on the part of the person who created the perm redirect
                    //but don't create a redirect or there will be a terminal loop
                    permRedirect = false;
                }
            }
            return bestFriendlyUrl;
        }
    }
}