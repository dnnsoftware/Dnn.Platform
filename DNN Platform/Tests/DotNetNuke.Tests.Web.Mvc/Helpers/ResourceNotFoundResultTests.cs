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

using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    [TestFixture]
    public class ResourceNotFoundResultTests
    {
        [Test]
        public void DefaultInnerResultFactory_Creates_EmptyResult_If_No_Default_Set()
        {
            ResourceNotFoundResult.DefaultInnerResultFactory = null;
            ResultAssert.IsEmpty(ResourceNotFoundResult.DefaultInnerResultFactory());
        }

        [Test]
        public void DefaultInnerResultFactory_Can_Be_Overridden()
        {
            ResourceNotFoundResult.DefaultInnerResultFactory = () => new HttpUnauthorizedResult();
            ResultAssert.IsUnauthorized(ResourceNotFoundResult.DefaultInnerResultFactory());
            ResourceNotFoundResult.DefaultInnerResultFactory = null;
        }

        [Test]
        public void ExecuteResult_Executes_Default_InnerResult_With_Context_If_No_InnerResult_Provided()
        {
            // Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var mockResult = new Mock<ActionResult>();
            ResourceNotFoundResult.DefaultInnerResultFactory = () => mockResult.Object;
            ResourceNotFoundResult result = new ResourceNotFoundResult();

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResult.Verify(r => r.ExecuteResult(context));
        }

        [Test]
        public void ExecuteResult_Executes_Provided_InnerResult_With_Context_If_No_InnerResult_Provided()
        {
            // Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            ResourceNotFoundResult.DefaultInnerResultFactory = () =>
            {
                Assert.Fail("Expected that the default inner result factory would not be used");
                return null;
            };
            var mockResult = new Mock<ActionResult>();
            ResourceNotFoundResult result = new ResourceNotFoundResult()
            {
                InnerResult = mockResult.Object
            };

            // Act
            result.ExecuteResult(context);

            // Assert
            mockResult.Verify(r => r.ExecuteResult(context));
        }
    }
}
