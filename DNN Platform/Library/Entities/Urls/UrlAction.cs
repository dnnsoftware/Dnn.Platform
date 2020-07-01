// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// The UrlAction class keeps state of the current Request throughout the rewriting process.
    /// </summary>
    public class UrlAction
    {
        private List<string> _licensedProviders;
        private PortalAliasInfo _portalAlias;

        // 829 add in constructor that works around physical path length restriction
        public UrlAction(HttpRequest request)
        {
            this.BrowserType = BrowserTypes.Normal;
            this.CanRewrite = StateBoolean.NotSet;
            this.Action = ActionType.Continue;
            string physicalPath = string.Empty;
            try
            {
                physicalPath = request.PhysicalPath;
            }
            catch (PathTooLongException)
            {
                // don't handle exception, but put something into the physical path
                physicalPath = request.ApplicationPath; // will be no file for this location
                this.VirtualPath = StateBoolean.True;
            }
            catch (ArgumentException)
            {
                // don't handle exception, but put something into the physical path
                physicalPath = request.ApplicationPath; // will be no file for this location
                this.VirtualPath = StateBoolean.True;
            }
            finally
            {
                this.Constructor(request.Url.Scheme, request.ApplicationPath, physicalPath);
            }
        }

        public UrlAction(string scheme, string applicationPath, string physicalPath)
        {
            this.BrowserType = BrowserTypes.Normal;
            this.CanRewrite = StateBoolean.NotSet;
            this.Action = ActionType.Continue;
            this.Constructor(scheme, applicationPath, physicalPath);
        }

        public Uri Url { get; set; }

        public bool DoRewrite { get; set; }

        public bool FriendlyRewrite { get; set; }

        // friendlyRewrite means it was rewritten without looking up the tabid in the url
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

        // the alias for the current request
        public PortalAliasInfo PortalAlias
        {
            get { return this._portalAlias; }

            set
            {
                if (value != null)
                {
                    this.PortalId = value.PortalID;
                    this.HttpAlias = value.HTTPAlias;
                }

                this._portalAlias = value;
            }
        }

        // the primary alias, if different to the current alias
        public PortalAliasInfo PrimaryAlias { get; set; }

        public DotNetNuke.Entities.Portals.PortalSettings.PortalAliasMapping PortalAliasMapping { get; set; }

        public bool CustomParmRewrite { get; set; }

        // 737 : mobile browser identificatino
        public BrowserTypes BrowserType { get; private set; }

        public bool IsPhysicalResource { get; set; }

        public string UnlicensedProviderMessage { get; set; }

        public bool UnlicensedProviderCalled { get; set; }

        /// <summary>
        /// Sets the action value, but checks to ensure that the action is
        /// not being 'downgraded' (example: cannot set 'Redirect301' to 'CheckFor301').
        /// </summary>
        /// <param name="newAction"></param>
        public void SetActionWithNoDowngrade(ActionType newAction)
        {
            switch (newAction)
            {
                case ActionType.CheckFor301:
                    if (this.Action != ActionType.Redirect301
                        && this.Action != ActionType.Redirect302
                        && this.Action != ActionType.Redirect302Now
                        && this.Action != ActionType.Output404)
                    {
                        this.Action = newAction;
                    }

                    break;
                default:
                    this.Action = newAction;
                    break;
            }
        }

        public void AddLicensedProviders(List<string> licensedProviders)
        {
            if (this._licensedProviders == null)
            {
                this._licensedProviders = new List<string>();
            }

            foreach (string lp in licensedProviders)
            {
                if (this._licensedProviders.Contains(lp.ToLowerInvariant()) == false)
                {
                    this._licensedProviders.Add(lp.ToLowerInvariant());
                }
            }
        }

        public void AddLicensedProvider(string providerName)
        {
            if (this._licensedProviders == null)
            {
                this._licensedProviders = new List<string>();
            }

            if (this._licensedProviders.Contains(providerName.ToLowerInvariant()) == false)
            {
                this._licensedProviders.Add(providerName.ToLowerInvariant());
            }
        }

        public bool IsProviderLicensed(string providerName)
        {
            if (this._licensedProviders == null)
            {
                return false;
            }

            return this._licensedProviders.Contains(providerName.ToLowerInvariant());
        }

        /// <summary>
        /// Copies the original request path to the OriginalPath variables (originalPath, originanPathNoAlias).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="settings"></param>
        public void SetOriginalPath(string path, FriendlyUrlSettings settings)
        {
            this.OriginalPath = path;
            this.OriginalPathNoAlias = path;
            if (!string.IsNullOrEmpty(this.HttpAlias) && path.Contains(this.HttpAlias))
            {
                this.OriginalPathNoAlias = path.Substring(path.IndexOf(this.HttpAlias, StringComparison.Ordinal) + this.HttpAlias.Length);
            }
        }

        public void SetBrowserType(HttpRequest request, HttpResponse response, FriendlyUrlSettings settings)
        {
            // set the mobile browser type
            if (request != null && response != null && settings != null)
            {
                this.BrowserType = FriendlyUrlController.GetBrowserType(request, response, settings);
            }
        }

        public void SetRedirectAllowed(string path, FriendlyUrlSettings settings)
        {
            string regexExpr = settings.DoNotRedirectRegex;
            try
            {
                if (!string.IsNullOrEmpty(regexExpr))
                {
                    // if a regex match, redirect Not allowed
                    this.RedirectAllowed = !Regex.IsMatch(path, regexExpr, RegexOptions.IgnoreCase);
                }
                else
                {
                    this.RedirectAllowed = true;
                }
            }
            catch (Exception ex)
            {
                this.RedirectAllowed = true; // default : true, unless regex allows it.  So if regex causes an exception

                // then we should allow the redirect
                UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", this);
                this.Ex = ex;
            }
        }

        private void Constructor(string scheme, string applicationPath, string physicalPath)
        {
            if (scheme.EndsWith("://") == false)
            {
                this.Scheme = scheme + "://";
            }
            else
            {
                this.Scheme = scheme;
            }

            this.ApplicationPath = applicationPath;
            string domainPath = applicationPath.Replace(scheme, string.Empty);
            this.DomainName = domainPath.Contains("/") ? domainPath.Substring(0, domainPath.IndexOf('/')) : domainPath;
            this.PhysicalPath = physicalPath;
            this.PortalId = -1;
            this.TabId = -1;
            this.Reason = RedirectReason.Not_Redirected;
            this.FriendlyRewrite = false;
            this.BypassCachedDictionary = false;
            this.VirtualPath = StateBoolean.NotSet;
            this.IsSecureConnection = false;
            this.IsSSLOffloaded = false;
            this.DebugMessages = new List<string>();
            this.CultureCode = null;
        }
    }
}
