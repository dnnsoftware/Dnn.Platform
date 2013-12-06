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

using DotNetNuke.Collections;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Entities.Urls
{
    [Serializable]
    public class FriendlyUrlSettings
    {
        #region Private Members

        private const string _oldTriggerRegexValue = @"(.+)(\&ctl=tab)(.+)|(/Admin/Tabs/)";

        //894 : new switch to disable custom url provider 
        private string _internalAliases;
        private string _pageExtensionUsage = PageExtensionUsageType.AlwaysUse.ToString();

        private string _deletedTabHandling;

        #endregion

        #region private helper methods

        internal List<string> PortalValues { get; set; }


        #endregion

        public const string ReplaceSpaceWithNothing = "None";
        public const string SpaceEncodingPlus = "+";
        public const string SpaceEncodingHex = "%20";

        // Settings Keys
        public const string DeletedTabHandlingTypeSetting = "AUM_DeletedTabHandlingType";
        public const string ErrorPage404Setting = "AUM_ErrorPage404";
        public const string ErrorPage500Setting = "AUM_ErrorPage500";
        public const string ForceLowerCaseSetting = "AUM_ForceLowerCase";
        public const string PageExtensionSetting = "AUM_PageExtension";
        public const string PageExtensionUsageSetting = "AUM_PageExtensionUsage";
        public const string RedirectOldProfileUrlSetting = "AUM_RedirectOldProfileUrl";
        public const string RedirectUnfriendlySetting = "AUM_RedirectUnfriendly";
        public const string ReplaceSpaceWithSetting = "AUM_ReplaceSpaceWith";
        public const string UrlFormatSetting = "AUM_UrlFormat";
        public const string RedirectMixedCaseSetting = "AUM_RedirectMixedCase";
        public const string SpaceEncodingValueSetting = "AUM_SpaceEncodingValue";
        public const string AutoAsciiConvertSetting = "AUM_AutoAsciiConvert";
        public const string ReplaceCharsSetting = "AUM_ReplaceChars";
        public const string CheckForDuplicatedUrlsSetting = "AUM_CheckForDuplicatedUrls";
        public const string FriendlyAdminHostUrlsSetting = "AUM_FriendlyAdminHostUrls";
        public const string EnableCustomProvidersSetting = "AUM_EnableCustomProviders";
        public const string ReplaceCharWithCharSetting = "AUM_ReplaceCharWithChar";
        public const string IgnoreRegexSetting = "AUM_IgnoreUrlRegex";
        public const string SiteUrlsOnlyRegexSetting = "AUM_SiteUrlsOnlyRegex";
        public const string DoNotRedirectUrlRegexSetting = "AUM_DoNotRedirectUrlRegex";
        public const string DoNotRedirectHttpsUrlRegexSetting = "AUM_DoNotRedirectHttpsUrlRegex";
        public const string PreventLowerCaseUrlRegexSetting = "AUM_PreventLowerCaseUrlRegex";
        public const string DoNotUseFriendlyUrlRegexSetting = "AUM_DoNotUseFriendlyUrlRegex";
        public const string KeepInQueryStringRegexSetting = "AUM_KeepInQueryStringRegex";
        public const string UrlsWithNoExtensionRegexSetting = "AUM_UrlsWithNoExtensionRegex";
        public const string ValidFriendlyUrlRegexSetting = "AUM_ValidFriendlyUrlRegex";
        public const string DoNotRewriteRegExSetting = "AUM_DoNotRewriteRegEx";
        public const string UsePortalDefaultLanguageSetting = "AUM_UsePortalDefaultLanguage";
        public const string AllowDebugCodeSetting = "AUM_AllowDebugCode";
        public const string LogCacheMessagesSetting = "AUM_LogCacheMessages";
        public const string VanityUrlPrefixSetting = "AUM_VanityUrlPrefix";

        #region Public Properties

        internal bool AllowReporting { get; set; }

        public bool AllowDebugCode { get; set; }

        public bool AutoAsciiConvert { get; set; }
        
        public TimeSpan CacheTime { get; set; }

        public bool CheckForDuplicateUrls { get; set; }

        public DeletedTabHandlingType DeletedTabHandlingType
        {
            get
            {
                DeletedTabHandlingType val;
                switch (_deletedTabHandling.ToLower())
                {
                    case "do301redirecttoportalhome":
                        val = DeletedTabHandlingType.Do301RedirectToPortalHome;
                        break;
                    default:
                        val = DeletedTabHandlingType.Do404Error;
                        break;
                }
                return val;
            }
            set
            {
                string newValue = value.ToString();
                _deletedTabHandling = newValue;
            }
        }

        public string DoNotIncludeInPathRegex { get; set; }

        public string DoNotRedirectRegex { get; set; }

        public string DoNotRedirectSecureRegex { get; set; }

        public string DoNotRewriteRegex { get; set; }

        public bool ForceLowerCase { get; set; }

        public string ForceLowerCaseRegex { get; set; }

        public bool ForcePortalDefaultLanguage { get; set; }

        public DNNPageForwardType ForwardExternalUrlsType
        {
            get
            {
                return DNNPageForwardType.Redirect301;
            }
        }

        public bool FriendlyAdminHostUrls { get; set; }

        public bool EnableCustomProviders { get; set; }

        public string IgnoreRegex { get; set; }

        public string IllegalChars { get; set; }

        public bool IncludePageName { get; set; }

        public List<InternalAlias> InternalAliasList { get; private set; }

        public bool IsDirty { get; set; }
        
        public bool IsLoading { get; private set; }

        public bool LogCacheMessages { get; set; }

        public string NoFriendlyUrlRegex { get; set; }

        public string PageExtension { get; set; }

        public PageExtensionUsageType PageExtensionUsageType
        {
            get
            {
                var val = PageExtensionUsageType.Never;
                switch (_pageExtensionUsage.ToLower())
                {
                    case "always":
                    case "alwaysuse":
                        val = PageExtensionUsageType.AlwaysUse;
                        break;
                    case "never":
                        val = PageExtensionUsageType.Never;
                        break;
                    case "pageonly":
                        val = PageExtensionUsageType.PageOnly;
                        break;
                }
                return val;
            }
            set
            {
                string newValue = value.ToString();
                _pageExtensionUsage = newValue;
            }
        }

        public int PortalId { get; set; }

        public List<string> ProcessRequestList { get; private set; }

        public bool RedirectDefaultPage { get; set; }

        public bool RedirectOldProfileUrl { get; set; }

        public bool RedirectUnfriendly { get; set; }

        public bool RedirectWrongCase { get; set; }

        public string Regex404 { get; set; }

        public string RegexMatch { get; set; }

        public Dictionary<string, string> ReplaceCharacterDictionary { get; private set; }

        public string ReplaceChars { get; set; }

        public bool ReplaceDoubleChars { get; set; }

        public string ReplaceSpaceWith { get; set; }

        public string SpaceEncodingValue { get; set; }

        public bool SSLClientRedirect { get; set; }

        public int TabId404 { get; set; }

        public int TabId500 { get; set; }

        public string Url404 { get; set; }

        public string Url500 { get; set; }

        public string UrlFormat { get; set; }

        public string UseBaseFriendlyUrls { get; set; }

        public string UseMobileAlias { get; set; }

        public string UseSiteUrlsRegex { get; set; }

        public string ValidExtensionlessUrlsRegex { get; set; }

        public string InternalAliases
        {
            get { return _internalAliases; }
            set
            {
                _internalAliases = value;
                ParseInternalAliases(); //splits into list
            }
        }

        public string VanityUrlPrefix { get; set; }

        #endregion

        #region initialization methods

        private bool GetBooleanSetting(string key, bool defaultValue)
        {
            //First Get the Host Value using the passed in value as default
            var returnValue = (PortalId < -1) ? defaultValue : HostController.Instance.GetBoolean(key, defaultValue);

            if (PortalId > -1)
            {
                //Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSettingAsBoolean(key, PortalId, returnValue);
            }

            return returnValue;
        }

        private int GetIntegerSetting(string key, int defaultValue)
        {
            //First Get the Host Value using the passed in value as default
            var returnValue = (PortalId < -1) ? defaultValue : HostController.Instance.GetInteger(key, defaultValue);

            if (PortalId > -1)
            {
                //Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSettingAsInteger(key, PortalId, returnValue);
            }

            return returnValue;
        }

        private string GetStringSetting(string key, string defaultValue)
        {
            //First Get the Host Value using the passed in value as default
            var returnValue = (PortalId < -1) ? defaultValue : HostController.Instance.GetString(key, defaultValue);

            if (PortalId > -1)
            {
                //Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSetting(key, PortalId, returnValue);
            }

            return returnValue;
        }

        /// <summary>
        /// Initialiser for FriendlyUrlSettings provider object.  Initialises values by reading in from the web.config file
        /// </summary>
        public FriendlyUrlSettings(int portalId)
        {
            PortalId = portalId;

            PortalValues = new List<string>();

            IncludePageName = GetBooleanSetting("includePageName", true);
            RegexMatch = GetStringSetting(ValidFriendlyUrlRegexSetting, @"[^\w\d _-]");
            RedirectUnfriendly = GetBooleanSetting(RedirectUnfriendlySetting, true);
            UrlFormat = GetStringSetting(UrlFormatSetting, "advanced");

            //541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
			DoNotRedirectRegex = GetStringSetting(DoNotRedirectUrlRegexSetting, @"(\.axd)|/Rss\.aspx|/SiteMap\.aspx|/ProfilePic\.ashx|/LinkClick\.aspx|/Providers/|/DesktopModules/|ctl=MobilePreview|/ctl/MobilePreview");
            DoNotRedirectSecureRegex = GetStringSetting(DoNotRedirectHttpsUrlRegexSetting, String.Empty);

            DoNotRewriteRegex = GetStringSetting(DoNotRewriteRegExSetting, @"/DesktopModules/|/Providers/|/LinkClick\.aspx|/profilepic\.ashx");

            RedirectDefaultPage = GetBooleanSetting("redirectDefaultPage", false);
            PageExtension = GetStringSetting(PageExtensionSetting, ".aspx");
            _pageExtensionUsage = GetStringSetting(PageExtensionUsageSetting, PageExtensionUsageType.Never.ToString());

            IgnoreRegex = GetStringSetting(IgnoreRegexSetting, @"(?<!linkclick\.aspx.+)(?:(?<!\?.+)(\.pdf$|\.gif$|\.png($|\?)|\.css($|\?)|\.js($|\?)|\.jpg$|\.axd($|\?)|\.swf$|\.flv$|\.ico$|\.xml($|\?)|\.txt$))");
            UseSiteUrlsRegex = GetStringSetting(SiteUrlsOnlyRegexSetting, @"/rss\.aspx|Telerik.RadUploadProgressHandler\.ashx|BannerClickThrough\.aspx|(?:/[^/]+)*/Tabid/\d+/.*default\.aspx");

            ForceLowerCase = GetBooleanSetting(ForceLowerCaseSetting, false);
            ForceLowerCaseRegex = GetStringSetting(PreventLowerCaseUrlRegexSetting, String.Empty);

            RedirectWrongCase = GetBooleanSetting(RedirectMixedCaseSetting, false);
            SSLClientRedirect = GetBooleanSetting("sslClientRedirect", true);
            LogCacheMessages = GetBooleanSetting(LogCacheMessagesSetting, false);

            //791 : use threadlocking option
            ReplaceSpaceWith = GetStringSetting(ReplaceSpaceWithSetting, "-");
            var replaceCharwithChar = GetStringSetting(ReplaceCharWithCharSetting, String.Empty);

            ReplaceCharacterDictionary = CollectionExtensions.CreateDictionaryFromString(replaceCharwithChar == "[]" ? string.Empty : replaceCharwithChar, ';', ',');

            SpaceEncodingValue = GetStringSetting(SpaceEncodingValueSetting, SpaceEncodingHex);


            //893 : new extensionless Urls check for validating urls which have no extension but aren't 404
            ValidExtensionlessUrlsRegex = GetStringSetting(UrlsWithNoExtensionRegexSetting, @"\.asmx/|\.ashx/|\.svc/|\.aspx/|\.axd/");

            //922 : new options for allowing user-configured replacement of characters
            ReplaceChars = GetStringSetting(ReplaceCharsSetting, @" &$+,/?~#<>()¿¡«»!""");
            IllegalChars = GetStringSetting("illegalChars", @"<>/\?:&=+|%#");
            ReplaceDoubleChars = GetBooleanSetting("replaceDoubleChars", true);

            //793 : checkforDupUrls not being read
            CheckForDuplicateUrls = GetBooleanSetting(CheckForDuplicatedUrlsSetting, true);

            /* 454 New 404 error handling */
            Regex404 = GetStringSetting("regex404", String.Empty);
            TabId404 = PortalController.GetPortalSettingAsInteger(ErrorPage404Setting, PortalId, -1);
            Url404 = GetStringSetting("url404", String.Empty);

            //get the 500 error values, if not supplied, use the 404 values
            TabId500 = PortalController.GetPortalSettingAsInteger(ErrorPage500Setting, PortalId, -1);
            if (TabId500 == -1)
            {
                TabId500 = TabId404;
            }
            Url500 = GetStringSetting("url500", null) ?? Url404;

            _deletedTabHandling = PortalController.GetPortalSetting(DeletedTabHandlingTypeSetting, PortalId, DeletedTabHandlingType.Do404Error.ToString());

            UseBaseFriendlyUrls = GetStringSetting("useBaseFriendlyUrls", "/SearchResults;/ModuleDefinitions");
            if (UseBaseFriendlyUrls.EndsWith(";") == false)
            {
                UseBaseFriendlyUrls += ";";
            }

            //655 : new noFriendlyUrlRegex value to ignore generation of certain urls
            NoFriendlyUrlRegex = GetStringSetting(DoNotUseFriendlyUrlRegexSetting, @"/Rss\.aspx");

            //703 default debug code to false
            AllowDebugCode = GetBooleanSetting(AllowDebugCodeSetting, false);
            //allow reporting value for checking for updates and reporting usage data
            AllowReporting = GetBooleanSetting("allowReporting", true);

            //737 : integrate mobile alias
            UseMobileAlias = GetStringSetting("useMobileAlias", null);

            VanityUrlPrefix = GetStringSetting(VanityUrlPrefixSetting, "users");

            // allow for a list of internal aliases
            _internalAliases = GetStringSetting("internalAliases", null);
            ParseInternalAliases();

            //661 : do not include in path
            //742 : was not reading and saving value when 'doNotIncludeInPathRegex' used
            DoNotIncludeInPathRegex = GetStringSetting(KeepInQueryStringRegexSetting, @"/nomo/\d+|/runningDefault/[^/]+|/popup/(?:true|false)|/(?:page|category|sort|tags)/[^/]+");

            string processRequests = GetStringSetting("processRequests", null);
            if (processRequests != null)
            {
                processRequests = processRequests.ToLower();
                ProcessRequestList = !string.IsNullOrEmpty(processRequests)
                        ? new List<string>(processRequests.Split(';'))
                        : null;
            }

            //urls to be modified in the output html stream
            AutoAsciiConvert = GetBooleanSetting(AutoAsciiConvertSetting, false);

            //810 : allow forcing of default language in rewrites
            ForcePortalDefaultLanguage = GetBooleanSetting(UsePortalDefaultLanguageSetting, true);
            FriendlyAdminHostUrls = GetBooleanSetting(FriendlyAdminHostUrlsSetting, true);
            //894 : new switch to disable custom providers if necessary
            EnableCustomProviders = GetBooleanSetting(EnableCustomProvidersSetting, true);

            CacheTime = new TimeSpan(0, GetIntegerSetting("cacheTime", 1440), 0);

            RedirectOldProfileUrl = PortalController.GetPortalSettingAsBoolean(RedirectOldProfileUrlSetting, PortalId, true);
        }

        private void ParseInternalAliases()
        {
            if (!string.IsNullOrEmpty(_internalAliases))
            {
                InternalAliasList = new List<InternalAlias>();
                string[] raw = _internalAliases.Split(';');
                foreach (string rawVal in raw)
                {
                    if (rawVal.Length > 0)
                    {
                        var ia = new InternalAlias { HttpAlias = rawVal };
                        if (InternalAliasList.Contains(ia) == false)
                        {
                            InternalAliasList.Add(ia);
                        }
                    }
                }
            }
        }

        #endregion
    }
}