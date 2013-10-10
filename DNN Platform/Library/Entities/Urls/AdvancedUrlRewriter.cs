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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Services.EventQueue;

using Assembly = System.Reflection.Assembly;

#endregion

namespace DotNetNuke.Entities.Urls
{
    public class AdvancedUrlRewriter : UrlRewriterBase
    {
        #region Private Members

        private const string _productName = "AdvancedUrlRewriter";
        private FriendlyUrlSettings _settings;
        internal static readonly Regex ServiceApi = new Regex(@"DesktopModules/.+/API", RegexOptions.Compiled);

        #endregion

        #region Overridden Methods

        internal override void RewriteUrl(object sender, EventArgs e)
        {
            Guid parentTraceId = Guid.Empty;
            const bool debug = true;
            bool failedInitialization = false;
            bool ignoreForInstall = false;
            var app = (HttpApplication) sender;
            try
            {
                //875 : completely ignore install/upgrade requests immediately
                ignoreForInstall = IgnoreRequestForInstall(app.Request);

                if (ignoreForInstall == false)
                {
                    _settings = new FriendlyUrlSettings(-1);

                    SecurityCheck(app);
                }
                
            }
            catch (Exception ex)
            {
                //exception handling for advanced Url Rewriting requests
                failedInitialization = true;
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                if (app.Context != null)
                {
                    ShowDebugData(app.Context, app.Request.Url.AbsoluteUri, null, ex);
                    Handle404OrException(_settings, app.Context, ex, null, false, debug);
                }
                else
                {
                    throw;
                }
            }
            if (!failedInitialization && !ignoreForInstall)
            {
                //if made it through there and not installing, go to next call.  Not in exception catch because it implements it's own top-level exception handling
                var request = app.Context.Request;

                //829 : change constructor to stop using physical path
                var result = new UrlAction(request)
                                        {
                                            IsSecureConnection = request.IsSecureConnection,
                                            IsSSLOffloaded = IsSSLOffloadEnabled(request),
                                            RawUrl = request.RawUrl
                                        };
                ProcessRequest(app.Context,
                                    app.Context.Request.Url,
                                     Host.Host.UseFriendlyUrls,
                                     result,
                                     _settings,
                                     true,
                                     parentTraceId);
            }
        }

        #endregion

        #region Public Methods

        public void ProcessTestRequestWithContext(HttpContext context, 
                                                    Uri requestUri,
                                                    bool useFriendlyUrls, 
                                                    UrlAction result, 
                                                    FriendlyUrlSettings settings)
        {
            Guid parentTraceId = Guid.Empty;
            _settings = settings;
            ProcessRequest(context, 
                                requestUri,
                                useFriendlyUrls, 
                                result,
                                settings, 
                                false, 
                                parentTraceId);
        }

        #endregion

        #region Private Methods

        private PortalAliasInfo GetPortalAlias(FriendlyUrlSettings settings, string requestUrl, out bool redirectAlias, out bool isPrimaryAlias, out string wrongAlias)
        {
            PortalAliasInfo alias = null;
            redirectAlias = false;
            wrongAlias = null;
            isPrimaryAlias = false;
            OrderedDictionary portalRegexes = TabIndexController.GetPortalAliasRegexes(settings);
            foreach (string regexPattern in portalRegexes.Keys)
            {
                //split out the portal alias from the regex pattern representing that alias
                Match aliasMatch = Regex.Match(requestUrl, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (aliasMatch.Success)
                {
                    //check for mobile browser and matching
                    var aliasEx = (PortalAliasInfo)portalRegexes[regexPattern];
                    redirectAlias = aliasEx.Redirect;
                    if (redirectAlias)
                    {
                        wrongAlias = aliasMatch.Groups["alias"].Value;
                    }
                    isPrimaryAlias = aliasEx.IsPrimary;
                    alias = aliasEx;
                    break;
                }
            }
            return alias;
        }

        private void ProcessRequest(HttpContext context,
                                Uri requestUri,
                                bool useFriendlyUrls,
                                UrlAction result,
                                FriendlyUrlSettings settings,
                                bool allowSettingsChange,
                                Guid parentTraceId)
        {
            bool finished = false;
            bool showDebug = false;
            bool postRequest = false;

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            string requestType = request.RequestType;
            NameValueCollection queryStringCol = request.QueryString;

            try
            {

                string fullUrl, querystring;
                //699: get the full url based on the request and the quersytring, rather than the requestUri.ToString()
                //there is a difference in encoding, which can corrupt results when an encoded value is in the querystring
                RewriteController.GetUrlWithQuerystring(request, requestUri, out fullUrl, out querystring);

                showDebug = CheckForDebug(request, queryStringCol, settings.AllowDebugCode);
                string ignoreRegex = settings.IgnoreRegex;
                bool ignoreRequest = IgnoreRequest(result, fullUrl, ignoreRegex, request);
                bool redirectAlias = false;
                if (!ignoreRequest)
                {
                    //set original path
                    context.Items["UrlRewrite:OriginalUrl"] = requestUri.AbsoluteUri;

                    //set the path of the result object, and determine if a redirect is allowed on this request
                    result.SetOriginalPath(requestUri.ToString(), settings);
                    //737 : set the mobile browser 
                    result.SetBrowserType(request, response, settings);
                    //add to context
                    context.Items["UrlRewrite:BrowserType"] = result.BrowserType.ToString();

                    //839 : split out this check
                    result.SetRedirectAllowed(result.OriginalPath, settings);

                    //find the portal alias first
                    string wrongAlias;
                    bool isPrimaryAlias;
                    var requestedAlias = GetPortalAlias(settings, fullUrl, out redirectAlias, out isPrimaryAlias, out wrongAlias);
                    
                    if (requestedAlias != null)
                    {
                        //827 : now get the correct settings for this portal (if not a test request)
                        //839 : separate out redirect check as well and move above first redirect test (ConfigurePortalAliasRedirect)
                        if (allowSettingsChange)
                        {
                            settings = new FriendlyUrlSettings(requestedAlias.PortalID);
                            result.SetRedirectAllowed(result.OriginalPath, settings);
                        }
                        result.PortalAlias = requestedAlias;
                        result.PrimaryAlias = requestedAlias;//this is the primary alias
                        result.PortalId = requestedAlias.PortalID;
                        result.CultureCode = requestedAlias.CultureCode;
                        //get the portal alias mapping for this portal
                        result.PortalAliasMapping = PortalSettings.GetPortalAliasMappingMode(requestedAlias.PortalID);

                        //if requested alias wasn't the primary, we have a replacement, redirects are allowed and the portal alias mapping mode is redirect
                        //then do a redirect based on the wrong portal
                        if ((redirectAlias && wrongAlias != null) && result.RedirectAllowed && result.PortalAliasMapping != PortalSettings.PortalAliasMapping.Redirect)
                        {
                            //this is the alias, we are going to enforce it as the primary alias
                            result.PortalAlias = requestedAlias;
                            result.PrimaryAlias = requestedAlias;
                            //going to redirect this alias because it is incorrect
                            //or do we just want to mark as 'check for 301??'
                            redirectAlias = ConfigurePortalAliasRedirect(ref result,
                                                                            wrongAlias,
                                                                            requestedAlias.HTTPAlias,
                                                                            false,
                                                                            settings.InternalAliasList,
                                                                            settings);
                        }
                        else
                        {
                            //do not redirect the wrong alias, but set the primary alias value
                            if (wrongAlias != null)
                            {
                                //get the portal alias info for the requested alias (which is the wrong one)
                                //and set that as the alias, but also set the found alias as the primary
                                PortalAliasInfo wrongAliasInfo = PortalAliasController.GetPortalAliasInfo(wrongAlias);
                                if (wrongAliasInfo != null)
                                {
                                    result.PortalAlias = wrongAliasInfo;
                                    result.PrimaryAlias = requestedAlias;
                                }
                                
                            }
                        }
                    }
                }
                ignoreRegex = settings.IgnoreRegex;
                ignoreRequest = IgnoreRequest(result, fullUrl, ignoreRegex, request);
                if (!ignoreRequest) 
                {
                    //check to see if a post request
                    if (request.RequestType == "POST")
                    {
                        postRequest = true;
                    }

                    //check the portal alias again.  This time, in more depth now that the portal Id is known 
                    //this check handles browser types/language specific aliases & mobile aliases
                    string primaryHttpAlias;
                    if (!redirectAlias && IsPortalAliasIncorrect(context, request, requestUri, result, queryStringCol, settings, parentTraceId, out primaryHttpAlias))
                    {
                        //it was an incorrect alias
                        PortalAliasInfo primaryAlias = PortalAliasController.GetPortalAliasInfo(primaryHttpAlias);
                        if (primaryAlias != null) result.PrimaryAlias = primaryAlias;
                        //try and redirect the alias if the settings allow it
                        redirectAlias = RedirectPortalAlias(primaryHttpAlias, ref result, settings);
                    }

                    if (redirectAlias)
                    {
                        //not correct alias for portal : will be redirected
                        //perform a 301 redirect if one has already been found
                        response.Status = "301 Moved Permanently";
                        response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                        response.AddHeader("Location", result.FinalUrl);
                        finished = true;
                    }
                    if (!finished)
                    {
                        //Check to see if this to be rewritten into default.aspx?tabId=nn format
                        //this call is the main rewriting matching call.  It makes the decision on whether it is a 
                        //physical file, whether it is toe be rewritten or redirected by way of a stored rule

                        //Check if we have a standard url
                        var uri = new Uri(fullUrl);
                        if (uri.PathAndQuery.ToLowerInvariant().StartsWith("/" + Globals.glbDefaultPage.ToLowerInvariant()))
                        {
                            result.DoRewrite = true;
                            result.Action = ActionType.CheckFor301;
                            result.RewritePath = Globals.glbDefaultPage + uri.Query;
                        }
                        else
                        {
                            bool isPhysicalResource;
                            CheckForRewrite(fullUrl, querystring, result, useFriendlyUrls, queryStringCol, settings, out isPhysicalResource, parentTraceId);
                        }

                        //return 404 if there is no portal alias for a rewritten request
                        if (result.DoRewrite && result.PortalAlias == null)
                        {
                            //882 : move this logic in from where it was before to here
                            //so that non-rewritten requests don't trip over it
                            //no portal alias found for the request : that's a 404 error
                            result.Action = ActionType.Output404;
                            result.Reason = RedirectReason.No_Portal_Alias;

                            Handle404OrException(settings, context, null, result, false, showDebug);
                            finished = true; //cannot fulfil request unless correct portal alias specified
                        }
                    }

                    if (!finished && result.DoRewrite)
                    {
                        //check the identified portal alias details for any extra rewrite information required
                        //this includes the culture and the skin, which can be placed into the rewrite path
                        //This logic is here because it will catch any Urls which are excluded from rewriting
                        var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();

                        if (result.PortalId > -1 && result.HttpAlias != null)
                        {
                            string culture;
                            string skin;
                            BrowserTypes browserType;
                            primaryAliases.GetSettingsByPortalIdAndAlias(result.PortalId, result.HttpAlias,
                                                                                            out culture, 
                                                                                            out browserType,
                                                                                            out skin);
                            //add language code to path if it exists (not null) and if it's not already there
                            string rewritePath = result.RewritePath;
                            if (RewriteController.AddLanguageCodeToRewritePath(ref rewritePath, culture))
                            {
                                result.CultureCode = culture;
                            }

                            //852: add skinSrc to path if it exists and if it's not already there
                            string debugMessage;
                            RewriteController.AddSkinToRewritePath(result.TabId, result.PortalId, ref rewritePath, skin, out debugMessage);
                            result.RewritePath = rewritePath; //reset back from ref temp var
                            if (debugMessage != null)
                            {
                                result.DebugMessages.Add(debugMessage);
                            }
                        }
                    }

                    if (!finished && result.DoRewrite)
                    {
                        //if so, do the rewrite
                        if (result.RewritePath.StartsWith(result.Scheme) || result.RewritePath.StartsWith(Globals.glbDefaultPage) == false)
                        {
                            if (result.RewritePath.Contains(Globals.glbDefaultPage) == false)
                            {
                                RewriterUtils.RewriteUrl(context, "~/" + result.RewritePath);
                            }
                            else
                            {
                                //if there is no TabId and we have the domain
                                if (!result.RewritePath.ToLowerInvariant().Contains("tabId="))
                                {
                                    RewriterUtils.RewriteUrl(context, "~/" + result.RewritePath);
                                }
                                else
                                {
                                    RewriterUtils.RewriteUrl(context, result.RewritePath);
                                }
                            }
                        }
                        else
                        {
                            RewriterUtils.RewriteUrl(context, "~/" + result.RewritePath);
                        }
                    }

                    //confirm which portal the request is for 
                    if (!finished)
                    {
                        IdentifyPortalAlias(context, request, requestUri, result, queryStringCol, settings, parentTraceId);
                        if (result.Action == ActionType.Redirect302Now)
                        {
                            //performs a 302 redirect if requested
                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                            response.Redirect(result.FinalUrl, false);
                            finished = true;
                        }
                        else
                        {
                            if (result.Action == ActionType.Redirect301 && !string.IsNullOrEmpty(result.FinalUrl))
                            {
                                finished = true;
                                //perform a 301 redirect if one has already been found
                                response.Status = "301 Moved Permanently";
                                response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                response.AddHeader("Location", result.FinalUrl);
                            }
                        }
                    }
                    //look for a 404 result from the rewrite, because of a deleted page or rule
                    if (!finished && result.Action == ActionType.Output404)
                    {
                        if (result.DomainName == result.HttpAlias && result.PortalAlias != null
								&& result.Reason != RedirectReason.Deleted_Page && result.Reason != RedirectReason.Disabled_Page)
                        {
                            //Request for domain with no page identified (and no home page set in Site Settings)
                            result.Action = ActionType.Continue;
                        }
                        else
                        {
                            finished = true;
                            response.AppendHeader("X-Result-Reason", result.Reason.ToString().Replace("_", " "));

                            if (showDebug)
                            {
                                ShowDebugData(context, requestUri.AbsoluteUri, result, null);
                            }
                            //show the 404 page if configured
                            result.Reason = RedirectReason.Requested_404;
                            Handle404OrException(settings, context, null, result, true, showDebug);
                        }
                    }

                    if (!finished)
                    {
                        //check to see if this tab has an external url that should be forwared or not
                        finished = CheckForTabExternalForwardOrRedirect(context, ref result, response, settings, parentTraceId);
                    }

                    //check for a parameter redirect (we had to do all the previous processing to know we are on the right portal and identify the tabid)
                    //if the CustomParmRewrite flag is set, it means we already rewrote these parameters, so they have to be correct, and aren't subject to 
                    //redirection.  The only reason to do a custom parm rewrite is to interpret already-friendly parameters
                    if (!finished
                        && !postRequest /* either request is null, or it's not a post - 551 */
                        && result.HttpAlias != null /* must have a http alias */
                        && !result.CustomParmRewrite && /* not custom rewritten parms */
                        ((settings.EnableCustomProviders &&
                          RedirectController.CheckForModuleProviderRedirect(requestUri, ref result, queryStringCol, settings, parentTraceId))
                        //894 : allow disable of all custom providers
                         ||
                         RedirectController.CheckForParameterRedirect(requestUri, ref result, queryStringCol, settings)))
                    {
                        //301 redirect to new location based on parameter match
                        if (response != null)
                        {
                            switch (result.Action)
                            {
                                case ActionType.Redirect301:
                                    response.Status = "301 Moved Permanently";
                                    response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                    response.AddHeader("Location", result.FinalUrl);
                                    break;
                                case ActionType.Redirect302:
                                    response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                    response.Redirect(result.FinalUrl);
                                    break;
                                case ActionType.Output404:
                                    response.AppendHeader("X-Result-Reason", result.Reason.ToString().Replace("_", " "));
                                    Handle404OrException(settings, context, null, result, true, showDebug);
                                    break;
                            }
                        }
                        finished = true;
                    }

                    if (!finished)
                    {
                        //add the portal settings to the app context if the portal alias has been found and is correct
                        if (result.PortalId != -1 && result.PortalAlias != null)
                        {
                            Globals.SetApplicationName(result.PortalId);
                            // load the PortalSettings into current context 
                            var portalSettings = new PortalSettings(result.TabId, result.PortalAlias);
                            //set the primary alias if one was specified
                            if (result.PrimaryAlias != null) portalSettings.PrimaryAlias = result.PrimaryAlias;

                            if (result.CultureCode != null && fullUrl.Contains(result.CultureCode) &&
                                portalSettings.DefaultLanguage == result.CultureCode)
                            {
                                //when the request culture code is the same as the portal default, check for a 301 redirect, because we try and remove the language from the url where possible
                                result.Action = ActionType.CheckFor301;
                            }

                            int portalHomeTabId = portalSettings.HomeTabId;
                            if (context != null && portalSettings != null && !context.Items.Contains("PortalSettings"))
                            {
                                context.Items.Add("PortalSettings", portalSettings);
                            }
                            //check if a secure redirection is needed
                            //this would be done earlier in the piece, but need to know the portal settings, tabid etc before processing it
                            bool redirectSecure = CheckForSecureRedirect(portalSettings, requestUri, result, queryStringCol, settings);
                            if (redirectSecure)
                            {
                                if (response != null)
                                {
                                    //702 : don't check final url until checked for null reference first
                                    if (result.FinalUrl != null)
                                    {
                                        if (result.FinalUrl.StartsWith("https://"))
                                        {
                                            if (showDebug)
                                            {
                                                /*
                                                string debugMsg = "{0}, {1}, {2}, {3}, {4}";
                                                string productVer = System.Reflection.Assembly.GetExecutingAssembly().GetName(false).Version.ToString();
                                                response.AppendHeader("X-" + prodName + "-Debug", string.Format(debugMsg, requestUri.AbsoluteUri, result.FinalUrl, result.RewritePath, result.Action, productVer));
                                                 */
                                                ShowDebugData(context, fullUrl, result, null);
                                            }
                                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.Redirect(result.FinalUrl, false);
                                            finished = true;
                                        }
                                        else
                                        {
                                            if (settings.SSLClientRedirect)
                                            {
                                                //redirect back to http version, use client redirect
                                                response.Clear();
                                                // add a refresh header to the response 
                                                response.AddHeader("Refresh", "0;URL=" + result.FinalUrl);
                                                // add the clientside javascript redirection script
                                                response.Write("<html><head><title></title>");
                                                response.Write(@"<!-- <script language=""javascript"">window.location.replace(""" + result.FinalUrl + @""")</script> -->");
                                                response.Write("</head><body><div><a href='" + result.FinalUrl + "'>" + result.FinalUrl + "</a></div></body></html>");
                                                if (showDebug)
                                                {
                                                    /*
                                                    string debugMsg = "{0}, {1}, {2}, {3}, {4}";
                                                    string productVer = System.Reflection.Assembly.GetExecutingAssembly().GetName(false).Version.ToString();
                                                    response.AppendHeader("X-" + prodName + "-Debug", string.Format(debugMsg, requestUri.AbsoluteUri, result.FinalUrl, result.RewritePath, result.Action, productVer));
                                                     */
                                                    ShowDebugData(context, fullUrl, result, null);
                                                }
                                                // send the response
                                                //891 : reinstate the response.end to stop the entire page loading
                                                response.End();
                                                finished = true;
                                            }
                                            else
                                            {
                                                response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                                response.Redirect(result.FinalUrl, false);
                                                finished = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //check for, and do a 301 redirect if required 
                                if (CheckForRedirects(requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId))
                                {
                                    if (response != null)
                                    {
                                        if (result.Action == ActionType.Redirect301)
                                        {
                                            response.Status = "301 Moved Permanently";
                                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.AddHeader("Location", result.FinalUrl);
                                            finished = true;
                                        }
                                        else if (result.Action == ActionType.Redirect302)
                                        {
                                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.Redirect(result.FinalUrl, false);
                                            finished = true;
                                        }
                                    }
                                }
                                else
                                {
                                    //612 : Don't clear out a 302 redirect if set
                                    if (result.Action != ActionType.Redirect302 &&
                                        result.Action != ActionType.Redirect302Now)
                                    {
                                        result.Reason = RedirectReason.Not_Redirected;
                                        result.FinalUrl = null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // alias does not exist in database 
                            // and all attempts to find another have failed 
                            //this should only happen if the HostPortal does not have any aliases 
                            result.Action = ActionType.Output404;
                            if (response != null)
                            {
                                if (showDebug)
                                {
                                    ShowDebugData(context, fullUrl, result, null);
                                }
                                result.Reason = RedirectReason.Requested_404;
                                //912 : change 404 type to transfer to allow transfer to main portal in single-portal installs
                                Handle404OrException(settings, context, null, result, true, showDebug);
                                finished = true;
                            }
                        }
                    }

                    //404 page ??
                    if (settings.TabId404 > 0 && settings.TabId404 == result.TabId)
                    {
                        string status = queryStringCol["status"];
                        if (status == "404")
                        {
                            //respond with a 404 error
                            result.Action = ActionType.Output404;
                            result.Reason = RedirectReason.Requested_404_In_Url;
                            Handle404OrException(settings, context, null, result, true, showDebug);
                        }
                    }
                    else
                    {
                        if (result.DoRewrite == false && result.CanRewrite != StateBoolean.False && !finished &&
                            result.Action == ActionType.Continue)
                        {
                            //739 : catch no-extension 404 errors
                            string pathWithNoQs = result.OriginalPath;
                            if (pathWithNoQs.Contains("?"))
                            {
                                pathWithNoQs = pathWithNoQs.Substring(0, pathWithNoQs.IndexOf("?", StringComparison.Ordinal));
                            }
                            if (!pathWithNoQs.Substring(pathWithNoQs.Length - 5, 5).Contains("."))
                            {
                                //no page extension, output a 404 if the Url is not found
                                //766 : check for physical path before passing off as a 404 error
                                //829 : change to use action physical path
                                //893 : filter by regex pattern to exclude urls which are valid, but show up as extensionless
                                if ((request != null && Directory.Exists(result.PhysicalPath))
                                    ||
                                    Regex.IsMatch(pathWithNoQs, settings.ValidExtensionlessUrlsRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                                {
                                    //do nothing : it's a request for a valid physical path, maybe including a default document
                                    result.VirtualPath = StateBoolean.False;
                                }
                                else
                                {
                                    if (!ServiceApi.IsMatch(context.Request.RawUrl))
                                    {
                                        //no physical path, intercept the request and hand out a 404 error
                                        result.Action = ActionType.Output404;
                                        result.Reason = RedirectReason.Page_404;
                                        result.VirtualPath = StateBoolean.True;
                                        //add in a message to explain this 404, becaue it can be cryptic
                                        result.DebugMessages.Add("404 Reason : Not found and no extension");
                                        Handle404OrException(settings, context, null, result, true, showDebug);
                                    }
                                }
                            }
                        }
                    }

                    //  show debug messages after extensionless-url special 404 handling
                    if (showDebug)
                    {
                        ShowDebugData(context, fullUrl, result, null);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //do nothing, a threadAbortException will have occured from using a server.transfer or response.redirect within the code block.  This is the highest
                //level try/catch block, so we handle it here.
            }
            catch (Exception ex)
            {
                if (showDebug)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                }
                if (response != null)
                {
                    if (showDebug)
                    {
                        ShowDebugData(context, requestUri.AbsoluteUri, result, ex);
                    }
                    if (result != null)
                    {
                        result.Ex = ex;
                        result.Reason = RedirectReason.Exception;
                    }
                    Handle404OrException(settings, context, ex, result, false, showDebug);
                }
                else
                {
                    if (result != null && result.DebugMessages != null)
                    {
                        result.DebugMessages.Add("Exception: " + ex.Message);
                        result.DebugMessages.Add("Stack Trace: " + ex.StackTrace);
                    }
                    throw;
                }
            }
            finally
            {
                //809 : add in new code copied from urlRewrite class in standard Url Rewrite module
                if (context != null && context.Items["FirstRequest"] != null)
                {
                    context.Items.Remove("FirstRequest");
                    //process any messages in the eventQueue for the Application_Start_FIrstRequest event
                    EventQueueController.ProcessMessages("Application_Start_FirstRequest");
                }
            }
        }

        private static void ShowDebugData(HttpContext context, string requestUri, UrlAction result, Exception ex)
        {
            if (context != null)
            {
                HttpResponse response = context.Response;

                //handle null responses wherever they might be found - this routine must be tolerant to all kinds of invalid inputs
                if (requestUri == null)
                {
                    requestUri = "null Uri";
                }
                string finalUrl = "null final Url";
                string rewritePath = "null rewrite path";
                string action = "null action";
                if (result != null)
                {
                    finalUrl = result.FinalUrl;
                    action = result.Action.ToString();
                    rewritePath = result.RewritePath;
                }
                //format up the error message to show
                const string debugMsg = "{0}, {1}, {2}, {3}, {4}, {5}, {6}";
                string productVer = Assembly.GetExecutingAssembly().GetName(false).Version.ToString();
                string portalSettings = "";
                string browser = "Unknown";
                //949 : don't rely on 'result' being non-null
                if (result != null)
                {
                    browser = result.BrowserType.ToString();
                }
                if (context.Items.Contains("PortalSettings"))
                {
                    var ps = (PortalSettings) context.Items["PortalSettings"];
                    if (ps != null)
                    {
                        portalSettings = ps.PortalId.ToString();
                        if (ps.PortalAlias != null)
                        {
                            portalSettings += ":" + ps.PortalAlias.HTTPAlias;
                        }
                    }
                }
                response.AppendHeader("X-" + _productName + "-Debug",
                                      string.Format(debugMsg, requestUri, finalUrl, rewritePath, action, productVer,
                                                    portalSettings, browser));
#if (DEBUG)
                var rawOutput = new StringWriter();
                rawOutput.WriteLine("<div style='background-color:black;color:white;'>");
                rawOutput.WriteLine("<p>Advanced Url Rewriting Debug Output</p>");
                rawOutput.WriteLine("<p>" +
                                    string.Format(debugMsg, requestUri, finalUrl, rewritePath, action, productVer,
                                                  portalSettings, browser) + "</p>");
#endif
                int msgNum = 1;
                if (result != null)
                {
                    foreach (string msg in result.DebugMessages)
                    {
#if (DEBUG)
                        rawOutput.WriteLine("<div>Debug Message " + msgNum.ToString("00") + ": " + msg + "</div>");
#endif
                        response.AppendHeader("X-" + _productName + "-Debug-" + msgNum.ToString("00"), msg);
                        msgNum++;
                    }
                }
                if (ex != null)
                {
                    response.AppendHeader("X-" + _productName + "-Ex", ex.Message);
                }
#if (DEBUG)
                if (ex != null)
                {
                    rawOutput.WriteLine("Exception : " + ex.Message);
                }
                else
                {
                    rawOutput.WriteLine("No Exception");
                }

                rawOutput.WriteLine("</div>");
                response.Write(rawOutput.ToString());
#endif
            }
        }

        private static void Handle404OrException(FriendlyUrlSettings settings, HttpContext context, Exception ex, UrlAction result, bool transfer, bool showDebug)
        {
            //handle Auto-Add Alias
            if (result.Action == ActionType.Output404 && CanAutoAddPortalAlias())
            {
                //Need to determine if this is a real 404 or a possible new alias.
                var portalId = Host.Host.HostPortalID;
                if (portalId > Null.NullInteger)
                {
                    //Get all the existing aliases
                    var aliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();

                    bool autoaddAlias;
                    bool isPrimary = false;
                    if (!aliases.Any())
                    {
                        autoaddAlias = true;
                        isPrimary = true;
                    }
                    else
                    {
                        autoaddAlias = true;
                        foreach (var alias in aliases)
                        {
                            if (result.DomainName.ToLowerInvariant().IndexOf(alias.HTTPAlias, StringComparison.Ordinal) == 0
                                    && result.DomainName.Length >= alias.HTTPAlias.Length)
                            {
                                autoaddAlias = false;
                                break;
                            }
                        }
                    }

                    if (autoaddAlias)
                    {
                        var portalAliasInfo = new PortalAliasInfo
                                                  {
                                                      PortalID = portalId, 
                                                      HTTPAlias = result.RewritePath,
                                                      IsPrimary = isPrimary
                                                  };
                        TestablePortalAliasController.Instance.AddPortalAlias(portalAliasInfo);

                        context.Response.Redirect(context.Request.Url.ToString(), true);
                    }
                }
            }


            if (context != null)
            {
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;
                HttpServerUtility server = context.Server;

                const string errorPageHtmlHeader = @"<html><head><title>{0}</title></head><body>";
                const string errorPageHtmlFooter = @"</body></html>";
                var errorPageHtml = new StringWriter();
                CustomErrorsSection ceSection = null;
                //876 : security catch for custom error reading
                try
                {
                    ceSection = (CustomErrorsSection) WebConfigurationManager.GetSection("system.web/customErrors");
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {
                    //on some medium trust environments, this will throw an exception for trying to read the custom Errors
                    //do nothing
                }

                /* 454 new 404/500 error handling routine */
                bool useDNNTab = false;
                int errTabId = -1;
                string errUrl = null;
                string status = "";
                bool isPostback = false;
                if (settings != null)
                {
                    if (request.RequestType == "POST")
                    {
                        isPostback = true;
                    }
                    if (result != null && ex != null)
                    {
                        result.DebugMessages.Add("Exception: " + ex.Message);
                        result.DebugMessages.Add("Stack Trace: " + ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            result.DebugMessages.Add("Inner Ex : " + ex.InnerException.Message);
                            result.DebugMessages.Add("Stack Trace: " + ex.InnerException.StackTrace);
                        }
                        else
                        {
                            result.DebugMessages.Add("Inner Ex : null");
                        }
                    }
                    string errRH;
                    string errRV;
                    int statusCode;
                    if (result != null && result.Action != ActionType.Output404)
                    {
                        //output everything but 404 (usually 500)
                        if (settings.TabId500 > -1) //tabid specified for 500 error page, use that
                        {
                            useDNNTab = true;
                            errTabId = settings.TabId500;
                        }
                        errUrl = settings.Url500;
                        errRH = "X-UrlRewriter-500";
                        errRV = "500 Rewritten to {0} : {1}";
                        statusCode = 500;
                        status = "500 Internal Server Error";
                    }
                    else //output 404 error 
                    {
                        if (settings.TabId404 > -1) //if the tabid is specified for a 404 page, then use that
                        {
                            useDNNTab = true;
                            errTabId = settings.TabId404;
                        }
                        if (!String.IsNullOrEmpty(settings.Regex404))
                            //with 404 errors, there's an option to catch certain urls and use an external url for extra processing.
                        {
                            try
                            {
                                //944 : check the original Url in case the requested Url has been rewritten before discovering it's a 404 error
                                string requestedUrl = request.Url.ToString();
                                if (result != null && string.IsNullOrEmpty(result.OriginalPath) == false)
                                {
                                    requestedUrl = result.OriginalPath;
                                }
                                if (Regex.IsMatch(requestedUrl, settings.Regex404, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                                {
                                    useDNNTab = false;
                                        //if we have a match in the 404 regex value, then don't use the tabid
                                }
                            }
                            catch (Exception regexEx)
                            {
                                //.some type of exception : output in response header, and go back to using the tabid 
                                response.AppendHeader("X-UrlRewriter-404Exception", regexEx.Message);
                            }
                        }
                        errUrl = settings.Url404;
                        errRH = "X-UrlRewriter-404";
                        errRV = "404 Rewritten to {0} : {1} : Reason {2}";
                        status = "404 Not Found";
                        statusCode = 404;
                    }

                    // check for 404 logging
                    if ((result == null || result.Action == ActionType.Output404))
                    {
                        //Log 404 errors to Event Log
                        UrlRewriterUtils.Log404(request, settings, result);
                    }
                    //912 : use unhandled 404 switch
                    string reason404 = null;
                    bool unhandled404 = true;
                    if (useDNNTab && errTabId > -1)
                    {
                        unhandled404 = false; //we're handling it here
                        var tc = new TabController();
                        TabInfo errTab = tc.GetTab(errTabId, result.PortalId, true);
                        if (errTab != null)
                        {
                            bool redirect = false;
                            //ok, valid tabid.  what we're going to do is to load up this tab via a rewrite of the url, and then change the output status
                            string reason = "Not Found";
                            if (result != null)
                            {
                                reason = result.Reason.ToString();
                            }
                            response.AppendHeader(errRH, string.Format(errRV, "DNN Tab",
                                                                errTab.TabName + "(Tabid:" + errTabId.ToString() + ")",
                                                                reason));
                            //show debug messages even if in debug mode
                            if (context != null && response != null && result != null && showDebug)
                            {
                                ShowDebugData(context, result.OriginalPath, result, null);
                            }
                            if (!isPostback)
                            {
                                response.ClearContent();
                                response.StatusCode = statusCode;
                                response.Status = status;
                            }
                            else
                            {
                                redirect = true;
                                    //redirect postbacks as you can't postback successfully to a server.transfer
                            }
                            errUrl = Globals.glbDefaultPage + TabIndexController.CreateRewritePath(errTab.TabID, "");
                            //have to update the portal settings with the new tabid
                            PortalSettings ps = null;
                            if (context != null && context.Items != null)
                            {
                                if (context.Items.Contains("PortalSettings"))
                                {
                                    ps = (PortalSettings) context.Items["PortalSettings"];
                                    context.Items.Remove("PortalSettings"); //nix it from the context
                                }
                            }
                            if (ps != null && ps.PortalAlias != null)
                            {
                                ps = new PortalSettings(errTabId, ps.PortalAlias);
                            }
                            else
                            {
                                if (result.HttpAlias != null && result.PortalId > -1)
                                {
                                    var pac = new PortalAliasController();
                                    PortalAliasInfo pa = pac.GetPortalAlias(result.HttpAlias, result.PortalId);
                                    ps = new PortalSettings(errTabId, pa);
                                }
                                else
                                {
                                    //912 : handle 404 when no valid portal can be identified
                                    //results when iis is configured to handle portal alias, but 
                                    //DNN isn't.  This always returns 404 because a multi-portal site
                                    //can't just show the 404 page of the host site.
                                    var pc = new PortalController();
                                    ArrayList portals = pc.GetPortals();
                                    if (portals != null && portals.Count == 1)
                                    {
                                        //single portal install, load up portal settings for this portal
                                        var singlePortal = (PortalInfo) portals[0];
                                        //list of aliases from database
                                        var aliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(singlePortal.PortalID).ToList();
                                        //list of aliases from Advanced Url settings
                                        List<string> chosen = aliases.GetAliasesForPortalId(singlePortal.PortalID);
                                        PortalAliasInfo useFor404 = null;
                                        //go through all aliases and either get the first valid one, or the first 
                                        //as chosen in the advanced url management settings
                                        foreach (var pa in aliases)
                                        {
                                            if (useFor404 == null)
                                            {
                                                useFor404 = pa; //first one by default
                                            }

                                            //matching?
                                            if (chosen != null && chosen.Count > 0)
                                            {
                                                if (chosen.Contains(pa.HTTPAlias))
                                                {
                                                    useFor404 = pa;
                                                }
                                            }
                                            else
                                            {
                                                break; //no further checking
                                            }
                                        }
                                        //now configure that as the portal settings
                                        if (useFor404 != null)
                                        {
                                            //create portal settings context for identified portal alias in single portal install
                                            ps = new PortalSettings(errTabId, useFor404);
                                        }
                                    }
                                    else
                                    {
                                        reason404 = "Requested domain name is not configured as valid website";
                                        unhandled404 = true;
                                    }
                                }
                            }
                            if (ps != null)
                            {
                                //re-add the context items portal settings back in
                                context.Items.Add("PortalSettings", ps);
                            }
                            if (redirect)
                            {
                                errUrl = Globals.NavigateURL();
                                response.Redirect(errUrl, true); //redirect and end response.  
                                //It will mean the user will have to postback again, but it will work the second time
                            }
                            else
                            {
                                if (transfer)
                                {
                                    //execute a server transfer to the default.aspx?tabid=xx url
                                    //767 : object not set error on extensionless 404 errors
                                    if (context.User == null)
                                    {
                                        context.User = Thread.CurrentPrincipal;
                                    }
                                    response.TrySkipIisCustomErrors = true;
                                    //881 : spoof the basePage object so that the client dependency framework
                                    //is satisfied it's working with a page-based handler
                                    IHttpHandler spoofPage = new CDefault();
                                    context.Handler = spoofPage;
                                    server.Transfer("~/" + errUrl, true);
                                }
                                else
                                {
                                    context.RewritePath("~/Default.aspx", false);
                                    response.TrySkipIisCustomErrors = true;
                                    response.Status = "404 Not Found";
                                    response.StatusCode = 404;
                                }
                            }
                        }
                    }
                    //912 : change to new if statement to handle cases where the TabId404 couldn't be handled correctly
                    if (unhandled404)
                    {
                        //proces the error on the external Url by rewriting to the external url
                        if (!String.IsNullOrEmpty(errUrl))
                        {
                            response.ClearContent();
                            response.TrySkipIisCustomErrors = true;
                            string reason = "Not Found";
                            if (result != null)
                            {
                                reason = result.Reason.ToString();
                            }
                            response.AppendHeader(errRH, string.Format(errRV, "Url", errUrl, reason));
                            if (reason404 != null)
                            {
                                response.AppendHeader("X-Url-Master-404-Data", reason404);
                            }
                            response.StatusCode = statusCode;
                            response.Status = status;
                            server.Transfer("~/" + errUrl, true);
                        }
                        else
                        {
#if (DEBUG)
                            //955: xss vulnerability when outputting raw url back to the page
                            //only do so in debug mode, and sanitize the Url.
                            //sanitize output Url to prevent xss attacks on the default 404 page
                            string requestedUrl = request.Url.ToString();
                            requestedUrl = HttpUtility.HtmlEncode(requestedUrl);
                            errorPageHtml.Write(status + "<br>The requested Url (" + requestedUrl +
                                                ") does not return any valid content.");
#else
                            errorPageHtml.Write(status + "<br>The requested Url does not return any valid content.");
#endif
                            if (reason404 != null)
                            {
                                errorPageHtml.Write(status + "<br>" + reason404);
                            }
                            errorPageHtml.Write("<div style='font-weight:bolder'>Administrators</div>");
                            errorPageHtml.Write("<div>Change this message by configuring a specific 404 Error Page or Url for this website.</div>");

                            //output a reason for the 404
                            string reason = "";
                            if (result != null)
                            {
                                reason = result.Reason.ToString();
                            }
                            if (!string.IsNullOrEmpty(errRH) && !string.IsNullOrEmpty(reason))
                            {
                                response.AppendHeader(errRH, reason);
                            }
                            response.StatusCode = statusCode;
                            response.Status = status;
                        }
                    }
                }
                else
                {
                    //fallback output if not valid settings
                    if (result != null && result.Action == ActionType.Output404)
                    {
                        //don't restate the requested Url to prevent cross site scripting
                        errorPageHtml.Write("404 Not Found<br>The requested Url does not return any valid content.");
                        response.StatusCode = 404;
                        response.Status = "404 Not Found";
                    }
                    else
                    {
                        //error, especially if invalid result object

                        errorPageHtml.Write("500 Server Error<br><div style='font-weight:bolder'>An error occured during processing : if possible, check the event log of the server</div>");
                        response.StatusCode = 500;
                        response.Status = "500 Internal Server Error";
                        if (result != null)
                        {
                            result.Action = ActionType.Output500;
                        }
                    }
                }

                if (ex != null)
                {
                    if (context != null)
                    {
                        if (context.Items.Contains("UrlRewrite:Exception") == false)
                        {
                            context.Items.Add("UrlRewrite:Exception", ex.Message);
                            context.Items.Add("UrlRewrite:StackTrace", ex.StackTrace);
                        }
                    }

                    if (ceSection != null && ceSection.Mode == CustomErrorsMode.Off)
                    {
                        errorPageHtml.Write(errorPageHtmlHeader);
                        errorPageHtml.Write("<div style='font-weight:bolder'>Exception:</div><div>" + ex.Message + "</div>");
                        errorPageHtml.Write("<div style='font-weight:bolder'>Stack Trace:</div><div>" + ex.StackTrace + "</div>");
                        errorPageHtml.Write("<div style='font-weight:bolder'>Administrators</div>");
                        errorPageHtml.Write("<div>You can see this exception because the customErrors attribute in the web.config is set to 'off'.  Change this value to 'on' or 'RemoteOnly' to show Error Handling</div>");
                        try
                        {
                            if (errUrl != null && errUrl.StartsWith("~"))
                            {
                                errUrl = VirtualPathUtility.ToAbsolute(errUrl);
                            }
                        }
                        finally
                        {
                            if (errUrl != null)
                            {
                                errorPageHtml.Write("<div>The error handling would have shown this page : <a href='" + errUrl + "'>" + errUrl + "</a></div>");
                            }
                            else
                            {
                                errorPageHtml.Write("<div>The error handling could not determine the correct page to show.</div>");
                            }
                        }
                    }
                }

                string errorPageHtmlBody = errorPageHtml.ToString();
                if (errorPageHtmlBody.Length > 0)
                {
                    response.Write(errorPageHtmlHeader);
                    response.Write(errorPageHtmlBody);
                    response.Write(errorPageHtmlFooter);
                }
                if (ex != null)
                {
                    UrlRewriterUtils.LogExceptionInRequest(ex, status, result);
                }
            }
        }

        private static bool CheckForDebug(HttpRequest request, NameValueCollection queryStringCol, bool debugEnabled)
        {
            string debugValue = "";
            bool retVal = false;

            if (debugEnabled)
            {
                const string debugToken = "_aumdebug";
                if (queryStringCol != null && queryStringCol[debugToken] != null)
                {
                    debugValue = queryStringCol[debugToken];
                }
                else
                {
                    if (request != null)
                    {
                        debugValue = (request.Params.Get("HTTP_" + debugToken.ToUpper()));
                    }
                    if (debugValue == null)
                    {
                        debugValue = "false";
                    }
                }
            }

            switch (debugValue.ToLower())
            {
                case "true":
                    retVal = true;
                    break;
            }
            return retVal;
        }

        private static bool CheckForTabExternalForwardOrRedirect(HttpContext context, 
                                                                    ref UrlAction result,
                                                                    HttpResponse response, 
                                                                    FriendlyUrlSettings settings,
                                                                    Guid parentTraceId)
        {
            bool finished = false;
            HttpRequest request = null;
            if (context != null)
            {
                request = context.Request;
            }
            //check for external forwarding or a permanent redirect request
            //592 : check for permanent redirect (823 : moved location from 'checkForRedirects')
            if (result.TabId > -1 && result.PortalId > -1 &&
                (settings.ForwardExternalUrlsType != DNNPageForwardType.NoForward ||
                 result.Reason == RedirectReason.Tab_Permanent_Redirect))
            {
                bool allowRedirect = !(result.RewritePath != null && result.RewritePath.ToLower().Contains("&ctl=tab"));
                //594 : do not redirect settings pages for external urls
                if (allowRedirect)
                {
                    TabInfo tab;
                    allowRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, false, out tab, settings);
                    if (allowRedirect)
                    {
                        //772 : not redirecting file type Urls when requested.
                        bool permanentRedirect = false;
                        string redirectUrl = null;
                        string cleanPath = null;
                        bool doRedirect = false;
                        switch (tab.TabType)
                        {
                            case TabType.File:
                                //have to fudge in a portal settings object for this to work - shortcoming of LinkClick URl generation
                                var portalSettings = new PortalSettings(result.TabId, result.PortalAlias);
                                if (context != null)
                                {
                                    context.Items.Add("PortalSettings", portalSettings);
                                    result.Reason = RedirectReason.File_Url;
                                    string fileUrl = Globals.LinkClick(tab.Url, tab.TabID, -1);
                                    context.Items.Remove("PortalSettings");
                                    //take back out again, because it will be done further downstream
                                    //do a check to make sure we're not repeating the Url again, because the tabid is set but we don't want to touch
                                    //a linkclick url
                                    if (!result.OriginalPathNoAlias.EndsWith(HttpUtility.UrlDecode(fileUrl), true, CultureInfo.InvariantCulture))
                                    {
                                        redirectUrl = fileUrl;
                                    }
                                }
                                if (redirectUrl != null)
                                {
                                    doRedirect = true;
                                }
                                break;
                            case TabType.Url:
                                result.Reason = RedirectReason.Tab_External_Url;
                                redirectUrl = tab.Url;
                                if (redirectUrl != null)
                                {
                                    doRedirect = true;
                                    if (settings.ForwardExternalUrlsType == DNNPageForwardType.Redirect301)
                                    {
                                        result.Action = ActionType.Redirect301;
                                        result.Reason = RedirectReason.Tab_External_Url;
                                    }
                                    else if (settings.ForwardExternalUrlsType == DNNPageForwardType.Redirect302)
                                    {
                                        result.Action = ActionType.Redirect302;
                                        result.Reason = RedirectReason.Tab_External_Url;
                                    }
                                }
                                break;
                            case TabType.Tab:
                                // if a tabType.tab is specified, it's either an external url or a permanent redirect

                                //get the redirect path of the specific tab, as long as we have a valid request to work from
                                if (request != null)
                                {
                                    //get the rewrite or requested path in a clean format, suitable for input to the friendly url provider
                                    cleanPath = RewriteController.GetRewriteOrRequestedPath(result, request.Url);
                                    //727 prevent redirectLoop with do301 in querystring
                                    if (result.Action == ActionType.Redirect301 ||
                                        result.Action == ActionType.Redirect302)
                                    {
                                        cleanPath = RedirectTokens.RemoveAnyRedirectTokens(cleanPath,
                                                                                           request.QueryString);
                                    }
                                    //get the redirect Url from the friendly url provider using the tab, path and settings
                                    redirectUrl = RedirectController.GetTabRedirectUrl(tab, settings, cleanPath, result,
                                                                                       out permanentRedirect,
                                                                                       parentTraceId);
                                }
                                //check to make sure there isn't a blank redirect Url
                                if (redirectUrl == null)
                                {
                                    //problem : no redirect Url to redirect to
                                    //solution : cancel the redirect
                                    string message = "Permanent Redirect chosen for Tab " +
                                                     tab.TabPath.Replace("//", "/") +
                                                     " but forwarding Url was not valid";
                                    RedirectController.CancelRedirect(ref result, context, settings, message);
                                }
                                else
                                {
                                    //if there was a redirect Url, set the redirect action and set the type of redirect going to use
                                    doRedirect = true;
                                    if (permanentRedirect)
                                    {
                                        result.Action = ActionType.Redirect301;
                                        result.Reason = RedirectReason.Tab_Permanent_Redirect;
                                            //should be already set, anyway
                                        result.RewritePath = cleanPath;
                                    }
                                    else
                                    {
                                        //not a permanent redirect, check if the page forwarding is set
                                        if (settings.ForwardExternalUrlsType == DNNPageForwardType.Redirect301)
                                        {
                                            result.Action = ActionType.Redirect301;
                                            result.Reason = RedirectReason.Tab_External_Url;
                                        }
                                        else if (settings.ForwardExternalUrlsType == DNNPageForwardType.Redirect302)
                                        {
                                            result.Action = ActionType.Redirect302;
                                            result.Reason = RedirectReason.Tab_External_Url;
                                        }
                                    }
                                }
                                break;
                            default:
                                //only concern here is if permanent redirect is requested, but there is no external url specified
                                if (result.Reason == RedirectReason.Tab_Permanent_Redirect)
                                {
                                    bool permRedirect = tab.PermanentRedirect;
                                    if (permRedirect)
                                    {
                                        //problem : permanent redirect marked, but no forwarding url supplied
                                        //solution : cancel redirect
                                        string message = "Permanent Redirect chosen for Tab " +
                                                         tab.TabPath.Replace("//", "/") +
                                                         " but no forwarding Url Supplied";
                                        RedirectController.CancelRedirect(ref result, context, settings, message);
                                    }
                                }
                                break;
                        }

                        //do the redirect we have specified
                        if (doRedirect &&
                            (result.Action == ActionType.Redirect301 || result.Action == ActionType.Redirect302))
                        {
                            result.FinalUrl = redirectUrl;
                            if (result.Action == ActionType.Redirect301)
                            {
                                if (response != null)
                                {
                                    //perform a 301 redirect to the external url of the tab
                                    response.Status = "301 Moved Permanently";
                                    response.AppendHeader("X-Redirect-Reason",
                                                          result.Reason.ToString().Replace("_", " ") + " Requested");
                                    response.AddHeader("Location", result.FinalUrl);
                                }
                            }
                            else
                            {
                                if (result.Action == ActionType.Redirect302)
                                {
                                    if (response != null)
                                    {
                                        //perform a 301 redirect to the external url of the tab
                                        response.AppendHeader("X-Redirect-Reason",
                                                              result.Reason.ToString().Replace("_", " ") + " Requested");
                                        response.Redirect(result.FinalUrl);
                                    }
                                }
                            }
                            finished = true;
                        }
                    }
                }
            }
            return finished;
        }

        private bool CheckForSecureRedirect(PortalSettings portalSettings, 
                                                Uri requestUri, 
                                                UrlAction result, 
                                                NameValueCollection queryStringCol,
                                                FriendlyUrlSettings settings)
        {
            bool redirectSecure = false;
            string url = requestUri.ToString();
            //889 : don't run secure redirect code for physical resources or requests that aren't a rewritten Url
            if (result.IsPhysicalResource == false && result.TabId >= 0)
                // no secure redirection for physical resources, only tab-specific requests can be redirected for ssl connections
            {
                if (portalSettings.ActiveTab != null)
                {
                    result.DebugMessages.Add("ActiveTab: " + portalSettings.ActiveTab.TabID.ToString() + "/" +
                                             portalSettings.ActiveTab.TabName + " IsSecure: " +
                                             portalSettings.ActiveTab.IsSecure.ToString());

                    //check ssl enabled
                    if (portalSettings.SSLEnabled)
                    {
                        //717 : check page is secure, connection is not secure
                        //952 : support SSl Offloading in DNN 6.2+
                        if (portalSettings.ActiveTab.IsSecure && !result.IsSecureConnection && !result.IsSSLOffloaded)
                        {
                            redirectSecure = true;
                            string stdUrl = portalSettings.STDURL;
                            string sslUrl = portalSettings.SSLURL;
                            if (string.IsNullOrEmpty(result.HttpAlias) == false)
                            {
                                stdUrl = result.HttpAlias;
                            }
                            url = url.Replace("http://", "https://");
                            url = ReplaceDomainName(url, stdUrl, sslUrl);
                        }
                    }
                    //check ssl enforced
                    if (portalSettings.SSLEnforced)
                    {
                        //check page is not secure, connection is secure
                        if (!portalSettings.ActiveTab.IsSecure && result.IsSecureConnection)
                        {
                            //has connection already been forced to secure?
                            if (queryStringCol["ssl"] == null)
                            {
                                //no? well this page shouldn't be secure
                                string stdUrl = portalSettings.STDURL;
                                string sslUrl = portalSettings.SSLURL;
                                url = url.Replace("https://", "http://");
                                url = ReplaceDomainName(url, sslUrl, stdUrl);
                                redirectSecure = true;
                            }
                        }
                    }
                }


                if (redirectSecure)
                {
                    //now check to see if excluded.  Why now? because less requests are made to redirect secure,
                    //so we don't have to check the exclusion as often.
                    bool exclude = false;
                    string doNotRedirectSecureRegex = settings.DoNotRedirectSecureRegex;
                    if (!string.IsNullOrEmpty(doNotRedirectSecureRegex))
                    {
                        //match the raw url
                        exclude = (Regex.IsMatch(result.RawUrl, doNotRedirectSecureRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                    }
                    if (!exclude)
                    {
                        result.Action = ActionType.Redirect302Now;
                        result.Reason = RedirectReason.Secure_Page_Requested;
                        //760 : get the culture specific home page tabid for a redirect comparison
                        int homePageTabId = portalSettings.HomeTabId;
                        homePageTabId = TabPathHelper.GetHomePageTabIdForCulture(portalSettings.DefaultLanguage,
                                                                                 portalSettings.PortalId,
                                                                                 result.CultureCode, homePageTabId);
                        if (result.TabId == homePageTabId)
                        {
                            //replace the /default.aspx in the Url if it was found
                            url = Regex.Replace(url, @"(?<!(\?.+))/default.aspx", "/", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        }
                        result.FinalUrl = url;
                    }
                    else
                    {
                        //702 : change return value if exclusion has occured
                        redirectSecure = false;
                    }
                }
            }
            return redirectSecure;
        }

        private string ReplaceDomainName(string url, string replaceDomain, string withDomain)
        {
            if (replaceDomain != "" && withDomain != "")
            {
                //951 : change find/replace routine to regex for more accurate replacement 
                //(previous method gives false positives if the SSL Url is contained within the STD url)
                string find = @"(?<=https?://)" + Regex.Escape(withDomain);
                if (Regex.IsMatch(url, find, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) == false)
                {
                    string replaceFind = @"(?<=https?://)" + Regex.Escape(replaceDomain);
                    url = Regex.Replace(url, replaceFind, withDomain, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
            }
            return url;
        }

        private bool IsSSLOffloadEnabled(HttpRequest request)
        {
            var ssloffloadheader = HostController.Instance.GetString("SSLOffloadHeader");
            //if the ssloffloadheader variable has been set check to see if a request header with that type exists
            if (!string.IsNullOrEmpty(ssloffloadheader) && request != null)
            {
                string ssloffload = request.Headers[ssloffloadheader];
                if (!string.IsNullOrEmpty(ssloffload))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        protected bool IsPortalAliasIncorrect(HttpContext context, 
                                                    HttpRequest request, 
                                                    Uri requestUri,
                                                    UrlAction result, 
                                                    NameValueCollection queryStringCol,
                                                    FriendlyUrlSettings settings, 
                                                    Guid parentTraceId,
                                                    out string httpAlias)
        {
            //now check to make sure it's the primary portal alias for this portal/language/browser
            bool incorrectAlias = false;
            httpAlias = null;
            //if (result.RedirectAllowed && result.PortalId > -1)
            if (result.PortalId > -1) //portal has been identified
            {
                var portalAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();

                if (queryStringCol != null && queryStringCol["forceAlias"] != "true")
                {
                    if (portalAliases.Count > 0)
                    {
                        string checkAlias = result.HttpAlias;
                        bool continueLoop = true;
                        bool triedWWW = false;
                        while (httpAlias == null && continueLoop)
                        {
                            if (portalAliases.ContainsAlias(result.PortalId, checkAlias))
                            {
                                if (portalAliases.Count > 0)
                                {
                                    var cpa = portalAliases.GetAliasByPortalIdAndSettings(result);
                                    if (cpa != null)
                                    {
                                        httpAlias = cpa.HTTPAlias;
                                        continueLoop = false;
                                    }
                                    if (String.IsNullOrEmpty(result.CultureCode) && cpa == null)
                                    {
                                        //if there is a specific culture for this portal alias, then check that
                                        string culture = portalAliases.GetCultureByPortalIdAndAlias(result.PortalId, result.HttpAlias);
                                        //if this matches the alias of the request, then we know we have the correct alias because it is a specific culture
                                        if (!string.IsNullOrEmpty(culture))
                                        {
                                            continueLoop = false;
                                        }
                                    }
                                }
                            }
                            //check whether to still go on or not
                            if (continueLoop)
                            {
                                //this alias doesn't exist in the list
                                //check if it has a www on it - if not, try adding, if it does, try removing
                                if (!triedWWW)
                                {
                                    triedWWW = true; //now tried adding/removing www
                                    if (checkAlias.ToLower().StartsWith("www."))
                                    {
                                        checkAlias = checkAlias.Substring(4);
                                    }
                                    else
                                    {
                                        checkAlias = "www." + checkAlias;
                                    }
                                }
                                else
                                {
                                    //last thing to try, get the default language and see if there is a portal alias for that
                                    //thus, any aliases not identified as belonging to a language are redirected back to the 
                                    //alias named for the default language
                                    continueLoop = false;
                                    //735 : switch to custom method for getting portal
                                    PortalInfo pi = CacheController.GetPortal(result.PortalId, false);
                                    if (pi != null)
                                    {
                                        string cultureCode = pi.DefaultLanguage;
                                        if (!string.IsNullOrEmpty(cultureCode))
                                        {
                                            var primaryPortalAlias = portalAliases.GetAliasByPortalIdAndSettings(result.PortalId, result,cultureCode,settings);
                                            if (primaryPortalAlias != null)
                                            {
                                                httpAlias = primaryPortalAlias.HTTPAlias;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    
                    //check to see if it is a custom tab alais - in that case, it is allowed to be requested for the tab
                    if (CheckIfAliasIsCustomTabAlias(ref result, httpAlias, settings))
                    {
                        //change the primary alias to the custom tab alias that has been requested.
                        result.PrimaryAlias = result.PortalAlias;
                    }
                    else
                        if (httpAlias != null && String.Compare(httpAlias, result.HttpAlias, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            incorrectAlias = true;
                        }
                }
            }
            return incorrectAlias;
        }
        /// <summary>
        /// Redirects an alias if that is allowed by the settings
        /// </summary>
        /// <param name="httpAlias"></param>
        /// <param name="result"></param>
        /// <param name="settings"></param>
        /// <returns></returns>

        private static bool RedirectPortalAlias(string httpAlias, ref UrlAction result, FriendlyUrlSettings settings)
        {
            bool redirected = false;
            //redirect to primary alias
            if (result.PortalAliasMapping == PortalSettings.PortalAliasMapping.Redirect && result.RedirectAllowed)
            {
                if (result.Reason == RedirectReason.Wrong_Portal_Alias_For_Browser_Type || result.Reason == RedirectReason.Wrong_Portal_Alias_For_Culture ||
                    result.Reason == RedirectReason.Wrong_Portal_Alias_For_Culture_And_Browser)
                {
                    redirected = ConfigurePortalAliasRedirect(ref result, result.HttpAlias, httpAlias, false, result.Reason, settings.InternalAliasList, settings);
                }
                else
                {
                    redirected = ConfigurePortalAliasRedirect(ref result, result.HttpAlias, httpAlias, false, settings.InternalAliasList, settings);
                }
            }

            return redirected;
        }

        private static bool ConfigurePortalAliasRedirect(ref UrlAction result, 
                                                            string wrongAlias, 
                                                            string rightAlias,
                                                            bool ignoreCustomAliasTabs, 
                                                            List<InternalAlias> internalAliases,
                                                            FriendlyUrlSettings settings)
        {
            return ConfigurePortalAliasRedirect(ref result, 
                                                    wrongAlias, 
                                                    rightAlias, 
                                                    ignoreCustomAliasTabs,
                                                    RedirectReason.Wrong_Portal_Alias, 
                                                    internalAliases, 
                                                    settings);
        }
        /// <summary>
        /// Checks to see whether the specified alias is a customTabAlias
        /// </summary>
        /// <param name="result"></param>
        /// <param name="httpAlias"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static bool CheckIfAliasIsCustomTabAlias(ref UrlAction result, string httpAlias, FriendlyUrlSettings settings)
        {
            List<string> customAliasesForTabs = TabIndexController.GetCustomPortalAliases(settings);
            bool isACustomTabAlias = false;
            if (customAliasesForTabs != null && customAliasesForTabs.Count > 0)
            {
                //remove any customAliases that are also primary aliases.
                foreach (var cpa in TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId))
                {
                    if (cpa.IsPrimary == true && customAliasesForTabs.Contains(cpa.HTTPAlias))
                    {
                        customAliasesForTabs.Remove(cpa.HTTPAlias);
                    }
                }
                isACustomTabAlias = customAliasesForTabs.Contains(httpAlias.ToLower());
            }
            return isACustomTabAlias;
        }
        /// <summary>
        /// Configures the result object to set the correct Alias redirect
        /// parameters and destination URL
        /// </summary>
        /// <param name="result"></param>
        /// <param name="wrongAlias"></param>
        /// <param name="rightAlias"></param>
        /// <param name="ignoreCustomAliasTabs"></param>
        /// <param name="redirectReason"></param>
        /// <param name="internalAliases"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static bool ConfigurePortalAliasRedirect(ref UrlAction result, 
                                                            string wrongAlias, 
                                                            string rightAlias,
                                                            bool ignoreCustomAliasTabs, 
                                                            RedirectReason redirectReason,
                                                            List<InternalAlias> internalAliases,
                                                            FriendlyUrlSettings settings)
        {
            //wrong alias for the portal
            //check to see if the wrong portal alias could be a custom alias for a tab
            bool doRedirect;
            if (ignoreCustomAliasTabs == false) //check out custom alias tabs collection
            {
                //if an alias is a custom tab alias for a specific tab, then don't redirect
                if (CheckIfAliasIsCustomTabAlias(ref result, wrongAlias, settings)) 
                    doRedirect = false;
                else
                {
                    doRedirect = true;
                }
            }
            else
            {
                doRedirect = true; //do redirect, ignore custom alias entries for tabs
            }
            //check to see if it is an internal alias.  These are used to block redirects
            //to allow for reverse proxy requests, which must change the rewritten alias
            //while leaving the requested alias
            bool internalAliasFound = false;
            if (doRedirect && internalAliases != null && internalAliases.Count > 0)
            {
                if (internalAliases.Any(ia => String.Compare(ia.HttpAlias, wrongAlias, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    internalAliasFound = true;
                    doRedirect = false;
                }
            }
            //if still need to do redirect, then set the settings that will cause the redirect (redirect not done here)
            if (doRedirect)
            {
                result.Action = ActionType.Redirect301;
                result.Reason = redirectReason;
                string destUrl = result.OriginalPath.Replace(wrongAlias, rightAlias);
                if (redirectReason == RedirectReason.Wrong_Portal_Alias_For_Culture ||
                    redirectReason == RedirectReason.Wrong_Portal_Alias_For_Culture_And_Browser)
                {
                    destUrl = destUrl.Replace("/language/" + result.CultureCode, "");
                }
                destUrl = CheckForSiteRootRedirect(rightAlias, destUrl);
                result.FinalUrl = destUrl;
            }
            else
            {
                //838 : don't overwrite the reason if already have checkfor301
                // and don't do a check on the basis that an internal alias was found
                if (result.Action != ActionType.CheckFor301 && internalAliasFound == false)
                {
                    //set status to 'check for redirect'
                    result.Action = ActionType.CheckFor301;
                    result.Reason = RedirectReason.Custom_Tab_Alias;
                }
            }
            return doRedirect;
        }

        private void IdentifyPortalAlias(HttpContext context, 
                                            HttpRequest request, 
                                            Uri requestUri, UrlAction result,
                                            NameValueCollection queryStringCol, 
                                            FriendlyUrlSettings settings,
                                            Guid parentTraceId)
        {
            //get the domain name of the request, if it isn't already supplied
            if (request != null && string.IsNullOrEmpty(result.DomainName))
            {
                result.DomainName = Globals.GetDomainName(request); //parse the domain name out of the request
            }

            // get tabId from querystring ( this is mandatory for maintaining portal context for child portals ) 
            if (queryStringCol["tabid"] != null)
            {
                string raw = queryStringCol["tabid"];
                int tabId;
                if (Int32.TryParse(raw, out tabId))
                {
                    result.TabId = tabId;
                }
                else
                {
                    //couldn't parse tab id
                    //split in two?
                    string[] tabids = raw.Split(',');
                    if (tabids.GetUpperBound(0) > 0)
                    {
                        //hmm more than one tabid
                        if (Int32.TryParse(tabids[0], out tabId))
                        {
                            result.TabId = tabId;
                            //but we want to warn against this!
                            var ex =
                                new Exception(
                                    "Illegal request exception : Two TabId parameters provided in a single request: " +
                                    requestUri);
                            UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", result);

                            result.Ex = ex;
                        }
                        else
                        {
                            //yeah, nothing, divert to 404 
                            result.Action = ActionType.Output404;
                            var ex =
                                new Exception(
                                    "Illegal request exception : TabId parameters in query string, but invalid TabId requested : " +
                                    requestUri);
                            UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", result);
                            result.Ex = ex;
                        }
                    }
                }
            }
            // get PortalId from querystring ( this is used for host menu options as well as child portal navigation ) 
            if (queryStringCol["portalid"] != null)
            {
                string raw = queryStringCol["portalid"];
                int portalId;
                if (Int32.TryParse(raw, out portalId))
                {
                    //848 : if portal already found is different to portal id in querystring, then load up different alias
                    //this is so the portal settings will be loaded correctly.
                    if (result.PortalId != portalId)
                    {
                        //portal id different to what we expected
                        result.PortalId = portalId;
                        //check the loaded portal alias, because it might be wrong
                        if (result.PortalAlias != null && result.PortalAlias.PortalID != portalId)
                        {
                            //yes, the identified portal alias is wrong.  Find the correct alias for this portal
                            PortalAliasInfo pa = TabIndexController.GetPortalAliasByPortal(portalId, result.DomainName);
                            if (pa != null)
                            {
                                //note: sets portal id and portal alias
                                result.PortalAlias = pa;
                            }
                        }
                    }
                }
            }
            else
            {
                //check for a portal alias if there's no portal Id in the query string
                //check for absence of captcha value, because the captcha string re-uses the alias querystring value
                if (queryStringCol["alias"] != null && queryStringCol["captcha"] == null)
                {
                    string alias = queryStringCol["alias"];
                    PortalAliasInfo portalAlias = PortalAliasController.GetPortalAliasInfo(alias);
                    if (portalAlias != null)
                    {
                        //ok the portal alias was found by the alias name
                        // check if the alias contains the domain name
                        if (alias.Contains(result.DomainName) == false)
                        {
                            // replaced to the domain defined in the alias 
                            if (request != null)
                            {
                                string redirectDomain = Globals.GetPortalDomainName(alias, request, true);
                                //retVal.Url = redirectDomain;
                                result.FinalUrl = redirectDomain;
                                result.Action = ActionType.Redirect302Now;
                                result.Reason = RedirectReason.Alias_In_Url;
                            }
                        }
                        else
                        {
                            // the alias is the same as the current domain 
                            result.HttpAlias = portalAlias.HTTPAlias;
                            result.PortalAlias = portalAlias;
                            result.PortalId = portalAlias.PortalID;
                            //don't use this crap though - we don't want ?alias=portalAlias in our Url
                            if (result.RedirectAllowed)
                            {
                                string redirect = requestUri.Scheme + Uri.SchemeDelimiter + result.PortalAlias.HTTPAlias +
                                                  "/";
                                result.Action = ActionType.Redirect301;
                                result.FinalUrl = redirect;
                                result.Reason = RedirectReason.Unfriendly_Url_Child_Portal;
                            }
                        }
                    }
                }
            }
            //first try and identify the portal using the tabId, but only if we identified this tab by looking up the tabid
            //from the original url
            //668 : error in child portal redirects to child portal home page because of mismatch in tab/domain name
            if (result.TabId != -1 && result.FriendlyRewrite == false)
            {
                // get the alias from the tabid, but only if it is for a tab in that domain 
                // 2.0 : change to compare retrieved alias to the already-set httpAlias
                string httpAliasFromTab = PortalAliasController.GetPortalAliasByTab(result.TabId, result.DomainName);
                if (httpAliasFromTab != null)
                {
                    //882 : account for situation when portalAlias is null.
                    if (result.PortalAlias != null && String.Compare(result.PortalAlias.HTTPAlias, httpAliasFromTab, StringComparison.OrdinalIgnoreCase) != 0
                        || result.PortalAlias == null)
                    {
                        //691 : change logic to force change in portal alias context rather than force back.
                        //This is because the tabid in the query string should take precedence over the portal alias
                        //to handle parent.com/default.aspx?tabid=xx where xx lives in parent.com/child/ 
                        var tc = new TabController();
                        var tab = tc.GetTab(result.TabId, Null.NullInteger, false);
                        //when result alias is null or result alias is different from tab-identified portalAlias
                        if (tab != null && (result.PortalAlias == null || tab.PortalID != result.PortalAlias.PortalID))
                        {
                            //the tabid is different to the identified portalid from the original alias identified
                            //so get a new alias 
                            var pac = new PortalAliasController();
                            PortalAliasInfo tabPortalAlias = pac.GetPortalAlias(httpAliasFromTab, tab.PortalID);
                            if (tabPortalAlias != null)
                            {
                                result.PortalId = tabPortalAlias.PortalID;
                                result.PortalAlias = tabPortalAlias;
                                result.Action = ActionType.CheckFor301;
                                result.Reason = RedirectReason.Wrong_Portal;
                            }
                        }
                    }
                }
            }
            //if no alias, try and set by using the identified http alias or domain name
            if (result.PortalAlias == null)
            {
                if (!string.IsNullOrEmpty(result.HttpAlias))
                {
                    result.PortalAlias = PortalAliasController.GetPortalAliasInfo(result.HttpAlias);
                }
                else
                {
                    result.PortalAlias = PortalAliasController.GetPortalAliasInfo(result.DomainName);
                    if (result.PortalAlias == null && result.DomainName.EndsWith("/"))
                    {
                        result.DomainName = result.DomainName.TrimEnd('/');
                        result.PortalAlias = PortalAliasController.GetPortalAliasInfo(result.DomainName);
                    }
                }
            }

            if (result.PortalId == -1)
            {
                if (!requestUri.LocalPath.ToLower().EndsWith(Globals.glbDefaultPage.ToLower()))
                {
                    // allows requests for aspx pages in custom folder locations to be processed 
                    return;
                }
                //the domain name was not found so try using the host portal's first alias
                if (Host.Host.HostPortalID != -1)
                {
                    result.PortalId = Host.Host.HostPortalID;
                    // use the host portal, but replaced to the host portal home page
                    var aliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
                    if (aliases.Count > 0)
                    {
                        string alias = null;

                        //get the alias as the chosen portal alias for the host portal based on the result culture code
                        var cpa = aliases.GetAliasByPortalIdAndSettings(result.PortalId, result, result.CultureCode, settings);
                        if (cpa != null)
                        {
                            alias = cpa.HTTPAlias;
                        }

                        if (alias != null)
                        {
                            result.Action = ActionType.Redirect301;
                            result.Reason = RedirectReason.Host_Portal_Used;
                            string destUrl = MakeUrlWithAlias(requestUri, alias);
                            destUrl = CheckForSiteRootRedirect(alias, destUrl);
                            result.FinalUrl = destUrl;
                        }
                        else
                        {
                            //Get the first Alias for the host portal
                            result.PortalAlias = aliases[result.PortalId];
                            string url = MakeUrlWithAlias(requestUri, result.PortalAlias);
                            if (result.TabId != -1)
                            {
                                url += requestUri.Query;
                            }
                            result.FinalUrl = url;
                            result.Reason = RedirectReason.Host_Portal_Used;
                            result.Action = ActionType.Redirect302Now;
                        }
                    }
                }
            }

            //double check to make sure we still have the correct alias now that all other information is known (ie tab, portal, culture)
            //770 : redirect alias based on tab id when custom alias used
            if (result.TabId == -1 && result.Action == ActionType.CheckFor301 &&
                result.Reason == RedirectReason.Custom_Tab_Alias)
            {
                //here because the portal alias matched, but no tab was found, and because there are custom tab aliases used for this portal
                //need to redirect back to the chosen portal alias and keep the current path.
                string wrongAlias = result.HttpAlias; //it's a valid alias, but only for certain tabs
                var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
                if (primaryAliases != null && result.PortalId > -1)
                {
                    //going to look for the correct alias based on the culture of the request
                    string requestCultureCode = result.CultureCode;
                    //if that didn't work use the default language of the portal
                    if (requestCultureCode == null)
                    {
                        //this might end up in a double redirect if the path of the Url is for a specific language as opposed
                        //to a path belonging to the default language domain
                        var pc = new PortalController();
                        PortalInfo portal = pc.GetPortal(result.PortalId);
                        if (portal != null)
                        {
                            requestCultureCode = portal.DefaultLanguage;
                        }
                    }
                    //now that the culture code is known, look up the correct portal alias for this portalid/culture code
                    var cpa = primaryAliases.GetAliasByPortalIdAndSettings(result.PortalId, result, requestCultureCode, settings);
                    if (cpa != null)
                    {
                        //if an alias was found that matches the request and the culture code, then run with that
                        string rightAlias = cpa.HTTPAlias;
                        //will cause a redirect to the primary portal alias - we know now that there was no custom alias tab
                        //found, so it's just a plain wrong alias
                        ConfigurePortalAliasRedirect(ref result, wrongAlias, rightAlias, true,
                                                     settings.InternalAliasList, settings);
                    }
                }
            }
            else
            {
                //then check to make sure it's the chosen portal alias for this portal
                //627 : don't do it if we're redirecting to the host portal 
                if (result.RedirectAllowed && result.Reason != RedirectReason.Host_Portal_Used)
                {
                    string primaryAlias;
                    //checking again in case the rewriting operation changed the values for the valid portal alias
                    bool incorrectAlias = IsPortalAliasIncorrect(context, request, requestUri, result, queryStringCol, settings, parentTraceId, out primaryAlias);
                    if (incorrectAlias) RedirectPortalAlias(primaryAlias, ref result, settings);
                }
            }

            //check to see if we have to avoid the core 302 redirect for the portal alias that is in the /defualt.aspx
            //for child portals
            //exception to that is when a custom alias is used but no rewrite has taken place
            if (result.DoRewrite == false && (result.Action == ActionType.Continue
                                              ||
                                              (result.Action == ActionType.CheckFor301 &&
                                               result.Reason == RedirectReason.Custom_Tab_Alias)))
            {
                string aliasQuerystring;
                bool isChildAliasRootUrl = CheckForChildPortalRootUrl(requestUri.AbsoluteUri, result, out aliasQuerystring);
                if (isChildAliasRootUrl)
                {
                    RewriteAsChildAliasRoot(context, result, aliasQuerystring, settings);
                }
            }
        }

        internal static void RewriteAsChildAliasRoot(HttpContext context, 
                                                        UrlAction result, 
                                                        string aliasQueryString,
                                                        FriendlyUrlSettings settings)
        {
            string culture = null;
            //look for specific alias to rewrite language parameter
            var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
            if (result.PortalId > -1 && result.HttpAlias != null)
            {
                culture = primaryAliases.GetCultureByPortalIdAndAlias(result.PortalId, result.HttpAlias);
            }
            if (string.IsNullOrEmpty(culture))
                //732 : when no culture returned can be "" as well as null : no culture causes no rewrite, which results in redirect to parent alias
            {
                //set the default culture code here
                //735 : switch to custom method for getting portal
                PortalInfo pi = CacheController.GetPortal(result.PortalId, false);
                if (pi != null)
                {
                    culture = pi.DefaultLanguage;
                }
            }
            if (!string.IsNullOrEmpty(culture)) //a culture was identified for the alias root
            {
                if (RewriteController.AddLanguageCodeToRewritePath(ref aliasQueryString, culture))
                {
                    result.CultureCode = culture;
                }
                result.DoRewrite = true;
                result.RewritePath = "~/" + Globals.glbDefaultPage + aliasQueryString;
                //the expected /default.aspx path (defaultPageUrl) matches the requested Url (/default.aspx)
                if (context != null)
                {
                    //only do if not testing
                    RewriterUtils.RewriteUrl(context, result.RewritePath);
                }
            }
        }

        internal static bool CheckForChildPortalRootUrl(string requestUrl, UrlAction result, out string aliasQueryString)
        {
            bool isChildPortalRootUrl = false;
            //what we are going to test for here is that if this is a child portal request, for the /default.aspx of the child portal
            //then we are going to avoid the core 302 redirect to ?alias=portalALias by rewriting to the /default.aspx of the site root
            //684 : don't convert querystring items to lower case
            //do the check by constructing what a child alias url would look like and compare it with the requested urls
            //912 : when requested without a valid portal alias, portalALias is null.  Refuse and return false.
            aliasQueryString = null;
            if (result.PortalAlias != null && result.PortalAlias.HTTPAlias != null)
            {
                string defaultPageUrl = result.Scheme + result.PortalAlias.HTTPAlias + "/" +
                                        Globals.glbDefaultPage.ToLower(); //child alias Url with /default.aspx
                //660 : look for a querystring on the site root for a child portal, and handle it if so
                if (String.CompareOrdinal(requestUrl.ToLower(), defaultPageUrl) == 0)
                {
                    //exact match : that's the alias root
                    isChildPortalRootUrl = true;
                    aliasQueryString = "";
                }
                if (!isChildPortalRootUrl && requestUrl.Contains("?"))
                {
                    //is we didn't get an exact match but there is a querystring, then investigate
                    string[] requestUrlParts = requestUrl.Split('?');
                    if (requestUrlParts.GetUpperBound(0) > 0)
                    {
                        string rootPart = requestUrlParts[0];
                        string queryString = requestUrlParts[1];
                        if (String.Compare(rootPart, defaultPageUrl, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            //rewrite, but put in the querystring on the rewrite path
                            isChildPortalRootUrl = true;
                            aliasQueryString = "?" + queryString;
                            //674: check for 301 if this value is a tabid/xx - otherwise the url will just evaluate as is
                            if (queryString.ToLower().StartsWith("tabid="))
                            {
                                result.Action = ActionType.CheckFor301;
                            }
                        }
                    }
                }
            }
            return isChildPortalRootUrl;
        }

        private static string MakeUrlWithAlias(Uri requestUri, string httpAlias)
        {
            return requestUri.AbsoluteUri.ToLower().StartsWith("https://")
                             ? "https://" + httpAlias.Replace("*.", "") + "/"
                             : "http://" + httpAlias.Replace("*.", "") + "/";
        }

        private static string MakeUrlWithAlias(Uri requestUri, PortalAliasInfo alias)
        {
            return MakeUrlWithAlias(requestUri, alias.HTTPAlias);
        }

        /// <summary>
        /// Determines if this is a request from an install / upgrade url
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <param name="refererPath"></param>
        /// <param name="requestedDomain"></param>
        /// <param name="refererDomain"></param>
        /// <returns></returns>
        /// <remarks>
        /// //875 : cater for the upgradewizard.aspx Url that is new to DNN 6.1
        /// </remarks>
        private static bool IgnoreRequestForInstall(string physicalPath, string refererPath, string requestedDomain, string refererDomain)
        {
            if (physicalPath.EndsWith("install.aspx", true, CultureInfo.InvariantCulture)
                || physicalPath.EndsWith("installwizard.aspx", true, CultureInfo.InvariantCulture)
                || physicalPath.EndsWith("upgradewizard.aspx", true, CultureInfo.InvariantCulture)
                || Globals.Status == Globals.UpgradeStatus.Install
                || Globals.Status == Globals.UpgradeStatus.Upgrade)
                return true;
            //954 : DNN 7.0 compatibility
            //check for /default.aspx which is default Url launched from the Upgrade/Install wizard page
            //961 : check domain as well as path for the referer
            if (physicalPath.EndsWith(Globals.glbDefaultPage, true, CultureInfo.InvariantCulture) == false
                && refererPath != null
                && String.Compare(requestedDomain, refererDomain, StringComparison.OrdinalIgnoreCase) == 0
                && (refererPath.EndsWith("install.aspx", true, CultureInfo.InvariantCulture)
                    || refererPath.EndsWith("installwizard.aspx", true, CultureInfo.InvariantCulture)
                    || refererPath.EndsWith("upgradewizard.aspx", true, CultureInfo.InvariantCulture)
                   ))
            {
                return true;
            }
            return false;
        }

        private static bool IgnoreRequestForInstall(HttpRequest request)
        {
            try
            {
                //string physicalPath = request.PhysicalPath;
                //return IgnoreRequestForInstall(physicalPath);
                string physicalPath = request.PhysicalPath;
                string requestedDomain = request.Url.Host;
                string refererPath = null, refererDomain = null;
                if (request.UrlReferrer != null)
                {
                    refererDomain = request.UrlReferrer.Host;
                    refererPath = request.UrlReferrer.LocalPath;
                }
                return IgnoreRequestForInstall(physicalPath, refererPath, requestedDomain, refererDomain);
            }
            catch (PathTooLongException)
            {
                //catch and handle this exception, caused by an excessively long file path based on the
                //mapped virtual url
                return false;
            }
        }

        private static bool IgnoreRequest(UrlAction result, string requestedPath, string ignoreRegex, HttpRequest request)
        {
            bool retVal = false;
            //check if we are upgrading/installing 
            //829 : use result physical path instead of requset physical path
            //875 : cater for the upgradewizard.aspx Url that is new to DNN 6.1
            if (request != null && (IgnoreRequestForInstall(request)))
            {
                //ignore all install requests
                retVal = true;
            }
            else if (request != null && request.Path.EndsWith("SetGettingStartedPageAsShown"))
            {
                //ignore request to the Getting Started Web Method
                retVal = true;
            }
            else
            {
                try
                {
                    if ((ignoreRegex.Length > 0))
                    {
                        if (Regex.IsMatch(requestedPath, ignoreRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                        {
                            retVal = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", result);
                    result.Ex = ex;
                }
            }
            return retVal;
        }

        private static void CheckForRewrite(string fullUrl, 
                                                string querystring, 
                                                UrlAction result, 
                                                bool useFriendlyUrls,
                                                NameValueCollection queryStringCol, 
                                                FriendlyUrlSettings settings,
                                                out bool isPhysicalResource, 
                                                Guid parentTraceId)
        {
            bool checkForRewrites;
            //just check to make sure it isn't a physical resource on the server
            RewriteController.IdentifyByPhysicalResource(result.PhysicalPath, 
                                                            fullUrl, 
                                                            queryStringCol, 
                                                            ref result,
                                                            useFriendlyUrls, 
                                                            settings, 
                                                            out isPhysicalResource,
                                                            out checkForRewrites, 
                                                            parentTraceId);

            if (checkForRewrites && RewriteController.CanRewriteRequest(result, fullUrl, settings))
            {
                bool doSiteUrlProcessing = false;
                //728 new regex expression to pass values straight onto the siteurls.config file
                if (!string.IsNullOrEmpty(settings.UseSiteUrlsRegex))
                {
                    doSiteUrlProcessing = Regex.IsMatch(fullUrl, settings.UseSiteUrlsRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }

                if (!doSiteUrlProcessing)
                    //if a virtual request, and not starting with the siteUrls.config file, go on to find the rewritten path
                {
                    //looks up the page index to find the correct Url
                    bool doRewrite = RewriteController.IdentifyByTabPathEx(fullUrl, querystring, result, queryStringCol, settings, parentTraceId);
                    if (!doRewrite)
                    {
                        doSiteUrlProcessing = true;
                    }
                }
                if (doSiteUrlProcessing)
                {
                    //728 : compare requests against the siteurls.config file, either if no other match was found, or if we want to skip the rest of the processing
                    // the standard DNN way of rewriting, using expressions found in the siteurls.config file
                    RewriteController.IdentifyByRegEx(fullUrl, querystring, result.ApplicationPath, ref result, settings, parentTraceId);
                }
            }
        }

        private void SecurityCheck(HttpApplication app)
        {
            HttpRequest request = app.Request;
            HttpServerUtility server = app.Server;

            //675 : unnecessarily strict url validation
            // URL validation 
            // check for ".." escape characters commonly used by hackers to traverse the folder tree on the server 
            // the application should always use the exact relative location of the resource it is requesting 
            string strURL = request.Url.AbsolutePath;
            string strDoubleDecodeURL = server.UrlDecode(server.UrlDecode(request.RawUrl));
            if (Regex.Match(strURL, "[\\\\/]\\.\\.[\\\\/]").Success |
                Regex.Match(strDoubleDecodeURL, "[\\\\/]\\.\\.[\\\\/]").Success)
            {
                throw new HttpException(404, "Not Found");
            }
        }

        private static bool CheckForRedirects(Uri requestUri, 
                                                string fullUrl, 
                                                NameValueCollection queryStringCol,
                                                UrlAction result, 
                                                string requestType, 
                                                FriendlyUrlSettings settings,
                                                int portalHomeTabId)
        {
            bool redirected = false;
            if (queryStringCol["error"] == null && queryStringCol["message"] == null && requestType != "POST")
            {
                //if the / is missing from an extension-less request, then check for a 301 redirect
                if (settings.PageExtensionUsageType == PageExtensionUsageType.Never)
                {
                    //575 check on absolutePath instead of absoluteUri : this ignores query strings and fragments like #
                    //610 don't always end with '/' - reverses previous setting
                    //687 don't double-check 301 redirects.  'CheckFor301' is less concise than 'Redirect301'
                    if (requestUri.AbsolutePath.EndsWith("/") && result.Action != ActionType.Redirect301)
                    {
                        result.Action = ActionType.CheckFor301;
                    }
                }
                if (settings.RedirectWrongCase && result.Action == ActionType.Continue)
                {
                    result.Action = ActionType.CheckFor301;
                }
                string scheme = requestUri.Scheme + Uri.SchemeDelimiter;
                bool queryStringHas301Parm = (queryStringCol["do301"] != null);
                //727 : keep a bool value if there is a do301 request in the querystring
                //check for a 301 request in the query string, or an explicit 301 or 302 request
                //2.0 - check for explicit do301=true instead of just do301 key
                string do301Val = queryStringCol["do301"];
                if (result.TabId > -1 //valid tab
                    && (result.Action == ActionType.Redirect301 //specific 301 redirect
                        || (do301Val != null && do301Val == "true") //or rewrite hint for specific 301 redirect 
                        || result.Action == ActionType.Redirect302)) //or specific 302 redirect
                {
                    //we have ordered a 301 redirect earlier in the code
                    var tc = new TabController();

                    //get the url for redirection by re-submitting the path into the Friendly Url Provider
                    string pathOnly = RewriteController.GetRewriteOrRequestedPath(result, requestUri);
                    //727 prevent redirectLoop with do301 in querystring
                    if (result.Action == ActionType.Redirect301 || queryStringHas301Parm || result.Action == ActionType.Redirect302)
                    {
                        pathOnly = RedirectTokens.RemoveAnyRedirectTokens(pathOnly, queryStringCol);
                    }

                    //check for exclusion by regex for this url
                    if (result.RedirectAllowed)
                    {
                        //get the tab so we know where to go
                        TabInfo tab;
                        bool checkRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, true, out tab, settings);

                        if (checkRedirect)
                        {
                            if ((result.Reason == RedirectReason.Deleted_Page || result.Reason == RedirectReason.Disabled_Page)
                                && portalHomeTabId > 0
                                && settings.DeletedTabHandlingType == DeletedTabHandlingType.Do301RedirectToPortalHome)
                            {
                                //redirecting to home page  
                                TabInfo homeTab = tc.GetTab(portalHomeTabId, result.PortalId, false);
                                if (homeTab != null)
                                {
                                    string homePageUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(homeTab, 
                                                                                                         pathOnly,
                                                                                                         Globals.glbDefaultPage,
                                                                                                         result.HttpAlias,
                                                                                                         false, 
                                                                                                         settings,
                                                                                                         Guid.Empty);
                                    result.Action = ActionType.Redirect301;
                                    result.FinalUrl = homePageUrl;
                                    result.RewritePath = pathOnly;
                                    redirected = true;
                                }
                            }
                            else
                            {
                                //get the rewrite or requested path in a clean format, suitable for input to the friendly url provider
                                string cleanPath = RewriteController.GetRewriteOrRequestedPath(result, requestUri);

                                //727 prevent redirectLoop with do301 in querystring
                                //also check for existing in path of do301 token
                                if (result.Action == ActionType.Redirect301 || do301Val != null || result.Action == ActionType.Redirect302)
                                {
                                    cleanPath = RedirectTokens.RemoveAnyRedirectTokens(cleanPath, queryStringCol);
                                }

                                //get best friendly url from friendly url provider
                                string bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab, 
                                                                                                         cleanPath,
                                                                                                         Globals.glbDefaultPage,
                                                                                                         result.HttpAlias,
                                                                                                         false, 
                                                                                                         settings,
                                                                                                         Guid.Empty);

                                //get what the friendly Url for this tab should be and stick it in as the redirect
                                //727 : using boolean because we wanted to get rid of the do301 before calculating the correct url
                                if (queryStringHas301Parm)
                                {
                                    result.Action = ActionType.Redirect301;
                                    if (result.Reason == RedirectReason.Not_Redirected)
                                    {
                                        result.Reason = RedirectReason.Unfriendly_Url_1;
                                    }
                                }
                                result.FinalUrl = bestFriendlyUrl;
                                result.RewritePath = pathOnly;
                                redirected = true; //mark as redirected
                            }
                        }
                        else
                        {
                            //redirect disallowed 
                            //618: dont' clear if 302 redirect selected
                            if (result.Action != ActionType.Redirect302Now || result.Action != ActionType.Redirect302)
                            {
                                RedirectController.CancelRedirect(ref result, null, settings, "Redirect requested but cancelled because disallowed");
                            }
                        }
                    }
                }
                else if (result.TabId > -1 && result.RedirectAllowed && result.Action == ActionType.CheckFor301)
                {
                    //301 check was requested in earlier processing
                    //get the tab controller and retrieve the tab the request is for 
                    //don't redirect unless allowed, the tab is valid, and it's not an admin or super tab 
                    if (settings.RedirectUnfriendly)
                    {
                        TabInfo tab;
                        bool allowRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, true, out tab, settings);
                        if (allowRedirect && tab != null)
                        {
                            //remove the http alias from the url. Do this by putting the url back together from the request and removing the alias 
                            string rewritePathOnly;
                            if (result.DoRewrite)
                            {
                                rewritePathOnly = result.RewritePath;
                                var pos = rewritePathOnly.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase);
                                if (pos > Null.NullInteger)
                                {
                                    rewritePathOnly = rewritePathOnly.Substring(pos);
                                }
                            }
                            else
                            {
                                rewritePathOnly = requestUri.Host + requestUri.PathAndQuery;
                            }

                            //remove the http alias from the path 
                            var pathAliasEnd = rewritePathOnly.IndexOf(result.PortalAlias.HTTPAlias, StringComparison.InvariantCultureIgnoreCase);
                            var queryStringIndex = rewritePathOnly.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
                            if (pathAliasEnd > Null.NullInteger && (queryStringIndex == Null.NullInteger || pathAliasEnd < queryStringIndex))
                            {
                                rewritePathOnly = rewritePathOnly.Substring(pathAliasEnd + result.PortalAlias.HTTPAlias.Length);
                            }
                            
                            rewritePathOnly = HttpUtility.UrlDecode(rewritePathOnly, Encoding.UTF8);
                            //now check to see if need to remove /default.aspx from the end of the requested Url
                            string requestedUrl = fullUrl;
                            int requestedUrlAliasEnd = requestedUrl.IndexOf(result.PortalAlias.HTTPAlias, StringComparison.InvariantCultureIgnoreCase) 
                                                        + (result.PortalAlias.HTTPAlias + "/").Length;
                            if (requestedUrlAliasEnd > Null.NullInteger)
                            {
                                //818 : when a site root is used for a custom page Url, then check for max length within bounds
                                if ((requestedUrl.Length - requestedUrlAliasEnd) >= 12 && requestedUrl.Substring(requestedUrlAliasEnd).ToLower() == "default.aspx")
                                {
                                    requestedUrl = requestedUrl.Substring(0, requestedUrl.Length - 12);
                                    //12 = default.aspx length
                                }
                            }

                            //what happens here is that the request is reverse-engineered to see if it matches what the friendly Url shoudl have been 
                            //get what the friendly Url for this tab should be 
                            string bestFriendlyUrl;
                            //819 : leaving /do301/check in Url because not using cleanPath to remove from

                            string cleanPath = RedirectTokens.RemoveAnyRedirectTokensAndReasons(rewritePathOnly);
                            //string cleanPath = rewritePathOnly.Replace("&do301=check","");//remove check parameter if it exists
                            //cleanPath = cleanPath.Replace("&do301=true", "");//don't pass through internal redirect check parameter
                            cleanPath = cleanPath.Replace("&_aumdebug=true", ""); //remove debug parameter if it exists

                            Match match = Regex.Match(rewritePathOnly, "(?:&(?<parm>.[^&]+)=$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            if (match.Success)
                            {
                                //when the pathOnly value ends with '=' it means there is a query string pair with a key and no value
                                //make the assumption that this was passed in as a page name OTHER than default page
                                string pageName = match.Groups["parm"].Value; //get the last parameter in the list

                                cleanPath = cleanPath.Replace(match.Value, "");
                                //remove the last parameter from the path

                                //generate teh friendly URl name with the last parameter as the page name, not a query string parameter
                                bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab, 
                                                                                                    cleanPath,
                                                                                                    pageName + settings.PageExtension,
                                                                                                    result.HttpAlias,
                                                                                                    false, 
                                                                                                    settings,
                                                                                                    Guid.Empty);
                            }
                            else
                            {
                                bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab, 
                                                                                                    cleanPath,
                                                                                                    Globals.glbDefaultPage,
                                                                                                    result.HttpAlias,
                                                                                                    false, 
                                                                                                    settings,
                                                                                                    Guid.Empty);
                            }

                            //if the incomign request doesn't match the 'most friendly' url, a 301 Moved Permanently status is returned, along with the friendly url 
                            //check the bestFriendlyUrl against either the url, or rawUrl (with and without host) 
                            //in each case, the aumdebug parameter will be searched for and replaced
                            var urlDecode = HttpUtility.UrlDecode(requestedUrl);
                            if (urlDecode != null)
                            {
                                string rawUrlWithHost = StripDebugParameter(urlDecode.ToLower());
                                //string rawUrlWithHost = StripDebugParameter(System.Web.HttpUtility.UrlDecode(scheme + requestUri.Host + requestUri.PathAndQuery).ToLower());
                                string rawUrlWithHostNoScheme = StripDebugParameter(rawUrlWithHost.Replace(scheme, ""));
                                string bestFriendlyNoScheme = StripDebugParameter(bestFriendlyUrl.ToLower().Replace(scheme, ""));
                                string requestedPathNoScheme = StripDebugParameter(requestUri.AbsoluteUri.Replace(scheme, "").ToLower());
                                string rawUrlLowerCase = StripDebugParameter(requestUri.AbsoluteUri.ToLower());

                                //check to see if just an alias redirect of an internal alias
                                var primaryAliases = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();

                                if (settings.InternalAliasList != null && settings.InternalAliasList.Count > 0 && primaryAliases.Count > 0)
                                {
                                    var cpa = primaryAliases.GetAliasByPortalIdAndSettings(result);
                                    if (cpa != null)
                                    {
                                        string chosenAlias = cpa.HTTPAlias.ToLower();
                                        foreach (InternalAlias ia in settings.InternalAliasList)
                                        {
                                            string internalAlias = ia.HttpAlias.ToLower();
                                            if (requestedPathNoScheme.Contains(internalAlias))
                                            {
                                                //an internal alias has been used.
                                                //replace this in the comparison charts to do a 'fair' comparison
                                                requestedPathNoScheme = requestedPathNoScheme.Replace(internalAlias,chosenAlias);
                                                rawUrlWithHost = rawUrlWithHost.Replace(scheme + internalAlias,scheme + chosenAlias);
                                                rawUrlWithHostNoScheme = rawUrlWithHostNoScheme.Replace(internalAlias,chosenAlias);
                                                rawUrlLowerCase = rawUrlLowerCase.Replace(internalAlias, chosenAlias);
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!(bestFriendlyNoScheme == requestedPathNoScheme
                                      || bestFriendlyNoScheme == rawUrlWithHost
                                      || bestFriendlyNoScheme == rawUrlWithHostNoScheme
                                      || bestFriendlyNoScheme == HttpUtility.UrlDecode(requestedPathNoScheme)
                                      || bestFriendlyNoScheme == rawUrlLowerCase))
                                {
                                    redirected = true;
                                    result.Action = ActionType.Redirect301;
                                    result.FinalUrl = bestFriendlyUrl;
                                    if (result.Reason != RedirectReason.Custom_Tab_Alias &&
                                        result.Reason != RedirectReason.Deleted_Page &&
                                        result.Reason != RedirectReason.Disabled_Page)
                                    {
                                        result.Reason = RedirectReason.Unfriendly_Url_2;
                                    }
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + requestedPathNoScheme + " [requested with no scheme]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + rawUrlWithHost + " [requested with host and scheme]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + rawUrlWithHostNoScheme + " [requested with host, no scheme]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + HttpUtility.UrlDecode(requestedPathNoScheme) + " [requested and decoded]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + rawUrlLowerCase + " [requested raw Url]");
                                }
                            }
                        }
                    }
                }

                if (result.RedirectAllowed && settings.RedirectWrongCase)
                {
                    //check for redirects where a redirectToSubDomain is specified, 
                    //redirect for Wrong case is specified, and there is a valid tab and it's not already redirected somewhere else
                    bool doRedirect = false;
                    string redirectPath = redirected ? result.FinalUrl : requestUri.AbsoluteUri;
                    string redirectPathOnly = redirectPath;
                    if (redirectPathOnly.Contains("?"))
                    {
                        redirectPathOnly = redirectPathOnly.Substring(0, redirectPathOnly.IndexOf("?", StringComparison.Ordinal));
                    }
                    // Thanks Etienne for the fix for Diacritic Characters Terminal Loop!
                    // if the url contains url encoded characters, they appear here uppercase -> %C3%83%C2
                    // decode the url to get back the original character and do proper casing comparison
                    string urlDecodedRedirectPath = HttpUtility.UrlDecode(redirectPathOnly);

                    //check for wrong case redirection
                    if (urlDecodedRedirectPath != null && (settings.RedirectWrongCase && String.CompareOrdinal(urlDecodedRedirectPath, urlDecodedRedirectPath.ToLower()) != 0))
                    {
                        TabInfo tab;
                        bool allowRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, true, out tab, settings);

                        if (allowRedirect && !string.IsNullOrEmpty(settings.ForceLowerCaseRegex))
                        {
                            //don't allow redirect if excluded from redirecting in the force lower case regex pattern (606)
                            allowRedirect = !Regex.IsMatch(redirectPath, settings.ForceLowerCaseRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        }

                        if (allowRedirect)
                        {
                            //special case : when IIS automatically places /default.aspx on the end of the string,
                            //then don't try and redirect to the lower case /default.aspx, just let it through.
                            //we don't know whether IIS appended /Default.aspx on the end, however, we can guess
                            //if the redirectDefault.aspx is turned on (511)
                            if (settings.RedirectDefaultPage == false && redirectPathOnly.EndsWith(Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
                            {
                                //ignore this, because it's just a redirect of the /Default.aspx to /default.aspx
                            }
                            else
                            {
                                redirectPath = redirectPath.Replace(redirectPathOnly, redirectPathOnly.ToLower());
                                doRedirect = true;
                                result.Reason = RedirectReason.Not_Lower_Case;
                            }
                        }
                    }
                    if (doRedirect)
                    {
                        result.Action = ActionType.Redirect301;
                        result.FinalUrl = CheckForSiteRootRedirect(result.PortalAlias.HTTPAlias, redirectPath);
                        redirected = true;
                    }
                }
            }
            return redirected;
        }

        private static string StripDebugParameter(string url)
        {
            string result = Regex.Replace(url, @"(&|\?)_aumdebug=[a-zA-Z]+(?:&|$)", "");
            return result;
        }

        private static bool CheckFor301RedirectExclusion(int tabId, int portalId, bool checkBaseUrls, out TabInfo tab, FriendlyUrlSettings settings)
        {
            bool doRedirect = false;
            var tc = new TabController();
            tab = tc.GetTab(tabId, portalId, false);
            //don't redirect unless allowed, the tab is valid, and it's not an admin or super tab 
            if (tab != null && tab.IsSuperTab == false && !tab.DoNotRedirect)
            {
                if (checkBaseUrls)
                {
                    //no redirect for friendly url purposes if the tab is in the 'base friendly urls' section
                    doRedirect = !RewriteController.IsExcludedFromFriendlyUrls(tab, settings, true);
                }
                else
                {
                    doRedirect = true;
                }
            }
            return doRedirect;
        }

        /// <summary>
        /// Make sure any redirect to the site root doesn't append the nasty /default.aspx on the end
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="destUrl"></param>
        /// <returns></returns>
        internal static string CheckForSiteRootRedirect(string alias, string destUrl)
        {
            //540 - don't append /default.aspx onto the end of a site root redirect.
            if (destUrl.EndsWith(alias + "/" + Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
            {
                //this is just the portal alias root + /defualt.aspx.
                //we don't want that, just the portalAliasRoot + "/"
                string aliasPlusSlash = alias + "/";
                //get everything up to the end of the portal alias
                destUrl = destUrl.Substring(0, destUrl.IndexOf(aliasPlusSlash, StringComparison.Ordinal) + aliasPlusSlash.Length);
            }
            return destUrl;
        }

        #endregion
    }
}