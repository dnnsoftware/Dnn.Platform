// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Licensing.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Application;
    using DotNetNuke.Instrumentation;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class LicensingController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LicensingController));

        /// GET: api/Licensing/GetProduct
        /// <summary>
        /// Gets product info.
        /// </summary>
        /// <param></param>
        /// <returns>product info.</returns>
        [HttpGet]
        public HttpResponseMessage GetProduct()
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        DotNetNukeContext.Current.Application.Name,
                        DotNetNukeContext.Current.Application.SKU,
                        DotNetNukeContext.Current.Application.Description
                    }
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
