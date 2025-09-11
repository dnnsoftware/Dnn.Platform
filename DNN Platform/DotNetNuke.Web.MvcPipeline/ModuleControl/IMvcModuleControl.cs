using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public interface IMvcModuleControl : IModuleControl
    {
        //public ViewContext ViewContext { get; set; }

        /// <summary>Gets the control Html</summary>
        IHtmlString Html(HtmlHelper htmlHelper);

    }
}
