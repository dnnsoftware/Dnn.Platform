using System.IO;
using System.Web;
using ClientDependency.Core;

namespace ClientDependency.SASS
{
    /// <summary>
    /// An http handler for .coffee extensions which is used when in debug mode
    /// </summary>
    public class SassHandler : IHttpHandler 
    {
        public void ProcessRequest(HttpContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            if (string.IsNullOrEmpty(localPath)) return;
            var file = new FileInfo(context.Server.MapPath(localPath));                
            var writer = new SassWriter();
            var output = writer.GetOutput(file);
            context.Response.ContentType = "text/css";
            context.Response.Write(output);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
