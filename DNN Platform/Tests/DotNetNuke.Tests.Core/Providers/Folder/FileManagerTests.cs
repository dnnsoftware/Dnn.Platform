// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Text;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [TestFixture]
    public class FileManagerTests
    {
        private FileManager _fileManager;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFolderPermissionController> _folderPermissionController;
        private Mock<IPortalController> _portalController;
        private Mock<IFolderMappingController> _folderMappingController;
        private Mock<IGlobals> _globals;
        private Mock<ICBO> _cbo;
        private Mock<DataProvider> _mockData;
        private Mock<FolderProvider> _mockFolder;
        private Mock<CachingProvider> _mockCache;
        private Mock<FileManager> _mockFileManager;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;
        private Mock<IPathUtils> _pathUtils;
        private Mock<IFileVersionController> _fileVersionController;
        private Mock<IWorkflowManager> _workflowManager;
        private Mock<IEventHandlersContainer<IFileEventHandlers>> _fileEventHandlersContainer;
        private Mock<IFileLockingController> _mockFileLockingController;
        private Mock<IFileDeletionController> _mockFileDeletionController;
        private Mock<IHostController> _hostController;

        [SetUp]
        public void Setup()
        {
            this._mockData = MockComponentProvider.CreateDataProvider();
            this._mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            this._mockCache = MockComponentProvider.CreateDataCacheProvider();

            this._folderManager = new Mock<IFolderManager>();
            this._folderPermissionController = new Mock<IFolderPermissionController>();
            this._portalController = new Mock<IPortalController>();
            this._hostController = new Mock<IHostController>();
            this._folderMappingController = new Mock<IFolderMappingController>();
            this._fileVersionController = new Mock<IFileVersionController>();
            this._workflowManager = new Mock<IWorkflowManager>();
            this._fileEventHandlersContainer = new Mock<IEventHandlersContainer<IFileEventHandlers>>();
            this._globals = new Mock<IGlobals>();
            this._cbo = new Mock<ICBO>();
            this._pathUtils = new Mock<IPathUtils>();
            this._mockFileLockingController = new Mock<IFileLockingController>();
            this._mockFileDeletionController = new Mock<IFileDeletionController>();

            EventLogController.SetTestableInstance(Mock.Of<IEventLogController>());
            FolderManager.RegisterInstance(this._folderManager.Object);
            FolderPermissionController.SetTestableInstance(this._folderPermissionController.Object);
            PortalController.SetTestableInstance(this._portalController.Object);
            HostController.RegisterInstance(this._hostController.Object);
            FolderMappingController.RegisterInstance(this._folderMappingController.Object);
            TestableGlobals.SetTestableInstance(this._globals.Object);
            CBO.SetTestableInstance(this._cbo.Object);
            PathUtils.RegisterInstance(this._pathUtils.Object);
            FileVersionController.RegisterInstance(this._fileVersionController.Object);
            WorkflowManager.SetTestableInstance(this._workflowManager.Object);
            EventHandlersContainer<IFileEventHandlers>.RegisterInstance(this._fileEventHandlersContainer.Object);
            this._mockFileManager = new Mock<FileManager> { CallBase = true };

            this._folderInfo = new Mock<IFolderInfo>();
            this._fileInfo = new Mock<IFileInfo>();

            this._fileManager = new FileManager();

            FileLockingController.SetTestableInstance(this._mockFileLockingController.Object);
            FileDeletionController.SetTestableInstance(this._mockFileDeletionController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();

            FolderPermissionController.ClearInstance();
            FileLockingController.ClearInstance();
            FileDeletionController.ClearInstance();
            MockComponentProvider.ResetContainer();
            PortalController.ClearInstance();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddFile_Throws_On_Null_Folder()
        {
            this._fileManager.AddFile(null, It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            this._fileManager.AddFile(this._folderInfo.Object, fileName, It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_Null_FileContent()
        {
            this._fileManager.AddFile(this._folderInfo.Object, It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void AddFile_Throws_When_Permissions_Are_Not_Met()
        {
            this._folderPermissionController.Setup(fpc => fpc.CanAddFolder(this._folderInfo.Object)).Returns(false);

            this._fileManager.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(), It.IsAny<bool>(), true, It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(NoSpaceAvailableException))]
        public void AddFile_Throws_When_Portal_Has_No_Space_Available()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._mockData.Setup(c => c.GetProviderPath()).Returns(string.Empty);

            var fileContent = new MemoryStream();

            this._globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(false);

            this._mockFileManager.Setup(fm => fm.CreateFileContentItem()).Returns(new ContentItem());
            this._mockFileManager.Setup(fm => fm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);

            this._mockFileManager.Object.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
        }

        [Test]
        public void AddFile_Checks_Space_For_Stream_Length()
        {
            // Arrange
            this.PrepareFileSecurityCheck();
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream(Encoding.ASCII.GetBytes("some data here"));

            this._portalController.Setup(pc => pc.HasSpaceAvailable(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            this._globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);
            this._mockFolder.Setup(mf => mf.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();

            this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            this._mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());
            this._mockFileManager.Setup(mfm => mfm.IsImageFile(It.IsAny<IFileInfo>())).Returns(false);

            this._workflowManager.Setup(we => we.GetWorkflow(It.IsAny<int>())).Returns((Workflow)null);

            // Act
            this._mockFileManager.Object.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, false, Constants.CONTENTTYPE_ValidContentType);

            // Assert
            this._portalController.Verify(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length));
        }

        [Test]
        [ExpectedException(typeof(InvalidFileExtensionException))]
        public void AddFile_Throws_When_Extension_Is_Invalid()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var fileContent = new MemoryStream();

            this._portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(false);

            this._mockFileManager.Object.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
        }

        [TestCase("invalid_script.svg")]
        [TestCase("invalid_onload.svg")]
        [TestCase("invalid_onerror.svg")]
        [ExpectedException(typeof(InvalidFileContentException))]
        public void AddFile_Throws_When_File_Content_Is_Invalid(string fileName)
        {
            this.PrepareFileSecurityCheck();

            using (var fileContent = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources\\{fileName}")))
            {
                this._portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
                this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidSvgFileName)).Returns(true);

                this._mockFileManager.Object.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidSvgFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
            }
        }

        [Test]
        public void AddFile_No_Error_When_File_Content_Is_Valid()
        {
            this.PrepareFileSecurityCheck();

            using (var fileContent = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\valid.svg")))
            {
                this._portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
                this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidSvgFileName)).Returns(true);
                this._mockFileManager.Setup(mfm => mfm.IsImageFile(It.IsAny<IFileInfo>())).Returns(false);

                this._mockFileManager.Object.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidSvgFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
            }
        }

        [Test]
        public void AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwritting_And_File_Exists()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream();

            this._portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            this._globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);
            this._mockFolder.Setup(mf => mf.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent));
            this._mockFolder.Setup(mf => mf.GetHashCode(It.IsAny<IFileInfo>())).Returns("aaa");

            this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            this._mockFileManager.Setup(mfm => mfm.UpdateFile(It.IsAny<IFileInfo>(), It.IsAny<Stream>()));
            this._mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());

            this._workflowManager.Setup(wc => wc.GetWorkflow(It.IsAny<int>())).Returns((Workflow)null);

            this._mockData.Setup(
                md =>
                md.AddFile(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
               .Returns(Constants.FOLDER_ValidFileId);

            this._mockData.Setup(md => md.UpdateFileLastModificationTime(It.IsAny<int>(), It.IsAny<DateTime>()));

            this._mockFileManager.Object.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);

            this._mockFolder.Verify(mf => mf.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyFile_Throws_On_Null_File()
        {
            this._fileManager.CopyFile(null, this._folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyFile_Throws_On_Null_DestinationFolder()
        {
            this._fileManager.CopyFile(this._fileInfo.Object, null);
        }

        [Test]
        public void CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal()
        {
            const int sourceFolderMappingID = Constants.FOLDER_ValidFolderMappingID;
            const int destinationFolderMappingID = Constants.FOLDER_ValidFolderMappingID + 1;

            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.ContentType).Returns(Constants.CONTENTTYPE_ValidContentType);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(sourceFolderMappingID);

            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(destinationFolderMappingID);

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var fileContent = new MemoryStream(bytes);

            this._mockFileManager.Setup(mfm => mfm.GetFileContent(this._fileInfo.Object)).Returns(fileContent);
            this._mockFileManager.Setup(mfm => mfm.CopyContentItem(It.IsAny<int>())).Returns(Constants.CONTENT_ValidContentItemId);
            this._mockFileManager.Setup(mfm => mfm.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<Stream>(), true, true, Constants.CONTENTTYPE_ValidContentType));

            this._mockFileManager.Object.CopyFile(this._fileInfo.Object, this._folderInfo.Object);

            this._mockFileManager.Verify(fm => fm.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, true, Constants.CONTENTTYPE_ValidContentType), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Cannot_Add_Folder()
        {
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._folderPermissionController.Setup(fpc => fpc.CanAddFolder(this._folderInfo.Object)).Returns(false);

            this._fileManager.CopyFile(this._fileInfo.Object, this._folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(NoSpaceAvailableException))]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Portal_Has_No_Space_Available()
        {
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._fileInfo.Setup(fi => fi.Size).Returns(Constants.FOLDER_ValidFileSize);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            this._folderPermissionController.Setup(fpc => fpc.CanAddFolder(this._folderInfo.Object)).Returns(true);
            this._portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileSize)).Returns(false);

            this._fileManager.CopyFile(this._fileInfo.Object, this._folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteFile_Throws_On_Null_File()
        {
            this._fileManager.DeleteFile(null);
        }

        [Test]
        public void DeleteFile_Calls_FileDeletionControllerDeleteFile()
        {
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(this._fileInfo.Object)).Verifiable();

            this._mockFileManager.Object.DeleteFile(this._fileInfo.Object);

            this._mockFileDeletionController.Verify();
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void DeleteFile_Throws_WhenFileDeletionControllerThrows()
        {
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(this._fileInfo.Object))
                                       .Throws<FolderProviderException>();

            this._mockFileManager.Object.DeleteFile(this._fileInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DownloadFile_Throws_On_Null_File()
        {
            this._fileManager.WriteFileToResponse(null, ContentDisposition.Inline);
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void DownloadFile_Throws_When_Permissions_Are_Not_Met()
        {
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._folderPermissionController.Setup(fpc => fpc.CanViewFolder(this._folderInfo.Object)).Returns(false);

            this._fileManager.WriteFileToResponse(this._fileInfo.Object, ContentDisposition.Inline);
        }

        [Test]
        public void DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()
        {
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._folderPermissionController.Setup(fpc => fpc.CanViewFolder(this._folderInfo.Object)).Returns(true);

            this._mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(true);
            this._mockFileManager.Setup(mfm => mfm.AutoSyncFile(this._fileInfo.Object)).Verifiable();
            this._mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this._fileInfo.Object, It.IsAny<ContentDisposition>()));

            this._mockFileManager.Object.WriteFileToResponse(this._fileInfo.Object, ContentDisposition.Inline);

            this._mockFileManager.Verify();
        }

        [Test]
        public void DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()
        {
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._folderPermissionController.Setup(fpc => fpc.CanViewFolder(this._folderInfo.Object)).Returns(true);

            this._mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this._fileInfo.Object, It.IsAny<ContentDisposition>()));

            this._mockFileManager.Object.WriteFileToResponse(this._fileInfo.Object, ContentDisposition.Inline);

            this._mockFileManager.Verify(mfm => mfm.AutoSyncFile(this._fileInfo.Object), Times.Never());
        }

        [Test]
        public void DownloadFile_Calls_FileManager_WriteBytesToHttpContext()
        {
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._folderPermissionController.Setup(fpc => fpc.CanViewFolder(this._folderInfo.Object)).Returns(true);

            this._mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this._fileInfo.Object, It.IsAny<ContentDisposition>())).Verifiable();

            this._mockFileManager.Object.WriteFileToResponse(this._fileInfo.Object, ContentDisposition.Inline);

            this._mockFileManager.Verify();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            this._fileManager.FileExists(null, It.IsAny<string>());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExistsFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            this._fileManager.FileExists(this._folderInfo.Object, fileName);
        }

        [Test]
        public void ExistsFile_Calls_FileManager_GetFile()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._mockFileManager.Setup(mfm => mfm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns<IFileInfo>(null).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFileManager.Object.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._mockFileManager.Verify();
        }

        [Test]
        public void ExistsFile_Calls_FolderProvider_ExistsFile()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._mockFileManager.Setup(mfm => mfm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this._fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true).Verifiable();

            this._mockFileManager.Object.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._mockFolder.Verify();
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._mockFileManager.Setup(mfm => mfm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this._fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);

            var result = this._mockFileManager.Object.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._mockFileManager.Setup(mfm => mfm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this._fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);

            var result = this._mockFileManager.Object.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void ExistsFile_Throws_When_FolderProvider_Throws()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._mockFileManager.Setup(mfm => mfm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this._fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Throws<Exception>();

            this._mockFileManager.Object.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            this._fileManager.GetFile(this._folderInfo.Object, fileName);
        }

        [Test]
        public void GetFile_Calls_DataProvider_GetFile()
        {
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this._fileManager.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._mockData.Verify();
        }

        [Test]
        public void GetFile_Handles_Path_In_Portal_Root()
        {
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, string.Empty)).Returns(this._folderInfo.Object).Verifiable();
            this._mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this._fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileName);

            this._folderManager.Verify();
            this._mockData.Verify();
        }

        [Test]
        public void GetFile_Handles_Path_Beyond_Portal_Root()
        {
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this._folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object).Verifiable();
            this._mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this._fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath + Constants.FOLDER_ValidFileName);

            this._folderManager.Verify();
            this._mockData.Verify();
        }

        [Test]
        public void GetFileByID_Does_Not_Call_DataCache_GetCache_If_FileId_Is_Not_Valid()
        {
            this._mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(this._fileInfo.Object).Verifiable();

            this._fileManager.GetFile(Constants.FOLDER_InvalidFileId);

            this._mockCache.Verify(mc => mc.GetItem(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void GetFileByID_Calls_DataCache_GetCache_First()
        {
            this._mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(this._fileInfo.Object).Verifiable();

            this._fileManager.GetFile(Constants.FOLDER_ValidFileId);

            this._mockCache.Verify();
        }

        [Test]
        public void GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()
        {
            this._mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            this._fileManager.GetFile(Constants.FOLDER_ValidFileId);

            this._mockData.Verify(md => md.GetFileById(Constants.FOLDER_ValidFileId, It.IsAny<bool>()), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MoveFile_Throws_On_Null_File()
        {
            this._fileManager.MoveFile(null, this._folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MoveFile_Throws_On_Null_DestinationFolder()
        {
            this._fileManager.MoveFile(this._fileInfo.Object, null);
        }

        [Test]
        public void MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            this._mockFileManager.Setup(mfm => mfm.GetFileContent(this._fileInfo.Object)).Returns(fileContent);
            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(this._fileInfo.Object, out someString)).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.MoveVersions(this._fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));

            this._mockFolder.Setup(mf => mf.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();
            this._mockFolder.Setup(mf => mf.DeleteFile(this._fileInfo.Object)).Verifiable();

            this._mockFileManager.Setup(mfm => mfm.UpdateFile(this._fileInfo.Object)).Verifiable();

            this._mockFileManager.Object.MoveFile(this._fileInfo.Object, this._folderInfo.Object);

            this._mockFolder.Verify();
            this._mockFileManager.Verify();
        }

        [Test]
        public void MoveFile_Updates_FolderId_And_Folder()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._fileInfo.Setup(fi => fi.Folder).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_OtherValidFolderRelativePath);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._fileInfo.SetupSet(fi => fi.FolderId = Constants.FOLDER_OtherValidFolderId).Verifiable();
            this._fileInfo.SetupSet(fi => fi.Folder = Constants.FOLDER_OtherValidFolderRelativePath).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            this._mockFileManager.Setup(mfm => mfm.GetFileContent(this._fileInfo.Object)).Returns(fileContent);
            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(this._fileInfo.Object, out someString)).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.MoveVersions(this._fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));
            this._mockFileManager.Object.MoveFile(this._fileInfo.Object, this._folderInfo.Object);

            this._fileInfo.Verify();
        }

        [Test]
        public void MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            this._mockFileManager.Setup(mfm => mfm.GetFileContent(this._fileInfo.Object)).Returns(fileContent);
            string someString;
            this._mockFileLockingController.Setup(mflc => mflc.IsFileLocked(this._fileInfo.Object, out someString)).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.MoveVersions(this._fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));

            var existingFile = new FileInfo();
            this._mockFileManager.Setup(mfm => mfm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(existingFile);

            this._mockFileManager.Setup(mfm => mfm.DeleteFile(existingFile)).Verifiable();

            this._mockFileManager.Object.MoveFile(this._fileInfo.Object, this._folderInfo.Object);

            this._mockFileManager.Verify();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RenameFile_Throws_On_Null_File()
        {
            this._fileManager.RenameFile(null, It.IsAny<string>());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void RenameFile_Throws_On_Null_Or_Empty_NewFileName(string newFileName)
        {
            this._fileManager.RenameFile(this._fileInfo.Object, newFileName);
        }

        [Test]
        public void RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._mockFileManager.Setup(mfm => mfm.FileExists(this._folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.UpdateFile(this._fileInfo.Object));
            this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFileManager.Object.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            this._mockFolder.Verify(mf => mf.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName), Times.Once());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this._fileManager.RenameFile(this._fileInfo.Object, Constants.FOLDER_ValidFileName);

            this._mockFolder.Verify(mf => mf.RenameFile(this._fileInfo.Object, It.IsAny<string>()), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(FileAlreadyExistsException))]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._mockFileManager.Setup(mfm => mfm.FileExists(this._folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(true);
            this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            this._mockFileManager.Object.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName);
        }

        [Test]
        [ExpectedException(typeof(InvalidFileExtensionException))]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_InvalidExtensionType()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);
            this._mockFileManager.Setup(fm => fm.IsAllowedExtension(It.IsAny<string>())).Returns(false);

            this._mockFileManager.Object.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherInvalidFileNameExtension);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void RenameFile_Throws_When_FolderProvider_Throws()
        {
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);

            this._mockFileManager.Setup(mfm => mfm.FileExists(this._folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.UpdateFile(this._fileInfo.Object));
            this._mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._mockFolder.Setup(mf => mf.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName)).Throws<Exception>();

            this._mockFileManager.Object.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnzipFile_Throws_On_Null_File()
        {
            this._fileManager.UnzipFile(null, It.IsAny<IFolderInfo>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnzipFile_Throws_On_Null_DestinationFolder()
        {
            this._fileManager.UnzipFile(It.IsAny<IFileInfo>(), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnzipFile_Throws_When_File_Extension_Is_Not_Zip()
        {
            this._fileInfo.Setup(fi => fi.Extension).Returns("txt");

            this._fileManager.UnzipFile(this._fileInfo.Object, It.IsAny<IFolderInfo>());
        }

        [Test]
        public void UnzipFile_Calls_FileManager_ExtractFiles()
        {
            this._fileInfo.Setup(fi => fi.Extension).Returns("zip");

            this._mockFileManager.Setup(mfm => mfm.ExtractFiles(this._fileInfo.Object, this._folderInfo.Object, null)).Verifiable();

            this._mockFileManager.Object.UnzipFile(this._fileInfo.Object, this._folderInfo.Object);

            this._mockFileManager.Verify();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_File()
        {
            this._fileManager.UpdateFile(null);
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFile()
        {
            this._fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this._mockFileManager.Object.UpdateFile(this._fileInfo.Object);

            this._mockData.Verify(
                md => md.UpdateFile(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<bool>(),
                It.IsAny<int>()),
                Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_File_Overload()
        {
            this._fileManager.UpdateFile(null, It.IsAny<Stream>());
        }

        [Test]
        public void UpdateFile_Sets_With_And_Height_When_File_Is_Image()
        {
            var image = new Bitmap(10, 20);

            this._mockFileManager.Setup(mfm => mfm.IsImageFile(this._fileInfo.Object)).Returns(true);
            this._mockFileManager.Setup(mfm => mfm.GetImageFromStream(It.IsAny<Stream>())).Returns(image);
            this._mockFileManager.Setup(mfm => mfm.GetHash(this._fileInfo.Object));

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            this._fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this._folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this._mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this._mockFileManager.Object.UpdateFile(this._fileInfo.Object, stream);

            this._fileInfo.VerifySet(fi => fi.Width = 10);
            this._fileInfo.VerifySet(fi => fi.Height = 20);
        }

        [Test]
        public void UpdateFile_Sets_SHA1Hash()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            this._mockFileManager.Setup(mfm => mfm.IsImageFile(this._fileInfo.Object)).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.GetHash(stream)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this._fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this._folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this._mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this._mockFileManager.Object.UpdateFile(this._fileInfo.Object, stream);

            this._fileInfo.VerifySet(fi => fi.SHA1Hash = Constants.FOLDER_UnmodifiedFileHash);
        }

        [Test]
        public void UpdateFile_Calls_FileManager_UpdateFile_Overload()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            this._mockFileManager.Setup(mfm => mfm.IsImageFile(this._fileInfo.Object)).Returns(false);
            this._mockFileManager.Setup(mfm => mfm.GetHash(this._fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this._fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this._folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this._mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this._mockFileManager.Object.UpdateFile(this._fileInfo.Object, stream);

            this._mockFileManager.Verify(mfm => mfm.UpdateFile(this._fileInfo.Object), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSeekableStream_Throws_On_Null_Stream()
        {
            this._fileManager.GetSeekableStream(null);
        }

        [Test]
        public void GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable()
        {
            var inputStream = new MemoryStream();
            var seekableStream = this._fileManager.GetSeekableStream(inputStream);

            Assert.AreEqual(inputStream, seekableStream);
        }

        [Test]
        public void GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension()
        {
            var inputStream = new Mock<Stream>();
            inputStream.Setup(s => s.CanSeek).Returns(false);
            inputStream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);

            this._mockFileManager.Setup(mfm => mfm.GetHostMapPath()).Returns(string.Empty).Verifiable();
            this._mockFileManager.Setup(mfm => mfm.GetAutoDeleteFileStream(It.Is((string x) => x.EndsWith(".resx")))).Returns(new MemoryStream()).Verifiable();

            this._mockFileManager.Object.GetSeekableStream(inputStream.Object);

            this._mockFileManager.Verify();
        }

        private void PrepareFileSecurityCheck()
        {
            this._mockData.Setup(p => p.GetListEntriesByListName("FileSecurityChecker", string.Empty, Null.NullInteger)).Returns(() =>
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("EntryID", typeof(int));
                dataTable.Columns.Add("ListName", typeof(string));
                dataTable.Columns.Add("Value", typeof(string));
                dataTable.Columns.Add("Text", typeof(string));
                dataTable.Columns.Add("Level", typeof(int));
                dataTable.Columns.Add("SortOrder", typeof(int));
                dataTable.Columns.Add("DefinitionID", typeof(int));
                dataTable.Columns.Add("ParentID", typeof(int));
                dataTable.Columns.Add("Description", typeof(string));
                dataTable.Columns.Add("PortalID", typeof(int));
                dataTable.Columns.Add("SystemList", typeof(bool));
                dataTable.Columns.Add("ParentKey", typeof(string));
                dataTable.Columns.Add("Parent", typeof(string));
                dataTable.Columns.Add("ParentList", typeof(string));
                dataTable.Columns.Add("MaxSortOrder", typeof(int));
                dataTable.Columns.Add("EntryCount", typeof(int));
                dataTable.Columns.Add("HasChildren", typeof(int));
                dataTable.Columns.Add("CreatedByUserID", typeof(int));
                dataTable.Columns.Add("CreatedOnDate", typeof(DateTime));
                dataTable.Columns.Add("LastModifiedByUserID", typeof(int));
                dataTable.Columns.Add("LastModifiedOnDate", typeof(DateTime));

                dataTable.Rows.Add(1, "FileSecurityChecker", "svg",
                    "DotNetNuke.Services.FileSystem.Internal.SecurityCheckers.SvgFileChecker, DotNetNuke",
                    0, 0, -1, -0, string.Empty, -1, 1, string.Empty, string.Empty, string.Empty, 0, 1, 0, -1, DateTime.Now, -1, DateTime.Now);

                return dataTable.CreateDataReader();
            });
            this._hostController.Setup(c => c.GetString("PerformanceSetting")).Returns("NoCaching");
            this._globals.Setup(g => g.HostMapPath).Returns(AppDomain.CurrentDomain.BaseDirectory);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this._folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
        }

        private class UnSeekableStream : MemoryStream
        {
            public override bool CanSeek
            {
                get { return false; }
            }
        }
    }
}
