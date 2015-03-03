#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Web.Mvc.Fakes;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    [TestFixture]
    public class DnnHelperTests
    {
        //[Test]
        //public void Constructor_Throws_On_Null_ViewContext()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();

        //    //Act,Assert
        //    Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, mockContainer.Object));
        //}

        //[Test]
        //public void Constructor_Overload_Throws_On_Null_ViewContext()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();

        //    //Act,Assert
        //    Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, mockContainer.Object, new RouteCollection()));
        //}

        //[Test]
        //public void Constructor_Throws_On_Null_DataContainer()
        //{
        //    //Arrange

        //    //Act,Assert
        //    Assert.Throws<ArgumentNullException>(() => new DnnHelper(new ViewContext(), null));
        //}

        //[Test]
        //public void Constructor_Overload_Throws_On_Null_DataContainer()
        //{
        //    //Arrange

        //    //Act,Assert
        //    Assert.Throws<ArgumentNullException>(() => new DnnHelper(new ViewContext(), null, new RouteCollection()));
        //}

        //[Test]
        //public void Constructor_Overload_Throws_On_Null_RouteCollection()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();

        //    //Act,Assert
        //    Assert.Throws<ArgumentNullException>(() => new DnnHelper(new ViewContext(), mockContainer.Object, null));
        //}

        //[Test]
        //public void Constructor_Sets_ViewContext_Property()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    mockContainer.Setup(c => c.ViewData).Returns(new ViewDataDictionary());

        //    var viewContext = new ViewContext();

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ViewContext, viewContext);
        //}

        //[Test]
        //public void Constructor_Sets_ViewDataContainer_Property()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    mockContainer.Setup(c => c.ViewData).Returns(new ViewDataDictionary());

        //    var viewContext = new ViewContext();

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ViewDataContainer, mockContainer.Object);
        //}

        //[Test]
        //public void Constructor_Sets_RouteCollection_Property()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    mockContainer.Setup(c => c.ViewData).Returns(new ViewDataDictionary());

        //    var routes = new RouteCollection();

        //    //Act
        //    var helper = new DnnHelper(new ViewContext(), mockContainer.Object, routes);

        //    //Assert
        //    Assert.AreEqual(helper.RouteCollection, routes);
        //}

        //[Test]
        //public void Constructor_Sets_ViewData_Property()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    var viewData = new ViewDataDictionary();
        //    mockContainer.Setup(c => c.ViewData).Returns(viewData);

        //    var viewContext = new ViewContext();

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ViewData, viewData);
        //}

        //[Test]
        //public void Constructor_Sets_ViewData_Model_Property()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    var dog = new Dog();
        //    var viewData = new ViewDataDictionary(dog);
        //    mockContainer.Setup(c => c.ViewData).Returns(viewData);

        //    var viewContext = new ViewContext();

        //    //Act
        //    var helper = new DnnHelper<Dog>(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ViewData.Model, dog);
        //}

        //[Test]
        //public void SiteContext_Property_Returns_SiteContext()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    var viewData = new ViewDataDictionary();
        //    mockContainer.Setup(c => c.ViewData).Returns(viewData);

        //    HttpContextBase context = MockHelper.CreateMockHttpContext();
        //    var siteContext = new SiteContext(context);
        //    context.SetSiteContext(siteContext);

        //    var viewContext = new ViewContext { HttpContext = context };

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.SiteContext, siteContext);
        //}

        //[Test]
        //public void ActivePage_Property_Returns_ActivePage_From_SiteContext()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    var viewData = new ViewDataDictionary();
        //    mockContainer.Setup(c => c.ViewData).Returns(viewData);

        //    HttpContextBase context = MockHelper.CreateMockHttpContext();
        //    var page = new TabInfo();
        //    var siteContext = new SiteContext(context) {ActivePage = page};
        //    context.SetSiteContext(siteContext);

        //    var viewContext = new ViewContext{ HttpContext = context };

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ActivePage, page);
        //}

        //[Test]
        //public void ActiveSite_Property_Returns_ActiveSite_From_SiteContext()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    var viewData = new ViewDataDictionary();
        //    mockContainer.Setup(c => c.ViewData).Returns(viewData);

        //    HttpContextBase context = MockHelper.CreateMockHttpContext();
        //    var site = new PortalInfo();
        //    var siteContext = new SiteContext(context) { ActiveSite = site };
        //    context.SetSiteContext(siteContext);

        //    var viewContext = new ViewContext { HttpContext = context };

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ActiveSite, site);
        //}

        //[Test]
        //public void ActiveSiteAlias_Property_Returns_ActiveSiteAlias_From_SiteContext()
        //{
        //    //Arrange
        //    var mockContainer = new Mock<IViewDataContainer>();
        //    var viewData = new ViewDataDictionary();
        //    mockContainer.Setup(c => c.ViewData).Returns(viewData);

        //    HttpContextBase context = MockHelper.CreateMockHttpContext();
        //    var siteAlias = new PortalAliasInfo();
        //    var siteContext = new SiteContext(context) { ActiveSiteAlias = siteAlias };
        //    context.SetSiteContext(siteContext);

        //    var viewContext = new ViewContext() { HttpContext = context };

        //    //Act
        //    var helper = new DnnHelper(viewContext, mockContainer.Object, new RouteCollection());

        //    //Assert
        //    Assert.AreEqual(helper.ActiveSiteAlias, siteAlias);
        //}
    }
}
