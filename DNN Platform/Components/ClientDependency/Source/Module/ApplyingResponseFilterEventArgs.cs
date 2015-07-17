using System.ComponentModel;
using System.Web;

namespace ClientDependency.Core.Module
{
    public class ApplyingResponseFilterEventArgs : CancelEventArgs
    {
        public HttpContextBase HttpContext { get; private set; }

        public ApplyingResponseFilterEventArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }
    }
}