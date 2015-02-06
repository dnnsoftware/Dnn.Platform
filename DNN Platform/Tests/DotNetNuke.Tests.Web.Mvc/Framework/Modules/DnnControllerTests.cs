using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Tests.Web.Mvc.Fakes;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    [TestFixture]
    public class DnnControllerTests
    {
        private const string TestViewName = "Foo";

        //[Test]
        //public void ActivePage_Property_Is_Null_If_Nt_Set_In_SiteContext()
        //{
        //    //Arrange
        //    HttpContextBase context = MockHelper.CreateMockHttpContext();
        //    context.SetSiteContext(new SiteContext(context));

        //    //Act
        //    var controller = SetupController(context);

        //    //Assert
        //    Assert.IsNull(controller.ActivePage);
        //}

        private DnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller );
            return controller;
        }
    }
}
