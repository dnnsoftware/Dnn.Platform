// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Mobile;

    public class ModuleSettingsModel
    {
        [Display(Name = "plModuleId")]
        public int ModuleId { get; set; }

        [Required]
        [Display(Name = "plTitle")]
        public string ModuleTitle { get; set; }

        [Display(Name = "plIcon")]
        public string IconFile { get; set; }

        [Display(Name = "plAllTabs")]
        public bool AllTabs { get; set; }

        [Display(Name = "AllowIndexLabel")]
        public bool AllowIndex { get; set; }

        [Display(Name = "plMoniker")]
        public string Moniker { get; set; }

        [Display(Name = "plVisibility")]
        public VisibilityState Visibility { get; set; }

        [Display(Name = "plAdminBorder")]
        public bool HideAdminBorder { get; set; }

        [Display(Name = "plCacheTime")]
        public int CacheTime { get; set; }

        [Display(Name = "CacheProvider")]
        public string CacheProvider { get; set; }

        [Display(Name = "plAlign")]
        public string Alignment { get; set; }

        [Display(Name = "plColor")]
        public string Color { get; set; }

        [Display(Name = "plBorder")]
        public string Border { get; set; }

        [Display(Name = "plHeader")]
        public string Header { get; set; }

        [Display(Name = "plFooter")]
        public string Footer { get; set; }

        [Display(Name = "plStartDate")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "plEndDate")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "plModuleContainer")]
        public string ContainerSrc { get; set; }

        public List<ModulePermissionInfo> ModulePermissions { get; set; }

        [Display(Name = "plDisplayTitle")]
        public bool DisplayTitle { get; set; }

        [Display(Name = "plDisplayPrint")]
        public bool DisplayPrint { get; set; }

        [Display(Name = "plDisplaySyndicate")]
        public bool DisplaySyndicate { get; set; }

        [Display(Name = "plDefault")]
        public bool IsDefaultModule { get; set; }

        [Display(Name = "plAllModules")]
        public bool AllModules { get; set; }

        [Display(Name = "cultureLabel")]
        public string CultureCode { get; set; }

        [Display(Name = "plFriendlyName")]
        public string FriendlyName { get; set; }

        [Display(Name = "plTags")]
        public string Tags { get; set; }

        [Display(Name = "plNewTabs")]
        public bool NewTabs { get; set; }

        [Display(Name = "isShareableLabel")]
        public bool IsShareable { get; set; }

        [Display(Name = "isShareableViewOnlyLabel")]
        public bool IsShareableViewOnly { get; set; }

        [Display(Name = "plAdminBorder")]
        public bool AdminBorder { get; set; }

        [Display(Name = "plTab")]
        public int TabId { get; set; }

        public IEnumerable<TabModel> AvailableTabs { get; set; }

        [Display(Name = "InheritPermissions")]
        public bool InheritViewPermissions { get; set; }

        public bool CacheWarningVisible { get; set; }

        public bool CacheInheritedVisible { get; set; }

        public bool IsShareableVisible { get; set; }

        public IEnumerable<SelectListItem> ContainerOptions { get; set; }

        public string ModuleContainerCombo { get; set; }

        public string CmdDelete { get; set; }

        public string IsShareableCheckBox { get; set; }

        public IEnumerable<TabModel> InstalledOnTabs { get; set; }

        public string ReturnUrl { get; internal set; }

        public string ModuleControllerName { get; set; }

        public string ModuleActionName { get; set; }

        public string ModuleLocalResourceFile { get; internal set; }

        public string ModuleControlSrc { get; internal set; }
    }
}
