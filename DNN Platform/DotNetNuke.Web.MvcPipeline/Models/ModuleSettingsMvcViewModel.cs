// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.UI.WebControls;

    public class ModuleSettingsMvcViewModel
    {
        public ModuleSettingsMvcViewModel()
        {
            this.AvailableTabs = new List<TabInfo>();
            this.Permissions = new List<ModulePermissionInfo>();
            this.Terms = new List<Term>();
            this.AvailableCacheProviders = new List<SelectListItem>();
        }

        public int ModuleId { get; set; }

        public string ModuleTitle { get; set; }

        public string Alignment { get; set; }

        public bool AllTabs { get; set; }

        public bool NewTabs { get; set; }

        public bool AllowIndex { get; set; }

        public bool IsShareable { get; set; }

        public bool IsShareableViewOnly { get; set; }

        public bool AdminBorder { get; set; }

        public string Header { get; set; }

        public string Footer { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Moniker { get; set; }

        public string CacheProvider { get; set; }

        public int CacheDuration { get; set; }

        public string Color { get; set; }

        public string Border { get; set; }

        public string IconFile { get; set; }

        public VisibilityState Visibility { get; set; }

        public bool DisplayTitle { get; set; }

        public bool DisplayPrint { get; set; }

        public bool DisplaySyndicate { get; set; }

        public string ContainerSrc { get; set; }

        public bool IsDefaultModule { get; set; }

        public bool AllModules { get; set; }

        public int TabId { get; set; }

        public List<TabInfo> AvailableTabs { get; set; }

        public List<ModulePermissionInfo> Permissions { get; set; }

        public List<Term> Terms { get; set; }

        public bool InheritViewPermissions { get; set; }

        public bool AllTabsChanged { get; set; }

        public List<SelectListItem> AvailableCacheProviders { get; set; }

        public void LoadSettings(ModuleInfo module)
        {
            this.ModuleId = module.ModuleID;
            this.ModuleTitle = module.ModuleTitle;
            this.Alignment = module.Alignment;
            this.AllTabs = module.AllTabs;
            this.Color = module.Color;
            this.Border = module.Border;
            this.IconFile = module.IconFile;
            this.CacheDuration = module.CacheTime;
            this.CacheProvider = module.CacheMethod;
            this.TabId = module.TabID;
            this.Visibility = module.Visibility;
            this.Header = module.Header;
            this.Footer = module.Footer;
            this.StartDate = module.StartDate;
            this.EndDate = module.EndDate;
            this.ContainerSrc = module.ContainerSrc;
            this.DisplayTitle = module.DisplayTitle;
            this.DisplayPrint = module.DisplayPrint;
            this.DisplaySyndicate = module.DisplaySyndicate;
            this.IsDefaultModule = module.IsDefaultModule;
            this.AllModules = module.AllModules;

            this.AllowIndex = bool.Parse(module.ModuleSettings["AllowIndex"] == null ? "true" : module.ModuleSettings["AllowIndex"].ToString());
            this.AdminBorder = bool.Parse(module.ModuleSettings["hideadminborder"] == null ? "false" : module.ModuleSettings["hideadminborder"].ToString());
            this.Moniker = module.ModuleSettings["Moniker"] as string ?? string.Empty;

            if (!module.IsShared)
            {
                this.InheritViewPermissions = module.InheritViewPermissions;
                this.IsShareable = module.IsShareable;
                this.IsShareableViewOnly = module.IsShareableViewOnly;
            }

            this.Permissions = new List<ModulePermissionInfo>(module.ModulePermissions.Cast<ModulePermissionInfo>());
            this.Terms = new List<Term>(module.Terms);

            // Laad beschikbare cache providers
            var cacheProviders = ModuleCachingProvider.GetProviderList();
            this.AvailableCacheProviders = cacheProviders.Select(p => new SelectListItem
            {
                Text = p.Key.Replace("ModuleCachingProvider", string.Empty),
                Value = p.Key,
            }).ToList();
            this.AvailableCacheProviders.Insert(0, new SelectListItem { Text = "None Specified", Value = string.Empty });
        }
    }
}
