// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Common
{
    using System.Net;
    using System.Net.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Services.Localization;

    public abstract class ControllerBase : PersonaBarApiController
    {
        protected HttpResponseMessage OkResponse(string msg, object data = null)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, new ResponseModel(false, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage UnauthorizedResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_NotAuthorized", Components.Constants.LocalResourcesFile, true);
            }
            else
            {
                msg += " " + Localization.GetString("Prompt_SessionTimedOut", Components.Constants.LocalResourcesFile, true);
            }
            return this.Request.CreateResponse(HttpStatusCode.Unauthorized, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage BadRequestResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_InvalidData", Components.Constants.LocalResourcesFile, true);
            }
            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage ServerErrorResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_ServerError", Components.Constants.LocalResourcesFile, true);
            }
            return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }

        protected HttpResponseMessage NotImplementedResponse(string msg = "", object data = null)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = Localization.GetString("Prompt_NotImplemented", Components.Constants.LocalResourcesFile, true);
            }
            return this.Request.CreateResponse(HttpStatusCode.NotImplemented, new ResponseModel(true, msg, data?.ToString() ?? ""));
        }
    }
}
