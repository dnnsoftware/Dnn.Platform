using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public class RazorModuleViewContext
    {
        public HttpContextBase HttpContext { get; internal set; }
    }
}
