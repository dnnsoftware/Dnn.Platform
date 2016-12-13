#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
// by DNN Corporation
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

using System.Web;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Routing
{
    [TestFixture]
    public class HttpContextExtensionsTests
    {
        [Test]
        public void GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()
        {
            //Arrange
            var httpContext = MockHelper.CreateMockHttpContext();
            var expectedResult = new ModuleRequestResult();
            httpContext.Items[HttpContextExtensions.ModuleRequestResultKey] = expectedResult;

            //Act
            var result = httpContext.GetModuleRequestResult();

            //Assert
            Assert.AreSame(expectedResult, result);
        }

        [Test]
        public void GetModuleRequestResult_Returns_Null_If_Not_Present()
        {
            //Arrange
            var httpContext = MockHelper.CreateMockHttpContext();

            //Act
            var result = httpContext.GetModuleRequestResult();

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void HasModuleRequestResult_Returns_True_If_Present()
        {
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext();
            var expectedResult = new ModuleRequestResult();
            httpContext.Items[HttpContextExtensions.ModuleRequestResultKey] = expectedResult;

            //Act and Assert
            Assert.IsTrue(httpContext.HasModuleRequestResult());
        }

        [Test]
        public void HasModuleRequestResult_Returns_False_If_Not_Present()
        {
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext();

            // Act and Assert
            Assert.IsFalse(httpContext.HasModuleRequestResult());
        }

        [Test]
        public void SetModuleRequestResult_Sets_ModuleRequestResult()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var expectedResult = new ModuleRequestResult();

            // Act
            context.SetModuleRequestResult(expectedResult);

            // Assert
            var actual = context.Items[HttpContextExtensions.ModuleRequestResultKey];
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<ModuleRequestResult>(actual);
        }
    }
}
