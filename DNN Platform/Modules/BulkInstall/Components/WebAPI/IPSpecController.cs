// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Exceptions;
    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;

    using DotNetNuke.Web.Api;

    /// <summary>A web API controller for <see cref="IPSpec"/>.</summary>
    /// <param name="ipSpecManager">The IP spec manager.</param>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class IPSpecController(IPSpecManager ipSpecManager) : DnnApiController
    {
        private readonly IPSpecManager ipSpecManager = ipSpecManager;

        /// <summary>Gets all <see cref="IPSpec"/> instances.</summary>
        /// <returns>A request with a list of <see cref="IPSpec"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            List<IPSpec> ipSpecs = this.ipSpecManager.GetAll().ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, ipSpecs);
        }

        /// <summary>Creates a new <see cref="IPSpec"/>.</summary>
        /// <param name="name">The label.</param>
        /// <param name="ip">The IP address.</param>
        /// <returns>A response with the new <see cref="IPSpec"/>.</returns>
        [HttpPost]
        public HttpResponseMessage Create(string name, string ip)
        {
            IPSpec ipSpec = null;

            try
            {
                 ipSpec = this.ipSpecManager.Create(name, ip);
            }
            catch (IPSpecExistsException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message);
            }
            catch (Exception ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.Created, ipSpec);
        }

        /// <summary>Deletes an <see cref="IPSpec"/>.</summary>
        /// <param name="id">The IP spec ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            IPSpec ipSpec = this.ipSpecManager.GetById(id);

            if (ipSpec == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "IP spec not found.");
            }

            try
            {
                this.ipSpecManager.Delete(ipSpec);
            }
            catch (Exception)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to delete IP spec.");
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
