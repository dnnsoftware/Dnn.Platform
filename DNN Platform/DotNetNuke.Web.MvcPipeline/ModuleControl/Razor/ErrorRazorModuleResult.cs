using System.Web;
using System.Web.Mvc;
using DotNetNuke.Services.Mail;
using DotNetNuke.Web.MvcPipeline.Modules;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Razor
{
    public class ErrorRazorModuleResult : IRazorModuleResult
    {
        public ErrorRazorModuleResult(string heading, string message)
        {
            this.Heading = heading;
            this.Message = message;
        }

        public string Heading { get; private set; }
        public string Message { get; private set; }

        public IHtmlString Execute(HtmlHelper htmlHelper)
        {
            return htmlHelper.ModuleMessage(this.Message, ModuleMessageType.Error, this.Heading);
        }
    }
}
