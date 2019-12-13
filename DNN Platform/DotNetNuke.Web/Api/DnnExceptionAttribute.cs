// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.Api
{
    public class DnnExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public string MessageKey { get; set; }

        public string LocalResourceFile { get; set; }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
            {
                var exception = actionExecutedContext.Exception;
                var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
                var controllerName = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;

                var resourceFile = string.IsNullOrEmpty(LocalResourceFile)
                    ? Localization.ExceptionsResourceFile
                    : LocalResourceFile;

                var key = string.IsNullOrEmpty(MessageKey) ? controllerName + "_" + actionName + ".Error" : MessageKey;

                HttpStatusCode statusCode;
                if (exception is NotImplementedException)
                {
                    statusCode = HttpStatusCode.NotImplemented;
                }
                else if (exception is TimeoutException)
                {
                    statusCode = HttpStatusCode.RequestTimeout;
                }
                else if (exception is WebApiException)
                {
                    statusCode = HttpStatusCode.BadRequest;
                }
                else if (exception is UnauthorizedAccessException)
                {
                    statusCode = HttpStatusCode.Unauthorized;
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                }

                var response = new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    ReasonPhrase = Localization.GetString(key, resourceFile)
                };

                actionExecutedContext.Response = response;
                Exceptions.LogException(exception);
            }
        }
    }
}
