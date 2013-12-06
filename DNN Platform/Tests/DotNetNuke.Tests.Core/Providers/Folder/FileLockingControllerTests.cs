#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Tests.Core.Providers.Builders;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class FileLockingControllerTests
    {
        private Mock<IContentWorkflowController> _mockContentWorkflowController;        
        private Mock<IUserSecurityController> _mockUserSecurityController;
        
        [SetUp]
        public void Setup()
        {
            _mockContentWorkflowController = new Mock<IContentWorkflowController>();
            _mockUserSecurityController = new Mock<IUserSecurityController>();

            ContentWorkflowController.RegisterInstance(_mockContentWorkflowController.Object);
            UserSecurityController.SetTestableInstance(_mockUserSecurityController.Object);

        }

        [TearDown]
        public void TearDown()
        {        
            UserSecurityController.ClearInstance(); 
            MockComponentProvider.ResetContainer();
        }

        #region IsFileLocked Method Tests
        [Test]
        public void IsFileLocked_ReturnsTrue_WhenPublishPeriodIsOut()
        {
            //Arrange
            //_fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            //_fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            //_fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            //_fileInfo.Setup(fi => fi.EnablePublishPeriod).Returns(true);
            //_fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Today.AddDays(-2));
            //_fileInfo.Setup(fi => fi.EndDate).Returns(DateTime.Today.AddDays(-1));
            //_fileInfo.Setup(fi => fi.ContentItemID).Returns(Null.NullInteger);
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            //Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(someReason, "FileLockedOutOfPublishPeriodError");
        }

        [Test]
        public void IsFileLocked_ReturnsTrue_WhenWorkflowIsNotComplete()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()                
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .WithContentItemId(It.IsAny<int>())
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);
            _mockContentWorkflowController.Setup(mwc => mwc.IsWorkflowCompleted(It.IsAny<int>())).Returns(false);

            //Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            //Assert
            Assert.IsTrue(result);
            Assert.AreEqual(someReason, "FileLockedRunningWorkflowError");
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenUserIsHostOrAdmin()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(true);

            //Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodIsIn()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()                
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            //Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodHasNotEndDate()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            //Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder().Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            //Act
            string someReason;
            var result = FileLockingController.Instance.IsFileLocked(fileInfo, out someReason);

            //Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region IsFileOutOfPublishPeriod Method Tests
        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsTrue_WhenPublishPeriodIsOut()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);
            
            //Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(),It.IsAny<int>());

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenUserIsHostOrAdmin()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            //Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsIn()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()                
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            //Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodHasNotEndDate()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()                
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            //Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled()
        {
            //Arrange

            var fileInfo = new FileInfoBuilder().Build();
            _mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            //Act
            var result = FileLockingController.Instance.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());

            //Assert
            Assert.IsFalse(result);
        }
        #endregion
    }
}
