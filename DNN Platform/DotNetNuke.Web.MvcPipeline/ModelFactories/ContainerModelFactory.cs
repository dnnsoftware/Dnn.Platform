// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using System;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Builds and configures <see cref="ContainerModel"/> instances for module containers.
    /// </summary>
    public class ContainerModelFactory : IContainerModelFactory
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerModelFactory"/> class.
        /// </summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public ContainerModelFactory(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        /// <inheritdoc/>
        public ContainerModel CreateContainerModel(ModuleInfo configuration, PortalSettings portalSettings, string containerSrc)
        {
            var container = new ContainerModel(configuration, portalSettings);
            container.ContainerSrc = containerSrc;
            container = this.ProcessModule(container, portalSettings);
            return container;
        }

        private ContainerModel ProcessModule(ContainerModel container, PortalSettings portalSettings)
        {
            // Process Content Pane Attributes
            container = this.ProcessContentPane(container, portalSettings);

            // Process Module Header
            container = this.ProcessHeader(container);

            // Process Module Footer
            container = this.ProcessFooter(container);

            // Add Module Stylesheets
            container = this.ProcessStylesheets(container, container.ModuleHost != null);

            return container;
        }

        private ContainerModel ProcessContentPane(ContainerModel container, PortalSettings portalSettings)
        {
            container = this.SetAlignment(container);

            container = this.SetBackground(container);

            container = this.SetBorder(container);

            // display visual indicator if module is only visible to administrators
            var viewRoles = container.ModuleConfiguration.InheritViewPermissions
                                   ? TabPermissionController.GetTabPermissions(container.ModuleConfiguration.TabID, container.ModuleConfiguration.PortalID).ToString("VIEW")
                                   : container.ModuleConfiguration.ModulePermissions.ToString("VIEW");

            var pageEditRoles = TabPermissionController.GetTabPermissions(container.ModuleConfiguration.TabID, container.ModuleConfiguration.PortalID).ToString("EDIT");
            var moduleEditRoles = container.ModuleConfiguration.ModulePermissions.ToString("EDIT");

            viewRoles = viewRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            pageEditRoles = pageEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            moduleEditRoles = moduleEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();

            var showMessage = false;
            var adminMessage = Null.NullString;
            if (viewRoles.Equals(portalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase)
                            && (moduleEditRoles.Equals(portalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase)
                                    || string.IsNullOrEmpty(moduleEditRoles))
                            && pageEditRoles.Equals(portalSettings.AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase))
            {
                adminMessage = Localization.GetString("ModuleVisibleAdministrator.Text");
                showMessage = !container.ModuleConfiguration.HideAdminBorder && !Globals.IsAdminControl();
            }

            if (container.ModuleConfiguration.StartDate >= DateTime.Now)
            {
                adminMessage = string.Format(Localization.GetString("ModuleEffective.Text"), container.ModuleConfiguration.StartDate);
                showMessage = !Globals.IsAdminControl();
            }

            if (container.ModuleConfiguration.EndDate <= DateTime.Now)
            {
                adminMessage = string.Format(Localization.GetString("ModuleExpired.Text"), container.ModuleConfiguration.EndDate);
                showMessage = !Globals.IsAdminControl();
            }

            if (showMessage)
            {
                container = this.AddAdministratorOnlyHighlighting(container, adminMessage);
            }

            return container;
        }

        /// <summary>ProcessFooter adds an optional footer (and an End_Module comment)..</summary>
        private ContainerModel ProcessFooter(ContainerModel container)
        {
            // inject the footer
            if (!string.IsNullOrEmpty(container.ModuleConfiguration.Footer))
            {
                container.Footer = container.ModuleConfiguration.Footer;
            }

            // inject an end comment around the module content
            if (!Globals.IsAdminControl())
            {
                // this.ContentPane.Controls.Add(new LiteralControl("<!-- End_Module_" + this.ModuleConfiguration.ModuleID + " -->"));
            }

            return container;
        }

        /// <summary>ProcessHeader adds an optional header (and a Start_Module_ comment)..</summary>
        private ContainerModel ProcessHeader(ContainerModel container)
        {
            if (!Globals.IsAdminControl())
            {
                // inject a start comment around the module content
                // this.ContentPane.Controls.Add(new LiteralControl("<!-- Start_Module_" + this.ModuleConfiguration.ModuleID + " -->"));
            }

            // inject the header
            if (!string.IsNullOrEmpty(container.ModuleConfiguration.Header))
            {
                container.Header = container.ModuleConfiguration.Header;
            }

            return container;
        }

        /// <summary>
        /// ProcessStylesheets processes the Module and Container stylesheets and adds
        /// them to the Page.
        /// </summary>
        private ContainerModel ProcessStylesheets(ContainerModel container, bool includeModuleCss)
        {
            this.clientResourceController.RegisterStylesheet(container.ContainerPath + "container.css", DotNetNuke.Abstractions.ClientResources.FileOrder.Css.ContainerCss, true);

            if (!string.IsNullOrEmpty(container.ContainerSrc))
            {
                this.clientResourceController.RegisterStylesheet(container.ContainerSrc.Replace(".ascx", ".css"), DotNetNuke.Abstractions.ClientResources.FileOrder.Css.SpecificContainerCss, true);
            }

            // process the base class module properties
            if (includeModuleCss)
            {
                var controlSrc = container.ModuleConfiguration.ModuleControl.ControlSrc;
                var folderName = container.ModuleConfiguration.DesktopModule.FolderName;

                var stylesheet = string.Empty;
                if (string.IsNullOrEmpty(folderName) == false)
                {
                    if (controlSrc.EndsWith(".mvc"))
                    {
                        stylesheet = Globals.ApplicationPath + "/DesktopModules/MVC/" + folderName.Replace("\\", "/") + "/module.css";
                    }
                    else
                    {
                        stylesheet = Globals.ApplicationPath + "/DesktopModules/" + folderName.Replace("\\", "/") + "/module.css";
                    }

                    this.clientResourceController.RegisterStylesheet(stylesheet, DotNetNuke.Abstractions.ClientResources.FileOrder.Css.ModuleCss, true);
                }

                var ix = controlSrc.LastIndexOf("/", StringComparison.Ordinal);
                if (ix >= 0)
                {
                    stylesheet = Globals.ApplicationPath + "/" + controlSrc.Substring(0, ix + 1) + "module.css";

                    this.clientResourceController.RegisterStylesheet(stylesheet, DotNetNuke.Abstractions.ClientResources.FileOrder.Css.ModuleCss, true);
                }
            }

            return container;
        }

        private ContainerModel SetAlignment(ContainerModel container)
        {
            if (!string.IsNullOrEmpty(container.ModuleConfiguration.Alignment))
            {
                container.ContentPaneCssClass += " DNNAlign" + container.ModuleConfiguration.Alignment.ToLowerInvariant();
            }

            return container;
        }

        private ContainerModel SetBackground(ContainerModel container)
        {
            if (!string.IsNullOrEmpty(container.ModuleConfiguration.Color))
            {
                container.ContentPaneStyle += "background-color:" + container.ModuleConfiguration.Color + ";";
            }

            return container;
        }

        private ContainerModel SetBorder(ContainerModel container)
        {
            if (!string.IsNullOrEmpty(container.ModuleConfiguration.Border))
            {
                container.ContentPaneStyle += "border:" + string.Format("{0}px #000000 solid", container.ModuleConfiguration.Border) + ";";
            }

            return container;
        }

        private ContainerModel AddAdministratorOnlyHighlighting(ContainerModel container, string message)
        {
            // todo  this.ContentPane.Controls.Add(new LiteralControl(string.Format("<div class=\"dnnFormMessage dnnFormInfo dnnFormInfoAdminErrMssg\">{0}</div>", message)));
            return container;
        }
    }
}
