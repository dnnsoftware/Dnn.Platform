// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.WebAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.Exceptions;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.WebAPI.ActionFilters;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Web.Api;

    /// <summary>A web API controller for <see cref="IPSpec"/>.</summary>
    /// <param name="ipSpecManager">The IP spec manager.</param>
    /// <param name="settingManager">The setting manager.</param>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InBulkInstallWhitelist]
    [MenuPermission(Scope = ServiceScope.Host)]
    public class BulkInstallIPSpecController(IPSpecManager ipSpecManager, SettingManager settingManager) : PersonaBarApiController
    {
        private readonly IPSpecManager ipSpecManager = ipSpecManager;
        private readonly SettingManager settingManager = settingManager;

        /// <summary>Gets all <see cref="IPSpec"/> instances.</summary>
        /// <returns>A request with a list of <see cref="IPSpec"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            List<IPSpec> ipSpecs = this.ipSpecManager.GetAll().ToList();
            var safelist = ipSpecs.Select(ip => new { ip.IPSpecId, ip.Name, ip.Address, });

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Safelist = safelist, });
        }

        /// <summary>Creates a new <see cref="IPSpec"/>.</summary>
        /// <param name="name">The label.</param>
        /// <param name="ip">The IP address.</param>
        /// <returns>A response with the new <see cref="IPSpec"/>.</returns>
        [HttpPost]
        public HttpResponseMessage Create(string name, string ip)
        {
            IPSpec ipSpec;

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

            return this.Request.CreateResponse(HttpStatusCode.Created, new { Ip = ipSpec, });
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

        /// <summary>Enables or disables the IP safelist.</summary>
        /// <returns>A response with a <see cref="bool"/> indicating whether it's enabled or not.</returns>
        [HttpGet]
        public HttpResponseMessage GetIpSafelistConfiguration()
        {
            var enabled = this.settingManager.GetSetting(Settings.IpSafelistGroup, Settings.IpSafelistKey);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Enabled = enabled.ValueAsBoolean, });
        }

        /// <summary>Enables or disables the IP safelist.</summary>
        /// <param name="enabled">Whether to enable the feature.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        public HttpResponseMessage SaveIpSafelistConfiguration(bool enabled)
        {
            this.settingManager.SetSetting(Settings.IpSafelistGroup, Settings.IpSafelistKey, enabled.ToString());
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
