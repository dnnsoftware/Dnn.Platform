using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public abstract class RazorModuleControlBase : DefaultMvcModuleControlBase
    {
        private RazorModuleViewContext _viewContext;
        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            this.ViewContext.HttpContext = htmlHelper.ViewContext.HttpContext;      
            this.ViewContext.ViewData = new ViewDataDictionary(htmlHelper.ViewData);
            this.ViewContext.ViewData["ModuleContext"] = this.ModuleContext;
            this.ViewContext.ViewData["ModuleId"] = this.ModuleId;
            this.ViewContext.ViewData["LocalResourceFile"] = this.LocalResourceFile;
            var res = this.Invoke();
            return res.Execute(htmlHelper);
        }

        protected virtual string DefaultViewName
        {
            get
            {
                return "~/" + this.ControlPath.Replace('\\', '/').Trim('/') + "/Views/" + this.ControlName + ".cshtml";
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
            return new ViewRazorModuleResult(viewName, model, ViewData);
        }

        public RazorModuleViewContext ViewContext
        {
            get
            {
                // This should run only for the ViewComponent unit test scenarios.
                if (_viewContext == null)
                {
                    _viewContext = new RazorModuleViewContext();
                }

                return _viewContext;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _viewContext = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Http.HttpContext"/>.
        /// </summary>
        public HttpContextBase HttpContext => ViewContext.HttpContext;

        /// <summary>
        /// Gets the <see cref="HttpRequest"/>.
        /// </summary>
        public HttpRequestBase Request => ViewContext.HttpContext.Request;

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData => ViewContext.ViewData;

    }
}
