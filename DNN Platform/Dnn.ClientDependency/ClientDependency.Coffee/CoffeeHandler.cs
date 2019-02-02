using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ClientDependency.Core;

namespace ClientDependency.Coffee
{
    /// <summary>
    /// An http handler for .coffee extensions which is used when in debug mode
    /// </summary>
    public class CoffeeHandler : IHttpHandler 
    {
        public void ProcessRequest(HttpContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            if (string.IsNullOrEmpty(localPath)) return;
            var reader = new VirtualPathFileReader();
            var fileContents = reader.ReadFile(localPath);
            var writer = new CoffeeWriter();
            var output = writer.GetOutput(fileContents);
            context.Response.ContentType = "text/javascript";
            context.Response.Write(output);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
