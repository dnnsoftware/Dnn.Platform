// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Exceptions;
    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;

    using DotNetNuke.Web.Api;

    /// <summary>A web API controller for <see cref="Setting"/>.</summary>
    /// <param name="settingManager">The setting manager.</param>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class SettingController(SettingManager settingManager) : DnnApiController
    {
        private readonly SettingManager settingManager = settingManager;

        /// <summary>Gets a <see cref="Setting"/>.</summary>
        /// <param name="group">The group.</param>
        /// <param name="key">The key.</param>
        /// <returns>A response with the setting as the body.</returns>
        [HttpGet]
        public HttpResponseMessage Get(string group, string key)
        {
            Setting setting;

            try
            {
                setting = this.settingManager.GetSetting(group, key);
            }
            catch (SettingNotFoundException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, setting);
        }

        /// <summary>Sets the value of the setting.</summary>
        /// <param name="group">The group.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        public HttpResponseMessage Set(string group, string key, string value)
        {
            try
            {
                this.settingManager.SetSetting(group, key, value);
            }
            catch (Exception ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
