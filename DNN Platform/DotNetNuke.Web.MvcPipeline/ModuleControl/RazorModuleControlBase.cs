using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl
{
    public abstract class RazorModuleControlBase : DefaultMvcModuleControlBase
    {

        public abstract object ViewModel();

        public override IHtmlString Html(HtmlHelper htmlHelper)
        {
            var model = this.ViewModel();
            return htmlHelper.Partial(this.ViewName, model);
        }

        public virtual string ViewName
        {
            get
            {
                return "~/" + this.ControlPath.Replace('\\', '/') + "/Views/" + this.ControlName + ".cshtml";
            }
        }

        public abstract object ViewModel();
    }
}
