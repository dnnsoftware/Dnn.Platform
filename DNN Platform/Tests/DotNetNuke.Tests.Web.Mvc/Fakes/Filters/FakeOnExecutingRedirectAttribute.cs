using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    public class FakeOnExecutingRedirectAttribute : FakeRedirectAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Result = Result;
        }
    }
}
