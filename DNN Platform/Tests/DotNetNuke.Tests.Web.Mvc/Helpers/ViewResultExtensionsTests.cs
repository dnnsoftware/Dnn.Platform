#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    [TestFixture]
    public class DnnViewResultTests
    {
        [Test]
        public void ExecuteResult_Throws_With_Null_ControllerContext()
        {
            //Arrange
            var result = new DnnViewResult();
            
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(null, new StringWriter()));
        }

        [Test]
        public void ExecuteResult_Throws_With_Null_TextWriter()
        {
            //Arrange
            var result = new DnnViewResult();
            var context = MockHelper.CreateMockControllerContext();

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context, null));
        }

        [Test]
        public void ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()
        {
            //Arrange
            var result = new DnnViewResult();
            var context = MockHelper.CreateMockControllerContext();

            //Act, Assert
            Assert.Throws<InvalidOperationException>(() => result.ExecuteResult(context, new StringWriter()));
        }

        [Test]
        public void ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()
        {
            //Arrange
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

            //Act
            result.ExecuteResult(context, new StringWriter());

            //Assert
            Assert.AreSame(mockView.Object, result.View);
        }
    }
}
