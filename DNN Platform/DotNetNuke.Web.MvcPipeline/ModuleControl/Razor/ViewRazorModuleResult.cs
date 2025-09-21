using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DotNetNuke.Web.MvcPipeline.Utils;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public class ViewRazorModuleResult : IRazorModuleResult
    {
        public ViewRazorModuleResult(string viewName, object model, ViewDataDictionary ViewData)
        {
            this.ViewName = viewName;
            this.Model = model;
            this.ViewData = ViewData ;
        }

        public string ViewName { get; private set; }
        public object Model { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData { get; private set; }

        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return htmlHelper.Partial(ViewName, Model, ViewData);
        }

    }
}
