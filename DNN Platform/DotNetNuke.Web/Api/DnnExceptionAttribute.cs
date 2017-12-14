#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
