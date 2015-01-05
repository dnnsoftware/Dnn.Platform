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
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    [TestFixture]
    public class PageOverrideResultTests
    {
        [Test]
        public void Constructor_Throws_On_Null_ActionResult()
        {
            //Arrange
            ActionResult innerResult = null;

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new PageOverrideResult(innerResult));
        }

        [Test]
        public void Constructor_Sets_InnerResult_On_NonNull_ActionResult()
        {
            //Arrange
            var mockInnerResult = new Mock<ActionResult>();

            //Act
            var result = new PageOverrideResult(mockInnerResult.Object);

            //Assert
            Assert.AreSame(mockInnerResult.Object, result.InnerResult);
        }

        [Test]
        public void ExecuteResult_Throws_On_Null_ControllerContext()
        {
            //Arrange
            ControllerContext context = null;
            var mockInnerResult = new Mock<ActionResult>();

            //Act
            var result = new PageOverrideResult(mockInnerResult.Object);
            
            //Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context));
        }

        [Test]
        public void ExecuteResult_Calls_InnerResults_ExecuteResult_Method()
        {
            //Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var mockInnerResult = new Mock<ActionResult>();

            //Act
            var result = new PageOverrideResult(mockInnerResult.Object);
            result.ExecuteResult(context);

            //Assert
            mockInnerResult.Verify(a => a.ExecuteResult(context));
        }
    }
}
