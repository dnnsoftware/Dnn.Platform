using System.Web;
using ClientDependency.Core;

namespace ClientDependency.TypeScript
{
    /// <summary>
    /// An http handler for .coffee extensions which is used when in debug mode
    /// </summary>
    public class TypeScriptHandler : IHttpHandler 
    {
        public void ProcessRequest(HttpContext context)
        {
            var localPath = context.Request.Url.LocalPath;
            if (string.IsNullOrEmpty(localPath)) return;
            var reader = new VirtualPathFileReader();
            var fileContents = reader.ReadFile(localPath);
            var writer = new TypeScriptWriter();
            var engine = writer.GetEngine();
            var output = writer.CompileTypeScript(engine, fileContents);
            context.Response.ContentType = "text/javascript";
            context.Response.Write(output);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
