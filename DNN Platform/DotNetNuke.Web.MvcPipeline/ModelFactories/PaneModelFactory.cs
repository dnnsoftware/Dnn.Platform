// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Builds and configures <see cref="PaneModel"/> instances for module placement.
    /// </summary>
    public class PaneModelFactory : IPaneModelFactory
    {
        private readonly IContainerModelFactory containerModelFactory;
        private readonly IHostSettings hostSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaneModelFactory"/> class.
        /// </summary>
        /// <param name="containerModelFactory">The container model factory.</param>
        /// <param name="hostSettings">The host settings.</param>
        public PaneModelFactory(IContainerModelFactory containerModelFactory, IHostSettings hostSettings)
        {
            this.containerModelFactory = containerModelFactory;
            this.hostSettings = hostSettings;
        }

        /// <inheritdoc/>
        public PaneModel CreatePane(string name)
        {
            var pane = new PaneModel(name);
            return pane;
        }

        /// <inheritdoc/>
        public PaneModel InjectModule(DnnPageController page, PaneModel pane, ModuleInfo moduleInfo, IPortalSettings portalSettings)
        {
            try
            {
                // Load container control
                var container = this.LoadModuleContainer(page, moduleInfo, portalSettings);

                // Add Container to Dictionary
                pane.Containers.Add(container.ID, container);
            }
            catch (ThreadAbortException)
            {
                // Response.Redirect may called in module control's OnInit method, so it will cause ThreadAbortException, no need any action here.
            }
            catch (Exception exc)
            {
                var lex = new ModuleLoadException(string.Format(CultureInfo.InvariantCulture, Skin.MODULEADD_ERROR, pane.Name), exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    throw lex;
                }

                Exceptions.LogException(exc);
            }

            return pane;
        }

        private ContainerModel LoadContainerFromCookie(HttpRequestBase request, IPortalSettings portalSettings)
        {
            ContainerModel container = null;
            var cookie = request.Cookies["_ContainerSrc" + portalSettings.PortalId];
            if (cookie != null)
            {
                if (!string.IsNullOrEmpty(cookie.Value))
                {
                    // container = this.LoadContainerByPath(SkinController.FormatSkinSrc(cookie.Value + ".ascx", this.PortalSettings));
                }
            }

            return container;
        }

        private ContainerModel LoadModuleContainer(DnnPageController page, ModuleInfo module, IPortalSettings portalSettings)
        {
            var containerSrc = Null.NullString;
            var cuurrentPage = TabController.CurrentPage;

            // var request = this.PaneControl.Page.Request;
            ContainerModel container = null;

            if (portalSettings.EnablePopUps && UrlUtils.InPopUp())
            {
                containerSrc = module.ContainerPath + "popUpContainer.ascx";

                // Check Skin for a popup Container
                if (module.ContainerSrc == cuurrentPage.ContainerSrc)
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(containerSrc)))
                    {
                        container = this.LoadContainerByPath(containerSrc, module, PortalSettings.Current);
                    }
                }

                // error loading container - load default popup container
                if (container == null)
                {
                    containerSrc = Globals.HostPath + "Containers/_default/popUpContainer.ascx";
                    container = this.LoadContainerByPath(containerSrc, module, PortalSettings.Current);
                }
            }
            else
            {
                container = (this.LoadContainerFromQueryString(module, page.Request, PortalSettings.Current) ?? this.LoadContainerFromCookie(page.Request, portalSettings)) ?? this.LoadNoContainer(module, portalSettings);
                /* not sur what this dous
                if (container == null)
                {
                   // Check Skin for Container
                   var masterModules = portalSettings.ActiveTab.ChildModules;
                   if (masterModules.ContainsKey(module.ModuleID) && string.IsNullOrEmpty(masterModules[module.ModuleID].ContainerSrc))
                   {
                       // look for a container specification in the skin pane

                       if (this.PaneControl != null)
                       {
                           if (this.PaneControl.Attributes["ContainerSrc"] != null)
                           {
                               container = this.LoadContainerFromPane();
                           }
                       }
                    }
                }
                */

                // else load assigned container
                if (container == null)
                {
                    containerSrc = module.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, PortalSettings.Current);
                        container = this.LoadContainerByPath(containerSrc, module, PortalSettings.Current);
                    }
                }

                // error loading container - load from tab
                if (container == null)
                {
                    containerSrc = cuurrentPage.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, PortalSettings.Current);
                        container = this.LoadContainerByPath(containerSrc, module, PortalSettings.Current);
                    }
                }

                // error loading container - load default
                if (container == null)
                {
                    containerSrc = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalContainer(), PortalSettings.Current);
                    container = this.LoadContainerByPath(containerSrc, module, PortalSettings.Current);
                }
            }

            // Set container path
            module.ContainerPath = SkinController.FormatSkinPath(containerSrc);

            // set container id to an explicit short name to reduce page payload
            container.ID = "ctr";

            // make the container id unique for the page
            if (module.ModuleID > -1)
            {
                container.ID += module.ModuleID.ToString(CultureInfo.InvariantCulture);
            }

            container.EditMode = Personalization.GetUserMode() == PortalSettings.Mode.Edit;

            return container;
        }

        private ContainerModel LoadContainerByPath(string containerPath, ModuleInfo module, IPortalSettings portalSettings)
        {
            if (containerPath.IndexOf("/skins/", StringComparison.InvariantCultureIgnoreCase) != -1 || containerPath.IndexOf("/skins\\", StringComparison.InvariantCultureIgnoreCase) != -1 || containerPath.IndexOf("\\skins\\", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                containerPath.IndexOf("\\skins/", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                throw new ArgumentException("containerPath /skins/");
            }

            ContainerModel container = null;

            try
            {
                var containerSrc = containerPath;
                if (containerPath.IndexOf(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    containerPath = containerPath.Remove(0, Globals.ApplicationPath.Length);
                }

                container = this.containerModelFactory.CreateContainerModel(module, portalSettings, containerSrc, containerPath);
            }
            catch (Exception exc)
            {
                // could not load user control
                var lex = new ModuleLoadException(Skin.MODULELOAD_ERROR, exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    throw lex;
                }

                Exceptions.LogException(lex);
            }

            return container;
        }

        private ContainerModel LoadContainerFromQueryString(ModuleInfo module, HttpRequestBase request, PortalSettings portalSettings)
        {
            ContainerModel container = null;
            int previewModuleId = -1;
            if (request.QueryString["ModuleId"] != null)
            {
                if (!int.TryParse(request.QueryString["ModuleId"], out previewModuleId))
                {
                    previewModuleId = -1;
                }
            }

            // load user container ( based on cookie )
            if (request.QueryString["ContainerSrc"] != null && (module.ModuleID == previewModuleId || previewModuleId == -1))
            {
                string containerSrc = SkinController.FormatSkinSrc(Globals.QueryStringDecode(request.QueryString["ContainerSrc"]) + ".ascx", portalSettings);
                container = this.LoadContainerByPath(containerSrc, module, portalSettings);
            }

            return container;
        }

        private ContainerModel LoadNoContainer(ModuleInfo module, IPortalSettings portalSettings)
        {
            string noContainerSrc = "[G]" + SkinController.RootContainer + "/_default/No Container.ascx";
            ContainerModel container = null;

            // if the module specifies that no container should be used
            if (module.DisplayTitle == false)
            {
                // always display container if the current user is the administrator or the module is being used in an admin case
                bool displayTitle = ModulePermissionController.CanEditModuleContent(module) || Globals.IsAdminSkin();

                // unless the administrator is in view mode
                if (displayTitle)
                {
                    displayTitle = Personalization.GetUserMode() != PortalSettings.Mode.View;
                }

                if (displayTitle == false)
                {
                    container = this.LoadContainerByPath(SkinController.FormatSkinSrc(noContainerSrc, PortalSettings.Current), module, portalSettings);
                }
            }

            return container;
        }
    }
}
