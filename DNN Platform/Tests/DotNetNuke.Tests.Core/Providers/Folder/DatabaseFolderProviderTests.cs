// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [TestFixture]
    public class DatabaseFolderProviderTests
    {
        private DatabaseFolderProvider _dfp;
        private Mock<DataProvider> _mockData;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFileManager> _fileManager;

        [SetUp]
        public void Setup()
        {
            this._dfp = new DatabaseFolderProvider();
            this._mockData = MockComponentProvider.CreateDataProvider();
            this._folderInfo = new Mock<IFolderInfo>();
            this._fileInfo = new Mock<IFileInfo>();
            this._folderManager = new Mock<IFolderManager>();
            this._fileManager = new Mock<IFileManager>();

            FolderManager.RegisterInstance(this._folderManager.Object);
            FileManager.RegisterInstance(this._fileManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();

            this._dfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            this._dfp.AddFile(this._folderInfo.Object, fileName, stream.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteFile_Throws_On_Null_File()
        {
            this._dfp.DeleteFile(null);
        }

        [Test]
        public void DeleteFile_Calls_DataProvider_ClearFileContent()
        {
            this._fileInfo.Setup(fi => fi.FileId).Returns(Constants.FOLDER_ValidFileId);

            this._dfp.DeleteFile(this._fileInfo.Object);

            this._mockData.Verify(md => md.ClearFileContent(Constants.FOLDER_ValidFileId), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            this._dfp.FileExists(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_FileName()
        {
            this._dfp.FileExists(this._folderInfo.Object, null);
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true)).Returns(new FileInfo());

            var result = this._dfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            FileInfo file = null;
            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(file);

            var result = this._dfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
            this._dfp.FolderExists(Constants.FOLDER_ValidFolderPath, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
            var folderMapping = new FolderMappingInfo();

            this._dfp.FolderExists(null, folderMapping);
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(new FolderInfo());

            var result = this._dfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            FolderInfo folder = null;
            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(folder);

            var result = this._dfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.IsFalse(result);
        }

        [Test]
        public void GetFileAttributes_Returns_Null()
        {
            var result = this._dfp.GetFileAttributes(It.IsAny<IFileInfo>());

            Assert.IsNull(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFiles_Throws_On_Null_Folder()
        {
            this._dfp.GetFiles(null);
        }

        [Test]
        public void GetFiles_Calls_FolderManager_GetFilesByFolder()
        {
            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
            };

            this._folderManager.Setup(fm => fm.GetFiles(this._folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            this._dfp.GetFiles(this._folderInfo.Object);

            this._folderManager.Verify(fm => fm.GetFiles(this._folderInfo.Object), Times.Once());
        }

        [Test]
        public void GetFiles_Count_Equals_DataProvider_GetFiles_Count()
        {
            var expectedFiles = new string[] { string.Empty, string.Empty, string.Empty };

            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
            };

            this._folderManager.Setup(fm => fm.GetFiles(this._folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            var files = this._dfp.GetFiles(this._folderInfo.Object);

            Assert.AreEqual(expectedFiles.Length, files.Length);
        }

        [Test]
        public void GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()
        {
            var expectedFiles = new string[] { "file1.txt", "file2.txt", "file3.txt" };

            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = "file1.txt" },
                new FileInfo { FileName = "file2.txt" },
                new FileInfo { FileName = "file3.txt" },
            };

            this._folderManager.Setup(fm => fm.GetFiles(this._folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            var files = this._dfp.GetFiles(this._folderInfo.Object);

            CollectionAssert.AreEqual(expectedFiles, files);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFileStream_Throws_On_Null_Folder()
        {
            this._dfp.GetFileStream(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileStream_Throws_On_NullOrEmpty_FileName()
        {
            this._dfp.GetFileStream(this._folderInfo.Object, null);
        }

        [Test]
        public void GetFileStream_Calls_FileManager_GetFile()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns((IFileInfo)null);

            this._dfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileManager.Verify(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true), Times.Once());
        }

        [Test]
        public void GetFileStream_Returns_Valid_Stream_When_File_Exists()
        {
            var validFileBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            var _filesTable = new DataTable("Files");
            _filesTable.Columns.Add("Content", typeof(byte[]));

            _filesTable.Rows.Add(validFileBytes);

            this._mockData.Setup(md => md.GetFileContent(Constants.FOLDER_ValidFileId)).Returns(_filesTable.CreateDataReader());

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true))
                .Returns(new FileInfo { FileId = Constants.FOLDER_ValidFileId, PortalId = Constants.CONTENT_ValidPortalId });

            var result = this._dfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            byte[] resultBytes;
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = result.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                resultBytes = ms.ToArray();
            }

            Assert.AreEqual(validFileBytes, resultBytes);
        }

        [Test]
        public void GetFileStream_Returns_Null_When_File_Does_Not_Exist()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>()))
                .Returns((IFileInfo)null);

            var result = this._dfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsNull(result);
        }

        [Test]
        public void GetImageUrl_Calls_IconControllerWrapper_IconURL()
        {
            var iconControllerWrapper = new Mock<IIconController>();
            IconControllerWrapper.RegisterInstance(iconControllerWrapper.Object);

            this._dfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderDatabase", "32x32"), Times.Once());
        }

        [Test]
        public void GetLastModificationTime_Returns_Null_Date()
        {
            var expectedResult = Null.NullDate;

            var result = this._dfp.GetLastModificationTime(this._fileInfo.Object);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
            this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
            var folderMappingInfo = new FolderMappingInfo();

            this._dfp.GetSubFolders(null, folderMappingInfo).ToList();
        }

        [Test]
        public void GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object);
            this._folderManager.Setup(fm => fm.GetFolders(this._folderInfo.Object)).Returns(new List<IFolderInfo>());

            this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            this._folderManager.Verify(fm => fm.GetFolders(this._folderInfo.Object), Times.Once());
        }

        [Test]
        public void GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            var subFolders = new List<IFolderInfo>()
            {
                new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath },
                new FolderInfo { FolderPath = Constants.FOLDER_OtherValidSubFolderRelativePath },
            };

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object);
            this._folderManager.Setup(fm => fm.GetFolders(this._folderInfo.Object)).Returns(subFolders);

            var result = this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            Assert.AreEqual(subFolders.Count, result.Count);
        }

        [Test]
        public void GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()
        {
            var expectedResult = new List<string>
            {
                Constants.FOLDER_ValidSubFolderRelativePath,
                Constants.FOLDER_OtherValidSubFolderRelativePath,
            };

            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            var subFolders = new List<IFolderInfo>
            {
                new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath },
                new FolderInfo { FolderPath = Constants.FOLDER_OtherValidSubFolderRelativePath },
            };

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object);
            this._folderManager.Setup(fm => fm.GetFolders(this._folderInfo.Object)).Returns(subFolders);

            var result = this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            CollectionAssert.AreEqual(expectedResult, result);
        }

        [Test]
        public void IsInSync_Returns_True()
        {
            var result = this._dfp.IsInSync(It.IsAny<IFileInfo>());

            Assert.IsTrue(result);
        }

        [Test]
        public void SupportsFileAttributes_Returns_False()
        {
            var result = this._dfp.SupportsFileAttributes();

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();

            this._dfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            this._dfp.UpdateFile(this._folderInfo.Object, fileName, stream.Object);
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFileContent_When_File_Exists()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileInfo.Setup(fi => fi.FileId).Returns(Constants.FOLDER_ValidFileId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true)).Returns(this._fileInfo.Object);

            this._dfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(new byte[16 * 1024]));

            this._mockData.Verify(md => md.UpdateFileContent(Constants.FOLDER_ValidFileId, It.IsAny<byte[]>()), Times.Once());
        }

        [Test]
        public void UpdateFile_Does_Not_Call_DataProvider_UpdateFileContent_When_File_Does_Not_Exist()
        {
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns((IFileInfo)null);

            this._dfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, null);

            this._mockData.Verify(md => md.UpdateFileContent(It.IsAny<int>(), It.IsAny<byte[]>()), Times.Never());
        }
    }
}
