#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class	 : ModuleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleInfo provides the Entity Layer for Modules
    /// </summary>
    /// -----------------------------------------------------------------------------
    [XmlRoot("module", IsNullable = false)]
    [Serializable]
    public class ModuleInfo : ContentItem, IPropertyAccess
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModuleInfo));
        private string _authorizedEditRoles;
        private string _authorizedViewRoles;
        private string _cultureCode;
        private Guid _defaultLanguageGuid;
        private ModuleInfo _defaultLanguageModule;
        private DesktopModuleInfo _desktopModule;
        private Dictionary<string, ModuleInfo> _localizedModules;
        private Guid _localizedVersionGuid;
        private ModuleControlInfo _moduleControl;
        private ModuleDefinitionInfo _moduleDefinition;
        private ModulePermissionCollection _modulePermissions;
        private TabInfo _parentTab;
        private Hashtable _moduleSettings;
        private Hashtable _tabModuleSettings;


        public ModuleInfo()
        {
            //initialize the properties that can be null
            //in the database
            PortalID = Null.NullInteger;
            OwnerPortalID = Null.NullInteger;
            TabModuleID = Null.NullInteger;
            DesktopModuleID = Null.NullInteger;
            ModuleDefID = Null.NullInteger;
            ModuleTitle = Null.NullString;
            ModuleVersion = Null.NullInteger;
            _authorizedEditRoles = Null.NullString;
            _authorizedViewRoles = Null.NullString;
            Alignment = Null.NullString;
            Color = Null.NullString;
            Border = Null.NullString;
            IconFile = Null.NullString;
            Header = Null.NullString;
            Footer = Null.NullString;
            StartDate = Null.NullDate;
            EndDate = Null.NullDate;
            ContainerSrc = Null.NullString;
            DisplayTitle = true;
            DisplayPrint = true;
            DisplaySyndicate = false;

            //Guid, Version Guid, and Localized Version Guid should be initialised to a new value
            UniqueId = Guid.NewGuid();
            VersionGuid = Guid.NewGuid();
            _localizedVersionGuid = Guid.NewGuid();

            //Default Language Guid should be initialised to a null Guid
            _defaultLanguageGuid = Null.NullGuid;
        }

        [XmlElement("alignment")]
        public string Alignment { get; set; }

        [XmlIgnore]
        public bool AllModules { get; set; }

        [XmlElement("alltabs")]
        public bool AllTabs { get; set; }

        [XmlElement("border")]
        public string Border { get; set; }

        [XmlElement("cachemethod")]
        public string CacheMethod { get; set; }

        [XmlElement("cachetime")]
        public int CacheTime { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }

        [XmlIgnore]
        public string ContainerPath { get; set; }

        [XmlElement("containersrc")]
        public string ContainerSrc { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Associated Desktop Module
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public DesktopModuleInfo DesktopModule
        {
            get {
                return _desktopModule ??
                       (_desktopModule = DesktopModuleID > Null.NullInteger
                            ? DesktopModuleController.GetDesktopModule(DesktopModuleID, PortalID)
                            : new DesktopModuleInfo());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ID of the Associated Desktop Module
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int DesktopModuleID { get; set; }

        [XmlElement("displayprint")]
        public bool DisplayPrint { get; set; }

        [XmlElement("displaysyndicate")]
        public bool DisplaySyndicate { get; set; }

        [XmlElement("displaytitle")]
        public bool DisplayTitle { get; set; }

        [XmlElement("enddate")]
        public DateTime EndDate { get; set; }

        [XmlElement("footer")]
        public string Footer { get; set; }

        [XmlElement("header")]
        public string Header { get; set; }

        [XmlIgnore]
        public bool HideAdminBorder
        {
            get
            {
                object setting = TabModuleSettings["hideadminborder"];
                if (setting == null || string.IsNullOrEmpty(setting.ToString()))
                {
                    return false;
                }

                bool val;
                Boolean.TryParse(setting.ToString(), out val);
                return val;
            }
        }

        [XmlElement("iconfile")]
        public string IconFile { get; set; }

        [XmlElement("inheritviewpermissions")]
        public bool InheritViewPermissions { get; set; }

        [XmlIgnore]
        public bool IsDefaultModule { get; set; }

        [XmlElement("isdeleted")]
        public bool IsDeleted { get; set; }

        [XmlIgnore]
        public bool IsShareable { get; set; }

        [XmlIgnore]
        public bool IsShared
        {
            get { return OwnerPortalID != PortalID; }
        }

        [XmlIgnore]
        public bool IsShareableViewOnly { get; set; }

        [XmlElement("iswebslice")]
        public bool IsWebSlice { get; set; }

        [XmlIgnore]
        public DateTime LastContentModifiedOnDate { get; set; }

        public ModuleControlInfo ModuleControl
        {
            get {
                return _moduleControl ??
                       (_moduleControl = ModuleControlId > Null.NullInteger
                            ? ModuleControlController.GetModuleControl(ModuleControlId)
                            : new ModuleControlInfo());
            }
        }

        [XmlIgnore]
        public int ModuleControlId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ID of the Associated Module Definition
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Associated Module Definition
        /// </summary>
        /// <returns>A ModuleDefinitionInfo</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public ModuleDefinitionInfo ModuleDefinition
        {
            get {
                return _moduleDefinition ??
                       (_moduleDefinition = ModuleDefID > Null.NullInteger
                            ? ModuleDefinitionController.GetModuleDefinitionByID(ModuleDefID)
                            : new ModuleDefinitionInfo());
            }
        }

        [XmlElement("moduleorder")]
        public int ModuleOrder { get; set; }

        /// <summary>
        /// Get the ModulePermissions for the Module DO NOT USE THE SETTTER
        /// <remarks>
        /// Since 5.0 the setter has been obsolete, directly setting the ModulePermissionCollection is likely an error, change the contenst of the collection instead.
        /// The setter still exists to preserve binary compatibility without the obsolete attribute since c# will not allow only a setter to be obsolete.
        /// </remarks>
        /// </summary>
        [XmlArray("modulepermissions"), XmlArrayItem("permission")]
        public ModulePermissionCollection ModulePermissions
        {
            get
            {
                return _modulePermissions ?? 
                    (_modulePermissions = ModuleID > 0
                            ? new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(ModuleID, TabID))
                            : new ModulePermissionCollection());
            }
            set
            {
                _modulePermissions = value;
            }
        }

        [XmlIgnore]
        public Hashtable ModuleSettings
        {
            get
            {
                if (_moduleSettings == null)
                {
                    if (ModuleID == Null.NullInteger)
                    {
                        _moduleSettings = new Hashtable();
                    }
                    else
                    {
                        _moduleSettings = new ModuleController().GetModuleSettings(ModuleID, TabID);
                    }
                }
                return _moduleSettings;
            }
        }

        [XmlElement("title")]
        public string ModuleTitle { get; set; }

        [XmlIgnore]
        public int ModuleVersion { get; set; }

        [XmlIgnore]
        public int OwnerPortalID { get; set; }

        [XmlIgnore]
        public int PaneModuleCount { get; set; }

        [XmlIgnore]
        public int PaneModuleIndex { get; set; }

        [XmlElement("panename")]
        public string PaneName { get; set; }

        [XmlElement("portalid")]
        public int PortalID { get; set; }

        [XmlElement("startdate")]
        public DateTime StartDate { get; set; }

        [XmlElement("tabmoduleid")]
        public int TabModuleID { get; set; }

        [XmlIgnore]
        public Hashtable TabModuleSettings
        {
            get
            {
                if (_tabModuleSettings == null)
                {
                    if (TabModuleID == Null.NullInteger)
                    {
                        _tabModuleSettings = new Hashtable();
                    }
                    else
                    {
                        _tabModuleSettings = new ModuleController().GetTabModuleSettings(TabModuleID, TabID);
                    }
                }

                return _tabModuleSettings;
            }
        }

        [XmlElement("uniqueId")]
        public Guid UniqueId { get; set; }

        [XmlElement("versionGuid")]
        public Guid VersionGuid { get; set; }

        [XmlElement("visibility")]
        public VisibilityState Visibility { get; set; }

        [XmlElement("websliceexpirydate")]
        public DateTime WebSliceExpiryDate { get; set; }

        [XmlElement("webslicetitle")]
        public string WebSliceTitle { get; set; }

        [XmlElement("webslicettl")]
        public int WebSliceTTL { get; set; }

        #region Localization Properties

        [XmlElement("cultureCode")]
        public string CultureCode
        {
            get
            {
                return _cultureCode;
            }
            set
            {
                _cultureCode = value;
            }
        }

        [XmlElement("defaultLanguageGuid")]
        public Guid DefaultLanguageGuid
        {
            get
            {
                return _defaultLanguageGuid;
            }
            set
            {
                _defaultLanguageGuid = value;
            }
        }

        [XmlIgnore]
        public ModuleInfo DefaultLanguageModule
        {
            get
            {
                if (_defaultLanguageModule == null && (!DefaultLanguageGuid.Equals(Null.NullGuid)) && ParentTab != null && ParentTab.DefaultLanguageTab != null &&
                    ParentTab.DefaultLanguageTab.ChildModules != null)
                {
                    _defaultLanguageModule = (from kvp in ParentTab.DefaultLanguageTab.ChildModules where kvp.Value.UniqueId == DefaultLanguageGuid select kvp.Value).SingleOrDefault();
                }
                return _defaultLanguageModule;
            }
        }

        public bool IsDefaultLanguage
        {
            get
            {
                return (DefaultLanguageGuid == Null.NullGuid);
            }
        }

        public bool IsLocalized
        {
            get
            {
                bool isLocalized = true;
                if (DefaultLanguageModule != null)
                {
                    //Child language
                    isLocalized = ModuleID != DefaultLanguageModule.ModuleID;
                }
                return isLocalized;
            }
        }

        public bool IsNeutralCulture
        {
            get
            {
                return string.IsNullOrEmpty(CultureCode);
            }
        }

        [XmlIgnore]
        public bool IsTranslated
        {
            get
            {
                bool isTranslated = true;
                if (DefaultLanguageModule != null)
                {
                    //Child language
                    isTranslated = (LocalizedVersionGuid == DefaultLanguageModule.LocalizedVersionGuid);
                }
                return isTranslated;
            }
        }

        [XmlIgnore]
        public Dictionary<string, ModuleInfo> LocalizedModules
        {
            get
            {
                if (_localizedModules == null && (DefaultLanguageGuid.Equals(Null.NullGuid)) && ParentTab != null && ParentTab.LocalizedTabs != null)
                {
                    //Cycle through all localized tabs looking for this module
                    _localizedModules = new Dictionary<string, ModuleInfo>();
                    foreach (TabInfo t in ParentTab.LocalizedTabs.Values)
                    {
                        foreach (ModuleInfo m in t.ChildModules.Values)
                        {
                            ModuleInfo tempModuleInfo;
                            if (m.DefaultLanguageGuid == UniqueId && !m.IsDeleted && !_localizedModules.TryGetValue(m.CultureCode, out tempModuleInfo))
                            {
                                _localizedModules.Add(m.CultureCode, m);
                            }
                        }
                    }
                }
                return _localizedModules;
            }
        }

        [XmlElement("localizedVersionGuid")]
        public Guid LocalizedVersionGuid
        {
            get
            {
                return _localizedVersionGuid;
            }
            set
            {
                _localizedVersionGuid = value;
            }
        }

        #endregion

        #region Tab Properties

        public TabInfo ParentTab
        {
            get
            {
                if (_parentTab == null)
                {
                    if (PortalID == Null.NullInteger || string.IsNullOrEmpty(CultureCode))
                    {
                        _parentTab = TabController.Instance.GetTab(TabID, PortalID, false);
                    }
                    else
                    {
                        Locale locale = LocaleController.Instance.GetLocale(CultureCode);
                        _parentTab = TabController.Instance.GetTabByCulture(TabID, PortalID, locale);
                    }
                }
                return _parentTab;
            }
        }

        #endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModuleInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public override void Fill(IDataReader dr)
        {
            //Call the base classes fill method to populate base class properties
            base.FillInternal(dr);

            UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            DefaultLanguageGuid = Null.SetNullGuid(dr["DefaultLanguageGuid"]);
            LocalizedVersionGuid = Null.SetNullGuid(dr["LocalizedVersionGuid"]);
            CultureCode = Null.SetNullString(dr["CultureCode"]);

            PortalID = Null.SetNullInteger(dr["PortalID"]);
			if (dr.GetSchemaTable().Select("ColumnName = 'OwnerPortalID'").Length > 0)
			{
				OwnerPortalID = Null.SetNullInteger(dr["OwnerPortalID"]);
			}

	        ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            ModuleTitle = Null.SetNullString(dr["ModuleTitle"]);
            AllTabs = Null.SetNullBoolean(dr["AllTabs"]);
            IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            InheritViewPermissions = Null.SetNullBoolean(dr["InheritViewPermissions"]);

			if (dr.GetSchemaTable().Select("ColumnName = 'IsShareable'").Length > 0)
			{
				IsShareable = Null.SetNullBoolean(dr["IsShareable"]);
			}

			if (dr.GetSchemaTable().Select("ColumnName = 'IsShareableViewOnly'").Length > 0)
			{
				IsShareableViewOnly = Null.SetNullBoolean(dr["IsShareableViewOnly"]);
			}

	        Header = Null.SetNullString(dr["Header"]);
            Footer = Null.SetNullString(dr["Footer"]);
            StartDate = Null.SetNullDateTime(dr["StartDate"]);
            EndDate = Null.SetNullDateTime(dr["EndDate"]);
            LastContentModifiedOnDate = Null.SetNullDateTime(dr["LastContentModifiedOnDate"]);
            try
            {
                TabModuleID = Null.SetNullInteger(dr["TabModuleID"]);
                ModuleOrder = Null.SetNullInteger(dr["ModuleOrder"]);
                PaneName = Null.SetNullString(dr["PaneName"]);
                CacheTime = Null.SetNullInteger(dr["CacheTime"]);
                CacheMethod = Null.SetNullString(dr["CacheMethod"]);
                Alignment = Null.SetNullString(dr["Alignment"]);
                Color = Null.SetNullString(dr["Color"]);
                Border = Null.SetNullString(dr["Border"]);
                IconFile = Null.SetNullString(dr["IconFile"]);
                int visible = Null.SetNullInteger(dr["Visibility"]);
                if (visible == Null.NullInteger)
                {
                    Visibility = VisibilityState.Maximized;
                }
                else
                {
                    switch (visible)
                    {
                        case 0:
                            Visibility = VisibilityState.Maximized;
                            break;
                        case 1:
                            Visibility = VisibilityState.Minimized;
                            break;
                        case 2:
                            Visibility = VisibilityState.None;
                            break;
                    }
                }
                ContainerSrc = Null.SetNullString(dr["ContainerSrc"]);
                DisplayTitle = Null.SetNullBoolean(dr["DisplayTitle"]);
                DisplayPrint = Null.SetNullBoolean(dr["DisplayPrint"]);
                DisplaySyndicate = Null.SetNullBoolean(dr["DisplaySyndicate"]);
                IsWebSlice = Null.SetNullBoolean(dr["IsWebSlice"]);
                if (IsWebSlice)
                {
                    WebSliceTitle = Null.SetNullString(dr["WebSliceTitle"]);
                    WebSliceExpiryDate = Null.SetNullDateTime(dr["WebSliceExpiryDate"]);
                    WebSliceTTL = Null.SetNullInteger(dr["WebSliceTTL"]);
                }
                DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
                ModuleControlId = Null.SetNullInteger(dr["ModuleControlID"]);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public override int KeyID
        {
            get
            {
                return ModuleID;
            }
            set
            {
                ModuleID = value;
            }
        }

        #endregion

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            string outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }
            if (currentScope == Scope.NoSettings)
            {
                propertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }
            propertyNotFound = true;
            string result = string.Empty;
            bool isPublic = true;
            switch (propertyName.ToLower())
            {
                case "portalid":
                    propertyNotFound = false;
                    result = (PortalID.ToString(outputFormat, formatProvider));
                    break;
                case "displayportalid":
                    propertyNotFound = false;
                    result = (OwnerPortalID.ToString(outputFormat, formatProvider));
                    break;
                case "tabid":
                    propertyNotFound = false;
                    result = (TabID.ToString(outputFormat, formatProvider));
                    break;
                case "tabmoduleid":
                    propertyNotFound = false;
                    result = (TabModuleID.ToString(outputFormat, formatProvider));
                    break;
                case "moduleid":
                    propertyNotFound = false;
                    result = (ModuleID.ToString(outputFormat, formatProvider));
                    break;
                case "moduledefid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (ModuleDefID.ToString(outputFormat, formatProvider));
                    break;
                case "moduleorder":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (ModuleOrder.ToString(outputFormat, formatProvider));
                    break;
                case "panename":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(PaneName, format);
                    break;
                case "moduletitle":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ModuleTitle, format);
                    break;
                case "cachetime":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (CacheTime.ToString(outputFormat, formatProvider));
                    break;
                case "cachemethod":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(CacheMethod, format);
                    break;
                case "alignment":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Alignment, format);
                    break;
                case "color":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Color, format);
                    break;
                case "border":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Border, format);
                    break;
                case "iconfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(IconFile, format);
                    break;
                case "alltabs":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(AllTabs, formatProvider));
                    break;
                case "isdeleted":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsDeleted, formatProvider));
                    break;
                case "header":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Header, format);
                    break;
                case "footer":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Footer, format);
                    break;
                case "startdate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (StartDate.ToString(outputFormat, formatProvider));
                    break;
                case "enddate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (EndDate.ToString(outputFormat, formatProvider));
                    break;
                case "containersrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ContainerSrc, format);
                    break;
                case "displaytitle":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DisplayTitle, formatProvider));
                    break;
                case "displayprint":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DisplayPrint, formatProvider));
                    break;
                case "displaysyndicate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DisplaySyndicate, formatProvider));
                    break;
                case "iswebslice":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsWebSlice, formatProvider));
                    break;
                case "webslicetitle":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(WebSliceTitle, format);
                    break;
                case "websliceexpirydate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (WebSliceExpiryDate.ToString(outputFormat, formatProvider));
                    break;
                case "webslicettl":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (WebSliceTTL.ToString(outputFormat, formatProvider));
                    break;
                case "inheritviewpermissions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(InheritViewPermissions, formatProvider));
                    break;
                case "isshareable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsShareable, formatProvider));
                    break;
                case "isshareableviewonly":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsShareableViewOnly, formatProvider));
                    break;
                case "desktopmoduleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (DesktopModuleID.ToString(outputFormat, formatProvider));
                    break;
                case "friendlyname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.FriendlyName, format);
                    break;
                case "foldername":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.FolderName, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.Description, format);
                    break;
                case "version":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.Version, format);
                    break;
                case "ispremium":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DesktopModule.IsPremium, formatProvider));
                    break;
                case "isadmin":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DesktopModule.IsAdmin, formatProvider));
                    break;
                case "businesscontrollerclass":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.BusinessControllerClass, format);
                    break;
                case "modulename":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.ModuleName, format);
                    break;
                case "supportedfeatures":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (DesktopModule.SupportedFeatures.ToString(outputFormat, formatProvider));
                    break;
                case "compatibleversions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.CompatibleVersions, format);
                    break;
                case "dependencies":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.Dependencies, format);
                    break;
                case "permissions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.Permissions, format);
                    break;
                case "defaultcachetime":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (ModuleDefinition.DefaultCacheTime.ToString(outputFormat, formatProvider));
                    break;
                case "modulecontrolid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (ModuleControlId.ToString(outputFormat, formatProvider));
                    break;
                case "controlsrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ModuleControl.ControlSrc, format);
                    break;
                case "controltitle":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ModuleControl.ControlTitle, format);
                    break;
                case "helpurl":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ModuleControl.HelpURL, format);
                    break;
                case "supportspartialrendering":
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(ModuleControl.SupportsPartialRendering, formatProvider));
                    break;
                case "containerpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ContainerPath, format);
                    break;
                case "panemoduleindex":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PaneModuleIndex.ToString(outputFormat, formatProvider));
                    break;
                case "panemodulecount":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PaneModuleCount.ToString(outputFormat, formatProvider));
                    break;
                case "isdefaultmodule":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsDefaultModule, formatProvider));
                    break;
                case "allmodules":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(AllModules, formatProvider));
                    break;
                case "isportable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DesktopModule.IsPortable, formatProvider));
                    break;
                case "issearchable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DesktopModule.IsSearchable, formatProvider));
                    break;
                case "isupgradeable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DesktopModule.IsUpgradeable, formatProvider));
                    break;
                case "adminpage":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.AdminPage, format);
                    break;
                case "hostpage":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(DesktopModule.HostPage, format);
                    break;
            }
            if (!isPublic && currentScope != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }
            return result;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        #endregion

        public ModuleInfo Clone()
        {
            var objModuleInfo = new ModuleInfo
                                    {
                                        PortalID = PortalID,
                                        OwnerPortalID = OwnerPortalID,
                                        TabID = TabID,
                                        TabModuleID = TabModuleID,
                                        ModuleID = ModuleID,
                                        ModuleOrder = ModuleOrder,
                                        PaneName = PaneName,
                                        ModuleTitle = ModuleTitle,
                                        CacheTime = CacheTime,
                                        CacheMethod = CacheMethod,
                                        Alignment = Alignment,
                                        Color = Color,
                                        Border = Border,
                                        IconFile = IconFile,
                                        AllTabs = AllTabs,
                                        Visibility = Visibility,
                                        IsDeleted = IsDeleted,
                                        Header = Header,
                                        Footer = Footer,
                                        StartDate = StartDate,
                                        EndDate = EndDate,
                                        ContainerSrc = ContainerSrc,
                                        DisplayTitle = DisplayTitle,
                                        DisplayPrint = DisplayPrint,
                                        DisplaySyndicate = DisplaySyndicate,
                                        IsWebSlice = IsWebSlice,
                                        WebSliceTitle = WebSliceTitle,
                                        WebSliceExpiryDate = WebSliceExpiryDate,
                                        WebSliceTTL = WebSliceTTL,
                                        InheritViewPermissions = InheritViewPermissions,
                                        IsShareable = IsShareable,
                                        IsShareableViewOnly = IsShareableViewOnly,
                                        DesktopModuleID = DesktopModuleID,
                                        ModuleDefID = ModuleDefID,
                                        ModuleControlId = ModuleControlId,
                                        ContainerPath = ContainerPath,
                                        PaneModuleIndex = PaneModuleIndex,
                                        PaneModuleCount = PaneModuleCount,
                                        IsDefaultModule = IsDefaultModule,
                                        AllModules = AllModules,
                                        UniqueId = Guid.NewGuid(),
                                        VersionGuid = Guid.NewGuid(),
                                        DefaultLanguageGuid = DefaultLanguageGuid,
                                        LocalizedVersionGuid = LocalizedVersionGuid,
                                        CultureCode = CultureCode
                                    };

            //localized properties
	        Clone(objModuleInfo, this);
            return objModuleInfo;
        }

        public string GetEffectiveCacheMethod()
        {
            string effectiveCacheMethod;
            if (!string.IsNullOrEmpty(CacheMethod))
            {
                effectiveCacheMethod = CacheMethod;
            }
            else if (!string.IsNullOrEmpty(Host.Host.ModuleCachingMethod))
            {
                effectiveCacheMethod = Host.Host.ModuleCachingMethod;
            }
            else
            {
                var defaultModuleCache = ComponentFactory.GetComponent<ModuleCachingProvider>();
                effectiveCacheMethod = (from provider in ModuleCachingProvider.GetProviderList() where provider.Value.Equals(defaultModuleCache) select provider.Key).SingleOrDefault();
            }
            if (string.IsNullOrEmpty(effectiveCacheMethod))
            {
                throw new InvalidOperationException(Localization.GetString("EXCEPTION_ModuleCacheMissing"));
            }
            return effectiveCacheMethod;
        }

        public void Initialize(int portalId)
        {
            PortalID = portalId;
            OwnerPortalID = portalId;
            ModuleDefID = Null.NullInteger;
            ModuleOrder = Null.NullInteger;
            PaneName = Null.NullString;
            ModuleTitle = Null.NullString;
            CacheTime = 0;
            CacheMethod = Null.NullString;
            Alignment = Null.NullString;
            Color = Null.NullString;
            Border = Null.NullString;
            IconFile = Null.NullString;
            AllTabs = Null.NullBoolean;
            Visibility = VisibilityState.Maximized;
            IsDeleted = Null.NullBoolean;
            Header = Null.NullString;
            Footer = Null.NullString;
            StartDate = Null.NullDate;
            EndDate = Null.NullDate;
            DisplayTitle = true;
            DisplayPrint = false;
            DisplaySyndicate = Null.NullBoolean;
            IsWebSlice = Null.NullBoolean;
            WebSliceTitle = "";
            WebSliceExpiryDate = Null.NullDate;
            WebSliceTTL = 0;
            InheritViewPermissions = Null.NullBoolean;
            IsShareable = true;
            IsShareableViewOnly = true;
            ContainerSrc = Null.NullString;
            DesktopModuleID = Null.NullInteger;
            ModuleControlId = Null.NullInteger;
            ContainerPath = Null.NullString;
            PaneModuleIndex = 0;
            PaneModuleCount = 0;
            IsDefaultModule = Null.NullBoolean;
            AllModules = Null.NullBoolean;
            if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
            {
                ModuleInfo objModule = ModuleController.Instance.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
                if (objModule != null)
                {
                    Alignment = objModule.Alignment;
                    Color = objModule.Color;
                    Border = objModule.Border;
                    IconFile = objModule.IconFile;
                    Visibility = objModule.Visibility;
                    ContainerSrc = objModule.ContainerSrc;
                    DisplayTitle = objModule.DisplayTitle;
                    DisplayPrint = objModule.DisplayPrint;
                    DisplaySyndicate = objModule.DisplaySyndicate;
                }
            }
        }
    }
}