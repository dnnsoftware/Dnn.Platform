// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    using System;
    using System.IO;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnViewResultTests
    {
        [Test]
        public void ExecuteResult_Throws_With_Null_ControllerContext()
        {
            // Arrange
            var result = new DnnViewResult();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(null, new StringWriter()));
        }

        [Test]
        public void ExecuteResult_Throws_With_Null_TextWriter()
        {
            // Arrange
            var result = new DnnViewResult();
            var context = MockHelper.CreateMockControllerContext();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context, null));
        }

        [Test]
        public void ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()
        {
            // Arrange
            var result = new DnnViewResult();
            var context = MockHelper.CreateMockControllerContext();

            // Act, Assert
            Assert.Throws<InvalidOperationException>(() => result.ExecuteResult(context, new StringWriter()));
        }

        [Test]
        public void ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()
        {
            // Arrange
            var result = new DnnViewResult();

            var routeData = new RouteData();
            routeData.Values["controller"] = "Controller";
            routeData.Values["action"] = "Action";
            routeData.Values["id"] = "Id";
            var context = MockHelper.CreateMockControllerContext(routeData);

            var mockView = new Mock<IView>();
            var mockViewEngine = new Mock<IViewEngine>();
            mockViewEngine.Setup(v => v.FindView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new ViewEngineResult(mockView.Object, mockViewEngine.Object));
            result.ViewEngineCollection.Clear();
            result.ViewEngineCollection.Add(mockViewEngine.Object);

            // Act
            result.ExecuteResult(context, new StringWriter());

            // Assert
            Assert.AreSame(mockView.Object, result.View);
        }
    }
}
