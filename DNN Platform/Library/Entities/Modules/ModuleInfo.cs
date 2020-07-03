// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
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

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class    : ModuleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleInfo provides the Entity Layer for Modules.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [XmlRoot("module", IsNullable = false)]
    [Serializable]
    public class ModuleInfo : ContentItem, IPropertyAccess
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleInfo));
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
            // initialize the properties that can be null
            // in the database
            this.PortalID = Null.NullInteger;
            this.OwnerPortalID = Null.NullInteger;
            this.TabModuleID = Null.NullInteger;
            this.DesktopModuleID = Null.NullInteger;
            this.ModuleDefID = Null.NullInteger;
            this.ModuleTitle = Null.NullString;
            this.ModuleVersion = Null.NullInteger;
            this._authorizedEditRoles = Null.NullString;
            this._authorizedViewRoles = Null.NullString;
            this.Alignment = Null.NullString;
            this.Color = Null.NullString;
            this.Border = Null.NullString;
            this.IconFile = Null.NullString;
            this.Header = Null.NullString;
            this.Footer = Null.NullString;
            this.StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this.ContainerSrc = Null.NullString;
            this.DisplayTitle = true;
            this.DisplayPrint = true;
            this.DisplaySyndicate = false;

            // Guid, Version Guid, and Localized Version Guid should be initialised to a new value
            this.UniqueId = Guid.NewGuid();
            this.VersionGuid = Guid.NewGuid();
            this._localizedVersionGuid = Guid.NewGuid();

            // Default Language Guid should be initialised to a null Guid
            this._defaultLanguageGuid = Null.NullGuid;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Associated Desktop Module.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public DesktopModuleInfo DesktopModule
        {
            get
            {
                return this._desktopModule ??
                       (this._desktopModule = this.DesktopModuleID > Null.NullInteger
                            ? DesktopModuleController.GetDesktopModule(this.DesktopModuleID, this.PortalID)
                            : new DesktopModuleInfo());
            }
        }

        [XmlIgnore]
        public bool HideAdminBorder
        {
            get
            {
                object setting = this.TabModuleSettings["hideadminborder"];
                if (setting == null || string.IsNullOrEmpty(setting.ToString()))
                {
                    return false;
                }

                bool val;
                bool.TryParse(setting.ToString(), out val);
                return val;
            }
        }

        [XmlIgnore]
        public bool IsShared
        {
            get { return this.OwnerPortalID != this.PortalID; }
        }

        public ModuleControlInfo ModuleControl
        {
            get
            {
                return this._moduleControl ??
                       (this._moduleControl = this.ModuleControlId > Null.NullInteger
                            ? ModuleControlController.GetModuleControl(this.ModuleControlId)
                            : new ModuleControlInfo());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Associated Module Definition.
        /// </summary>
        /// <returns>A ModuleDefinitionInfo.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public ModuleDefinitionInfo ModuleDefinition
        {
            get
            {
                return this._moduleDefinition ??
                       (this._moduleDefinition = this.ModuleDefID > Null.NullInteger
                            ? ModuleDefinitionController.GetModuleDefinitionByID(this.ModuleDefID)
                            : new ModuleDefinitionInfo());
            }
        }

        [XmlIgnore]
        public Hashtable ModuleSettings
        {
            get
            {
                if (this._moduleSettings == null)
                {
                    if (this.ModuleID == Null.NullInteger)
                    {
                        this._moduleSettings = new Hashtable();
                    }
                    else
                    {
                        this._moduleSettings = new ModuleController().GetModuleSettings(this.ModuleID, this.TabID);
                    }
                }

                return this._moduleSettings;
            }
        }

        [XmlIgnore]
        public Hashtable TabModuleSettings
        {
            get
            {
                if (this._tabModuleSettings == null)
                {
                    if (this.TabModuleID == Null.NullInteger)
                    {
                        this._tabModuleSettings = new Hashtable();
                    }
                    else
                    {
                        this._tabModuleSettings = new ModuleController().GetTabModuleSettings(this.TabModuleID, this.TabID);
                    }
                }

                return this._tabModuleSettings;
            }
        }

        [XmlIgnore]
        public ModuleInfo DefaultLanguageModule
        {
            get
            {
                if (this._defaultLanguageModule == null && (!this.DefaultLanguageGuid.Equals(Null.NullGuid)) && this.ParentTab != null && this.ParentTab.DefaultLanguageTab != null &&
                    this.ParentTab.DefaultLanguageTab.ChildModules != null)
                {
                    this._defaultLanguageModule = (from kvp in this.ParentTab.DefaultLanguageTab.ChildModules where kvp.Value.UniqueId == this.DefaultLanguageGuid select kvp.Value).SingleOrDefault();
                }

                return this._defaultLanguageModule;
            }
        }

        public bool IsDefaultLanguage
        {
            get
            {
                return this.DefaultLanguageGuid == Null.NullGuid;
            }
        }

        public bool IsLocalized
        {
            get
            {
                bool isLocalized = true;
                if (this.DefaultLanguageModule != null)
                {
                    // Child language
                    isLocalized = this.ModuleID != this.DefaultLanguageModule.ModuleID;
                }

                return isLocalized;
            }
        }

        public bool IsNeutralCulture
        {
            get
            {
                return string.IsNullOrEmpty(this.CultureCode);
            }
        }

        [XmlIgnore]
        public bool IsTranslated
        {
            get
            {
                bool isTranslated = true;
                if (this.DefaultLanguageModule != null)
                {
                    // Child language
                    isTranslated = this.LocalizedVersionGuid == this.DefaultLanguageModule.LocalizedVersionGuid;
                }

                return isTranslated;
            }
        }

        [XmlIgnore]
        public Dictionary<string, ModuleInfo> LocalizedModules
        {
            get
            {
                if (this._localizedModules == null && this.DefaultLanguageGuid.Equals(Null.NullGuid) && this.ParentTab != null && this.ParentTab.LocalizedTabs != null)
                {
                    // Cycle through all localized tabs looking for this module
                    this._localizedModules = new Dictionary<string, ModuleInfo>();
                    foreach (TabInfo t in this.ParentTab.LocalizedTabs.Values)
                    {
                        foreach (ModuleInfo m in t.ChildModules.Values)
                        {
                            ModuleInfo tempModuleInfo;
                            if (m.DefaultLanguageGuid == this.UniqueId && !m.IsDeleted && !this._localizedModules.TryGetValue(m.CultureCode, out tempModuleInfo))
                            {
                                this._localizedModules.Add(m.CultureCode, m);
                            }
                        }
                    }
                }

                return this._localizedModules;
            }
        }

        public TabInfo ParentTab
        {
            get
            {
                if (this._parentTab == null)
                {
                    if (this.PortalID == Null.NullInteger || string.IsNullOrEmpty(this.CultureCode))
                    {
                        this._parentTab = TabController.Instance.GetTab(this.TabID, this.PortalID, false);
                    }
                    else
                    {
                        Locale locale = LocaleController.Instance.GetLocale(this.CultureCode);
                        this._parentTab = TabController.Instance.GetTabByCulture(this.TabID, this.PortalID, locale);
                    }
                }

                return this._parentTab;
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
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
        /// Gets or sets and sets the ID of the Associated Desktop Module.
        /// </summary>
        /// <returns>An Integer.</returns>
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
        public bool IsShareableViewOnly { get; set; }

        [XmlElement("iswebslice")]
        public bool IsWebSlice { get; set; }

        [XmlIgnore]
        public DateTime LastContentModifiedOnDate { get; set; }

        [XmlIgnore]
        public int ModuleControlId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ID of the Associated Module Definition.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int ModuleDefID { get; set; }

        [XmlElement("moduleorder")]
        public int ModuleOrder { get; set; }

        /// <summary>
        /// Gets or sets get the ModulePermissions for the Module DO NOT USE THE SETTTER.
        /// <remarks>
        /// Since 5.0 the setter has been obsolete, directly setting the ModulePermissionCollection is likely an error, change the contenst of the collection instead.
        /// The setter still exists to preserve binary compatibility without the obsolete attribute since c# will not allow only a setter to be obsolete.
        /// </remarks>
        /// </summary>
        [XmlArray("modulepermissions")]
        [XmlArrayItem("permission")]
        public ModulePermissionCollection ModulePermissions
        {
            get
            {
                return this._modulePermissions ??
                    (this._modulePermissions = this.ModuleID > 0
                            ? new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(this.ModuleID, this.TabID))
                            : new ModulePermissionCollection());
            }

            set
            {
                this._modulePermissions = value;
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

        [XmlElement("cultureCode")]
        public string CultureCode
        {
            get
            {
                return this._cultureCode;
            }

            set
            {
                this._cultureCode = value;
            }
        }

        [XmlElement("defaultLanguageGuid")]
        public Guid DefaultLanguageGuid
        {
            get
            {
                return this._defaultLanguageGuid;
            }

            set
            {
                this._defaultLanguageGuid = value;
            }
        }

        [XmlElement("localizedVersionGuid")]
        public Guid LocalizedVersionGuid
        {
            get
            {
                return this._localizedVersionGuid;
            }

            set
            {
                this._localizedVersionGuid = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public override int KeyID
        {
            get
            {
                return this.ModuleID;
            }

            set
            {
                this.ModuleID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModuleInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public override void Fill(IDataReader dr)
        {
            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);

            this.UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            this.VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            this.DefaultLanguageGuid = Null.SetNullGuid(dr["DefaultLanguageGuid"]);
            this.LocalizedVersionGuid = Null.SetNullGuid(dr["LocalizedVersionGuid"]);
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);

            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            if (dr.GetSchemaTable().Select("ColumnName = 'OwnerPortalID'").Length > 0)
            {
                this.OwnerPortalID = Null.SetNullInteger(dr["OwnerPortalID"]);
            }

            this.ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            this.ModuleTitle = Null.SetNullString(dr["ModuleTitle"]);
            this.AllTabs = Null.SetNullBoolean(dr["AllTabs"]);
            this.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            this.InheritViewPermissions = Null.SetNullBoolean(dr["InheritViewPermissions"]);

            if (dr.GetSchemaTable().Select("ColumnName = 'IsShareable'").Length > 0)
            {
                this.IsShareable = Null.SetNullBoolean(dr["IsShareable"]);
            }

            if (dr.GetSchemaTable().Select("ColumnName = 'IsShareableViewOnly'").Length > 0)
            {
                this.IsShareableViewOnly = Null.SetNullBoolean(dr["IsShareableViewOnly"]);
            }

            this.Header = Null.SetNullString(dr["Header"]);
            this.Footer = Null.SetNullString(dr["Footer"]);
            this.StartDate = Null.SetNullDateTime(dr["StartDate"]);
            this.EndDate = Null.SetNullDateTime(dr["EndDate"]);
            this.LastContentModifiedOnDate = Null.SetNullDateTime(dr["LastContentModifiedOnDate"]);
            try
            {
                this.TabModuleID = Null.SetNullInteger(dr["TabModuleID"]);
                this.ModuleOrder = Null.SetNullInteger(dr["ModuleOrder"]);
                this.PaneName = Null.SetNullString(dr["PaneName"]);
                this.CacheTime = Null.SetNullInteger(dr["CacheTime"]);
                this.CacheMethod = Null.SetNullString(dr["CacheMethod"]);
                this.Alignment = Null.SetNullString(dr["Alignment"]);
                this.Color = Null.SetNullString(dr["Color"]);
                this.Border = Null.SetNullString(dr["Border"]);
                this.IconFile = Null.SetNullString(dr["IconFile"]);
                int visible = Null.SetNullInteger(dr["Visibility"]);
                if (visible == Null.NullInteger)
                {
                    this.Visibility = VisibilityState.Maximized;
                }
                else
                {
                    switch (visible)
                    {
                        case 0:
                            this.Visibility = VisibilityState.Maximized;
                            break;
                        case 1:
                            this.Visibility = VisibilityState.Minimized;
                            break;
                        case 2:
                            this.Visibility = VisibilityState.None;
                            break;
                    }
                }

                this.ContainerSrc = Null.SetNullString(dr["ContainerSrc"]);
                this.DisplayTitle = Null.SetNullBoolean(dr["DisplayTitle"]);
                this.DisplayPrint = Null.SetNullBoolean(dr["DisplayPrint"]);
                this.DisplaySyndicate = Null.SetNullBoolean(dr["DisplaySyndicate"]);
                this.IsWebSlice = Null.SetNullBoolean(dr["IsWebSlice"]);
                if (this.IsWebSlice)
                {
                    this.WebSliceTitle = Null.SetNullString(dr["WebSliceTitle"]);
                    this.WebSliceExpiryDate = Null.SetNullDateTime(dr["WebSliceExpiryDate"]);
                    this.WebSliceTTL = Null.SetNullInteger(dr["WebSliceTTL"]);
                }

                this.DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
                this.ModuleControlId = Null.SetNullInteger(dr["ModuleControlID"]);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

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
            switch (propertyName.ToLowerInvariant())
            {
                case "portalid":
                    propertyNotFound = false;
                    result = this.PortalID.ToString(outputFormat, formatProvider);
                    break;
                case "displayportalid":
                    propertyNotFound = false;
                    result = this.OwnerPortalID.ToString(outputFormat, formatProvider);
                    break;
                case "tabid":
                    propertyNotFound = false;
                    result = this.TabID.ToString(outputFormat, formatProvider);
                    break;
                case "tabmoduleid":
                    propertyNotFound = false;
                    result = this.TabModuleID.ToString(outputFormat, formatProvider);
                    break;
                case "moduleid":
                    propertyNotFound = false;
                    result = this.ModuleID.ToString(outputFormat, formatProvider);
                    break;
                case "moduledefid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleDefID.ToString(outputFormat, formatProvider);
                    break;
                case "moduleorder":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleOrder.ToString(outputFormat, formatProvider);
                    break;
                case "panename":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.PaneName, format);
                    break;
                case "moduletitle":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleTitle, format);
                    break;
                case "cachetime":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.CacheTime.ToString(outputFormat, formatProvider);
                    break;
                case "cachemethod":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.CacheMethod, format);
                    break;
                case "alignment":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Alignment, format);
                    break;
                case "color":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Color, format);
                    break;
                case "border":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Border, format);
                    break;
                case "iconfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.IconFile, format);
                    break;
                case "alltabs":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.AllTabs, formatProvider);
                    break;
                case "isdeleted":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsDeleted, formatProvider);
                    break;
                case "header":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Header, format);
                    break;
                case "footer":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Footer, format);
                    break;
                case "startdate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.StartDate.ToString(outputFormat, formatProvider);
                    break;
                case "enddate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.EndDate.ToString(outputFormat, formatProvider);
                    break;
                case "containersrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ContainerSrc, format);
                    break;
                case "displaytitle":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisplayTitle, formatProvider);
                    break;
                case "displayprint":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisplayPrint, formatProvider);
                    break;
                case "displaysyndicate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisplaySyndicate, formatProvider);
                    break;
                case "iswebslice":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsWebSlice, formatProvider);
                    break;
                case "webslicetitle":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.WebSliceTitle, format);
                    break;
                case "websliceexpirydate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.WebSliceExpiryDate.ToString(outputFormat, formatProvider);
                    break;
                case "webslicettl":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.WebSliceTTL.ToString(outputFormat, formatProvider);
                    break;
                case "inheritviewpermissions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.InheritViewPermissions, formatProvider);
                    break;
                case "isshareable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsShareable, formatProvider);
                    break;
                case "isshareableviewonly":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsShareableViewOnly, formatProvider);
                    break;
                case "desktopmoduleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.DesktopModuleID.ToString(outputFormat, formatProvider);
                    break;
                case "friendlyname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.FriendlyName, format);
                    break;
                case "foldername":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.FolderName, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Description, format);
                    break;
                case "version":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Version, format);
                    break;
                case "ispremium":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsPremium, formatProvider);
                    break;
                case "isadmin":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsAdmin, formatProvider);
                    break;
                case "businesscontrollerclass":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.BusinessControllerClass, format);
                    break;
                case "modulename":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.ModuleName, format);
                    break;
                case "supportedfeatures":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.DesktopModule.SupportedFeatures.ToString(outputFormat, formatProvider);
                    break;
                case "compatibleversions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.CompatibleVersions, format);
                    break;
                case "dependencies":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Dependencies, format);
                    break;
                case "permissions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Permissions, format);
                    break;
                case "defaultcachetime":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleDefinition.DefaultCacheTime.ToString(outputFormat, formatProvider);
                    break;
                case "modulecontrolid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleControlId.ToString(outputFormat, formatProvider);
                    break;
                case "controlsrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleControl.ControlSrc, format);
                    break;
                case "controltitle":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleControl.ControlTitle, format);
                    break;
                case "helpurl":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleControl.HelpURL, format);
                    break;
                case "supportspartialrendering":
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.ModuleControl.SupportsPartialRendering, formatProvider);
                    break;
                case "containerpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ContainerPath, format);
                    break;
                case "panemoduleindex":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.PaneModuleIndex.ToString(outputFormat, formatProvider);
                    break;
                case "panemodulecount":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.PaneModuleCount.ToString(outputFormat, formatProvider);
                    break;
                case "isdefaultmodule":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsDefaultModule, formatProvider);
                    break;
                case "allmodules":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.AllModules, formatProvider);
                    break;
                case "isportable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsPortable, formatProvider);
                    break;
                case "issearchable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsSearchable, formatProvider);
                    break;
                case "isupgradeable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsUpgradeable, formatProvider);
                    break;
                case "adminpage":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.AdminPage, format);
                    break;
                case "hostpage":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.HostPage, format);
                    break;
            }

            if (!isPublic && currentScope != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }

            return result;
        }

        public ModuleInfo Clone()
        {
            var objModuleInfo = new ModuleInfo
            {
                PortalID = this.PortalID,
                OwnerPortalID = this.OwnerPortalID,
                TabID = this.TabID,
                TabModuleID = this.TabModuleID,
                ModuleID = this.ModuleID,
                ModuleOrder = this.ModuleOrder,
                PaneName = this.PaneName,
                ModuleTitle = this.ModuleTitle,
                CacheTime = this.CacheTime,
                CacheMethod = this.CacheMethod,
                Alignment = this.Alignment,
                Color = this.Color,
                Border = this.Border,
                IconFile = this.IconFile,
                AllTabs = this.AllTabs,
                Visibility = this.Visibility,
                IsDeleted = this.IsDeleted,
                Header = this.Header,
                Footer = this.Footer,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                ContainerSrc = this.ContainerSrc,
                DisplayTitle = this.DisplayTitle,
                DisplayPrint = this.DisplayPrint,
                DisplaySyndicate = this.DisplaySyndicate,
                IsWebSlice = this.IsWebSlice,
                WebSliceTitle = this.WebSliceTitle,
                WebSliceExpiryDate = this.WebSliceExpiryDate,
                WebSliceTTL = this.WebSliceTTL,
                InheritViewPermissions = this.InheritViewPermissions,
                IsShareable = this.IsShareable,
                IsShareableViewOnly = this.IsShareableViewOnly,
                DesktopModuleID = this.DesktopModuleID,
                ModuleDefID = this.ModuleDefID,
                ModuleControlId = this.ModuleControlId,
                ContainerPath = this.ContainerPath,
                PaneModuleIndex = this.PaneModuleIndex,
                PaneModuleCount = this.PaneModuleCount,
                IsDefaultModule = this.IsDefaultModule,
                AllModules = this.AllModules,
                UniqueId = Guid.NewGuid(),
                VersionGuid = Guid.NewGuid(),
                DefaultLanguageGuid = this.DefaultLanguageGuid,
                LocalizedVersionGuid = this.LocalizedVersionGuid,
                CultureCode = this.CultureCode,
            };

            // localized properties
            this.Clone(objModuleInfo, this);
            return objModuleInfo;
        }

        public string GetEffectiveCacheMethod()
        {
            string effectiveCacheMethod;
            if (!string.IsNullOrEmpty(this.CacheMethod))
            {
                effectiveCacheMethod = this.CacheMethod;
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
            this.PortalID = portalId;
            this.OwnerPortalID = portalId;
            this.ModuleDefID = Null.NullInteger;
            this.ModuleOrder = Null.NullInteger;
            this.PaneName = Null.NullString;
            this.ModuleTitle = Null.NullString;
            this.CacheTime = 0;
            this.CacheMethod = Null.NullString;
            this.Alignment = Null.NullString;
            this.Color = Null.NullString;
            this.Border = Null.NullString;
            this.IconFile = Null.NullString;
            this.AllTabs = Null.NullBoolean;
            this.Visibility = VisibilityState.Maximized;
            this.IsDeleted = Null.NullBoolean;
            this.Header = Null.NullString;
            this.Footer = Null.NullString;
            this.StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this.DisplayTitle = true;
            this.DisplayPrint = false;
            this.DisplaySyndicate = Null.NullBoolean;
            this.IsWebSlice = Null.NullBoolean;
            this.WebSliceTitle = string.Empty;
            this.WebSliceExpiryDate = Null.NullDate;
            this.WebSliceTTL = 0;
            this.InheritViewPermissions = Null.NullBoolean;
            this.IsShareable = true;
            this.IsShareableViewOnly = true;
            this.ContainerSrc = Null.NullString;
            this.DesktopModuleID = Null.NullInteger;
            this.ModuleControlId = Null.NullInteger;
            this.ContainerPath = Null.NullString;
            this.PaneModuleIndex = 0;
            this.PaneModuleCount = 0;
            this.IsDefaultModule = Null.NullBoolean;
            this.AllModules = Null.NullBoolean;
            if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
            {
                ModuleInfo objModule = ModuleController.Instance.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
                if (objModule != null)
                {
                    this.Alignment = objModule.Alignment;
                    this.Color = objModule.Color;
                    this.Border = objModule.Border;
                    this.IconFile = objModule.IconFile;
                    this.Visibility = objModule.Visibility;
                    this.ContainerSrc = objModule.ContainerSrc;
                    this.DisplayTitle = objModule.DisplayTitle;
                    this.DisplayPrint = objModule.DisplayPrint;
                    this.DisplaySyndicate = objModule.DisplaySyndicate;
                }
            }
        }
    }
}
