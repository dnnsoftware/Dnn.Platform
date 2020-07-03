// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Routing;

    public class DnnUrlHelper
    {
        private readonly ViewContext _viewContext;

        private readonly IDnnController _controller;

        public DnnUrlHelper(ViewContext viewContext)
            : this(viewContext, RouteTable.Routes)
        {
        }

        public DnnUrlHelper(RequestContext requestContext, IDnnController controller)
        {
            Requires.NotNull("requestContext", requestContext);
            Requires.NotNull("controller", controller);

            this.UrlHelper = new UrlHelper(requestContext);
            this._controller = controller;
            this.ModuleContext = this._controller.ModuleContext;
        }

        public DnnUrlHelper(ViewContext viewContext, RouteCollection routeCollection)
        {
            Requires.NotNull("viewContext", viewContext);

            this.UrlHelper = new UrlHelper(viewContext.RequestContext, routeCollection);

            this._viewContext = viewContext;

            this._controller = viewContext.Controller as IDnnController;

            if (this._controller == null)
            {
                throw new InvalidOperationException("The DnnUrlHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            this.ModuleContext = this._controller.ModuleContext;
        }

        public ModuleInstanceContext ModuleContext { get; set; }

        internal UrlHelper UrlHelper { get; set; }

        public virtual string Encode(string url)
        {
            return HttpUtility.UrlEncode(url);
        }

        /// <summary>
        /// Converts a virtual (relative) path to an application absolute path.
        /// </summary>
        ///
        /// <returns>
        /// The application absolute path.
        /// </returns>
        /// <param name="contentPath">The virtual path of the content.</param>
        public virtual string Content(string contentPath)
        {
            return this.UrlHelper.Content(contentPath);
        }

        /// <summary>
        /// Returns a value that indicates whether the URL is local.
        /// </summary>
        ///
        /// <returns>
        /// true if the URL is local; otherwise, false.
        /// </returns>
        /// <param name="url">The URL.</param>
        public virtual bool IsLocalUrl(string url)
        {
            return this.UrlHelper.IsLocalUrl(url);
        }

        public virtual string Action()
        {
            return this.UrlHelper.RequestContext.HttpContext.Request.RawUrl;
        }

        public virtual string Action(string actionName)
        {
            return this.GenerateUrl(actionName, null, new RouteValueDictionary());
        }

        public virtual string Action(string actionName, RouteValueDictionary routeValues)
        {
            return this.GenerateUrl(actionName, null, routeValues);
        }

        public virtual string Action(string actionName, object routeValues)
        {
            return this.GenerateUrl(actionName, null, TypeHelper.ObjectToDictionary(routeValues));
        }

        public virtual string Action(string actionName, string controllerName)
        {
            return this.GenerateUrl(actionName, controllerName, new RouteValueDictionary());
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return this.GenerateUrl(actionName, controllerName, routeValues);
        }

        public virtual string Action(string actionName, string controllerName, object routeValues)
        {
            return this.GenerateUrl(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues));
        }

        private string GenerateUrl(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            routeValues["controller"] = controllerName ?? this._controller.ControllerContext?.RouteData.Values["controller"];
            routeValues["action"] = actionName;
            return ModuleRoutingProvider.Instance().GenerateUrl(routeValues, this.ModuleContext);
        }
    }
}
