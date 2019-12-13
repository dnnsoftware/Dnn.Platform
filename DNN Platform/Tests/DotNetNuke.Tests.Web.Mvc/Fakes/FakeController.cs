using System.Web.Mvc;

namespace DotNetNuke.Tests.Web.Mvc.Fakes
{
    public class FakeController : Controller
    {
        public ActionResult Index()
        {
            return new ViewResult();
        }
    }
}
