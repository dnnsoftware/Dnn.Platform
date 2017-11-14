using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    public class FakeHandleExceptionRedirectAttribute : FakeRedirectAttribute, IExceptionFilter
    {        
        public static bool IsExceptionHandled { get; set; }
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = Result;
            filterContext.ExceptionHandled = IsExceptionHandled;
        }
    }
}
