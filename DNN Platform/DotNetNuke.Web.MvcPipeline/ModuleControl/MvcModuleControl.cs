// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Internal;

    public class MvcModuleControl : DefaultMvcModuleControlBase
    {

        private const string ExcludedQueryStringParams = "tabid,mid,ctl,language,popup,action,controller";
        private const string ExcludedRouteValues = "mid,ctl,popup";

        public override string ControlName
        {
            get
            {
                return RouteActionName;
            }
        }


        /// <summary>Gets or Sets the Path for this control (used primarily for UserControls).</summary>
        /// <returns>A String.</returns>
        public override string ControlPath
        {
            get
            {
                return string.Format("~/DesktopModules/MVC/{0}", ModuleConfiguration.DesktopModule.FolderName);
            }
        }

        /// <summary>Gets or sets the action name for the MVC route.</summary>
        /// <returns>A String.</returns>
        public string RouteActionName
        {
            get
            {
                var segments = GetSegments();
                if (segments.Length < 2)
                {
                    return "";
                }
                if (segments.Length == 3)
                {
                    return segments[2];
                }
                else
                {
                    return segments[1];
                }
            }
        }

        /// <summary>Gets or sets the controller name for the MVC route.</summary>
        /// <returns>A String.</returns>
        public string RouteControllerName
        {
            get
            {
                var segments = GetSegments();
                if (segments.Length < 2)
                {
                    return string.Empty;
                }
                return segments[1];
            }
        }

        public string RouteNamespace
        {
            get
            {
                var segments = GetSegments();
                if (segments.Length < 1)
                {
                    return string.Empty;
                }
                return segments[0];
            }
        }

        public string[] GetSegments()
        {
            return this.ModuleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');
        }

        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            var module = this.ModuleConfiguration;
            var controlSrc = module.ModuleControl.ControlSrc;
            var area = module.DesktopModule.FolderName;
            if (!controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The controlSrc is not a MVC control: " + controlSrc);
            }

            var segments = GetSegments();
            if (segments.Length < 2)
            {
                throw new Exception("The controlSrc is not a MVC control: " + controlSrc);
            }

            string controllerName = string.Empty;
            string actionName = string.Empty;
            var controlKey = module.ModuleControl.ControlKey;


            this.LocalResourceFile = string.Format(
                "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                module.DesktopModule.FolderName,
                Localization.LocalResourceDirectory,
                RouteActionName);

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

            if (moduleId != module.ModuleID && string.IsNullOrEmpty(controlKey))
            {
                // Set default routeData for module that is not the "selected" module
                actionName = RouteActionName;
                controllerName = RouteControllerName;

                // routeData.Values.Add("controller", controllerName);
                // routeData.Values.Add("action", actionName);

                if (!string.IsNullOrEmpty(RouteNamespace))
                {
                    // routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
                }
            }
            else
            {
                var control = ModuleControlController.GetModuleControlByControlKey(controlKey, module.ModuleDefID);
                actionName = queryString.GetValueOrDefault("action", RouteActionName);
                controllerName = queryString.GetValueOrDefault("controller", RouteControllerName);

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

                if (!string.IsNullOrEmpty(RouteNamespace))
                {
                    // routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
                }
            }
            
            string[] routeValues = { $"moduleId={ModuleConfiguration.ModuleID}", $"controller={RouteControllerName}", $"action={RouteActionName}" };

            var request = htmlHelper.ViewContext.HttpContext.Request;
            var req = request.Params;
            var isMyRoute = req["MODULEID"] != null && req["CONTROLLER"] != null && int.TryParse(req["MODULEID"], out var modId) && modId == ModuleConfiguration.ModuleID;

            var url = isMyRoute ? 
                    request.Url.ToString() : 
                    TestableGlobals.Instance.NavigateURL(ModuleConfiguration.TabID, TestableGlobals.Instance.IsHostTab(ModuleConfiguration.TabID), PortalSettings, string.Empty, routeValues);

            var formTag = new TagBuilder("form");
            formTag.Attributes.Add("action", url);
            formTag.Attributes.Add("method", "post");
            formTag.InnerHtml += htmlHelper.AntiForgeryToken().ToHtmlString();

            formTag.InnerHtml += htmlHelper.Action(
                                            actionName,
                                            controllerName,
                                            values);
             return new MvcHtmlString(formTag.ToString());
        }
    }
}
