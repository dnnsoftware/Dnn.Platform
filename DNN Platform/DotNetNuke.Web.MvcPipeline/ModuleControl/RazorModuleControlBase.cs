using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public abstract class RazorModuleControlBase : DefaultMvcModuleControlBase
    {
        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            var res = this.Invoke();
            return res.Execute(htmlHelper);
        }

        protected virtual string DefaultViewName
        {
            get
            {
                return "~/" + this.ControlPath.Replace('\\', '/') + "/Views/" + this.ControlName + ".cshtml";
            }
        }

        public abstract IRazorModuleResult Invoke();

        public IRazorModuleResult View()
        {
            return View(null);
        }

        public IRazorModuleResult View(string viewName)
        {
            return View(viewName, null);
        }

        public IRazorModuleResult View(object model)
        {
            return View(null, model);
        }
        public IRazorModuleResult View(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName= this.DefaultViewName;
            }
            return new ViewRazorModuleResult(viewName, model);
        }
    }
}
