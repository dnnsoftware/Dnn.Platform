using DotNetNuke.Common;
using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Web.Api
{
    public class DnnHttpControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            // first try to just get it from the DI - if it's there
            return (IHttpController) Globals.DependencyProvider.GetService(controllerType) ??
                   // If it's not found (null), then it's probably a dynamically compiled type from a .cs file or similar
                   // Such types are never registered in the DI catalog, as they may change on-the-fly.
                   // In this case we must use ActivatorUtilities, which will create the object and if it expects 
                   // any DI parameters, they will come from the DependencyInjection as should be best practice
                   (IHttpController) ActivatorUtilities.CreateInstance(Globals.DependencyProvider, controllerType);
        }
    }
}
