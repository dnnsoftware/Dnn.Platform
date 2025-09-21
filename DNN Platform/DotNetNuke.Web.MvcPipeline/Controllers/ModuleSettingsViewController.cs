// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;

    public class ModuleSettingsViewController : ModuleControllerBase
    {
        private readonly ModuleController moduleController;
        private readonly IContentSecurityPolicy contentSecurityPolicy;
        private int moduleId = -1;
        private ModuleInfo module;

        public ModuleSettingsViewController(IContentSecurityPolicy csp)
        {
            this.moduleController = new ModuleController();
            this.contentSecurityPolicy = csp;
        }

        public int TabId
        {
            get
            {
                return this.PortalSettings.ActiveTab.TabID;
            }
        }

        public int TabModuleId { get; private set; }

        private PortalInfo Portal
        {
            get { return this.PortalSettings.PortalId == Null.NullInteger ? null : PortalController.Instance.GetPortal(this.PortalSettings.PortalId); }
        }

        private ModuleInfo Module
        {
            get { return this.module ?? (this.module = ModuleController.Instance.GetModule(this.moduleId, this.TabId, false)); }
        }

        [HttpGet]
        public ActionResult Invoke(int moduleId)
        {
            this.Initialize();
            var model = new ModuleSettingsModel();
            model.TabId = this.TabId;
            model.ReturnUrl = this.Request.QueryString["ReturnURL"];

            ModuleControlInfo moduleControlInfo = ModuleControlController.GetModuleControlByControlKey("Settings", this.Module.ModuleDefID);
            if (moduleControlInfo != null)
            {
                model.ModuleControlSrc = moduleControlInfo.ControlSrc;
                model.ModuleControllerName = this.Module.DesktopModule.ModuleName;
                model.ModuleActionName = "LoadSettings";
                model.ModuleLocalResourceFile = Path.Combine(Path.GetDirectoryName(moduleControlInfo.ControlSrc), Localization.LocalResourceDirectory + "/" + Path.GetFileNameWithoutExtension(moduleControlInfo.ControlSrc));
            }

            this.BindData(this.Module, model);
            MvcJavaScript.RequestRegistration(CommonJs.DnnPlugins);
            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/Resources/Shared/scripts/jquery/jquery.form.min.js");
            MvcClientResourceManager.RegisterScript(this.ControllerContext, "~/admin/Modules/module.js");
            this.contentSecurityPolicy.ImgSource.AddScheme("data:");
            this.contentSecurityPolicy.StyleSource.AddInline();
            return this.View(model);
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
