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
