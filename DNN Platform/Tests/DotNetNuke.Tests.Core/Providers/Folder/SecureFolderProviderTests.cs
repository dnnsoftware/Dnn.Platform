// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SecureFolderProviderTests
    {
        private SecureFolderProvider _sfp;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;
        private Mock<IFile> _fileWrapper;
        private Mock<IDirectory> _directoryWrapper;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFileManager> _fileManager;
        private Mock<IPathUtils> _pathUtils;

        [SetUp]
        public void Setup()
        {
            this._sfp = new SecureFolderProvider();
            this._folderInfo = new Mock<IFolderInfo>();
            this._fileInfo = new Mock<IFileInfo>();
            this._fileWrapper = new Mock<IFile>();
            this._directoryWrapper = new Mock<IDirectory>();
            this._folderManager = new Mock<IFolderManager>();
            this._fileManager = new Mock<IFileManager>();
            this._pathUtils = new Mock<IPathUtils>();

            FileWrapper.RegisterInstance(this._fileWrapper.Object);
            DirectoryWrapper.RegisterInstance(this._directoryWrapper.Object);
            FolderManager.RegisterInstance(this._folderManager.Object);
            FileManager.RegisterInstance(this._fileManager.Object);
            PathUtils.RegisterInstance(this._pathUtils.Object);
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

            this._sfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void AddFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            this._sfp.AddFile(this._folderInfo.Object, fileName, stream.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddFile_Throws_On_Null_Content()
        {
            this._sfp.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteFile_Throws_On_Null_File()
        {
            this._sfp.DeleteFile(null);
        }

        [Test]
        public void DeleteFile_Calls_FileWrapper_Delete_When_File_Exists()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(true);

            this._sfp.DeleteFile(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }

        [Test]
        public void DeleteFile_Does_Not_Call_FileWrapper_Delete_When_File_Does_Not_Exists()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

            this._sfp.DeleteFile(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            this._sfp.FileExists(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFile_Throws_On_Null_FileName()
        {
            this._sfp.FileExists(this._folderInfo.Object, null);
        }

        [Test]
        public void ExistsFile_Calls_FileWrapper_Exists()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileWrapper.Verify(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(true);

            var result = this._sfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(false);

            var result = this._sfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
            this._sfp.FolderExists(Constants.FOLDER_ValidFolderPath, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
            var folderMapping = new FolderMappingInfo();

            this._sfp.FolderExists(null, folderMapping);
        }

        [Test]
        public void ExistsFolder_Calls_DirectoryWrapper_Exists()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            this._directoryWrapper.Verify(dw => dw.Exists(Constants.FOLDER_ValidFolderPath), Times.Once());
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            var result = this._sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.IsTrue(result);
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(false);

            var result = this._sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.IsFalse(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFileAttributes_Throws_On_Null_File()
        {
            this._sfp.GetFileAttributes(null);
        }

        [Test]
        public void GetFileAttributes_Calls_FileWrapper_GetAttributes()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._sfp.GetFileAttributes(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.GetAttributes(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }

        [Test]
        public void GetFileAttributes_Returns_File_Attributes_When_File_Exists()
        {
            var expectedFileAttributes = FileAttributes.Normal;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidSecureFilePath)).Returns(expectedFileAttributes);

            var result = this._sfp.GetFileAttributes(this._fileInfo.Object);

            Assert.AreEqual(expectedFileAttributes, result);
        }

        [Test]
        public void GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidSecureFilePath)).Throws<FileNotFoundException>();

            var result = this._sfp.GetFileAttributes(this._fileInfo.Object);

            Assert.IsNull(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFiles_Throws_On_Null_Folder()
        {
            this._sfp.GetFiles(null);
        }

        [Test]
        public void GetFiles_Calls_DirectoryWrapper_GetFiles()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.GetFiles(this._folderInfo.Object);

            this._directoryWrapper.Verify(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath));
        }

        [Test]
        public void GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new string[] { "C:\\folder\\file1.txt.resources", "C:\\folder\\file2.txt.resources", "C:\\folder\\file3.txt.resources" };

            this._directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this._sfp.GetFiles(this._folderInfo.Object);

            Assert.AreEqual(filesReturned.Length, files.Length);
        }

        [Test]
        public void GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new string[] { "C:\\folder\\file1.txt.resources", "C:\\folder\\file2.txt.resources", "C:\\folder\\file3.txt.resources" };
            var expectedValues = new string[] { "file1.txt", "file2.txt", "file3.txt" };

            this._directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this._sfp.GetFiles(this._folderInfo.Object);

            CollectionAssert.AreEqual(expectedValues, files);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFileStream_Throws_On_Null_Folder()
        {
            this._sfp.GetFileStream(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileStream_Throws_On_Null_FileName()
        {
            this._sfp.GetFileStream(this._folderInfo.Object, null);
        }

        [Test]
        public void GetFileStream_Calls_FileWrapper_OpenRead()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileWrapper.Verify(fw => fw.OpenRead(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }

        [Test]
        public void GetFileStream_Returns_Valid_Stream_When_File_Exists()
        {
            var validFileBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var memoryStream = new MemoryStream(validFileBytes);

            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidSecureFilePath)).Returns(memoryStream);

            var result = this._sfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

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
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidSecureFilePath)).Throws<FileNotFoundException>();

            var result = this._sfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.IsNull(result);
        }

        [Test]
        public void GetImageUrl_Calls_IconControllerWrapper_IconURL()
        {
            var iconControllerWrapper = new Mock<IIconController>();
            IconControllerWrapper.RegisterInstance(iconControllerWrapper.Object);

            this._sfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderSecure", "32x32"), Times.Once());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLastModificationTime_Throws_On_Null_File()
        {
            this._sfp.GetLastModificationTime(null);
        }

        [Test]
        public void GetLastModificationTime_Calls_FileWrapper_GetLastWriteTime()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._sfp.GetLastModificationTime(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }

        [Test]
        public void GetLastModificationTime_Returns_Valid_Date_When_File_Exists()
        {
            var expectedDate = DateTime.Now;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidSecureFilePath)).Returns(expectedDate);

            var result = this._sfp.GetLastModificationTime(this._fileInfo.Object);

            Assert.AreEqual(expectedDate, result);
        }

        [Test]
        public void GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()
        {
            var expectedDate = Null.NullDate;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidSecureFilePath)).Throws<FileNotFoundException>();

            var result = this._sfp.GetLastModificationTime(this._fileInfo.Object);

            Assert.AreEqual(expectedDate, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
            this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
            var folderMapping = new FolderMappingInfo();

            this._sfp.GetSubFolders(null, folderMapping).ToList();
        }

        [Test]
        public void GetSubFolders_Calls_DirectoryWrapper_GetFiles()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            this._directoryWrapper.Verify(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath), Times.Once());
        }

        [Test]
        public void GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this._directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            Assert.AreEqual(subFolders.Length, result.Count);
        }

        [Test]
        public void GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()
        {
            var expectedSubFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderRelativePath,
                Constants.FOLDER_OtherValidSubFolderRelativePath,
            };

            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this._directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            CollectionAssert.AreEqual(expectedSubFolders, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsInSync_Throws_On_Null_File()
        {
            this._sfp.IsInSync(null);
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_In_Sync()
        {
            this._fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<SecureFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this._fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var result = sfp.Object.IsInSync(this._fileInfo.Object);

            Assert.IsTrue(result);
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_Not_In_Sync()
        {
            this._fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<SecureFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this._fileInfo.Object)).Returns(Constants.FOLDER_ModifiedFileHash);

            var result = sfp.Object.IsInSync(this._fileInfo.Object);

            Assert.IsTrue(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RenameFile_Throws_On_Null_File()
        {
            this._sfp.RenameFile(null, Constants.FOLDER_ValidFileName);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void RenameFile_Throws_On_NullOrEmpty_NewFileName(string newFileName)
        {
            this._sfp.RenameFile(this._fileInfo.Object, newFileName);
        }

        [Test]
        public void RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            this._fileWrapper.Verify(fw => fw.Move(Constants.FOLDER_ValidSecureFilePath, Constants.FOLDER_OtherValidSecureFilePath), Times.Once());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()
        {
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this._sfp.RenameFile(this._fileInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileWrapper.Verify(fw => fw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetFileAttributes_Throws_On_Null_File()
        {
            this._sfp.SetFileAttributes(null, FileAttributes.Archive);
        }

        [Test]
        public void SetFileAttributes_Calls_FileWrapper_SetAttributes()
        {
            const FileAttributes validFileAttributes = FileAttributes.Archive;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._sfp.SetFileAttributes(this._fileInfo.Object, validFileAttributes);

            this._fileWrapper.Verify(fw => fw.SetAttributes(Constants.FOLDER_ValidSecureFilePath, validFileAttributes), Times.Once());
        }

        [Test]
        public void SupportsFileAttributes_Returns_True()
        {
            var result = this._sfp.SupportsFileAttributes();

            Assert.IsTrue(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();

            this._sfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            this._sfp.UpdateFile(this._folderInfo.Object, fileName, stream.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFile_Throws_On_Null_Content()
        {
            this._sfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, null);
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(true);

            var stream = new Mock<Stream>();

            this._sfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this._fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Create()
        {
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(false);

            var stream = new Mock<Stream>();

            this._sfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this._fileWrapper.Verify(fw => fw.Create(Constants.FOLDER_ValidSecureFilePath), Times.Once());
        }
    }
}
