// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;

    [Serializable]
    public class FriendlyUrlSettings
    {
        public const string ReplaceSpaceWithNothing = "None";
        public const string SpaceEncodingPlus = "+";
        public const string SpaceEncodingHex = "%20";

        // Settings Keys
        public const string DeletedTabHandlingTypeSetting = "AUM_DeletedTabHandlingType";
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
        public const string RedirectDefaultPageSetting = "AUM_RedirectDefaultPage";
        public const string SslClientRedirectSetting = "AUM_SSLClientRedirect";
        public const string IllegalCharsSetting = "AUM_IllegalChars";
        public const string ReplaceDoubleCharsSetting = "AUM_ReplaceDoubleChars";
        public const string Regex404Setting = "AUM_Regex404";
        public const string Url404Setting = "AUM_Url404";
        public const string Url500Setting = "AUM_Url500";
        public const string UseBaseFriendlyUrlsSetting = "AUM_UseBaseFriendlyUrls";
        public const string InternalAliasesSetting = "AUM_InternalAliases";
        public const string ProcessRequestsSetting = "AUM_ProcessRequests";
        public const string CacheTimeSetting = "AUM_CacheTime";
        public const string IncludePageNameSetting = "AUM_IncludePageName";

        private readonly IHostController _hostControllerInstance = HostController.Instance;

        // 894 : new switch to disable custom url provider
        private bool? _allowDebugCode;
        private bool? _autoAsciiConvert;
        private bool? _checkForDuplicateUrls;
        private bool? _enableCustomProviders;
        private bool? _forceLowerCase;
        private bool? _forcePortalDefaultLanguage;
        private bool? _friendlyAdminHostUrls;
        private bool? _includePageName;
        private bool? _logCacheMessages;
        private bool? _redirectDefaultPage;
        private bool? _redirectOldProfileUrl;
        private bool? _redirectUnfriendly;
        private bool? _redirectWrongCase;
        private bool? _replaceDoubleChars;
        private bool? _sslClientRedirect;
        private string _deletedTabHandling;
        private string _doNotIncludeInPathRegex;
        private string _doNotRedirectRegex;
        private string _doNotRedirectSecureRegex;
        private string _doNotRewriteRegex;
        private string _forceLowerCaseRegex;
        private string _ignoreRegex;
        private string _illegalChars;
        private string _internalAliases;
        private string _noFriendlyUrlRegex;
        private string _pageExtension;
        private string _pageExtensionUsage;
        private string _regex404;
        private string _regexMatch;
        private string _replaceChars;
        private string _replaceSpaceWith;
        private string _spaceEncodingValue;
        private string _url404;
        private string _url500;
        private string _urlFormat;
        private string _useBaseFriendlyUrls;
        private string _useSiteUrlsRegex;
        private string _validExtensionlessUrlsRegex;
        private string _vanityUrlPrefix;
        private TimeSpan? _cacheTime;
        private List<string> _processRequestList;
        private Dictionary<string, string> _replaceCharacterDictionary;

        public FriendlyUrlSettings(int portalId)
        {
            this.PortalId = portalId < -1 ? -1 : portalId;
            this.IsDirty = false;
            this.IsLoading = false;

            this.PortalValues = new List<string>();

            this.TabId500 = this.TabId404 = -1;

            if (portalId > -1)
            {
                var portal = PortalController.Instance.GetPortal(portalId);
                this.TabId500 = this.TabId404 = portal.Custom404TabId;

                if (this.TabId500 == -1)
                {
                    this.TabId500 = this.TabId404;
                }
            }
        }

        public List<string> ProcessRequestList
        {
            get
            {
                if (this._processRequestList == null)
                {
                    var processRequests = this.GetStringSetting(ProcessRequestsSetting, null);
                    if (processRequests != null)
                    {
                        processRequests = processRequests.ToLowerInvariant();
                        this._processRequestList = !string.IsNullOrEmpty(processRequests)
                            ? new List<string>(processRequests.Split(';'))
                            : new List<string>();
                    }
                }

                return this._processRequestList;
            }
        }

        public bool AllowDebugCode
        {
            get
            {
                if (!this._allowDebugCode.HasValue)
                {
                    // 703 default debug code to false
                    this._allowDebugCode = Host.Host.DebugMode;
                }

                return this._allowDebugCode.Value;
            }
        }

        public TimeSpan CacheTime
        {
            get
            {
                if (!this._cacheTime.HasValue)
                {
                    this._cacheTime = new TimeSpan(0, this.GetIntegerSetting(CacheTimeSetting, 1440), 0);
                }

                return this._cacheTime.Value;
            }
        }

        public bool CheckForDuplicateUrls
        {
            get
            {
                if (!this._checkForDuplicateUrls.HasValue)
                {
                    // 793 : checkforDupUrls not being read
                    this._checkForDuplicateUrls = this.GetBooleanSetting(CheckForDuplicatedUrlsSetting, true);
                }

                return this._checkForDuplicateUrls.Value;
            }
        }

        public DNNPageForwardType ForwardExternalUrlsType
        {
            get
            {
                return DNNPageForwardType.Redirect301;
            }
        }

        public bool EnableCustomProviders
        {
            get
            {
                if (!this._enableCustomProviders.HasValue)
                {
                    // 894 : new switch to disable custom providers if necessary
                    this._enableCustomProviders = this.GetBooleanSetting(EnableCustomProvidersSetting, true);
                }

                return this._enableCustomProviders.Value;
            }
        }

        public string IllegalChars
        {
            get
            {
                // 922 : new options for allowing user-configured replacement of characters
                return this._illegalChars ?? (this._illegalChars = this.GetStringSetting(IllegalCharsSetting, @"<>/\?:&=+|%#"));
            }
        }

        public bool LogCacheMessages
        {
            get
            {
                if (!this._logCacheMessages.HasValue)
                {
                    this._logCacheMessages = this.GetBooleanSetting(LogCacheMessagesSetting, false);
                }

                return this._logCacheMessages.Value;
            }
        }

        public string Regex404
        {
            get { return this._regex404 ?? (this._regex404 = this.GetStringSetting(Regex404Setting, string.Empty)); }
        }

        public Dictionary<string, string> ReplaceCharacterDictionary
        {
            get
            {
                if (this._replaceCharacterDictionary == null)
                {
                    var replaceCharwithChar = this.GetStringSetting(ReplaceCharWithCharSetting, string.Empty);
                    this._replaceCharacterDictionary = CollectionExtensions.CreateDictionaryFromString(replaceCharwithChar == "[]" ? string.Empty : replaceCharwithChar, ';', ',');
                }

                return this._replaceCharacterDictionary;
            }
        }

        public string ReplaceChars
        {
            get
            {
                // 922 : new options for allowing user-configured replacement of characters
                return this._replaceChars ?? (this._replaceChars = this.GetStringSetting(ReplaceCharsSetting, @" &$+,/?~#<>()¿¡«»!"""));
            }
        }

        public bool ReplaceDoubleChars
        {
            get
            {
                if (!this._replaceDoubleChars.HasValue)
                {
                    // 922 : new options for allowing user-configured replacement of characters
                    this._replaceDoubleChars = this.GetBooleanSetting(ReplaceDoubleCharsSetting, true);
                }

                return this._replaceDoubleChars.Value;
            }
        }

        public string Url404
        {
            get { return this._url404 ?? (this._url404 = this.GetStringSetting(Url404Setting, string.Empty)); }
        }

        public string Url500
        {
            get { return this._url500 ?? (this._url500 = this.GetStringSetting(Url500Setting, null) ?? this.Url404); }
        }

        public List<InternalAlias> InternalAliasList { get; private set; }

        public int PortalId { get; private set; }

        public bool IsDirty { get; private set; }

        public bool IsLoading { get; private set; }

        public bool AutoAsciiConvert
        {
            get
            {
                if (!this._autoAsciiConvert.HasValue)
                {
                    // urls to be modified in the output html stream
                    this._autoAsciiConvert = this.GetBooleanSetting(AutoAsciiConvertSetting, false);
                }

                return this._autoAsciiConvert.Value;
            }

            internal set { this._autoAsciiConvert = value; }
        }

        public DeletedTabHandlingType DeletedTabHandlingType
        {
            get
            {
                if (this._deletedTabHandling == null)
                {
                    this._deletedTabHandling = PortalController.GetPortalSetting(
                        DeletedTabHandlingTypeSetting, this.PortalId, DeletedTabHandlingType.Do404Error.ToString());
                }

                return "do301redirecttoportalhome".Equals(this._deletedTabHandling, StringComparison.InvariantCultureIgnoreCase)
                    ? DeletedTabHandlingType.Do301RedirectToPortalHome
                    : DeletedTabHandlingType.Do404Error;
            }

            internal set
            {
                var newValue = value.ToString();
                this._deletedTabHandling = newValue;
            }
        }

        public string DoNotIncludeInPathRegex
        {
            get
            {
                // 661 : do not include in path
                // 742 : was not reading and saving value when 'doNotIncludeInPathRegex' used
                return this._doNotIncludeInPathRegex ??
                       (this._doNotIncludeInPathRegex =
                           this.GetStringSetting(
                               KeepInQueryStringRegexSetting,
                               @"/nomo/\d+|/runningDefault/[^/]+|/popup/(?:true|false)|/(?:page|category|sort|tags)/[^/]+|tou/[^/]+|(/utm[^/]+/[^/]+)+"));
            }

            internal set { this._doNotIncludeInPathRegex = value; }
        }

        public string DoNotRedirectRegex
        {
            get
            {
                // 541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
                return this._doNotRedirectRegex ?? (this._doNotRedirectRegex = this.GetStringSetting(
                    DoNotRedirectUrlRegexSetting,
                    @"(\.axd)|/Rss\.aspx|/SiteMap\.aspx|\.ashx|/LinkClick\.aspx|/Providers/|/DesktopModules/|ctl=MobilePreview|/ctl/MobilePreview|/API/"));
            }

            internal set { this._doNotRedirectRegex = value; }
        }

        public string DoNotRedirectSecureRegex
        {
            get
            {
                // 541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
                return this._doNotRedirectSecureRegex ?? (this._doNotRedirectSecureRegex = this.GetStringSetting(DoNotRedirectHttpsUrlRegexSetting, string.Empty));
            }

            internal set { this._doNotRedirectSecureRegex = value; }
        }

        public string DoNotRewriteRegex
        {
            get
            {
                return this._doNotRewriteRegex ??
                       (this._doNotRewriteRegex =
                           this.GetStringSetting(DoNotRewriteRegExSetting, @"/DesktopModules/|/Providers/|/LinkClick\.aspx|/profilepic\.ashx|/DnnImageHandler\.ashx|/__browserLink/|/API/"));
            }

            internal set { this._doNotRewriteRegex = value; }
        }

        public bool ForceLowerCase
        {
            get
            {
                if (!this._forceLowerCase.HasValue)
                {
                    this._forceLowerCase = this.GetBooleanSetting(ForceLowerCaseSetting, false);
                }

                return this._forceLowerCase.Value;
            }

            internal set { this._forceLowerCase = value; }
        }

        public string ForceLowerCaseRegex
        {
            get { return this._forceLowerCaseRegex ?? (this._forceLowerCaseRegex = this.GetStringSetting(PreventLowerCaseUrlRegexSetting, string.Empty)); }
            internal set { this._forceLowerCaseRegex = value; }
        }

        public bool ForcePortalDefaultLanguage
        {
            get
            {
                if (!this._forcePortalDefaultLanguage.HasValue)
                {
                    // 810 : allow forcing of default language in rewrites
                    this._forcePortalDefaultLanguage = this.GetBooleanSetting(UsePortalDefaultLanguageSetting, true);
                }

                return this._forcePortalDefaultLanguage.Value;
            }

            internal set { this._forcePortalDefaultLanguage = value; }
        }

        public bool FriendlyAdminHostUrls
        {
            get
            {
                if (!this._friendlyAdminHostUrls.HasValue)
                {
                    this._friendlyAdminHostUrls = this.GetBooleanSetting(FriendlyAdminHostUrlsSetting, true);
                }

                return this._friendlyAdminHostUrls.Value;
            }

            internal set { this._friendlyAdminHostUrls = value; }
        }

        public string IgnoreRegex
        {
            get
            {
                return this._ignoreRegex ??
                       (this._ignoreRegex =
                           this.GetStringSetting(
                               IgnoreRegexSetting,
                               @"(?<!linkclick\.aspx.+)(?:(?<!\?.+)(\.pdf$|\.gif$|\.png($|\?)|\.css($|\?)|\.js($|\?)|\.jpg$|\.axd($|\?)|\.swf$|\.flv$|\.ico$|\.xml($|\?)|\.txt$))"));
            }

            internal set { this._ignoreRegex = value; }
        }

        public bool IncludePageName
        {
            get
            {
                if (!this._includePageName.HasValue)
                {
                    this._includePageName = this.GetBooleanSetting(IncludePageNameSetting, true);
                }

                return this._includePageName.Value;
            }

            internal set { this._includePageName = value; }
        }

        public string NoFriendlyUrlRegex
        {
            get
            {
                // 655 : new noFriendlyUrlRegex value to ignore generation of certain urls
                return this._noFriendlyUrlRegex ?? (this._noFriendlyUrlRegex = this.GetStringSetting(DoNotUseFriendlyUrlRegexSetting, @"/Rss\.aspx"));
            }

            internal set { this._noFriendlyUrlRegex = value; }
        }

        public string PageExtension
        {
            get { return this._pageExtension ?? (this._pageExtension = this.GetStringSetting(PageExtensionSetting, ".aspx")); }
            internal set { this._pageExtension = value; }
        }

        public PageExtensionUsageType PageExtensionUsageType
        {
            get
            {
                if (this._pageExtensionUsage == null)
                {
                    this._pageExtensionUsage = this.GetStringSetting(PageExtensionUsageSetting, PageExtensionUsageType.Never.ToString());
                }

                PageExtensionUsageType val;
                switch (this._pageExtensionUsage.ToLowerInvariant())
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
                    default:
                        val = PageExtensionUsageType.Never;
                        break;
                }

                return val;
            }

            internal set
            {
                var newValue = value.ToString();
                this._pageExtensionUsage = newValue;
            }
        }

        public bool RedirectDefaultPage
        {
            get
            {
                if (!this._redirectDefaultPage.HasValue)
                {
                    this._redirectDefaultPage = this.GetBooleanSetting(RedirectDefaultPageSetting, false);
                }

                return this._redirectDefaultPage.Value;
            }

            internal set { this._redirectUnfriendly = value; }
        }

        public bool RedirectOldProfileUrl
        {
            get
            {
                if (!this._redirectOldProfileUrl.HasValue)
                {
                    this._redirectOldProfileUrl = PortalController.GetPortalSettingAsBoolean(RedirectOldProfileUrlSetting, this.PortalId, true);
                }

                return this._redirectOldProfileUrl.Value;
            }

            internal set { this._redirectOldProfileUrl = value; }
        }

        public bool RedirectUnfriendly
        {
            get
            {
                if (!this._redirectUnfriendly.HasValue)
                {
                    this._redirectUnfriendly = this.GetBooleanSetting(RedirectUnfriendlySetting, true);
                }

                return this._redirectUnfriendly.Value;
            }

            internal set { this._redirectUnfriendly = value; }
        }

        public bool RedirectWrongCase
        {
            get
            {
                if (!this._redirectWrongCase.HasValue)
                {
                    this._redirectWrongCase = this.GetBooleanSetting(RedirectMixedCaseSetting, false);
                }

                return this._redirectWrongCase.Value;
            }

            internal set { this._redirectWrongCase = value; }
        }

        public string RegexMatch
        {
            get { return this._regexMatch ?? (this._regexMatch = this.GetStringSetting(ValidFriendlyUrlRegexSetting, @"[^\w\d _-]")); }
            internal set { this._regexMatch = value; }
        }

        public string ReplaceSpaceWith
        {
            get
            {
                // 791 : use threadlocking option
                return this._replaceSpaceWith ?? (this._replaceSpaceWith = this.GetStringSetting(ReplaceSpaceWithSetting, "-"));
            }

            internal set { this._replaceSpaceWith = value; }
        }

        public string SpaceEncodingValue
        {
            get { return this._spaceEncodingValue ?? (this._spaceEncodingValue = this.GetStringSetting(SpaceEncodingValueSetting, SpaceEncodingHex)); }
            internal set { this._spaceEncodingValue = value; }
        }

        public bool SSLClientRedirect
        {
            get
            {
                if (!this._sslClientRedirect.HasValue)
                {
                    this._sslClientRedirect = this.GetBooleanSetting(SslClientRedirectSetting, false);
                }

                return this._sslClientRedirect.Value;
            }

            internal set { this._sslClientRedirect = value; }
        }

        public int TabId404 { get; private set; }

        public int TabId500 { get; private set; }

        public string UrlFormat
        {
            get
            {
                return this._urlFormat ?? (this._urlFormat = this.GetStringSetting(UrlFormatSetting, "advanced"));
            }

            internal set { this._urlFormat = value; }
        }

        public string UseBaseFriendlyUrls
        {
            get
            {
                if (this._useBaseFriendlyUrls == null)
                {
                    this._useBaseFriendlyUrls = this.GetStringSetting(UseBaseFriendlyUrlsSetting, string.Empty);
                    if (!string.IsNullOrEmpty(this.UseBaseFriendlyUrls) && !this.UseBaseFriendlyUrls.EndsWith(";"))
                    {
                        this._useBaseFriendlyUrls += ";";
                    }
                }

                return this._useBaseFriendlyUrls;
            }

            internal set { this._useBaseFriendlyUrls = value; }
        }

        public string UseSiteUrlsRegex
        {
            get
            {
                return this._useSiteUrlsRegex ??
                       (this._useSiteUrlsRegex =
                           this.GetStringSetting(
                               SiteUrlsOnlyRegexSetting,
                               @"/rss\.aspx|Telerik.RadUploadProgressHandler\.ashx|BannerClickThrough\.aspx|(?:/[^/]+)*/Tabid/\d+/.*default\.aspx"));
            }

            internal set { this._useSiteUrlsRegex = value; }
        }

        public string ValidExtensionlessUrlsRegex
        {
            get
            {
                // 893 : new extensionless Urls check for validating urls which have no extension but aren't 404
                return this._validExtensionlessUrlsRegex ??
                       (this._validExtensionlessUrlsRegex = this.GetStringSetting(UrlsWithNoExtensionRegexSetting, @"\.asmx/|\.ashx/|\.svc/|\.aspx/|\.axd/"));
            }

            internal set { this._validExtensionlessUrlsRegex = value; }
        }

        public string InternalAliases
        {
            get
            {
                if (this._internalAliases == null)
                {
                    // allow for a list of internal aliases
                    this.InternalAliases = this.GetStringSetting(InternalAliasesSetting, string.Empty); // calls the setter
                }

                return this._internalAliases;
            }

            internal set
            {
                this._internalAliases = value;
                this.ParseInternalAliases(); // splits into list
            }
        }

        public string VanityUrlPrefix
        {
            get { return this._vanityUrlPrefix ?? (this._vanityUrlPrefix = this.GetStringSetting(VanityUrlPrefixSetting, "users")); }
            internal set { this._vanityUrlPrefix = value; }
        }

        internal List<string> PortalValues { get; private set; }

        private bool GetBooleanSetting(string key, bool defaultValue)
        {
            // First Get the Host Value using the passed in value as default
            var returnValue = this._hostControllerInstance.GetBoolean(key, defaultValue);

            if (this.PortalId > -1)
            {
                // Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSettingAsBoolean(key, this.PortalId, returnValue);
            }

            return returnValue;
        }

        private int GetIntegerSetting(string key, int defaultValue)
        {
            // First Get the Host Value using the passed in value as default
            var returnValue = this._hostControllerInstance.GetInteger(key, defaultValue);

            if (this.PortalId > -1)
            {
                // Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSettingAsInteger(key, this.PortalId, returnValue);
            }

            return returnValue;
        }

        private string GetStringSetting(string key, string defaultValue)
        {
            // First Get the Host Value using the passed in value as default
            var returnValue = this._hostControllerInstance.GetString(key, defaultValue);

            if (this.PortalId > -1)
            {
                // Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSetting(key, this.PortalId, returnValue);
            }

            return returnValue;
        }

        private void ParseInternalAliases()
        {
            if (!string.IsNullOrEmpty(this._internalAliases))
            {
                this.InternalAliasList = new List<InternalAlias>();
                var raw = this._internalAliases.Split(';');
                foreach (var rawVal in raw)
                {
                    if (rawVal.Length > 0)
                    {
                        var ia = new InternalAlias { HttpAlias = rawVal };
                        if (this.InternalAliasList.Contains(ia) == false)
                        {
                            this.InternalAliasList.Add(ia);
                        }
                    }
                }
            }
        }
    }
}
