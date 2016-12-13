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
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// The UrlAction class keeps state of the current Request throughout the rewriting process
    /// </summary>
    public class UrlAction
    {
        //829 add in constructor that works around physical path length restriction
        public UrlAction(HttpRequest request)
        {
            BrowserType = BrowserTypes.Normal;
            CanRewrite = StateBoolean.NotSet;
            Action = ActionType.Continue;
            string physicalPath = "";
            try
            {
                physicalPath = request.PhysicalPath;
            }
            catch (PathTooLongException)
            {
                //don't handle exception, but put something into the physical path
                physicalPath = request.ApplicationPath; //will be no file for this location
                VirtualPath = StateBoolean.True;
            }
            finally
            {
                Constructor(request.Url.Scheme, request.ApplicationPath, physicalPath);
            }
        }

        public UrlAction(string scheme, string applicationPath, string physicalPath)
        {
            BrowserType = BrowserTypes.Normal;
            CanRewrite = StateBoolean.NotSet;
            Action = ActionType.Continue;
            Constructor(scheme, applicationPath, physicalPath);
        }

        private void Constructor(string scheme, string applicationPath, string physicalPath)
        {
            if (scheme.EndsWith("://") == false)
            {
                Scheme = scheme + "://";
            }
            else
            {
                Scheme = scheme;
            }
            ApplicationPath = applicationPath;
            string domainPath = applicationPath.Replace(scheme, "");
            DomainName = domainPath.Contains("/") ? domainPath.Substring(0, domainPath.IndexOf('/')) : domainPath;
            PhysicalPath = physicalPath;
            PortalId = -1;
            TabId = -1;
            Reason = RedirectReason.Not_Redirected;
            FriendlyRewrite = false;
            BypassCachedDictionary = false;
            VirtualPath = StateBoolean.NotSet;
            IsSecureConnection = false;
            IsSSLOffloaded = false;
            DebugMessages = new List<string>();
            CultureCode = null;
        }

        #region Private Members

        private List<string> _licensedProviders;
        private PortalAliasInfo _portalAlias;

        #endregion

        #region Public Properties

        public Uri Url { get; set; }
        public bool DoRewrite { get; set; }
        public bool FriendlyRewrite { get; set; }
        //friendlyRewrite means it was rewritten without looking up the tabid in the url
        public bool BypassCachedDictionary { get; set; }
        public string RewritePath { get; set; }
        public string RawUrl { get; set; }
        public string DebugData { get; set; }
        public string PhysicalPath { get; set; }
        public StateBoolean VirtualPath { get; set; }
        public string ApplicationPath { get; set; }
        public bool RebuildRequested { get; set; }
        public string FinalUrl { get; set; }
        public string Scheme { get; set; }
        public bool IsSecureConnection { get; set; }
        public bool IsSSLOffloaded { get; set; }
        public string DomainName { get; set; }
        public Exception Ex { get; set; }
        public string dictKey { get; set; }
        public string dictVal { get; set; }
        public List<string> DebugMessages { get; set; }

        public int TabId { get; set; }

        public int PortalId { get; set; }

        public RedirectReason Reason { get; set; }

        public string HttpAlias { get; set; }

        public ActionType Action { get; set; }

        public string CultureCode { get; set; }

        public string OriginalPath { get; private set; }

        public string OriginalPathNoAlias { get; private set; }

        public bool RedirectAllowed { get; private set; }

        public StateBoolean CanRewrite { get; set; }

        //the alias for the current request
        public PortalAliasInfo PortalAlias
        {
            get { return _portalAlias; }
            set
            {
                if (value != null)
                {
                    PortalId = value.PortalID;
                    HttpAlias = value.HTTPAlias;
                }
                _portalAlias = value;
            }
        }
        //the primary alias, if different to the current alias
        public PortalAliasInfo PrimaryAlias { get; set; }
        public DotNetNuke.Entities.Portals.PortalSettings.PortalAliasMapping PortalAliasMapping {get; set;}
        public bool CustomParmRewrite { get; set; }

        //737 : mobile browser identificatino
        public BrowserTypes BrowserType { get; private set; }

        public bool IsPhysicalResource { get; set; }

        #endregion

        #region public methods

        public string UnlicensedProviderMessage { get; set; }

        public bool UnlicensedProviderCalled { get; set; }

        /// <summary>
        /// Sets the action value, but checks to ensure that the action is 
        /// not being 'downgraded' (example: cannot set 'Redirect301' to 'CheckFor301')
        /// </summary>
        /// <param name="newAction"></param>
        public void SetActionWithNoDowngrade(ActionType newAction)
        {
            switch (newAction)
            {
                case ActionType.CheckFor301:
                    if (Action != ActionType.Redirect301
                        && Action != ActionType.Redirect302
                        && Action != ActionType.Redirect302Now
                        && Action != ActionType.Output404)
                    {
                        Action = newAction;
                    }
                    break;
                default:
                    Action = newAction;
                    break;
            }
        }


        public void AddLicensedProviders(List<string> licensedProviders)
        {
            if (_licensedProviders == null)
            {
                _licensedProviders = new List<string>();
            }
            foreach (string lp in licensedProviders)
            {
                if (_licensedProviders.Contains(lp.ToLower()) == false)
                {
                    _licensedProviders.Add(lp.ToLower());
                }
            }
        }

        public void AddLicensedProvider(string providerName)
        {
            if (_licensedProviders == null)
            {
                _licensedProviders = new List<string>();
            }
            if (_licensedProviders.Contains(providerName.ToLower()) == false)
            {
                _licensedProviders.Add(providerName.ToLower());
            }
        }

        public bool IsProviderLicensed(string providerName)
        {
            if (_licensedProviders == null)
            {
                return false;
            }
            return _licensedProviders.Contains(providerName.ToLower());
        }

        /// <summary>
        /// Copies the original request path to the OriginalPath variables (originalPath, originanPathNoAlias)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="settings"></param>
        public void SetOriginalPath(string path, FriendlyUrlSettings settings)
        {
            OriginalPath = path;
            OriginalPathNoAlias = path;
            if (!string.IsNullOrEmpty(HttpAlias) && path.Contains(HttpAlias))
            {
                OriginalPathNoAlias = path.Substring(path.IndexOf(HttpAlias, StringComparison.Ordinal) + HttpAlias.Length);
            }
        }

        public void SetBrowserType(HttpRequest request, HttpResponse response, FriendlyUrlSettings settings)
        {
            //set the mobile browser type
            if (request != null && response != null && settings != null)
            {
                BrowserType = FriendlyUrlController.GetBrowserType(request, response, settings);
            }
        }

        public void SetRedirectAllowed(string path, FriendlyUrlSettings settings)
        {
            string regexExpr = settings.DoNotRedirectRegex;
            try
            {
                if (!string.IsNullOrEmpty(regexExpr))
                {
                    //if a regex match, redirect Not allowed
                    RedirectAllowed = !Regex.IsMatch(path, regexExpr, RegexOptions.IgnoreCase);
                }
                else
                {
                    RedirectAllowed = true;
                }
            }
            catch (Exception ex)
            {
                RedirectAllowed = true; //default : true, unless regex allows it.  So if regex causes an exception
                //then we should allow the redirect

                UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", this);
                Ex = ex;
            }
        }

        #endregion
    }
}