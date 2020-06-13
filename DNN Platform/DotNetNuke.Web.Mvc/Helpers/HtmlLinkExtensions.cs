// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Routing;

    public static class HtmlLinkExtensions
    {
        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, new RouteValueDictionary(), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, object routeValues)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, TypeHelper.ObjectToDictionary(routeValues), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, null, routeValues, htmlAttributes);
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, string controllerName)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            Requires.NotNullOrEmpty("linkText", linkText);

            return MvcHtmlString.Create(GenerateLink(linkText, actionName, controllerName, routeValues, htmlAttributes, htmlHelper.ModuleContext));
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes)
        {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, protocol, hostName, fragment, TypeHelper.ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this DnnHtmlHelper htmlHelper, string linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes)
        {
            Requires.NotNullOrEmpty("linkText", linkText);

            return MvcHtmlString.Create(GenerateLink(linkText, actionName, controllerName, routeValues, htmlAttributes, htmlHelper.ModuleContext));
        }

        private static string GenerateLink(string linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, ModuleInstanceContext moduleContext)
        {
            routeValues["controller"] = controllerName;
            routeValues["action"] = actionName;
            string url = ModuleRoutingProvider.Instance().GenerateUrl(routeValues, moduleContext);

            TagBuilder tagBuilder = new TagBuilder("a")
            {
                InnerHtml = (!string.IsNullOrEmpty(linkText)) ? HttpUtility.HtmlEncode(linkText) : string.Empty,
            };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", url);
            return tagBuilder.ToString(TagRenderMode.Normal);
        }
    }
}
