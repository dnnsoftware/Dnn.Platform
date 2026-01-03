// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.SessionState;

    /// <summary>
    /// Route handler that creates <see cref="DnnMvcPageHandler"/> instances for MVC page routes.
    /// </summary>
    public class DnnMvcPageRouteHandler : IRouteHandler
    {
        private readonly IControllerFactory controllerFactory;

        /// <summary>Initializes a new instance of the <see cref="DnnMvcPageRouteHandler"/> class.</summary>
        public DnnMvcPageRouteHandler()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnMvcPageRouteHandler"/> class.</summary>
        /// <param name="controllerFactory">The controller factory.</param>
        public DnnMvcPageRouteHandler(IControllerFactory controllerFactory)
        {
            this.controllerFactory = controllerFactory;
        }

        /// <inheritdoc/>
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            return this.GetHttpHandler(requestContext);
        }

        /// <summary>
        /// Creates the HTTP handler for the given request context.
        /// </summary>
        /// <param name="requestContext">The current request context.</param>
        /// <returns>The HTTP handler.</returns>
        protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            requestContext.HttpContext.SetSessionStateBehavior(this.GetSessionStateBehavior(requestContext));
            return new DnnMvcPageHandler(requestContext);
        }

        /// <summary>
        /// Gets the session state behavior for the controller handling the request.
        /// </summary>
        /// <param name="requestContext">The current request context.</param>
        /// <returns>The desired session state behavior.</returns>
        protected virtual SessionStateBehavior GetSessionStateBehavior(RequestContext requestContext)
        {
            string controllerName = (string)requestContext.RouteData.Values["controller"];
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                throw new InvalidOperationException("No Controller");
            }

            IControllerFactory controllerFactory = this.controllerFactory ?? ControllerBuilder.Current.GetControllerFactory();
            return controllerFactory.GetControllerSessionBehavior(requestContext, controllerName);
        }
    }
}
