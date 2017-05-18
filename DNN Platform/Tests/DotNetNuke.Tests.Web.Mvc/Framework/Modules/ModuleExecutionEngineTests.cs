#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.IO;
using System.Management.Instrumentation;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using DotNetNuke.Web.Mvc.Framework.Modules;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    [TestFixture]
    public class ModuleExecutionEngineTests
    {
        [Test]
        public void ExecuteModule_Requires_NonNull_ModuleRequestContext()
        {
            var engine = new ModuleExecutionEngine();
            Assert.Throws<ArgumentNullException>(() => engine.ExecuteModule(null));
        }

        [Test]
        public void ExecuteModule_Returns_Null_If_Application_Is_Null()
        {
            // Arrange
            var engine = new ModuleExecutionEngine();
            var requestContext = new ModuleRequestContext() {ModuleApplication = null};

            // Act
            ModuleRequestResult result = engine.ExecuteModule(requestContext);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ExecuteModule_Executes_ModuleApplication_If_Not_Null()
        {
            // Arrange
            var engine = new ModuleExecutionEngine();

            var moduleApp = new Mock<ModuleApplication>();
            var requestContext = new ModuleRequestContext() { ModuleApplication = moduleApp.Object };

            // Act
            engine.ExecuteModule(requestContext);

            // Assert
            moduleApp.Verify(app => app.ExecuteRequest(It.IsAny<ModuleRequestContext>()));
        }

        [Test]
        public void ExecuteModule_Returns_Result_Of_Executing_ModuleApplication()
        {
            // Arrange
            var engine = new ModuleExecutionEngine();
            var expected = new ModuleRequestResult();

            var moduleApp = new Mock<ModuleApplication>();
            moduleApp.Setup(app => app.ExecuteRequest(It.IsAny<ModuleRequestContext>()))
                    .Returns(expected);
            var requestContext = new ModuleRequestContext() { ModuleApplication = moduleApp.Object };

            // Act
            ModuleRequestResult actual = engine.ExecuteModule(requestContext);

            // Assert
            Assert.AreSame(expected, actual);
        }

        [Test]
        public void ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult()
        {
            //Arrange
            var engine = new ModuleExecutionEngine();

            var actionResultMock = new Mock<ActionResult>();

            var viewResultMock = actionResultMock.As<IDnnViewResult>();

            var controllerContext = MockHelper.CreateMockControllerContext();
            var moduleRequestResult = new ModuleRequestResult
                                            {
                                                ActionResult = actionResultMock.Object,
                                                ControllerContext = controllerContext
                                            };

            //Act
            engine.ExecuteModuleResult(moduleRequestResult, new StringWriter());


            //Arrange
            viewResultMock.Verify(v => v.ExecuteResult(It.IsAny<ControllerContext>(), It.IsAny<TextWriter>()));
        }

        [Test]
        public void ExecuteModuleResult_Calls_IDnnViewResult_ExecuteResult_With_ModuleRequestResult_ControllerContext()
        {
            //Arrange
            var engine = new ModuleExecutionEngine();

            var actionResultMock = new Mock<ActionResult>();

            var viewResultMock = actionResultMock.As<IDnnViewResult>();

            var controllerContext = MockHelper.CreateMockControllerContext();
            var moduleRequestResult = new ModuleRequestResult
                                            {
                                                ActionResult = actionResultMock.Object,
                                                ControllerContext = controllerContext
                                            };

            //Act
            engine.ExecuteModuleResult(moduleRequestResult, new StringWriter());


            //Arrange
            viewResultMock.Verify(v => v.ExecuteResult(controllerContext, It.IsAny<TextWriter>()));
        }
    }
}
