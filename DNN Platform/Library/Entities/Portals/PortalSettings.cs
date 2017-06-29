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
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Common;

#endregion

namespace DotNetNuke.Entities.Portals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PortalSettings class encapsulates all of the settings for the Portal, 
    /// as well as the configuration settings required to execute the current tab
    /// view within the portal.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public partial class PortalSettings : BaseEntityInfo, IPropertyAccess
    {
        #region ControlPanelPermission enum

        public enum ControlPanelPermission
        {
            TabEditor,
            ModuleEditor
        }

        #endregion

        #region Mode enum

        public enum Mode
        {
            View,
            Edit,
            Layout
        }

        #endregion

        #region PortalAliasMapping enum

        public enum PortalAliasMapping
        {
            None,
            CanonicalUrl,
            Redirect
        }

        #endregion

        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;

        #region Constructors

        public PortalSettings()
        {
            Registration = new RegistrationSettings();
        }

        public PortalSettings(int portalId)
            : this(Null.NullInteger, portalId)
        {
        }

        public PortalSettings(int tabId, int portalId)
        {
            PortalId = portalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            BuildPortalSettings(tabId, portal);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The PortalSettings Constructor encapsulates all of the logic
        /// necessary to obtain configuration settings necessary to render
        /// a Portal Tab view for a given request.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="tabId">The current tab</param>
        ///	<param name="portalAliasInfo">The current portal</param>
        /// -----------------------------------------------------------------------------
        public PortalSettings(int tabId, PortalAliasInfo portalAliasInfo)
        {
            PortalId = portalAliasInfo.PortalID;
            PortalAlias = portalAliasInfo;
            var portal = string.IsNullOrEmpty(portalAliasInfo.CultureCode) ?
                            PortalController.Instance.GetPortal(portalAliasInfo.PortalID)
                            : PortalController.Instance.GetPortal(portalAliasInfo.PortalID, portalAliasInfo.CultureCode);

            BuildPortalSettings(tabId, portal);
        }

        public PortalSettings(PortalInfo portal)
            : this(Null.NullInteger, portal)
        {
        }

        public PortalSettings(int tabId, PortalInfo portal)
        {
            PortalId = portal != null ? portal.PortalID : Null.NullInteger;
            BuildPortalSettings(tabId, portal);
        }

        private void BuildPortalSettings(int tabId, PortalInfo portal)
        {
            PortalSettingsController.Instance().LoadPortalSettings(this);

            if (portal == null) return;

            PortalSettingsController.Instance().LoadPortal(portal, this);

            var key = string.Join(":", "ActiveTab", portal.PortalID.ToString(), tabId.ToString());
            var items = HttpContext.Current != null ? HttpContext.Current.Items : null;
            if (items != null && items.Contains(key))
            {
                ActiveTab = items[key] as TabInfo;
            }
            else
            {
                ActiveTab = PortalSettingsController.Instance().GetActiveTab(tabId, this);
                if (items != null && ActiveTab != null)
                {
                    items[key] = ActiveTab;
                }
            }
        }

        #endregion

        #region Auto-Properties

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

        [Obsolete("Deprecated in 8.0.0")]
        public int SiteLogHistory { get; set; }

        public int SplashTabId { get; set; }

        public int SuperTabId { get; set; }

        public int UserQuota { get; set; }

        public int UserRegistration { get; set; }

        public int Users { get; set; }

        public int UserTabId { get; set; }

        #endregion

        #region Read-Only Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows users to select their own UI culture.
        /// When set to false (default) framework will allways same culture for both
        /// CurrentCulture (content) and CurrentUICulture (interface)
        /// </summary>
        /// <remarks>Defaults to False</remarks>
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
        /// Gets the Default Module Id
        /// </summary>
        /// <remarks>Defaults to Null.NullInteger</remarks>
        /// -----------------------------------------------------------------------------
        public int DefaultModuleId { get; internal set; }

        public string DefaultPortalContainer { get; internal set; }

        public string DefaultPortalSkin { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Tab Id
        /// </summary>
        /// <remarks>Defaults to Null.NullInteger</remarks>
        /// -----------------------------------------------------------------------------
        public int DefaultTabId { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether Browser Language Detection is Enabled
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableBrowserLanguage { get; internal set; }

        public bool EnableCompositeFiles { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to use the module effect in edit mode.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        [Obsolete("Deprecated in Platform 7.4.0.")]
        public bool EnableModuleEffect { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to use the popup.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnablePopUps { get; internal set; }

        /// <summary>
        /// Website Administrator whether receive the notification email when new user register.
        /// </summary>
        public bool EnableRegisterNotification { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Skin Widgets are enabled/supported
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableSkinWidgets { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether enable url language.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableUrlLanguage { get; internal set; }

        public int ErrorPage404 { get; internal set; }

        public int ErrorPage500 { get; internal set; }

        /// -----------------------------------------------------------------------------
		/// <summary>
		///   Gets whether folders which are hidden or whose name begins with underscore
		///   are included in folder synchronization.
		/// </summary>
		/// <remarks>
		///   Defaults to True
		/// </remarks>
		/// -----------------------------------------------------------------------------
        public bool HideFoldersEnabled { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether hide the login link.
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool HideLoginControl { get; internal set; }

        public string HomeDirectoryMapPath { get; internal set; }

        public string HomeSystemDirectoryMapPath { get; internal set; }

        /// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether the Inline Editor is enabled
		/// </summary>
		/// <remarks>Defaults to True</remarks>
		/// -----------------------------------------------------------------------------
        public bool InlineEditorEnabled { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to inlcude Common Words in the Search Index
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeCommon { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to inlcude Numbers in the Search Index
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeNumeric { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets the filter used for inclusion of tag info
        /// </summary>
        /// <remarks>
        ///   Defaults to ""
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string SearchIncludedTagInfoFilter { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum Search Word length to index
        /// </summary>
        /// <remarks>Defaults to 3</remarks>
        /// -----------------------------------------------------------------------------
        public int SearchMaxWordlLength { get; internal set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the minum Search Word length to index
        /// </summary>
        /// <remarks>Defaults to 3</remarks>
        /// -----------------------------------------------------------------------------
        public int SearchMinWordlLength { get; internal set; }

        public bool SSLEnabled { get; internal set; }

        public bool SSLEnforced { get; internal set; }

        public string SSLURL { get; internal set; }

        public string STDURL { get; internal set; }

        public int SMTPConnectionLimit { get; internal set; }

        public int SMTPMaxIdleTime { get; internal set; }

        #endregion

        #region Public Properties

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
                var setting = Convert.ToString(Personalization.GetProfile("Usability", "ControlPanelVisible" + PortalId));
                return String.IsNullOrEmpty(setting) ? DefaultControlPanelVisibility : Convert.ToBoolean(setting);
            }
        }

        public static PortalSettings Current
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        public string DefaultPortalAlias
        {
            get
            {
                foreach (var alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(PortalId).Where(alias => alias.IsPrimary))
                {
                    return alias.HTTPAlias;
                }
                return String.Empty;
            }
        }

        public PortalAliasMapping PortalAliasMappingMode
        {
            get
            {
                return PortalSettingsController.Instance().GetPortalAliasMappingMode(PortalId);
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
                    return UserInfo.UserID;
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
                    mode = DefaultControlPanelMode;
                    string setting = Convert.ToString(Personalization.GetProfile("Usability", "UserMode" + PortalId));
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

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set
            {
                _timeZone = value;
                PortalController.UpdatePortalSetting(PortalId, "TimeZone", value.Id, true);
            }
        }


        public string PageHeadText
        {
            get
            {
                // For New Install
                string pageHead = "<meta content=\"text/html; charset=UTF-8\" http-equiv=\"Content-Type\" />";
                string setting;
                if (PortalController.Instance.GetPortalSettings(PortalId).TryGetValue("PageHeadText", out setting))
                {
                    // Hack to store empty string portalsetting with non empty default value
                    pageHead = (setting == "false") ? "" : setting;
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
                return PortalController.GetPortalSettingAsBoolean("InjectModuleHyperLink", PortalId, true);
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
                if (PortalController.Instance.GetPortalSettings(PortalId).TryGetValue("AddCompatibleHttpHeader", out setting))
                {
                    // Hack to store empty string portalsetting with non empty default value
                    CompatibleHttpHeader = (setting == "false") ? "" : setting;
                }
                return CompatibleHttpHeader;
            }
        }

        /*
         * add a cachebuster parameter to generated file URI's
         * 
         * of the form ver=[file timestame] ie ver=2015-02-17-162255-735
         * 
         */
        public bool AddCachebusterToResourceUris
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("AddCachebusterToResourceUris", PortalId, true);
            }
        }

        /// <summary>
        /// If this is true, then regular users can't send message to specific user/group.
        /// </summary>
        public bool DisablePrivateMessage
        {
            get
            {
                return PortalController.GetPortalSetting("DisablePrivateMessage", PortalId, "N") == "Y";
            }
        }

        #endregion

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            var outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }
            var lowerPropertyName = propertyName.ToLower();
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
                case "url":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(PortalAlias.HTTPAlias, format);
                    break;
                case "passwordreminderurl": //if regsiter page defined in portal settings, then get that page url, otherwise return home page.
                    propertyNotFound = false;
                    var reminderUrl = Globals.AddHTTP(PortalAlias.HTTPAlias);
                    if (RegisterTabId > Null.NullInteger)
                    {
                        reminderUrl = Globals.RegisterURL(string.Empty, string.Empty);
                    }
                    result = PropertyAccess.FormatString(reminderUrl, format);
                    break;
                case "portalid":
                    propertyNotFound = false;
                    result = (PortalId.ToString(outputFormat, formatProvider));
                    break;
                case "portalname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(PortalName, format);
                    break;
                case "homedirectory":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(HomeDirectory, format);
                    break;
                case "homedirectorymappath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(HomeDirectoryMapPath, format);
                    break;
                case "logofile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(LogoFile, format);
                    break;
                case "footertext":
                    propertyNotFound = false;
                    var footerText = FooterText.Replace("[year]", DateTime.Now.Year.ToString());
                    result = PropertyAccess.FormatString(footerText, format);
                    break;
                case "expirydate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (ExpiryDate.ToString(outputFormat, formatProvider));
                    break;
                case "userregistration":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (UserRegistration.ToString(outputFormat, formatProvider));
                    break;
                case "banneradvertising":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (BannerAdvertising.ToString(outputFormat, formatProvider));
                    break;
                case "currency":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Currency, format);
                    break;
                case "administratorid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (AdministratorId.ToString(outputFormat, formatProvider));
                    break;
                case "email":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Email, format);
                    break;
                case "hostfee":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (HostFee.ToString(outputFormat, formatProvider));
                    break;
                case "hostspace":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (HostSpace.ToString(outputFormat, formatProvider));
                    break;
                case "pagequota":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PageQuota.ToString(outputFormat, formatProvider));
                    break;
                case "userquota":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (UserQuota.ToString(outputFormat, formatProvider));
                    break;
                case "administratorroleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (AdministratorRoleId.ToString(outputFormat, formatProvider));
                    break;
                case "administratorrolename":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(AdministratorRoleName, format);
                    break;
                case "registeredroleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (RegisteredRoleId.ToString(outputFormat, formatProvider));
                    break;
                case "registeredrolename":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(RegisteredRoleName, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Description, format);
                    break;
                case "keywords":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(KeyWords, format);
                    break;
                case "backgroundfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(BackgroundFile, format);
                    break;
                case "admintabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = AdminTabId.ToString(outputFormat, formatProvider);
                    break;
                case "supertabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = SuperTabId.ToString(outputFormat, formatProvider);
                    break;
                case "splashtabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = SplashTabId.ToString(outputFormat, formatProvider);
                    break;
                case "hometabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = HomeTabId.ToString(outputFormat, formatProvider);
                    break;
                case "logintabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = LoginTabId.ToString(outputFormat, formatProvider);
                    break;
                case "registertabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = RegisterTabId.ToString(outputFormat, formatProvider);
                    break;
                case "usertabid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = UserTabId.ToString(outputFormat, formatProvider);
                    break;
                case "defaultlanguage":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DefaultLanguage, format);
                    break;
                case "users":
                    isPublic = false;
                    propertyNotFound = false;
                    result = Users.ToString(outputFormat, formatProvider);
                    break;
                case "pages":
                    isPublic = false;
                    propertyNotFound = false;
                    result = Pages.ToString(outputFormat, formatProvider);
                    break;
                case "contentvisible":
                    isPublic = false;
                    break;
                case "controlpanelvisible":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(ControlPanelVisible, formatProvider);
                    break;
            }
            if (!isPublic && accessLevel != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }
            return result;
        }

        #endregion

        #region Public Methods

        public PortalSettings Clone()
        {
            return (PortalSettings)MemberwiseClone();
        }

        #endregion
    }
}
