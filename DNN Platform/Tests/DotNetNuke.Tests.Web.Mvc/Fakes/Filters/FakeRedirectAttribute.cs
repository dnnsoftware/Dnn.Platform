using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    public abstract class FakeRedirectAttribute : ActionFilterAttribute
    {
        public static ActionResult Result = new EmptyResult();
    }
}
