// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.NewDDRMenu
{
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.UI;

    using DotNetNuke.UI;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.NewDDRMenu.TemplateEngine;

    /// <summary>
    /// Provides helper methods for rendering DDRMenu in a skin.
    /// </summary>
    public static class SkinHelpers
    {
        /// <summary>
        /// Renders the DDRMenu with the specified settings.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance that this method extends.</param>
        /// <param name="clientID">The client ID of the menu.</param>
        /// <param name="menuStyle">The style of the menu.</param>
        /// <param name="cssClass">The CSS class to apply to the menu.</param>
        /// <param name="nodeXmlPath">The XML path for the nodes.</param>
        /// <param name="nodeSelector">The selector for the nodes.</param>
        /// <param name="includeContext">Whether to include context.</param>
        /// <param name="includeHidden">Whether to include hidden nodes.</param>
        /// <param name="includeNodes">Nodes to include.</param>
        /// <param name="excludeNodes">Nodes to exclude.</param>
        /// <param name="nodeManipulator">The node manipulator.</param>
        /// <param name="clientOptions">The client options.</param>
        /// <param name="templateArguments">The template arguments.</param>
        /// <returns>An HTML string representing the rendered menu.</returns>
        public static IHtmlString DDRMenu(
                                        this System.Web.Mvc.HtmlHelper<PageModel> htmlHelper,
                                        string clientID,
                                        string menuStyle,
                                        string cssClass = "",
                                        string nodeXmlPath = "",
                                        string nodeSelector = "*",
                                        bool includeContext = false,
                                        bool includeHidden = false,
                                        string includeNodes = "",
                                        string excludeNodes = "",
                                        string nodeManipulator = "",
                                        List<ClientOption> clientOptions = null,
                                        List<TemplateArgument> templateArguments = null)
        {
            MvcMenuBase menu;
            menu = MvcMenuBase.Instantiate(menuStyle, htmlHelper.ViewData.Model.Skin.SkinPath);
            menu.ApplySettings(
                new Settings
                {
                    MenuStyle = menuStyle,
                    NodeXmlPath = nodeXmlPath,
                    NodeSelector = nodeSelector,
                    IncludeContext = includeContext,
                    IncludeHidden = includeHidden,
                    IncludeNodes = includeNodes,
                    ExcludeNodes = excludeNodes,
                    NodeManipulator = nodeManipulator,
                    ClientOptions = clientOptions,
                    TemplateArguments = templateArguments,
                });

            var localiser = new Localiser();
            if (string.IsNullOrEmpty(nodeXmlPath))
            {
                // Use cached nodes if available
                DotNetNuke.UI.WebControls.DNNNodeCollection dnnNodes = htmlHelper.ViewContext.HttpContext.Items["DNNMenuNodes"] as DotNetNuke.UI.WebControls.DNNNodeCollection;
                if (dnnNodes == null)
                {
                    dnnNodes = localiser.LocaliseDNNNodeCollection(
                                Navigation.GetNavigationNodes(
                                    clientID,
                                    Navigation.ToolTipSource.None,
                                    -1,
                                    -1,
                                    DNNAbstract.GetNavNodeOptions(true)));
                    htmlHelper.ViewContext.HttpContext.Items["DNNMenuNodes"] = dnnNodes;
                }

                menu.RootNode =
                    new DDRMenu.MenuNode(
                        dnnNodes);
            }

            menu.PreRender();

            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                menu.Render(writer);
            }

            return MvcHtmlString.Create(stringWriter.ToString());
        }
    }
}
