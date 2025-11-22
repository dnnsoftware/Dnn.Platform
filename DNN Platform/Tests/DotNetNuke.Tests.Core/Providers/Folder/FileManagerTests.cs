// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Text;

    using DotNetNuke.Abstractions.Application;
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
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [TestFixture]
    public class FileManagerTests
    {
        private FileManager fileManager;
        private Mock<IFolderManager> folderManager;
        private Mock<IFolderPermissionController> folderPermissionController;
        private Mock<IPortalController> portalController;
        private Mock<IFolderMappingController> folderMappingController;
        private Mock<IGlobals> globals;
        private Mock<ICBO> cbo;
        private Mock<DataProvider> mockData;
        private Mock<FolderProvider> mockFolder;
        private Mock<CachingProvider> mockCache;
        private Mock<FileManager> mockFileManager;
        private Mock<IFolderInfo> folderInfo;
        private Mock<IFileInfo> fileInfo;
        private Mock<IPathUtils> pathUtils;
        private Mock<IFileVersionController> fileVersionController;
        private Mock<IWorkflowManager> workflowManager;
        private Mock<IEventHandlersContainer<IFileEventHandlers>> fileEventHandlersContainer;
        private Mock<IFileLockingController> mockFileLockingController;
        private Mock<IFileDeletionController> mockFileDeletionController;
        private Mock<IHostController> hostController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.mockData = MockComponentProvider.CreateDataProvider();
            this.mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();

            this.folderManager = new Mock<IFolderManager>();
            this.folderPermissionController = new Mock<IFolderPermissionController>();
            this.portalController = new Mock<IPortalController>();
            this.hostController = new Mock<IHostController>();
            this.hostController.As<IHostSettingsService>();
            this.folderMappingController = new Mock<IFolderMappingController>();
            this.fileVersionController = new Mock<IFileVersionController>();
            this.workflowManager = new Mock<IWorkflowManager>();
            this.fileEventHandlersContainer = new Mock<IEventHandlersContainer<IFileEventHandlers>>();
            this.globals = new Mock<IGlobals>();
            this.cbo = new Mock<ICBO>();
            this.pathUtils = new Mock<IPathUtils>();
            this.mockFileLockingController = new Mock<IFileLockingController>();
            this.mockFileDeletionController = new Mock<IFileDeletionController>();

            EventLogController.SetTestableInstance(Mock.Of<IEventLogController>());
            FolderManager.RegisterInstance(this.folderManager.Object);
            FolderPermissionController.SetTestableInstance(this.folderPermissionController.Object);
            PortalController.SetTestableInstance(this.portalController.Object);
            FolderMappingController.RegisterInstance(this.folderMappingController.Object);
            TestableGlobals.SetTestableInstance(this.globals.Object);
            CBO.SetTestableInstance(this.cbo.Object);
            PathUtils.RegisterInstance(this.pathUtils.Object);
            FileVersionController.RegisterInstance(this.fileVersionController.Object);
            WorkflowManager.SetTestableInstance(this.workflowManager.Object);
            EventHandlersContainer<IFileEventHandlers>.RegisterInstance(this.fileEventHandlersContainer.Object);
            this.mockFileManager = new Mock<FileManager> { CallBase = true };

            this.folderInfo = new Mock<IFolderInfo>();
            this.fileInfo = new Mock<IFileInfo>();

            this.fileManager = new FileManager();

            FileLockingController.SetTestableInstance(this.mockFileLockingController.Object);
            FileDeletionController.SetTestableInstance(this.mockFileDeletionController.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockData.Object);
                    services.AddSingleton(this.mockFolder.Object);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(this.folderManager.Object);
                    services.AddSingleton(this.folderPermissionController.Object);
                    services.AddSingleton(this.portalController.Object);
                    services.AddSingleton(this.hostController.Object);
                    services.AddSingleton((IHostSettingsService)this.hostController.Object);
                    services.AddSingleton(this.folderMappingController.Object);
                    services.AddSingleton(this.fileVersionController.Object);
                    services.AddSingleton(this.workflowManager.Object);
                    services.AddSingleton(this.fileEventHandlersContainer.Object);
                    services.AddSingleton(this.globals.Object);
                    services.AddSingleton(this.cbo.Object);
                    services.AddSingleton(this.pathUtils.Object);
                    services.AddSingleton(this.mockFileLockingController.Object);
                    services.AddSingleton(this.mockFileDeletionController.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();

            FolderPermissionController.ClearInstance();
            FileLockingController.ClearInstance();
            FileDeletionController.ClearInstance();
            MockComponentProvider.ResetContainer();
            PortalController.ClearInstance();
        }

        [Test]
        public void AddFile_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.AddFile(null, It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            Assert.Throws<ArgumentException>(() => this.fileManager.AddFile(this.folderInfo.Object, fileName, It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()));
        }

        [Test]
        public void AddFile_Throws_On_Null_FileContent()
        {
            Assert.Throws<ArgumentException>(() => this.fileManager.AddFile(this.folderInfo.Object, It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()));
        }

        [Test]
        public void AddFile_Throws_When_Permissions_Are_Not_Met()
        {
            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(false);

            Assert.Throws<PermissionsNotMetException>(() => this.fileManager.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(), It.IsAny<bool>(), true, It.IsAny<string>()));
        }

        [Test]
        public void AddFile_Throws_When_Portal_Has_No_Space_Available()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockData.Setup(c => c.GetProviderPath()).Returns(string.Empty);

            var fileContent = new MemoryStream();

            this.globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(false);

            this.mockFileManager.Setup(fm => fm.CreateFileContentItem()).Returns(new ContentItem());
            this.mockFileManager.Setup(fm => fm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);

            Assert.Throws<NoSpaceAvailableException>(() => this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType));
        }

        [Test]
        public void AddFile_Checks_Space_For_Stream_Length()
        {
            // Arrange
            this.PrepareFileSecurityCheck();
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream(Encoding.ASCII.GetBytes("some data here"));

            this.hostController.Setup(c => c.GetString("FileExtensions")).Returns("");

            this.portalController.Setup(pc => pc.HasSpaceAvailable(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            this.globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);
            this.mockFolder.Setup(mf => mf.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();

            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());
            this.mockFileManager.Setup(mfm => mfm.IsImageFile(It.IsAny<IFileInfo>())).Returns(false);

            this.workflowManager.Setup(we => we.GetWorkflow(It.IsAny<int>())).Returns((Workflow)null);

            // Act
            this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, false, Constants.CONTENTTYPE_ValidContentType);

            // Assert
            this.portalController.Verify(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length));
        }

        [Test]
        public void AddFile_Throws_When_Extension_Is_Invalid()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var fileContent = new MemoryStream();

            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(false);

            Assert.Throws<InvalidFileExtensionException>(() => this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType));
        }

        [TestCase("invalid_script.svg")]
        [TestCase("invalid_namespaced-script.svg")]
        [TestCase("invalid_onload.svg")]
        [TestCase("invalid_onload-uppercase.svg")]
        [TestCase("invalid_onerror.svg")]
        [TestCase("invalid_onerror-uppercase.svg")]
        [TestCase("invalid_foreignObject-iframe-src-data.svg")]
        [TestCase("invalid_foreignObject-iframe-srcdoc.svg")]
        [TestCase("DOMPurify/invalid_attribute-mXSS_1.svg")]
        [TestCase("DOMPurify/invalid_attribute-mXSS_2.svg")]
        [TestCase("DOMPurify/invalid_embedded-MathML.svg")]
        [TestCase("DOMPurify/invalid_fake-element-based-namespace-confusion.svg")]
        [TestCase("DOMPurify/invalid_mXSS-Chrome-77_1.svg")]
        [TestCase("DOMPurify/invalid_mXSS-Chrome-77_2.svg")]
        [TestCase("DOMPurify/invalid_mXSS-template-Chrome-77.svg")]
        [TestCase("XSS-Payloads/Password_steal.svg")]
        [TestCase("XSS-Payloads/hero-xss.svg")]
        [TestCase("XSS-Payloads/xss-ww.svg")]
        [TestCase("XSS_SCRIPTS/desc.svg")]
        [TestCase("XSS_SCRIPTS/foreignObject.svg")]
        [TestCase("XSS_SCRIPTS/onload.svg")]
        [TestCase("XSS_SCRIPTS/title.svg")]
        [TestCase("OWASP/chameleon.svg")]
        [TestCase("OWASP/foreignObject.svg")]
        [TestCase("OWASP/handler.svg")]
        [TestCase("OWASP/href.svg")]
        [TestCase("OWASP/onload.svg")]
        [TestCase("OWASP/set.svg")]
        public void AddFile_Throws_When_File_Content_Is_Invalid(string fileName)
        {
            this.PrepareFileSecurityCheck();

            using (var fileContent = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources/{fileName}")))
            {
                this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
                this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidSvgFileName)).Returns(true);

                Assert.Throws<InvalidFileContentException>(() => this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidSvgFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType));
            }
        }

        [TestCase("valid.svg")]
        [TestCase("DOMPurify/valid_data-URI.svg")]
        [TestCase("DOMPurify/valid_data-URI-href.svg")]
        [TestCase("DOMPurify/valid_filter.svg")]
        public void AddFile_No_Error_When_File_Content_Is_Valid(string fileName)
        {
            this.PrepareFileSecurityCheck();

            using (var fileContent = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources/{fileName}")))
            {
                this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
                this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidSvgFileName)).Returns(true);
                this.mockFileManager.Setup(mfm => mfm.IsImageFile(It.IsAny<IFileInfo>())).Returns(false);
                this.hostController.Setup(c => c.GetString("FileExtensions")).Returns("");

                this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidSvgFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
            }
        }

        [Test]
        public void AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwritting_And_File_Exists()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream();

            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            this.globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);
            this.mockFolder.Setup(mf => mf.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent));
            this.mockFolder.Setup(mf => mf.GetHashCode(It.IsAny<IFileInfo>())).Returns("aaa");

            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(It.IsAny<IFileInfo>(), It.IsAny<Stream>()));
            this.mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());

            this.workflowManager.Setup(wc => wc.GetWorkflow(It.IsAny<int>())).Returns((Workflow)null);

            this.mockData.Setup(
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

            this.mockData.Setup(md => md.UpdateFileLastModificationTime(It.IsAny<int>(), It.IsAny<DateTime>()));

            this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);

            this.mockFolder.Verify(mf => mf.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());
        }

        [Test]
        public void CopyFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.CopyFile(null, this.folderInfo.Object));
        }

        [Test]
        public void CopyFile_Throws_On_Null_DestinationFolder()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.CopyFile(this.fileInfo.Object, null));
        }

        [Test]
        public void CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal()
        {
            // Arrange
            const int sourceFolderMappingID = Constants.FOLDER_ValidFolderMappingID;
            const int destinationFolderMappingID = Constants.FOLDER_ValidFolderMappingID + 1;
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.ContentType).Returns(Constants.CONTENTTYPE_ValidContentType);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(sourceFolderMappingID);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(destinationFolderMappingID);
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var fileContent = new MemoryStream(bytes);
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
            this.mockFileManager.Setup(mfm => mfm.CopyContentItem(It.IsAny<int>())).Returns(Constants.CONTENT_ValidContentItemId);
            this.mockFileManager.Setup(mfm => mfm.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<Stream>(), true, true, Constants.CONTENTTYPE_ValidContentType))
                .Returns(this.fileInfo.Object);
            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(true);

            // Act
            this.mockFileManager.Object.CopyFile(this.fileInfo.Object, this.folderInfo.Object);

            // Assert
            this.mockFileManager.Verify(fm => fm.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, true, Constants.CONTENTTYPE_ValidContentType), Times.Once());
        }

        [Test]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Cannot_Add_Folder()
        {
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(false);

            Assert.Throws<PermissionsNotMetException>(() => this.fileManager.CopyFile(this.fileInfo.Object, this.folderInfo.Object));
        }

        [Test]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Portal_Has_No_Space_Available()
        {
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.Size).Returns(Constants.FOLDER_ValidFileSize);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(true);
            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileSize)).Returns(false);

            Assert.Throws<NoSpaceAvailableException>(() => this.fileManager.CopyFile(this.fileInfo.Object, this.folderInfo.Object));
        }

        [Test]
        public void DeleteFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.DeleteFile(null));
        }

        [Test]
        public void DeleteFile_Calls_FileDeletionControllerDeleteFile()
        {
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(this.fileInfo.Object)).Verifiable();

            this.mockFileManager.Object.DeleteFile(this.fileInfo.Object);

            this.mockFileDeletionController.Verify();
        }

        [Test]
        public void DeleteFile_Throws_WhenFileDeletionControllerThrows()
        {
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(this.fileInfo.Object))
                                       .Throws<FolderProviderException>();

            Assert.Throws<FolderProviderException>(() => this.mockFileManager.Object.DeleteFile(this.fileInfo.Object));
        }

        [Test]
        public void DownloadFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.WriteFileToResponse(null, ContentDisposition.Inline));
        }

        [Test]
        public void DownloadFile_Throws_When_Permissions_Are_Not_Met()
        {
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(false);

            Assert.Throws<PermissionsNotMetException>(() => this.fileManager.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline));
        }

        [Test]
        public void DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()
        {
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.AutoSyncFile(this.fileInfo.Object)).Verifiable();
            this.mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this.fileInfo.Object, It.IsAny<ContentDisposition>()));

            this.mockFileManager.Object.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline);

            this.mockFileManager.Verify();
        }

        [Test]
        public void DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()
        {
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this.fileInfo.Object, It.IsAny<ContentDisposition>()));

            this.mockFileManager.Object.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline);

            this.mockFileManager.Verify(mfm => mfm.AutoSyncFile(this.fileInfo.Object), Times.Never());
        }

        [Test]
        public void DownloadFile_Calls_FileManager_WriteBytesToHttpContext()
        {
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this.fileInfo.Object, It.IsAny<ContentDisposition>())).Verifiable();

            this.mockFileManager.Object.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline);

            this.mockFileManager.Verify();
        }

        [Test]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.FileExists(null, It.IsAny<string>()));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void ExistsFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            Assert.Throws<ArgumentException>(() => this.fileManager.FileExists(this.folderInfo.Object, fileName));
        }

        [Test]
        public void ExistsFile_Calls_FileManager_GetFile()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns<IFileInfo>(null).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockFileManager.Verify();
        }

        [Test]
        public void ExistsFile_Calls_FolderProvider_ExistsFile()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true).Verifiable();

            this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockFolder.Verify();
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);

            var result = this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);

            var result = this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsFile_Throws_When_FolderProvider_Throws()
        {
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Throws<Exception>();

            Assert.Throws<FolderProviderException>(() => this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void GetFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            Assert.Throws<ArgumentException>(() => this.fileManager.GetFile(this.folderInfo.Object, fileName));
        }

        [Test]
        public void GetFile_Calls_DataProvider_GetFile()
        {
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this.mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this.fileManager.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockData.Verify();
        }

        [Test]
        public void GetFile_Handles_Path_In_Portal_Root()
        {
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, string.Empty)).Returns(this.folderInfo.Object).Verifiable();
            this.mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this.fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileName);

            this.folderManager.Verify();
            this.mockData.Verify();
        }

        [Test]
        public void GetFile_Handles_Path_Beyond_Portal_Root()
        {
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this.folderInfo.Object).Verifiable();
            this.mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this.fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath + Constants.FOLDER_ValidFileName);

            this.folderManager.Verify();
            this.mockData.Verify();
        }

        [Test]
        public void GetFileByID_Does_Not_Call_DataCache_GetCache_If_FileId_Is_Not_Valid()
        {
            this.mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(this.fileInfo.Object).Verifiable();

            this.fileManager.GetFile(Constants.FOLDER_InvalidFileId);

            this.mockCache.Verify(mc => mc.GetItem(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void GetFileByID_Calls_DataCache_GetCache_First()
        {
            this.mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(this.fileInfo.Object).Verifiable();

            this.fileManager.GetFile(Constants.FOLDER_ValidFileId);

            this.mockCache.Verify();
        }

        [Test]
        public void GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()
        {
            this.mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            this.fileManager.GetFile(Constants.FOLDER_ValidFileId);

            this.mockData.Verify(md => md.GetFileById(Constants.FOLDER_ValidFileId, It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void MoveFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.MoveFile(null, this.folderInfo.Object));
        }

        [Test]
        public void MoveFile_Throws_On_Null_DestinationFolder()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.MoveFile(this.fileInfo.Object, null));
        }

        [Test]
        public void MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile()
        {
            // Arrange
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            var fileContent = new MemoryStream();
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(this.fileInfo.Object, out someString)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.MoveVersions(this.fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));
            this.mockFolder.Setup(mf => mf.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();
            this.mockFolder.Setup(mf => mf.DeleteFile(this.fileInfo.Object)).Verifiable();
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(this.fileInfo.Object)).Verifiable();

            // Act
            this.mockFileManager.Object.MoveFile(this.fileInfo.Object, this.folderInfo.Object);

            // Assert
            this.mockFolder.Verify();
            this.mockFileManager.Verify();
        }

        [Test]
        public void MoveFile_Updates_FolderId_And_Folder()
        {
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.Folder).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_OtherValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.fileInfo.SetupSet(fi => fi.FolderId = Constants.FOLDER_OtherValidFolderId).Verifiable();
            this.fileInfo.SetupSet(fi => fi.Folder = Constants.FOLDER_OtherValidFolderRelativePath).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(this.fileInfo.Object, out someString)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.MoveVersions(this.fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));
            this.mockFileManager.Object.MoveFile(this.fileInfo.Object, this.folderInfo.Object);

            this.fileInfo.Verify();
        }

        [Test]
        public void MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder()
        {
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(this.fileInfo.Object, out someString)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.MoveVersions(this.fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));

            var existingFile = new FileInfo();
            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(existingFile);

            this.mockFileManager.Setup(mfm => mfm.DeleteFile(existingFile)).Verifiable();

            this.mockFileManager.Object.MoveFile(this.fileInfo.Object, this.folderInfo.Object);

            this.mockFileManager.Verify();
        }

        [Test]
        public void RenameFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.RenameFile(null, It.IsAny<string>()));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void RenameFile_Throws_On_Null_Or_Empty_NewFileName(string newFileName)
        {
            Assert.Throws<ArgumentException>(() => this.fileManager.RenameFile(this.fileInfo.Object, newFileName));
        }

        [Test]
        public void RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()
        {
            // Arrange
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);
            this.mockFileManager.Setup(mfm => mfm.FileExists(this.folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(this.fileInfo.Object));
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);
            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            // Act
            this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            // Assert
            this.mockFolder.Verify(mf => mf.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName), Times.Once());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()
        {
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this.fileManager.RenameFile(this.fileInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockFolder.Verify(mf => mf.RenameFile(this.fileInfo.Object, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()
        {
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.mockFileManager.Setup(mfm => mfm.FileExists(this.folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            Assert.Throws<FileAlreadyExistsException>(() => this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName));
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_InvalidExtensionType()
        {
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);
            this.mockFileManager.Setup(fm => fm.IsAllowedExtension(It.IsAny<string>())).Returns(false);

            Assert.Throws<InvalidFileExtensionException>(() => this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherInvalidFileNameExtension));
        }

        [Test]
        public void RenameFile_Throws_When_FolderProvider_Throws()
        {
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.mockFileManager.Setup(mfm => mfm.FileExists(this.folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(this.fileInfo.Object));
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName)).Throws<Exception>();

            Assert.Throws<FolderProviderException>(() => this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName));
        }

        [Test]
        public void UnzipFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UnzipFile(null, It.IsAny<IFolderInfo>()));
        }

        [Test]
        public void UnzipFile_Throws_On_Null_DestinationFolder()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UnzipFile(It.IsAny<IFileInfo>(), null));
        }

        [Test]
        public void UnzipFile_Throws_When_File_Extension_Is_Not_Zip()
        {
            this.fileInfo.Setup(fi => fi.Extension).Returns("txt");

            Assert.Throws<ArgumentNullException>(() => this.fileManager.UnzipFile(this.fileInfo.Object, It.IsAny<IFolderInfo>()));
        }

        [Test]
        public void UnzipFile_Calls_FileManager_ExtractFiles()
        {
            // Arrange
            this.fileInfo.Setup(fi => fi.Extension).Returns("zip");
            this.mockFileManager.Setup(mfm => mfm.ExtractFiles(this.fileInfo.Object, this.folderInfo.Object, null, true)).Verifiable();
            var stream = Constants.ValidZipFileContent;
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(stream);

            // Act
            this.mockFileManager.Object.UnzipFile(this.fileInfo.Object, this.folderInfo.Object);

            // Assert
            this.mockFileManager.Verify();
        }

        [Test]
        public void UpdateFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UpdateFile(null));
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFile()
        {
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object);

            this.mockData.Verify(
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
        public void UpdateFile_Throws_On_Null_File_Overload()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UpdateFile(null, It.IsAny<Stream>()));
        }

        [Test]
        public void UpdateFile_Sets_With_And_Height_When_File_Is_Image()
        {
            var image = new Bitmap(10, 20);

            this.mockFileManager.Setup(mfm => mfm.IsImageFile(this.fileInfo.Object)).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.GetImageFromStream(It.IsAny<Stream>())).Returns(image);
            this.mockFileManager.Setup(mfm => mfm.GetHash(this.fileInfo.Object));

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this.mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object, stream);

            this.fileInfo.VerifySet(fi => fi.Width = 10);
            this.fileInfo.VerifySet(fi => fi.Height = 20);
        }

        [Test]
        public void UpdateFile_Sets_SHA1Hash()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            this.mockFileManager.Setup(mfm => mfm.IsImageFile(this.fileInfo.Object)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.GetHash(stream)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this.mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object, stream);

            this.fileInfo.VerifySet(fi => fi.SHA1Hash = Constants.FOLDER_UnmodifiedFileHash);
        }

        [Test]
        public void UpdateFile_Calls_FileManager_UpdateFile_Overload()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            this.mockFileManager.Setup(mfm => mfm.IsImageFile(this.fileInfo.Object)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.GetHash(this.fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this.mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object, stream);

            this.mockFileManager.Verify(mfm => mfm.UpdateFile(this.fileInfo.Object), Times.Once());
        }

        [Test]
        public void GetSeekableStream_Throws_On_Null_Stream()
        {
            Assert.Throws<ArgumentNullException>(() => this.fileManager.GetSeekableStream(null));
        }

        [Test]
        public void GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable()
        {
            var inputStream = new MemoryStream();
            var seekableStream = this.fileManager.GetSeekableStream(inputStream);

            Assert.That(seekableStream, Is.EqualTo(inputStream));
        }

        [Test]
        public void GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension()
        {
            var inputStream = new Mock<Stream>();
            inputStream.Setup(s => s.CanSeek).Returns(false);
            inputStream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);

            this.mockFileManager.Setup(mfm => mfm.GetHostMapPath()).Returns(string.Empty).Verifiable();
            this.mockFileManager.Setup(mfm => mfm.GetAutoDeleteFileStream(It.Is((string x) => x.EndsWith(".resx")))).Returns(new MemoryStream()).Verifiable();

            this.mockFileManager.Object.GetSeekableStream(inputStream.Object);

            this.mockFileManager.Verify();
        }

        private void PrepareFileSecurityCheck()
        {
            this.mockData.Setup(p => p.GetListEntriesByListName("FileSecurityChecker", string.Empty, Null.NullInteger)).Returns(() =>
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
            this.hostController.Setup(c => c.GetString("PerformanceSetting")).Returns("NoCaching");
            this.globals.Setup(g => g.HostMapPath).Returns(AppDomain.CurrentDomain.BaseDirectory);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
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
