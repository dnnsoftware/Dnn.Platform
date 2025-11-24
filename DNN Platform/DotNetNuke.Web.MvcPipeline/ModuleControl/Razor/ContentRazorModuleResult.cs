using System.Web;
using System.Web.Mvc;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public class ContentRazorModuleResult : IRazorModuleResult
    {
        public ContentRazorModuleResult(string content)
        {
            this.content = content;
        }

        public string content { get; private set; }

        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(this.content);
        }
    }
}
