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

        private Dictionary<string, string> _settings;
        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;

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
		/// <history>
		/// 	[cnurse]	10/21/2004	documented
		/// </history>
		/// -----------------------------------------------------------------------------
		public PortalSettings(int tabId, PortalAliasInfo portalAliasInfo)
		{
		    PortalId = portalAliasInfo.PortalID;
			PortalAlias = portalAliasInfo;
			var portal = PortalController.Instance.GetPortal(portalAliasInfo.PortalID);
            BuildPortalSettings(tabId, portal);
        }

		public PortalSettings(PortalInfo portal) 
            : this(Null.NullInteger, portal)
		{
		}

		public PortalSettings(int tabId, PortalInfo portal)
		{
		    PortalId = portal.PortalID;
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

            ActiveTab = PortalSettingsController.Instance().GetActiveTab(tabId, this);
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

        public string HomeDirectoryMapPath { get; private set; }

        public string HomeSystemDirectoryMapPath { get; private set; }

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

		public PortalAliasMapping PortalAliasMappingMode
		{
			get
			{
                return PortalSettingsController.Instance().GetPortalAliasMappingMode(PortalId);
			}
		}

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

		#endregion
	}
}
