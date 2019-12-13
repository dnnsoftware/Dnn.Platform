using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Web.Routing;
using DotNetNuke.Tests.Web.Mvc.Fakes.Filters;
using System;

namespace DotNetNuke.Tests.Web.Mvc.Fakes
{
    public class FakeDnnController : DnnController
    {
        public ActionResult Action1()
        {
            return View("Action1");
        }

        public ActionResult Action2()
        {
            return View("Action2", "Master2");
        }

        public ActionResult Action3(Dog dog)
        {
            return View("Action3", "Master3", dog);
        }

        [FakeHandleExceptionRedirect]
        public ActionResult ActionWithExceptionFilter()
        {
            throw new Exception();
        }

        [FakeOnExecutingRedirect]
        public ActionResult ActionWithOnExecutingFilter()
        {
            return View("Action1");
        }

        [FakeOnExecutedRedirect]
        public ActionResult ActionWithOnExecutedFilter()
        {
            return View("Action1");
        }

        public void MockInitialize(RequestContext requestContext)
        {
            // Mocking out the entire MvcHandler and Controller lifecycle proved to be difficult
            // This method executes the initialization logic that occurs on every request which is
            // executed from the Execute method.
            Initialize(requestContext);
        }    
    }
}
