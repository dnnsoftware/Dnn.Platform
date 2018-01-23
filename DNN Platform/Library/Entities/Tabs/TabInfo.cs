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
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    [XmlRoot("tab", IsNullable = false)]
    [Serializable]
    public class TabInfo : ContentItem, IPropertyAccess
    {
        #region Private Members

        private string _administratorRoles;
        private string _authorizedRoles;
        private TabInfo _defaultLanguageTab;
        private bool _isSuperTab;
        private Dictionary<string, TabInfo> _localizedTabs;
        private TabPermissionCollection _permissions;
        private Hashtable _settings;
        private string _skinDoctype;
        private bool _superTabIdSet = Null.NullBoolean;
        private readonly SharedDictionary<string, string> _localizedTabNameDictionary;
        private readonly SharedDictionary<string, string> _fullUrlDictionary;
        private string _iconFile;
        private string _iconFileLarge;

        private List<TabAliasSkinInfo> _aliasSkins;
        private Dictionary<string, string> _customAliases;
        private List<TabUrlInfo> _tabUrls;
        private ArrayList _modules;


        #endregion

        #region Constructors

        public TabInfo()
            : this(new SharedDictionary<string, string>(), new SharedDictionary<string, string>())
        {

        }


        private TabInfo(SharedDictionary<string, string> localizedTabNameDictionary, SharedDictionary<string, string> fullUrlDictionary)
        {
            _localizedTabNameDictionary = localizedTabNameDictionary;
            _fullUrlDictionary = fullUrlDictionary;

            PortalID = Null.NullInteger;
            _authorizedRoles = Null.NullString;
            ParentId = Null.NullInteger;
            IconFile = Null.NullString;
            IconFileLarge = Null.NullString;
            _administratorRoles = Null.NullString;
            Title = Null.NullString;
            Description = Null.NullString;
            KeyWords = Null.NullString;
            Url = Null.NullString;
            SkinSrc = Null.NullString;
            _skinDoctype = Null.NullString;
            ContainerSrc = Null.NullString;
            TabPath = Null.NullString;
            StartDate = Null.NullDate;
            EndDate = Null.NullDate;
            RefreshInterval = Null.NullInteger;
            PageHeadText = Null.NullString;
            SiteMapPriority = 0.5F;

            //UniqueId, Version Guid, and Localized Version Guid should be initialised to a new value
            UniqueId = Guid.NewGuid();
            VersionGuid = Guid.NewGuid();
            LocalizedVersionGuid = Guid.NewGuid();

            //Default Language Guid should be initialised to a null Guid
            DefaultLanguageGuid = Null.NullGuid;

            IsVisible = true;
            HasBeenPublished = true;
            DisableLink = false;

            Panes = new ArrayList();

            IsSystem = false;
        }

        #endregion

        #region Auto-Properties

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

        [XmlIgnore]
        public bool HasAVisibleVersion {
            get
            {
			    return HasBeenPublished || TabVersionUtils.CanSeeVersionedPages(this);
            }
        }

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
                return _modules ?? (_modules = TabModulesController.Instance.GetTabModules(this));
            }
            set
            {
                _modules = value;
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

        #endregion

        #region Public Properties

        [XmlIgnore]
        public Dictionary<int, ModuleInfo> ChildModules
        {
            get
            {
                return ModuleController.Instance.GetTabModules(TabID);
            }
        }

        [XmlIgnore]
        public TabInfo DefaultLanguageTab
        {
            get
            {
                if (_defaultLanguageTab == null && (!DefaultLanguageGuid.Equals(Null.NullGuid)))
                {
                    _defaultLanguageTab = (from kvp in TabController.Instance.GetTabsByPortal(PortalID) where kvp.Value.UniqueId == DefaultLanguageGuid select kvp.Value).SingleOrDefault();
                }
                return _defaultLanguageTab;
            }
        }

        [XmlIgnore]
        public bool DoNotRedirect
        {
            get
            {
                bool doNotRedirect;
                if (TabSettings.ContainsKey("DoNotRedirect") && !string.IsNullOrEmpty(TabSettings["DoNotRedirect"].ToString()))
                {
                    doNotRedirect = bool.Parse(TabSettings["DoNotRedirect"].ToString());
                }
                else
                {
                    doNotRedirect = false;
                }
                return doNotRedirect;
            }
        }

        [XmlElement("iconfile")]
        public string IconFile
        {
            get
            {
                IconFileGetter(ref _iconFile, IconFileRaw);
                return _iconFile;
            }

            set
            {
                IconFileRaw = value;
                _iconFile = null;
            }
        }

        [XmlElement("iconfilelarge")]
        public string IconFileLarge
        {
            get
            {
                IconFileGetter(ref _iconFileLarge, IconFileLargeRaw);
                return _iconFileLarge;
            }

            set
            {
                IconFileLargeRaw = value;
                _iconFileLarge = null;
            }
        }

        [XmlIgnore]
        public string IndentedTabName
        {
            get
            {
                string indentedTabName = Null.NullString;
                for (int intCounter = 1; intCounter <= Level; intCounter++)
                {
                    indentedTabName += "...";
                }
                indentedTabName += LocalizedTabName;
                return indentedTabName;
            }
        }

        [XmlIgnore]
        public bool IsDefaultLanguage
        {
            get
            {
                return (DefaultLanguageGuid == Null.NullGuid);
            }
        }

        [XmlIgnore]
        public bool IsNeutralCulture
        {
            get
            {
                return string.IsNullOrEmpty(CultureCode);
            }
        }

        [XmlIgnore]
        public bool IsSuperTab
        {
            get
            {
                if (_superTabIdSet)
                {
                    return _isSuperTab;
                }
                return (PortalID == Null.NullInteger);
            }
            set
            {
                _isSuperTab = value;
                _superTabIdSet = true;
            }
        }

        [XmlIgnore]
        public bool IsTranslated
        {
            get
            {
                bool isTranslated = true;
                if (DefaultLanguageTab != null)
                {
                    //Child language
                    isTranslated = (LocalizedVersionGuid == DefaultLanguageTab.LocalizedVersionGuid);
                }
                return isTranslated;
            }
        }

        [XmlIgnore]
        public override int KeyID
        {
            get
            {
                return TabID;
            }
            set
            {
                TabID = value;
            }
        }

        [XmlIgnore]
        public string LocalizedTabName
        {
            get
            {
                if (String.IsNullOrEmpty(TabPath)) return TabName;

                var key = Thread.CurrentThread.CurrentUICulture.ToString();
                string localizedTabName;
                using (_localizedTabNameDictionary.GetReadLock())
                {
                    _localizedTabNameDictionary.TryGetValue(key, out localizedTabName);
                }

                if (String.IsNullOrEmpty(localizedTabName))
                {
                    using (_localizedTabNameDictionary.GetWriteLock())
                    {
                        localizedTabName = Localization.GetString(TabPath + ".String", Localization.GlobalResourceFile, true);
                        if (string.IsNullOrEmpty(localizedTabName))
                        {
                            localizedTabName = TabName;
                        }

                        if (!_localizedTabNameDictionary.ContainsKey(key))
                        {
                            _localizedTabNameDictionary.Add(key, localizedTabName.Trim());
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
                if (_localizedTabs == null)
                {
                    _localizedTabs =
                        (from kvp in TabController.Instance.GetTabsByPortal(PortalID)
                         where kvp.Value.DefaultLanguageGuid == UniqueId && LocaleController.Instance.GetLocale(PortalID, kvp.Value.CultureCode) != null
                         select kvp.Value).ToDictionary(t => t.CultureCode);
                }
                return _localizedTabs;
            }
        }

        [XmlElement("skindoctype")]
        public string SkinDoctype
        {
            get
            {
                if (string.IsNullOrEmpty(SkinSrc) == false && string.IsNullOrEmpty(_skinDoctype))
                {
                    _skinDoctype = CheckIfDoctypeConfigExists();
                    if (string.IsNullOrEmpty(_skinDoctype))
                    {
                        _skinDoctype = Host.Host.DefaultDocType;
                    }
                }
                return _skinDoctype;
            }
            set
            {
                _skinDoctype = value;
            }
        }

        [XmlArray("tabpermissions"), XmlArrayItem("permission")]
        public TabPermissionCollection TabPermissions
        {
            get
            {
                return _permissions ?? (_permissions = new TabPermissionCollection(TabPermissionController.GetTabPermissions(TabID, PortalID)));
            }
        }

        [XmlIgnore]
        public Hashtable TabSettings
        {
            get
            {
                return _settings ?? (_settings = (TabID == Null.NullInteger) ? new Hashtable() : TabController.Instance.GetTabSettings(TabID));
            }
        }

        [XmlIgnore]
        public TabType TabType
        {
            get
            {
                return Globals.GetURLType(Url);
            }
        }

        #endregion

        #region Url Properties

        [XmlIgnore]
        public List<TabAliasSkinInfo> AliasSkins
        {
            get
            {
                return _aliasSkins ?? (_aliasSkins = (TabID == Null.NullInteger) ? new List<TabAliasSkinInfo>() : TabController.Instance.GetAliasSkins(TabID, PortalID));
            }
        }

        [XmlIgnore]
        public Dictionary<string, string> CustomAliases
        {
            get
            {
                return _customAliases ?? (_customAliases = (TabID == Null.NullInteger) ? new Dictionary<string, string>() : TabController.Instance.GetCustomAliases(TabID, PortalID));
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
                using (_fullUrlDictionary.GetReadLock())
                {
                    _fullUrlDictionary.TryGetValue(key, out fullUrl);
                }

                if (String.IsNullOrEmpty(fullUrl))
                {
                    using (_fullUrlDictionary.GetWriteLock())
                    {
                        switch (TabType)
                        {
                            case TabType.Normal:
                                //normal tab
                                fullUrl = TestableGlobals.Instance.NavigateURL(TabID, IsSuperTab);
                                break;
                            case TabType.Tab:
                                //alternate tab url
                                fullUrl = TestableGlobals.Instance.NavigateURL(Convert.ToInt32(Url));
                                break;
                            case TabType.File:
                                //file url
                                fullUrl = TestableGlobals.Instance.LinkClick(Url, TabID, Null.NullInteger);
                                break;
                            case TabType.Url:
                                //external url
                                fullUrl = Url;
                                break;
                        }

                        if (!_fullUrlDictionary.ContainsKey(key))
                        {
                            if (fullUrl != null)
                            {
                                _fullUrlDictionary.Add(key, fullUrl.Trim());
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
                return _tabUrls ?? (_tabUrls = (TabID == Null.NullInteger) ? new List<TabUrlInfo>() : TabController.Instance.GetTabUrls(TabID, PortalID));
            }
        }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlIgnore]
        public bool UseBaseFriendlyUrls { get; set; }

        #endregion

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            string outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }

            string lowerPropertyName = propertyName.ToLower();
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
                    result = (TabID.ToString(outputFormat, formatProvider));
                    break;
                case "taborder":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (TabOrder.ToString(outputFormat, formatProvider));
                    break;
                case "portalid":
                    propertyNotFound = false;
                    result = (PortalID.ToString(outputFormat, formatProvider));
                    break;
                case "tabname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(LocalizedTabName, format);
                    break;
                case "isvisible":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsVisible, formatProvider));
                    break;
                case "parentid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (ParentId.ToString(outputFormat, formatProvider));
                    break;
                case "level":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (Level.ToString(outputFormat, formatProvider));
                    break;
                case "iconfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(IconFile, format);
                    break;
                case "iconfilelarge":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(IconFileLarge, format);
                    break;
                case "disablelink":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(DisableLink, formatProvider));
                    break;
                case "title":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Title, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Description, format);
                    break;
                case "keywords":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(KeyWords, format);
                    break;
                case "isdeleted":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsDeleted, formatProvider));
                    break;
                case "url":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(Url, format);
                    break;
                case "skinsrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(SkinSrc, format);
                    break;
                case "containersrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ContainerSrc, format);
                    break;
                case "tabpath":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(TabPath, format);
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
                case "haschildren":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(HasChildren, formatProvider));
                    break;
                case "refreshinterval":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (RefreshInterval.ToString(outputFormat, formatProvider));
                    break;
                case "pageheadtext":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(PageHeadText, format);
                    break;
                case "skinpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(SkinPath, format);
                    break;
                case "skindoctype":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(SkinDoctype, format);
                    break;
                case "containerpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(ContainerPath, format);
                    break;
                case "issupertab":
                    isPublic = false;
                    propertyNotFound = false;
                    result = (PropertyAccess.Boolean2LocalizedYesNo(IsSuperTab, formatProvider));
                    break;
                case "fullurl":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(FullUrl, format);
                    break;
                case "sitemappriority":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(SiteMapPriority.ToString(), format);
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

        #region Private Methods

        private static Dictionary<string, string> _docTypeCache = new Dictionary<string, string>();
        private static ReaderWriterLockSlim _docTypeCacheLock = new ReaderWriterLockSlim();
        private static readonly Regex SkinSrcRegex = new Regex(@"([^/]+$)", RegexOptions.CultureInvariant);

        /// <summary>
        /// Look for skin level doctype configuration file, and inject the value into the top of default.aspx
        /// when no configuration if found, the doctype for versions prior to 4.4 is used to maintain backwards compatibility with existing skins.
        /// Adds xmlns and lang parameters when appropiate.
        /// </summary>
        /// <remarks></remarks>
        private string CheckIfDoctypeConfigExists()
        {
            if (string.IsNullOrEmpty(SkinSrc))
                return string.Empty;

            // loading an XML document from disk for each page request is expensive
            // let's implement some local caching
            if (!_docTypeCache.ContainsKey(SkinSrc)) {
                // appply lock after IF, locking is more expensive than worst case scenario (check disk twice)
                _docTypeCacheLock.EnterWriteLock();
                try {

                    var docType = LoadDocType();
                    _docTypeCache[SkinSrc] = docType == null ? string.Empty : docType.FirstChild.InnerText;

                } catch (Exception ex) {
                    Exceptions.LogException(ex);
                } finally {
                    _docTypeCacheLock.ExitWriteLock();
                }
            }

            // return if file exists from cache
            _docTypeCacheLock.EnterReadLock();
            try {
                return _docTypeCache[SkinSrc];
            } finally {
                _docTypeCacheLock.ExitReadLock();
            }
        }

        XmlDocument LoadDocType()
        {
            var xmlSkinDocType = new XmlDocument { XmlResolver = null };

            // default to the skinname.doctype.xml to allow the individual skin to override the skin package
            var skinFileName = HttpContext.Current.Server.MapPath(SkinSrc.Replace(".ascx", ".doctype.xml"));
            if (File.Exists(skinFileName)) {
                xmlSkinDocType.Load(skinFileName);
                return xmlSkinDocType;
            }

            // use the skin.doctype.xml file
            var packageFileName = HttpContext.Current.Server.MapPath(SkinSrcRegex.Replace(SkinSrc, "skin.doctype.xml"));
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
            if ((!String.IsNullOrEmpty(iconRaw) && iconRaw.StartsWith("~")) || PortalID == Null.NullInteger)
            {
                iconFile = iconRaw;
            }
            else if (iconFile == null && !String.IsNullOrEmpty(iconRaw) && PortalID != Null.NullInteger)
            {
                IFileInfo fileInfo;
                if (iconRaw.StartsWith("FileID=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var fileId = Convert.ToInt32(iconRaw.Substring(7));
                    fileInfo = FileManager.Instance.GetFile(fileId);
                }
                else
                {
                    fileInfo = FileManager.Instance.GetFile(PortalID, iconRaw);
                }

                iconFile = fileInfo != null ? FileManager.Instance.GetUrl(fileInfo) : iconRaw;
            }
        }

        #endregion

        #region Internal Methods

        internal void ClearTabUrls()
        {
            _tabUrls = null;
        }

        internal void ClearSettingsCache()
        {
            _settings = null;
        }

        #endregion

        #region Public Methods

        public TabInfo Clone()
        {
            var clonedTab = new TabInfo(_localizedTabNameDictionary, _fullUrlDictionary)
            {
                TabID = TabID,
                TabOrder = TabOrder,
                PortalID = PortalID,
                TabName = TabName,
                IsVisible = IsVisible,
                HasBeenPublished = HasBeenPublished,
                ParentId = ParentId,
                Level = Level,
                IconFile = IconFileRaw,
                IconFileLarge = IconFileLargeRaw,
                DisableLink = DisableLink,
                Title = Title,
                Description = Description,
                KeyWords = KeyWords,
                IsDeleted = IsDeleted,
                Url = Url,
                SkinSrc = SkinSrc,
                ContainerSrc = ContainerSrc,
                TabPath = TabPath,
                StartDate = StartDate,
                EndDate = EndDate,
                HasChildren = HasChildren,
                SkinPath = SkinPath,
                ContainerPath = ContainerPath,
                IsSuperTab = IsSuperTab,
                RefreshInterval = RefreshInterval,
                PageHeadText = PageHeadText,
                IsSecure = IsSecure,
                PermanentRedirect = PermanentRedirect,
                IsSystem = IsSystem
            };

            if (BreadCrumbs != null)
            {
                clonedTab.BreadCrumbs = new ArrayList();
                foreach (TabInfo t in BreadCrumbs)
                {
                    clonedTab.BreadCrumbs.Add(t.Clone());
                }
            }

            Clone(clonedTab, this);

            //localized properties
            clonedTab.UniqueId = UniqueId;
            clonedTab.VersionGuid = VersionGuid;
            clonedTab.DefaultLanguageGuid = DefaultLanguageGuid;
            clonedTab.LocalizedVersionGuid = LocalizedVersionGuid;
            clonedTab.CultureCode = CultureCode;

            clonedTab.Panes = new ArrayList();
            clonedTab.Modules = _modules;

            return clonedTab;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a TabInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public override void Fill(IDataReader dr)
        {
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);
            UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            DefaultLanguageGuid = Null.SetNullGuid(dr["DefaultLanguageGuid"]);
            LocalizedVersionGuid = Null.SetNullGuid(dr["LocalizedVersionGuid"]);
            CultureCode = Null.SetNullString(dr["CultureCode"]);

            TabOrder = Null.SetNullInteger(dr["TabOrder"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            TabName = Null.SetNullString(dr["TabName"]);
            IsVisible = Null.SetNullBoolean(dr["IsVisible"]);
            HasBeenPublished = Null.SetNullBoolean(dr["HasBeenPublished"]);
            ParentId = Null.SetNullInteger(dr["ParentId"]);
            Level = Null.SetNullInteger(dr["Level"]);
            IconFile = Null.SetNullString(dr["IconFile"]);
            IconFileLarge = Null.SetNullString(dr["IconFileLarge"]);
            DisableLink = Null.SetNullBoolean(dr["DisableLink"]);
            Title = Null.SetNullString(dr["Title"]);
            Description = Null.SetNullString(dr["Description"]);
            KeyWords = Null.SetNullString(dr["KeyWords"]);
            IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
            Url = Null.SetNullString(dr["Url"]);
            SkinSrc = Null.SetNullString(dr["SkinSrc"]);
            ContainerSrc = Null.SetNullString(dr["ContainerSrc"]);
            TabPath = Null.SetNullString(dr["TabPath"]);
            StartDate = Null.SetNullDateTime(dr["StartDate"]);
            EndDate = Null.SetNullDateTime(dr["EndDate"]);
            HasChildren = Null.SetNullBoolean(dr["HasChildren"]);
            RefreshInterval = Null.SetNullInteger(dr["RefreshInterval"]);
            PageHeadText = Null.SetNullString(dr["PageHeadText"]);
            IsSecure = Null.SetNullBoolean(dr["IsSecure"]);
            PermanentRedirect = Null.SetNullBoolean(dr["PermanentRedirect"]);
            SiteMapPriority = Null.SetNullSingle(dr["SiteMapPriority"]);
            BreadCrumbs = null;
            Modules = null;
            IsSystem = Null.SetNullBoolean(dr["IsSystem"]);
        }

        public string GetCurrentUrl(string cultureCode)
        {
            string url = null;
            if (_tabUrls != null && _tabUrls.Count > 0)
            {
                TabUrlInfo tabUrl = _tabUrls.CurrentUrl(cultureCode);
                if (tabUrl != null)
                {
                    url = tabUrl.Url;
                }
            }
            return url ?? ("");
        }

        #endregion
    }
}
