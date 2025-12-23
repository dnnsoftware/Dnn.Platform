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

    /// <summary>
    /// Base controller for MVC view controllers hosting classic module controls.
    /// </summary>
    public abstract class ModuleViewControllerBase : Controller, IMvcController
    {
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleViewControllerBase"/> class.
        /// </summary>
        public ModuleViewControllerBase()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the current tab is a host (superuser) tab.
        /// </summary>
        public bool IsHostMenu
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the current module is editable for the user.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return this.ModuleContext.IsEditable;
            }
        }

        /// <summary>
        /// Gets the portal identifier for the current module context.
        /// </summary>
        public int PortalId
        {
            get
            {
                return this.ModuleContext.PortalId;
            }
        }

        /// <summary>
        /// Gets the tab identifier for the current module context.
        /// </summary>
        public int TabId
        {
            get
            {
                return this.ModuleContext.TabId;
            }
        }

        /// <summary>
        /// Gets the user information for the current user.
        /// </summary>
        public UserInfo UserInfo
        {
            get
            {
                return this.PortalSettings.UserInfo;
            }
        }

        /// <summary>
        /// Gets the identifier of the current user.
        /// </summary>
        public int UserId
        {
            get
            {
                return this.PortalSettings.UserId;
            }
        }

        /// <summary>
        /// Gets the current portal alias information.
        /// </summary>
        public PortalAliasInfo PortalAlias
        {
            get
            {
                return this.PortalSettings.PortalAlias;
            }
        }

        /// <summary>
        /// Gets the settings for the current module instance.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the help URL associated with the module.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the module configuration.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the tab-module identifier for the current instance.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the module identifier for the current instance.
        /// </summary>
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

        /// <summary>
        /// Gets the folder name of the associated desktop module.
        /// </summary>
        public virtual string FolderName
        {
            get
            {
                return this.ModuleContext.Configuration.DesktopModule.FolderName;
            }
        }

        /// <summary>
        /// Gets the base name of the resource file used for localization.
        /// </summary>
        public virtual string ResourceName
        {
            get
            {
                return this.GetType().Name.Replace("ViewController", string.Empty);
            }
        }

        /// <summary>
        /// Builds an edit URL for the current module instance.
        /// </summary>
        /// <returns>The edit URL.</returns>
        public string EditUrl()
        {
            return this.ModuleContext.EditUrl();
        }

        /// <summary>
        /// Builds an edit URL for the specified control key.
        /// </summary>
        /// <param name="controlKey">The control key.</param>
        /// <returns>The edit URL.</returns>
        public string EditUrl(string controlKey)
        {
            return this.ModuleContext.EditUrl(controlKey);
        }

        /// <summary>
        /// Builds an edit URL for the specified key and value.
        /// </summary>
        /// <param name="keyName">The query string key.</param>
        /// <param name="keyValue">The query string value.</param>
        /// <returns>The edit URL.</returns>
        public string EditUrl(string keyName, string keyValue)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue);
        }

        /// <summary>
        /// Builds an edit URL for the specified key, value, and control key.
        /// </summary>
        /// <param name="keyName">The query string key.</param>
        /// <param name="keyValue">The query string value.</param>
        /// <param name="controlKey">The control key.</param>
        /// <returns>The edit URL.</returns>
        public string EditUrl(string keyName, string keyValue, string controlKey)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue, controlKey);
        }

        /// <summary>
        /// Builds an edit URL for the specified key, value, control key, and additional parameters.
        /// </summary>
        /// <param name="keyName">The query string key.</param>
        /// <param name="keyValue">The query string value.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="additionalParameters">Additional route parameters.</param>
        /// <returns>The edit URL.</returns>
        public string EditUrl(string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            return this.ModuleContext.EditUrl(keyName, keyValue, controlKey, additionalParameters);
        }

        /// <summary>
        /// Builds an edit URL for the specified tab, control key, and additional parameters.
        /// </summary>
        /// <param name="tabID">The target tab identifier.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="pageRedirect">A value indicating whether to perform a page redirect.</param>
        /// <param name="additionalParameters">Additional route parameters.</param>
        /// <returns>The edit URL.</returns>
        public string EditUrl(int tabID, string controlKey, bool pageRedirect, params string[] additionalParameters)
        {
            return this.ModuleContext.NavigateUrl(tabID, controlKey, pageRedirect, additionalParameters);
        }

        /// <summary>
        /// Gets the next available action identifier.
        /// </summary>
        /// <returns>The next action identifier.</returns>
        public int GetNextActionID()
        {
            return this.ModuleContext.GetNextActionID();
        }

        /// <summary>
        /// Gets the view model for the module view.
        /// </summary>
        /// <returns>The view model object.</returns>
        public abstract object ViewModel();

        /// <summary>
        /// Gets the name of the view used to render the module.
        /// </summary>
        /// <returns>The view name.</returns>
        public abstract string ViewName();

        /// <summary>
        /// Gets the localized string for the specified key.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized string.</returns>
        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        /// <summary>
        /// Gets a localized string that is safe for use in JavaScript.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized JavaScript-safe string.</returns>
        protected string LocalizeSafeJsString(string key)
        {
            return Localization.GetSafeJSString(key, this.LocalResourceFile);
        }

        /// <summary>
        /// Invokes the module view for the specified input configuration.
        /// </summary>
        /// <param name="input">The control view model input.</param>
        /// <returns>The partial view result.</returns>
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
            this.moduleContext.Configuration = activeModule;
            var model = this.ViewModel();
            return this.PartialView(this.ViewName(), model);
        }
    }
}
