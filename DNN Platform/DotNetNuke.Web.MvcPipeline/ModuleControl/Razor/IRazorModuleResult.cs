using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.Web.MvcPipeline.Utils;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public interface IRazorModuleResult
    {
        IHtmlString Execute(HtmlHelper htmlHelper);

       
    }
}
