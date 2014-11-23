#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Entities.Portals
{
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The PortalSettings class encapsulates all of the settings for the Portal, 
    /// as well as the configuration settings required to execute the current tab
	/// view within the portal.
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// -----------------------------------------------------------------------------
	[Serializable]
	public class PortalSettings : BaseEntityInfo, IPropertyAccess
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

        private Dictionary<string, string> _settings;
        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
        private string _version;

		#region Constructors

		public PortalSettings()
		{
			_settings = new Dictionary<string, string>();
			Registration = new RegistrationSettings();
		}

		public PortalSettings(int portalId)
			: this(Null.NullInteger, portalId)
		{
		}

		public PortalSettings(int tabId, int portalId)
		{
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
		///	<param name="objPortalAliasInfo">The current portal</param>
		/// <history>
		/// 	[cnurse]	10/21/2004	documented
		/// </history>
		/// -----------------------------------------------------------------------------
		public PortalSettings(int tabId, PortalAliasInfo objPortalAliasInfo)
		{
			PortalAlias = objPortalAliasInfo;
			var portal = PortalController.Instance.GetPortal(objPortalAliasInfo.PortalID);
            BuildPortalSettings(tabId, portal);
        }

		public PortalSettings(PortalInfo portal) 
            : this(Null.NullInteger, portal)
		{
		}

		public PortalSettings(int tabId, PortalInfo portal)
		{
            BuildPortalSettings(tabId, portal);
		}

        private void BuildPortalSettings(int tabId, PortalInfo portal)
        {
            ActiveTab = new TabInfo();

            _settings = PortalController.GetPortalSettingsDictionary(PortalId);
            Registration = new RegistrationSettings(_settings);

            MapPortalSettingsDictionary();

            if (portal == null) return;
            MapPortalInfoSettings(portal);

            if (!VerifyPortalTab(PortalId, tabId)) return;

            if (ActiveTab.TabID != Null.NullInteger)
            {
                ConfigureActiveTab();
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

		public int SearchTabId { get; set; }

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
		/// <history>
		/// 	[vmasanas]	03/22/2012   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool AllowUserUICulture { get; private set; }

		public int CdfVersion { get; private set; }

		public bool ContentLocalizationEnabled { get; private set; }

        public ControlPanelPermission ControlPanelSecurity { get; private set; }

        public string DefaultAdminContainer { get; private set; }

		public string DefaultAdminSkin { get; private set; }

        public string DefaultAuthProvider { get; private set; }

        public Mode DefaultControlPanelMode { get; private set; }

        public bool DefaultControlPanelVisibility { get; private set; }

        public string DefaultIconLocation { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the Default Module Id
		/// </summary>
		/// <remarks>Defaults to Null.NullInteger</remarks>
		/// <history>
		/// 	[cnurse]	05/02/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public int DefaultModuleId { get; private set; }

		public string DefaultPortalContainer { get; private set; }

		public string DefaultPortalSkin { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the Default Tab Id
		/// </summary>
		/// <remarks>Defaults to Null.NullInteger</remarks>
		/// <history>
		/// 	[cnurse]	05/02/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public int DefaultTabId { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether Browser Language Detection is Enabled
		/// </summary>
		/// <remarks>Defaults to True</remarks>
		/// <history>
		/// 	[cnurse]	02/19/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool EnableBrowserLanguage { get; private set; }

		public bool EnableCompositeFiles { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether to use the module effect in edit mode.
		/// </summary>
		/// <remarks>Defaults to True</remarks>
		/// -----------------------------------------------------------------------------
		public bool EnableModuleEffect { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to use the popup.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnablePopUps { get; private set; }

        /// <summary>
        /// Website Administrator whether receive the notification email when new user register.
        /// </summary>
        public bool EnableRegisterNotification { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether the Skin Widgets are enabled/supported
		/// </summary>
		/// <remarks>Defaults to True</remarks>
		/// <history>
		/// 	[cnurse]	07/03/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool EnableSkinWidgets { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether enable url language.
		/// </summary>
		/// <remarks>Defaults to True</remarks>
		/// -----------------------------------------------------------------------------
		public bool EnableUrlLanguage { get; private set; }

        public int ErrorPage404 { get; private set; }

        public int ErrorPage500 { get; private set; }

        /// -----------------------------------------------------------------------------
		/// <summary>
		///   Gets whether folders which are hidden or whose name begins with underscore
		///   are included in folder synchronization.
		/// </summary>
		/// <remarks>
		///   Defaults to True
		/// </remarks>
		/// <history>
		///   [cnurse]	08/28/2008 Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool HideFoldersEnabled { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether hide the login link.
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        /// -----------------------------------------------------------------------------
        public bool HideLoginControl { get; private set; }

        /// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether the Inline Editor is enabled
		/// </summary>
		/// <remarks>Defaults to True</remarks>
		/// <history>
		/// 	[cnurse]	08/28/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool InlineEditorEnabled { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether to inlcude Common Words in the Search Index
		/// </summary>
		/// <remarks>Defaults to False</remarks>
		/// <history>
		/// 	[cnurse]	03/10/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool SearchIncludeCommon { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether to inlcude Numbers in the Search Index
		/// </summary>
		/// <remarks>Defaults to False</remarks>
		/// <history>
		/// 	[cnurse]	03/10/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public bool SearchIncludeNumeric { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		///   Gets the filter used for inclusion of tag info
		/// </summary>
		/// <remarks>
		///   Defaults to ""
		/// </remarks>
		/// <history>
		///   [vnguyen]   09/03/2010   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public string SearchIncludedTagInfoFilter { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the maximum Search Word length to index
		/// </summary>
		/// <remarks>Defaults to 3</remarks>
		/// <history>
		/// 	[cnurse]	03/10/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public int SearchMaxWordlLength { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the minum Search Word length to index
		/// </summary>
		/// <remarks>Defaults to 3</remarks>
		/// <history>
		/// 	[cnurse]	03/10/2008   Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public int SearchMinWordlLength { get; private set; }

		public bool SSLEnabled { get; private set; }

		public bool SSLEnforced { get; private set; }

		public string SSLURL { get; private set; }

		public string STDURL { get; private set; }

		public int SMTPConnectionLimit { get; private set; }

		public int SMTPMaxIdleTime { get; private set; }

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

		public string HomeDirectoryMapPath { get; private set; }

		public string HomeSystemDirectoryMapPath { get; private set; }

		public PortalAliasMapping PortalAliasMappingMode
		{
			get
			{
				return GetPortalAliasMappingMode(PortalId);
			}
		}

        public RegistrationSettings Registration { get; set; }

		public int UserId
		{
			get
			{
				if (HttpContext.Current.Request.IsAuthenticated)
				{
					return UserInfo.UserID;
				}
				return Null.NullInteger;
			}
		}

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

		#endregion

        public static PortalAliasMapping GetPortalAliasMappingMode(int portalId)
        {
            PortalAliasMapping aliasMapping = PortalAliasMapping.None;
            string setting;
            if (PortalController.GetPortalSettingsDictionary(portalId).TryGetValue("PortalAliasMapping", out setting))
            {
                switch (setting.ToUpperInvariant())
                {
                    case "CANONICALURL":
                        aliasMapping = PortalAliasMapping.CanonicalUrl;
                        break;
                    case "REDIRECT":
                        aliasMapping = PortalAliasMapping.Redirect;
                        break;
                    default:
                        aliasMapping = PortalAliasMapping.None;
                        break;
                }
            }
            return aliasMapping;
        }

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
				case "siteloghistory":
					isPublic = false;
					propertyNotFound = false;
					result = SiteLogHistory.ToString(outputFormat, formatProvider);
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

        #region Private Methods

        private void ConfigureActiveTab()
		{
			if (Globals.IsAdminSkin())
			{
				ActiveTab.SkinSrc = DefaultAdminSkin;
			}
			else if (String.IsNullOrEmpty(ActiveTab.SkinSrc))
			{
				ActiveTab.SkinSrc = DefaultPortalSkin;
			}
			ActiveTab.SkinSrc = SkinController.FormatSkinSrc(ActiveTab.SkinSrc, this);
			ActiveTab.SkinPath = SkinController.FormatSkinPath(ActiveTab.SkinSrc);

			if (Globals.IsAdminSkin())
			{
				ActiveTab.ContainerSrc = DefaultAdminContainer;
			}
			else if (String.IsNullOrEmpty(ActiveTab.ContainerSrc))
			{
				ActiveTab.ContainerSrc = DefaultPortalContainer;
			}

			ActiveTab.ContainerSrc = SkinController.FormatSkinSrc(ActiveTab.ContainerSrc, this);
			ActiveTab.ContainerPath = SkinController.FormatSkinPath(ActiveTab.ContainerSrc);

			ActiveTab.Panes = new ArrayList();
			var crumbs = new ArrayList();
			GetBreadCrumbsRecursively(ref crumbs, ActiveTab.TabID);
			ActiveTab.BreadCrumbs = crumbs;
		}

		private void GetBreadCrumbsRecursively(ref ArrayList breadCrumbs, int tabId)
		{
			TabInfo tab;
			var portalTabs = TabController.Instance.GetTabsByPortal(PortalId);
			var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);
			bool tabFound = portalTabs.TryGetValue(tabId, out tab);
			if (!tabFound)
			{
				tabFound = hostTabs.TryGetValue(tabId, out tab);
			}
			//if tab was found
			if (tabFound)
			{
				//add tab to breadcrumb collection
				breadCrumbs.Insert(0, tab.Clone());

				//get the tab parent
				if (!Null.IsNull(tab.ParentId) && tabId != tab.ParentId)
				{
					GetBreadCrumbsRecursively(ref breadCrumbs, tab.ParentId);
				}
			}
		}

        private void MapPortalSettingsDictionary()
        {
            AllowUserUICulture = _settings.GetValueOrDefault("AllowUserUICulture", false);
            CdfVersion = _settings.GetValueOrDefault("CdfVersion", Null.NullInteger);
            ContentLocalizationEnabled = _settings.GetValueOrDefault("ContentLocalizationEnabled", false);
            DefaultAdminContainer = _settings.GetValueOrDefault("DefaultAdminContainer", Host.Host.DefaultAdminContainer);
            DefaultAdminSkin = _settings.GetValueOrDefault("DefaultAdminSkin", Host.Host.DefaultAdminSkin);
            DefaultIconLocation = _settings.GetValueOrDefault("DefaultIconLocation", "icons/sigma");
            DefaultModuleId = _settings.GetValueOrDefault("defaultmoduleid", Null.NullInteger);
            DefaultPortalContainer = _settings.GetValueOrDefault("DefaultPortalContainer", Host.Host.DefaultPortalContainer);
            DefaultPortalSkin = _settings.GetValueOrDefault("DefaultPortalSkin", Host.Host.DefaultPortalSkin);
            DefaultTabId = _settings.GetValueOrDefault("defaulttabid", Null.NullInteger);
            EnableBrowserLanguage = _settings.GetValueOrDefault("EnableBrowserLanguage", Host.Host.EnableBrowserLanguage);
            EnableCompositeFiles = _settings.GetValueOrDefault("EnableCompositeFiles", false);
            EnablePopUps = _settings.GetValueOrDefault("EnablePopUps", true);
            EnableModuleEffect = _settings.GetValueOrDefault("EnableModuleEffect", true);
            HideLoginControl = _settings.GetValueOrDefault("HideLoginControl", false);
            EnableSkinWidgets = _settings.GetValueOrDefault("EnableSkinWidgets", true);
            EnableUrlLanguage = _settings.GetValueOrDefault("EnableUrlLanguage", Host.Host.EnableUrlLanguage);
            HideFoldersEnabled = _settings.GetValueOrDefault("HideFoldersEnabled", true);
            InlineEditorEnabled = _settings.GetValueOrDefault("InlineEditorEnabled", true);
            SearchIncludeCommon = _settings.GetValueOrDefault("SearchIncludeCommon", Host.Host.SearchIncludeCommon);
            SearchIncludeNumeric = _settings.GetValueOrDefault("SearchIncludeNumeric", Host.Host.SearchIncludeNumeric);
            SearchIncludedTagInfoFilter = _settings.GetValueOrDefault("SearchIncludedTagInfoFilter", Host.Host.SearchIncludedTagInfoFilter);
            SearchMaxWordlLength = _settings.GetValueOrDefault("MaxSearchWordLength", Host.Host.SearchMaxWordlLength);
            SearchMinWordlLength = _settings.GetValueOrDefault("MinSearchWordLength", Host.Host.SearchMinWordlLength);
            SSLEnabled = _settings.GetValueOrDefault("SSLEnabled", false);
            SSLEnforced = _settings.GetValueOrDefault("SSLEnforced", false);
            SSLURL = _settings.GetValueOrDefault("SSLURL", Null.NullString);
            STDURL = _settings.GetValueOrDefault("STDURL", Null.NullString);
            EnableRegisterNotification = _settings.GetValueOrDefault("EnableRegisterNotification", true);
            DefaultAuthProvider = _settings.GetValueOrDefault("DefaultAuthProvider", "DNN");
            SMTPConnectionLimit = _settings.GetValueOrDefault("SMTPConnectionLimit", 1);
            SMTPMaxIdleTime = _settings.GetValueOrDefault("SMTPMaxIdleTime", 0);

            ControlPanelSecurity = ControlPanelPermission.ModuleEditor;
            string setting = _settings.GetValueOrDefault("ControlPanelSecurity", "");
            if (setting.ToUpperInvariant() == "TAB")
            {
                ControlPanelSecurity = ControlPanelPermission.TabEditor;
            }
            DefaultControlPanelMode = Mode.View;
            setting = _settings.GetValueOrDefault("ControlPanelMode", "");
            if (setting.ToUpperInvariant() == "EDIT")
            {
                DefaultControlPanelMode = Mode.Edit;
            }
            setting = _settings.GetValueOrDefault("ControlPanelVisibility", "");
            DefaultControlPanelVisibility = setting.ToUpperInvariant() != "MIN";
            setting = _settings.GetValueOrDefault("TimeZone", "");
            if (!string.IsNullOrEmpty(setting))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(setting);
                if (timeZone != null)
                    _timeZone = timeZone;
            }


        }

	    ///  -----------------------------------------------------------------------------
	    ///  <summary>
	    ///  The MapPortalInfoSettings method builds the site Settings
	    ///  </summary>
	    ///  <remarks>
	    ///  </remarks>
	    /// <param name="portal">The Portal object</param>
	    ///  <history>
	    ///  </history>
	    ///  -----------------------------------------------------------------------------
	    private void MapPortalInfoSettings(PortalInfo portal)
		{
			PortalId = portal.PortalID;
			PortalName = portal.PortalName;
			LogoFile = portal.LogoFile;
			FooterText = portal.FooterText;
			ExpiryDate = portal.ExpiryDate;
			UserRegistration = portal.UserRegistration;
			BannerAdvertising = portal.BannerAdvertising;
			Currency = portal.Currency;
			AdministratorId = portal.AdministratorId;
			Email = portal.Email;
			HostFee = portal.HostFee;
            HostSpace = Null.IsNull(portal.HostSpace) ? 0 : portal.HostSpace ;
			PageQuota = portal.PageQuota;
			UserQuota = portal.UserQuota;
			AdministratorRoleId = portal.AdministratorRoleId;
			AdministratorRoleName = portal.AdministratorRoleName;
			RegisteredRoleId = portal.RegisteredRoleId;
			RegisteredRoleName = portal.RegisteredRoleName;
			Description = portal.Description;
			KeyWords = portal.KeyWords;
			BackgroundFile = portal.BackgroundFile;
			GUID = portal.GUID;
			SiteLogHistory = portal.SiteLogHistory;
			AdminTabId = portal.AdminTabId;
			SuperTabId = portal.SuperTabId;
			SplashTabId = portal.SplashTabId;
			HomeTabId = portal.HomeTabId;
			LoginTabId = portal.LoginTabId;
			RegisterTabId = portal.RegisterTabId;
			UserTabId = portal.UserTabId;
			SearchTabId = portal.SearchTabId;
			ErrorPage404 = portal.Custom404TabId;
			ErrorPage500 = portal.Custom500TabId;
			DefaultLanguage = Null.IsNull(portal.DefaultLanguage) ? Localization.SystemLocale : portal.DefaultLanguage;
			HomeDirectory = Globals.ApplicationPath + "/" + portal.HomeDirectory + "/";
			HomeDirectoryMapPath = portal.HomeDirectoryMapPath;
			HomeSystemDirectory = Globals.ApplicationPath + "/" + portal.HomeSystemDirectory + "/";
			HomeSystemDirectoryMapPath = portal.HomeSystemDirectoryMapPath;
			Pages = portal.Pages;
			Users = portal.Users;
			CultureCode = portal.CultureCode;
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// The VerifyPortalTab method verifies that the TabId/PortalId combination
		/// is allowed and returns default/home tab ids if not
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// </remarks>
		///	<param name="portalId">The Portal's id</param>
		///	<param name="tabId">The current tab's id</param>
		/// <history>
		/// </history>
		/// -----------------------------------------------------------------------------
		private bool VerifyPortalTab(int portalId, int tabId)
		{
			var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
			var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);

			//Check portal
			bool isVerified = VerifyTabExists(tabId, portalTabs);

			if (!isVerified)
			{
				//check host
				isVerified = VerifyTabExists(tabId, hostTabs);
			}

			if (!isVerified)
			{
				//check splash tab
				isVerified = VerifySpecialTab(portalId, SplashTabId);
			}

			if (!isVerified)
			{
				//check home tab
				isVerified = VerifySpecialTab(portalId, HomeTabId);
			}

			if (!isVerified)
			{
				TabInfo tab = (from TabInfo t in portalTabs.AsList() where !t.IsDeleted && t.IsVisible && t.HasAVisibleVersion select t).FirstOrDefault();

				if (tab != null)
				{
					isVerified = true;
					ActiveTab = tab.Clone();
				}
			}

			if (ActiveTab != null)
			{
				if (Null.IsNull(ActiveTab.StartDate))
				{
					ActiveTab.StartDate = DateTime.MinValue;
				}
				if (Null.IsNull(ActiveTab.EndDate))
				{
					ActiveTab.EndDate = DateTime.MaxValue;
				}
			}

			return isVerified;
		}

		private bool VerifySpecialTab(int portalId, int tabId)
		{
			bool isVerified = false;

			if (tabId > 0)
			{
				TabInfo tab = TabController.Instance.GetTab(tabId, portalId, false);
				if (tab != null)
				{
					ActiveTab = tab.Clone();
					isVerified = true;
				}
			}

			return isVerified;
		}

		private bool VerifyTabExists(int tabId, TabCollection tabs)
		{
			bool isVerified = false;

			if (tabId != Null.NullInteger)
			{
				TabInfo tab;
				if (tabs.TryGetValue(tabId, out tab))
				{
					if (!tab.IsDeleted)
					{
						ActiveTab = tab.Clone();
						isVerified = true;
					}
				}
			}
			return isVerified;
		}

		#endregion

		#region Obsolete Methods

		private ArrayList _desktopTabs;

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultAdminContainer")]
		public SkinInfo AdminContainer { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultAdminSkin")]
		public SkinInfo AdminSkin { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Replaced by Host.GetHostSettingsDictionary")]
		public Hashtable HostSettings
		{
			get
			{
				var h = new Hashtable();
				foreach (ConfigurationSetting kvp in HostController.Instance.GetSettings().Values)
				{
					h.Add(kvp.Key, kvp.Value);
				}
				return h;
			}
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by extended UserMode property.")]
		public bool ContentVisible
		{
			get
			{
				return UserMode != Mode.Layout;
			}
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultPortalContainer")]
		public SkinInfo PortalContainer { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Replaced by DefaultPortalSkin")]
		public SkinInfo PortalSkin { get; set; }

		[Obsolete("Deprecated in DNN 5.0. Tabs are cached independeently of Portal Settings, and this property is thus redundant")]
		public ArrayList DesktopTabs
		{
			get
			{
				if (_desktopTabs == null)
				{
					_desktopTabs = new ArrayList();

					//Add each portal Tab to DesktopTabs
					TabInfo objPortalTab;
					foreach (TabInfo objTab in TabController.GetTabsBySortOrder(PortalId, CultureCode, true))
					{
						// clone the tab object ( to avoid creating an object reference to the data cache )
						objPortalTab = objTab.Clone();

						// set custom properties
						if (objPortalTab.TabOrder == 0)
						{
							objPortalTab.TabOrder = 999;
						}
						if (Null.IsNull(objPortalTab.StartDate))
						{
							objPortalTab.StartDate = DateTime.MinValue;
						}
						if (Null.IsNull(objPortalTab.EndDate))
						{
							objPortalTab.EndDate = DateTime.MaxValue;
						}

						_desktopTabs.Add(objPortalTab);
					}

					//Add each host Tab to DesktopTabs
					TabInfo objHostTab;
					foreach (TabInfo objTab in TabController.GetTabsBySortOrder(Null.NullInteger, Null.NullString, true))
					{
						// clone the tab object ( to avoid creating an object reference to the data cache )
						objHostTab = objTab.Clone();
						objHostTab.PortalID = PortalId;
						objHostTab.StartDate = DateTime.MinValue;
						objHostTab.EndDate = DateTime.MaxValue;

						_desktopTabs.Add(objHostTab);
					}
				}

				return _desktopTabs;
			}
		}

		[Obsolete("Deprecated in DNN 5.1. Replaced by Application.Version")]
		public string Version
		{
			get
			{
				if (string.IsNullOrEmpty(_version))
				{
					_version = DotNetNukeContext.Current.Application.Version.ToString(3);
				}
				return _version;
			}
			set
			{
				_version = value;
			}
		}

		[Obsolete("Deprecated in DNN 6.0")]
		public int TimeZoneOffset
		{
			get
			{
				return Convert.ToInt32(TimeZone.BaseUtcOffset.TotalMinutes);
			}
			set
			{
				TimeZone = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(value);
			}
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.ExecuteScript")]
		public static string ExecuteScript(string strScript)
		{
			return DataProvider.Instance().ExecuteScript(strScript);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.ExecuteScript")]
		public static string ExecuteScript(string strScript, bool useTransactions)
		{
			return DataProvider.Instance().ExecuteScript(strScript);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by Globals.FindDatabaseVersion")]
		public static bool FindDatabaseVersion(int major, int minor, int build)
		{
			return Globals.FindDatabaseVersion(major, minor, build);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.GetDatabaseVersion")]
		public static IDataReader GetDatabaseVersion()
		{
			return DataProvider.Instance().GetDatabaseVersion();
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by Host.GetHostSettingsDictionary")]
		public static Hashtable GetHostSettings()
		{
			var h = new Hashtable();
			foreach (KeyValuePair<string, string> kvp in HostController.Instance.GetSettingsDictionary())
			{
				h.Add(kvp.Key, kvp.Value);
			}
			return h;
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetModuleSettings(ModuleId)")]
		public static Hashtable GetModuleSettings(int moduleId)
		{
			return new ModuleController().GetModuleSettings(moduleId);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasInfo")]
		public static PortalAliasInfo GetPortalAliasInfo(string portalAlias)
		{
			return PortalAliasController.Instance.GetPortalAlias(portalAlias);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasByPortal")]
		public static string GetPortalByID(int portalId, string portalAlias)
		{
			return PortalAliasController.GetPortalAliasByPortal(portalId, portalAlias);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasByTab")]
		public static string GetPortalByTab(int tabID, string portalAlias)
		{
			return PortalAliasController.GetPortalAliasByTab(tabID, portalAlias);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalAliasController.GetPortalAliasLookup")]
		public static PortalAliasCollection GetPortalAliasLookup()
		{
			var portalAliasCollection = new PortalAliasCollection();
			var aliasController = new PortalAliasController();
			foreach (var kvp in aliasController.GetPortalAliasesInternal())
			{
				portalAliasCollection.Add(kvp.Key, kvp.Value);
			}

			return portalAliasCollection;
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.GetProviderPath")]
		public static string GetProviderPath()
		{
			return DataProvider.Instance().GetProviderPath();
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalController.GetPortalSettingsDictionary")]
		public static Hashtable GetSiteSettings(int portalId)
		{
			var h = new Hashtable();
			foreach (KeyValuePair<string, string> kvp in PortalController.GetPortalSettingsDictionary(portalId))
			{
				h.Add(kvp.Key, kvp.Value);
			}
			return h;
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalController.GetPortalSettingsDictionary(portalId).TryGetValue(settingName) or for the most part by proeprties of PortalSettings")]
		public static string GetSiteSetting(int portalId, string settingName)
		{
			string setting;
			PortalController.GetPortalSettingsDictionary(portalId).TryGetValue(settingName, out setting);
			return setting;
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetTabModuleSettings(TabModuleId)")]
		public static Hashtable GetTabModuleSettings(int tabModuleId)
		{
			return new ModuleController().GetTabModuleSettings(tabModuleId);
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetTabModuleSettings(ModuleId)")]
		public static Hashtable GetTabModuleSettings(int tabModuleId, Hashtable moduleSettings)
		{
			Hashtable tabModuleSettings = new ModuleController().GetTabModuleSettings(tabModuleId);

			// add the TabModuleSettings to the ModuleSettings
			foreach (string strKey in tabModuleSettings.Keys)
			{
				moduleSettings[strKey] = tabModuleSettings[strKey];
			}

			return moduleSettings;
		}

		[Obsolete("Deprecated in DNN 5.0.  Please use ModuleController.GetTabModuleSettings(ModuleId)")]
		public static Hashtable GetTabModuleSettings(Hashtable moduleSettings, Hashtable tabModuleSettings)
		{
			// add the TabModuleSettings to the ModuleSettings
			foreach (string strKey in tabModuleSettings.Keys)
			{
				moduleSettings[strKey] = tabModuleSettings[strKey];
			}

			//Return the modifed ModuleSettings
			return moduleSettings;
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.UpdateDatabaseVersion")]
		public static void UpdateDatabaseVersion(int major, int minor, int build)
		{
			DataProvider.Instance().UpdateDatabaseVersion(major, minor, build, DotNetNukeContext.Current.Application.Name);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by DataProvider.UpdatePortalSetting(Integer, String, String)")]
		public static void UpdatePortalSetting(int portalId, string settingName, string settingValue)
		{
			PortalController.UpdatePortalSetting(portalId, settingName, settingValue);
		}

		[Obsolete("Deprecated in DNN 5.0. Replaced by PortalController.UpdatePortalSetting(Integer, String, String)")]
		public static void UpdateSiteSetting(int portalId, string settingName, string settingValue)
		{
			PortalController.UpdatePortalSetting(portalId, settingName, settingValue);
		}

		#endregion
	}
}
