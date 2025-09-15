// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Tests.Core.Providers.Builders;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [TestFixture]
    public class FolderManagerTests
    {
        private FolderManager folderManager;
        private Mock<FolderProvider> mockFolder;
        private Mock<DataProvider> mockData;
        private Mock<FolderManager> mockFolderManager;
        private Mock<IFolderInfo> folderInfo;
        private Mock<IFolderMappingController> folderMappingController;
        private Mock<IDirectory> directory;
        private Mock<IFile> file;
        private Mock<ICBO> cbo;
        private Mock<IPathUtils> pathUtils;
        private Mock<IUserSecurityController> mockUserSecurityController;
        private Mock<IFileDeletionController> mockFileDeletionController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            this.mockData = MockComponentProvider.CreateDataProvider();

            this.folderMappingController = new Mock<IFolderMappingController>();
            this.directory = new Mock<IDirectory>();
            this.file = new Mock<IFile>();
            this.cbo = new Mock<ICBO>();
            this.pathUtils = new Mock<IPathUtils>();
            this.mockUserSecurityController = new Mock<IUserSecurityController>();
            this.mockFileDeletionController = new Mock<IFileDeletionController>();

            FolderMappingController.RegisterInstance(this.folderMappingController.Object);
            DirectoryWrapper.RegisterInstance(this.directory.Object);
            FileWrapper.RegisterInstance(this.file.Object);
            CBO.SetTestableInstance(this.cbo.Object);
            PathUtils.RegisterInstance(this.pathUtils.Object);
            UserSecurityController.SetTestableInstance(this.mockUserSecurityController.Object);
            FileDeletionController.SetTestableInstance(this.mockFileDeletionController.Object);

            this.mockFolderManager = new Mock<FolderManager> { CallBase = true };

            this.folderManager = new FolderManager();

            this.folderInfo = new Mock<IFolderInfo>();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockFolder.Object);
                    services.AddSingleton(this.mockData.Object);
                    services.AddSingleton(this.folderMappingController.Object);
                    services.AddSingleton(this.cbo.Object);
                    services.AddSingleton(this.pathUtils.Object);
                    services.AddSingleton(this.mockUserSecurityController.Object);
                    services.AddSingleton(this.mockFileDeletionController.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            UserSecurityController.ClearInstance();
            FileLockingController.ClearInstance();
            MockComponentProvider.ResetContainer();
            CBO.ClearInstance();
            FileDeletionController.ClearInstance();
            MockComponentProvider.ResetContainer();
            this.serviceProvider.Dispose();
        }

        [Test]
        public void AddFolder_Throws_On_Null_FolderPath()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.AddFolder(It.IsAny<FolderMappingInfo>(), null));
        }

        // [Test]
        // public void AddFolder_Calls_FolderProvider_AddFolder()
        // {
        //    _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
        //    _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
        //    _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

        // var folderMapping = new FolderMappingInfo
        //                            {
        //                                FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                FolderProviderType = Constants.FOLDER_ValidFolderProviderType,
        //                                PortalID = Constants.CONTENT_ValidPortalId
        //                            };

        // _mockFolder.Setup(mf => mf.AddFolder(Constants.FOLDER_ValidSubFolderRelativePath, folderMapping)).Verifiable();

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(Constants.FOLDER_ValidSubFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(false);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidSubFolderPath));
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath, Constants.FOLDER_ValidFolderMappingID));

        // _mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath);

        // _mockFolder.Verify();
        // }

        // [Test]
        // [ExpectedException(typeof(FolderProviderException))]
        // public void AddFolder_Throws_When_FolderProvider_Throws()
        // {
        //    var folderMapping = new FolderMappingInfo
        //                            {
        //                                PortalID = Constants.CONTENT_ValidPortalId,
        //                                FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                FolderProviderType = Constants.FOLDER_ValidFolderProviderType
        //                            };

        // _mockFolderManager.Setup(mfm => mfm.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(false);
        //    _mockFolder.Setup(mf => mf.AddFolder(Constants.FOLDER_ValidSubFolderRelativePath, folderMapping)).Throws<Exception>();

        // _mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath);
        // }

        // [Test]
        // public void AddFolder_Calls_FolderManager_CreateFolderInFileSystem_And_CreateFolderInDatabase_If_Folder_Does_Not_Exist()
        // {
        //    _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
        //    _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
        //    _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

        // var folderMapping = new FolderMappingInfo
        //                            {
        //                                FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                FolderProviderType = Constants.FOLDER_ValidFolderProviderType,
        //                                PortalID = Constants.CONTENT_ValidPortalId
        //                            };

        // _mockFolder.Setup(mf => mf.AddFolder(Constants.FOLDER_ValidSubFolderRelativePath, folderMapping));

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(Constants.FOLDER_ValidSubFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(false);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidSubFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath, Constants.FOLDER_ValidFolderMappingID)).Verifiable();

        // _mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath);

        // _mockFolderManager.Verify();
        // }
        [Test]
        public void AddFolder_Throws_When_Folder_Already_Exists()
        {
            var folderMapping = new FolderMappingInfo
            {
                PortalID = Constants.CONTENT_ValidPortalId,
            };

            this.mockFolderManager.Setup(mfm => mfm.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderRelativePath)).Returns(true);

            Assert.Throws<FolderAlreadyExistsException>(() => this.mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath));
        }

        [Test]
        public void AddFolder_Throws_When_FolderPath_Is_Invalid()
        {
            // arrange
            var folderMapping = new FolderMappingInfo
            {
                PortalID = Constants.CONTENT_ValidPortalId,
            };

            this.mockFolderManager
                .Setup(mfm => mfm.FolderExists(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(false);

            this.mockFolderManager
                .Setup(mfm => mfm.IsValidFolderPath(It.IsAny<string>()))
                .Returns(false);

            // act
            Assert.Throws<InvalidFolderPathException>(() => this.mockFolderManager.Object.AddFolder(folderMapping, Constants.FOLDER_ValidSubFolderRelativePath));

            // assert (implicit)
        }

        [Test]
        public void IsValidFolderPath_Returns_True_When_FolderPath_Is_Valid()
        {
            // arrange (implicit)

            // act
            var result = this.mockFolderManager.Object.IsValidFolderPath(Constants.FOLDER_ValidSubFolderRelativePath);

            // assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValidFolderPath_Returns_False_When_FolderPath_Is_Invalid()
        {
            // arrange (implicit)

            // act
            var result = this.mockFolderManager.Object.IsValidFolderPath(Constants.FOLDER_InvalidSubFolderRelativePath);

            // assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void DeleteFolder_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.DeleteFolder(null));
        }

        [Test]
        public void DeleteFolder_Throws_OnNullFolder_WhenRecursive()
        {
            // Arrange
            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            // Act
            var notDeletedSubfolders = new List<IFolderInfo>();
            Assert.Throws<ArgumentNullException>(() => this.folderManager.DeleteFolder(null, notDeletedSubfolders));
        }

        [Test]
        public void DeleteFolder_CallsFolderProviderDeleteFolder_WhenRecursive()
        {
            // Arrange
            var folderInfo = new FolderInfoBuilder()
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath)
                .Build();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(folderInfo)).Verifiable();

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));
            this.mockFolderManager.Setup(mfm => mfm.GetFolders(folderInfo)).Returns(new List<IFolderInfo>());
            this.mockFolderManager.Setup(mfm => mfm.GetFiles(folderInfo, It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());

            this.mockUserSecurityController.Setup(musc => musc.HasFolderPermission(folderInfo, "DELETE")).Returns(true);

            // Act
            var subfoldersNotDeleted = new List<IFolderInfo>();
            this.mockFolderManager.Object.DeleteFolder(folderInfo, subfoldersNotDeleted);

            // Assert
            this.mockFolder.Verify();
            Assert.That(subfoldersNotDeleted, Is.Empty);
        }

        [Test]
        public void DeleteFolder_CallsFolderProviderDeleteFolder_WhenRecursive_WhenExistSubfolders()
        {
            // Arrange
            var folderInfo = new FolderInfoBuilder()
                .WithFolderId(1)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath)
                .Build();

            var subfolder1 = new FolderInfoBuilder()
                .WithFolderId(2)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath + "\\subfolder1")
                .Build();
            var subfolder2 = new FolderInfoBuilder()
                .WithFolderId(3)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath + "\\subfolder2")
                .Build();
            var subfolders = new List<IFolderInfo>
                {
                    subfolder1,
                    subfolder2,
                };

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(folderInfo)).Verifiable();
            this.mockFolder.Setup(mf => mf.DeleteFolder(subfolder1)).Verifiable();
            this.mockFolder.Setup(mf => mf.DeleteFolder(subfolder2)).Verifiable();

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));
            this.mockFolderManager.Setup(mfm => mfm.GetFolders(folderInfo)).Returns(subfolders);
            this.mockFolderManager.Setup(mfm => mfm.GetFolders(It.IsNotIn(folderInfo))).Returns(new List<IFolderInfo>());

            this.mockFolderManager.Setup(mfm => mfm.GetFiles(It.IsAny<IFolderInfo>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());

            this.mockUserSecurityController.Setup(musc => musc.HasFolderPermission(It.IsAny<IFolderInfo>(), "DELETE")).Returns(true);

            // Act
            var subfoldersNotDeleted = new List<IFolderInfo>();
            this.mockFolderManager.Object.DeleteFolder(folderInfo, subfoldersNotDeleted);

            // Assert
            this.mockFolder.Verify();
            Assert.That(subfoldersNotDeleted, Is.Empty);
        }

        [Test]
        public void DeleteFolder_SubFoldersCollectionIsNotEmpty_WhenRecursive_WhenUserHasNotDeletePermission()
        {
            // Arrange
            var folderInfo = new FolderInfoBuilder()
                .WithFolderId(1)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath)
                .Build();

            var subfolder1 = new FolderInfoBuilder()
                .WithFolderId(2)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath + "\\subfolder1")
                .Build();
            var subfolder2 = new FolderInfoBuilder()
                .WithFolderId(3)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath + "\\subfolder2")
                .Build();
            var subfolders = new List<IFolderInfo>
                {
                    subfolder1,
                    subfolder2,
                };

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(subfolder1));

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));
            this.mockFolderManager.Setup(mfm => mfm.GetFolders(folderInfo)).Returns(subfolders);
            this.mockFolderManager.Setup(mfm => mfm.GetFolders(It.IsNotIn(folderInfo))).Returns(new List<IFolderInfo>());

            this.mockFolderManager.Setup(mfm => mfm.GetFiles(It.IsAny<IFolderInfo>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());

            this.mockUserSecurityController.Setup(musc => musc.HasFolderPermission(subfolder2, "DELETE")).Returns(false);
            this.mockUserSecurityController.Setup(musc => musc.HasFolderPermission(It.IsNotIn(subfolder2), "DELETE")).Returns(true);

            // Act
            var subfoldersNotDeleted = new List<IFolderInfo>();
            this.mockFolderManager.Object.DeleteFolder(folderInfo, subfoldersNotDeleted);

            // Assert
            Assert.That(subfoldersNotDeleted, Has.Count.EqualTo(2)); // folderInfo and subfolder2 are not deleted
        }

        [Test]
        public void DeleteFolder_Throws_OnFileDeletionControllerThrows_WhenRecursive_WhenFileIsLocked()
        {
            // Arrange
            var folderInfo = new FolderInfoBuilder()
                .WithFolderId(1)
                .WithPhysicalPath(Constants.FOLDER_ValidFolderPath)
                .Build();

            var fileInfo1 = new FileInfoBuilder()
                .WithFileId(1)
                .Build();
            var fileInfo2 = new FileInfoBuilder()
                .WithFileId(2)
                .Build();
            var files = new List<IFileInfo>
                {
                    fileInfo1,
                    fileInfo2,
                };

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            // _mockFolder.Setup(mf => mf.DeleteFolder(folderInfo));
            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));
            this.mockFolderManager.Setup(mfm => mfm.GetFolders(folderInfo)).Returns(new List<IFolderInfo>());

            this.mockFolderManager.Setup(mfm => mfm.GetFiles(folderInfo, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files);

            this.mockUserSecurityController.Setup(musc => musc.HasFolderPermission(It.IsAny<IFolderInfo>(), "DELETE")).Returns(true);

            this.mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(fileInfo1));
            this.mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(fileInfo2)).Throws<FileLockedException>();

            // Act
            Assert.Throws<FileLockedException>(() => this.mockFolderManager.Object.DeleteFolder(folderInfo, new List<IFolderInfo>()));
        }

        [Test]
        public void DeleteFolder_Calls_FolderProvider_DeleteFolder()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(this.folderInfo.Object)).Verifiable();

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));

            this.mockFolderManager.Object.DeleteFolder(this.folderInfo.Object);

            this.mockFolder.Verify();
        }

        [Test]
        public void DeleteFolder_Throws_When_FolderProvider_Throws()
        {
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(this.folderInfo.Object)).Throws<Exception>();

            Assert.Throws<FolderProviderException>(() => this.mockFolderManager.Object.DeleteFolder(this.folderInfo.Object));
        }

        [Test]
        public void DeleteFolder_Calls_Directory_Delete_When_Directory_Exists()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(this.folderInfo.Object));

            this.directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);
            this.directory.Setup(d => d.Delete(Constants.FOLDER_ValidFolderPath, true)).Verifiable();

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));

            this.mockFolderManager.Object.DeleteFolder(this.folderInfo.Object);

            this.directory.Verify();
        }

        [Test]
        public void DeleteFolder_Calls_FolderManager_DeleteFolder_Overload()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.DeleteFolder(this.folderInfo.Object));

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

            this.mockFolderManager.Object.DeleteFolder(this.folderInfo.Object);

            this.mockFolderManager.Verify();
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.FolderExists(Constants.CONTENT_ValidPortalId, null));
        }

        [Test]
        public void ExistsFolder_Calls_FolderManager_GetFolder()
        {
            this.mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this.folderInfo.Object).Verifiable();

            this.mockFolderManager.Object.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            this.mockFolderManager.Verify();
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
            this.mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this.folderInfo.Object);

            var result = this.mockFolderManager.Object.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
            this.mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns<IFolderInfo>(null);

            var result = this.mockFolderManager.Object.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.That(result, Is.False);
        }

        [Test]
        public void GetFilesByFolder_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.GetFiles(null));
        }

        [Test]
        public void GetFilesByFolder_Calls_DataProvider_GetFiles()
        {
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var files = new DataTable();
            files.Columns.Add("FolderName");

            var dr = files.CreateDataReader();

            this.mockData.Setup(md => md.GetFiles(Constants.FOLDER_ValidFolderId, It.IsAny<bool>(), It.IsAny<bool>())).Returns(dr).Verifiable();

            var filesList = new List<FileInfo> { new FileInfo() { FileName = Constants.FOLDER_ValidFileName } };

            this.cbo.Setup(cbo => cbo.FillCollection<FileInfo>(dr)).Returns(filesList);

            this.folderManager.GetFiles(this.folderInfo.Object);

            this.mockData.Verify();
        }

        [Test]
        public void GetFilesByFolder_Count_Equals_DataProvider_GetFiles_Count()
        {
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var files = new DataTable();
            files.Columns.Add("FileName");
            files.Rows.Add(Constants.FOLDER_ValidFileName);

            var dr = files.CreateDataReader();

            this.mockData.Setup(md => md.GetFiles(Constants.FOLDER_ValidFolderId, It.IsAny<bool>(), It.IsAny<bool>())).Returns(dr);

            var filesList = new List<FileInfo> { new FileInfo { FileName = Constants.FOLDER_ValidFileName } };

            this.cbo.Setup(cbo => cbo.FillCollection<FileInfo>(dr)).Returns(filesList);

            var result = this.folderManager.GetFiles(this.folderInfo.Object).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public void GetFilesByFolder_Returns_Valid_FileNames_When_Folder_Contains_Files()
        {
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var files = new DataTable();
            files.Columns.Add("FileName");
            files.Rows.Add(Constants.FOLDER_ValidFileName);
            files.Rows.Add(Constants.FOLDER_OtherValidFileName);

            var dr = files.CreateDataReader();

            this.mockData.Setup(md => md.GetFiles(Constants.FOLDER_ValidFolderId, It.IsAny<bool>(), It.IsAny<bool>())).Returns(dr);

            var filesList = new List<FileInfo>
                                {
                                    new FileInfo { FileName = Constants.FOLDER_ValidFileName },
                                    new FileInfo { FileName = Constants.FOLDER_OtherValidFileName },
                                };

            this.cbo.Setup(cbo => cbo.FillCollection<FileInfo>(dr)).Returns(filesList);

            var result = this.folderManager.GetFiles(this.folderInfo.Object).Cast<FileInfo>();

            Assert.That(result, Is.EqualTo(filesList).AsCollection);
        }

        [Test]
        public void GetFolder_Calls_DataProvider_GetFolder()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            this.mockData.Setup(md => md.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(folderDataTable.CreateDataReader()).Verifiable();

            this.folderManager.GetFolder(Constants.FOLDER_ValidFolderId);

            this.mockData.Verify();
        }

        [Test]
        public void GetFolder_Returns_Null_When_Folder_Does_Not_Exist()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            var dr = folderDataTable.CreateDataReader();

            this.mockData.Setup(md => md.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(dr);
            this.cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns<FolderInfo>(null);

            var result = this.folderManager.GetFolder(Constants.FOLDER_ValidFolderId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetFolder_Returns_Valid_Folder_When_Folder_Exists()
        {
            this.folderInfo.Setup(fi => fi.FolderName).Returns(Constants.FOLDER_ValidFolderName);

            this.pathUtils.Setup(pu => pu.RemoveTrailingSlash(Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderName);

            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");
            folderDataTable.Rows.Add(Constants.FOLDER_ValidFolderName);

            var dr = folderDataTable.CreateDataReader();

            this.mockData.Setup(md => md.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(dr);

            var folderInfo = new FolderInfo { FolderPath = Constants.FOLDER_ValidFolderRelativePath };

            this.cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns(folderInfo);

            var result = this.mockFolderManager.Object.GetFolder(Constants.FOLDER_ValidFolderId);

            Assert.That(result.FolderName, Is.EqualTo(Constants.FOLDER_ValidFolderName));
        }

        [Test]
        public void GetFolder_Throws_On_Null_FolderPath()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.GetFolder(It.IsAny<int>(), null));
        }

        [Test]
        public void GetFolder_Calls_GetFolders()
        {
            var foldersSorted = new List<IFolderInfo>();

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(foldersSorted).Verifiable();

            this.mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            this.mockFolderManager.Verify();
        }

        [Test]
        public void GetFolder_Calls_DataProvider_GetFolder_When_Folder_Is_Not_In_Cache()
        {
            this.pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var foldersSorted = new List<IFolderInfo>();

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(foldersSorted);

            this.mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            this.mockData.Verify(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath), Times.Once());
        }

        [Test]
        public void GetFolder_Returns_Null_When_Folder_Does_Not_Exist_Overload()
        {
            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");

            var dr = folderDataTable.CreateDataReader();

            this.mockData.Setup(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(dr);

            this.cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns<FolderInfo>(null);

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(new List<IFolderInfo>());

            var result = this.mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetFolder_Returns_Valid_Folder_When_Folder_Exists_Overload()
        {
            this.pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.pathUtils.Setup(pu => pu.RemoveTrailingSlash(Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderName);

            var folderDataTable = new DataTable();
            folderDataTable.Columns.Add("FolderName");
            folderDataTable.Rows.Add(Constants.FOLDER_ValidFolderName);

            var dr = folderDataTable.CreateDataReader();

            this.mockData.Setup(md => md.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(dr);

            var folderInfo = new FolderInfo { FolderPath = Constants.FOLDER_ValidFolderRelativePath };

            this.cbo.Setup(cbo => cbo.FillObject<FolderInfo>(dr)).Returns(folderInfo);

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(new List<IFolderInfo>());

            var result = this.mockFolderManager.Object.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath);

            Assert.That(result.FolderName, Is.EqualTo(Constants.FOLDER_ValidFolderName));
        }

        [Test]
        public void GetFoldersByParentFolder_Throws_On_Null_ParentFolder()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.GetFolders((IFolderInfo)null));
        }

        [Test]
        public void GetFoldersByParentFolder_Returns_Empty_List_When_ParentFolder_Contains_No_Subfolders()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(new List<IFolderInfo>());

            var result = this.mockFolderManager.Object.GetFolders(this.folderInfo.Object).ToList();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetFoldersByParentFolder_Returns_Valid_Subfolders()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            var foldersSorted = new List<IFolderInfo>
                                    {
                                        new FolderInfo { FolderID = Constants.FOLDER_ValidFolderId, ParentID = Null.NullInteger },
                                        new FolderInfo { FolderID = Constants.FOLDER_OtherValidFolderId, ParentID = Constants.FOLDER_ValidFolderId },
                                    };

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(foldersSorted);

            var result = this.mockFolderManager.Object.GetFolders(this.folderInfo.Object).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].FolderID, Is.EqualTo(Constants.FOLDER_OtherValidFolderId));
        }

        [Test]
        public void GetFolders_Calls_CBO_GetCachedObject()
        {
            var folders = new List<FolderInfo>();

            this.cbo.Setup(cbo => cbo.GetCachedObject<List<FolderInfo>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>(), false)).Returns(folders).Verifiable();

            this.mockFolderManager.Object.GetFolders(Constants.CONTENT_ValidPortalId);

            this.cbo.Verify();
        }

        [Test]
        public void RenameFolder_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.RenameFolder(null, It.IsAny<string>()));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void RenameFolder_Throws_On_Null_Or_Empty_NewFolderName(string newFolderName)
        {
            Assert.Throws<ArgumentException>(() => this.folderManager.RenameFolder(this.folderInfo.Object, newFolderName));
        }

        [Test]
        public void RenameFolder_Throws_When_DestinationFolder_Exists()
        {
            this.pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_OtherValidFolderName)).Returns(Constants.FOLDER_OtherValidFolderRelativePath);

            this.folderInfo.Setup(fi => fi.FolderName).Returns(Constants.FOLDER_ValidFolderName);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);

            this.mockFolderManager.Setup(mfm => mfm.FolderExists(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidFolderRelativePath)).Returns(true);

            Assert.Throws<FolderAlreadyExistsException>(() => this.mockFolderManager.Object.RenameFolder(this.folderInfo.Object, Constants.FOLDER_OtherValidFolderName));
        }

        [Test]
        public void UpdateFolder_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.UpdateFolder(null));
        }

        [Test]
        public void UpdateFolder_Calls_DataProvider_UpdateFolder()
        {
            this.mockFolderManager.Setup(mfm => mfm.AddLogEntry(this.folderInfo.Object, It.IsAny<EventLogController.EventLogType>()));
            this.mockFolderManager.Setup(mfm => mfm.SaveFolderPermissions(this.folderInfo.Object));
            this.mockFolderManager.Setup(mfm => mfm.ClearFolderCache(It.IsAny<int>()));

            this.mockFolderManager.Object.UpdateFolder(this.folderInfo.Object);

            this.mockData.Verify(
                md => md.UpdateFolder(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void SynchronizeFolder_Throws_On_Null_RelativePath()
        {
            Assert.Throws<ArgumentNullException>(() => this.folderManager.Synchronize(It.IsAny<int>(), null, It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test]
        public void SynchronizeFolder_Throws_When_Some_Folder_Mapping_Requires_Network_Connectivity_But_There_Is_No_Network_Available()
        {
            this.mockFolderManager.Setup(mfm => mfm.AreThereFolderMappingsRequiringNetworkConnectivity(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false)).Returns(true);
            this.mockFolderManager.Setup(mfm => mfm.IsNetworkAvailable()).Returns(false);

            Assert.Throws<NoNetworkAvailableException>(() => this.mockFolderManager.Object.Synchronize(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false, false));
        }

        [Test]
        public void GetFileSystemFolders_Returns_Empty_List_When_Folder_Does_Not_Exist()
        {
            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(false);

            var result = this.mockFolderManager.Object.GetFileSystemFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetFileSystemFolders_Returns_One_Item_When_Folder_Exists_And_Is_Not_Recursive()
        {
            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            var result = this.mockFolderManager.Object.GetFileSystemFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Values[0].ExistsInFileSystem, Is.True);
        }

        [Test]
        public void GetFileSystemFolders_Calls_FolderManager_GetFileSystemFoldersRecursive_When_Folder_Exists_And_Is_Recursive()
        {
            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(Constants.FOLDER_ValidFolderPath);

            this.mockFolderManager.Setup(mfm => mfm.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderPath))
                .Returns(It.IsAny<SortedList<string, FolderManager.MergedTreeItem>>())
                .Verifiable();

            this.directory.Setup(d => d.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            this.mockFolderManager.Object.GetFileSystemFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, true);

            this.mockFolderManager.Verify();
        }

        [Test]
        public void GetFileSystemFoldersRecursive_Returns_One_Item_When_Folder_Does_Not_Have_SubFolders()
        {
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderPath)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            this.directory.Setup(d => d.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(new string[0]);

            var result = this.mockFolderManager.Object.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderPath);

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public void GetFileSystemFoldersRecursive_Returns_All_The_Folders_In_Folder_Tree()
        {
            var relativePaths = new Dictionary<string, string>
                                    {
                                        { @"C:\folder", "folder/" },
                                        { @"C:\folder\subfolder", "folder/subfolder/" },
                                        { @"C:\folder\subfolder2", "folder/subfolder2/" },
                                        { @"C:\folder\subfolder2\subsubfolder", "folder/subfolder2/subsubfolder/" },
                                        { @"C:\folder\subfolder2\subsubfolder2", "folder/subfolder2/subsubfolder2/" },
                                    };

            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, It.IsAny<string>()))
                .Returns<int, string>((portalID, physicalPath) => relativePaths[physicalPath]);

            var directories = new List<string> { @"C:\folder\subfolder", @"C:\folder\subfolder2", @"C:\folder\subfolder2\subsubfolder", @"C:\folder\subfolder2\subsubfolder2" };

            this.directory.Setup(d => d.GetDirectories(It.IsAny<string>()))
                .Returns<string>(path => directories.FindAll(sub => sub.StartsWith(path + "\\") && sub.LastIndexOf("\\") == path.Length).ToArray());

            var result = this.mockFolderManager.Object.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, @"C:\folder");

            Assert.That(result, Has.Count.EqualTo(5));
        }

        [Test]
        public void GetFileSystemFoldersRecursive_Sets_ExistsInFileSystem_For_All_Items()
        {
            var relativePaths = new Dictionary<string, string>
                                    {
                                        { @"C:\folder", "folder/" },
                                        { @"C:\folder\subfolder", "folder/subfolder/" },
                                        { @"C:\folder\subfolder2", "folder/subfolder2/" },
                                        { @"C:\folder\subfolder2\subsubfolder", "folder/subfolder2/subsubfolder/" },
                                        { @"C:\folder\subfolder2\subsubfolder2", "folder/subfolder2/subsubfolder2/" },
                                    };

            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, It.IsAny<string>()))
                .Returns<int, string>((portalID, physicalPath) => relativePaths[physicalPath]);

            var directories = new List<string> { @"C:\folder", @"C:\folder\subfolder", @"C:\folder\subfolder2", @"C:\folder\subfolder2\subsubfolder", @"C:\folder\subfolder2\subsubfolder2" };

            this.directory.Setup(d => d.GetDirectories(It.IsAny<string>()))
                .Returns<string>(path => directories.FindAll(sub => sub.StartsWith(path + "\\") && sub.LastIndexOf("\\") == path.Length).ToArray());

            var result = this.mockFolderManager.Object.GetFileSystemFoldersRecursive(Constants.CONTENT_ValidPortalId, @"C:\folder");

            foreach (var mergedTreeItem in result.Values)
            {
                Assert.That(mergedTreeItem.ExistsInFileSystem, Is.True);
            }
        }

        [Test]
        public void GetDatabaseFolders_Returns_Empty_List_When_Folder_Does_Not_Exist()
        {
            this.mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns<IFolderInfo>(null);

            var result = this.mockFolderManager.Object.GetDatabaseFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetDatabaseFolders_Returns_One_Item_When_Folder_Exists_And_Is_Not_Recursive()
        {
            this.mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this.folderInfo.Object);

            var result = this.mockFolderManager.Object.GetDatabaseFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, false);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Values[0].ExistsInDatabase, Is.True);
        }

        [Test]
        public void GetDatabaseFolders_Calls_FolderManager_GetDatabaseFoldersRecursive_When_Folder_Exists_And_Is_Recursive()
        {
            this.mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
                .Returns(this.folderInfo.Object);

            this.mockFolderManager.Setup(mfm => mfm.GetDatabaseFoldersRecursive(this.folderInfo.Object))
                .Returns(It.IsAny<SortedList<string, FolderManager.MergedTreeItem>>())
                .Verifiable();

            this.mockFolderManager.Object.GetDatabaseFolders(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, true);

            this.mockFolderManager.Verify();
        }

        [Test]
        public void GetDatabaseFoldersRecursive_Returns_One_Item_When_Folder_Does_Not_Have_SubFolders()
        {
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var subfolders = new List<IFolderInfo>();

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(this.folderInfo.Object)).Returns(subfolders);

            var result = this.mockFolderManager.Object.GetDatabaseFoldersRecursive(this.folderInfo.Object);

            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public void GetDatabaseFoldersRecursive_Returns_All_The_Folders_In_Folder_Tree()
        {
            this.folderInfo.Setup(fi => fi.FolderPath).Returns("folder/");
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var subfolders = new List<IFolderInfo>
                                 {
                                     new FolderInfo { FolderPath = "folder/subfolder/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                     new FolderInfo { FolderPath = "folder/subfolder2/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                     new FolderInfo { FolderPath = "folder/subfolder2/subsubfolder/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                     new FolderInfo { FolderPath = "folder/subfolder2/subsubfolder2/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                 };

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(It.IsAny<IFolderInfo>()))
                .Returns<IFolderInfo>(parent => subfolders.FindAll(sub =>
                    sub.FolderPath.StartsWith(parent.FolderPath) &&
                    sub.FolderPath.Length > parent.FolderPath.Length &&
                    sub.FolderPath.Substring(parent.FolderPath.Length).IndexOf("/") == sub.FolderPath.Substring(parent.FolderPath.Length).LastIndexOf("/")));

            var result = this.mockFolderManager.Object.GetDatabaseFoldersRecursive(this.folderInfo.Object);

            Assert.That(result, Has.Count.EqualTo(5));
        }

        [Test]
        public void GetDatabaseFoldersRecursive_Sets_ExistsInDatabase_For_All_Items()
        {
            this.folderInfo.Setup(fi => fi.FolderPath).Returns("folder/");
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var subfolders = new List<IFolderInfo>
                                 {
                                     new FolderInfo() { FolderPath = "folder/subfolder/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                     new FolderInfo() { FolderPath = "folder/subfolder2/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                     new FolderInfo() { FolderPath = "folder/subfolder2/subsubfolder/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                     new FolderInfo() { FolderPath = "folder/subfolder2/subsubfolder2/", FolderMappingID = Constants.FOLDER_ValidFolderMappingID },
                                 };

            this.mockFolderManager.Setup(mfm => mfm.GetFolders(It.IsAny<IFolderInfo>()))
                .Returns<IFolderInfo>(parent => subfolders.FindAll(sub =>
                    sub.FolderPath.StartsWith(parent.FolderPath) &&
                    sub.FolderPath.Length > parent.FolderPath.Length &&
                    sub.FolderPath.Substring(parent.FolderPath.Length).IndexOf("/") == sub.FolderPath.Substring(parent.FolderPath.Length).LastIndexOf("/")));

            var result = this.mockFolderManager.Object.GetDatabaseFoldersRecursive(this.folderInfo.Object);

            foreach (var mergedTreeItem in result.Values)
            {
                Assert.That(mergedTreeItem.ExistsInDatabase, Is.True);
            }
        }

        // [Test]
        // public void GetFolderMappingFoldersRecursive_Returns_One_Item_When_Folder_Does_Not_Have_SubFolders()
        // {
        //    var folderMapping = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

        // var subfolders = new List<string>();

        // _mockFolder.Setup(mf => mf.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping)).Returns(subfolders);

        // var result = _mockFolderManager.Object.GetFolderMappingFoldersRecursive(folderMapping, Constants.FOLDER_ValidFolderRelativePath);

        // Assert.AreEqual(1, result.Count);
        // }

        // [Test]
        // public void GetFolderMappingFoldersRecursive_Returns_All_The_Folders_In_Folder_Tree()
        // {
        //    var mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        //    mockCache.Setup(c => c.GetItem(It.IsAny<string>())).Returns(null);

        // var settings = new Hashtable();
        //    settings["SyncAllSubFolders"] = "true";

        // _folderMappingController.Setup(c => c.GetFolderMappingSettings(It.IsAny<int>())).Returns(settings);

        // var hostSettingsTable = new DataTable("HostSettings");
        //    var nameCol = hostSettingsTable.Columns.Add("SettingName");
        //    hostSettingsTable.Columns.Add("SettingValue");
        //    hostSettingsTable.Columns.Add("SettingIsSecure");
        //    hostSettingsTable.PrimaryKey = new[] { nameCol };

        // _mockData.Setup(c => c.GetHostSettings()).Returns(hostSettingsTable.CreateDataReader());
        //    _mockData.Setup(c => c.GetProviderPath()).Returns(String.Empty);

        // var folderMapping = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

        // var subfolders = new List<string> { "folder/subfolder", "folder/subfolder2", "folder/subfolder2/subsubfolder", "folder/subfolder2/subsubfolder2" };

        // _mockFolder.Setup(mf => mf.GetSubFolders(It.IsAny<string>(), folderMapping))
        //        .Returns<string, FolderMappingInfo>((parent, fm) => subfolders.FindAll(sub =>
        //            sub.StartsWith(parent) &&
        //            sub.Length > parent.Length &&
        //            sub.Substring(parent.Length).IndexOf("/") == sub.Substring(parent.Length).LastIndexOf("/")));

        // var result = _mockFolderManager.Object.GetFolderMappingFoldersRecursive(folderMapping, "folder/");

        // Assert.AreEqual(5, result.Count);
        // }

        // [Test]
        // public void GetDatabaseFoldersRecursive_Sets_ExistsInFolderMappings_For_All_Items()
        // {
        //    var folderMapping = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

        // var subfolders = new List<string> { "folder/subfolder", "folder/subfolder2", "folder/subfolder2/subsubfolder", "folder/subfolder2/subsubfolder2" };

        // _mockFolder.Setup(mf => mf.GetSubFolders(It.IsAny<string>(), folderMapping))
        //        .Returns<string, FolderMappingInfo>((parent, fm) => subfolders.FindAll(sub =>
        //            sub.StartsWith(parent) &&
        //            sub.Length > parent.Length &&
        //            sub.Substring(parent.Length).IndexOf("/") == sub.Substring(parent.Length).LastIndexOf("/")));

        // var result = _mockFolderManager.Object.GetFolderMappingFoldersRecursive(folderMapping, "folder/");

        // foreach (var mergedTreeItem in result.Values)
        //    {
        //        Assert.True(mergedTreeItem.ExistsInFolderMappings.Contains(Constants.FOLDER_ValidFolderMappingID));
        //    }
        // }
        [Test]
        public void MergeFolderLists_Returns_Empty_List_When_Both_Lists_Are_Empty()
        {
            var list1 = new SortedList<string, FolderManager.MergedTreeItem>();
            var list2 = new SortedList<string, FolderManager.MergedTreeItem>();

            var result = this.folderManager.MergeFolderLists(list1, list2);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void MergeFolderLists_Count_Equals_The_Intersection_Count_Between_Both_Lists()
        {
            var list1 = new SortedList<string, FolderManager.MergedTreeItem>
                            {
                                { "folder1", new FolderManager.MergedTreeItem { FolderPath = "folder1" } },
                                { "folder2", new FolderManager.MergedTreeItem { FolderPath = "folder2" } },
                            };

            var list2 = new SortedList<string, FolderManager.MergedTreeItem>
                            {
                                { "folder1", new FolderManager.MergedTreeItem { FolderPath = "folder1" } },
                                { "folder3", new FolderManager.MergedTreeItem { FolderPath = "folder3" } },
                            };

            var result = this.folderManager.MergeFolderLists(list1, list2);

            Assert.That(result, Has.Count.EqualTo(3));
        }

        // [Test]
        // public void MergeFolderLists_Merges_TreeItem_Properties()
        // {
        //    var list1 = new SortedList<string, FolderManager.MergedTreeItem>
        //                    {
        //                        {
        //                            "folder1",
        //                            new FolderManager.MergedTreeItem {FolderPath = "folder1", ExistsInFileSystem = true, ExistsInDatabase = true, FolderMappingID = Constants.FOLDER_ValidFolderMappingID}
        //                            }
        //                    };

        // var list2 = new SortedList<string, FolderManager.MergedTreeItem>
        //                    {
        //                        {
        //                            "folder1",
        //                            new FolderManager.MergedTreeItem {FolderPath = "folder1", ExistsInFileSystem = false, ExistsInDatabase = false, ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID}}
        //                            }
        //                    };

        // var result = _folderManager.MergeFolderLists(list1, list2);

        // Assert.AreEqual(1, result.Count);
        //    Assert.IsTrue(result.Values[0].ExistsInFileSystem);
        //    Assert.IsTrue(result.Values[0].ExistsInDatabase);
        //    Assert.AreEqual(Constants.FOLDER_ValidFolderMappingID, result.Values[0].FolderMappingID);
        //    Assert.IsTrue(result.Values[0].ExistsInFolderMappings.Contains(Constants.FOLDER_ValidFolderMappingID));
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Sets_StorageLocation_To_Default_When_Folder_Exists_Only_In_FileSystem_And_Database_And_FolderMapping_Is_Not_Default_And_Has_SubFolders()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID
        //                                     }
        //                                 },
        //                             {Constants.FOLDER_ValidSubFolderRelativePath, new FolderManager.MergedTreeItem {FolderPath = Constants.FOLDER_ValidSubFolderRelativePath}}
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo();
        //    var defaultFolderMapping = new FolderMappingInfo { FolderMappingID = 1 };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

        // _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
        //    _mockFolderManager.Setup(mfm => mfm.GetFolders(_folderInfo.Object)).Returns(new List<IFolderInfo>());
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, 1)).Verifiable();

        // _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Deletes_Folder_From_FileSystem_And_Database_When_Folder_Exists_Only_In_FileSystem_And_Database_And_FolderMapping_Is_Not_Default_And_Has_Not_SubFolders()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo();

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

        // _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
        //    _mockFolderManager.Setup(mfm => mfm.GetFolders(_folderInfo.Object)).Returns(new List<IFolderInfo>());

        // _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // _directory.Verify(d => d.Delete(Constants.FOLDER_ValidFolderPath, false), Times.Once());
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_FolderMapping_Is_Default()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderProviderType = "StandardFolderProvider" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Sets_StorageLocation_To_The_Highest_Priority_One_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Default_And_Folder_Does_Not_Contain_Files()
        // {
        //    const int externalStorageLocation = 15;
        //    const string externalMappingName = "External Mapping";

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, FolderProviderType = "StandardFolderProvider" };

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation, MappingName = externalMappingName };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
        //        .Returns(_folderInfo.Object);

        // _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

        // var mockFolder = MockComponentProvider.CreateFolderProvider("StandardFolderProvider");

        // mockFolder.Setup(mf => mf.GetFiles(_folderInfo.Object)).Returns(new string[0]);

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalMappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Default_And_Folder_Contains_Files()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, FolderProviderType = "StandardFolderProvider", MappingName = "Default Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath))
        //        .Returns(_folderInfo.Object);

        // var mockFolder = MockComponentProvider.CreateFolderProvider("StandardFolderProvider");

        // mockFolder.Setup(mf => mf.GetFiles(_folderInfo.Object)).Returns(new string[1]);

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Sets_StorageLocation_To_The_Highest_Priority_One_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Different_From_Actual()
        // {
        //    const int externalStorageLocation = 15;
        //    const string externalMappingName = "External Mapping";

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID };

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation, MappingName = externalMappingName };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalMappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_More_Than_One_FolderMappings_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Equal_Than_Actual()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID, externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, Priority = 0, MappingName = "Default Mapping" };
        //    var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation, Priority = 1, MappingName = "External Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Null_And_Does_Nothing_When_Folder_Exists_Only_In_FileSystem_And_Database_And_One_FolderMapping_And_FolderMapping_Is_Not_Default()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, MappingName = "Default Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.IsNull(result);
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Creates_Folder_In_Database_With_Default_StorageLocation_When_Folder_Exists_Only_In_FileSystem()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {FolderPath = Constants.FOLDER_ValidFolderRelativePath, ExistsInFileSystem = true, ExistsInDatabase = false}
        //                                 }
        //                         };

        // var defaultFolderMapping = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID };

        // _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

        // _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidFolderMappingID)).Verifiable();

        // _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_Database_With_Default_StorageLocation_When_Folder_Exists_Only_In_FileSystem_And_One_Or_More_FolderMappings_And_Folder_Contains_Files()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = false,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var defaultFolderMapping = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, MappingName = "Default Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

        // _directory.Setup(d => d.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(new string[1]);

        // _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidFolderMappingID)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, defaultFolderMapping.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_Database_With_The_Highest_Priority_StorageLocation_When_Folder_Exists_Only_In_FileSystem_And_One_Or_More_FolderMappings_And_Folder_Does_Not_Contain_Files()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = true,
        //                                         ExistsInDatabase = false,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation, MappingName = "External Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

        // _directory.Setup(d => d.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(new string[0]);

        // _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_FolderMapping_Is_Default()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderProviderType = "StandardFolderProvider" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.IsNull(result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_Default_When_Folder_Exists_Only_In_Database_And_FolderMapping_Is_Not_Default_And_Folder_Has_SubFolders()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID
        //                                     }
        //                                 },
        //                             {Constants.FOLDER_ValidSubFolderRelativePath, new FolderManager.MergedTreeItem {FolderPath = Constants.FOLDER_ValidSubFolderRelativePath}}
        //                         };

        // var defaultFolderMapping = new FolderMappingInfo { FolderMappingID = 1 };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(new FolderMappingInfo());
        //    _folderMappingController.Setup(fmc => fmc.GetDefaultFolderMapping(Constants.CONTENT_ValidPortalId)).Returns(defaultFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, defaultFolderMapping.FolderMappingID)).Verifiable();

        // _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Deletes_Folder_In_Database_When_Folder_Exists_Only_In_Database_And_FolderMapping_Is_Not_Default_And_Folder_Has_Not_SubFolders()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID
        //                                     }
        //                                 }
        //                         };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(new FolderMappingInfo());

        // _mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

        // _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInFileSystem(It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_Only_External_FolderMapping_When_Folder_Exists_Only_In_Database_And_One_FolderMapping_And_FolderMapping_Is_Default_But_Not_Database()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderProviderType = "StandardFolderProvider" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.IsNull(result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_More_Than_One_FolderMapping_And_FolderMapping_Is_Default_But_Not_Database()
        // {
        //    const int externalStorageLocation1 = 15;
        //    const int externalStorageLocation2 = 16;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation1, externalStorageLocation2}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderProviderType = "StandardFolderProvider" };

        // var externalFolderMapping1 = new FolderMappingInfo { FolderMappingID = externalStorageLocation1, Priority = 0, MappingName = "External Mapping" };

        // var externalFolderMapping2 = new FolderMappingInfo { Priority = 1 };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation1)).Returns(externalFolderMapping1);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation2)).Returns(externalFolderMapping2);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation1)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping1.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Database_And_Folder_Does_Not_Contain_Files()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderProviderType = "DatabaseFolderProvider" };

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation, MappingName = "External Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
        //    _mockFolderManager.Setup(mfm => mfm.GetFiles(_folderInfo.Object, It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo>());
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_One_Or_More_FolderMappings_And_FolderMapping_Is_Database_And_Folder_Contains_Files()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderProviderType = "DatabaseFolderProvider", MappingName = "Database Mapping" };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
        //    _mockFolderManager.Setup(mfm => mfm.GetFiles(_folderInfo.Object, It.IsAny<bool>(), It.IsAny<bool>())).Returns(new List<IFileInfo> { new FileInfo() });
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Different_From_Actual()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo();

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.IsNull(result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Sets_StorageLocation_To_The_One_With_Highest_Priority_When_Folder_Exists_Only_In_Database_And_More_Than_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_IsDifferent_From_Actual()
        // {
        //    const int externalStorageLocation1 = 15;
        //    const int externalStorageLocation2 = 16;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {externalStorageLocation1, externalStorageLocation2}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo();

        // var externalFolderMapping1 = new FolderMappingInfo { FolderMappingID = externalStorageLocation1, Priority = 0, MappingName = "External Mapping" };

        // var externalFolderMapping2 = new FolderMappingInfo { Priority = 1 };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation1)).Returns(externalFolderMapping1);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation2)).Returns(externalFolderMapping2);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.UpdateFolderMappingID(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, externalStorageLocation1)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping1.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Equal_Than_Actual()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.IsNull(result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_When_Folder_Exists_Only_In_Database_And_More_Than_One_FolderMapping_And_FolderMapping_Is_Not_Default_And_New_FolderMapping_Is_Equal_Than_Actual()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = true,
        //                                         FolderMappingID = Constants.FOLDER_ValidFolderMappingID,
        //                                         ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID, externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var folderMappingOfItem = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, Priority = 0, MappingName = "External Mapping" };

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = externalStorageLocation, Priority = 1 };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMappingOfItem);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, folderMappingOfItem.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.CreateFolderInDatabase(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Null_And_Creates_Folder_In_FileSystem_And_Creates_Folder_In_Database_With_The_Highest_Priority_StorageLocation_When_Folder_Exists_Only_In_One_FolderMapping()
        // {
        //    var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = false,
        //                                         ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID}
        //                                     }
        //                                 }
        //                         };

        // var externalFolderMapping = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(externalFolderMapping);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidFolderMappingID)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.IsNull(result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }

        // [Test]
        // public void ProcessMergedTreeItem_Returns_Collision_And_Creates_Folder_In_FileSystem_And_Creates_Folder_In_Database_With_The_Highest_Priority_StorageLocation_When_Folder_Exists_Only_In_More_Than_One_FolderMapping()
        // {
        //    const int externalStorageLocation = 15;

        // var mergedTree = new SortedList<string, FolderManager.MergedTreeItem>
        //                         {
        //                             {
        //                                 Constants.FOLDER_ValidFolderRelativePath,
        //                                 new FolderManager.MergedTreeItem {
        //                                         FolderPath = Constants.FOLDER_ValidFolderRelativePath,
        //                                         ExistsInFileSystem = false,
        //                                         ExistsInDatabase = false,
        //                                         ExistsInFolderMappings = new List<int> {Constants.FOLDER_ValidFolderMappingID, externalStorageLocation}
        //                                     }
        //                                 }
        //                         };

        // var externalFolderMapping1 = new FolderMappingInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID, Priority = 0, MappingName = "External Mapping" };

        // var externalFolderMapping2 = new FolderMappingInfo { FolderMappingID = externalStorageLocation, Priority = 1 };

        // _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(externalFolderMapping1);
        //    _folderMappingController.Setup(fmc => fmc.GetFolderMapping(externalStorageLocation)).Returns(externalFolderMapping2);

        // _pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInFileSystem(Constants.FOLDER_ValidFolderPath)).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.CreateFolderInDatabase(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_ValidFolderMappingID)).Verifiable();

        // var result = _mockFolderManager.Object.ProcessMergedTreeItem(mergedTree.Values[0], 0, mergedTree, Constants.CONTENT_ValidPortalId);

        // Assert.AreEqual(string.Format("Collision on path '{0}'. Resolved using '{1}' folder mapping.", Constants.FOLDER_ValidFolderRelativePath, externalFolderMapping1.MappingName), result);
        //    _mockFolderManager.Verify();
        //    _mockFolderManager.Verify(mfm => mfm.UpdateFolderMappingID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
        //    _mockFolderManager.Verify(mfm => mfm.DeleteFolder(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        //    _directory.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        // }


        [Test]
        public void MoveFolder_Returns_The_Same_Folder_If_The_Paths_Are_The_Same()
        {
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);

            IFolderInfo destinationFolder = new FolderInfo();
            destinationFolder.FolderPath = Constants.FOLDER_ValidFolderRelativePath;

            this.pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var movedFolder = this.folderManager.MoveFolder(this.folderInfo.Object, destinationFolder);

            Assert.That(movedFolder, Is.EqualTo(this.folderInfo.Object));
        }

        [Test]
        public void MoveFolder_Throws_When_Move_Operation_Is_Not_Valid()
        {
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            IFolderInfo destinationFolder = new FolderInfo();
            destinationFolder.FolderPath = Constants.FOLDER_OtherValidFolderRelativePath;
            destinationFolder.FolderMappingID = Constants.FOLDER_ValidFolderMappingID;

            this.pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_OtherValidFolderRelativePath)).Returns(Constants.FOLDER_OtherValidFolderRelativePath);

            this.mockFolderManager.Setup(mfm => mfm.FolderExists(It.IsAny<int>(), It.IsAny<string>())).Returns(false);
            this.mockFolderManager.Setup(mfm => mfm.CanMoveBetweenFolderMappings(It.IsAny<FolderMappingInfo>(), It.IsAny<FolderMappingInfo>())).Returns(true);
            this.mockFolderManager.Setup(mfm => mfm.IsMoveOperationValid(this.folderInfo.Object, destinationFolder, It.IsAny<string>())).Returns(false);

            Assert.Throws<InvalidOperationException>(() => this.mockFolderManager.Object.MoveFolder(this.folderInfo.Object, destinationFolder));
        }

        // [Test]
        // public void MoveFolder_Calls_Internal_OverwriteFolder_When_Target_Folder_Already_Exists()
        // {
        //    _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
        //    _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

        // _pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_OtherValidFolderRelativePath)).Returns(Constants.FOLDER_OtherValidFolderRelativePath);

        // _mockFolderManager.Setup(mfm => mfm.IsMoveOperationValid(_folderInfo.Object, Constants.FOLDER_OtherValidFolderRelativePath)).Returns(true);

        // var folders = new List<IFolderInfo> { _folderInfo.Object };
        //    var targetFolder = new FolderInfo { FolderPath = Constants.FOLDER_OtherValidFolderRelativePath };

        // _mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(folders);
        //    _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidFolderRelativePath)).Returns(targetFolder);

        // _mockFolderManager.Setup(mfm => mfm.OverwriteFolder(_folderInfo.Object, targetFolder, It.IsAny<Dictionary<int, FolderMappingInfo>>(), It.IsAny<SortedList<string, IFolderInfo>>())).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.RenameFolderInFileSystem(_folderInfo.Object, Constants.FOLDER_OtherValidFolderRelativePath));

        // _mockFolderManager.Object.MoveFolder(_folderInfo.Object, Constants.FOLDER_OtherValidFolderRelativePath);

        // _mockFolderManager.Verify();
        // }

        // [Test]
        // public void MoveFolder_Calls_Internal_MoveFolder_When_Target_Folder_Does_Not_Exist()
        // {
        //    _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
        //    _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

        // _pathUtils.Setup(pu => pu.FormatFolderPath(Constants.FOLDER_OtherValidFolderRelativePath)).Returns(Constants.FOLDER_OtherValidFolderRelativePath);

        // _mockFolderManager.Setup(mfm => mfm.IsMoveOperationValid(_folderInfo.Object, Constants.FOLDER_OtherValidFolderRelativePath)).Returns(true);

        // var folders = new List<IFolderInfo> { _folderInfo.Object };

        // _mockFolderManager.Setup(mfm => mfm.GetFolders(Constants.CONTENT_ValidPortalId)).Returns(folders);
        //    _mockFolderManager.Setup(mfm => mfm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidFolderRelativePath)).Returns((IFolderInfo)null);

        // _mockFolderManager.Setup(
        //        mfm =>
        //        mfm.MoveFolder(_folderInfo.Object,
        //                       Constants.FOLDER_OtherValidFolderRelativePath,
        //                       Constants.FOLDER_OtherValidFolderRelativePath,
        //                       It.IsAny<List<int>>(),
        //                       _folderInfo.Object,
        //                       It.IsAny<Dictionary<int, FolderMappingInfo>>())).Verifiable();
        //    _mockFolderManager.Setup(mfm => mfm.RenameFolderInFileSystem(_folderInfo.Object, Constants.FOLDER_OtherValidFolderRelativePath));

        // _mockFolderManager.Object.MoveFolder(_folderInfo.Object, Constants.FOLDER_OtherValidFolderRelativePath);

        // _mockFolderManager.Verify();
        // }
        [Test]
        public void OverwriteFolder_Calls_MoveFile_For_Each_File_In_Source_Folder()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var destinationFolder = new FolderInfo();

            var file1 = new FileInfo();
            var file2 = new FileInfo();
            var file3 = new FileInfo();

            var files = new List<IFileInfo> { file1, file2, file3 };
            this.mockFolderManager.Setup(mfm => mfm.GetFiles(this.folderInfo.Object, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files);

            var fileManager = new Mock<IFileManager>();
            FileManager.RegisterInstance(fileManager.Object);

            fileManager.Setup(fm => fm.MoveFile(It.IsAny<IFileInfo>(), destinationFolder));

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));

            var folderMapping = new FolderMappingInfo();

            this.mockFolderManager.Setup(mfm => mfm.GetFolderMapping(It.IsAny<Dictionary<int, FolderMappingInfo>>(), Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.mockFolderManager.Setup(mfm => mfm.IsFolderMappingEditable(folderMapping)).Returns(false);

            this.mockFolderManager.Object.OverwriteFolder(this.folderInfo.Object, destinationFolder, new Dictionary<int, FolderMappingInfo>(), new SortedList<string, IFolderInfo>());

            fileManager.Verify(fm => fm.MoveFile(It.IsAny<IFileInfo>(), destinationFolder), Times.Exactly(3));
        }

        [Test]
        public void OverwriteFolder_Deletes_Source_Folder_In_Database()
        {
            var fileManager = new Mock<IFileManager>();
            FileManager.RegisterInstance(fileManager.Object);

            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var files = new List<IFileInfo>();
            this.mockFolderManager.Setup(mfm => mfm.GetFiles(this.folderInfo.Object, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files);

            var destinationFolder = new FolderInfo();

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Verifiable();

            var folderMapping = new FolderMappingInfo();

            this.mockFolderManager.Setup(mfm => mfm.GetFolderMapping(It.IsAny<Dictionary<int, FolderMappingInfo>>(), Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.mockFolderManager.Setup(mfm => mfm.IsFolderMappingEditable(folderMapping)).Returns(false);

            this.mockFolderManager.Object.OverwriteFolder(this.folderInfo.Object, destinationFolder, new Dictionary<int, FolderMappingInfo>(), new SortedList<string, IFolderInfo>());

            this.mockFolderManager.Verify();
        }

        [Test]
        public void OverwriteFolder_Adds_Folder_To_FoldersToDelete_If_FolderMapping_Is_Editable()
        {
            var fileManager = new Mock<IFileManager>();
            FileManager.RegisterInstance(fileManager.Object);

            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            var files = new List<IFileInfo>();
            this.mockFolderManager.Setup(mfm => mfm.GetFiles(this.folderInfo.Object, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files);

            var destinationFolder = new FolderInfo();

            this.mockFolderManager.Setup(mfm => mfm.DeleteFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath));

            var folderMapping = new FolderMappingInfo();

            this.mockFolderManager.Setup(mfm => mfm.GetFolderMapping(It.IsAny<Dictionary<int, FolderMappingInfo>>(), Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            this.mockFolderManager.Setup(mfm => mfm.IsFolderMappingEditable(folderMapping)).Returns(true);

            var foldersToDelete = new SortedList<string, IFolderInfo>();
            this.mockFolderManager.Object.OverwriteFolder(this.folderInfo.Object, destinationFolder, new Dictionary<int, FolderMappingInfo>(), foldersToDelete);

            Assert.That(foldersToDelete, Has.Count.EqualTo(1));
        }

        // [Test]
        // public void MoveFolder_Calls_FolderProvider_MoveFolder_When_FolderMapping_Is_Not_Already_Processed_And_Is_Editable()
        // {
        //    _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_ValidFolderRelativePath);

        // var subFolder = new FolderInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID };
        //    var folderMappingsProcessed = new List<int>();
        //    var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
        //    var folderMappings = new Dictionary<int, FolderMappingInfo>();

        // _mockFolderManager.Setup(mfm => mfm.GetFolderMapping(folderMappings, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
        //    _mockFolderManager.Setup(mfm => mfm.IsFolderMappingEditable(folderMapping)).Returns(true);

        // _mockFolder.Setup(mf => mf.MoveFolder(Constants.FOLDER_ValidFolderRelativePath, Constants.FOLDER_OtherValidFolderRelativePath, folderMapping)).Verifiable();

        // _mockFolderManager.Setup(mfm => mfm.UpdateFolder(subFolder));

        // _mockFolderManager.Object.MoveFolder(_folderInfo.Object,
        //                                         Constants.FOLDER_OtherValidFolderRelativePath,
        //                                         Constants.FOLDER_OtherValidFolderRelativePath,
        //                                         folderMappingsProcessed,
        //                                         subFolder,
        //                                         folderMappings);

        // _mockFolder.Verify();
        // }

        // [Test]
        // public void MoveFolder_Calls_UpdateFolder()
        // {
        //    var subFolder = new FolderInfo { FolderMappingID = Constants.FOLDER_ValidFolderMappingID };
        //    var folderMappingsProcessed = new List<int> { Constants.FOLDER_ValidFolderMappingID };
        //    var folderMappings = new Dictionary<int, FolderMappingInfo>();

        // _mockFolderManager.Setup(mfm => mfm.UpdateFolder(subFolder)).Verifiable();

        // _mockFolderManager.Object.MoveFolder(_folderInfo.Object,
        //                                         Constants.FOLDER_OtherValidFolderRelativePath,
        //                                         Constants.FOLDER_OtherValidFolderRelativePath,
        //                                         folderMappingsProcessed,
        //                                         subFolder,
        //                                         folderMappings);

        // _mockFolderManager.Verify();
        // }
    }
}
