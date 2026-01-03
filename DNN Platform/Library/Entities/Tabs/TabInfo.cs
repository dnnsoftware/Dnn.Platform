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

    using DotNetNuke.Abstractions.Application;
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

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>Information about a page within a DNN site.</summary>
    /// <seealso cref="ContentItem" />
    /// <seealso cref="IPropertyAccess" />
    [XmlRoot("tab", IsNullable = false)]
    [Serializable]
    public class TabInfo : ContentItem, IPropertyAccess
    {
        private static readonly Regex SkinSrcRegex = new Regex(@"([^/]+$)", RegexOptions.CultureInvariant);
        private static Dictionary<string, string> docTypeCache = new Dictionary<string, string>();
        private static ReaderWriterLockSlim docTypeCacheLock = new ReaderWriterLockSlim();
        private readonly SharedDictionary<string, string> localizedTabNameDictionary;
        private readonly SharedDictionary<string, string> fullUrlDictionary;

        private TabInfo defaultLanguageTab;
        private bool isSuperTab;
        private Dictionary<string, TabInfo> localizedTabs;
        private TabPermissionCollection permissions;
        private Hashtable settings;
        private string skinDoctype;
        private bool superTabIdSet = Null.NullBoolean;
        private string iconFile;
        private string iconFileLarge;

        private List<TabAliasSkinInfo> aliasSkins;
        private Dictionary<string, string> customAliases;
        private List<TabUrlInfo> tabUrls;
        private ArrayList modules;

        /// <summary>Initializes a new instance of the <see cref="TabInfo"/> class.</summary>
        public TabInfo()
            : this(new SharedDictionary<string, string>(), new SharedDictionary<string, string>())
        {
        }

        private TabInfo(SharedDictionary<string, string> localizedTabNameDictionary, SharedDictionary<string, string> fullUrlDictionary)
        {
            this.localizedTabNameDictionary = localizedTabNameDictionary;
            this.fullUrlDictionary = fullUrlDictionary;

            this.PortalID = Null.NullInteger;
            this.ParentId = Null.NullInteger;
            this.IconFile = Null.NullString;
            this.IconFileLarge = Null.NullString;
            this.Title = Null.NullString;
            this.Description = Null.NullString;
            this.KeyWords = Null.NullString;
            this.Url = Null.NullString;
            this.SkinSrc = Null.NullString;
            this.skinDoctype = Null.NullString;
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

        /// <summary>Gets a value indicating whether this page has a version that is visible to the current user.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool HasAVisibleVersion
        {
            get
            {
                return this.HasBeenPublished || TabVersionUtils.CanSeeVersionedPages(this);
            }
        }

        /// <summary>Gets a value indicating whether DNN's search index should include this page.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool AllowIndex
        {
            get
            {
                return this.TabSettings["AllowIndex"] == null
                       || "true".Equals(this.TabSettings["AllowIndex"].ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>Gets the modules on this page.</summary>
        /// <value>A <see cref="Dictionary{TKey,TValue}"/> mapping between Module ID and <see cref="ModuleInfo"/>.</value>
        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<int, ModuleInfo> ChildModules
        {
            get
            {
                return ModuleController.Instance.GetTabModules(this.TabID);
            }
        }

        /// <summary>Gets info for the default language version of this page, if it exists.</summary>
        /// <value>A <see cref="TabInfo"/> instance, or <c>null</c>.</value>
        [XmlIgnore]
        [JsonIgnore]
        public TabInfo DefaultLanguageTab
        {
            get
            {
                if (this.defaultLanguageTab == null && (!this.DefaultLanguageGuid.Equals(Null.NullGuid)))
                {
                    this.defaultLanguageTab = (from kvp in TabController.Instance.GetTabsByPortal(this.PortalID) where kvp.Value.UniqueId == this.DefaultLanguageGuid select kvp.Value).SingleOrDefault();
                }

                return this.defaultLanguageTab;
            }
        }

        /// <summary>Gets a value indicating whether this page is configured not to redirect.</summary>
        [XmlIgnore]
        [JsonIgnore]
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

        /// <summary>Gets the indented name of this page (i.e. with <c>"..."</c> at the beginning based on the page's <see cref="Level"/>).</summary>
        [XmlIgnore]
        [JsonIgnore]
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

        /// <summary>Gets a value indicating whether this page is the default language version.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool IsDefaultLanguage
        {
            get
            {
                return this.DefaultLanguageGuid == Null.NullGuid;
            }
        }

        /// <summary>Gets a value indicating whether this page is not for any specific culture/language.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool IsNeutralCulture
        {
            get
            {
                return string.IsNullOrEmpty(this.CultureCode);
            }
        }

        /// <summary>Gets a value indicating whether this page is translated.</summary>
        [XmlIgnore]
        [JsonIgnore]
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

        /// <summary>Gets the name of the page for the current culture.</summary>
        [XmlIgnore]
        [JsonIgnore]
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
                using (this.localizedTabNameDictionary.GetReadLock())
                {
                    this.localizedTabNameDictionary.TryGetValue(key, out localizedTabName);
                }

                if (string.IsNullOrEmpty(localizedTabName))
                {
                    using (this.localizedTabNameDictionary.GetWriteLock())
                    {
                        localizedTabName = Localization.GetString(this.TabPath + ".String", Localization.GlobalResourceFile, true);
                        if (string.IsNullOrEmpty(localizedTabName))
                        {
                            localizedTabName = this.TabName;
                        }

                        if (!this.localizedTabNameDictionary.ContainsKey(key))
                        {
                            this.localizedTabNameDictionary.Add(key, localizedTabName.Trim());
                        }
                    }
                }

                return localizedTabName;
            }
        }

        /// <summary>Gets a collection of pages that are localized children of this page.</summary>
        /// <value>A <see cref="Dictionary{TKey,TValue}"/> mapping from <see cref="CultureCode"/> to <see cref="TabInfo"/>.</value>
        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<string, TabInfo> LocalizedTabs
        {
            get
            {
                if (this.localizedTabs == null)
                {
                    this.localizedTabs =
                        (from kvp in TabController.Instance.GetTabsByPortal(this.PortalID)
                         where kvp.Value.DefaultLanguageGuid == this.UniqueId && LocaleController.Instance.GetLocale(this.PortalID, kvp.Value.CultureCode) != null
                         select kvp.Value).ToDictionary(t => t.CultureCode);
                }

                return this.localizedTabs;
            }
        }

        /// <summary>Gets the permissions collection for the page.</summary>
        [XmlArray("tabpermissions")]
        [XmlArrayItem("permission")]
        public TabPermissionCollection TabPermissions
        {
            get
            {
                return this.permissions ?? (this.permissions = new TabPermissionCollection(TabPermissionController.GetTabPermissions(this.TabID, this.PortalID)));
            }
        }

        /// <summary>Gets the settings collection for the page.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public Hashtable TabSettings
        {
            get
            {
                return this.settings ?? (this.settings = (this.TabID == Null.NullInteger) ? new Hashtable() : TabController.Instance.GetTabSettings(this.TabID));
            }
        }

        /// <summary>Gets the <see cref="Tabs.TabType"/> for the page.</summary>
        /// <seealso cref="Tabs.TabType"/>
        [XmlIgnore]
        [JsonIgnore]
        public TabType TabType
        {
            get
            {
                return Globals.GetURLType(this.Url);
            }
        }

        /// <summary>Gets a collection of <seealso cref="TabAliasSkinInfo"/> for this page.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public List<TabAliasSkinInfo> AliasSkins
        {
            get
            {
                return this.aliasSkins ?? (this.aliasSkins = (this.TabID == Null.NullInteger) ? new List<TabAliasSkinInfo>() : TabController.Instance.GetAliasSkins(this.TabID, this.PortalID));
            }
        }

        /// <summary>Gets a collection of custom aliases for this page.</summary>
        /// <value>A <see cref="Dictionary{TKey,TValue}"/> mapping from <see cref="CultureCode"/> to HTTP Alias.</value>
        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<string, string> CustomAliases
        {
            get
            {
                return this.customAliases ?? (this.customAliases = (this.TabID == Null.NullInteger) ? new Dictionary<string, string>() : TabController.Instance.GetCustomAliases(this.TabID, this.PortalID));
            }
        }

        /// <summary>Gets the full URL for this page.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public string FullUrl
        {
            get
            {
                var key =
                    $"{TestableGlobals.Instance.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias)}_{Thread.CurrentThread.CurrentCulture}";

                string fullUrl;
                using (this.fullUrlDictionary.GetReadLock())
                {
                    this.fullUrlDictionary.TryGetValue(key, out fullUrl);
                }

                if (string.IsNullOrEmpty(fullUrl))
                {
                    using (this.fullUrlDictionary.GetWriteLock())
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

                        if (!this.fullUrlDictionary.ContainsKey(key))
                        {
                            if (fullUrl != null)
                            {
                                this.fullUrlDictionary.Add(key, fullUrl.Trim());
                            }
                        }
                    }
                }

                return fullUrl;
            }
        }

        /// <summary>Gets a value indicating whether the tab permissions are specified.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.7.0. No replacement. Scheduled removal in v11.0.0.")]
        [XmlIgnore]
        [JsonIgnore]
        public bool TabPermissionsSpecified
        {
            get { return false; }
        }

        /// <summary>Gets a collection of page-specific URLs.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public List<TabUrlInfo> TabUrls
        {
            get
            {
                return this.tabUrls ?? (this.tabUrls = (this.TabID == Null.NullInteger) ? new List<TabUrlInfo>() : TabController.Instance.GetTabUrls(this.TabID, this.PortalID));
            }
        }

        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        /// <summary>Gets or sets the collection of bread crumb pages (i.e. the parent, grandparent, etc. of this page).</summary>
        /// <value>An <see cref="ArrayList"/> with <see cref="TabInfo"/> instances.</value>
        [XmlIgnore]
        [JsonIgnore]
        public ArrayList BreadCrumbs { get; set; }

        /// <summary>Gets or sets the path to the directory with <see cref="ContainerSrc"/>.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public string ContainerPath { get; set; }

        /// <summary>Gets or sets the path to the default container for this page, if there is one.</summary>
        [XmlElement("containersrc")]
        public string ContainerSrc { get; set; }

        /// <summary>Gets or sets the culture code for this page, if there is one.</summary>
        [XmlElement("cultureCode")]
        public string CultureCode { get; set; }

        /// <summary>Gets or sets the <see cref="Guid"/> for the default language version of this page.</summary>
        [XmlElement("defaultLanguageGuid")]
        public Guid DefaultLanguageGuid { get; set; }

        /// <summary>Gets or sets the page description.</summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>Gets or sets a value indicating whether the link to this page should be rendered as disabled.</summary>
        [XmlElement("disabled")]
        public bool DisableLink { get; set; }

        /// <summary>Gets or sets this page's end date.</summary>
        [XmlElement("enddate")]
        public DateTime EndDate { get; set; }

        /// <summary>Gets or sets a value indicating whether this instance has children.</summary>
        [XmlElement("haschildren")]
        public bool HasChildren { get; set; }

        /// <summary>Gets the icon file URL, as it is stored in the database.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public string IconFileRaw { get; private set; }

        /// <summary>Gets the large icon file URL, as it is stored in the database.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public string IconFileLargeRaw { get; private set; }

        /// <summary>Gets or sets a value indicating whether this page is deleted.</summary>
        [XmlElement("isdeleted")]
        public bool IsDeleted { get; set; }

        /// <summary>Gets or sets a value indicating whether this page requires HTTPS.</summary>
        [XmlElement("issecure")]
        public bool IsSecure { get; set; }

        /// <summary>Gets or sets a value indicating whether this page should be visible in the menu.</summary>
        [XmlElement("visible")]
        public bool IsVisible { get; set; }

        /// <summary>Gets or sets a value indicating whether this page is required by DNN.</summary>
        [XmlElement("issystem")]
        public bool IsSystem { get; set; }

        /// <summary>Gets or sets a value indicating whether this page has been published.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool HasBeenPublished { get; set; }

        /// <summary>Gets or sets the meta keywords for the page.</summary>
        [XmlElement("keywords")]
        public string KeyWords { get; set; }

        /// <summary>Gets or sets the level of the page, i.e. how may ancestors (parent, grandparent, etc.) it has.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public int Level { get; set; }

        /// <summary>Gets or sets the <see cref="Guid"/> for the localized version.</summary>
        [XmlElement("localizedVersionGuid")]
        public Guid LocalizedVersionGuid { get; set; }

        /// <summary>Gets or sets a collection of the modules on this page.</summary>
        /// <value>An <see cref="ArrayList"/> of <see cref="ModuleInfo"/>.</value>
        [XmlIgnore]
        [JsonIgnore]
        public ArrayList Modules
        {
            get
            {
                return this.modules ?? (this.modules = TabModulesController.Instance.GetTabModules(this));
            }

            set
            {
                this.modules = value;
            }
        }

        /// <summary>Gets or sets the page head text, i.e. content to render in the <c>&lt;head&gt;</c> of the page.</summary>
        [XmlElement("pageheadtext")]
        public string PageHeadText { get; set; }

        /// <summary>Gets a list of the names of the panes available to the page.</summary>
        /// <value>An <see cref="ArrayList"/> of <see cref="string"/> values with the IDs of the panes on the page.</value>
        [XmlIgnore]
        [JsonIgnore]
        public ArrayList Panes { get; private set; }

        /// <summary>Gets or sets the ID of this page's parent page (or <see cref="Null.NullInteger"/> if it has no parent).</summary>
        [XmlElement("parentid")]
        public int ParentId { get; set; }

        /// <summary>Gets or sets a value indicating whether the redirect configured for this page should be a permanent redirect.</summary>
        [XmlElement("permanentredirect")]
        public bool PermanentRedirect { get; set; }

        /// <summary>Gets or sets the ID of the portal/site this page belongs to.</summary>
        [XmlElement("portalid")]
        public int PortalID { get; set; }

        /// <summary>Gets or sets the number of seconds after which the page should refresh.</summary>
        /// <remarks>If this value is greater than zero, it is set as the content of the <c>&lt;meta http-equiv="refresh"&gt;</c> tag, which instructs the browser to refresh the page.</remarks>
        [XmlElement("refreshinterval")]
        public int RefreshInterval { get; set; }

        /// <summary>Gets or sets the priority of this page in the search engine site map, a value between 0.0 and 1.0, indicating the relative priority of the page compared to other pages on the site.</summary>
        [XmlElement("sitemappriority")]
        public float SiteMapPriority { get; set; }

        /// <summary>Gets or sets the path to the directory with <see cref="SkinSrc"/>.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public string SkinPath { get; set; }

        /// <summary>Gets or sets the path to the skin control for this page.</summary>
        [XmlElement("skinsrc")]
        public string SkinSrc { get; set; }

        /// <summary>Gets or sets the page's start date.</summary>
        [XmlElement("startdate")]
        public DateTime StartDate { get; set; }

        /// <summary>Gets or sets the name of the page.</summary>
        [XmlElement("name")]
        public string TabName { get; set; }

        /// <summary>Gets or sets the relative order of this page in relation to its siblings.</summary>
        [XmlElement("taborder")]
        public int TabOrder { get; set; }

        /// <summary>Gets or sets the tab path, an identifier in the format <c>"//PageOne//MyParentPage//ThisPage"</c>.</summary>
        [XmlElement("tabpath")]
        public string TabPath { get; set; }

        /// <summary>Gets or sets the HTML title to be rendered for this page.</summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>Gets or sets the unique ID identifying this page.</summary>
        [XmlElement("uniqueid")]
        public Guid UniqueId { get; set; }

        /// <summary>Gets or sets the <see cref="Guid"/> identifying this version of the page.</summary>
        [XmlElement("versionguid")]
        public Guid VersionGuid { get; set; }

        /// <summary>Gets or sets the path to the icon.</summary>
        [XmlElement("iconfile")]
        public string IconFile
        {
            get
            {
                this.IconFileGetter(ref this.iconFile, this.IconFileRaw);
                return this.iconFile;
            }

            set
            {
                this.IconFileRaw = value;
                this.iconFile = null;
            }
        }

        /// <summary>Gets or sets the path to the large icon.</summary>
        [XmlElement("iconfilelarge")]
        public string IconFileLarge
        {
            get
            {
                this.IconFileGetter(ref this.iconFileLarge, this.IconFileLargeRaw);
                return this.iconFileLarge;
            }

            set
            {
                this.IconFileLargeRaw = value;
                this.iconFileLarge = null;
            }
        }

        /// <summary>Gets or sets a value indicating whether this page is a host level page that doesn't belong to any portal.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool IsSuperTab
        {
            get
            {
                if (this.superTabIdSet)
                {
                    return this.isSuperTab;
                }

                return this.PortalID == Null.NullInteger;
            }

            set
            {
                this.isSuperTab = value;
                this.superTabIdSet = true;
            }
        }

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
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

        /// <summary>Gets or sets the doctype statement to be used when rendering this page.</summary>
        [XmlElement("skindoctype")]
        public string SkinDoctype
        {
            get => this.GetSkinDoctype(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
            set => this.skinDoctype = value;
        }

        /// <summary>Gets or sets the URL for this page if it is a redirect.</summary>
        [XmlElement("url")]
        public string Url { get; set; }

        /// <summary>Gets or sets a value indicating whether this page uses friendly URLs.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool UseBaseFriendlyUrls { get; set; }

        /// <summary>Gets a value indicating the pipeline type.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public string PagePipeline
        {
            get
            {
                string pagePipeline;
                if (this.TabSettings.ContainsKey("PagePipeline") && !string.IsNullOrEmpty(this.TabSettings["PagePipeline"].ToString()))
                {
                    pagePipeline = this.TabSettings["PagePipeline"].ToString();
                }
                else
                {
                    pagePipeline = string.Empty;
                }

                return pagePipeline;
            }
        }

        /// <inheritdoc />
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
                case "pagepipeline":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.PagePipeline, format);
                    break;
            }

            if (!isPublic && currentScope != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }

            return result;
        }

        /// <summary>Gets the doctype statement to use when rendering this page.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>The doctype statement (e.g. <c>&lt;!DOCTYPE html&gt;</c>).</returns>
        public string GetSkinDoctype(IHostSettings hostSettings)
        {
            if (string.IsNullOrEmpty(this.SkinSrc) == false && string.IsNullOrEmpty(this.skinDoctype))
            {
                this.skinDoctype = this.CheckIfDoctypeConfigExists();
                if (string.IsNullOrEmpty(this.skinDoctype))
                {
                    this.skinDoctype = hostSettings.DefaultDocType;
                }
            }

            return this.skinDoctype;
        }

        /// <summary>Clones this instance.</summary>
        /// <returns>A new <see cref="TabInfo"/> instance.</returns>
        public TabInfo Clone()
        {
            var clonedTab = new TabInfo(this.localizedTabNameDictionary, this.fullUrlDictionary)
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
            clonedTab.Modules = this.modules;

            return clonedTab;
        }

        /// <inheritdoc />
        public override void Fill(IDataReader dr)
        {
            // Call the base classes fill method to populate base class properties
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
            this.StartDate = Null.SetNullDateTime(dr["StartDate"], DateTimeKind.Utc);
            this.EndDate = Null.SetNullDateTime(dr["EndDate"], DateTimeKind.Utc);
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

        /// <summary>Gets the URL for the given <paramref name="cultureCode"/>.</summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>The URL or <see cref="string.Empty"/>.</returns>
        public string GetCurrentUrl(string cultureCode)
        {
            string url = null;
            if (this.tabUrls != null && this.tabUrls.Count > 0)
            {
                TabUrlInfo tabUrl = this.tabUrls.CurrentUrl(cultureCode);
                if (tabUrl != null)
                {
                    url = tabUrl.Url;
                }
            }

            return url ?? string.Empty;
        }

        /// <summary>Gets the tag names as a comma-delimited <see cref="string"/>.</summary>
        /// <returns>A comma-delimited <see cref="string"/>.</returns>
        public string GetTags()
        {
            return string.Join(",", this.Terms.Select(t => t.Name));
        }

        /// <summary>Clears the tab URLs collection.</summary>
        internal void ClearTabUrls()
        {
            this.tabUrls = null;
        }

        /// <summary>Clears the settings collection.</summary>
        internal void ClearSettingsCache()
        {
            this.settings = null;
        }

        /// <summary>
        /// Look for skin level doctype configuration file, and inject the value into the top of default.aspx
        /// when no configuration if found, the doctype for versions prior to 4.4 is used to maintain backwards compatibility with existing skins.
        /// Adds xmlns and lang parameters when appropriate.
        /// </summary>
        private string CheckIfDoctypeConfigExists()
        {
            if (string.IsNullOrEmpty(this.SkinSrc))
            {
                return string.Empty;
            }

            // loading an XML document from disk for each page request is expensive
            // let's implement some local caching
            if (!docTypeCache.ContainsKey(this.SkinSrc))
            {
                // appply lock after IF, locking is more expensive than worst case scenario (check disk twice)
                docTypeCacheLock.EnterWriteLock();
                try
                {
                    var docType = this.LoadDocType();
                    docTypeCache[this.SkinSrc] = docType == null ? string.Empty : docType.FirstChild.InnerText;
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
                finally
                {
                    docTypeCacheLock.ExitWriteLock();
                }
            }

            // return if file exists from cache
            docTypeCacheLock.EnterReadLock();
            try
            {
                return docTypeCache[this.SkinSrc];
            }
            finally
            {
                docTypeCacheLock.ExitReadLock();
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
