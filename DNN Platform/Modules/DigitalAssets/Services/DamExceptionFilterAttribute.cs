// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    using DotNetNuke.Services.Exceptions;

    public class DamExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                Exceptions.LogException(context.Exception);
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, context.Exception.Message, context.Exception);
            }
        }
    }
}
