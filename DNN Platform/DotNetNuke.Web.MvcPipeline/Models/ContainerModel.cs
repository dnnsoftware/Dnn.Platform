// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.UI.Modules;

    /// <summary>
    /// Represents the data and behavior required to render a module container.
    /// </summary>
    public class ContainerModel
    {
        private ModuleInfo moduleConfiguration;
        private ModuleHostModel moduleHost;
        private PortalSettings portalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerModel"/> class.
        /// </summary>
        /// <param name="moduleConfiguration">The module configuration.</param>
        /// <param name="portalSettings">The current portal settings.</param>
        public ContainerModel(ModuleInfo moduleConfiguration, PortalSettings portalSettings)
        {
            this.moduleConfiguration = moduleConfiguration;
            this.moduleHost = new ModuleHostModel(moduleConfiguration);
            this.portalSettings = portalSettings;
        }

        /// <summary>
        /// Gets the portal settings associated with this container.
        /// </summary>
        public PortalSettings PortalSettings
        {
            get
            {
                return this.portalSettings;
            }
        }

        /// <summary>
        /// Gets the module host model used to render the module.
        /// </summary>
        public ModuleHostModel ModuleHost
        {
            get
            {
                return this.moduleHost;
            }
        }

        /// <summary>
        /// Gets the module control instance associated with this container.
        /// </summary>
        public IModuleControl ModuleControl
        {
            get
            {
                IModuleControl moduleControl = null;
                if (this.ModuleHost != null)
                {
                    moduleControl = this.ModuleHost.ModuleControl;
                }

                return moduleControl;
            }
        }

        /// <summary>
        /// Gets the HTML identifier assigned to the container.
        /// </summary>
        public string ID { get; internal set; }

        /// <summary>
        /// Gets the directory path of the container control.
        /// </summary>
        public string ContainerPath
        {
            get
            {
                return Path.GetDirectoryName(this.ContainerSrc) + "/";
            }
        }

        /// <summary>
        /// Gets the source path of the container control.
        /// </summary>
        public string ContainerSrc { get; internal set; }

        /// <summary>
        /// Gets the MVC action name used to render the module within the container.
        /// </summary>
        public string ActionName
        {
            get
            {
                if (this.moduleConfiguration.ModuleControl.ControlKey == "Module")
                {
                    return "LoadDefaultSettings";
                }
                else
                {
                    return string.IsNullOrEmpty(this.ModuleName) ? "Index" : this.FileNameWithoutExtension;
                }
            }
        }

        /// <summary>
        /// Gets the MVC controller name used to render the module within the container.
        /// </summary>
        public string ControllerName
        {
            get
            {
                if (this.moduleConfiguration.ModuleControl.ControlKey == "Module")
                {
                    return "ModuleSettings";
                }
                else
                {
                    return string.IsNullOrEmpty(this.ModuleName) ? this.FileNameWithoutExtension : this.ModuleName;
                }
            }
        }

        /// <summary>
        /// Gets the Razor view path that corresponds to the container control.
        /// </summary>
        public string ContainerRazorFile
        {
            get
            {
                return "~" + Path.GetDirectoryName(this.ContainerSrc) + "/Views/" + Path.GetFileName(this.ContainerSrc).Replace(".ascx", ".cshtml");
            }
        }

        /// <summary>
        /// Gets the module configuration for this container.
        /// </summary>
        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this.moduleConfiguration;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the container is rendered in edit mode.
        /// </summary>
        public bool EditMode { get; internal set; }

        /// <summary>
        /// Gets the footer markup for the container.
        /// </summary>
        public string Footer { get; internal set; }

        /// <summary>
        /// Gets the header markup for the container.
        /// </summary>
        public string Header { get; internal set; }

        /// <summary>
        /// Gets the CSS class applied to the content pane.
        /// </summary>
        public string ContentPaneCssClass { get; internal set; }

        /// <summary>
        /// Gets the inline style applied to the content pane.
        /// </summary>
        public string ContentPaneStyle { get; internal set; }

        /// <summary>
        /// Gets or sets the stylesheets registered by the container.
        /// </summary>
        public List<RegisteredStylesheet> RegisteredStylesheets { get; set; } = new List<RegisteredStylesheet>();

        private string ModuleName
        {
            get
            {
                return this.moduleConfiguration.DesktopModule.ModuleName;
            }
        }

        private string FileNameWithoutExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(this.moduleConfiguration.ModuleControl.ControlSrc);
            }
        }
    }
}
