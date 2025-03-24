using System.Web;
using DotNetNuke.Web.MvcPipeline.Entities.Urls;

namespace DotNetNuke.Web.MvcPipeline.HttpModules
{
    public class MvcUrlRewriteModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            var mvcRewriter = new MvcAdvancedUrlRewriter();
            context.BeginRequest += mvcRewriter.RewriteUrl;
        }
    }
}
