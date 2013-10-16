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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class FileManagerTests
    {
        #region Private Variables

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
        private Mock<IContentWorkflowController> _contentWorkflowController;
        private Mock<IFileEventHandlersContainer> _fileEventHandlersContainer;
        private Mock<IFileLockingController> _mockFileLockingController;
        private Mock<IFileDeletionController> _mockFileDeletionController;

        #endregion

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
            _mockData = MockComponentProvider.CreateDataProvider();
            _mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
            _mockCache = MockComponentProvider.CreateDataCacheProvider();

            _folderManager = new Mock<IFolderManager>();
            _folderPermissionController = new Mock<IFolderPermissionController>();
            _portalController = new Mock<IPortalController>();
            _folderMappingController = new Mock<IFolderMappingController>();
            _fileVersionController = new Mock<IFileVersionController>();
            _contentWorkflowController = new Mock<IContentWorkflowController>();
            _fileEventHandlersContainer = new Mock<IFileEventHandlersContainer>();
            _globals = new Mock<IGlobals>();
            _cbo = new Mock<ICBO>();
            _pathUtils = new Mock<IPathUtils>();
            _mockFileLockingController = new Mock<IFileLockingController>();
            _mockFileDeletionController = new Mock<IFileDeletionController>();
            
            FolderManager.RegisterInstance(_folderManager.Object);
            FolderPermissionControllerWrapper.RegisterInstance(_folderPermissionController.Object);
            PortalControllerWrapper.RegisterInstance(_portalController.Object);
            FolderMappingController.RegisterInstance(_folderMappingController.Object);
            TestableGlobals.SetTestableInstance(_globals.Object);
            CBOWrapper.RegisterInstance(_cbo.Object);
            PathUtils.RegisterInstance(_pathUtils.Object);
            FileVersionController.RegisterInstance(_fileVersionController.Object);
            ContentWorkflowController.RegisterInstance(_contentWorkflowController.Object);
            FileEventHandlersContainer.RegisterInstance(_fileEventHandlersContainer.Object);
            _mockFileManager = new Mock<FileManager> { CallBase = true };

            _folderInfo = new Mock<IFolderInfo>();
            _fileInfo = new Mock<IFileInfo>();

            _fileManager = new FileManager();

            FileLockingController.SetTestableInstance(_mockFileLockingController.Object);
            FileDeletionController.SetTestableInstance(_mockFileDeletionController.Object);
        }

        [TearDown]
        public void TearDown()
        {
            FileLockingController.ClearInstance();
            FileDeletionController.ClearInstance();
            MockComponentProvider.ResetContainer();
        }

        #endregion

        #region AddFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddFile_Throws_On_Null_Folder()
        {
            _fileManager.AddFile(null, It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            _fileManager.AddFile(_folderInfo.Object, fileName, It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_Null_FileContent()
        {
            _fileManager.AddFile(_folderInfo.Object, It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void AddFile_Throws_When_Permissions_Are_Not_Met()
        {
            _folderPermissionController.Setup(fpc => fpc.CanAddFolder(_folderInfo.Object)).Returns(false);

            _fileManager.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(), It.IsAny<bool>(), true, It.IsAny<string>());
        }

        [Test]
        [ExpectedException(typeof(NoSpaceAvailableException))]
        public void AddFile_Throws_When_Portal_Has_No_Space_Available()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _mockData.Setup(c => c.GetProviderPath()).Returns(String.Empty);

            var fileContent = new MemoryStream();

            _globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(false);
            
            _mockFileManager.Setup(fm => fm.CreateFileContentItem()).Returns(new ContentItem());

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
        }

        [Test]
        public void AddFile_Checks_Space_For_Stream_Length()
        {
            //Arrange
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream(Encoding.ASCII.GetBytes("some data here"));

            _portalController.Setup(pc => pc.HasSpaceAvailable(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            _globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);


            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);
            _mockFolder.Setup(mf => mf.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            _mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());

            _contentWorkflowController.Setup(wc => wc.GetWorkflowByID(It.IsAny<int>())).Returns((ContentWorkflow)null);

            //Act
            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, false, Constants.CONTENTTYPE_ValidContentType);

            //Assert
            _portalController.Verify(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length));
        }

        class UnSeekableStream : MemoryStream
        {
            public override bool CanSeek
            {
                get { return false; }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidFileExtensionException))]
        public void AddFile_Throws_When_Extension_Is_Invalid()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(false);

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
        }

        [Test]
        public void AddFile_Calls_FolderProvider_AddFile_When_Overwritting_Or_File_Does_Not_Exist()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            _globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);
            _mockFolder.Setup(mf => mf.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            _mockFileManager.Setup(mfm => mfm.UpdateFile(It.IsAny<IFileInfo>(), It.IsAny<Stream>()));
            _mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());

            _contentWorkflowController.Setup(wc => wc.GetWorkflowByID(It.IsAny<int>())).Returns((ContentWorkflow)null);

            _mockData.Setup(
                md =>
                md.AddFile(It.IsAny<int>(),
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
                           It.IsAny<DateTime>(),
                           It.IsAny<DateTime>(),
                           It.IsAny<bool>(),
                           It.IsAny<int>()))
               .Returns(Constants.FOLDER_ValidFileId);

            _mockData.Setup(md => md.UpdateFileLastModificationTime(It.IsAny<int>(), It.IsAny<DateTime>()));

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, false, Constants.CONTENTTYPE_ValidContentType);

            _mockFolder.Verify();
        }

        [Test]
        public void AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwritting_And_File_Exists()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream();

            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            _globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);
            _mockFolder.Setup(mf => mf.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent));

            _mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            _mockFileManager.Setup(mfm => mfm.UpdateFile(It.IsAny<IFileInfo>(), It.IsAny<Stream>()));
            _mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());

            _contentWorkflowController.Setup(wc => wc.GetWorkflowByID(It.IsAny<int>())).Returns((ContentWorkflow)null);

            _mockData.Setup(
                md =>
                md.AddFile(It.IsAny<int>(),
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
                           It.IsAny<DateTime>(),
                           It.IsAny<DateTime>(),
                           It.IsAny<bool>(),
                           It.IsAny<int>()))
               .Returns(Constants.FOLDER_ValidFileId);
            
            _mockData.Setup(md => md.UpdateFileLastModificationTime(It.IsAny<int>(), It.IsAny<DateTime>()));

            _mockFileManager.Object.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);

            _mockFolder.Verify(mf => mf.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());
        }
        
        #endregion

        #region CopyFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyFile_Throws_On_Null_File()
        {
            _fileManager.CopyFile(null, _folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyFile_Throws_On_Null_DestinationFolder()
        {
            _fileManager.CopyFile(_fileInfo.Object, null);
        }

        [Test]
        public void CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal()
        {
            const int sourceFolderMappingID = Constants.FOLDER_ValidFolderMappingID;
            const int destinationFolderMappingID = Constants.FOLDER_ValidFolderMappingID + 1;

            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.ContentType).Returns(Constants.CONTENTTYPE_ValidContentType);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(sourceFolderMappingID);

            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(destinationFolderMappingID);

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var fileContent = new MemoryStream(bytes);

            _mockFileManager.Setup(mfm => mfm.GetFileContent(_fileInfo.Object)).Returns(fileContent);
            _mockFileManager.Setup(mfm => mfm.CopyContentItem(It.IsAny<int>())).Returns(Constants.CONTENT_ValidContentItemId);
            _mockFileManager.Setup(mfm => mfm.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<Stream>(), true, true, Constants.CONTENTTYPE_ValidContentType));

            _mockFileManager.Object.CopyFile(_fileInfo.Object, _folderInfo.Object);

            _mockFileManager.Verify(fm => fm.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, true, Constants.CONTENTTYPE_ValidContentType), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Cannot_Add_Folder()
        {
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _folderPermissionController.Setup(fpc => fpc.CanAddFolder(_folderInfo.Object)).Returns(false);

            _fileManager.CopyFile(_fileInfo.Object, _folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(NoSpaceAvailableException))]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Portal_Has_No_Space_Available()
        {
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _fileInfo.Setup(fi => fi.Size).Returns(Constants.FOLDER_ValidFileSize);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            _folderPermissionController.Setup(fpc => fpc.CanAddFolder(_folderInfo.Object)).Returns(true);
            _portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileSize)).Returns(false);

            _fileManager.CopyFile(_fileInfo.Object, _folderInfo.Object);
        }

        #endregion

        #region DeleteFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteFile_Throws_On_Null_File()
        {
            _fileManager.DeleteFile(null);
        }

        [Test]
        public void DeleteFile_Calls_FileDeletionControllerDeleteFile()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(_fileInfo.Object)).Verifiable();

            _mockFileManager.Object.DeleteFile(_fileInfo.Object);

            _mockFileDeletionController.Verify();
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void DeleteFile_Throws_WhenFileDeletionControllerThrows()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _mockFileDeletionController.Setup(mfdc => mfdc.DeleteFile(_fileInfo.Object))
                                       .Throws<FolderProviderException>();

            _mockFileManager.Object.DeleteFile(_fileInfo.Object);
        }

        #endregion

        #region WriteFileToResponse

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DownloadFile_Throws_On_Null_File()
        {
            _fileManager.WriteFileToResponse(null, ContentDisposition.Inline);
        }

        [Test]
        [ExpectedException(typeof(PermissionsNotMetException))]
        public void DownloadFile_Throws_When_Permissions_Are_Not_Met()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(false);

            _fileManager.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);
        }

        [Test]
        public void DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(true);
            _mockFileManager.Setup(mfm => mfm.AutoSyncFile(_fileInfo.Object)).Verifiable();
            _mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(_fileInfo.Object, It.IsAny<ContentDisposition>()));

            _mockFileManager.Object.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);

            _mockFileManager.Verify();
        }

        [Test]
        public void DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            _mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(_fileInfo.Object, It.IsAny<ContentDisposition>()));

            _mockFileManager.Object.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);

            _mockFileManager.Verify(mfm => mfm.AutoSyncFile(_fileInfo.Object), Times.Never());
        }

        [Test]
        public void DownloadFile_Calls_FileManager_WriteBytesToHttpContext()
        {
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _folderPermissionController.Setup(fpc => fpc.CanViewFolder(_folderInfo.Object)).Returns(true);

            _mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            _mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(_fileInfo.Object, It.IsAny<ContentDisposition>())).Verifiable();

            _mockFileManager.Object.WriteFileToResponse(_fileInfo.Object, ContentDisposition.Inline);

            _mockFileManager.Verify();
        }

        #endregion

        #region FileExists

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            _fileManager.FileExists(null, It.IsAny<string>());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExistsFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            _fileManager.FileExists(_folderInfo.Object, fileName);
        }

        [Test]
        public void ExistsFile_Calls_FileManager_GetFile()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns<IFileInfo>(null).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _mockFileManager.Verify();
        }

        [Test]
        public void ExistsFile_Calls_FolderProvider_ExistsFile()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true).Verifiable();

            _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _mockFolder.Verify();
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);

            var result = _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);

            var result = _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void ExistsFile_Throws_When_FolderProvider_Throws()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(_fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Throws<Exception>();

            _mockFileManager.Object.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);
        }

        #endregion

        #region GetFile

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
            _fileManager.GetFile(_folderInfo.Object, fileName);
        }

        [Test]
        public void GetFile_Calls_DataProvider_GetFile()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            _fileManager.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _mockData.Verify();
        }

        [Test]
        public void GetFile_Handles_Path_In_Portal_Root()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, "")).Returns(_folderInfo.Object).Verifiable();
            _mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            _fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileName);

            _folderManager.Verify();
            _mockData.Verify();
        }

        [Test]
        public void GetFile_Handles_Path_Beyond_Portal_Root()
        {
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            _folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object).Verifiable();
            _mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            _fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath + Constants.FOLDER_ValidFileName);

            _folderManager.Verify();
            _mockData.Verify();
        }

        #endregion

        #region GetFile

        [Test]
        public void GetFileByID_Does_Not_Call_DataCache_GetCache_If_FileId_Is_Not_Valid()
        {
            _mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(_fileInfo.Object).Verifiable();

            _fileManager.GetFile(Constants.FOLDER_InvalidFileId);

            _mockCache.Verify(mc => mc.GetItem(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void GetFileByID_Calls_DataCache_GetCache_First()
        {
            _mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(_fileInfo.Object).Verifiable();

            _fileManager.GetFile(Constants.FOLDER_ValidFileId);

            _mockCache.Verify();
        }

        [Test]
        public void GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()
        {
            _mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            _fileManager.GetFile(Constants.FOLDER_ValidFileId);

            _mockData.Verify(md => md.GetFileById(Constants.FOLDER_ValidFileId, It.IsAny<bool>()), Times.Once());
        }

        #endregion

        #region MoveFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MoveFile_Throws_On_Null_File()
        {
            _fileManager.MoveFile(null, _folderInfo.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MoveFile_Throws_On_Null_DestinationFolder()
        {
            _fileManager.MoveFile(_fileInfo.Object, null);
        }

        [Test]
        public void MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            _mockFileManager.Setup(mfm => mfm.GetFileContent(_fileInfo.Object)).Returns(fileContent);
            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(_fileInfo.Object, out someString)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.MoveVersions(_fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));

            _mockFolder.Setup(mf => mf.AddFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();
            _mockFolder.Setup(mf => mf.DeleteFile(_fileInfo.Object)).Verifiable();

            _mockFileManager.Setup(mfm => mfm.UpdateFile(_fileInfo.Object)).Verifiable();

            _mockFileManager.Object.MoveFile(_fileInfo.Object, _folderInfo.Object);

            _mockFolder.Verify();
            _mockFileManager.Verify();
        }

        [Test]
        public void MoveFile_Updates_FolderId_And_Folder()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.Folder).Returns(Constants.FOLDER_ValidFolderRelativePath);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_OtherValidFolderRelativePath);
            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _fileInfo.SetupSet(fi => fi.FolderId = Constants.FOLDER_OtherValidFolderId).Verifiable();
            _fileInfo.SetupSet(fi => fi.Folder = Constants.FOLDER_OtherValidFolderRelativePath).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            _mockFileManager.Setup(mfm => mfm.GetFileContent(_fileInfo.Object)).Returns(fileContent);
            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(_fileInfo.Object, out someString)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.MoveVersions(_fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));
            _mockFileManager.Object.MoveFile(_fileInfo.Object, _folderInfo.Object);

            _fileInfo.Verify();
        }

        [Test]
        public void MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            _folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

            _mockFileManager.Setup(mfm => mfm.GetFileContent(_fileInfo.Object)).Returns(fileContent);
            string someString;
            _mockFileLockingController.Setup(mflc => mflc.IsFileLocked(_fileInfo.Object, out someString)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.MoveVersions(_fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));

            var existingFile = new FileInfo();
            _mockFileManager.Setup(mfm => mfm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(existingFile);

            _mockFileManager.Setup(mfm => mfm.DeleteFile(existingFile)).Verifiable();

            _mockFileManager.Object.MoveFile(_fileInfo.Object, _folderInfo.Object);

            _mockFileManager.Verify();
        }

        #endregion

        #region RenameFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RenameFile_Throws_On_Null_File()
        {
            _fileManager.RenameFile(null, It.IsAny<string>());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void RenameFile_Throws_On_Null_Or_Empty_NewFileName(string newFileName)
        {
            _fileManager.RenameFile(_fileInfo.Object, newFileName);
        }

        [Test]
        public void RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _mockFileManager.Setup(mfm => mfm.FileExists(_folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            _mockFileManager.Setup(mfm => mfm.UpdateFile(_fileInfo.Object));

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            _mockFolder.Verify(mf => mf.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName), Times.Once());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            _fileManager.RenameFile(_fileInfo.Object, Constants.FOLDER_ValidFileName);

            _mockFolder.Verify(mf => mf.RenameFile(_fileInfo.Object, It.IsAny<string>()), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(FileAlreadyExistsException))]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _mockFileManager.Setup(mfm => mfm.FileExists(_folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(true);

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName);
        }

        [Test]
        [ExpectedException(typeof(InvalidFileExtensionException))]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_InvalidExtensionType()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);
            _mockFileManager.Setup(fm => fm.IsAllowedExtension(It.IsAny<string>())).Returns(false);

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherInvalidFileNameExtension);
        }

        [Test]
        [ExpectedException(typeof(FolderProviderException))]
        public void RenameFile_Throws_When_FolderProvider_Throws()
        {
            _fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            _fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            _fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            _fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            _folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(_folderInfo.Object);

            _mockFileManager.Setup(mfm => mfm.FileExists(_folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            _mockFileManager.Setup(mfm => mfm.UpdateFile(_fileInfo.Object));

            var folderMapping = new FolderMappingInfo();
            folderMapping.FolderProviderType = Constants.FOLDER_ValidFolderProviderType;

            _folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            _mockFolder.Setup(mf => mf.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName)).Throws<Exception>();

            _mockFileManager.Object.RenameFile(_fileInfo.Object, Constants.FOLDER_OtherValidFileName);
        }

        #endregion

        #region UnzipFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnzipFile_Throws_On_Null_File()
        {
            _fileManager.UnzipFile(null, It.IsAny<IFolderInfo>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnzipFile_Throws_On_Null_DestinationFolder()
        {
            _fileManager.UnzipFile(It.IsAny<IFileInfo>(), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UnzipFile_Throws_When_File_Extension_Is_Not_Zip()
        {
            _fileInfo.Setup(fi => fi.Extension).Returns("txt");

            _fileManager.UnzipFile(_fileInfo.Object, It.IsAny<IFolderInfo>());
        }

        [Test]
        public void UnzipFile_Calls_FileManager_ExtractFiles()
        {
            _fileInfo.Setup(fi => fi.Extension).Returns("zip");

            _mockFileManager.Setup(mfm => mfm.ExtractFiles(_fileInfo.Object, _folderInfo.Object)).Verifiable();

            _mockFileManager.Object.UnzipFile(_fileInfo.Object, _folderInfo.Object);

            _mockFileManager.Verify();
        }

        #endregion

        #region UpdateFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_File()
        {
            _fileManager.UpdateFile(null);
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFile()
        {
            _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            _mockFileManager.Object.UpdateFile(_fileInfo.Object);

            _mockData.Verify(md => md.UpdateFile(
                It.IsAny<int>(),
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
            _fileManager.UpdateFile(null, It.IsAny<Stream>());
        }

        [Test]
        public void UpdateFile_Sets_With_And_Height_When_File_Is_Image()
        {
            var image = new Bitmap(10, 20);

            _mockFileManager.Setup(mfm => mfm.IsImageFile(_fileInfo.Object)).Returns(true);
            _mockFileManager.Setup(mfm => mfm.GetImageFromStream(It.IsAny<Stream>())).Returns(image);
            _mockFileManager.Setup(mfm => mfm.GetHash(_fileInfo.Object));

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            _mockFileManager.Object.UpdateFile(_fileInfo.Object, stream);

            _fileInfo.VerifySet(fi => fi.Width = 10);
            _fileInfo.VerifySet(fi => fi.Height = 20);
        }

        [Test]
        public void UpdateFile_Sets_SHA1Hash()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            _mockFileManager.Setup(mfm => mfm.IsImageFile(_fileInfo.Object)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.GetHash(stream)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            _mockFileManager.Object.UpdateFile(_fileInfo.Object, stream);

            _fileInfo.VerifySet(fi => fi.SHA1Hash = Constants.FOLDER_UnmodifiedFileHash);
        }

        [Test]
        public void UpdateFile_Calls_FileManager_UpdateFile_Overload()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

            _mockFileManager.Setup(mfm => mfm.IsImageFile(_fileInfo.Object)).Returns(false);
            _mockFileManager.Setup(mfm => mfm.GetHash(_fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            _fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            _mockFileManager.Object.UpdateFile(_fileInfo.Object, stream);

            _mockFileManager.Verify(mfm => mfm.UpdateFile(_fileInfo.Object), Times.Once());
        }

        #endregion

        #region GetSeekableStream

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSeekableStream_Throws_On_Null_Stream()
        {
            _fileManager.GetSeekableStream(null);
        }

        [Test]
        public void GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable()
        {
            var inputStream = new MemoryStream();
            var seekableStream = _fileManager.GetSeekableStream(inputStream);

            Assert.AreEqual(inputStream, seekableStream);
        }

        [Test]
        public void GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension()
        {
            var inputStream = new Mock<Stream>();
            inputStream.Setup(s => s.CanSeek).Returns(false);
            inputStream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);

            _mockFileManager.Setup(mfm => mfm.GetHostMapPath()).Returns("").Verifiable();
            _mockFileManager.Setup(mfm => mfm.GetAutoDeleteFileStream(It.Is((string x) => x.EndsWith(".resx")))).Returns(new MemoryStream()).Verifiable();

            _mockFileManager.Object.GetSeekableStream(inputStream.Object);

            _mockFileManager.Verify();
        }


        #endregion

        #region GetContentType

        [Test]
        public void GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed()
        {
            const string notManagedExtension = "asdf609vas21AS:F,l/&%/(%$";

            var contentType = _fileManager.GetContentType(notManagedExtension);

            Assert.AreEqual("application/octet-stream", contentType);
        }

        #endregion
    }
}
