using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public class RazorModuleViewContext
    {
        public HttpContextBase HttpContext { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <remarks>
        /// This is an alias for <c>ViewContext.ViewData</c>.
        /// </remarks>
        public ViewDataDictionary ViewData { get; internal set; }
    }
}
