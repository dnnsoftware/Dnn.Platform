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
using DotNetNuke.ComponentModel;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using DotNetNuke.Web.Mvc.Framework.Modules;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    [TestFixture]
    public class RenderModuleResultTests
    {
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container= new SimpleContainer();  
        }

        [Test]
        public void ExecuteResult_Throws_On_Null_Context()
        {
            //Arrange
            ControllerContext context = null;
            var result= new RenderModuleResult();

            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context));
        }

        [Test]
        public void ExecuteResult_Does_Not_Call_ModuleExecutionEngine_On_Null_ModuleRequestResult()
        {
            //Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var result = new RenderModuleResult();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            //Act
            result.ExecuteResult(context);

            //Assert
            mockEngine.Verify(e => e.ExecuteModuleResult(It.IsAny<SiteContext>(), It.IsAny<ModuleRequestResult>()), Times.Never);
        }

        [Test]
        public void ExecuteResult_Calls_ModuleExecutionEngine_On_ModuleRequestResult()
        {
            //Arrange
            ControllerContext context = MockHelper.CreateMockControllerContext();
            var result = new RenderModuleResult();
            result.ModuleRequestResult = new ModuleRequestResult();

            var mockEngine = new Mock<IModuleExecutionEngine>();
            ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(mockEngine.Object);

            //Act
            result.ExecuteResult(context);

            //Assert
            mockEngine.Verify(e => e.ExecuteModuleResult(It.IsAny<SiteContext>(), It.IsAny<ModuleRequestResult>()), Times.Once);
        }
    }
}
