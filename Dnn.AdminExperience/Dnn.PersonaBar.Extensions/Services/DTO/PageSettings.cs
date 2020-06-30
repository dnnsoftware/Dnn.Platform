// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Themes.Components;
    using Newtonsoft.Json.Linq;

    [DataContract]
    public class PageSettings
    {
        [DataMember(Name = "tabId")]
        public int TabId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "absoluteUrl")]
        public string AbsoluteUrl { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "localizedName")]
        public string LocalizedName { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "keywords")]
        public string Keywords { get; set; }

        [DataMember(Name = "tags")]
        public string Tags { get; set; }

        [DataMember(Name = "alias")]
        public string Alias { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "includeInMenu")]
        public bool IncludeInMenu { get; set; }

        [DataMember(Name = "disableLink")]
        public bool DisableLink { get; set; }

        [DataMember(Name = "created")]
        public string Created { get; set; }

        [DataMember(Name = "createdOnDate")]
        public DateTime CreatedOnDate { get; set; }

        [DataMember(Name = "hierarchy")]
        public string Hierarchy { get; set; }

        [DataMember(Name = "hasChild")]
        public bool HasChild { get; set; }

        [DataMember(Name = "customUrlEnabled")]
        public bool CustomUrlEnabled { get; set; }

        [DataMember(Name = "pageType")]
        public string PageType { get; set; }

        [DataMember(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        [DataMember(Name = "permissions")]
        public PagePermissions Permissions { get; set; }

        [DataMember(Name = "pagePermissions")]
        public JObject PagePermissions { get; set; }

        [DataMember(Name = "modules")]
        public IEnumerable<ModuleItem> Modules { get; set; }

        [DataMember(Name = "pageUrls")]
        public IEnumerable<Url> PageUrls { get; set; }

        [DataMember(Name = "isSecure")]
        public bool IsSecure { get; set; }

        [DataMember(Name = "allowIndex")]
        public bool AllowIndex { get; set; }

        [DataMember(Name = "cacheProvider")]
        public string CacheProvider { get; set; }

        [DataMember(Name = "cacheDuration")]
        public int? CacheDuration { get; set; }

        [DataMember(Name = "cacheIncludeExclude")]
        public bool? CacheIncludeExclude { get; set; }

        [DataMember(Name = "cacheIncludeVaryBy")]
        public string CacheIncludeVaryBy { get; set; }

        [DataMember(Name = "cacheExcludeVaryBy")]
        public string CacheExcludeVaryBy { get; set; }

        [DataMember(Name = "cacheMaxVaryByCount")]
        public int? CacheMaxVaryByCount { get; set; }

        [DataMember(Name = "pageHeadText")]
        public string PageHeadText { get; set; }

        [DataMember(Name = "sitemapPriority")]
        public float SiteMapPriority { get; set; }

        [DataMember(Name = "permanentRedirect")]
        public bool PermanentRedirect { get; set; }

        [DataMember(Name = "linkNewWindow")]
        public bool LinkNewWindow { get; set; }

        [DataMember(Name = "pageStyleSheet")]
        public string PageStyleSheet { get; set; }

        [DataMember(Name = "themeName")]
        public string ThemeName { get; set; }

        [DataMember(Name = "themeLevel")]
        public int ThemeLevel { get; set; }

        [DataMember(Name = "skinSrc")]
        public string SkinSrc { get; set; }

        [DataMember(Name = "containerSrc")]
        public string ContainerSrc { get; set; }

        [DataMember(Name = "externalRedirection")]
        public string ExternalRedirection { get; set; }

        [DataMember(Name = "fileIdRedirection")]
        public int? FileIdRedirection { get; set; }

        [DataMember(Name = "fileNameRedirection")]
        public string FileNameRedirection { get; set; }

        [DataMember(Name = "fileFolderPathRedirection")]
        public string FileFolderPathRedirection { get; set; }

        [DataMember(Name = "existingTabRedirection")]
        public string ExistingTabRedirection { get; set; }

        [DataMember(Name = "siteAliases")]
        public IEnumerable<KeyValuePair<int, string>> SiteAliases { get; set; }

        [DataMember(Name = "primaryAliasId")]
        public int? PrimaryAliasId { get; set; }

        [DataMember(Name = "locales")]
        public IOrderedEnumerable<KeyValuePair<int, string>> Locales { get; set; }

        [DataMember(Name = "hasParent")]
        public bool HasParent { get; set; }

        [DataMember(Name = "templateTabId")]
        public int TemplateTabId { get; set; }

        [DataMember(Name = "templates")]
        public IEnumerable<Template> Templates { get; set; }

        [DataMember(Name = "templateId")]
        public int TemplateId { get; set; }

        [DataMember(Name = "parentId")]
        public int? ParentId { get; set; }

        [DataMember(Name = "isspecial")]
        public bool IsSpecial { get; set; }

        [DataMember(Name = "iconFile")]
        public FileDto IconFile { get; set; }

        [DataMember(Name = "iconFileLarge")]
        public FileDto IconFileLarge { get; set; }
    }
}
