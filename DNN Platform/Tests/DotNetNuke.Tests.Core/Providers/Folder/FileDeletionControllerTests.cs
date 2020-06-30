// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Core.Providers.Builders;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    public class FileDeletionControllerTests
    {
        private Mock<IFileLockingController> _mockFileLockingController;
        private Mock<DataProvider> _mockData;
        private Mock<IFileVersionController> _fileVersionController;
        private Mock<IFolderMappingController> _folderMappingController;
        private Mock<FolderProvider> _mockFolderProvider;
        private Mock<IContentController> _mockContentController;

        [SetUp]
        public void Setup()
        {
            this._mockFileLockingController = new Mock<IFileLockingController>();
            this._mockData = MockComponentProvider.CreateDataProvider();
            this._fileVersionController = new Mock<IFileVersionController>();
            this._folderMappingController = new Mock<IFolderMappingController>();
            this._mockFolderProvider = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            this._mockContentController = new Mock<IContentController>();

            FileLockingController.SetTestableInstance(this._mockFileLockingController.Object);
            FileVersionController.RegisterInstance(this._fileVersionController.Object);
            FolderMappingController.RegisterInstance(this._folderMappingController.Object);

            ComponentFactory.RegisterComponentInstance<IContentController>(this._mockContentController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            FileLockingController.ClearInstance();
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void DeleteFile_Calls_FolderProviderDeleteFile()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();
            this._fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockData.Setup(md => md.DeleteFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));

            this._mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo)).Verifiable();

            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            // Act
            FileDeletionController.Instance.DeleteFile(fileInfo);

            // Assert
            this._mockFolderProvider.Verify();
        }

        [Test]
        [ExpectedException(typeof(FileLockedException))]
        public void DeleteFile_Throws_WhenFileIsLocked()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();

            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(true);

            // Act
            FileDeletionController.Instance.DeleteFile(fileInfo);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void DeleteFile_Throws_WhenFolderProviderThrows()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();

            this._fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            this._mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo)).Throws<Exception>();

            FileDeletionController.Instance.DeleteFile(fileInfo);
        }

        [Test]
        public void DeleteFileData_Calls_DataProviderDeleteFile()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();

            this._fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockData.Setup(md => md.DeleteFile(Constants.CONTENT_ValidPortalId, It.IsAny<string>(), Constants.FOLDER_ValidFolderId)).Verifiable();

            this._mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo));

            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            // Act
            FileDeletionController.Instance.DeleteFileData(fileInfo);

            // Assert
            this._mockData.Verify();
        }

        [Test]
        public void DeleteFileData_Calls_ContentControllerDeleteContentItem()
        {
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithContentItemId(Constants.CONTENT_ValidContentItemId)
                .Build();

            this._fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockData.Setup(md => md.DeleteFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));

            this._mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo));

            this._mockContentController.Setup(mcc => mcc.DeleteContentItem(Constants.CONTENT_ValidContentItemId)).Verifiable();

            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            // Act
            FileDeletionController.Instance.DeleteFileData(fileInfo);

            // Assert
            this._mockContentController.Verify();
        }
    }
}
