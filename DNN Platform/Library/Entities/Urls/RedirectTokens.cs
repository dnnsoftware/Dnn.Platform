// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class contains helpers which set the redirect action and reason tokens.  These are fake additions to the rewritten query string
    /// which are used as a type of property to store intent of a particular url in the page index.  This is done to keep the base type
    /// stored in the page index dictionary as a value type (string) rather than a object type with properties.  So the two 'properties'
    /// of a Url are the action (ie 301 redirect, 302 redirect, 404, etc) and the reason (home page redirect, etc) are stored as
    /// part of the rewritten querystring in the index.   These then have to be removed and translated back to 'action' parameters
    /// when the rewriting actually happens.  So all methods in this class are to do with either storing or retrieving these tokens.
    /// </summary>
    internal static class RedirectTokens
    {
        private static readonly Regex RewritePathRx = new Regex(
            @"&rr=(?<rr>[^&].)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex RedirectTokensRx = new Regex(
            @"(?<=(?<p>&|\?))(?<tk>do301|do302|do404)=(?<val>[^&]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Adds on a redirect reason to the rewrite path.
        /// </summary>
        /// <param name="existingRewritePath"></param>
        /// <param name="action"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        internal static string AddRedirectReasonToRewritePath(string existingRewritePath, ActionType action, RedirectReason reason)
        {
            string result = existingRewritePath;
            string token, value;
            GetRedirectActionTokenAndValue(action, out token, out value);
            string tokenAndValue = token + "=" + value;
            bool addToken = true;

            // look for existing action
            bool hasDupes;
            Dictionary<string, string> tokensAndValues = GetRedirectTokensAndValuesFromRewritePath(
                existingRewritePath,
                out hasDupes);

            // can't overwrite existing tokens in certain cases
            if (tokensAndValues.Count > 0)
            {
                // only case we allow is an ovewrite of a do301=check by a do301=true or do302=true
                if (token == "do301" || token == "do302")
                {
                    if (tokensAndValues.ContainsKey("do301") && tokensAndValues["do301"] == "check")
                    {
                        result = existingRewritePath.Replace("do301=check", tokenAndValue);
                    }
                }

                addToken = false; // already done
            }

            if (addToken)
            {
                if (result.Contains(tokenAndValue) == false)
                {
                    if (result.Contains("?"))
                    {
                        result += "&" + tokenAndValue;
                    }
                    else
                    {
                        result += "?" + tokenAndValue;
                    }

                    // the reasonToken helps the rewrite process determine why a redirect is required
                    // after the token is stored in the page dictionary
                    string reasonToken = GetRedirectReasonRewriteToken(reason);
                    if (reasonToken != string.Empty)
                    {
                        result += reasonToken;
                    }
                }
                else
                {
                    // special case : add the number of times a 301 has been requested
                    if (tokenAndValue == "do301=true" && reason == RedirectReason.User_Profile_Url)
                    {
                        result += "&num301=2";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// DetermineRedirectReasonAndAction extracts the redirect value from the rewrite url and
        /// returns the new rewritten url, and the reason for the redirection, and an action value for the type of redirect.
        /// </summary>
        /// <param name="rewrittenUrl">Rewritten url as found in page dictionary.</param>
        /// <param name="result">The current rewrite result.</param>
        /// <param name="wasParms">true if there are parameters in the path, false if not.</param>
        /// <param name="settings">current FriendlyUrlSettings object.</param>
        /// <param name="action">New action value for UrlAction object.</param>
        /// <param name="reason">New redirect reason value for UrlAction object.</param>
        /// <param name="newUrl">Url to used for rewrite process.</param>
        internal static void DetermineRedirectReasonAndAction(
            string rewrittenUrl,
            UrlAction result,
            bool wasParms,
            FriendlyUrlSettings settings,
            out string newUrl,
            out RedirectReason reason,
            out ActionType action)
        {
            // init parms
            newUrl = rewrittenUrl;
            action = result.Action;
            reason = result.Reason;

            // get the action type from the rewrite path
            ActionType foundAction;
            bool actionInPath = GetActionFromRewritePath(rewrittenUrl, out foundAction);

            // only overrwrite action if it was found in the rewrite path
            if (actionInPath)
            {
                action = foundAction;
            }

            // get the list of redirect reason tokens from the url
            List<string> redirectReasons = GetRedirectReasonTokensFromRewritePath(rewrittenUrl);

            // when redirect action in path, and redirect reasons are empty, add custom redirect
            if (redirectReasons.Count == 0 && action != ActionType.Continue)
            {
                redirectReasons.Add("cr");
            }

            bool clearActionToken = false;
            foreach (string rrTkn in redirectReasons)
            {
                switch (rrTkn)
                {
                    case "up":
                        // user profile redirect
                        clearActionToken = true;
                        if (wasParms)
                        {
                            if (reason == RedirectReason.Not_Redirected)
                            {
                                if (settings.RedirectOldProfileUrl)
                                {
                                    reason = RedirectReason.User_Profile_Url;
                                    action = ActionType.CheckFor301;
                                }
                                else
                                {
                                    action = ActionType.Continue;
                                }
                            }
                        }
                        else
                        {
                            // if no parms, then we're not doing a userprofileaction redirect
                            reason = RedirectReason.Custom_Redirect;

                            // then check for a 301 redirect
                            action = ActionType.CheckFor301;
                        }

                        break;

                    case "dl":
                    case "db":
                        // deleted tab dl
                        // disabled tab db
                        clearActionToken = true;

                        // 626 Deleted tab hanlding not working properyly - override
                        if (settings.DeletedTabHandlingType == DeletedTabHandlingType.Do404Error)
                        {
                            action = ActionType.Output404; // output a 404 as per settings
                        }

                        // 838 : handle disabled pages separately
                        reason = rrTkn == "dl" ? RedirectReason.Deleted_Page : RedirectReason.Disabled_Page;
                        break;

                    case "pr":
                        // pr = permanent redirect
                        reason = RedirectReason.Tab_Permanent_Redirect;
                        clearActionToken = true;
                        break;

                    case "sr":
                        // sr = spaces replaced in url
                        clearActionToken = true;
                        reason = RedirectReason.Spaces_Replaced;
                        break;

                    case "hp":
                        // hp = home page redirect
                        if (wasParms)
                        {
                            // cancel the home page replaced if there were parameters added and page extensions
                            // are in use - otherwise a 404 will occur for the relative path
                            reason = RedirectReason.Not_Redirected;
                            action = ActionType.Continue;
                            clearActionToken = true;
                        }
                        else
                        {
                            reason = RedirectReason.Site_Root_Home;
                            clearActionToken = true;
                        }

                        break;

                    default:
                        // any other redirect with no reason is a custom redirect
                        if (reason == RedirectReason.Not_Redirected)
                        {
                            reason = RedirectReason.Custom_Redirect;
                        }

                        clearActionToken = true;
                        break;
                }
            }

            if (clearActionToken)
            {
                // clear both action and reason
                newUrl = RemoveAnyRedirectTokensAndReasons(newUrl);
            }
            else
            {
                // clear just reason
                newUrl = RemoveAnyRedirectReasons(newUrl);
            }
        }

        /// <summary>
        /// Return the action type from a rewritten Url.
        /// </summary>
        /// <param name="rewrittenUrl"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static bool GetActionFromRewritePath(string rewrittenUrl, out ActionType action)
        {
            bool found = false;
            action = ActionType.Continue; // default
            MatchCollection actionMatches = RedirectTokensRx.Matches(rewrittenUrl);
            foreach (Match actionMatch in actionMatches)
            {
                if (actionMatch.Success)
                {
                    found = true;
                    string tk = actionMatch.Groups["tk"].Value.ToLowerInvariant();
                    string val = actionMatch.Groups["val"].Value.ToLowerInvariant();
                    switch (tk)
                    {
                        case "do301":
                            if (val == "true")
                            {
                                action = ActionType.Redirect301;
                            }
                            else
                            {
                                if (val == "check")
                                {
                                    action = ActionType.CheckFor301;
                                }
                            }

                            break;
                        case "do404":
                            if (val == "true")
                            {
                                action = ActionType.Output404;
                            }

                            break;
                        case "do302":
                            if (val == "true")
                            {
                                action = ActionType.Redirect302;
                            }

                            break;
                    }

                    // if there is more than one match, if we have a solid action, then break
                    if (action != ActionType.Continue)
                    {
                        break; // this should, by rights, not happen in normal operation
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Removes any reason tokens from the querystring.
        /// </summary>
        /// <param name="rewritePath"></param>
        /// <returns></returns>
        internal static string RemoveAnyRedirectReasons(string rewritePath)
        {
            return RewritePathRx.Replace(rewritePath, string.Empty);
        }

        /// <summary>
        /// Removes any redirect tokens from the rewrite path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="queryStringCol"></param>
        /// <returns></returns>
        internal static string RemoveAnyRedirectTokens(string path, NameValueCollection queryStringCol)
        {
            // don't really care what the value is, but need it for replacing
            // the do301 is an internal value, used to control redirects from the page index
            if (string.IsNullOrEmpty(path) == false)
            {
                string val = null;
                if (queryStringCol != null)
                {
                    val = queryStringCol["do301"];
                }

                if (string.IsNullOrEmpty(val))
                {
                    val = "true";
                }

                // nix the 301 redirect query string value or terminal loops-a-plenty
                path = path.Replace("&do301=" + val, string.Empty);
                path = path.Replace("?do301=" + val, string.Empty);

                // 911 : object not set error
                if (queryStringCol != null)
                {
                    val = queryStringCol["do302"];
                }

                if (string.IsNullOrEmpty(val))
                {
                    val = "true";
                }

                // nix the 302 redirect query string value or terminal loops-a-plenty
                path = path.Replace("&do302=" + val, string.Empty);
                path = path.Replace("?do302=" + val, string.Empty);
            }

            return path;
        }

        /// <summary>
        /// Removes and redirect tokens and redirect reasons from the rewritePath.
        /// </summary>
        /// <param name="rewritePath"></param>
        /// <returns></returns>
        internal static string RemoveAnyRedirectTokensAndReasons(string rewritePath)
        {
            string result = RemoveAnyRedirectReasons(rewritePath);

            // regex expression matches a token and removes it
            Match tokenMatch = RedirectTokensRx.Match(result);
            if (tokenMatch.Success)
            {
                // tokenAndValue is the do301=true
                string tokenAndValue = tokenMatch.Value;

                // p is either a ? or a &
                string p = tokenMatch.Groups["p"].Value;
                if (p == "?")
                {
                    result = result.Replace(tokenAndValue, string.Empty);
                    if (result.Contains("?&"))
                    {
                        result = result.Replace("?&", "?");
                    }
                    else
                    {
                        if (result.EndsWith("?") || result.EndsWith("&"))
                        {
                            // trim end
                            result = result.Substring(0, result.Length - 1);
                        }
                    }
                }
                else
                {
                    // p == "&"
                    result = result.Replace("&" + tokenAndValue, string.Empty);
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the Action and Reason values in the UrlAction parameter.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="settings"></param>
        internal static void SetRedirectReasonAndAction(ref UrlAction result, FriendlyUrlSettings settings)
        {
            RedirectReason reason;
            ActionType action;
            string newUrl;
            DetermineRedirectReasonAndAction(result.RewritePath, result, true, settings, out newUrl, out reason,
                                             out action);
            result.Action = action;
            result.Reason = reason;
            result.RewritePath = newUrl;
        }

        /// <summary>
        /// Returns the list of tokens found in a rewrite path as a key/value dictionary.
        /// </summary>
        /// <param name="rewritePath">
        ///     Rewritten Url path.
        /// </param>
        /// <returns></returns>
        /// <summary>
        /// Returns a list of the redirect tokens found in the querystring.
        /// </summary>
        /// <returns></returns>
        private static List<string> GetRedirectReasonTokensFromRewritePath(string rewritePath)
        {
            var reasons = new List<string>();
            MatchCollection matches = RewritePathRx.Matches(rewritePath);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    Group rrG = match.Groups["rr"];
                    if (rrG.Success)
                    {
                        string rr = match.Groups["rr"].Value;
                        reasons.Add(rr);
                    }
                }
            }

            return reasons;
        }

        private static void GetRedirectActionTokenAndValue(ActionType action, out string token, out string value)
        {
            switch (action)
            {
                case ActionType.CheckFor301:
                    token = "do301";
                    value = "check";
                    break;
                case ActionType.Redirect301:
                    token = "do301";
                    value = "true";
                    break;
                case ActionType.Output404:
                    token = "do404";
                    value = "true";
                    break;
                case ActionType.Redirect302:
                    token = "do302";
                    value = "true";
                    break;
                default:
                    token = string.Empty;
                    value = string.Empty;
                    break;
            }
        }

        private static string GetRedirectReasonRewriteToken(RedirectReason reason)
        {
            string result = string.Empty;
            switch (reason)
            {
                case RedirectReason.Deleted_Page:
                    result = "&rr=dl";
                    break;
                case RedirectReason.Disabled_Page:
                    // 838 : handle disabled page separately
                    result = "&rr=db";
                    break;
                case RedirectReason.Tab_Permanent_Redirect:
                    result = "&rr=pr";
                    break;
                case RedirectReason.Spaces_Replaced:
                    result = "&rr=sr";
                    break;
                case RedirectReason.Site_Root_Home:
                    result = "&rr=hp";
                    break;
                case RedirectReason.Diacritic_Characters:
                    result = "&rr=dc";
                    break;
                case RedirectReason.User_Profile_Url:
                    result = "&rr=up";
                    break;
                case RedirectReason.Custom_Redirect:
                    result = "&rr=cr";
                    break;
            }

            return result;
        }

        private static Dictionary<string, string> GetRedirectTokensAndValuesFromRewritePath(string rewritePath, out bool hasDupes)
        {
            hasDupes = false;
            var results = new Dictionary<string, string>();
            MatchCollection matches = RedirectTokensRx.Matches(rewritePath);
            foreach (Match tokenMatch in matches)
            {
                string tk = tokenMatch.Groups["tk"].Value;
                string val = tokenMatch.Groups["val"].Value;
                if (results.ContainsKey(tk))
                {
                    hasDupes = true;
                }
                else
                {
                    results.Add(tk, val);
                }
            }

            return results;
        }
    }
}
