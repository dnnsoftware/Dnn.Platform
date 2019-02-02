using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace ClientDependency.Core
{
    //TODO: Create custom handler for web resources since we dont want to have to make HttpRequests for webresource.axd and instead just
    //get the bytes but unfortunately we cannot use the webresource.axd query string parameters since we cannot decrypt the string since
    //in med trust we cannot access the settings required.

    //public class ClientDependencyResourceHandler : IHttpHandler
    //{
    //    public void ProcessRequest(HttpContext context)
    //    {
    //        FormsAuthentication.
    //    }

    //    public bool IsReusable
    //    {
    //        get { return true; }
    //    }
    //}
}
