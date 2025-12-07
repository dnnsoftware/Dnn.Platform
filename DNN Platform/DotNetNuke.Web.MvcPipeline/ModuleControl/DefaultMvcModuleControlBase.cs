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

    public abstract class DefaultMvcModuleControlBase : IMvcModuleControl, IDisposable
    {
        private readonly Lazy<ServiceScopeContainer> serviceScopeContainer = new Lazy<ServiceScopeContainer>(ServiceScopeContainer.GetRequestOrCreateScope);
        private string localResourceFile;
        private ModuleInstanceContext moduleContext;

        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return ModuleContext.Configuration;
            }
        }

        public int TabId
        {
            get
            {
                return this.ModuleContext.TabId;
            }
        }

        public int ModuleId
        {
            get
            {
                return this.ModuleContext.ModuleId;
            }
        }

        public int TabModuleId
        {
            get
            {
                return this.ModuleContext.TabModuleId;
            }
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

        public int PortalId
        {
            get
            {
                return this.ModuleContext.PortalId;
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

        /// <summary>Gets the underlying base control for this ModuleControl.</summary>
        /// <returns>A String.</returns>
        public Control Control { get; set; }

        /// <summary>Gets the Name for this control.</summary>
        /// <returns>A String.</returns>
        public virtual string ControlName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(this.ModuleConfiguration.ModuleControl.ControlSrc);

            }
        }

        /// <summary>Gets or Sets the Path for this control (used primarily for UserControls).</summary>
        /// <returns>A String.</returns>
        public virtual string ControlPath
        {
            get
            {
                if (this.ModuleConfiguration.DesktopModule == null)
                {
                    return Path.GetDirectoryName(this.ModuleConfiguration.ModuleControl.ControlSrc);
                }
                else
                {
                    return "DesktopModules/" + this.ModuleConfiguration.DesktopModule.FolderName;
                }
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


        /// <summary>Gets or sets the local resource file for this control.</summary>
        /// <returns>A String.</returns>
        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = Path.Combine(this.ControlPath, Localization.LocalResourceDirectory + "/" + this.ControlName);
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

        public int GetNextActionID()
        {
            return this.ModuleContext.GetNextActionID();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // base.Dispose();
            if (this.serviceScopeContainer.IsValueCreated)
            {
                this.serviceScopeContainer.Value.Dispose();
            }
        }
      
        public abstract IHtmlString Html(HtmlHelper htmlHelper);
    }
}
