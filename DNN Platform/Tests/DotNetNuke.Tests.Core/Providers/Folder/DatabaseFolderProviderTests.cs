#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class DatabaseFolderProviderTests
    {
        #region Private Variables

        private DatabaseFolderProvider _dfp;
        private Mock<DataProvider> _mockData;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFileManager> _fileManager;

        #endregion

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
            _dfp = new DatabaseFolderProvider();
            _mockData = MockComponentProvider.CreateDataProvider();
            _folderInfo = new Mock<IFolderInfo>();
            _fileInfo = new Mock<IFileInfo>();
            _folderManager = new Mock<IFolderManager>();
            _fileManager = new Mock<IFileManager>();

            FolderManager.RegisterInstance(_folderManager.Object);
            FileManager.RegisterInstance(_fileManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        #endregion

        #region AddFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();

            _dfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            _dfp.AddFile(_folderInfo.Object, fileName, stream.Object);
        }

        #endregion

        #region DeleteFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteFile_Throws_On_Null_File()
        {
            _dfp.DeleteFile(null);
        }

        [Test]
        public void DeleteFile_Calls_DataProvider_ClearFileContent()
        {
            _fileInfo.Setup(fi => fi.FileId).Returns(Constants.FOLDER_ValidFileId);

            _dfp.DeleteFile(_fileInfo.Object);

            _mockData.Verify(md => md.ClearFileContent(Constants.FOLDER_ValidFileId), Times.Once());
        }

        #endregion

        #region FileExists

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            _dfp.FileExists(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_FileName()
        {
            _dfp.FileExists(_folderInfo.Object, null);
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, true)).Returns(new FileInfo());

            var result = _dfp.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            FileInfo file = null;
            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(file);

            var result = _dfp.FileExists(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsFalse(result);
        }

        #endregion

        #region FolderExists

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
            _dfp.FolderExists(Constants.FOLDER_ValidFolderPath, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
            var folderMapping = new FolderMappingInfo();

            _dfp.FolderExists(null, folderMapping);
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            _folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(new FolderInfo());

            var result = _dfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            FolderInfo folder = null;
            _folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(folder);

            var result = _dfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.IsFalse(result);
        }

        #endregion

        #region GetFileAttributes

        [Test]
        public void GetFileAttributes_Returns_Null()
        {
            var result = _dfp.GetFileAttributes(It.IsAny<IFileInfo>());

            Assert.IsNull(result);
        }

        #endregion

        #region GetFiles

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFiles_Throws_On_Null_Folder()
        {
            _dfp.GetFiles(null);
        }

        [Test]
        public void GetFiles_Calls_FolderManager_GetFilesByFolder()
        {
            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = "" },
                new FileInfo { FileName = "" },
                new FileInfo { FileName = "" }
            };

            _folderManager.Setup(fm => fm.GetFiles(_folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            _dfp.GetFiles(_folderInfo.Object);

            _folderManager.Verify(fm => fm.GetFiles(_folderInfo.Object), Times.Once());
        }

        [Test]
        public void GetFiles_Count_Equals_DataProvider_GetFiles_Count()
        {
            var expectedFiles = new string[] { "", "", "" };

            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = "" },
                new FileInfo { FileName = "" },
                new FileInfo { FileName = "" }
            };

            _folderManager.Setup(fm => fm.GetFiles(_folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            var files = _dfp.GetFiles(_folderInfo.Object);

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
                new FileInfo { FileName = "file3.txt" }
            };

            _folderManager.Setup(fm => fm.GetFiles(_folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            var files = _dfp.GetFiles(_folderInfo.Object);

            CollectionAssert.AreEqual(expectedFiles, files);
        }

        #endregion

        #region GetFileContent

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFileStream_Throws_On_Null_Folder()
        {
            _dfp.GetFileStream(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileStream_Throws_On_NullOrEmpty_FileName()
        {
            _dfp.GetFileStream(_folderInfo.Object, null);
        }

        [Test]
        public void GetFileStream_Calls_FileManager_GetFile()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns((IFileInfo)null);

            _dfp.GetFileStream(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            _fileManager.Verify(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, true), Times.Once());
        }

        [Test]
        public void GetFileStream_Returns_Valid_Stream_When_File_Exists()
        {
            var validFileBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            var _filesTable = new DataTable("Files");
            _filesTable.Columns.Add("Content", typeof(byte[]));

            _filesTable.Rows.Add(validFileBytes);

            _mockData.Setup(md => md.GetFileContent(Constants.FOLDER_ValidFileId)).Returns(_filesTable.CreateDataReader());

            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, true))
                .Returns(new FileInfo { FileId = Constants.FOLDER_ValidFileId, PortalId = Constants.CONTENT_ValidPortalId });

            var result = _dfp.GetFileStream(_folderInfo.Object, Constants.FOLDER_ValidFileName);

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
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>()))
                .Returns((IFileInfo)null);

            var result = _dfp.GetFileStream(_folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsNull(result);
        }

        #endregion

        #region GetFolderProviderIconPath

        [Test]
        public void GetImageUrl_Calls_IconControllerWrapper_IconURL()
        {
            var iconControllerWrapper = new Mock<IIconController>();
            IconControllerWrapper.RegisterInstance(iconControllerWrapper.Object);

            _dfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderDatabase", "32x32"), Times.Once());
        }

        #endregion

        #region GetLastModificationTime

        [Test]
        public void GetLastModificationTime_Returns_Null_Date()
        {
            var expectedResult = Null.NullDate;

            var result = _dfp.GetLastModificationTime(_fileInfo.Object);

            Assert.AreEqual(expectedResult, result);
        }

        #endregion

        #region GetSubFolders

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
            _dfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
            var folderMappingInfo = new FolderMappingInfo();

            _dfp.GetSubFolders(null, folderMappingInfo).ToList();
        }

        [Test]
        public void GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            _folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
            _folderManager.Setup(fm => fm.GetFolders(_folderInfo.Object)).Returns(new List<IFolderInfo>());

            _dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            _folderManager.Verify(fm => fm.GetFolders(_folderInfo.Object), Times.Once());
        }

        [Test]
        public void GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count()
        {
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            var subFolders = new List<IFolderInfo>() {
                new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath },
                new FolderInfo { FolderPath = Constants.FOLDER_OtherValidSubFolderRelativePath }
            };

            _folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
            _folderManager.Setup(fm => fm.GetFolders(_folderInfo.Object)).Returns(subFolders);

            var result = _dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            Assert.AreEqual(subFolders.Count, result.Count);
        }

        [Test]
        public void GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()
        {
            var expectedResult = new List<string> {
                Constants.FOLDER_ValidSubFolderRelativePath,
                Constants.FOLDER_OtherValidSubFolderRelativePath
            };

            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;

            var subFolders = new List<IFolderInfo> {
                new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath },
                new FolderInfo { FolderPath = Constants.FOLDER_OtherValidSubFolderRelativePath }
            };

            _folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(_folderInfo.Object);
            _folderManager.Setup(fm => fm.GetFolders(_folderInfo.Object)).Returns(subFolders);

            var result = _dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            CollectionAssert.AreEqual(expectedResult, result);
        }

        #endregion

        #region IsInSync

        [Test]
        public void IsInSync_Returns_True()
        {
            var result = _dfp.IsInSync(It.IsAny<IFileInfo>());

            Assert.IsTrue(result);
        }

        #endregion

        #region SupportsFileAttributes

        [Test]
        public void SupportsFileAttributes_Returns_False()
        {
            var result = _dfp.SupportsFileAttributes();

            Assert.IsFalse(result);
        }

        #endregion

        #region UpdateFile

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();
            
            _dfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            _dfp.UpdateFile(_folderInfo.Object, fileName, stream.Object);
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFileContent_When_File_Exists()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _fileInfo.Setup(fi => fi.FileId).Returns(Constants.FOLDER_ValidFileId);

            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, true)).Returns(_fileInfo.Object);

            _dfp.UpdateFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(new byte[16 * 1024]));

            _mockData.Verify(md => md.UpdateFileContent(Constants.FOLDER_ValidFileId, It.IsAny<byte[]>()), Times.Once());
        }

        [Test]
        public void UpdateFile_Does_Not_Call_DataProvider_UpdateFileContent_When_File_Does_Not_Exist()
        {
            _folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            _folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            _fileManager.Setup(fm => fm.GetFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns((IFileInfo)null);

            _dfp.UpdateFile(_folderInfo.Object, Constants.FOLDER_ValidFileName, null);

            _mockData.Verify(md => md.UpdateFileContent(It.IsAny<int>(), It.IsAny<byte[]>()), Times.Never());
        }

        #endregion
    }
}
