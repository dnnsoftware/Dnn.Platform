// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public class DnnUrlHelper
    {
        private readonly ViewContext _viewContext;

        public DnnUrlHelper(ViewContext viewContext)
        {
            Requires.NotNull("viewContext", viewContext); 
            
            _viewContext = viewContext;

            var controller = viewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnUrlHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            ModuleContext = controller.ModuleContext;
        }

        public ModuleInstanceContext ModuleContext { get; set; }

        public virtual string Action()
        {
            return _viewContext.RequestContext.HttpContext.Request.RawUrl;
        }

        public virtual string Action(string actionName)
        {
            return GenerateUrl(actionName, null, new RouteValueDictionary());
        }

        public virtual string Action(string actionName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(actionName, null, routeValues);
        }

        public virtual string Action(string actionName, object routeValues)
        {
            return GenerateUrl(actionName, null, TypeHelper.ObjectToDictionary(routeValues));
        }

        public virtual string Action(string actionName, string controllerName)
        {
            return GenerateUrl(actionName, controllerName, new RouteValueDictionary());
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(actionName, controllerName, routeValues);
        }

        public virtual string Action(string actionName, string controllerName, object routeValues)
        {
            return GenerateUrl(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues));
        }

        private string GenerateUrl(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            routeValues["controller"] = controllerName;
            routeValues["action"] = actionName;
            return ModuleRoutingProvider.Instance().GenerateUrl(routeValues, ModuleContext);
        }
    }
}
