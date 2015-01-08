using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Web.Mvc.Fakes;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Helpers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    [TestFixture]
    public class DnnControllerTests
    {
        private const string TestViewName = "Foo";

        [Test]
        public void SiteContext_Property_Is_Null_If_Not_Set() 
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.SiteContext);
        }

        [Test]
        public void SiteContext_Property_Is_Set_If_Added_To_HttpContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.SiteContext);
        }

        [Test]
        public void ActiveSite_Property_Is_Null_If_Nt_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.ActiveSite);
        }

        [Test]
        public void ActiveSite_Property_Is_Set_If_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context) { ActiveSite = new PortalInfo()});

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.ActiveSite);
        }

        [Test]
        public void ActiveSiteAlias_Property_Is_Set_If_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context) { ActiveSiteAlias = new PortalAliasInfo() });

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.ActiveSiteAlias);
        }

        [Test]
        public void ActiveSiteAlias_Property_Is_Null_If_Nt_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.ActiveSiteAlias);
        }

        [Test]
        public void ActivePage_Property_Is_Set_If_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context) { ActivePage = new TabInfo() });

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNotNull(controller.ActivePage);
        }

        [Test]
        public void ActivePage_Property_Is_Null_If_Nt_Set_In_SiteContext()
        {
            //Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.SetSiteContext(new SiteContext(context));

            //Act
            var controller = SetupController(context);

            //Assert
            Assert.IsNull(controller.ActivePage);
        }

        private DnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller );
            return controller;
        }
    }
}
