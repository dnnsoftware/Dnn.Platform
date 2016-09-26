using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Dnn.EditBar.UI.Helpers;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

namespace Dnn.EditBar.UI.Services
{
    [DnnPageEditor]
    public class CommonController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage IsPrivatePage()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new {IsPrivate = PagesHelper.IsPrivatePage()});
        }
    }
}
