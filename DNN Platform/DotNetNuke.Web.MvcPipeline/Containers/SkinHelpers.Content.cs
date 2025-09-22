// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Containers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Content(this HtmlHelper<ContainerModel> htmlHelper)
        {
            var model = htmlHelper.ViewData.Model;
            if (model == null)
            {
                throw new InvalidOperationException("The model need to be present.");
            }

            var moduleContentPaneDiv = new TagBuilder("div");
            if (!string.IsNullOrEmpty(model.ContentPaneCssClass))
            {
                moduleContentPaneDiv.AddCssClass(model.ContentPaneCssClass);
            }

            if (!ModuleHostModel.IsViewMode(model.ModuleConfiguration, model.PortalSettings) && htmlHelper.ViewContext.HttpContext.Request.QueryString["dnnprintmode"] != "true")
            {
                MvcJavaScript.RequestRegistration(CommonJs.DnnPlugins);
                if (model.EditMode && model.ModuleConfiguration.ModuleID > 0)
                {
                    // render module actions
                    moduleContentPaneDiv.InnerHtml += htmlHelper.Control("ModuleActions", model.ModuleConfiguration);
                }

                // register admin.css
                MvcClientResourceManager.RegisterAdminStylesheet(htmlHelper.ViewContext, Globals.HostPath + "admin.css");
            }

            if (!string.IsNullOrEmpty(model.ContentPaneStyle))
            {
                moduleContentPaneDiv.Attributes["style"] = model.ContentPaneStyle;
            }

            if (!string.IsNullOrEmpty(model.Header))
            {
                moduleContentPaneDiv.InnerHtml += model.Header;
            }

            var moduleDiv = new TagBuilder("div");
            moduleDiv.AddCssClass(model.ModuleHost.CssClass);
            // render module control
            try
            {
                moduleDiv.InnerHtml += htmlHelper.Control(model.ModuleConfiguration);
            }
            catch (Exception ex)
            {
                if (model.EditMode)
                {
                    moduleDiv.InnerHtml += "<div class=\"dnnFormMessage dnnFormError\"> Error loading module: " + ex.Message + "</div>";
                }
            }
            moduleContentPaneDiv.InnerHtml += moduleDiv.ToString();
            if (!string.IsNullOrEmpty(model.Footer))
            {
                moduleContentPaneDiv.InnerHtml += model.Footer;
            }

            return MvcHtmlString.Create(moduleContentPaneDiv.InnerHtml);
        }
    }
}
