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

    /// <summary>
    /// MVC module control that routes requests to MVC controllers and views based on the control source.
    /// </summary>
    public class MvcModuleControl : DefaultMvcModuleControlBase
    {

        private const string ExcludedQueryStringParams = "tabid,mid,ctl,language,popup,action,controller";
        private const string ExcludedRouteValues = "mid,ctl,popup";

        /// <summary>
        /// Gets the control name, which maps to the MVC action name.
        /// </summary>
        public override string ControlName
        {
            get
            {
                return this.RouteActionName;
            }
        }


        /// <summary>Gets the path for this control (used primarily for user controls).</summary>
        /// <returns>A string representing the control path.</returns>
        public override string ControlPath
        {
            get
            {
                return string.Format("~/DesktopModules/MVC/{0}", this.ModuleConfiguration.DesktopModule.FolderName);
            }
        }

        /// <summary>Gets the action name for the MVC route.</summary>
        /// <returns>A string representing the action name.</returns>
        public string RouteActionName
        {
            get
            {
                var segments = this.GetSegments();
                if (segments.Length < 2)
                {
                    return string.Empty;
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

        /// <summary>Gets the controller name for the MVC route.</summary>
        /// <returns>A string representing the controller name.</returns>
        public string RouteControllerName
        {
            get
            {
                var segments = this.GetSegments();
                if (segments.Length < 2)
                {
                    return string.Empty;
                }
                return segments[1];
            }
        }

        /// <summary>
        /// Gets the namespace that contains the MVC controllers for this module.
        /// </summary>
        public string RouteNamespace
        {
            get
            {
                var segments = this.GetSegments();
                if (segments.Length < 1)
                {
                    return string.Empty;
                }
                return segments[0];
            }
        }

        /// <summary>
        /// Gets the segments that compose the MVC control source.
        /// </summary>
        /// <returns>An array of route segments.</returns>
        public string[] GetSegments()
        {
            return this.ModuleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');
        }

        /// <summary>
        /// Renders the MVC module using the appropriate controller and action.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The rendered HTML.</returns>
        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            var module = this.ModuleConfiguration;
            var controlSrc = module.ModuleControl.ControlSrc;
            var area = module.DesktopModule.FolderName;
            if (!controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The controlSrc is not a MVC control: " + controlSrc);
            }

            var segments = this.GetSegments();
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
                actionName = queryString.GetValueOrDefault("action", this.RouteActionName);
                controllerName = queryString.GetValueOrDefault("controller", this.RouteControllerName);

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

                if (!string.IsNullOrEmpty(this.RouteNamespace))
                {
                    // routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
                }
            }
            
            string[] routeValues =
            {
                $"moduleId={this.ModuleConfiguration.ModuleID}",
                $"controller={this.RouteControllerName}",
                $"action={this.RouteActionName}",
            };

            var request = htmlHelper.ViewContext.HttpContext.Request;
            var req = request.Params;
            var isMyRoute = req["MODULEID"] != null && req["CONTROLLER"] != null && int.TryParse(req["MODULEID"], out var modId) && modId == this.ModuleConfiguration.ModuleID;

            var url = isMyRoute
                ? request.Url.ToString()
                : TestableGlobals.Instance.NavigateURL(this.ModuleConfiguration.TabID, TestableGlobals.Instance.IsHostTab(this.ModuleConfiguration.TabID), this.PortalSettings, string.Empty, routeValues);

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
