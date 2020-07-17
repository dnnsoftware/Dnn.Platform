using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Tabs
{
    [TestFixture]
    public class TabChangeTrackerTests
    {
        [Test]
        public void TrackModuleModification_WithSharedModule_ThrowsException()
        {
            // Arrange
            var tabChangeTracker = new TabChangeTracker();
            var mockedModuleController = new Mock<IModuleController>();
            mockedModuleController
                .Setup(s => s.IsSharedModule(It.IsAny<ModuleInfo>()))
                .Returns(true);
            ServiceLocator<IModuleController, ModuleController>.SetTestableInstance(mockedModuleController.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => tabChangeTracker.TrackModuleModification(null, 1, 0));
            Assert.AreEqual(true, exception.Data?[TabChangeTracker.IsModuleDoesNotBelongToPage]);
        }
    }
}
