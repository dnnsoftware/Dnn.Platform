// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.MvcPipeline.Routing;
    using DotNetNuke.Web.MvcPipeline.Utils;

    public class ModuleControllerBase : Controller, IMvcController
    {
        private readonly Lazy<ModuleInfo> activeModule;
        /*
        private readonly ILog tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly Lazy<ServiceScopeContainer> serviceScopeContainer = new Lazy<ServiceScopeContainer>(ServiceScopeContainer.GetRequestOrCreateScope);
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;
        private DesktopModuleInfo desktopModule;
        */

        public ModuleControllerBase()
        {
            this.activeModule = new Lazy<ModuleInfo>(this.InitModuleInfo);
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>Gets userInfo for the current user.</summary>
        public UserInfo UserInfo
        {
            get { return this.PortalSettings.UserInfo; }
        }

        public ModuleInfo ActiveModule
        {
            get { return this.activeModule.Value; }
        }

        protected ActionResult View(ModuleInfo module, object model)
        {
            return this.View(MvcUtils.GetControlViewName(module), model);
        }

        protected ActionResult PartialView(ModuleInfo module, string viewName, object model)
        {
            return this.View(MvcUtils.GetControlViewName(module, viewName), model);
        }

        private ModuleInfo InitModuleInfo()
        {
            return this.HttpContext.Request.FindModuleInfo();
        }

        /*
        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return PortalController.Instance.GetCurrentPortalSettings();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the EditMode property is used to determine whether the user is in the
        /// Administrator role
        /// Cache.
        /// </summary>
        public bool EditMode
        {
            get
            {
                return this.ModuleContext.EditMode;
            }
        }

        public bool IsEditable
        {
            get
            {
                return this.ModuleContext.IsEditable;
            }
        }

        public int PortalId
        {
            get
            {
                return this.ModuleContext.PortalId;
            }
        }

        public int TabId
        {
            get
            {
                return this.ModuleContext.TabId;
            }
        }

        public UserInfo UserInfo
        {
            get
            {
                return this.PortalSettings.UserInfo;
            }
        }

        public int UserId
        {
            get
            {
                return this.PortalSettings.UserId;
            }
        }

        public PortalAliasInfo PortalAlias
        {
            get
            {
                return this.PortalSettings.PortalAlias;
            }
        }

        public Hashtable Settings
        {
            get
            {
                return this.ModuleContext.Settings;
            }
        }

        /// <summary>Gets the Path for this control (used primarily for UserControls).</summary>
        /// <returns>A String.</returns>
        public string ControlPath
        {
            get
            {
                return "/" + Path.GetDirectoryName(this.ModuleConfiguration.ModuleControl.ControlSrc);
            }
        }

        /// <summary>Gets the Name for this control.</summary>
        /// <returns>A String.</returns>
        public string ControlName
        {
            get
            {
                return this.GetType().Name.Replace("_", ".");
            }
        }

        /// <summary>Gets the Module Context for this control.</summary>
        /// <returns>A ModuleInstanceContext.</returns>
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (this.moduleContext == null)
                {
                    this.moduleContext = new ModuleInstanceContext()
                    {
                        Configuration = this.ActiveModule,
                    };
                }

                return this.moduleContext;
            }
        }

        public DesktopModuleInfo DesktopModule
        {
            get
            {
                if (this.desktopModule == null)
                {
                    this.desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(this.ActiveModule.DesktopModuleID, this.ActiveModule.PortalID);
                }

                return this.desktopModule;
            }
        }

        public ModuleActionCollection Actions
        {
            get
            {
                return this.ModuleContext.Actions;
            }

            set
            {
                this.ModuleContext.Actions = value;
            }
        }

        public string HelpURL
        {
            get
            {
                return this.ModuleContext.HelpURL;
            }

            set
            {
                this.ModuleContext.HelpURL = value;
            }
        }

        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this.ModuleContext.Configuration;
            }

            set
            {
                this.ModuleContext.Configuration = value;
            }
        }

        public int TabModuleId
        {
            get
            {
                return this.ModuleContext.TabModuleId;
            }

            set
            {
                this.ModuleContext.TabModuleId = value;
            }
        }

        public int ModuleId
        {
            get
            {
                return this.ModuleContext.ModuleId;
            }

            set
            {
                this.ModuleContext.ModuleId = value;
            }
        }

        public string ID
        {
            get
            {
                return Path.GetFileName(this.ModuleConfiguration.ModuleControl.ControlSrc);
            }
        }

        /// <summary>Gets or sets the local resource file for this control.</summary>
        /// <returns>A String.</returns>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = Path.Combine(this.ControlPath, Localization.LocalResourceDirectory + "/" + this.ID);
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        /// <summary>
        /// Gets the Dependency Provider to resolve registered
        /// services with the container.
        /// </summary>
        /// <value>
        /// The Dependency Service.
        /// </value>
        protected IServiceProvider DependencyProvider => this.serviceScopeContainer.Value.ServiceScope.ServiceProvider;

        public string EditUrl()
        {
            return this.ModuleContext.EditUrl();
        }

        public string EditUrl(string controlKey)
        {
            return this.ModuleContext.EditUrl(controlKey);
        }

        public string EditUrl(string keyName, string keyValue)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue);
        }

        public string EditUrl(string keyName, string keyValue, string controlKey)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue, controlKey);
        }

        public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue, controlKey, additionalParameters);
        }

        public string EditUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return this.ModuleContext.NavigateUrl(tabID, controlKey, pageRedirect, additionalParameters);
        }

        public int GetNextActionID()
        {
            return this.ModuleContext.GetNextActionID();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            if (this.serviceScopeContainer.IsValueCreated)
            {
                this.serviceScopeContainer.Value.Dispose();
            }
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected string LocalizeSafeJsString(string key)
        {
            return Localization.GetSafeJSString(key, this.LocalResourceFile);
        }
        */
    }
}
