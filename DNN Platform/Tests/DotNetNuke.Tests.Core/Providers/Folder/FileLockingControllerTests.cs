// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Core.Providers.Builders;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileLockingControllerTests
    {
        private Mock<IWorkflowEngine> _mockWorkFlowEngine;
        private Mock<IUserSecurityController> _mockUserSecurityController;

        [SetUp]
        public void Setup()
        {
            this._mockWorkFlowEngine = new Mock<IWorkflowEngine>();
            this._mockUserSecurityController = new Mock<IUserSecurityController>();

            WorkflowEngine.SetTestableInstance(this._mockWorkFlowEngine.Object);
            UserSecurityController.SetTestableInstance(this._mockUserSecurityController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            UserSecurityController.ClearInstance();
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void IsFileLocked_ReturnsTrue_WhenPublishPeriodIsOut()
        {
            // Arrange
            // _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            // _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            // _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            // _fileInfo.Setup(fi => fi.EnablePublishPeriod).Returns(true);
            // _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Today.AddDays(-2));
            // _fileInfo.Setup(fi => fi.EndDate).Returns(DateTime.Today.AddDays(-1));
            // _fileInfo.Setup(fi => fi.ContentItemID).Returns(Null.NullInteger);
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(someReason, "FileLockedOutOfPublishPeriodError");
        }

        [Test]
        public void IsFileLocked_ReturnsTrue_WhenWorkflowIsNotComplete()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .WithContentItemId(It.IsAny<int>())
                .Build();

            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);
            this._mockWorkFlowEngine.Setup(mwc => mwc.IsWorkflowCompleted(It.IsAny<int>())).Returns(false);

            // Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(someReason, "FileLockedRunningWorkflowError");
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenUserIsHostOrAdmin()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(true);

            // Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodIsIn()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodHasNotEndDate()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsTrue_WhenPublishPeriodIsOut()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenUserIsHostOrAdmin()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            // Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsIn()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodHasNotEndDate()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();
            this._mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            // Assert
            Assert.IsFalse(result);
        }
    }
}
