// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    [XmlRoot("tab", IsNullable = false)]
    [Serializable]
    public class TabInfo : ContentItem, IPropertyAccess
    {
        private static readonly Regex SkinSrcRegex = new Regex(@"([^/]+$)", RegexOptions.CultureInvariant);
        private static Dictionary<string, string> _docTypeCache = new Dictionary<string, string>();
        private static ReaderWriterLockSlim _docTypeCacheLock = new ReaderWriterLockSlim();
        private readonly SharedDictionary<string, string> _localizedTabNameDictionary;
        private readonly SharedDictionary<string, string> _fullUrlDictionary;

        private string _administratorRoles;
        private string _authorizedRoles;
        private TabInfo _defaultLanguageTab;
        private bool _isSuperTab;
        private Dictionary<string, TabInfo> _localizedTabs;
        private TabPermissionCollection _permissions;
        private Hashtable _settings;
        private string _skinDoctype;
        private bool _superTabIdSet = Null.NullBoolean;
        private string _iconFile;
        private string _iconFileLarge;

        private List<TabAliasSkinInfo> _aliasSkins;
        private Dictionary<string, string> _customAliases;
        private List<TabUrlInfo> _tabUrls;
        private ArrayList _modules;

        public TabInfo()
            : this(new SharedDictionary<string, string>(), new SharedDictionary<string, string>())
        {
        }

        private TabInfo(SharedDictionary<string, string> localizedTabNameDictionary, SharedDictionary<string, string> fullUrlDictionary)
        {
            this._localizedTabNameDictionary = localizedTabNameDictionary;
            this._fullUrlDictionary = fullUrlDictionary;

            this.PortalID = Null.NullInteger;
            this._authorizedRoles = Null.NullString;
            this.ParentId = Null.NullInteger;
            this.IconFile = Null.NullString;
            this.IconFileLarge = Null.NullString;
            this._administratorRoles = Null.NullString;
            this.Title = Null.NullString;
            this.Description = Null.NullString;
            this.KeyWords = Null.NullString;
            this.Url = Null.NullString;
            this.SkinSrc = Null.NullString;
            this._skinDoctype = Null.NullString;
            this.ContainerSrc = Null.NullString;
            this.TabPath = Null.NullString;
            this.StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this.RefreshInterval = Null.NullInteger;
            this.PageHeadText = Null.NullString;
            this.SiteMapPriority = 0.5F;

            // UniqueId, Version Guid, and Localized Version Guid should be initialised to a new value
            this.UniqueId = Guid.NewGuid();
            this.VersionGuid = Guid.NewGuid();
            this.LocalizedVersionGuid = Guid.NewGuid();

            // Default Language Guid should be initialised to a null Guid
            this.DefaultLanguageGuid = Null.NullGuid;

            this.IsVisible = true;
            this.HasBeenPublished = true;
            this.DisableLink = false;

            this.Panes = new ArrayList();

            this.IsSystem = false;
        }

        [XmlIgnore]
        public bool HasAVisibleVersion
        {
            get
            {
                return this.HasBeenPublished || TabVersionUtils.CanSeeVersionedPages(this);
            }
        }

        [XmlIgnore]
        public Dictionary<int, ModuleInfo> ChildModules
        {
            get
            {
                return ModuleController.Instance.GetTabModules(this.TabID);
            }
        }

        [XmlIgnore]
        public TabInfo DefaultLanguageTab
        {
            get
            {
                if (this._defaultLanguageTab == null && (!this.DefaultLanguageGuid.Equals(Null.NullGuid)))
                {
                    this._defaultLanguageTab = (from kvp in TabController.Instance.GetTabsByPortal(this.PortalID) where kvp.Value.UniqueId == this.DefaultLanguageGuid select kvp.Value).SingleOrDefault();
                }

                return this._defaultLanguageTab;
            }
        }

        [XmlIgnore]
        public bool DoNotRedirect
        {
            get
            {
                bool doNotRedirect;
                if (this.TabSettings.ContainsKey("DoNotRedirect") && !string.IsNullOrEmpty(this.TabSettings["DoNotRedirect"].ToString()))
                {
                    doNotRedirect = bool.Parse(this.TabSettings["DoNotRedirect"].ToString());
                }
                else
                {
                    doNotRedirect = false;
                }

                return doNotRedirect;
            }
        }

        [XmlIgnore]
        public string IndentedTabName
        {
            get
            {
                string indentedTabName = Null.NullString;
                for (int intCounter = 1; intCounter <= this.Level; intCounter++)
                {
                    indentedTabName += "...";
                }

                indentedTabName += this.LocalizedTabName;
                return indentedTabName;
            }
        }

        [XmlIgnore]
        public bool IsDefaultLanguage
        {
            get
            {
                return this.DefaultLanguageGuid == Null.NullGuid;
            }
        }

        [XmlIgnore]
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
                if (this.DefaultLanguageTab != null)
                {
                    // Child language
                    isTranslated = this.LocalizedVersionGuid == this.DefaultLanguageTab.LocalizedVersionGuid;
                }

                return isTranslated;
            }
        }

        [XmlIgnore]
        public string LocalizedTabName
        {
            get
            {
                if (string.IsNullOrEmpty(this.TabPath))
                {
                    return this.TabName;
                }

                var key = Thread.CurrentThread.CurrentUICulture.ToString();
                string localizedTabName;
                using (this._localizedTabNameDictionary.GetReadLock())
                {
                    this._localizedTabNameDictionary.TryGetValue(key, out localizedTabName);
                }

                if (string.IsNullOrEmpty(localizedTabName))
                {
                    using (this._localizedTabNameDictionary.GetWriteLock())
                    {
                        localizedTabName = Localization.GetString(this.TabPath + ".String", Localization.GlobalResourceFile, true);
                        if (string.IsNullOrEmpty(localizedTabName))
                        {
                            localizedTabName = this.TabName;
                        }

                        if (!this._localizedTabNameDictionary.ContainsKey(key))
                        {
                            this._localizedTabNameDictionary.Add(key, localizedTabName.Trim());
                        }
                    }
                }

                return localizedTabName;
            }
        }

        [XmlIgnore]
        public Dictionary<string, TabInfo> LocalizedTabs
        {
            get
            {
                if (this._localizedTabs == null)
                {
                    this._localizedTabs =
                        (from kvp in TabController.Instance.GetTabsByPortal(this.PortalID)
                         where kvp.Value.DefaultLanguageGuid == this.UniqueId && LocaleController.Instance.GetLocale(this.PortalID, kvp.Value.CultureCode) != null
                         select kvp.Value).ToDictionary(t => t.CultureCode);
                }

                return this._localizedTabs;
            }
        }

        [XmlArray("tabpermissions")]
        [XmlArrayItem("permission")]
        public TabPermissionCollection TabPermissions
        {
            get
            {
                return this._permissions ?? (this._permissions = new TabPermissionCollection(TabPermissionController.GetTabPermissions(this.TabID, this.PortalID)));
            }
        }

        [XmlIgnore]
        public Hashtable TabSettings
        {
            get
            {
                return this._settings ?? (this._settings = (this.TabID == Null.NullInteger) ? new Hashtable() : TabController.Instance.GetTabSettings(this.TabID));
            }
        }

        [XmlIgnore]
        public TabType TabType
        {
            get
            {
                return Globals.GetURLType(this.Url);
            }
        }

        [XmlIgnore]
        public List<TabAliasSkinInfo> AliasSkins
        {
            get
            {
                return this._aliasSkins ?? (this._aliasSkins = (this.TabID == Null.NullInteger) ? new List<TabAliasSkinInfo>() : TabController.Instance.GetAliasSkins(this.TabID, this.PortalID));
            }
        }

        [XmlIgnore]
        public Dictionary<string, string> CustomAliases
        {
            get
            {
                return this._customAliases ?? (this._customAliases = (this.TabID == Null.NullInteger) ? new Dictionary<string, string>() : TabController.Instance.GetCustomAliases(this.TabID, this.PortalID));
            }
        }

        [XmlIgnore]
        public string FullUrl
        {
            get
            {
                var key = string.Format("{0}_{1}", TestableGlobals.Instance.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias),
                                            Thread.CurrentThread.CurrentCulture);

                string fullUrl;
                using (this._fullUrlDictionary.GetReadLock())
                {
                    this._fullUrlDictionary.TryGetValue(key, out fullUrl);
                }

                if (string.IsNullOrEmpty(fullUrl))
                {
                    using (this._fullUrlDictionary.GetWriteLock())
                    {
                        switch (this.TabType)
                        {
                            case TabType.Normal:
                                // normal tab
                                fullUrl = TestableGlobals.Instance.NavigateURL(this.TabID, this.IsSuperTab);
                                break;
                            case TabType.Tab:
                                // alternate tab url
                                fullUrl = TestableGlobals.Instance.NavigateURL(Convert.ToInt32(this.Url));
                                break;
                            case TabType.File:
                                // file url
                                fullUrl = TestableGlobals.Instance.LinkClick(this.Url, this.TabID, Null.NullInteger);
                                break;
                            case TabType.Url:
                                // external url
                                fullUrl = this.Url;
                                break;
                        }

                        if (!this._fullUrlDictionary.ContainsKey(key))
                        {
                            if (fullUrl != null)
                            {
                                this._fullUrlDictionary.Add(key, fullUrl.Trim());
                            }
                        }
                    }
                }

                return fullUrl;
            }
        }

        [XmlIgnore]
        public bool TabPermissionsSpecified
        {
            get { return false; }
        }

        [XmlIgnore]
        public List<TabUrlInfo> TabUrls
        {
            get
            {
                return this._tabUrls ?? (this._tabUrls = (this.TabID == Null.NullInteger) ? new List<TabUrlInfo>() : TabController.Instance.GetTabUrls(this.TabID, this.PortalID));
            }
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        [XmlIgnore]
        public ArrayList BreadCrumbs { get; set; }

        [XmlIgnore]
        public string ContainerPath { get; set; }

        [XmlElement("containersrc")]
        public string ContainerSrc { get; set; }

        [XmlElement("cultureCode")]
        public string CultureCode { get; set; }

        [XmlElement("defaultLanguageGuid")]
        public Guid DefaultLanguageGuid { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("disabled")]
        public bool DisableLink { get; set; }

        [XmlElement("enddate")]
        public DateTime EndDate { get; set; }

        [XmlElement("haschildren")]
        public bool HasChildren { get; set; }

        [XmlIgnore]
        public string IconFileRaw { get; private set; }

        [XmlIgnore]
        public string IconFileLargeRaw { get; private set; }

        [XmlElement("isdeleted")]
        public bool IsDeleted { get; set; }

        [XmlElement("issecure")]
        public bool IsSecure { get; set; }

        [XmlElement("visible")]
        public bool IsVisible { get; set; }

        [XmlElement("issystem")]
        public bool IsSystem { get; set; }

        [XmlIgnore]
        public bool HasBeenPublished { get; set; }

        [XmlElement("keywords")]
        public string KeyWords { get; set; }

        [XmlIgnore]
        public int Level { get; set; }

        [XmlElement("localizedVersionGuid")]
        public Guid LocalizedVersionGuid { get; set; }

        [XmlIgnore]
        public ArrayList Modules
        {
            get
            {
                return this._modules ?? (this._modules = TabModulesController.Instance.GetTabModules(this));
            }

            set
            {
                this._modules = value;
            }
        }

        [XmlElement("pageheadtext")]
        public string PageHeadText { get; set; }

        [XmlIgnore]
        public ArrayList Panes { get; private set; }

        [XmlElement("parentid")]
        public int ParentId { get; set; }

        [XmlElement("permanentredirect")]
        public bool PermanentRedirect { get; set; }

        [XmlElement("portalid")]
        public int PortalID { get; set; }

        [XmlElement("refreshinterval")]
        public int RefreshInterval { get; set; }

        [XmlElement("sitemappriority")]
        public float SiteMapPriority { get; set; }

        [XmlIgnore]
        public string SkinPath { get; set; }

        [XmlElement("skinsrc")]
        public string SkinSrc { get; set; }

        [XmlElement("startdate")]
        public DateTime StartDate { get; set; }

        [XmlElement("name")]
        public string TabName { get; set; }

        [XmlElement("taborder")]
        public int TabOrder { get; set; }

        [XmlElement("tabpath")]
        public string TabPath { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("uniqueid")]
        public Guid UniqueId { get; set; }

        [XmlElement("versionguid")]
        public Guid VersionGuid { get; set; }

        [XmlElement("iconfile")]
        public string IconFile
        {
            get
            {
                this.IconFileGetter(ref this._iconFile, this.IconFileRaw);
                return this._iconFile;
            }

            set
            {
                this.IconFileRaw = value;
                this._iconFile = null;
            }
        }

        [XmlElement("iconfilelarge")]
        public string IconFileLarge
        {
            get
            {
                this.IconFileGetter(ref this._iconFileLarge, this.IconFileLargeRaw);
                return this._iconFileLarge;
            }

            set
            {
                this.IconFileLargeRaw = value;
                this._iconFileLarge = null;
            }
        }

        [XmlIgnore]
        public bool IsSuperTab
        {
            get
            {
                if (this._superTabIdSet)
                {
                    return this._isSuperTab;
                }

                return this.PortalID == Null.NullInteger;
            }

            set
            {
                this._isSuperTab = value;
                this._superTabIdSet = true;
            }
        }

        [XmlIgnore]
        public override int KeyID
        {
            get
            {
                return this.TabID;
            }

            set
            {
                this.TabID = value;
            }
        }

        [XmlElement("skindoctype")]
        public string SkinDoctype
        {
            get
            {
                if (string.IsNullOrEmpty(this.SkinSrc) == false && string.IsNullOrEmpty(this._skinDoctype))
                {
                    this._skinDoctype = this.CheckIfDoctypeConfigExists();
                    if (string.IsNullOrEmpty(this._skinDoctype))
                    {
                        this._skinDoctype = Host.Host.DefaultDocType;
                    }
                }

                return this._skinDoctype;
            }

            set
            {
                this._skinDoctype = value;
            }
        }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlIgnore]
        public bool UseBaseFriendlyUrls { get; set; }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            string outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }

            string lowerPropertyName = propertyName.ToLowerInvariant();
            if (currentScope == Scope.NoSettings)
            {
                propertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }

            propertyNotFound = true;

            string result = string.Empty;
            bool isPublic = true;
            switch (lowerPropertyName)
            {
                case "tabid":
                    propertyNotFound = false;
                    result = this.TabID.ToString(outputFormat, formatProvider);
                    break;
                case "taborder":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.TabOrder.ToString(outputFormat, formatProvider);
                    break;
                case "portalid":
                    propertyNotFound = false;
                    result = this.PortalID.ToString(outputFormat, formatProvider);
                    break;
                case "tabname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.LocalizedTabName, format);
                    break;
                case "isvisible":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsVisible, formatProvider);
                    break;
                case "parentid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ParentId.ToString(outputFormat, formatProvider);
                    break;
                case "level":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.Level.ToString(outputFormat, formatProvider);
                    break;
                case "iconfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.IconFile, format);
                    break;
                case "iconfilelarge":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.IconFileLarge, format);
                    break;
                case "disablelink":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisableLink, formatProvider);
                    break;
                case "title":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Title, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Description, format);
                    break;
                case "keywords":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.KeyWords, format);
                    break;
                case "isdeleted":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsDeleted, formatProvider);
                    break;
                case "url":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Url, format);
                    break;
                case "skinsrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.SkinSrc, format);
                    break;
                case "containersrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ContainerSrc, format);
                    break;
                case "tabpath":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.TabPath, format);
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
                case "haschildren":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.HasChildren, formatProvider);
                    break;
                case "refreshinterval":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.RefreshInterval.ToString(outputFormat, formatProvider);
                    break;
                case "pageheadtext":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.PageHeadText, format);
                    break;
                case "skinpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.SkinPath, format);
                    break;
                case "skindoctype":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.SkinDoctype, format);
                    break;
                case "containerpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ContainerPath, format);
                    break;
                case "issupertab":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsSuperTab, formatProvider);
                    break;
                case "fullurl":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.FullUrl, format);
                    break;
                case "sitemappriority":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.SiteMapPriority.ToString(), format);
                    break;
            }

            if (!isPublic && currentScope != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }

            return result;
        }

        public TabInfo Clone()
        {
            var clonedTab = new TabInfo(this._localizedTabNameDictionary, this._fullUrlDictionary)
            {
                TabID = this.TabID,
                TabOrder = this.TabOrder,
                PortalID = this.PortalID,
                TabName = this.TabName,
                IsVisible = this.IsVisible,
                HasBeenPublished = this.HasBeenPublished,
                ParentId = this.ParentId,
                Level = this.Level,
                IconFile = this.IconFileRaw,
                IconFileLarge = this.IconFileLargeRaw,
                DisableLink = this.DisableLink,
                Title = this.Title,
                Description = this.Description,
                KeyWords = this.KeyWords,
                IsDeleted = this.IsDeleted,
                Url = this.Url,
                SkinSrc = this.SkinSrc,
                ContainerSrc = this.ContainerSrc,
                TabPath = this.TabPath,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                HasChildren = this.HasChildren,
                SkinPath = this.SkinPath,
                ContainerPath = this.ContainerPath,
                IsSuperTab = this.IsSuperTab,
                RefreshInterval = this.RefreshInterval,
                PageHeadText = this.PageHeadText,
                IsSecure = this.IsSecure,
                PermanentRedirect = this.PermanentRedirect,
                IsSystem = this.IsSystem,
            };

            if (this.BreadCrumbs != null)
            {
                clonedTab.BreadCrumbs = new ArrayList();
                foreach (TabInfo t in this.BreadCrumbs)
                {
                    clonedTab.BreadCrumbs.Add(t.Clone());
                }
            }

            this.Clone(clonedTab, this);

            // localized properties
            clonedTab.UniqueId = this.UniqueId;
            clonedTab.VersionGuid = this.VersionGuid;
            clonedTab.DefaultLanguageGuid = this.DefaultLanguageGuid;
            clonedTab.LocalizedVersionGuid = this.LocalizedVersionGuid;
            clonedTab.CultureCode = this.CultureCode;

            clonedTab.Panes = new ArrayList();
            clonedTab.Modules = this._modules;

            return clonedTab;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a TabInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public override void Fill(IDataReader dr)
        {
            // Call the base classes fill method to populate base class proeprties
            this.FillInternal(dr);
            this.UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            this.VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            this.DefaultLanguageGuid = Null.SetNullGuid(dr["DefaultLanguageGuid"]);
            this.LocalizedVersionGuid = Null.SetNullGuid(dr["LocalizedVersionGuid"]);
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);

            this.TabOrder = Null.SetNullInteger(dr["TabOrder"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.TabName = Null.SetNullString(dr["TabName"]);
            this.IsVisible = Null.SetNullBoolean(dr["IsVisible"]);
            this.HasBeenPublished = Null.SetNullBoolean(dr["HasBeenPublished"]);
            this.ParentId = Null.SetNullInteger(dr["ParentId"]);
            this.Level = Null.SetNullInteger(dr["Level"]);
            this.IconFile = Null.SetNullString(dr["IconFile"]);
            this.IconFileLarge = Null.SetNullString(dr["IconFileLarge"]);
            this.DisableLink = Null.SetNullBoolean(dr["DisableLink"]);
            this.Title = Null.SetNullString(dr["Title"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.KeyWords = Null.SetNullString(dr["KeyWords"]);
            this.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            this.Url = Null.SetNullString(dr["Url"]);
            this.SkinSrc = Null.SetNullString(dr["SkinSrc"]);
            this.ContainerSrc = Null.SetNullString(dr["ContainerSrc"]);
            this.TabPath = Null.SetNullString(dr["TabPath"]);
            this.StartDate = Null.SetNullDateTime(dr["StartDate"]);
            this.EndDate = Null.SetNullDateTime(dr["EndDate"]);
            this.HasChildren = Null.SetNullBoolean(dr["HasChildren"]);
            this.RefreshInterval = Null.SetNullInteger(dr["RefreshInterval"]);
            this.PageHeadText = Null.SetNullString(dr["PageHeadText"]);
            this.IsSecure = Null.SetNullBoolean(dr["IsSecure"]);
            this.PermanentRedirect = Null.SetNullBoolean(dr["PermanentRedirect"]);
            this.SiteMapPriority = Null.SetNullSingle(dr["SiteMapPriority"]);
            this.BreadCrumbs = null;
            this.Modules = null;
            this.IsSystem = Null.SetNullBoolean(dr["IsSystem"]);
        }

        public string GetCurrentUrl(string cultureCode)
        {
            string url = null;
            if (this._tabUrls != null && this._tabUrls.Count > 0)
            {
                TabUrlInfo tabUrl = this._tabUrls.CurrentUrl(cultureCode);
                if (tabUrl != null)
                {
                    url = tabUrl.Url;
                }
            }

            return url ?? string.Empty;
        }

        public string GetTags()
        {
            return string.Join(",", this.Terms.Select(t => t.Name));
        }

        internal void ClearTabUrls()
        {
            this._tabUrls = null;
        }

        internal void ClearSettingsCache()
        {
            this._settings = null;
        }

        /// <summary>
        /// Look for skin level doctype configuration file, and inject the value into the top of default.aspx
        /// when no configuration if found, the doctype for versions prior to 4.4 is used to maintain backwards compatibility with existing skins.
        /// Adds xmlns and lang parameters when appropiate.
        /// </summary>
        /// <remarks></remarks>
        private string CheckIfDoctypeConfigExists()
        {
            if (string.IsNullOrEmpty(this.SkinSrc))
            {
                return string.Empty;
            }

            // loading an XML document from disk for each page request is expensive
            // let's implement some local caching
            if (!_docTypeCache.ContainsKey(this.SkinSrc))
            {
                // appply lock after IF, locking is more expensive than worst case scenario (check disk twice)
                _docTypeCacheLock.EnterWriteLock();
                try
                {
                    var docType = this.LoadDocType();
                    _docTypeCache[this.SkinSrc] = docType == null ? string.Empty : docType.FirstChild.InnerText;
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
                finally
                {
                    _docTypeCacheLock.ExitWriteLock();
                }
            }

            // return if file exists from cache
            _docTypeCacheLock.EnterReadLock();
            try
            {
                return _docTypeCache[this.SkinSrc];
            }
            finally
            {
                _docTypeCacheLock.ExitReadLock();
            }
        }

        private XmlDocument LoadDocType()
        {
            var xmlSkinDocType = new XmlDocument { XmlResolver = null };

            // default to the skinname.doctype.xml to allow the individual skin to override the skin package
            var skinFileName = HttpContext.Current.Server.MapPath(this.SkinSrc.Replace(".ascx", ".doctype.xml"));
            if (File.Exists(skinFileName))
            {
                xmlSkinDocType.Load(skinFileName);
                return xmlSkinDocType;
            }

            // use the skin.doctype.xml file
            var packageFileName = HttpContext.Current.Server.MapPath(SkinSrcRegex.Replace(this.SkinSrc, "skin.doctype.xml"));
            if (File.Exists(packageFileName))
            {
                xmlSkinDocType.Load(packageFileName);
                return xmlSkinDocType;
            }

            // no doctype
            return null;
        }

        private void IconFileGetter(ref string iconFile, string iconRaw)
        {
            if ((!string.IsNullOrEmpty(iconRaw) && iconRaw.StartsWith("~")) || this.PortalID == Null.NullInteger)
            {
                iconFile = iconRaw;
            }
            else if (iconFile == null && !string.IsNullOrEmpty(iconRaw) && this.PortalID != Null.NullInteger)
            {
                IFileInfo fileInfo;
                if (iconRaw.StartsWith("FileID=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var fileId = Convert.ToInt32(iconRaw.Substring(7));
                    fileInfo = FileManager.Instance.GetFile(fileId);
                }
                else
                {
                    fileInfo = FileManager.Instance.GetFile(this.PortalID, iconRaw);
                }

                iconFile = fileInfo != null ? FileManager.Instance.GetUrl(fileInfo) : iconRaw;
            }
        }
    }
}
