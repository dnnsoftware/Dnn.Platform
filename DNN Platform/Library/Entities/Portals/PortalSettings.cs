// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Tokens;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PortalSettings class encapsulates all of the settings for the Portal,
    /// as well as the configuration settings required to execute the current tab
    /// view within the portal.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public partial class PortalSettings : BaseEntityInfo, IPropertyAccess, IPortalSettings
    {
        public PortalSettings()
        {
            this.Registration = new RegistrationSettings();
        }

        public PortalSettings(int portalId)
            : this(Null.NullInteger, portalId)
        {
        }

        public PortalSettings(int tabId, int portalId)
        {
            this.PortalId = portalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            this.BuildPortalSettings(tabId, portal);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettings"/> class.
        /// The PortalSettings Constructor encapsulates all of the logic
        /// necessary to obtain configuration settings necessary to render
        /// a Portal Tab view for a given request.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///     <param name="tabId">The current tab.</param>
        ///     <param name="portalAliasInfo">The current portal.</param>
        /// -----------------------------------------------------------------------------
        public PortalSettings(int tabId, PortalAliasInfo portalAliasInfo)
        {
            this.PortalId = portalAliasInfo.PortalID;
            this.PortalAlias = portalAliasInfo;
            var portal = string.IsNullOrEmpty(portalAliasInfo.CultureCode) ?
                            PortalController.Instance.GetPortal(portalAliasInfo.PortalID)
                            : PortalController.Instance.GetPortal(portalAliasInfo.PortalID, portalAliasInfo.CultureCode);

            this.BuildPortalSettings(tabId, portal);
        }

        public PortalSettings(PortalInfo portal)
            : this(Null.NullInteger, portal)
        {
        }

        public PortalSettings(int tabId, PortalInfo portal)
        {
            this.PortalId = portal != null ? portal.PortalID : Null.NullInteger;
            this.BuildPortalSettings(tabId, portal);
        }

        public enum ControlPanelPermission
        {
            TabEditor,
            ModuleEditor,
        }

        public enum Mode
        {
            View,
            Edit,
            Layout,
        }

        public enum PortalAliasMapping
        {
            None,
            CanonicalUrl,
            Redirect,
        }

        public enum UserDeleteAction
        {
            Off = 0,
            Manual = 1,
            DelayedHardDelete = 2,
            HardDelete = 3,
        }

        public static PortalSettings Current
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public bool ControlPanelVisible
        {
            get
            {
                var setting = Convert.ToString(Personalization.GetProfile("Usability", "ControlPanelVisible" + this.PortalId));
                return string.IsNullOrEmpty(setting) ? this.DefaultControlPanelVisibility : Convert.ToBoolean(setting);
            }
        }

        public string DefaultPortalAlias
        {
            get
            {
                foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(this.PortalId).Where(alias => alias.IsPrimary))
                {
                    return alias.HTTPAlias;
                }

                return string.Empty;
            }
        }

        public PortalAliasMapping PortalAliasMappingMode
        {
            get
            {
                return PortalSettingsController.Instance().GetPortalAliasMappingMode(this.PortalId);
            }
        }

        /// <summary>Gets the currently logged in user identifier.</summary>
        /// <value>The user identifier.</value>
        public int UserId
        {
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
                {
                    return this.UserInfo.UserID;
                }

                return Null.NullInteger;
            }
        }

        /// <summary>Gets the currently logged in user.</summary>
        /// <value>The current user information.</value>
        public UserInfo UserInfo
        {
            get
            {
                return UserController.Instance.GetCurrentUserInfo();
            }
        }

        public Mode UserMode
        {
            get
            {
                Mode mode;
                if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
                {
                    mode = this.DefaultControlPanelMode;
                    string setting = Convert.ToString(Personalization.GetProfile("Usability", "UserMode" + this.PortalId));
                    switch (setting.ToUpper())
                    {
                        case "VIEW":
                            mode = Mode.View;
                            break;
                        case "EDIT":
                            mode = Mode.Edit;
                            break;
                        case "LAYOUT":
                            mode = Mode.Layout;
                            break;
                    }
                }
                else
                {
                    mode = Mode.View;
                }

                return mode;
            }
        }

        /// <summary>
        /// Gets a value indicating whether get a value indicating whether the current portal is in maintenance mode (if either this specific portal or the entire instance is locked). If locked, any actions which update the database should be disabled.
        /// </summary>
        public bool IsLocked
        {
            get { return this.IsThisPortalLocked || Host.Host.IsLocked; }
        }

        /// <summary>
        /// Gets a value indicating whether get a value indicating whether the current portal is in maintenance mode (note, the entire instance may still be locked, this only indicates whether this portal is specifically locked). If locked, any actions which update the database should be disabled.
        /// </summary>
        public bool IsThisPortalLocked
        {
            get { return PortalController.GetPortalSettingAsBoolean("IsLocked", this.PortalId, false); }
        }

        public string PageHeadText
        {
            get
            {
                // For New Install
                string pageHead = "<meta content=\"text/html; charset=UTF-8\" http-equiv=\"Content-Type\" />";
                string setting;
                if (PortalController.Instance.GetPortalSettings(this.PortalId).TryGetValue("PageHeadText", out setting))
                {
                    // Hack to store empty string portalsetting with non empty default value
                    pageHead = (setting == "false") ? string.Empty : setting;
                }

                return pageHead;
            }
        }

        /*
         * add <a name="[moduleid]"></a> on the top of the module
         *
         * Desactivate this remove the html5 compatibility warnings
         * (and make the output smaller)
         *
         */
        public bool InjectModuleHyperLink
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("InjectModuleHyperLink", this.PortalId, true);
            }
        }

        /*
         * generates a : Page.Response.AddHeader("X-UA-Compatible", "");
         *

         */
        public string AddCompatibleHttpHeader
        {
            get
            {
                string CompatibleHttpHeader = "IE=edge";
                string setting;
                if (PortalController.Instance.GetPortalSettings(this.PortalId).TryGetValue("AddCompatibleHttpHeader", out setting))
                {
                    // Hack to store empty string portalsetting with non empty default value
                    CompatibleHttpHeader = (setting == "false") ? string.Empty : setting;
                }

                return CompatibleHttpHeader;
            }
        }

        /// <summary>
        /// Gets a value indicating whether if true then add a cachebuster parameter to generated file URI's.
        /// </summary>
        public bool AddCachebusterToResourceUris
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("AddCachebusterToResourceUris", this.PortalId, true);
            }
        }

        /// <summary>
        /// Gets a value indicating whether if this is true, then regular users can't send message to specific user/group.
        /// </summary>
        public bool DisablePrivateMessage
        {
            get
            {
                return PortalController.GetPortalSetting("DisablePrivateMessage", this.PortalId, "N") == "Y";
            }
        }

        public TabInfo ActiveTab { get; set; }

        public int AdministratorId { get; set; }

        public int AdministratorRoleId { get; set; }

        public string AdministratorRoleName { get; set; }

        public int AdminTabId { get; set; }

        public string BackgroundFile { get; set; }

        public int BannerAdvertising { get; set; }

        public string CultureCode { get; set; }

        public string Currency { get; set; }

        public string DefaultLanguage { get; set; }

        public string Description { get; set; }

        public string Email { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string FooterText { get; set; }

        public Guid GUID { get; set; }

        public string HomeDirectory { get; set; }

        public string HomeSystemDirectory { get; set; }

        public int HomeTabId { get; set; }

        public float HostFee { get; set; }

        public int HostSpace { get; set; }

        public string KeyWords { get; set; }

        public int LoginTabId { get; set; }

        public string LogoFile { get; set; }

        public int PageQuota { get; set; }

        public int Pages { get; set; }

        public int PortalId { get; set; }

        public PortalAliasInfo PortalAlias { get; set; }

        public PortalAliasInfo PrimaryAlias { get; set; }

        public string PortalName { get; set; }

        public int RegisteredRoleId { get; set; }

        public string RegisteredRoleName { get; set; }

        public int RegisterTabId { get; set; }

        public RegistrationSettings Registration { get; set; }

        public int SearchTabId { get; set; }

        [Obsolete("Deprecated in 8.0.0. Scheduled removal in v10.0.0.")]
        public int SiteLogHistory { get; set; }

        public int SplashTabId { get; set; }

        public int SuperTabId { get; set; }

        public int UserQuota { get; set; }

        public int UserRegistration { get; set; }

        public int Users { get; set; }

        public int UserTabId { get; set; }

        public int TermsTabId { get; set; }

        public int PrivacyTabId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether allows users to select their own UI culture.
        /// When set to false (default) framework will allways same culture for both
        /// CurrentCulture (content) and CurrentUICulture (interface).
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool AllowUserUICulture { get; internal set; }

        public int CdfVersion { get; internal set; }

        public bool ContentLocalizationEnabled { get; internal set; }

        public ControlPanelPermission ControlPanelSecurity { get; internal set; }

        public string DefaultAdminContainer { get; internal set; }

        public string DefaultAdminSkin { get; internal set; }

        public string DefaultAuthProvider { get; internal set; }

        public Mode DefaultControlPanelMode { get; internal set; }

        public bool DefaultControlPanelVisibility { get; internal set; }

        public string DefaultIconLocation { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Module Id.
        /// </summary>
        /// <remarks>Defaults to Null.NullInteger.</remarks>
        /// -----------------------------------------------------------------------------
        public int DefaultModuleId { get; internal set; }

        public string DefaultModuleActionMenu { get; internal set; }

        public string DefaultPortalContainer { get; internal set; }

        public string DefaultPortalSkin { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Tab Id.
        /// </summary>
        /// <remarks>Defaults to Null.NullInteger.</remarks>
        /// -----------------------------------------------------------------------------
        public int DefaultTabId { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether Browser Language Detection is Enabled.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableBrowserLanguage { get; internal set; }

        public bool EnableCompositeFiles { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to use the module effect in edit mode.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
        public bool EnableModuleEffect { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to use the popup.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnablePopUps { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether website Administrator whether receive the notification email when new user register.
        /// </summary>
        public bool EnableRegisterNotification { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Skin Widgets are enabled/supported.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableSkinWidgets { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether a cookie consent popup should be shown.
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool ShowCookieConsent { get; internal set; }

        /// <summary>
        /// Gets link for the user to find out more about cookies. If not specified the link
        /// shown will point to cookiesandyou.com.
        /// </summary>
        public string CookieMoreLink { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether enable url language.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableUrlLanguage { get; internal set; }

        public int ErrorPage404 { get; internal set; }

        public int ErrorPage500 { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets a value indicating whether gets whether folders which are hidden or whose name begins with underscore
        ///   are included in folder synchronization.
        /// </summary>
        /// <remarks>
        ///   Defaults to True.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool HideFoldersEnabled { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether hide the login link.
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool HideLoginControl { get; internal set; }

        public string HomeDirectoryMapPath { get; internal set; }

        public string HomeSystemDirectoryMapPath { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Inline Editor is enabled.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// -----------------------------------------------------------------------------
        public bool InlineEditorEnabled { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to inlcude Common Words in the Search Index.
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeCommon { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether to inlcude Numbers in the Search Index.
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeNumeric { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the filter used for inclusion of tag info.
        /// </summary>
        /// <remarks>
        ///   Defaults to "".
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string SearchIncludedTagInfoFilter { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum Search Word length to index.
        /// </summary>
        /// <remarks>Defaults to 3.</remarks>
        /// -----------------------------------------------------------------------------
        public int SearchMaxWordlLength { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the minum Search Word length to index.
        /// </summary>
        /// <remarks>Defaults to 3.</remarks>
        /// -----------------------------------------------------------------------------
        public int SearchMinWordlLength { get; internal set; }

        public bool SSLEnabled { get; internal set; }

        public bool SSLEnforced { get; internal set; }

        public string SSLURL { get; internal set; }

        public string STDURL { get; internal set; }

        public int SMTPConnectionLimit { get; internal set; }

        public int SMTPMaxIdleTime { get; internal set; }

        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// Gets a value indicating whether if true then all users will be pushed through the data consent workflow.
        /// </summary>
        public bool DataConsentActive { get; internal set; }

        /// <summary>
        /// Gets last time the terms and conditions have been changed. This will determine if the user needs to
        /// reconsent or not. Legally once the terms have changed, users need to sign again. This value is set
        /// by the "reset consent" button on the UI.
        /// </summary>
        public DateTime DataConsentTermsLastChange { get; internal set; }

        /// <summary>
        /// Gets if set this is a tab id of a page where the user will be redirected to for consent. If not set then
        /// the platform's default logic is used.
        /// </summary>
        public int DataConsentConsentRedirect { get; internal set; }

        /// <summary>
        /// Gets what should happen to the user account if a user has been deleted. This is important as
        /// under certain circumstances you may be required by law to destroy the user's data within a
        /// certain timeframe after a user has requested deletion.
        /// </summary>
        public UserDeleteAction DataConsentUserDeleteAction { get; internal set; }

        /// <summary>
        /// Gets the delay time (in conjunction with DataConsentDelayMeasurement) for the DataConsentUserDeleteAction.
        /// </summary>
        public int DataConsentDelay { get; internal set; }

        /// <summary>
        /// Gets units for DataConsentDelay.
        /// </summary>
        public string DataConsentDelayMeasurement { get; internal set; }

        /// <summary>
        /// Gets whitelist of file extensions for end users.
        /// </summary>
        public FileExtensionWhitelist AllowedExtensionsWhitelist { get; internal set; }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            var outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }

            var lowerPropertyName = propertyName.ToLowerInvariant();
            if (accessLevel == Scope.NoSettings)
            {
                propertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }

            propertyNotFound = true;
            var result = string.Empty;
            var isPublic = true;
            switch (lowerPropertyName)
            {
                case "scheme":
                    propertyNotFound = false;
                    result = SSLEnabled ? "https" : "http";
                    break;
                case "url":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.PortalAlias.HTTPAlias, format);
                    break;
                case "fullurl": // return portal alias with protocol - note this depends on HttpContext
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Globals.AddHTTP(this.PortalAlias.HTTPAlias), format);
                    break;
                case "passwordreminderurl": // if regsiter page defined in portal settings, then get that page url, otherwise return home page. - note this depends on HttpContext
                    propertyNotFound = false;
                    var reminderUrl = Globals.AddHTTP(this.PortalAlias.HTTPAlias);
                    if (this.RegisterTabId > Null.NullInteger)
                    {
                        reminderUrl = Globals.RegisterURL(string.Empty, string.Empty);
                    }

                    result = PropertyAccess.FormatString(reminderUrl, format);
                    break;
                case "portalid":
                    propertyNotFound = false;
                    result = this.PortalId.ToString(outputFormat, formatProvider);
                    break;
                case "portalname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.PortalName, format);
                    break;
                case "homedirectory":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.HomeDirectory, format);
                    break;
                case "homedirectorymappath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.HomeDirectoryMapPath, format);
                    break;
                case "logofile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.LogoFile, format);
                    break;
                case "footertext":
                    propertyNotFound = false;
                    var footerText = this.FooterText.Replace("[year]", DateTime.Now.Year.ToString());
                    result = PropertyAccess.FormatString(footerText, format);
                    break;
                case "expirydate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ExpiryDate.ToString(outputFormat, formatProvider);
                    break;
                case "userregistration":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.UserRegistration.ToString(outputFormat, formatProvider);
                    break;
                case "banneradvertising":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.BannerAdvertising.ToString(outputFormat, formatProvider);
                    break;
                case "currency":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Currency, format);
                    break;
                case "administratorid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.AdministratorId.ToString(outputFormat, formatProvider);
                    break;
                case "email":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Email, format);
                    break;
                case "hostfee":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.HostFee.ToString(outputFormat, formatProvider);
                    break;
                case "hostspace":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.HostSpace.ToString(outputFormat, formatProvider);
                    break;
                case "pagequota":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.PageQuota.ToString(outputFormat, formatProvider);
                    break;
                case "userquota":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.UserQuota.ToString(outputFormat, formatProvider);
                    break;
                case "administratorroleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.AdministratorRoleId.ToString(outputFormat, formatProvider);
                    break;
                case "administratorrolename":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.AdministratorRoleName, format);
                    break;
                case "registeredroleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.RegisteredRoleId.ToString(outputFormat, formatProvider);
                    break;
                case "registeredrolename":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.RegisteredRoleName, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Description, format);
                    break;
                case "keywords":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.KeyWords, format);
                    break;
                case "backgroundfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.BackgroundFile, format);
                    break;
                case "admintabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.AdminTabId.ToString(outputFormat, formatProvider);
                    break;
                case "supertabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.SuperTabId.ToString(outputFormat, formatProvider);
                    break;
                case "splashtabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.SplashTabId.ToString(outputFormat, formatProvider);
                    break;
                case "hometabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.HomeTabId.ToString(outputFormat, formatProvider);
                    break;
                case "logintabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.LoginTabId.ToString(outputFormat, formatProvider);
                    break;
                case "registertabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.RegisterTabId.ToString(outputFormat, formatProvider);
                    break;
                case "usertabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.UserTabId.ToString(outputFormat, formatProvider);
                    break;
                case "defaultlanguage":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DefaultLanguage, format);
                    break;
                case "users":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.Users.ToString(outputFormat, formatProvider);
                    break;
                case "pages":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.Pages.ToString(outputFormat, formatProvider);
                    break;
                case "contentvisible":
                    isPublic = false;
                    break;
                case "controlpanelvisible":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.ControlPanelVisible, formatProvider);
                    break;
            }

            if (!isPublic && accessLevel != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }

            return result;
        }

        public PortalSettings Clone()
        {
            return (PortalSettings)this.MemberwiseClone();
        }

        private void BuildPortalSettings(int tabId, PortalInfo portal)
        {
            PortalSettingsController.Instance().LoadPortalSettings(this);

            if (portal == null)
            {
                return;
            }

            PortalSettingsController.Instance().LoadPortal(portal, this);

            var key = string.Join(":", "ActiveTab", portal.PortalID.ToString(), tabId.ToString());
            var items = HttpContext.Current != null ? HttpContext.Current.Items : null;
            if (items != null && items.Contains(key))
            {
                this.ActiveTab = items[key] as TabInfo;
            }
            else
            {
                this.ActiveTab = PortalSettingsController.Instance().GetActiveTab(tabId, this);
                if (items != null && this.ActiveTab != null)
                {
                    items[key] = this.ActiveTab;
                }
            }
        }
    }
}
