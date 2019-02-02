using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClientDependency.Web.Test.Pages
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpResponse Response = HttpContext.Current.Response;
            Response.Clear();
            Response.Filter = new System.IO.Compression.GZipStream(Response.Filter, System.IO.Compression.CompressionMode.Compress);
            Response.AppendHeader("Content-Encoding", "gzip");
        }
    }
}
