// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// Base implementation for MVC module controls, exposing common DNN context and services.
    /// </summary>
    public abstract class DefaultMvcModuleControlBase : IMvcModuleControl, IDisposable
    {
        private readonly Lazy<ServiceScopeContainer> serviceScopeContainer = new Lazy<ServiceScopeContainer>(ServiceScopeContainer.GetRequestOrCreateScope);
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;

        /// <summary>
        /// Gets the module configuration associated with this control.
        /// </summary>
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this.ModuleContext.Configuration;
            }
        }

        /// <summary>
        /// Gets the identifier of the current tab.
        /// </summary>
        public int TabId
        {
            get
            {
                return this.ModuleContext.TabId;
            }
        }

        /// <summary>
        /// Gets the identifier of the current module.
        /// </summary>
        public int ModuleId
        {
            get
            {
                return this.ModuleContext.ModuleId;
            }
        }

        /// <summary>
        /// Gets the identifier of the current tab-module instance.
        /// </summary>
        public int TabModuleId
        {
            get
            {
                return this.ModuleContext.TabModuleId;
            }
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
        /// Gets the identifier of the current portal.
        /// </summary>
        public int PortalId
        {
            get
            {
                return this.ModuleContext.PortalId;
            }
        }

        /// <summary>
        /// Gets information about the current user.
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

        /// <summary>Gets or sets the underlying base control for this module control.</summary>
        /// <returns>A <see cref="Control"/> instance.</returns>
        public Control Control { get; set; }

        /// <summary>Gets the name for this control.</summary>
        /// <returns>A string representing the control name.</returns>
        public virtual string ControlName
        {
            get
            {
                if (string.IsNullOrEmpty(this.ModuleConfiguration.ModuleControl.ControlKey))
                {
                    return "View";
                }
                else
                {
                    return this.ModuleConfiguration.ModuleControl.ControlKey;
                }
            }
        }

        /// <summary>Gets the path for this control (used primarily for user controls).</summary>
        /// <returns>A string representing the control path.</returns>
        public virtual string ControlPath
        {
            get
            {
                if (this.ModuleConfiguration.DesktopModule != null)
                {
                    return "DesktopModules/" + this.ModuleConfiguration.DesktopModule.FolderName;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the default resource file name for this control.
        /// </summary>
        public virtual string ResourceName
        {
            get
            {
                return this.ControlName + ".resx";
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
                    this.moduleContext = new ModuleInstanceContext(this);
                }

                return this.moduleContext;
            }
        }

        /// <summary>
        /// Gets or sets the module action collection for this control.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the help URL associated with this control.
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

        /// <summary>Gets or sets the local resource file for this control.</summary>
        /// <returns>A string representing the resource file path.</returns>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = "~/" + this.ControlPath + "/" + Localization.LocalResourceDirectory + "/" + this.ResourceName;
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

        /// <summary>
        /// Gets the next available action identifier for this module.
        /// </summary>
        /// <returns>The next action identifier.</returns>
        public int GetNextActionID()
        {
            return this.ModuleContext.GetNextActionID();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // base.Dispose();
            if (this.serviceScopeContainer.IsValueCreated)
            {
                this.serviceScopeContainer.Value.Dispose();
            }
        }

        /// <summary>
        /// Renders the module control as HTML using the specified HTML helper.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The rendered HTML.</returns>
        public abstract IHtmlString Html(HtmlHelper htmlHelper);
    }
}
