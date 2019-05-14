using DotNetNuke.Common;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace DotNetNuke.Web.Mvc
{
    /// <summary>
    /// DNN Specific MVC Controller Factory that attempts to use 
    /// Dependency Injection to include all dependencies on the
    /// Controller.
    /// </summary>
    public class DnnMvcControllerFactory : DefaultControllerFactory, IControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            var controller = (IController)Globals.DependencyProvider.GetService(controllerType);
            return controller ?? base.GetControllerInstance(requestContext, controllerType);
        }
    }
}
