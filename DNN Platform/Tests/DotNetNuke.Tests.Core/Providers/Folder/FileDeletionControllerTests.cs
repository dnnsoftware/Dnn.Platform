// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

namespace DotNetNuke.Tests.Core.Providers.Folder
{
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
            _mockFileLockingController = new Mock<IFileLockingController>();
            _mockData = MockComponentProvider.CreateDataProvider();
            _fileVersionController = new Mock<IFileVersionController>();
            _folderMappingController = new Mock<IFolderMappingController>();
            _mockFolderProvider = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            _mockContentController = new Mock<IContentController>();
            
            FileLockingController.SetTestableInstance(_mockFileLockingController.Object);
            FileVersionController.RegisterInstance(_fileVersionController.Object);
            FolderMappingController.RegisterInstance(_folderMappingController.Object);
            
            ComponentFactory.RegisterComponentInstance<IContentController>(_mockContentController.Object);                          
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
            //Arrange
            var fileInfo = new FileInfoBuilder().Build();
            _fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            
            _mockData.Setup(md => md.DeleteFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));

            _mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo)).Verifiable();

            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            //Act
            FileDeletionController.Instance.DeleteFile(fileInfo);

            //Assert
            _mockFolderProvider.Verify();
        }

        [Test]
        [ExpectedException(typeof(FileLockedException))]
        public void DeleteFile_Throws_WhenFileIsLocked()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder().Build();
            
            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(true);

            //Act
            FileDeletionController.Instance.DeleteFile(fileInfo);            
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void DeleteFile_Throws_WhenFolderProviderThrows()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder().Build();
            
            _fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            _mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo)).Throws<Exception>();

            
            FileDeletionController.Instance.DeleteFile(fileInfo);
        }

        [Test]
        public void DeleteFileData_Calls_DataProviderDeleteFile()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder().Build();

            _fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            
            _mockData.Setup(md => md.DeleteFile(Constants.CONTENT_ValidPortalId, It.IsAny<string>(), Constants.FOLDER_ValidFolderId)).Verifiable();

            _mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo));

            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            //Act
            FileDeletionController.Instance.DeleteFileData(fileInfo);

            //Assert
            _mockData.Verify();
        }

        [Test]
        public void DeleteFileData_Calls_ContentControllerDeleteContentItem()
        {
            //Arrange
            var fileInfo = new FileInfoBuilder()
                .WithContentItemId(Constants.CONTENT_ValidContentItemId)
                .Build();
            
            _fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockData.Setup(md => md.DeleteFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));

            _mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo));

            _mockContentController.Setup(mcc => mcc.DeleteContentItem(Constants.CONTENT_ValidContentItemId)).Verifiable();

            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            //Act
            FileDeletionController.Instance.DeleteFileData(fileInfo);

            //Assert
            _mockContentController.Verify();
        }
    }
}
