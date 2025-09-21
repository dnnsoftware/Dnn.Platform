// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.MvcPipeline.Models;

    public class ModuleSettingsController : ModuleControllerBase
    {
        private readonly ModuleController moduleController;

        private int moduleId = -1;
        private ModuleInfo module;

        public ModuleSettingsController(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;

            // Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.moduleController = new ModuleController();
        }

        public int TabId
        {
            get
            {
                return this.PortalSettings.ActiveTab.TabID;
            }
        }

        public int TabModuleId { get; private set; }

        protected INavigationManager NavigationManager { get; private set; }

        private PortalInfo Portal
        {
            get { return this.PortalSettings.PortalId == Null.NullInteger ? null : PortalController.Instance.GetPortal(this.PortalSettings.PortalId); }
        }

        private ModuleInfo Module
        {
            get { return this.module ?? (this.module = ModuleController.Instance.GetModule(this.moduleId, this.TabId, false)); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDefaultSettings(ModuleSettingsModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View("Index", model);
            }

            var module = this.moduleController.GetModule(model.ModuleId, model.TabId, false);
            if (module == null)
            {
                return this.HttpNotFound();
            }

            this.module = module;

            this.CheckPermissions();

            module.ModuleTitle = model.ModuleTitle;
            module.IconFile = model.IconFile;
            module.AllTabs = model.AllTabs;
            module.Visibility = model.Visibility;
            module.CacheTime = model.CacheTime;
            module.CacheMethod = model.CacheProvider;
            module.Alignment = model.Alignment;
            module.Color = model.Color;
            module.Border = model.Border;
            module.Header = model.Header;
            module.Footer = model.Footer;
            module.StartDate = model.StartDate ?? Null.NullDate;
            module.EndDate = model.EndDate ?? Null.NullDate;
            module.ContainerSrc = model.ContainerSrc;

            // module.ModulePermissions.Clear();
            // module.ModulePermissions.AddRange(model.ModulePermissions);
            module.DisplayTitle = model.DisplayTitle;
            module.DisplayPrint = model.DisplayPrint;
            module.DisplaySyndicate = model.DisplaySyndicate;
            /*
            module.IsWebSlice = model.IsWebSlice;
            module.WebSliceTitle = model.WebSliceTitle;
            module.WebSliceExpiryDate = model.WebSliceExpiryDate ?? Null.NullDate;
            module.WebSliceTTL = model.WebSliceTTL ?? Null.NullInteger;
            */

            module.IsDefaultModule = model.IsDefaultModule;
            module.AllModules = model.AllModules;

            this.moduleController.UpdateModule(module);

            this.moduleController.UpdateTabModuleSetting(module.TabModuleID, "AllowIndex", model.AllowIndex.ToString());
            this.moduleController.UpdateTabModuleSetting(module.TabModuleID, "Moniker", model.Moniker);
            this.moduleController.UpdateTabModuleSetting(module.TabModuleID, "hideadminborder", model.HideAdminBorder.ToString());

            return new EmptyResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(ModuleSettingsModel model)
        {
            var module = this.moduleController.GetModule(model.ModuleId, model.TabId, false);
            if (module == null)
            {
                return this.HttpNotFound();
            }

            this.module = module;

            this.CheckPermissions();

            ModuleController.Instance.DeleteTabModule(model.TabId, model.ModuleId, true);

            return new EmptyResult();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        private void Initialize()
        {
            try
            {
                // get ModuleId
                if (this.Request.QueryString["ModuleId"] != null)
                {
                    this.moduleId = int.Parse(this.Request.QueryString["ModuleId"]);
                }

                if (this.Module.ContentItemId == Null.NullInteger && this.Module.ModuleID != Null.NullInteger)
                {
                    // This tab does not have a valid ContentItem
                    ModuleController.Instance.CreateContentItem(this.Module);
                    ModuleController.Instance.UpdateModule(this.Module);
                }

                this.CheckPermissions();

                if (this.Module != null)
                {
                    // get module
                    this.TabModuleId = this.Module.TabModuleID;

                    // get Settings Control
                    ModuleControlInfo moduleControlInfo = ModuleControlController.GetModuleControlByControlKey("Settings", this.Module.ModuleDefID);

                    if (moduleControlInfo != null)
                    {
                        /*
                        this.control = ModuleControlFactory.LoadSettingsControl(this.Page, this.Module, moduleControlInfo.ControlSrc);

                        var settingsControl = this.control as ISettingsControl;
                        if (settingsControl != null)
                        {
                            this.hlSpecificSettings.Text = Localization.GetString(
                                "ControlTitle_settings",
                                settingsControl.LocalResourceFile);
                            if (string.IsNullOrEmpty(this.hlSpecificSettings.Text))
                            {
                                this.hlSpecificSettings.Text =
                                    string.Format(
                                        Localization.GetString("ControlTitle_settings", this.LocalResourceFile),
                                        this.Module.DesktopModule.FriendlyName);
                            }

                            this.pnlSpecific.Controls.Add(this.control);
                        }
                        */
                    }
                }
            }
            catch (Exception exc)
            {
                // Log the exception
                DotNetNuke.Services.Exceptions.Exceptions.LogException(exc);
                throw;
            }
        }

        private void CheckPermissions()
        {
            // Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", this.Module))
            {
                if (!(this.IsSharedViewOnly() && TabPermissionController.CanAddContentToPage()))
                {
                    throw new UnauthorizedAccessException("Access denied");
                }
            }
        }

        private bool IsSharedViewOnly()
        {
            return this.Module.IsShared && this.Module.IsShareableViewOnly;
        }

        private void BindData(ModuleInfo module, ModuleSettingsModel model)
        {
            var desktopModule = DesktopModuleController.GetDesktopModule(module.DesktopModuleID, this.PortalSettings.PortalId);

            if (!module.IsShared)
            {
                model.InheritViewPermissions = module.InheritViewPermissions;
            }

            model.ModuleId = module.ModuleID;
            model.FriendlyName = module.DesktopModule.FriendlyName;
            model.ModuleTitle = module.ModuleTitle;
            model.IconFile = module.IconFile;

            model.AllTabs = module.AllTabs;
            model.AllowIndex = this.GetBooleanSetting(module, "AllowIndex", true);
            model.Moniker = this.GetStringSetting(module, "Moniker");

            model.Visibility = module.Visibility;
            model.HideAdminBorder = this.GetBooleanSetting(module, "hideadminborder", false);

            var objModuleDef = ModuleDefinitionController.GetModuleDefinitionByID(module.ModuleDefID);
            if (objModuleDef.DefaultCacheTime == Null.NullInteger)
            {
                model.CacheWarningVisible = true;
                model.CacheTime = module.CacheTime;
            }
            else
            {
                model.CacheWarningVisible = false;
                model.CacheTime = module.CacheTime;
            }

            model.CacheProvider = module.CacheMethod;
            model.CacheInheritedVisible = module.CacheMethod != module.GetEffectiveCacheMethod();

            model.Alignment = module.Alignment;
            model.Color = module.Color;
            model.Border = module.Border;

            model.Header = module.Header;
            model.Footer = module.Footer;

            if (!Null.IsNull(module.StartDate))
            {
                model.StartDate = module.StartDate;
            }

            if (!Null.IsNull(module.EndDate))
            {
                model.EndDate = module.EndDate;
            }

            this.BindContainers(model, module);

            model.DisplayTitle = module.DisplayTitle;
            model.DisplayPrint = module.DisplayPrint;
            model.DisplaySyndicate = module.DisplaySyndicate;

            if (module.ModuleID == this.PortalSettings.DefaultModuleId && module.TabID == this.PortalSettings.DefaultTabId)
            {
                model.IsDefaultModule = true;
            }

            if (!module.IsShared && module.DesktopModule.Shareable != ModuleSharing.Unsupported)
            {
                model.IsShareable = module.IsShareable;
                model.IsShareableViewOnly = module.IsShareableViewOnly;
                model.IsShareableVisible = true;
            }

            model.AvailableTabs = new List<TabModel>();
            var tabsByModule = TabController.Instance.GetTabsByModuleID(this.moduleId);
            tabsByModule.Remove(this.TabId);
            model.InstalledOnTabs = tabsByModule.Select(t => new TabModel()
            {
                Id = t.Value.TabID,
                Name = t.Value.TabName,
                InstalledOnLink = this.GetInstalledOnLink(t.Value),
                InstalledOnSite = this.GetInstalledOnSite(t.Value),
            });
        }

        private string GetInstalledOnLink(TabInfo tab)
        {
            var returnValue = new StringBuilder();
            if (tab != null)
            {
                var index = 0;
                TabController.Instance.PopulateBreadCrumbs(ref tab);
                var defaultAlias = PortalAliasController.Instance.GetPortalAliasesByPortalId(tab.IsSuperTab ? Host.HostPortalID : tab.PortalID)
                                        .OrderByDescending(a => a.IsPrimary)
                                        .FirstOrDefault();
                var portalSettings = new PortalSettings(tab.PortalID)
                {
                    PortalAlias = defaultAlias,
                };

                var tabUrl = this.NavigationManager.NavigateURL(tab.TabID, portalSettings, string.Empty);

                foreach (TabInfo t in tab.BreadCrumbs)
                {
                    if (index > 0)
                    {
                        returnValue.Append(" > ");
                    }

                    if (tab.BreadCrumbs.Count - 1 == index)
                    {
                        returnValue.AppendFormat("<a href=\"{0}\">{1}</a>", tabUrl, t.LocalizedTabName);
                    }
                    else
                    {
                        returnValue.AppendFormat("{0}", t.LocalizedTabName);
                    }

                    index = index + 1;
                }
            }

            return returnValue.ToString();
        }

        private string GetInstalledOnSite(TabInfo tab)
        {
            string returnValue = string.Empty;
            if (tab != null)
            {
                var portal = PortalController.Instance.GetPortal(tab.PortalID);
                if (portal != null)
                {
                    returnValue = portal.PortalName;
                }
            }

            return returnValue;
        }

        private void BindContainers(ModuleSettingsModel model, ModuleInfo module)
        {
            model.ContainerSrc = module.ContainerSrc;
            model.ContainerOptions = SkinController.GetSkins(this.Portal, SkinController.RootContainer, SkinScope.All)
                                        .Select(c => new SelectListItem { Text = c.Key, Value = c.Value });
        }

        private bool GetBooleanSetting(ModuleInfo module, string settingName, bool defaultValue)
        {
            var setting = module.TabModuleSettings[settingName];
            return setting != null ? bool.Parse(setting.ToString()) : defaultValue;
        }

        private string GetStringSetting(ModuleInfo module, string settingName)
        {
            var setting = module.TabModuleSettings[settingName];
            return setting?.ToString() ?? string.Empty;
        }
    }
}
