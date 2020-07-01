// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    public class LanguageTokenReplace : TokenReplace
    {
        // see http://support.dotnetnuke.com/issue/ViewIssue.aspx?id=6505
        public LanguageTokenReplace()
            : base(Scope.NoSettings)
        {
            this.UseObjectLessExpression = true;
            this.PropertySource[ObjectLessToken] = new LanguagePropertyAccess(this, Globals.GetPortalSettings());
        }

        public string resourceFile { get; set; }
    }

    public class LanguagePropertyAccess : IPropertyAccess
    {
        private const string FlagIconPhysicalLocation = @"~\images\Flags";
        private const string NonExistingFlagIconFileName = "none.gif";
        public LanguageTokenReplace objParent;
        private readonly PortalSettings objPortal;

        public LanguagePropertyAccess(LanguageTokenReplace parent, PortalSettings settings)
        {
            this.objPortal = settings;
            this.objParent = parent;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope CurrentScope, ref bool PropertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "url":
                    return this.NewUrl(this.objParent.Language);
                case "flagsrc":
                    var mappedGifFile = PathUtils.Instance.MapPath($@"{FlagIconPhysicalLocation}\{this.objParent.Language}.gif");
                    return File.Exists(mappedGifFile) ? $"/{this.objParent.Language}.gif" : $@"/{NonExistingFlagIconFileName}";
                case "selected":
                    return (this.objParent.Language == CultureInfo.CurrentCulture.Name).ToString();
                case "label":
                    return Localization.GetString("Label", this.objParent.resourceFile);
                case "i":
                    return Globals.ResolveUrl("~/images/Flags");
                case "p":
                    return Globals.ResolveUrl(PathUtils.Instance.RemoveTrailingSlash(this.objPortal.HomeDirectory));
                case "s":
                    return Globals.ResolveUrl(PathUtils.Instance.RemoveTrailingSlash(this.objPortal.ActiveTab.SkinPath));
                case "g":
                    return Globals.ResolveUrl("~/portals/" + Globals.glbHostSkinFolder);
                default:
                    PropertyNotFound = true;
                    return string.Empty;
            }
        }

        /// <summary>
        /// getQSParams builds up a new querystring. This is necessary
        /// in order to prep for navigateUrl.
        /// we don't ever want a tabid, a ctl and a language parameter in the qs
        /// also, the portalid param is not allowed when the tab is a supertab
        /// (because NavigateUrl adds the portalId param to the qs).
        /// </summary>
        /// <param name="newLanguage">Language to switch into.</param>
        /// <param name="isLocalized"></param>
        /// <returns></returns>
        private string[] GetQsParams(string newLanguage, bool isLocalized)
        {
            string returnValue = string.Empty;
            NameValueCollection queryStringCollection = HttpContext.Current.Request.QueryString;
            var rawQueryStringCollection =
                HttpUtility.ParseQueryString(new Uri(HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.RawUrl).Query);

            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            string[] arrKeys = queryStringCollection.AllKeys;

            for (int i = 0; i <= arrKeys.GetUpperBound(0); i++)
            {
                if (arrKeys[i] != null)
                {
                    switch (arrKeys[i].ToLowerInvariant())
                    {
                        case "tabid":
                        case "ctl":
                        case "language": // skip parameter
                            break;
                        case "mid":
                        case "moduleid": // start of patch (Manzoni Fausto) gemini 14205
                            if (isLocalized)
                            {
                                string ModuleIdKey = arrKeys[i].ToLowerInvariant();
                                int moduleID;
                                int tabid;

                                int.TryParse(queryStringCollection[ModuleIdKey], out moduleID);
                                int.TryParse(queryStringCollection["tabid"], out tabid);
                                ModuleInfo localizedModule = ModuleController.Instance.GetModuleByCulture(moduleID, tabid, settings.PortalId, LocaleController.Instance.GetLocale(newLanguage));
                                if (localizedModule != null)
                                {
                                    if (!string.IsNullOrEmpty(returnValue))
                                    {
                                        returnValue += "&";
                                    }

                                    returnValue += ModuleIdKey + "=" + localizedModule.ModuleID;
                                }
                            }

                            break;
                        default:
                            if ((arrKeys[i].ToLowerInvariant() == "portalid") && this.objPortal.ActiveTab.IsSuperTab)
                            {
                                // skip parameter
                                // navigateURL adds portalid to querystring if tab is superTab
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(rawQueryStringCollection.Get(arrKeys[i])))
                                {
                                    // skip parameter as it is part of a querystring param that has the following form
                                    // [friendlyURL]/?param=value
                                    // gemini 25516
                                    if (!DotNetNuke.Entities.Host.Host.UseFriendlyUrls)
                                    {
                                        if (!string.IsNullOrEmpty(returnValue))
                                        {
                                            returnValue += "&";
                                        }

                                        returnValue += arrKeys[i] + "=" + HttpUtility.UrlEncode(rawQueryStringCollection.Get(arrKeys[i]));
                                    }
                                }

                                // on localised pages most of the module parameters have no sense and generate duplicate urls for the same content
                                // because we are on a other tab with other modules (example : /en-US/news/articleid/1)
                                else // if (!isLocalized) -- this applies only when a portal "Localized Content" is enabled.
                                {
                                    string[] arrValues = queryStringCollection.GetValues(i);
                                    if (arrValues != null)
                                    {
                                        for (int j = 0; j <= arrValues.GetUpperBound(0); j++)
                                        {
                                            if (!string.IsNullOrEmpty(returnValue))
                                            {
                                                returnValue += "&";
                                            }

                                            var qsv = arrKeys[i];
                                            qsv = qsv.Replace("\"", string.Empty);
                                            qsv = qsv.Replace("'", string.Empty);
                                            returnValue += qsv + "=" + HttpUtility.UrlEncode(arrValues[j]);
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }
            }

            if (!settings.ContentLocalizationEnabled && LocaleController.Instance.GetLocales(settings.PortalId).Count > 1 && !settings.EnableUrlLanguage)
            {
                // because useLanguageInUrl is false, navigateUrl won't add a language param, so we need to do that ourselves
                if (returnValue != string.Empty)
                {
                    returnValue += "&";
                }

                returnValue += "language=" + newLanguage.ToLowerInvariant();
            }

            // return the new querystring as a string array
            return returnValue.Split('&');
        }

        /// <summary>
        /// newUrl returns the new URL based on the new language.
        /// Basically it is just a call to NavigateUrl, with stripped qs parameters.
        /// </summary>
        /// <param name="newLanguage"></param>
        private string NewUrl(string newLanguage)
        {
            var newLocale = LocaleController.Instance.GetLocale(newLanguage);

            // Ensure that the current ActiveTab is the culture of the new language
            var tabId = this.objPortal.ActiveTab.TabID;
            var islocalized = false;

            var localizedTab = TabController.Instance.GetTabByCulture(tabId, this.objPortal.PortalId, newLocale);
            if (localizedTab != null)
            {
                islocalized = true;
                if (localizedTab.IsDeleted || !TabPermissionController.CanViewPage(localizedTab))
                {
                    var localizedPortal = PortalController.Instance.GetPortal(this.objPortal.PortalId, newLocale.Code);
                    tabId = localizedPortal.HomeTabId;
                }
                else
                {
                    var fullurl = string.Empty;
                    switch (localizedTab.TabType)
                    {
                        case TabType.Normal:
                            // normal tab
                            tabId = localizedTab.TabID;
                            break;
                        case TabType.Tab:
                            // alternate tab url
                            fullurl = TestableGlobals.Instance.NavigateURL(Convert.ToInt32(localizedTab.Url));
                            break;
                        case TabType.File:
                            // file url
                            fullurl = TestableGlobals.Instance.LinkClick(localizedTab.Url, localizedTab.TabID, Null.NullInteger);
                            break;
                        case TabType.Url:
                            // external url
                            fullurl = localizedTab.Url;
                            break;
                    }

                    if (!string.IsNullOrEmpty(fullurl))
                    {
                        return this.GetCleanUrl(fullurl);
                    }
                }
            }

            var rawQueryString = string.Empty;
            if (Entities.Host.Host.UseFriendlyUrls)
            {
                // Remove returnurl from query parameters to prevent that the language is changed back after the user has logged in
                // Example: Accessing protected page /de-de/Page1 redirects to /de-DE/Login?returnurl=%2f%2fde-de%2fPage1 and changing language to en-us on the login page
                // using the language links won't change the language in the returnurl parameter and the user will be redirected to the de-de version after logging in
                // Assumption: Loosing the returnurl information is better than confusing the user by switching the language back after the login
                var queryParams = HttpUtility.ParseQueryString(new Uri(string.Concat(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), HttpContext.Current.Request.RawUrl)).Query);
                queryParams.Remove("returnurl");
                var queryString = queryParams.ToString();
                if (queryString.Length > 0)
                {
                    rawQueryString = string.Concat("?", queryString);
                }
            }

            var controlKey = HttpContext.Current.Request.QueryString["ctl"];
            var queryStrings = this.GetQsParams(newLocale.Code, islocalized);
            var isSuperTab = this.objPortal.ActiveTab.IsSuperTab;
            var url = $"{TestableGlobals.Instance.NavigateURL(tabId, isSuperTab, this.objPortal, controlKey, newLanguage, queryStrings)}{rawQueryString}";

            return this.GetCleanUrl(url);
        }

        private string GetCleanUrl(string url)
        {
            var cleanUrl = PortalSecurity.Instance.InputFilter(url, PortalSecurity.FilterFlag.NoScripting);
            if (url != cleanUrl)
            {
                return string.Empty;
            }

            return url;
        }
    }
}
