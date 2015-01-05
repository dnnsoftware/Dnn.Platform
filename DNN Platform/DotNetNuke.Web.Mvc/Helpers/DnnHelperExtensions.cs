#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Mvc.Framework.Modules;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public static class DnnHelperExtensions
    {
        public static string Action(this DnnHelper helper, string actionName)
        {
            var defaultControllerName = helper.SiteContext.ActiveModuleRequest.Application.DefaultControllerName;
            return Action(helper, actionName, defaultControllerName);
        }

        public static string Action(this DnnHelper helper, string actionName, string controllerName)
        {
            const string parentActionName = "Render";
            const string parentControllerName = "Module";

            var moduleId = helper.ActiveModule.ModuleID;

            var routeValues = new RouteValueDictionary
                            {
                                {"moduleId", moduleId},
                                {"moduleRoute", String.Format("{0}/{1}/{2}", controllerName, actionName, moduleId)}
                            };

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);

            return urlHelper.Action(parentActionName, parentControllerName, routeValues);
        }

        public static string Action(this DnnHelper<ModuleRequestResult> helper, bool isModal)
        {
            var defaultActionName = helper.ViewData.Model.Application.DefaultActionName;
            return Action(helper, defaultActionName, isModal);
        }

        public static string Action(this DnnHelper<ModuleRequestResult> helper, string actionName, bool isModal)
        {
            string parentActionName = (isModal) ? "Render" : "Index";
            string parentControllerName = (isModal) ? "Module" : "Page";

            var moduleId = helper.ViewData.Model.Module.ModuleID;
            var defaultControllerName = helper.ViewData.Model.Application.DefaultControllerName;

            var routeValues = new RouteValueDictionary
                            {
                                {"moduleId", moduleId},
                                {"moduleRoute", String.Format("{0}/{1}/{2}", defaultControllerName, actionName, moduleId)}
                            };

            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext, helper.RouteCollection);

            return urlHelper.Action(parentActionName, parentControllerName, routeValues);
        }

        public static MvcHtmlString ActionLink(this DnnHelper<ModuleRequestResult> helper, string linkText)
        {
            return ActionLink(helper, linkText, false);
        }

        public static MvcHtmlString ActionLink(this DnnHelper<ModuleRequestResult> helper, string linkText, bool isModal)
        {
            string parentActionName = (isModal) ? "Render" : "Index";
            string parentControllerName = (isModal) ? "Module" : "Page";

            var moduleId = helper.ViewData.Model.Module.ModuleID;
            var defaultActionName = helper.ViewData.Model.Application.DefaultActionName;
            var defaultControllerName = helper.ViewData.Model.Application.DefaultControllerName;

            var routeValues = new RouteValueDictionary
                            {
                                {"moduleId", moduleId},
                                {"moduleRoute", String.Format("{0}/{1}/{2}", defaultControllerName, defaultActionName, moduleId)}
                            };

            var htmlHelper = new HtmlHelper(helper.ViewContext, helper.ViewDataContainer, helper.RouteCollection);

            return htmlHelper.ActionLink(linkText, parentActionName, parentControllerName, routeValues);
        }

        public static MvcHtmlString ActionLink(DnnHelper helper, ModuleInfo module, string actionName, string controllerName)
        {
            var routeValues = new RouteValueDictionary
                            {
                                {"moduleId", module.ModuleID},
                                {"moduleRoute", "Html/Edit/" + module.ModuleID}
                            };

            var htmlHelper = new HtmlHelper(helper.ViewContext, helper.ViewDataContainer, helper.RouteCollection);

            return htmlHelper.ActionLink(actionName, controllerName, routeValues);
        }
    }
}