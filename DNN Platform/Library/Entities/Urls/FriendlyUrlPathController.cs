// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    internal class FriendlyUrlPathController
    {
        /// <summary>
        /// This method checks the list of rules for parameter replacement and modifies the parameter path accordingly.
        /// </summary>
        /// <param name="parameterPath"></param>
        /// <param name="tab"></param>
        /// <param name="settings"></param>
        /// <param name="portalId"></param>
        /// <param name="replacedPath"></param>
        /// <param name="messages"></param>
        /// <param name="changeToSiteRoot"></param>
        /// <param name="parentTraceId"></param>
        /// <returns></returns>
        internal static bool CheckParameterRegexReplacement(
            string parameterPath,
            TabInfo tab,
            FriendlyUrlSettings settings,
            int portalId,
            out string replacedPath,
            ref List<string> messages,
            out bool changeToSiteRoot,
            Guid parentTraceId)
        {
            bool replaced = false;
            replacedPath = string.Empty;
            changeToSiteRoot = false;
            if (messages == null)
            {
                messages = new List<string>();
            }

            var replaceActions = CacheController.GetParameterReplacements(settings, portalId, ref messages);
            if (replaceActions != null && replaceActions.Count > 0)
            {
                List<ParameterReplaceAction> parmReplaces = null;
                int tabId = tab.TabID;

                if (replaceActions.ContainsKey(tabId))
                {
                    // find the right set of replaced actions for this tab
                    parmReplaces = replaceActions[tabId];
                }

                // check for 'all tabs' replaceions
                if (replaceActions.ContainsKey(-1)) // -1 means 'all tabs' - replacing across all tabs
                {
                    // initialise to empty collection if there are no specific tab replaces
                    if (parmReplaces == null)
                    {
                        parmReplaces = new List<ParameterReplaceAction>();
                    }

                    // add in the all replaces
                    List<ParameterReplaceAction> allReplaces = replaceActions[-1];
                    parmReplaces.AddRange(allReplaces); // add the 'all' range to the tab range
                }

                if (parmReplaces != null)
                {
                    // OK what we have now is a list of replaces for the currently requested tab (either because it was specified by tab id,
                    // or because there is a replaced for 'all tabs'
                    try
                    {
                        foreach (ParameterReplaceAction parmReplace in parmReplaces)
                        {
                            // do a regex on the 'lookFor' in the parameter path
                            var parmRegex = RegexUtils.GetCachedRegex(
                                parmReplace.LookFor,
                                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            if (parmRegex.IsMatch(parameterPath))
                            {
                                replacedPath = parmRegex.Replace(parameterPath, parmReplace.ReplaceWith);
                                messages.Add(parmReplace.Name + " replace rule match, replaced : " + parameterPath + " with: " + replacedPath);
                                replaced = true;

                                // 593: if this replacement is marked as a site root replacement, we will be
                                // removing the page path from the final url
                                changeToSiteRoot = parmReplace.ChangeToSiteRoot;
                                break;
                            }

                            messages.Add(parmReplace.Name + " replace rule not matched {" + parameterPath + "}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // catch exceptions here because most likely to be related to regular expressions
                        // don't want to kill entire site because of this
                        Services.Exceptions.Exceptions.LogException(ex);
                        messages.Add("Exception : " + ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }

            return replaced;
        }

        internal static bool CheckUserProfileReplacement(
            string newPath,
            TabInfo tab,
            PortalSettings portalSettings,
            FriendlyUrlSettings settings,
            FriendlyUrlOptions options,
            out string changedPath,
            out bool changeToSiteRoot,
            out bool allowOtherParameters,
            ref List<string> meessages,
            Guid parentTraceId)
        {
            if (meessages == null)
            {
                meessages = new List<string>();
            }

            bool urlWasChanged = false;

            // initialise defaults to always return valid items
            changedPath = newPath;
            changeToSiteRoot = false;
            allowOtherParameters = true;

            // determine if this url should be converted to a userprofile url by checking the saved rules matching the tab/portalid
            if (portalSettings != null && tab.PortalID == portalSettings.PortalId &&
                    (tab.TabID == portalSettings.UserTabId || portalSettings.UserTabId == -1 ||
                        tab.ParentId == portalSettings.UserTabId)) // -1 == all tabs in portal
            {
                int userId;
                string rawUserId, remainingPath;

                // split the userid and other profile parameters from the friendly url path,
                // and return the userid and remaining parts as separate items
                SplitUserIdFromFriendlyUrlPath(
                    newPath,
                    "UserId",
                    string.Empty,
                    out rawUserId,
                    out remainingPath);
                if (rawUserId != null)
                {
                    meessages.Add("User Profile Url : RawUserId = " + rawUserId + " remainingPath = " + remainingPath);
                }
                else
                {
                    meessages.Add("User Profile Url : RawUserId = " + "null" + " remainingPath = " + remainingPath);
                }

                // the rawuserid is just the string representation of the userid from the path.
                // It should be considered 'untrusted' until cleaned up,
                // converted to an int and checked against the database
                if (!string.IsNullOrEmpty(rawUserId) && int.TryParse(rawUserId, out userId))
                {
                    bool doReplacement = false;
                    string urlName = string.Empty;

                    // Get the User
                    var user = UserController.GetUserById(portalSettings.PortalId, userId);

                    if (user != null && !string.IsNullOrEmpty(user.VanityUrl))
                    {
                        doReplacement = true;
                        urlName = (!string.IsNullOrEmpty(settings.VanityUrlPrefix)) ? string.Format("{0}/{1}", settings.VanityUrlPrefix, user.VanityUrl) : user.VanityUrl;
                        urlWasChanged = true;
                    }

                    if (doReplacement)
                    {
                        // check to see whether this is a match on the parentid or not
                        if (portalSettings.UserTabId == tab.ParentId && portalSettings.UserTabId > -1)
                        {
                            // replacing for the parent tab id
                            string childTabPath = TabIndexController.GetTabPath(tab, options, parentTraceId);
                            if (string.IsNullOrEmpty(childTabPath) == false)
                            {
                                // remove the parent tab path from the child tab path
                                TabInfo profilePage = TabController.Instance.GetTab(tab.ParentId, tab.PortalID, false);
                                string profilePagePath = TabIndexController.GetTabPath(profilePage, options, parentTraceId);
                                if (childTabPath.Contains(profilePagePath))
                                {
                                    // only replace when the child tab path contains the parent path - if it's a custom url that
                                    // doesn't incorporate the parent path, then leave it alone
                                    childTabPath = childTabPath.Replace(profilePagePath, string.Empty);
                                    childTabPath = childTabPath.Replace("//", "/");
                                    urlName += FriendlyUrlController.EnsureLeadingChar("/", childTabPath);
                                }
                            }
                        }

                        changedPath = "/" + urlName;

                        // append any extra remaining path value to the end
                        if (!string.IsNullOrEmpty(remainingPath))
                        {
                            if (remainingPath.StartsWith("/") == false)
                            {
                                changedPath += "/" + remainingPath;
                            }
                            else
                            {
                                changedPath += remainingPath;
                            }
                        }

                        urlWasChanged = true;
                        changeToSiteRoot = true; // we will be doing domain.com/urlname
                        allowOtherParameters = false;

                        // can't have any others (wouldn't have matched in the regex if there were)
                    }
                    else
                    {
                        meessages.Add("User Profile : doReplacement = false");
                    }
                }
            }

            return urlWasChanged;
        }

        /// <summary>
        /// Splits out the userid value from the supplied Friendly Url Path.
        /// </summary>
        /// <param name="parmName"></param>
        /// <param name="otherParametersPath">The 'other' parameters which form the total UserProfile Url (if supplied).</param>
        /// <param name="rawUserId"></param>
        /// <param name="remainingPath">The remaining path not associated with the user id.</param>
        /// <param name="urlPath"></param>
        private static void SplitUserIdFromFriendlyUrlPath(
            string urlPath,
            string parmName,
            string otherParametersPath,
            out string rawUserId,
            out string remainingPath)
        {
            // 688 : allow for other parts to be in the url by capturing more with the regex filters
            string regexPattern;
            rawUserId = null;
            remainingPath = string.Empty;

            // generally the path will start with a / and not end with one, but it's possible to get all sorts of things
            if (!string.IsNullOrEmpty(otherParametersPath))
            {
                // remove the trailing slash from otherParamtersPath if it exists, because the other parameters may be anywhere in the path
                if (otherParametersPath.EndsWith("/"))
                {
                    otherParametersPath = otherParametersPath.Substring(0, otherParametersPath.Length - 1);
                }

                const string patternFormatWithParameters = @"/?(?<rem1>.*)(?=_parm_)(?<parm1>(?<=/|^)(?:_parm_)/(?<p1v>[\d\w]+)){0,1}/?(?<op>_otherparm_){0,1}/?(?<parm2>(?<=/)(?:_parm_)/(?<p2v>[\d\w]+)){0,1}(?<rem2>.*)";
                regexPattern = patternFormatWithParameters.Replace("_parm_", parmName);
                regexPattern = regexPattern.Replace("_otherparm_", otherParametersPath);
            }
            else
            {
                const string patternNoParameters = @"/?(?<rem1>.*)(?<parm1>(?<=/|^)(?:_parm_)/(?<p1v>[\d\w]+)/?)+(?<rem2>.*)";
                regexPattern = patternNoParameters.Replace("_parm_", parmName);
            }

            // check the regex match
            Match parmMatch = Regex.Match(urlPath, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (parmMatch.Success)
            {
                // must be nothing in the op1 and op2 values
                Group otherParmsGp = parmMatch.Groups["op"];
                Group parm1ValueGp = parmMatch.Groups["p1v"];
                Group parm2ValueGp = parmMatch.Groups["p2v"];
                Group rem1ParmsGp = parmMatch.Groups["rem1"]; // remainder at the start of the match
                Group rem2ParmsGp = parmMatch.Groups["rem2"]; // remainder at the end of the match

                if (otherParmsGp != null && otherParmsGp.Success && (parm1ValueGp.Success || parm2ValueGp.Success))
                {
                    // matched the other parm value and either the p1 or p2 value
                    rawUserId = parm1ValueGp.Success ? parm1ValueGp.Value : parm2ValueGp.Value;
                }
                else
                {
                    if ((otherParmsGp == null || otherParmsGp.Success == false) && parm1ValueGp != null &&
                        parm1ValueGp.Success)
                    {
                        rawUserId = parm1ValueGp.Value;
                    }
                }

                // add back the remainders
                if (rem1ParmsGp != null && rem1ParmsGp.Success)
                {
                    remainingPath = rem1ParmsGp.Value;
                }

                if (rem2ParmsGp != null && rem2ParmsGp.Success)
                {
                    remainingPath += rem2ParmsGp.Value;
                }

                if (remainingPath.EndsWith("/"))
                {
                    remainingPath = remainingPath.Substring(0, remainingPath.Length - 1);
                }

                // 722: drop out the parts of the remaining path that are in the 'otherParameters' path.
                // the other parameters path will be automatically provided upon rewrite
                if (otherParametersPath != null)
                {
                    remainingPath = Regex.Replace(remainingPath, Regex.Escape(otherParametersPath), string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }

                if (parmName.Contains("|") && rawUserId != null)
                {
                    // eliminate any dups from the remaining path
                    string[] vals = parmName.Split('|');
                    foreach (string val in vals)
                    {
                        string find = "/?" + Regex.Escape(val + "/" + rawUserId);
                        remainingPath = Regex.Replace(remainingPath, find, string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                }

                if (remainingPath.Length > 0 && remainingPath.StartsWith("/") == false)
                {
                    remainingPath = "/" + remainingPath;
                }
            }
        }
    }
}
