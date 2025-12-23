// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModelFactories
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Builds and configures <see cref="PaneModel"/> instances for module placement.
    /// </summary>
    public class PaneModelFactory : IPaneModelFactory
    {
        private readonly IContainerModelFactory containerModelFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaneModelFactory"/> class.
        /// </summary>
        /// <param name="containerModelFactory">The container model factory.</param>
        public PaneModelFactory(IContainerModelFactory containerModelFactory)
        {
            this.containerModelFactory = containerModelFactory;
        }

        /// <inheritdoc/>
        public PaneModel CreatePane(string name)
        {
            var pane = new PaneModel(name);
            return pane;
        }

        /// <inheritdoc/>
        public PaneModel InjectModule(PaneModel pane, ModuleInfo module, PortalSettings portalSettings)
        {
            // this.containerWrapperControl = new HtmlGenericControl("div");
            // this.PaneControl.Controls.Add(this.containerWrapperControl);

            // inject module classes
            var classFormatString = "DnnModule DnnModule-{0} DnnModule-{1}";
            var sanitizedModuleName = Null.NullString;

            if (!string.IsNullOrEmpty(module.DesktopModule.ModuleName))
            {
                sanitizedModuleName = Globals.CreateValidClass(module.DesktopModule.ModuleName, false);
            }

            if (this.IsVesionableModule(module))
            {
                classFormatString += " DnnVersionableControl";
            }

            // this.containerWrapperControl.Attributes["class"] = string.Format(classFormatString, sanitizedModuleName, module.ModuleID);
            try
            {
                if (!Globals.IsAdminControl() && (portalSettings.InjectModuleHyperLink || Personalization.GetUserMode() != PortalSettings.Mode.View))
                {
                    // this.containerWrapperControl.Controls.Add(new LiteralControl("<a name=\"" + module.ModuleID + "\"></a>"));
                }

                // Load container control
                var container = this.LoadModuleContainer(module, portalSettings);

                // Add Container to Dictionary
                pane.Containers.Add(container.ID, container);
            }
            catch (ThreadAbortException)
            {
                // Response.Redirect may called in module control's OnInit method, so it will cause ThreadAbortException, no need any action here.
            }
            catch (Exception exc)
            {
                var lex = new ModuleLoadException(string.Format(Skin.MODULEADD_ERROR, pane.Name), exc);
                /*
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    this.containerWrapperControl.Controls.Add(new ErrorContainer(this.PortalSettings, Skin.MODULELOAD_ERROR, lex).Container);
                }

                Exceptions.LogException(exc);
                */
                throw lex;
            }

            return pane;
        }

        /// <inheritdoc/>
        public PaneModel ProcessPane(PaneModel pane)
        {
            if (Globals.IsLayoutMode())
            {
                /*
                this.PaneControl.Visible = true;

                // display pane border
                string cssclass = this.PaneControl.Attributes["class"];
                if (string.IsNullOrEmpty(cssclass))
                {
                    this.PaneControl.Attributes["class"] = CPaneOutline;
                }
                else
                {
                    this.PaneControl.Attributes["class"] = cssclass.Replace(CPaneOutline, string.Empty).Trim().Replace("  ", " ") + " " + CPaneOutline;
                }

                // display pane name
                var ctlLabel = new Label { Text = "<center>" + this.Name + "</center><br />", CssClass = "SubHead" };
                this.PaneControl.Controls.AddAt(0, ctlLabel);
                */
            }

            /*
            else
            {
                if (this.CanCollapsePane(pane))
                {
                    pane.CssClass += " DNNEmptyPane";
                }

                // Add support for drag and drop
                if (Globals.IsEditMode())
                {
                    pane.CssClass += " dnnSortable";

                }
            }
            */
            return pane;
        }

        private bool CanCollapsePane(PaneModel pane)
        {
            // This section sets the width to "0" on panes that have no modules.
            // This preserves the integrity of the HTML syntax so we don't have to set
            // the visiblity of a pane to false. Setting the visibility of a pane to
            // false where there are colspans and rowspans can render the skin incorrectly.
            var canCollapsePane = true;
            if (pane.Containers.Count > 0)
            {
                canCollapsePane = false;
            }

            return canCollapsePane;
        }

        private bool IsVesionableModule(ModuleInfo moduleInfo)
        {
            if (string.IsNullOrEmpty(moduleInfo.DesktopModule.BusinessControllerClass))
            {
                return false;
            }

            // TODO: Replace with DI-based resolution when business controller classes are migrated.
            var controller = DotNetNuke.Framework.Reflection.CreateObject(moduleInfo.DesktopModule.BusinessControllerClass, string.Empty);
            return controller is IVersionable;
        }

        private ContainerModel LoadContainerFromCookie(HttpRequest request, PortalSettings portalSettings)
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

        private ContainerModel LoadModuleContainer(ModuleInfo module, PortalSettings portalSettings)
        {
            var containerSrc = Null.NullString;

            // var request = this.PaneControl.Page.Request;
            ContainerModel container = null;

            if (portalSettings.EnablePopUps && UrlUtils.InPopUp())
            {
                containerSrc = module.ContainerPath + "popUpContainer.ascx";

                // Check Skin for a popup Container
                if (module.ContainerSrc == portalSettings.ActiveTab.ContainerSrc)
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(containerSrc)))
                    {
                        container = this.LoadContainerByPath(containerSrc, module, portalSettings);
                    }
                }

                // error loading container - load default popup container
                if (container == null)
                {
                    containerSrc = Globals.HostPath + "Containers/_default/popUpContainer.ascx";
                    container = this.LoadContainerByPath(containerSrc, module, portalSettings);
                }
            }
            else
            {
                /*
                container = (this.LoadContainerFromQueryString(module, request) ?? this.LoadContainerFromCookie(request)) ?? this.LoadNoContainer(module);
                if (container == null)
                {
                    // Check Skin for Container
                    var masterModules = this.PortalSettings.ActiveTab.ChildModules;
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
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, portalSettings);
                        container = this.LoadContainerByPath(containerSrc, module, portalSettings);
                    }
                }

                // error loading container - load from tab
                if (container == null)
                {
                    containerSrc = portalSettings.ActiveTab.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, portalSettings);
                        container = this.LoadContainerByPath(containerSrc, module, portalSettings);
                    }
                }

                // error loading container - load default
                if (container == null)
                {
                    containerSrc = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalContainer(), portalSettings);
                    container = this.LoadContainerByPath(containerSrc, module, portalSettings);
                }
            }

            // Set container path
            module.ContainerPath = SkinController.FormatSkinPath(containerSrc);

            // set container id to an explicit short name to reduce page payload
            container.ID = "ctr";

            // make the container id unique for the page
            if (module.ModuleID > -1)
            {
                container.ID += module.ModuleID.ToString();
            }

            container.EditMode = Personalization.GetUserMode() == PortalSettings.Mode.Edit;

            return container;
        }

        private ContainerModel LoadContainerByPath(string containerPath, ModuleInfo module, PortalSettings portalSettings)
        {
            if (containerPath.IndexOf("/skins/", StringComparison.InvariantCultureIgnoreCase) != -1 || containerPath.IndexOf("/skins\\", StringComparison.InvariantCultureIgnoreCase) != -1 || containerPath.IndexOf("\\skins\\", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                containerPath.IndexOf("\\skins/", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                throw new Exception();
            }

            ContainerModel container = null;

            try
            {
                var containerSrc = containerPath;
                if (containerPath.IndexOf(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    containerPath = containerPath.Remove(0, Globals.ApplicationPath.Length);
                }

                // container = ControlUtilities.LoadControl<MvcContainer>(this.PaneControl.Page, containerPath);
                container = this.containerModelFactory.CreateContainerModel(module, portalSettings, containerSrc);

                // container.ContainerSrc = containerSrc;

                // call databind so that any server logic in the container is executed
                // container.DataBind();
            }
            catch (Exception exc)
            {
                // could not load user control
                var lex = new ModuleLoadException(Skin.MODULELOAD_ERROR, exc);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    /*
                    this.containerWrapperControl.Controls.Add(new ErrorContainer(this.PortalSettings, string.Format(Skin.CONTAINERLOAD_ERROR, containerPath), lex).Container);
                    */
                }

                Exceptions.LogException(lex);
            }

            return container;
        }
    }
}
