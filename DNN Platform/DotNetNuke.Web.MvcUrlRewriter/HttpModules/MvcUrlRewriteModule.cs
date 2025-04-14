using System.Web;
using DotNetNuke.Web.MvcUrlRewriter.Entities.Urls;

namespace DotNetNuke.Web.MvcUrlRewriter.HttpModules
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
