// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using System.Web.Mvc;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.Utils;

    public abstract class ModuleViewControllerBase : Controller, IMvcController
    {
        /*
        private ModuleInfo activeModule;
        private readonly ILog tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly Lazy<ServiceScopeContainer> serviceScopeContainer = new Lazy<ServiceScopeContainer>(ServiceScopeContainer.GetRequestOrCreateScope);
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;
        private DesktopModuleInfo desktopModule;
        */

        private string localResourceFile;
        private ModuleInstanceContext moduleContext;

        public ModuleViewControllerBase()
        {
        }

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

        /// <summary>Gets the Module Context for this control.</summary>
        /// <returns>A ModuleInstanceContext.</returns>
        public ModuleInstanceContext ModuleContext
        {
            get
            {
                if (this.moduleContext == null)
                {
                    this.moduleContext = new ModuleInstanceContext();
                }

                return this.moduleContext;
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

        /// <summary>Gets or sets the local resource file for this control.</summary>
        /// <returns>A String.</returns>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = "~/DesktopModules/" + this.FolderName + "/" + Localization.LocalResourceDirectory + "/" + this.ResourceName;
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

        public virtual string FolderName
        {
            get
            {
                return this.moduleContext.Configuration.DesktopModule.FolderName;
            }
        }
        public virtual string ResourceName
        {
            get
            {
                return this.GetType().Name.Replace("ViewController", "");
            }
        }

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

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected string LocalizeSafeJsString(string key)
        {
            return Localization.GetSafeJSString(key, this.LocalResourceFile);
        }

        [ChildActionOnly]
        public virtual ActionResult Invoke(ControlViewModel input)
        {
            this.moduleContext = new ModuleInstanceContext();
            var activeModule = ModuleController.Instance.GetModule(input.ModuleId, input.TabId, false);

            if (activeModule.ModuleControlId != input.ModuleControlId)
            {
                activeModule = activeModule.Clone();
                activeModule.ContainerPath = input.ContainerPath;
                activeModule.ContainerSrc = input.ContainerSrc;
                activeModule.ModuleControlId = input.ModuleControlId;
                activeModule.PaneName = input.PanaName;
                activeModule.IconFile = input.IconFile;
            }
            moduleContext.Configuration = activeModule;
            var model = this.ViewModel();
            return this.PartialView(this.ModuleConfiguration, model);
        }

        protected abstract object ViewModel();

        protected ActionResult PartialView(ModuleInfo module, object model)
        {
            return this.View(MvcUtils.GetControlViewName(module), model);
        }

        protected ActionResult PartialView(ModuleInfo module, string viewName, object model)
        {
            return this.View(MvcUtils.GetControlViewName(module, viewName), model);
        }

    }
}
