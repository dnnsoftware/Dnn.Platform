using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Helpers;

namespace DotNetNuke.Web.Mvc.Framework
{
    public abstract class DnnWebViewPage : WebViewPage
    {
        public DnnHelper<object> Dnn { get; set; }

        public new DnnHtmlHelper<object> Html { get; set; }

        public new DnnUrlHelper Url { get; set; } 

        public override void InitHelpers()
        {
            Ajax = new AjaxHelper<object>(ViewContext, this);
            Html = new DnnHtmlHelper<object>(ViewContext, this);
            Url = new DnnUrlHelper(ViewContext);
            Dnn = new DnnHelper<object>(ViewContext, this);
        }
    }
}