using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DotNetNuke.Web.MvcPipeline.Utils;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public class ViewRazorModuleResult : IRazorModuleResult
    {
        public ViewRazorModuleResult(string viewName, object model)
        {
            this.ViewName = viewName;
            this.Model = model;
        }

        public string ViewName { get; private set; }
        public object Model { get; private set; }

        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial(ViewName, Model);
        }

    }
}
