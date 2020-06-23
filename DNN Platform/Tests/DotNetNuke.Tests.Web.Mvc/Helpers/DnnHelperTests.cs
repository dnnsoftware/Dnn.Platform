// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ObjectCreationAsStatement
namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnHelperTests
    {
        private Mock<ControllerBase> _mockController;
        private Mock<IViewDataContainer> _mockViewDataContainer;
        private ViewContext _viewContext;

        [SetUp]
        public void SetUp()
        {
            this._mockController = new Mock<ControllerBase>();
            this._viewContext = new ViewContext();

            this._mockViewDataContainer = new Mock<IViewDataContainer>();
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, this._mockViewDataContainer.Object));
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewDataContainer()
        {
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(null, this._mockViewDataContainer.Object));
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
            // Arrange
            this._viewContext.Controller = this._mockController.Object;

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnHelper(this._viewContext, this._mockViewDataContainer.Object));
        }

        [Test]
        public void Constructor_Sets_ModuleContext_Property()
        {
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var mockDnnController = this._mockController.As<IDnnController>();
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            this._viewContext.Controller = this._mockController.Object;

            // Act
            var helper = new DnnHelper(this._viewContext, this._mockViewDataContainer.Object);

            // Assert
            Assert.AreEqual(expectedContext, helper.ModuleContext);
        }
    }
}
