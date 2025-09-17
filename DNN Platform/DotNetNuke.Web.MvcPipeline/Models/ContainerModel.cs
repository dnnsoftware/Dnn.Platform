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

    public class ContainerModel
    {
        private ModuleInfo moduleConfiguration;
        private ModuleHostModel moduleHost;
        private PortalSettings portalSettings;

        public ContainerModel(ModuleInfo moduleConfiguration, PortalSettings portalSettings)
        {
            this.moduleConfiguration = moduleConfiguration;
            this.moduleHost = new ModuleHostModel(moduleConfiguration);
            this.portalSettings = portalSettings;
        }

        public PortalSettings PortalSettings
        {
            get
            {
                return this.portalSettings;
            }
        }
        public ModuleHostModel ModuleHost
        {
            get
            {
                return this.moduleHost;
            }
        }

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

        public string ID { get; internal set; }

        public string ContainerPath
        {
            get
            {
                return Path.GetDirectoryName(this.ContainerSrc) + "/";
            }
        }

        public string ContainerSrc { get; internal set; }

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

        public string ContainerRazorFile
        {
            get
            {
                return "~" + Path.GetDirectoryName(this.ContainerSrc) + "/Views/" + Path.GetFileName(this.ContainerSrc).Replace(".ascx", ".cshtml");
            }
        }

        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this.moduleConfiguration;
            }
        }

        public bool EditMode { get; internal set; }

        public string Footer { get; internal set; }

        public string Header { get; internal set; }

        public string ContentPaneCssClass { get; internal set; }

        public string ContentPaneStyle { get; internal set; }

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
