using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    public class FakeOnExecutedRedirectAttribute : FakeRedirectAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.Result = Result;
        }
    }
}
