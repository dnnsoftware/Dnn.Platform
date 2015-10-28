// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services.Attributes
{
    /// <summary>
    /// Custom exception filter attribute for the services of the Dynamic Content Manager module
    /// </summary>
    public class DccExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// This method overrides the base to change the response status code from
        /// "500 HttpStatusCode.InternalServerError" to "422 HttpStatusCode.UnprocessableEntity"
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var ex = actionExecutedContext.Exception;
            if (ex == null)
                return;

            base.OnException(actionExecutedContext);

            if (ex is TimeoutException)
            {
                actionExecutedContext.Response.StatusCode = HttpStatusCode.RequestTimeout;
            }
            else if (ex is InvalidOperationException)
            { 
                actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse((HttpStatusCode)HttpStatusCodeAdditions.UnprocessableEntity, ex.Message);
            }
            else
            {
                actionExecutedContext.Response.StatusCode = HttpStatusCode.InternalServerError;
                Exceptions.LogException(ex);
            }
        }
    }
}