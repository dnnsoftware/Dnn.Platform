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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Skins;

#endregion

namespace DotNetNuke.Entities.Portals
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PortalSettings Class
    ///
    /// This class encapsulates all of the settings for the Portal, as well
    /// as the configuration settings required to execute the current tab
    /// view within the portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	10/21/2004	documented
    /// 	[cnurse]	10/21/2004	added GetTabModuleSettings
    /// </history>
    /// -----------------------------------------------------------------------------
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

        private string _version;

        #region Constructors

        public PortalSettings()
        {
        }

        public PortalSettings(int portalID)
            : this(Null.NullInteger, portalID)
        {
        }

        public PortalSettings(int tabID, int portalID)
        {
            var portal = PortalController.Instance.GetPortal(portalID);
            GetPortalSettings(tabID, portal);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The PortalSettings Constructor encapsulates all of the logic
        /// necessary to obtain configuration settings necessary to render
        /// a Portal Tab view for a given request.
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="tabID">The current tab</param>
        ///	<param name="objPortalAliasInfo">The current portal</param>
        /// <history>
        /// 	[cnurse]	10/21/2004	documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public PortalSettings(int tabID, PortalAliasInfo objPortalAliasInfo)
        {
            ActiveTab = new TabInfo();
            PortalId = objPortalAliasInfo.PortalID;
            PortalAlias = objPortalAliasInfo;
            var portal = PortalController.Instance.GetPortal(PortalId);
            if (portal != null)
            {
                GetPortalSettings(tabID, portal);
            }
        }

        public PortalSettings(PortalInfo portal)
        {
            ActiveTab = new TabInfo();
            GetPortalSettings(Null.NullInteger, portal);
        }

        public PortalSettings(int tabID, PortalInfo portal)
        {
            ActiveTab = new TabInfo();
            GetPortalSettings(tabID, portal);
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
        public int ErrorPage404 { get; private set; }
        public int ErrorPage500 { get; private set; }
        public int SiteLogHistory { get; set; }
        public int SplashTabId { get; set; }
        public int SuperTabId { get; set; }
        public int UserQuota { get; set; }
        public int UserRegistration { get; set; }
        public int Users { get; set; }
        public int UserTabId { get; set; }

        #endregion

        #region Public Properties

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
        public bool AllowUserUICulture
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("AllowUserUICulture", PortalId, false);
            }
        }

        public int CdfVersion
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("CdfVersion", PortalId, Null.NullInteger);
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public bool ContentLocalizationEnabled
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", PortalId, false);
            }
        }

        public ControlPanelPermission ControlPanelSecurity
        {
            get
            {
                ControlPanelPermission security = ControlPanelPermission.ModuleEditor;
                string setting;
                if (PortalController.GetPortalSettingsDictionary(PortalId).TryGetValue("ControlPanelSecurity", out setting))
                {
                    security = (setting.ToUpperInvariant() == "TAB") ? ControlPanelPermission.TabEditor : ControlPanelPermission.ModuleEditor;
                }
                return security;
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

        public string DefaultAdminContainer
        {
            get
            {
                return PortalController.GetPortalSetting("DefaultAdminContainer", PortalId, Host.Host.DefaultAdminContainer);
            }
        }

        public string DefaultAdminSkin
        {
            get
            {
                return PortalController.GetPortalSetting("DefaultAdminSkin", PortalId, Host.Host.DefaultAdminSkin);
            }
        }

        public Mode DefaultControlPanelMode
        {
            get
            {
                var mode = Mode.View;
                string setting;
                if (PortalController.GetPortalSettingsDictionary(PortalId).TryGetValue("ControlPanelMode", out setting))
                {
                    if (setting.ToUpperInvariant() == "EDIT")
                    {
                        mode = Mode.Edit;
                    }
                }
                return mode;
            }
        }

        public bool DefaultControlPanelVisibility
        {
            get
            {
                bool isVisible = true;
                string setting;
                if (PortalController.GetPortalSettingsDictionary(PortalId).TryGetValue("ControlPanelVisibility", out setting))
                {
                    isVisible = setting.ToUpperInvariant() != "MIN";
                }
                return isVisible;
            }
        }

        public string DefaultIconLocation
        {
            get
            {
                return PortalController.GetPortalSetting("DefaultIconLocation", PortalId, "icons/sigma");
            }
        }
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Module Id
        /// </summary>
        /// <remarks>Defaults to Null.NullInteger</remarks>
        /// <history>
        /// 	[cnurse]	05/02/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int DefaultModuleId
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("defaultmoduleid", PortalId, Null.NullInteger);
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

        public string DefaultPortalContainer
        {
            get
            {
                return PortalController.GetPortalSetting("DefaultPortalContainer", PortalId, Host.Host.DefaultPortalContainer);
            }
        }

        public string DefaultPortalSkin
        {
            get
            {
                return PortalController.GetPortalSetting("DefaultPortalSkin", PortalId, Host.Host.DefaultPortalSkin);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Tab Id
        /// </summary>
        /// <remarks>Defaults to Null.NullInteger</remarks>
        /// <history>
        /// 	[cnurse]	05/02/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int DefaultTabId
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("defaulttabid", PortalId, Null.NullInteger);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether Browser Language Detection is Enabled
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <history>
        /// 	[cnurse]	02/19/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool EnableBrowserLanguage
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("EnableBrowserLanguage", PortalId, Host.Host.EnableBrowserLanguage);
            }
        }

        public bool EnableCompositeFiles
		{
			get
			{
                return PortalController.GetPortalSettingAsBoolean("EnableCompositeFiles", PortalId, false);
			}
		}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to use the popup.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnablePopUps
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("EnablePopups", PortalId, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to use the module effect in edit mode.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableModuleEffect
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("EnableModuleEffect", PortalId, true);
            }
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether hide the login link.
		/// </summary>
		/// <remarks>Defaults to False.</remarks>
		/// -----------------------------------------------------------------------------
		public bool HideLoginControl
		{
			get
			{
				return PortalController.GetPortalSettingAsBoolean("HideLoginControl", PortalId, false);
			}
		}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Skin Widgets are enabled/supported
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// <history>
        /// 	[cnurse]	07/03/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool EnableSkinWidgets
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("EnableSkinWidgets", PortalId, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether enable url language.
        /// </summary>
        /// <remarks>Defaults to True</remarks>
        /// -----------------------------------------------------------------------------
        public bool EnableUrlLanguage
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("EnableUrlLanguage", PortalId, Host.Host.EnableUrlLanguage);
            }
        }

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
        public bool HideFoldersEnabled
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("HideFoldersEnabled", PortalId, true);
            }
        }

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
        public bool InlineEditorEnabled
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("InlineEditorEnabled", PortalId, true);
            }
        }

        public PortalAliasMapping PortalAliasMappingMode
        {
            get
            {
                return GetPortalAliasMappingMode(PortalId);
            }
        }

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to inlcude Common Words in the Search Index
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// <history>
        /// 	[cnurse]	03/10/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeCommon
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("SearchIncludeCommon", PortalId, Host.Host.SearchIncludeNumeric);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to inlcude Numbers in the Search Index
        /// </summary>
        /// <remarks>Defaults to False</remarks>
        /// <history>
        /// 	[cnurse]	03/10/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool SearchIncludeNumeric
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("SearchIncludeNumeric", PortalId, Host.Host.SearchIncludeNumeric);
            }
        }

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
        public string SearchIncludedTagInfoFilter
        {
            get
            {
                return PortalController.GetPortalSetting("SearchIncludedTagInfoFilter", PortalId, Host.Host.SearchIncludedTagInfoFilter);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum Search Word length to index
        /// </summary>
        /// <remarks>Defaults to 3</remarks>
        /// <history>
        /// 	[cnurse]	03/10/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int SearchMaxWordlLength
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("MaxSearchWordLength", PortalId, Host.Host.SearchMaxWordlLength);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the minum Search Word length to index
        /// </summary>
        /// <remarks>Defaults to 3</remarks>
        /// <history>
        /// 	[cnurse]	03/10/2008   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int SearchMinWordlLength
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("MinSearchWordLength", PortalId, Host.Host.SearchMinWordlLength);
            }
        }

        public bool SSLEnabled
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("SSLEnabled", PortalId, false);
            }
        }

        public bool SSLEnforced
        {
            get
            {
                return PortalController.GetPortalSettingAsBoolean("SSLEnforced", PortalId, false);
            }
        }

        public string SSLURL
        {
            get
            {
                return PortalController.GetPortalSetting("SSLURL", PortalId, Null.NullString);
            }
        }

        public string STDURL
        {
            get
            {
                return PortalController.GetPortalSetting("STDURL", PortalId, Null.NullString);
            }
        }

        public TimeZoneInfo TimeZone
        {
            get
            {
                //check if there is a PortalSetting
                string timeZoneId = PortalController.GetPortalSetting("TimeZone", PortalId, string.Empty);
                if (!string.IsNullOrEmpty(timeZoneId))
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    if (timeZone != null)
                        return timeZone;
                }

                return TimeZoneInfo.Local;
            }
            set
            {
                PortalController.UpdatePortalSetting(PortalId, "TimeZone", value.Id, true);
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

		/// <summary>
		/// Website Administrator whether receive the notification email when new user register.
		/// </summary>
	    public bool EnableRegisterNotification
	    {
		    get
		    {
				return PortalController.GetPortalSettingAsBoolean("EnableRegisterNotification", PortalId, true);
		    }
	    }

        /// <summary>
        /// Website Administrator whether receive the notification email when new user register.
        /// </summary>
        public string DefaultAuthProvider
        {
            get
            {
                return PortalController.GetPortalSetting("DefaultAuthProvider", PortalId, "DNN");
            }
        }

        public int SMTPConnectionLimit
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", PortalId, 1);
            }
        }

        public int SMTPMaxIdleTime
        {
            get
            {
                return PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", PortalId,  0);
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
            ActiveTab.Modules = new ArrayList();
            var crumbs = new ArrayList();
            GetBreadCrumbsRecursively(ref crumbs, ActiveTab.TabID);
            ActiveTab.BreadCrumbs = crumbs;
        }

        private void ConfigureModule(ModuleInfo cloneModule)
        {
            if (Null.IsNull(cloneModule.StartDate))
            {
                cloneModule.StartDate = DateTime.MinValue;
            }
            if (Null.IsNull(cloneModule.EndDate))
            {
                cloneModule.EndDate = DateTime.MaxValue;
            }
            if (String.IsNullOrEmpty(cloneModule.ContainerSrc))
            {
                cloneModule.ContainerSrc = ActiveTab.ContainerSrc;
            }

            cloneModule.ContainerSrc = SkinController.FormatSkinSrc(cloneModule.ContainerSrc, this);
            cloneModule.ContainerPath = SkinController.FormatSkinPath(cloneModule.ContainerSrc);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The GetPortalSettings method builds the site Settings
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="tabID">The current tabs id</param>
        ///	<param name="portal">The Portal object</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        private void GetPortalSettings(int tabID, PortalInfo portal)
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
            HostSpace = portal.HostSpace;
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
            DefaultLanguage = portal.DefaultLanguage;
            //HomeDirectory = portal.HomeDirectory;            
            HomeDirectoryMapPath = portal.HomeDirectoryMapPath;
            HomeSystemDirectoryMapPath = portal.HomeSystemDirectoryMapPath;
            Pages = portal.Pages;
            Users = portal.Users;
            CultureCode = portal.CultureCode;

            //update properties with default values
            if (Null.IsNull(HostSpace))
            {
                HostSpace = 0;
            }
            if (Null.IsNull(DefaultLanguage))
            {
                DefaultLanguage = Localization.SystemLocale;
            }
            HomeDirectory = Globals.ApplicationPath + "/" + portal.HomeDirectory + "/";
            HomeSystemDirectory = Globals.ApplicationPath + "/" + portal.HomeSystemDirectory + "/";

            //verify tab for portal. This assigns the Active Tab based on the Tab Id/PortalId
            if (VerifyPortalTab(PortalId, tabID))
            {
                if (ActiveTab != null)
                {
                    ConfigureActiveTab();
                }
            }
            if (ActiveTab != null)
            {
                var objPaneModules = new Dictionary<string, int>();
                foreach (ModuleInfo cloneModule in ActiveTab.ChildModules.Select(kvp => kvp.Value.Clone()))
                {
                    ConfigureModule(cloneModule);

                    if (objPaneModules.ContainsKey(cloneModule.PaneName) == false)
                    {
                        objPaneModules.Add(cloneModule.PaneName, 0);
                    }
                    cloneModule.PaneModuleCount = 0;
                    if (!cloneModule.IsDeleted)
                    {
                        objPaneModules[cloneModule.PaneName] = objPaneModules[cloneModule.PaneName] + 1;
                        cloneModule.PaneModuleIndex = objPaneModules[cloneModule.PaneName] - 1;
                    }

                    ActiveTab.Modules.Add(cloneModule);
                }
                foreach (ModuleInfo module in ActiveTab.Modules)
                {
                    module.PaneModuleCount = objPaneModules[module.PaneName];
                }
            }
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
                TabInfo tab = (from TabInfo t in portalTabs.AsList() where !t.IsDeleted && t.IsVisible select t).FirstOrDefault();

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
