// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline
{
    using System;
    using System.Net.Http;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    // using DotNetNuke.Framework.Models;
    using DotNetNuke.Collections;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using System.Linq;
    using System.Reflection;
    using System.IO;

    public static partial class HtmlHelpers
    {

        private const string ExcludedQueryStringParams = "tabid,mid,ctl,language,popup,action,controller";
        private const string ExcludedRouteValues = "mid,ctl,popup";

        public static IHtmlString ViewComponent(this HtmlHelper htmlHelper, string controllerName, object model)
        {
            return htmlHelper.Action("Invoke", controllerName, model);
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, object model)
        {
            try
            {
                return htmlHelper.Action(
                    "Invoke",
                    MvcUtils.GetControlControllerName(controlSrc),
                    model);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} - {MvcUtils.GetControlControllerName(controlSrc)} - Invoke", ex);
            }
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, ModuleInfo module)
        {
            string controllerName= string.Empty;
            string actionName = string.Empty;
            try
            {
                var area = module.DesktopModule.FolderName;
                if (controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase))
                {
                    var controlKey = module.ModuleControl.ControlKey;
                    var segments = controlSrc.Replace(".mvc", string.Empty).Split('/');

                    var localResourceFile = string.Format(
                        "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                        module.DesktopModule.FolderName,
                        Localization.LocalResourceDirectory,
                        segments[0]);

                    RouteValueDictionary values = new RouteValueDictionary
                    {
                        { "mvcpage", true },
                        { "ModuleId", module.ModuleID },
                        { "TabId", module.TabID },
                        { "ModuleControlId", module.ModuleControlId },
                        { "PanaName", module.PaneName },
                        { "ContainerSrc", module.ContainerSrc },
                        { "ContainerPath", module.ContainerPath },
                        { "IconFile", module.IconFile },
                        { "area", area }
                    };

                    var queryString = htmlHelper.ViewContext.HttpContext.Request.QueryString;

                    if (string.IsNullOrEmpty(controlKey))
                    {
                        controlKey = queryString.GetValueOrDefault("ctl", string.Empty);
                    }

                    var moduleId = Null.NullInteger;
                    if (queryString["moduleid"] != null)
                    {
                        int.TryParse(queryString["moduleid"], out moduleId);
                    }

                    string routeNamespace = string.Empty;
                    string routeControllerName;
                    string routeActionName;
                    if (segments.Length == 3)
                    {
                        routeNamespace = segments[0];
                        routeControllerName = segments[1];
                        routeActionName = segments[2];
                    }
                    else
                    {
                        routeControllerName = segments[0];
                        routeActionName = segments[1];
                    }
                    
                    if (moduleId != module.ModuleID && string.IsNullOrEmpty(controlKey))
                    {
                        // Set default routeData for module that is not the "selected" module
                        actionName = routeActionName;
                        controllerName = routeControllerName;

                        // routeData.Values.Add("controller", controllerName);
                        // routeData.Values.Add("action", actionName);

                        if (!string.IsNullOrEmpty(routeNamespace))
                        {
                            // routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
                        }
                    }
                    else
                    {
                        var control = ModuleControlController.GetModuleControlByControlKey(controlKey, module.ModuleDefID);
                        actionName = queryString.GetValueOrDefault("action", routeActionName);
                        controllerName = queryString.GetValueOrDefault("controller", routeControllerName);

                        // values.Add("controller", controllerName);
                        // values.Add("action", actionName);

                        foreach (var param in queryString.AllKeys)
                        {
                            if (!ExcludedQueryStringParams.Split(',').ToList().Contains(param.ToLowerInvariant()))
                            {
                                if (!values.ContainsKey(param))
                                {
                                    values.Add(param, queryString[param]);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(routeNamespace))
                        {
                            // routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
                        }
                    }
                   
                    return htmlHelper.Action(
                            actionName,
                            controllerName,
                            values);
                }
                else
                {
                    controllerName= MvcUtils.GetControlControllerName(controlSrc);
                    actionName = "Invoke";
                    return htmlHelper.Action(
                        actionName,
                        controllerName,
                        new 
                        {
                            ModuleId = module.ModuleID,
                            TabId = module.TabID,
                            ModuleControlId = module.ModuleControlId,
                            PanaName = module.PaneName,
                            ContainerSrc = module.ContainerSrc,
                            ContainerPath = module.ContainerPath,
                            IconFile = module.IconFile,
                            area= area,
                        });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} - {controlSrc} - {controllerName} - {actionName}", ex);
            }
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, ModuleInfo module)
        {
            return htmlHelper.Control(module.ModuleControl.ControlSrc, module);
        }

        public static IHtmlString CspNonce(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(htmlHelper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
        }

        public static IHtmlString RegisterAjaxScriptIfRequired(this HtmlHelper htmlHelper)
        {
            if (MvcServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                MvcServicesFrameworkInternal.Instance.RegisterAjaxScript(htmlHelper.ViewContext.Controller.ControllerContext);
            }

            return new MvcHtmlString(string.Empty);
        }

        public static IHtmlString AntiForgeryIfRequired(this HtmlHelper htmlHelper)
        {
            // ServicesFramework.Instance.RequestAjaxAntiForgerySupport(); // add also jquery
            if (ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                // var antiForgery = AntiForgery.GetHtml().ToHtmlString();
                return AntiForgery.GetHtml();
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
