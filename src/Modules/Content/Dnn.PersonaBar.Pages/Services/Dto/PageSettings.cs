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

        [DataMember(Name = "thumbnail")]
        public string Thumbnail { get; set; }

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

        [DataMember(Name = "workflowId")]
        public int WorkflowId { get; set; }

        [DataMember(Name = "isWorkflowCompleted")]
        public bool IsWorkflowCompleted { get; set; }

        [DataMember(Name = "applyWorkflowToChildren")]
        public bool ApplyWorkflowToChildren { get; set; }

        [DataMember(Name = "isWorkflowPropagationAvailable")]
        public bool IsWorkflowPropagationAvailable { get; set; }

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
    }
}