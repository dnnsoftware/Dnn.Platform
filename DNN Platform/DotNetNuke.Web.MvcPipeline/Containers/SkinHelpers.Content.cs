// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Containers
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Content(this HtmlHelper<ContainerModel> htmlHelper, PortalSettings portalSettings)
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

            if (!ModuleHostModel.IsViewMode(model.ModuleConfiguration, portalSettings) && htmlHelper.ViewContext.HttpContext.Request.QueryString["dnnprintmode"] != "true")
            {
                MvcJavaScript.RequestRegistration(CommonJs.DnnPlugins);
                if (model.EditMode && model.ModuleConfiguration.ModuleID > 0)
                {
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

            /*
           if (model.ModuleConfiguration.ModuleControl.ControlSrc.StartsWith("DesktopModules/RazorModules"))
           {
               var controlFolder = Path.GetDirectoryName(model.ModuleConfiguration.ModuleControl.ControlSrc);
               var controlFileNameWithoutExtension = Path.GetFileNameWithoutExtension(model.ModuleConfiguration.ModuleControl.ControlSrc);
               var srcPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, controlFolder, "_" + controlFileNameWithoutExtension + ".cshtml");
               var scriptFile = Path.Combine("~/" + controlFolder, "Views/", "_" + controlFileNameWithoutExtension + ".cshtml");
               if (File.Exists(srcPhysicalPath))
               {
                   try
                   {
                       moduleDiv.InnerHtml += htmlHelper.Partial(scriptFile, model.ModuleConfiguration);
                   }
                   catch (Exception ex2)
                   {
                       throw new Exception($"Error : {ex2.Message} ( razor : {scriptFile}, module : {model.ModuleConfiguration.ModuleID})", ex2);
                   }
               }
               else
               {
                   throw new Exception($"Error : Razor file dous not exist ( razor : {scriptFile}, module : {model.ModuleConfiguration.ModuleID})");

                   // moduleDiv.InnerHtml += $"Error : {ex.Message} (Controller : {model.ControllerName}, Action : {model.ActionName}, module : {model.ModuleConfiguration.ModuleTitle}) {ex.StackTrace}";
               }
           }
           */

            var controlFolder = Path.GetDirectoryName(model.ModuleConfiguration.ModuleControl.ControlSrc);
            var controlFileNameWithoutExtension = Path.GetFileNameWithoutExtension(model.ModuleConfiguration.ModuleControl.ControlSrc);
            var srcPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, controlFolder, "Partials", controlFileNameWithoutExtension + ".cshtml");
            if (File.Exists(srcPhysicalPath))
            {
                var scriptFile = Path.Combine("~/" + controlFolder, "Partials", controlFileNameWithoutExtension + ".cshtml").Replace("\\", "/");
                try
                {
                    moduleDiv.InnerHtml += htmlHelper.Partial(scriptFile, model.ModuleConfiguration);
                }
                catch (Exception ex2)
                {
                    throw new Exception($"Error : {ex2.Message} ( razor : {scriptFile}, module : {model.ModuleConfiguration.ModuleID})", ex2);
                }
            }
            else
            {
                moduleDiv.InnerHtml += htmlHelper.Control(model.ModuleConfiguration);
            }

            /*
            try
            {
                // module
                moduleDiv.InnerHtml += htmlHelper.Control(model.ModuleConfiguration);
            }
            catch (HttpException ex)
            {
                var scriptFolder = Path.GetDirectoryName(model.ModuleConfiguration.ModuleControl.ControlSrc);
                var fileRoot = Path.GetFileNameWithoutExtension(model.ModuleConfiguration.ModuleControl.ControlSrc);
                var srcPhysicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptFolder, "_" + fileRoot + ".cshtml");
                var scriptFile = Path.Combine("~/" + scriptFolder, "Views/", "_" + fileRoot + ".cshtml");
                if (File.Exists(srcPhysicalPath))
                {
                    try
                    {
                        moduleDiv.InnerHtml += htmlHelper.Partial(scriptFile, model.ModuleConfiguration);
                    }
                    catch (Exception ex2)
                    {
                        throw new Exception($"Error : {ex2.Message} ( razor : {scriptFile}, module : {model.ModuleConfiguration.ModuleID})", ex2);
                    }
                }
                else
                {
                    // moduleDiv.InnerHtml += $"Error : {ex.Message} (Controller : {model.ControllerName}, Action : {model.ActionName}, module : {model.ModuleConfiguration.ModuleTitle}) {ex.StackTrace}";
                    throw new Exception($"Error : {ex.Message} (Controller : {model.ControllerName}, Action : {model.ActionName}, module : {model.ModuleConfiguration.ModuleID})", ex);
                }
            }
            catch (Exception ex)
            {
                    // moduleDiv.InnerHtml += $"Error : {ex.Message} (Controller : {model.ControllerName}, Action : {model.ActionName}, module : {model.ModuleConfiguration.ModuleTitle}) {ex.StackTrace}";
                    throw new Exception($"Error : {ex.Message} (Controller : {model.ControllerName}, Action : {model.ActionName}, module : {model.ModuleConfiguration.ModuleID})", ex);
            }
            */

            moduleContentPaneDiv.InnerHtml += moduleDiv.ToString();
            if (!string.IsNullOrEmpty(model.Footer))
            {
                moduleContentPaneDiv.InnerHtml += model.Footer;
            }

            return MvcHtmlString.Create(moduleContentPaneDiv.InnerHtml);
        }
    }
}
