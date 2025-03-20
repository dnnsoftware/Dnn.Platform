// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.MvcPipeline.Models;

    public class ContainerModelFactory : IContainerModelFactory
    {
        public ContainerModelFactory()
        {
        }

        public ContainerModel CreateContainerModel(ModuleInfo configuration, PortalSettings portalSettings)
        {
            var container = new ContainerModel(configuration);
            container = this.ProcessModule(container, portalSettings);
            return container;
        }

        private ContainerModel ProcessModule(ContainerModel container, PortalSettings portalSettings)
        {
            /*
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"Container.ProcessModule Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{this.ModuleConfiguration.ModuleDefinition.FriendlyName}')");
            }
            */
            // Process Content Pane Attributes
            container = this.ProcessContentPane(container, portalSettings);

            // always add the actions menu as the first item in the content pane.
            /*
            if (this.InjectActionMenu && !ModuleHost.IsViewMode(this.ModuleConfiguration, this.PortalSettings) && this.Request.QueryString["dnnprintmode"] != "true")
            {
                MvcJavaScript.RequestRegistration(CommonJs.DnnPlugins);
                this.ContentPane.Controls.Add(this.LoadControl(this.PortalSettings.DefaultModuleActionMenu));

                // register admin.css
                MvcClientResourceManager.RegisterAdminStylesheet(this.Page, Globals.HostPath + "admin.css");
            }
            */

            // Process Module Header
            container = this.ProcessHeader(container);

            // Try to load the module control
            // container.moduleHost = new ModuleHostModel(this.ModuleConfiguration, this.ParentSkin, this);
            // container.moduleHost.OnPreRender();

            /*
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"Container.ProcessModule Info (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): ControlPane.Controls.Add(ModuleHost:{this.moduleHost.ID})");
            }

            this.ContentPane.Controls.Add(this.ModuleHost);
            */

            // Process Module Footer
            container = this.ProcessFooter(container);

            /*
            // Process the Action Controls
            if (this.ModuleHost != null && this.ModuleControl != null)
            {
                this.ProcessChildControls(this);
            }
            */

            // Add Module Stylesheets
            container = this.ProcessStylesheets(container, container.ModuleHost != null);

            /*
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"Container.ProcessModule End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{this.ModuleConfiguration.ModuleDefinition.FriendlyName}')");
            }
            */

            return container;
        }

        private ContainerModel ProcessContentPane(ContainerModel container, PortalSettings portalSettings)
        {
            container = this.SetAlignment(container);

            container = this.SetBackground(container);

            container = this.SetBorder(container);

            // display visual indicator if module is only visible to administrators
            string viewRoles = container.ModuleConfiguration.InheritViewPermissions
                                   ? TabPermissionController.GetTabPermissions(container.ModuleConfiguration.TabID, container.ModuleConfiguration.PortalID).ToString("VIEW")
                                   : container.ModuleConfiguration.ModulePermissions.ToString("VIEW");

            string pageEditRoles = TabPermissionController.GetTabPermissions(container.ModuleConfiguration.TabID, container.ModuleConfiguration.PortalID).ToString("EDIT");
            string moduleEditRoles = container.ModuleConfiguration.ModulePermissions.ToString("EDIT");

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
            // MvcClientResourceManager.RegisterStyleSheet(this.Page.ControllerContext, container.ContainerPath + "container.css", FileOrder.Css.ContainerCss);
            container.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = container.ContainerPath + "container.css", FileOrder = FileOrder.Css.ContainerCss });

            // MvcClientResourceManager.RegisterStyleSheet(this.Page.ControllerContext, container.ContainerSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificContainerCss);
            container.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = container.ContainerSrc.Replace(".ascx", ".css"), FileOrder = FileOrder.Css.SpecificContainerCss });

            // process the base class module properties
            if (includeModuleCss)
            {
                string controlSrc = container.ModuleConfiguration.ModuleControl.ControlSrc;
                string folderName = container.ModuleConfiguration.DesktopModule.FolderName;

                string stylesheet = string.Empty;
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

                    // MvcClientResourceManager.RegisterStyleSheet(this.Page.ControllerContext, stylesheet, FileOrder.Css.ModuleCss);
                    container.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = stylesheet, FileOrder = FileOrder.Css.ModuleCss });
                }

                var ix = controlSrc.LastIndexOf("/", StringComparison.Ordinal);
                if (ix >= 0)
                {
                    stylesheet = Globals.ApplicationPath + "/" + controlSrc.Substring(0, ix + 1) + "module.css";

                    // MvcClientResourceManager.RegisterStyleSheet(this.Page.ControllerContext, stylesheet, FileOrder.Css.ModuleCss);
                    container.RegisteredStylesheets.Add(new RegisteredStylesheet { Stylesheet = stylesheet, FileOrder = FileOrder.Css.ModuleCss });
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
            // this.ContentPane.Controls.Add(new LiteralControl(string.Format("<div class=\"dnnFormMessage dnnFormInfo dnnFormInfoAdminErrMssg\">{0}</div>", message)));
            return container;
        }
    }
}
