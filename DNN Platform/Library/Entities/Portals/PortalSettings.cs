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

    /// <summary>
    /// The PortalSettings class encapsulates all of the settings for the Portal,
    /// as well as the configuration settings required to execute the current tab
    /// view within the portal.
    /// </summary>
    [Serializable]
    public partial class PortalSettings : BaseEntityInfo, IPropertyAccess, IPortalSettings
    {
        /// <summary>Initializes a new instance of the <see cref="PortalSettings"/> class.</summary>
        public PortalSettings()
        {
            this.Registration = new RegistrationSettings();
        }

        /// <summary>Initializes a new instance of the <see cref="PortalSettings"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        public PortalSettings(int portalId)
            : this(Null.NullInteger, portalId)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalSettings"/> class.</summary>
        /// <param name="tabId">The active tab ID.</param>
        /// <param name="portalId">The portal ID.</param>
        public PortalSettings(int tabId, int portalId)
        {
            this.PortalId = portalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            this.BuildPortalSettings(tabId, portal);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettings"/> class.
        /// The PortalSettings Constructor encapsulates all of the logic
        /// necessary to obtain configuration settings necessary to render
        /// a Portal Tab view for a given request.
        /// </summary>
        /// <param name="tabId">The current tab.</param>
        /// <param name="portalAliasInfo">The current portal.</param>
        public PortalSettings(int tabId, PortalAliasInfo portalAliasInfo)
        {
            this.PortalId = portalAliasInfo.PortalID;
            this.PortalAlias = portalAliasInfo;
            var portal = string.IsNullOrEmpty(portalAliasInfo.CultureCode) ?
                            PortalController.Instance.GetPortal(portalAliasInfo.PortalID)
                            : PortalController.Instance.GetPortal(portalAliasInfo.PortalID, portalAliasInfo.CultureCode);

            this.BuildPortalSettings(tabId, portal);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalSettings"/> class.</summary>
        /// <param name="portal">The portal info.</param>
        public PortalSettings(PortalInfo portal)
            : this(Null.NullInteger, portal)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalSettings"/> class.</summary>
        /// <param name="tabId">The active tab ID.</param>
        /// <param name="portal">The portal info.</param>
        public PortalSettings(int tabId, PortalInfo portal)
        {
            this.PortalId = portal != null ? portal.PortalID : Null.NullInteger;
            this.BuildPortalSettings(tabId, portal);
        }

        public enum ControlPanelPermission
        {
            /// <summary>A page editor.</summary>
            TabEditor = 0,

            /// <summary>A module editor.</summary>
            ModuleEditor = 1,
        }

        /// <summary>Enumerates the possible view modes of a page.</summary>
        public enum Mode
        {
            /// <summary>The user is viewing the page in normal mode like a visitor.</summary>
            View = 0,

            /// <summary>The user is editing the page.</summary>
            Edit = 1,

            /// <summary>The user is viewing the page in layout mode.</summary>
            Layout = 2,
        }

        public enum PortalAliasMapping
        {
            /// <summary>No mapping.</summary>
            None = 0,

            /// <summary>Add a <c>rel="canonical"</c> link for the primary alias.</summary>
            CanonicalUrl = 1,

            /// <summary>Redirect to the primary alias.</summary>
            Redirect = 2,
        }

        public enum UserDeleteAction
        {
            /// <summary>Soft delete without an option to hard delete.</summary>
            Off = 0,

            /// <summary>Soft delete with the option to manually hard delete.</summary>
            Manual = 1,

            /// <summary>Hard delete after a delay.</summary>
            DelayedHardDelete = 2,

            /// <summary>Always hard delete.</summary>
            HardDelete = 3,
        }

        public enum CspMode
        {
            /// <summary>Content Security Header is not added.</summary>
            Off = 0,

            /// <summary>Content Security Header is not added in Report Only.</summary>
            ReportOnly = 1,

            /// <summary>Content Security Header is added.</summary>
            On = 2,
        }

        public static PortalSettings Current
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        /// <inheritdoc/>
        public bool ControlPanelVisible
        {
            get
            {
                var setting = Convert.ToString(Personalization.GetProfile("Usability", "ControlPanelVisible" + this.PortalId));
                return string.IsNullOrEmpty(setting) ? this.DefaultControlPanelVisibility : Convert.ToBoolean(setting);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc />
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

        /// <summary>Gets the mode the user is viewing the page in.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. Use Personalization.GetUserMode() instead. Scheduled removal in v11.0.0.")]
        public Mode UserMode
        {
            get => Personalization.GetUserMode();
        }

        /// <inheritdoc />
        public bool IsLocked
        {
            get { return this.IsThisPortalLocked || Host.Host.IsLocked; }
        }

        /// <inheritdoc />
        public bool IsThisPortalLocked
        {
            get { return PortalController.GetPortalSettingAsBoolean("IsLocked", this.PortalId, false); }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public string AddCompatibleHttpHeader
        {
            get
            {
                string compatibleHttpHeader = "IE=edge";
                string setting;
                if (PortalController.Instance.GetPortalSettings(this.PortalId).TryGetValue("AddCompatibleHttpHeader", out setting))
                {
                    // Hack to store empty string portalsetting with non empty default value
                    compatibleHttpHeader = (setting == "false") ? string.Empty : setting;
                }

                return compatibleHttpHeader;
            }
        }

        /// <inheritdoc />
        public bool AddCachebusterToResourceUris
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("AddCachebusterToResourceUris", this.PortalId, true);
            }
        }

        /// <inheritdoc />
        public bool DisablePrivateMessage
        {
            get
            {
                return PortalController.GetPortalSetting("DisablePrivateMessage", this.PortalId, "N") == "Y";
            }
        }

        public TabInfo ActiveTab { get; set; }

        /// <inheritdoc/>
        public int AdministratorId { get; set; }

        /// <inheritdoc/>
        public int AdministratorRoleId { get; set; }

        /// <inheritdoc/>
        public string AdministratorRoleName { get; set; }

        /// <inheritdoc/>
        public int AdminTabId { get; set; }

        /// <inheritdoc/>
        public string BackgroundFile { get; set; }

        /// <inheritdoc/>
        public int BannerAdvertising { get; set; }

        /// <inheritdoc/>
        public string CultureCode { get; set; }

        /// <inheritdoc/>
        public string Currency { get; set; }

        /// <inheritdoc/>
        public string DefaultLanguage { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public string Email { get; set; }

        /// <inheritdoc/>
        public DateTime ExpiryDate { get; set; }

        /// <inheritdoc/>
        public string FooterText { get; set; }

        /// <inheritdoc/>
        public Guid GUID { get; set; }

        /// <inheritdoc/>
        public string HomeDirectory { get; set; }

        /// <inheritdoc/>
        public string HomeSystemDirectory { get; set; }

        /// <inheritdoc/>
        public int HomeTabId { get; set; }

        /// <inheritdoc/>
        public float HostFee { get; set; }

        /// <inheritdoc/>
        public int HostSpace { get; set; }

        /// <inheritdoc/>
        public string KeyWords { get; set; }

        /// <inheritdoc/>
        public int LoginTabId { get; set; }

        /// <inheritdoc/>
        public string LogoFile { get; set; }

        /// <inheritdoc/>
        public int PageQuota { get; set; }

        /// <inheritdoc/>
        public int Pages { get; set; }

        /// <inheritdoc/>
        public int PortalId { get; set; }

        public PortalAliasInfo PortalAlias { get; set; }

        public PortalAliasInfo PrimaryAlias { get; set; }

        /// <inheritdoc/>
        public string PortalName { get; set; }

        /// <inheritdoc/>
        public int RegisteredRoleId { get; set; }

        /// <inheritdoc/>
        public string RegisteredRoleName { get; set; }

        /// <inheritdoc/>
        public int RegisterTabId { get; set; }

        public RegistrationSettings Registration { get; set; }

        /// <inheritdoc/>
        public int SearchTabId { get; set; }

        /// <inheritdoc/>
        public int SplashTabId { get; set; }

        /// <inheritdoc/>
        public int SuperTabId { get; set; }

        /// <inheritdoc/>
        public int UserQuota { get; set; }

        /// <inheritdoc/>
        public int UserRegistration { get; set; }

        /// <inheritdoc/>
        public int Users { get; set; }

        /// <inheritdoc/>
        public int UserTabId { get; set; }

        /// <inheritdoc/>
        public int TermsTabId { get; set; }

        /// <inheritdoc/>
        public int PrivacyTabId { get; set; }

        /// <inheritdoc />
        public bool AllowUserUICulture { get; internal set; }

        /// <inheritdoc/>
        public int CdfVersion { get; internal set; }

        /// <inheritdoc/>
        public bool ContentLocalizationEnabled { get; internal set; }

        public ControlPanelPermission ControlPanelSecurity { get; internal set; }

        /// <inheritdoc/>
        public string DefaultAdminContainer { get; internal set; }

        /// <inheritdoc/>
        public string DefaultAdminSkin { get; internal set; }

        /// <inheritdoc/>
        public string DefaultAuthProvider { get; internal set; }

        public Mode DefaultControlPanelMode { get; internal set; }

        /// <inheritdoc/>
        public bool DefaultControlPanelVisibility { get; internal set; }

        /// <inheritdoc/>
        public string DefaultIconLocation { get; internal set; }

        /// <inheritdoc />
        public int DefaultModuleId { get; internal set; }

        /// <inheritdoc/>
        public string DefaultModuleActionMenu { get; internal set; }

        /// <inheritdoc/>
        public string DefaultPortalContainer { get; internal set; }

        /// <inheritdoc/>
        public string DefaultPortalSkin { get; internal set; }

        /// <inheritdoc />
        public int DefaultTabId { get; internal set; }

        /// <inheritdoc />
        public bool EnableBrowserLanguage { get; internal set; }

        /// <inheritdoc/>
        public bool EnableCompositeFiles { get; internal set; }

        /// <inheritdoc />
        public bool EnablePopUps { get; internal set; }

        /// <inheritdoc />
        public bool EnableRegisterNotification { get; internal set; }

        /// <summary>Gets a value indicating whether to send an admin notification when an unapproved user resets their password.</summary>
        public bool EnableUnapprovedPasswordReminderNotification { get; internal set; }

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 9.8.1. This setting is no longer relevant as skin widgets are no longer supported. Scheduled removal in v11.0.0.")]
        public bool EnableSkinWidgets { get; internal set; }

        /// <inheritdoc />
        public bool ShowCookieConsent { get; internal set; }

        /// <inheritdoc />
        public string CookieMoreLink { get; internal set; }

        /// <inheritdoc />
        public bool EnableUrlLanguage { get; internal set; }

        /// <inheritdoc/>
        public int ErrorPage404 { get; internal set; }

        /// <inheritdoc/>
        public int ErrorPage500 { get; internal set; }

        /// <inheritdoc />
        public bool HideFoldersEnabled { get; internal set; }

        /// <inheritdoc />
        public bool HideLoginControl { get; internal set; }

        /// <inheritdoc/>
        public string HomeDirectoryMapPath { get; internal set; }

        /// <inheritdoc/>
        public string HomeSystemDirectoryMapPath { get; internal set; }

        /// <inheritdoc />
        public bool InlineEditorEnabled { get; internal set; }

        /// <inheritdoc />
        public bool SearchIncludeCommon { get; internal set; }

        /// <inheritdoc />
        public bool SearchIncludeNumeric { get; internal set; }

        /// <inheritdoc />
        public string SearchIncludedTagInfoFilter { get; internal set; }

        /// <inheritdoc />
        public int SearchMaxWordlLength { get; internal set; }

        /// <inheritdoc />
        public int SearchMinWordlLength { get; internal set; }

        /// <inheritdoc/>
        public Abstractions.Security.SiteSslSetup SSLSetup { get; internal set; }

        /// <inheritdoc/>
        public bool SSLEnabled => this.SSLSetup != Abstractions.Security.SiteSslSetup.Off;

        /// <inheritdoc/>
        public bool SSLEnforced { get; internal set; }

        /// <inheritdoc/>
        public string SSLURL { get; internal set; }

        /// <inheritdoc/>
        public string STDURL { get; internal set; }

        /// <inheritdoc/>
        public int SMTPConnectionLimit { get; internal set; }

        /// <inheritdoc/>
        public int SMTPMaxIdleTime { get; internal set; }

        /// <inheritdoc/>
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

        /// <inheritdoc />
        public bool DataConsentActive { get; internal set; }

        /// <inheritdoc />
        public DateTime DataConsentTermsLastChange { get; internal set; }

        /// <inheritdoc />
        public int DataConsentConsentRedirect { get; internal set; }

        /// <summary>
        /// Gets what should happen to the user account if a user has been deleted. This is important as
        /// under certain circumstances you may be required by law to destroy the user's data within a
        /// certain timeframe after a user has requested deletion.
        /// </summary>
        public UserDeleteAction DataConsentUserDeleteAction { get; internal set; }

        /// <inheritdoc />
        public int DataConsentDelay { get; internal set; }

        /// <inheritdoc />
        public string DataConsentDelayMeasurement { get; internal set; }

        /// <summary>Gets whitelist of file extensions for end users.</summary>
        public FileExtensionWhitelist AllowedExtensionsWhitelist { get; internal set; }

        /// <inheritdoc/>
        public bool ShowQuickModuleAddMenu
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("ShowQuickModuleAddMenu", this.PortalId, false);
            }
        }

        public CspMode CspHeaderMode { get; internal set; }

        public string CspHeader { get; internal set; }

        public string CspReportingHeader { get; internal set; }

        /// <inheritdoc/>
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
                    result = this.SSLEnabled ? "https" : "http";
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
