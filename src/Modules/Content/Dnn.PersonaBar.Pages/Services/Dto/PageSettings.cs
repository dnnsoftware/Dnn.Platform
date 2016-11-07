using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
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

        [DataMember(Name = "templateId")]
        public int TemplateId { get; set; }

        [DataMember(Name = "pageType")]
        public string PageType { get; set; }

        [DataMember(Name = "trackLinks")]
        public bool TrackLinks { get; set; }

        [DataMember(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

        [DataMember(Name = "permissions")]
        public PagePermissions Permissions { get; set; }

        [DataMember(Name = "modules")]
        public IEnumerable<ModuleItem> Modules { get; set; }

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
    }
}