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

using System;
using System.Web.Mvc;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Helpers;
using Moq;
using NUnit.Framework;
// ReSharper disable ObjectCreationAsStatement

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    [TestFixture]
    public class DnnHelperTests
    {
        private Mock<ControllerBase> _mockController;
        private Mock<IViewDataContainer> _mockViewDataContainer ;
        private ViewContext _viewContext;

        [SetUp]
        public void SetUp()
        {
            _mockController = new Mock<ControllerBase>();
            _viewContext = new ViewContext();

            _mockViewDataContainer = new Mock<IViewDataContainer>();
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, _mockViewDataContainer.Object));
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewDataContainer()
        {
            //Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, _mockViewDataContainer.Object));
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
            //Arrange
            _viewContext.Controller = _mockController.Object;

            //Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnHelper(_viewContext, _mockViewDataContainer.Object));
        }

        [Test]
        public void Constructor_Sets_ModuleContext_Property()
        {
            //Arrange
            var expectedContext = new ModuleInstanceContext();
            var mockDnnController = _mockController.As<IDnnController>();
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            _viewContext.Controller = _mockController.Object;

            //Act
            var helper = new DnnHelper(_viewContext, _mockViewDataContainer.Object);

            //Assert
            Assert.AreEqual(expectedContext, helper.ModuleContext);
        }
    }
}
