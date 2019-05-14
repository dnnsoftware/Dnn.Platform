using DotNetNuke.Common;
using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace DotNetNuke.Web.Api
{
    public class DnnHttpControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)Globals.DependencyProvider.GetService(controllerType);
        }
    }
}
