using System;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public interface IMvcModuleControl : IModuleControl
    {
        /// <summary>Gets the control Html</summary>
        IHtmlString Html(HtmlHelper htmlHelper);

    }
}
