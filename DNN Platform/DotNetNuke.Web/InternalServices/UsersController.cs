using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    public class UsersController : DnnApiController
    {
        [HttpGet()]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        public HttpResponseMessage Search()
        {
            string field = HttpContext.Current.Request.Params["field"];
            string searchText = HttpContext.Current.Request.Params["searchText"];
            searchText = String.Format("%{0}%", searchText);
            ArrayList res = new ArrayList();
            int total = 0;
            switch (field.ToLower())
            {
                case "username":
                    res = UserController.GetUsersByUserName(ActiveModule.PortalID, searchText, 0,
                                            20, ref total, false, false);
                    break;
                case "email":
                    res = UserController.GetUsersByEmail(ActiveModule.PortalID, searchText, 0,
                         20, ref total, false, false);
                    break;
                case "displayname":
                    res = UserController.GetUsersByDisplayName(ActiveModule.PortalID, searchText, 0,
                         20, ref total, false, false);
                    break;
                default:
                    res = UserController.GetUsersByProfileProperty(ActiveModule.PortalID, field, searchText, 0,
                         20, ref total, false, false);
                    break;
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
    }
}
