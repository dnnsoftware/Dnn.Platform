using DotNetNuke.BulkInstall.Components.DataAccess.Models;
using DotNetNuke.BulkInstall.Components.Exceptions;
using DotNetNuke.BulkInstall.Components.WebAPI.ActionFilters;
using DotNetNuke.Web.Api;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DotNetNuke.BulkInstall.Components.WebAPI
{
    using DotNetNuke.BulkInstall.Components.DataAccess.Models;
    using DotNetNuke.BulkInstall.Components.Exceptions;
    using DotNetNuke.BulkInstall.Components.WebAPI.ActionFilters;

    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class SettingController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(string group, string key)
        {
            Setting setting;

            try
            {
                setting = SettingManager.GetSetting(group, key);
            }
            catch (SettingNotFoundException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, setting);
        }

        [HttpPost]
        public HttpResponseMessage Set(string group, string key, string value)
        {
            try
            {
                SettingManager.SetSetting(group, key, value);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
