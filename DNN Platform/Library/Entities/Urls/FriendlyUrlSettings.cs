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

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Entities.Urls
{
    [Serializable]
    public class FriendlyUrlSettings
    {
        #region Private Members

        private readonly IHostController _hostControllerInstance = HostController.Instance;

        //894 : new switch to disable custom url provider 
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

        #endregion

        #region private helper methods

        internal List<string> PortalValues { get; private set; }

        #endregion

        #region Constants

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

        #endregion

        #region Public Properties

        public List<InternalAlias> InternalAliasList { get; private set; }

        public List<string> ProcessRequestList
        {
            get
            {
                if (_processRequestList == null)
                {
                    var processRequests = GetStringSetting(ProcessRequestsSetting, null);
                    if (processRequests != null)
                    {
                        processRequests = processRequests.ToLower();
                        _processRequestList = !string.IsNullOrEmpty(processRequests)
                            ? new List<string>(processRequests.Split(';'))
                            : new List<string>();
                    }
                }

                return _processRequestList;
            }
        }

        public int PortalId { get; private set; }

        public bool IsDirty { get; private set; }

        public bool IsLoading { get; private set; }

        public bool AllowDebugCode
        {
            get
            {
                if (!_allowDebugCode.HasValue)
                {
                    //703 default debug code to false
                    _allowDebugCode = Host.Host.DebugMode;
                }

                return _allowDebugCode.Value;
            }
        }

        public bool AutoAsciiConvert
        {
            get
            {
                if (!_autoAsciiConvert.HasValue)
                {
                    //urls to be modified in the output html stream
                    _autoAsciiConvert = GetBooleanSetting(AutoAsciiConvertSetting, false);
                }
                return _autoAsciiConvert.Value;
            }
            internal set { _autoAsciiConvert = value; }
        }

        public TimeSpan CacheTime
        {
            get
            {
                if (!_cacheTime.HasValue)
                {
                    _cacheTime = new TimeSpan(0, GetIntegerSetting(CacheTimeSetting, 1440), 0);
                }
                return _cacheTime.Value;
            }
        }

        public bool CheckForDuplicateUrls
        {
            get
            {
                if (!_checkForDuplicateUrls.HasValue)
                {
                    //793 : checkforDupUrls not being read
                    _checkForDuplicateUrls = GetBooleanSetting(CheckForDuplicatedUrlsSetting, true);
                }
                return _checkForDuplicateUrls.Value;
            }
        }

        public DeletedTabHandlingType DeletedTabHandlingType
        {
            get
            {
                if (_deletedTabHandling == null)
                {
                    _deletedTabHandling = PortalController.GetPortalSetting(
                        DeletedTabHandlingTypeSetting, PortalId, DeletedTabHandlingType.Do404Error.ToString());
                }

                return "do301redirecttoportalhome".Equals(_deletedTabHandling, StringComparison.InvariantCultureIgnoreCase)
                    ? DeletedTabHandlingType.Do301RedirectToPortalHome
                    : DeletedTabHandlingType.Do404Error;
            }
            internal set
            {
                var newValue = value.ToString();
                _deletedTabHandling = newValue;
            }
        }

        public string DoNotIncludeInPathRegex
        {
            get
            {
                //661 : do not include in path
                //742 : was not reading and saving value when 'doNotIncludeInPathRegex' used
                return _doNotIncludeInPathRegex ??
                       (_doNotIncludeInPathRegex =
                           GetStringSetting(KeepInQueryStringRegexSetting,
                               @"/nomo/\d+|/runningDefault/[^/]+|/popup/(?:true|false)|/(?:page|category|sort|tags)/[^/]+|tou/[^/]+|(/utm[^/]+/[^/]+)+"));
            }
            internal set { _doNotIncludeInPathRegex = value; }
        }

        public string DoNotRedirectRegex
        {
            get
            {
                //541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
                return _doNotRedirectRegex ?? (_doNotRedirectRegex = GetStringSetting(DoNotRedirectUrlRegexSetting,
                    @"(\.axd)|/Rss\.aspx|/SiteMap\.aspx|\.ashx|/LinkClick\.aspx|/Providers/|/DesktopModules/|ctl=MobilePreview|/ctl/MobilePreview|/API/"));
            }
            internal set { _doNotRedirectRegex = value; }
        }

        public string DoNotRedirectSecureRegex
        {
            get
            {
                //541 moved doNotRedirect and doNotRedirectRegex from under 'redirectUnfriendly' code
                return _doNotRedirectSecureRegex ?? (_doNotRedirectSecureRegex = GetStringSetting(DoNotRedirectHttpsUrlRegexSetting, string.Empty));
            }
            internal set { _doNotRedirectSecureRegex = value; }
        }

        public string DoNotRewriteRegex
        {
            get
            {
                return _doNotRewriteRegex ??
                       (_doNotRewriteRegex =
                           GetStringSetting(DoNotRewriteRegExSetting, @"/DesktopModules/|/Providers/|/LinkClick\.aspx|/profilepic\.ashx|/DnnImageHandler\.ashx|/__browserLink/|/API/"));
            }
            internal set { _doNotRewriteRegex = value; }
        }

        public bool ForceLowerCase
        {
            get
            {
                if (!_forceLowerCase.HasValue)
                {
                    _forceLowerCase = GetBooleanSetting(ForceLowerCaseSetting, false);
                }
                return _forceLowerCase.Value;
            }
            internal set { _forceLowerCase = value; }
        }

        public string ForceLowerCaseRegex
        {
            get { return _forceLowerCaseRegex ?? (_forceLowerCaseRegex = GetStringSetting(PreventLowerCaseUrlRegexSetting, string.Empty)); }
            internal set { _forceLowerCaseRegex = value; }
        }

        public bool ForcePortalDefaultLanguage
        {
            get
            {
                if (!_forcePortalDefaultLanguage.HasValue)
                {
                    //810 : allow forcing of default language in rewrites
                    _forcePortalDefaultLanguage = GetBooleanSetting(UsePortalDefaultLanguageSetting, true);
                }
                return _forcePortalDefaultLanguage.Value;
            }
            internal set { _forcePortalDefaultLanguage = value; }
        }

        public DNNPageForwardType ForwardExternalUrlsType
        {
            get
            {
                return DNNPageForwardType.Redirect301;
            }
        }

        public bool FriendlyAdminHostUrls
        {
            get
            {
                if (!_friendlyAdminHostUrls.HasValue)
                {
                    _friendlyAdminHostUrls = GetBooleanSetting(FriendlyAdminHostUrlsSetting, true);
                }
                return _friendlyAdminHostUrls.Value;
            }
            internal set { _friendlyAdminHostUrls = value; }
        }

        public bool EnableCustomProviders
        {
            get
            {
                if (!_enableCustomProviders.HasValue)
                {
                    //894 : new switch to disable custom providers if necessary
                    _enableCustomProviders = GetBooleanSetting(EnableCustomProvidersSetting, true);
                }
                return _enableCustomProviders.Value;
            }
        }

        public string IgnoreRegex
        {
            get
            {
                return _ignoreRegex ??
                       (_ignoreRegex =
                           GetStringSetting(IgnoreRegexSetting,
                               @"(?<!linkclick\.aspx.+)(?:(?<!\?.+)(\.pdf$|\.gif$|\.png($|\?)|\.css($|\?)|\.js($|\?)|\.jpg$|\.axd($|\?)|\.swf$|\.flv$|\.ico$|\.xml($|\?)|\.txt$))"));
            }
            internal set { _ignoreRegex = value; }
        }

        public string IllegalChars
        {
            get
            {
                //922 : new options for allowing user-configured replacement of characters
                return _illegalChars ?? (_illegalChars = GetStringSetting(IllegalCharsSetting, @"<>/\?:&=+|%#"));
            }
        }

        public bool IncludePageName
        {
            get
            {
                if (!_includePageName.HasValue)
                {
                    _includePageName = GetBooleanSetting(IncludePageNameSetting, true);
                }

                return _includePageName.Value;
            }
            internal set { _includePageName = value; }
        }

        public bool LogCacheMessages
        {
            get
            {
                if (!_logCacheMessages.HasValue)
                {
                    _logCacheMessages = GetBooleanSetting(LogCacheMessagesSetting, false);
                }
                return _logCacheMessages.Value;
            }
        }

        public string NoFriendlyUrlRegex
        {
            get
            {
                //655 : new noFriendlyUrlRegex value to ignore generation of certain urls
                return _noFriendlyUrlRegex ?? (_noFriendlyUrlRegex = GetStringSetting(DoNotUseFriendlyUrlRegexSetting, @"/Rss\.aspx"));
            }
            internal set { _noFriendlyUrlRegex = value; }
        }

        public string PageExtension
        {
            get { return _pageExtension ?? (_pageExtension = GetStringSetting(PageExtensionSetting, ".aspx")); }
            internal set { _pageExtension = value; }
        }

        public PageExtensionUsageType PageExtensionUsageType
        {
            get
            {
                if (_pageExtensionUsage == null)
                {
                    _pageExtensionUsage = GetStringSetting(PageExtensionUsageSetting, PageExtensionUsageType.Never.ToString());
                }

                PageExtensionUsageType val;
                switch (_pageExtensionUsage.ToLowerInvariant())
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
                _pageExtensionUsage = newValue;
            }
        }

        public bool RedirectDefaultPage
        {
            get
            {
                if (!_redirectDefaultPage.HasValue)
                {
                    _redirectDefaultPage = GetBooleanSetting(RedirectDefaultPageSetting, false);
                }
                return _redirectDefaultPage.Value;
            }
            internal set { _redirectUnfriendly = value; }
        }

        public bool RedirectOldProfileUrl
        {
            get
            {
                if (!_redirectOldProfileUrl.HasValue)
                {
                    _redirectOldProfileUrl = PortalController.GetPortalSettingAsBoolean(RedirectOldProfileUrlSetting, PortalId, true);
                }
                return _redirectOldProfileUrl.Value;
            }
            internal set { _redirectOldProfileUrl = value; }
        }

        public bool RedirectUnfriendly
        {
            get
            {
                if (!_redirectUnfriendly.HasValue)
                {
                    _redirectUnfriendly = GetBooleanSetting(RedirectUnfriendlySetting, true);
                }
                return _redirectUnfriendly.Value;
            }
            internal set { _redirectUnfriendly = value; }
        }

        public bool RedirectWrongCase
        {
            get
            {
                if (!_redirectWrongCase.HasValue)
                {
                    _redirectWrongCase = GetBooleanSetting(RedirectMixedCaseSetting, false);
                }
                return _redirectWrongCase.Value;
            }
            internal set { _redirectWrongCase = value; }
        }

        public string Regex404
        {
            get { return _regex404 ?? (_regex404 = GetStringSetting(Regex404Setting, string.Empty)); }
        }

        public string RegexMatch
        {
            get { return _regexMatch ?? (_regexMatch = GetStringSetting(ValidFriendlyUrlRegexSetting, @"[^\w\d _-]")); }
            internal set { _regexMatch = value; }
        }

        public Dictionary<string, string> ReplaceCharacterDictionary
        {
            get
            {
                if (_replaceCharacterDictionary == null)
                {
                    var replaceCharwithChar = GetStringSetting(ReplaceCharWithCharSetting, string.Empty);
                    _replaceCharacterDictionary = CollectionExtensions.CreateDictionaryFromString(replaceCharwithChar == "[]" ? string.Empty : replaceCharwithChar, ';', ',');
                }
                return _replaceCharacterDictionary;
            }
        }

        public string ReplaceChars
        {
            get
            {
                //922 : new options for allowing user-configured replacement of characters
                return _replaceChars ?? (_replaceChars = GetStringSetting(ReplaceCharsSetting, @" &$+,/?~#<>()¿¡«»!"""));
            }
        }

        public bool ReplaceDoubleChars
        {
            get
            {
                if (!_replaceDoubleChars.HasValue)
                {
                    //922 : new options for allowing user-configured replacement of characters
                    _replaceDoubleChars = GetBooleanSetting(ReplaceDoubleCharsSetting, true);
                }
                return _replaceDoubleChars.Value;
            }
        }

        public string ReplaceSpaceWith
        {
            get
            {
                //791 : use threadlocking option
                return _replaceSpaceWith ?? (_replaceSpaceWith = GetStringSetting(ReplaceSpaceWithSetting, "-"));
            }
            internal set { _replaceSpaceWith = value; }
        }

        public string SpaceEncodingValue
        {
            get { return _spaceEncodingValue ?? (_spaceEncodingValue = GetStringSetting(SpaceEncodingValueSetting, SpaceEncodingHex)); }
            internal set { _spaceEncodingValue = value; }
        }

        public bool SSLClientRedirect
        {
            get
            {
                if (!_sslClientRedirect.HasValue)
                {
                    _sslClientRedirect = GetBooleanSetting(SslClientRedirectSetting, false);
                }
                return _sslClientRedirect.Value;
            }
            internal set { _sslClientRedirect = value; }
        }

        public int TabId404 { get; private set; }

        public int TabId500 { get; private set; }

        public string Url404
        {
            get { return _url404 ?? (_url404 = GetStringSetting(Url404Setting, string.Empty)); }
        }

        public string Url500
        {
            get { return _url500 ?? (_url500 = GetStringSetting(Url500Setting, null) ?? Url404); }
        }

        public string UrlFormat
        {
            get
            {
                return _urlFormat ?? (_urlFormat = GetStringSetting(UrlFormatSetting, "advanced"));
            }
            internal set { _urlFormat = value; }
        }

        public string UseBaseFriendlyUrls
        {
            get
            {
                if (_useBaseFriendlyUrls == null)
                {
                    _useBaseFriendlyUrls = GetStringSetting(UseBaseFriendlyUrlsSetting, string.Empty);
                    if (!string.IsNullOrEmpty(UseBaseFriendlyUrls) && !UseBaseFriendlyUrls.EndsWith(";"))
                    {
                        _useBaseFriendlyUrls += ";";
                    }
                }
                return _useBaseFriendlyUrls;
            }
            internal set { _useBaseFriendlyUrls = value; }
        }

        public string UseSiteUrlsRegex
        {
            get
            {
                return _useSiteUrlsRegex ??
                       (_useSiteUrlsRegex =
                           GetStringSetting(SiteUrlsOnlyRegexSetting,
                               @"/rss\.aspx|Telerik.RadUploadProgressHandler\.ashx|BannerClickThrough\.aspx|(?:/[^/]+)*/Tabid/\d+/.*default\.aspx"));
            }
            internal set { _useSiteUrlsRegex = value; }
        }

        public string ValidExtensionlessUrlsRegex
        {
            get
            {
                //893 : new extensionless Urls check for validating urls which have no extension but aren't 404
                return _validExtensionlessUrlsRegex ??
                       (_validExtensionlessUrlsRegex = GetStringSetting(UrlsWithNoExtensionRegexSetting, @"\.asmx/|\.ashx/|\.svc/|\.aspx/|\.axd/"));
            }
            internal set { _validExtensionlessUrlsRegex = value; }
        }

        public string InternalAliases
        {
            get
            {
                if (_internalAliases == null)
                {
                    // allow for a list of internal aliases
                    InternalAliases = GetStringSetting(InternalAliasesSetting, string.Empty); // calls the setter
                }
                return _internalAliases;
            }

            internal set
            {
                _internalAliases = value;
                ParseInternalAliases(); //splits into list
            }
        }

        public string VanityUrlPrefix
        {
            get { return _vanityUrlPrefix ?? (_vanityUrlPrefix = GetStringSetting(VanityUrlPrefixSetting, "users")); }
            internal set { _vanityUrlPrefix = value; }
        }

        #endregion

        #region initialization methods

        private bool GetBooleanSetting(string key, bool defaultValue)
        {
            //First Get the Host Value using the passed in value as default
            var returnValue = _hostControllerInstance.GetBoolean(key, defaultValue);

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
            var returnValue = _hostControllerInstance.GetInteger(key, defaultValue);

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
            var returnValue = _hostControllerInstance.GetString(key, defaultValue);

            if (PortalId > -1)
            {
                //Next check if there is a Portal Value, using the Host value as default
                returnValue = PortalController.GetPortalSetting(key, PortalId, returnValue);
            }

            return returnValue;
        }

        #endregion

        #region others

        public FriendlyUrlSettings(int portalId)
        {
            PortalId = portalId < -1 ? -1 : portalId;
            IsDirty = false;
            IsLoading = false;

            PortalValues = new List<string>();

            TabId500 = TabId404 = -1;

            if (portalId > -1)
            {
                var portal = PortalController.Instance.GetPortal(portalId);
                TabId500 = TabId404 = portal.Custom404TabId;

                if (TabId500 == -1)
                {
                    TabId500 = TabId404;
                }
            }
        }

        private void ParseInternalAliases()
        {
            if (!string.IsNullOrEmpty(_internalAliases))
            {
                InternalAliasList = new List<InternalAlias>();
                var raw = _internalAliases.Split(';');
                foreach (var rawVal in raw)
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