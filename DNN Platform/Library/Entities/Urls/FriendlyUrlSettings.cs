// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

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
        private readonly IHostController hostControllerInstance = HostController.Instance;

        // 894 : new switch to disable custom url provider
        private bool? allowDebugCode;
        private bool? autoAsciiConvert;
        private bool? checkForDuplicateUrls;
        private bool? enableCustomProviders;
        private bool? forceLowerCase;
        private bool? forcePortalDefaultLanguage;
        private bool? friendlyAdminHostUrls;
        private bool? includePageName;
        private bool? logCacheMessages;
        private bool? redirectDefaultPage;
        private bool? redirectOldProfileUrl;
        private bool? redirectUnfriendly;
        private bool? redirectWrongCase;
        private bool? replaceDoubleChars;
        private bool? sslClientRedirect;
        private string deletedTabHandling;
        private string doNotIncludeInPathRegex;
        private string doNotRedirectRegex;
        private string doNotRedirectSecureRegex;
        private string doNotRewriteRegex;
        private string forceLowerCaseRegex;
        private string ignoreRegex;
        private string illegalChars;
        private string internalAliases;
        private string noFriendlyUrlRegex;
        private string pageExtension;
        private string pageExtensionUsage;
        private string regex404;
        private string regexMatch;
        private string replaceChars;
        private string replaceSpaceWith;
        private string spaceEncodingValue;
        private string url404;
        private string url500;
        private string urlFormat;
        private string useBaseFriendlyUrls;
        private string useSiteUrlsRegex;
        private string validExtensionlessUrlsRegex;
        private string vanityUrlPrefix;
        private TimeSpan? cacheTime;
        private List<string> processRequestList;
        private Dictionary<string, string> replaceCharacterDictionary;

        /// <summary>Initializes a new instance of the <see cref="FriendlyUrlSettings"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        public FriendlyUrlSettings(int portalId)
            : this(PortalController.Instance, portalId)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FriendlyUrlSettings"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        public FriendlyUrlSettings(IPortalController portalController, int portalId)
        {
            this.PortalId = portalId < -1 ? -1 : portalId;
            this.IsDirty = false;
            this.IsLoading = false;

            this.PortalValues = new List<string>();

            this.TabId500 = this.TabId404 = -1;

            if (portalId > -1)
            {
                var portal = portalController.GetPortal(portalId);
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
                if (this.processRequestList == null)
                {
                    var processRequests = this.GetStringSetting(ProcessRequestsSetting, null);
                    if (processRequests != null)
                    {
                        processRequests = processRequests.ToLowerInvariant();
                        this.processRequestList = !string.IsNullOrEmpty(processRequests)
                            ? new List<string>(processRequests.Split(';'))
                            : new List<string>();
                    }
                }

                return this.processRequestList;
            }
        }

        public bool AllowDebugCode
        {
            get
            {
                if (!this.allowDebugCode.HasValue)
                {
                    // 703 default debug code to false
                    this.allowDebugCode = Host.Host.DebugMode;
                }

                return this.allowDebugCode.Value;
            }
        }

        public TimeSpan CacheTime
        {
            get
            {
                if (!this.cacheTime.HasValue)
                {
                    this.cacheTime = new TimeSpan(0, this.GetIntegerSetting(CacheTimeSetting, 1440), 0);
                }

                return this.cacheTime.Value;
            }
        }

        public bool CheckForDuplicateUrls
        {
            get
            {
                if (!this.checkForDuplicateUrls.HasValue)
                {
                    // 793 : checkforDupUrls not being read
                    this.checkForDuplicateUrls = this.GetBooleanSetting(CheckForDuplicatedUrlsSetting, true);
                }

                return this.checkForDuplicateUrls.Value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public DNNPageForwardType ForwardExternalUrlsType => DNNPageForwardType.Redirect301;

        public bool EnableCustomProviders
        {
            get
            {
                if (!this.enableCustomProviders.HasValue)
                {
                    // 894 : new switch to disable custom providers if necessary
                    this.enableCustomProviders = this.GetBooleanSetting(EnableCustomProvidersSetting, true);
                }

                return this.enableCustomProviders.Value;
            }
        }

        public string IllegalChars
        {
            get
            {
                // 922 : new options for allowing user-configured replacement of characters
                return this.illegalChars ?? (this.illegalChars = this.GetStringSetting(IllegalCharsSetting, @"<>/\?:&=+|%#"));
            }
        }

        public bool LogCacheMessages
        {
            get
            {
                if (!this.logCacheMessages.HasValue)
                {
                    this.logCacheMessages = this.GetBooleanSetting(LogCacheMessagesSetting, false);
                }

                return this.logCacheMessages.Value;
            }
        }

        public string Regex404
        {
            get { return this.regex404 ?? (this.regex404 = this.GetStringSetting(Regex404Setting, string.Empty)); }
        }

        public Dictionary<string, string> ReplaceCharacterDictionary
        {
            get
            {
                if (this.replaceCharacterDictionary == null)
                {
                    var replaceCharwithChar = this.GetStringSetting(ReplaceCharWithCharSetting, string.Empty);
                    this.replaceCharacterDictionary = CollectionExtensions.CreateDictionaryFromString(replaceCharwithChar == "[]" ? string.Empty : replaceCharwithChar, ';', ',');
                }

                return this.replaceCharacterDictionary;
            }
        }

        public string ReplaceChars
        {
            get
            {
                // 922 : new options for allowing user-configured replacement of characters
                return this.replaceChars ?? (this.replaceChars = this.GetStringSetting(ReplaceCharsSetting, @" &$+,/?~#<>()¿¡«»!"""));
            }
        }

        public bool ReplaceDoubleChars
        {
            get
            {
                if (!this.replaceDoubleChars.HasValue)
                {
                    // 922 : new options for allowing user-configured replacement of characters
                    this.replaceDoubleChars = this.GetBooleanSetting(ReplaceDoubleCharsSetting, true);
                }

                return this.replaceDoubleChars.Value;
            }
        }

        public string Url404
        {
            get { return this.url404 ?? (this.url404 = this.GetStringSetting(Url404Setting, string.Empty)); }
        }

        public string Url500
        {
            get { return this.url500 ?? (this.url500 = this.GetStringSetting(Url500Setting, null) ?? this.Url404); }
        }

        public List<InternalAlias> InternalAliasList { get; private set; }

        public int PortalId { get; private set; }

        public bool IsDirty { get; private set; }

        public bool IsLoading { get; private set; }

        public bool AutoAsciiConvert
        {
            get
            {
                if (!this.autoAsciiConvert.HasValue)
                {
                    // urls to be modified in the output html stream
                    this.autoAsciiConvert = this.GetBooleanSetting(AutoAsciiConvertSetting, false);
                }

                return this.autoAsciiConvert.Value;
            }

            internal set
            {
                this.autoAsciiConvert = value;
            }
        }

        public DeletedTabHandlingType DeletedTabHandlingType
        {
            get
            {
                if (this.deletedTabHandling == null)
                {
                    this.deletedTabHandling = PortalController.GetPortalSetting(
                        DeletedTabHandlingTypeSetting, this.PortalId, DeletedTabHandlingType.Do404Error.ToString());
                }

                return "do301redirecttoportalhome".Equals(this.deletedTabHandling, StringComparison.InvariantCultureIgnoreCase)
                    ? DeletedTabHandlingType.Do301RedirectToPortalHome
                    : DeletedTabHandlingType.Do404Error;
            }

            internal set
            {
                var newValue = value.ToString();
                this.deletedTabHandling = newValue;
            }
        }

        public string DoNotIncludeInPathRegex
        {
            get
            {
                // 661 : do not include in path
                // 742 : was not reading and saving value when 'doNotIncludeInPathRegex' used
                // FUTURE: DNN 11.x Update to remove the runningDefault value
                return this.doNotIncludeInPathRegex ??
                       (this.doNotIncludeInPathRegex =
                           this.GetStringSetting(
                               KeepInQueryStringRegexSetting,
                               @"/nomo/\d+|/runningDefault/[^/]+|/popup/(?:true|false)|/(?:page|category|sort|tags)/[^/]+|tou/[^/]+|(/utm[^/]+/[^/]+)+"));
            }

            internal set
            {
                this.doNotIncludeInPathRegex = value;
            }
        }

        public string DoNotRedirectRegex
        {
            get
            {
                // 541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
                return this.doNotRedirectRegex ?? (this.doNotRedirectRegex = this.GetStringSetting(
                    DoNotRedirectUrlRegexSetting,
                    @"(\.axd)|/Rss\.aspx|/SiteMap\.aspx|\.ashx|/LinkClick\.aspx|/Providers/|/DesktopModules/|ctl=MobilePreview|/ctl/MobilePreview|/API/"));
            }

            internal set
            {
                this.doNotRedirectRegex = value;
            }
        }

        public string DoNotRedirectSecureRegex
        {
            get
            {
                // 541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
                return this.doNotRedirectSecureRegex ?? (this.doNotRedirectSecureRegex = this.GetStringSetting(DoNotRedirectHttpsUrlRegexSetting, string.Empty));
            }

            internal set
            {
                this.doNotRedirectSecureRegex = value;
            }
        }

        public string DoNotRewriteRegex
        {
            get
            {
                return this.doNotRewriteRegex ??
                       (this.doNotRewriteRegex =
                           this.GetStringSetting(DoNotRewriteRegExSetting, @"/DesktopModules/|/Providers/|/LinkClick\.aspx|/profilepic\.ashx|/DnnImageHandler\.ashx|/__browserLink/|/API/"));
            }

            internal set
            {
                this.doNotRewriteRegex = value;
            }
        }

        public bool ForceLowerCase
        {
            get
            {
                if (!this.forceLowerCase.HasValue)
                {
                    this.forceLowerCase = this.GetBooleanSetting(ForceLowerCaseSetting, false);
                }

                return this.forceLowerCase.Value;
            }

            internal set
            {
                this.forceLowerCase = value;
            }
        }

        public string ForceLowerCaseRegex
        {
            get { return this.forceLowerCaseRegex ?? (this.forceLowerCaseRegex = this.GetStringSetting(PreventLowerCaseUrlRegexSetting, @"\bverificationcode\b")); }
            internal set { this.forceLowerCaseRegex = value; }
        }

        public bool ForcePortalDefaultLanguage
        {
            get
            {
                if (!this.forcePortalDefaultLanguage.HasValue)
                {
                    // 810 : allow forcing of default language in rewrites
                    this.forcePortalDefaultLanguage = this.GetBooleanSetting(UsePortalDefaultLanguageSetting, true);
                }

                return this.forcePortalDefaultLanguage.Value;
            }

            internal set
            {
                this.forcePortalDefaultLanguage = value;
            }
        }

        public bool FriendlyAdminHostUrls
        {
            get
            {
                if (!this.friendlyAdminHostUrls.HasValue)
                {
                    this.friendlyAdminHostUrls = this.GetBooleanSetting(FriendlyAdminHostUrlsSetting, true);
                }

                return this.friendlyAdminHostUrls.Value;
            }

            internal set
            {
                this.friendlyAdminHostUrls = value;
            }
        }

        public string IgnoreRegex
        {
            get
            {
                return this.ignoreRegex ??
                       (this.ignoreRegex =
                           this.GetStringSetting(
                               IgnoreRegexSetting,
                               @"(?<!linkclick\.aspx.+)(?:(?<!\?.+)(\.pdf$|\.gif$|\.png($|\?)|\.css($|\?)|\.js($|\?)|\.jpg$|\.axd($|\?)|\.swf$|\.flv$|\.ico$|\.xml($|\?)|\.txt$))"));
            }

            internal set
            {
                this.ignoreRegex = value;
            }
        }

        public bool IncludePageName
        {
            get
            {
                if (!this.includePageName.HasValue)
                {
                    this.includePageName = this.GetBooleanSetting(IncludePageNameSetting, true);
                }

                return this.includePageName.Value;
            }

            internal set
            {
                this.includePageName = value;
            }
        }

        public string NoFriendlyUrlRegex
        {
            get
            {
                // 655 : new noFriendlyUrlRegex value to ignore generation of certain urls
                return this.noFriendlyUrlRegex ?? (this.noFriendlyUrlRegex = this.GetStringSetting(DoNotUseFriendlyUrlRegexSetting, @"/Rss\.aspx"));
            }

            internal set
            {
                this.noFriendlyUrlRegex = value;
            }
        }

        public string PageExtension
        {
            get { return this.pageExtension ?? (this.pageExtension = this.GetStringSetting(PageExtensionSetting, ".aspx")); }
            internal set { this.pageExtension = value; }
        }

        public PageExtensionUsageType PageExtensionUsageType
        {
            get
            {
                if (this.pageExtensionUsage == null)
                {
                    this.pageExtensionUsage = this.GetStringSetting(PageExtensionUsageSetting, PageExtensionUsageType.Never.ToString());
                }

                PageExtensionUsageType val;
                switch (this.pageExtensionUsage.ToLowerInvariant())
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
                this.pageExtensionUsage = newValue;
            }
        }

        public bool RedirectDefaultPage
        {
            get
            {
                if (!this.redirectDefaultPage.HasValue)
                {
                    this.redirectDefaultPage = this.GetBooleanSetting(RedirectDefaultPageSetting, false);
                }

                return this.redirectDefaultPage.Value;
            }

            internal set
            {
                this.redirectUnfriendly = value;
            }
        }

        public bool RedirectOldProfileUrl
        {
            get
            {
                if (!this.redirectOldProfileUrl.HasValue)
                {
                    this.redirectOldProfileUrl = PortalController.GetPortalSettingAsBoolean(RedirectOldProfileUrlSetting, this.PortalId, true);
                }

                return this.redirectOldProfileUrl.Value;
            }

            internal set
            {
                this.redirectOldProfileUrl = value;
            }
        }

        public bool RedirectUnfriendly
        {
            get
            {
                if (!this.redirectUnfriendly.HasValue)
                {
                    this.redirectUnfriendly = this.GetBooleanSetting(RedirectUnfriendlySetting, true);
                }

                return this.redirectUnfriendly.Value;
            }

            internal set
            {
                this.redirectUnfriendly = value;
            }
        }

        public bool RedirectWrongCase
        {
            get
            {
                if (!this.redirectWrongCase.HasValue)
                {
                    this.redirectWrongCase = this.GetBooleanSetting(RedirectMixedCaseSetting, false);
                }

                return this.redirectWrongCase.Value;
            }

            internal set
            {
                this.redirectWrongCase = value;
            }
        }

        public string RegexMatch
        {
            get { return this.regexMatch ?? (this.regexMatch = this.GetStringSetting(ValidFriendlyUrlRegexSetting, @"[^\w\d _-]")); }
            internal set { this.regexMatch = value; }
        }

        public string ReplaceSpaceWith
        {
            get
            {
                // 791 : use threadlocking option
                return this.replaceSpaceWith ?? (this.replaceSpaceWith = this.GetStringSetting(ReplaceSpaceWithSetting, "-"));
            }

            internal set
            {
                this.replaceSpaceWith = value;
            }
        }

        public string SpaceEncodingValue
        {
            get { return this.spaceEncodingValue ?? (this.spaceEncodingValue = this.GetStringSetting(SpaceEncodingValueSetting, SpaceEncodingHex)); }
            internal set { this.spaceEncodingValue = value; }
        }

        public bool SSLClientRedirect
        {
            get
            {
                if (!this.sslClientRedirect.HasValue)
                {
                    this.sslClientRedirect = this.GetBooleanSetting(SslClientRedirectSetting, false);
                }

                return this.sslClientRedirect.Value;
            }

            internal set
            {
                this.sslClientRedirect = value;
            }
        }

        public int TabId404 { get; private set; }

        public int TabId500 { get; private set; }

        public string UrlFormat
        {
            get
            {
                return this.urlFormat ?? (this.urlFormat = this.GetStringSetting(UrlFormatSetting, "advanced"));
            }

            internal set
            {
                this.urlFormat = value;
            }
        }

        public string UseBaseFriendlyUrls
        {
            get
            {
                if (this.useBaseFriendlyUrls == null)
                {
                    this.useBaseFriendlyUrls = this.GetStringSetting(UseBaseFriendlyUrlsSetting, string.Empty);
                    if (!string.IsNullOrEmpty(this.useBaseFriendlyUrls) && !this.useBaseFriendlyUrls.EndsWith(";"))
                    {
                        this.useBaseFriendlyUrls += ";";
                    }
                }

                return this.useBaseFriendlyUrls;
            }

            internal set
            {
                this.useBaseFriendlyUrls = value;
            }
        }

        public string UseSiteUrlsRegex
        {
            get
            {
                return this.useSiteUrlsRegex ??
                       (this.useSiteUrlsRegex =
                           this.GetStringSetting(
                               SiteUrlsOnlyRegexSetting,
                               @"/rss\.aspx|Telerik.RadUploadProgressHandler\.ashx|BannerClickThrough\.aspx|(?:/[^/]+)*/Tabid/\d+/.*default\.aspx"));
            }

            internal set
            {
                this.useSiteUrlsRegex = value;
            }
        }

        public string ValidExtensionlessUrlsRegex
        {
            get
            {
                // 893 : new extensionless Urls check for validating urls which have no extension but aren't 404
                return this.validExtensionlessUrlsRegex ??
                       (this.validExtensionlessUrlsRegex = this.GetStringSetting(UrlsWithNoExtensionRegexSetting, @"\.asmx/|\.ashx/|\.svc/|\.aspx/|\.axd/"));
            }

            internal set
            {
                this.validExtensionlessUrlsRegex = value;
            }
        }

        public string InternalAliases
        {
            get
            {
                if (this.internalAliases == null)
                {
                    // allow for a list of internal aliases
                    this.InternalAliases = this.GetStringSetting(InternalAliasesSetting, string.Empty); // calls the setter
                }

                return this.internalAliases;
            }

            internal set
            {
                this.internalAliases = value;
                this.ParseInternalAliases(); // splits into list
            }
        }

        public string VanityUrlPrefix
        {
            get { return this.vanityUrlPrefix ?? (this.vanityUrlPrefix = this.GetStringSetting(VanityUrlPrefixSetting, "users")); }
            internal set { this.vanityUrlPrefix = value; }
        }

        internal List<string> PortalValues { get; private set; }

        private bool GetBooleanSetting(string key, bool defaultValue)
        {
            // First Get the Host Value using the passed in value as default
            var returnValue = this.hostControllerInstance.GetBoolean(key, defaultValue);

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
            var returnValue = this.hostControllerInstance.GetInteger(key, defaultValue);

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
            var returnValue = this.hostControllerInstance.GetString(key, defaultValue);

            if (this.PortalId > -1)
            {
                // Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSetting(key, this.PortalId, returnValue);
            }

            return returnValue;
        }

        private void ParseInternalAliases()
        {
            if (!string.IsNullOrEmpty(this.internalAliases))
            {
                this.InternalAliasList = new List<InternalAlias>();
                var raw = this.internalAliases.Split(';');
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
