using DotNetNuke.Common;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace DotNetNuke.Web.Mvc
{
    class DnnMvcControllerFactory : DefaultControllerFactory, IControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            var controller = (IController)Globals.DependencyProvider.GetService(controllerType);
            return controller ?? base.GetControllerInstance(requestContext, controllerType);
        }
    }
}
