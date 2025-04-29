// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc;

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

public class DnnMvcRouteHandler : IRouteHandler
{
    private readonly IControllerFactory controllerFactory;

    /// <summary>Initializes a new instance of the <see cref="DnnMvcRouteHandler"/> class.</summary>
    public DnnMvcRouteHandler()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DnnMvcRouteHandler"/> class.</summary>
    /// <param name="controllerFactory">The controller factory.</param>
    public DnnMvcRouteHandler(IControllerFactory controllerFactory)
    {
        this.controllerFactory = controllerFactory;
    }

    /// <inheritdoc/>
    IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
    {
        return this.GetHttpHandler(requestContext);
    }

    protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
    {
        requestContext.HttpContext.SetSessionStateBehavior(this.GetSessionStateBehavior(requestContext));
        return new DnnMvcHandler(requestContext);
    }

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
