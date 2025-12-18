// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class StandardFolderProviderTests
    {
        private StandardFolderProvider sfp;
        private Mock<IFolderInfo> folderInfo;
        private Mock<IFileInfo> fileInfo;
        private Mock<IFile> fileWrapper;
        private Mock<IDirectory> directoryWrapper;
        private Mock<IFolderManager> folderManager;
        private Mock<IFileManager> fileManager;
        private Mock<IPathUtils> pathUtils;
        private Mock<IPortalController> portalControllerMock;
        private Mock<CryptographyProvider> cryptographyProviderMock;
        private Mock<ILocaleController> localeControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.sfp = new StandardFolderProvider();
            this.folderInfo = new Mock<IFolderInfo>();
            this.fileInfo = new Mock<IFileInfo>();
            this.fileWrapper = new Mock<IFile>();
            this.directoryWrapper = new Mock<IDirectory>();
            this.folderManager = new Mock<IFolderManager>();
            this.fileManager = new Mock<IFileManager>();
            this.pathUtils = new Mock<IPathUtils>();
            this.portalControllerMock = new Mock<IPortalController>();
            this.portalControllerMock.Setup(p => p.GetPortalSettings(Constants.CONTENT_ValidPortalId))
                .Returns(this.GetPortalSettingsDictionaryMock());
            this.portalControllerMock.Setup(p => p.GetCurrentPortalSettings()).Returns(this.GetPortalSettingsMock());
            this.cryptographyProviderMock = new Mock<CryptographyProvider>();
            this.cryptographyProviderMock.Setup(c => c.EncryptParameter(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Guid.NewGuid().ToString("N"));
            this.localeControllerMock = new Mock<ILocaleController>();
            this.localeControllerMock.Setup(l => l.GetLocales(Constants.CONTENT_ValidPortalId)).Returns(new Dictionary<string, Locale>
            {
                { "en-us", new Locale() },
            });

            FileWrapper.RegisterInstance(this.fileWrapper.Object);
            DirectoryWrapper.RegisterInstance(this.directoryWrapper.Object);
            FolderManager.RegisterInstance(this.folderManager.Object);
            FileManager.RegisterInstance(this.fileManager.Object);
            PathUtils.RegisterInstance(this.pathUtils.Object);
            PortalController.SetTestableInstance(this.portalControllerMock.Object);
            ComponentFactory.RegisterComponentInstance<CryptographyProvider>("CryptographyProviderMock", this.cryptographyProviderMock.Object);
            LocaleController.RegisterInstance(this.localeControllerMock.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.folderManager.Object);
                    services.AddSingleton(this.fileManager.Object);
                    services.AddSingleton(this.pathUtils.Object);
                    services.AddSingleton(this.portalControllerMock.Object);
                    services.AddSingleton(this.cryptographyProviderMock.Object);
                    services.AddSingleton(this.localeControllerMock.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
            TestableGlobals.ClearInstance();
            PortalController.ClearInstance();
        }

        [Test]
        public void AddFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();

            Assert.Throws<ArgumentNullException>(() => this.sfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            Assert.Throws<ArgumentException>(() => this.sfp.AddFile(this.folderInfo.Object, fileName, stream.Object));
        }

        [Test]
        public void AddFile_Throws_On_Null_Content()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, null));
        }

        [Test]
        public void DeleteFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.DeleteFile(null));
        }

        // [Test]
        // public void DeleteFile_Calls_FileWrapper_Delete_When_File_Exists()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(true);

        // sfp.DeleteFile(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Once());
        // }

        // [Test]
        // public void DeleteFile_Does_Not_Call_FileWrapper_Delete_When_File_Does_Not_Exist()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

        // sfp.DeleteFile(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Never());
        // }
        [Test]
        public void ExistsFile_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.FileExists(null, Constants.FOLDER_ValidFileName));
        }

        [Test]
        public void ExistsFile_Throws_On_Null_FileName()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.FileExists(this.folderInfo.Object, null));
        }

        [Test]
        public void ExistsFile_Calls_FileWrapper_Exists()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.fileWrapper.Verify(fw => fw.Exists(Constants.FOLDER_ValidFilePath), Times.Once());
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(true);

            var result = this.sfp.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

            var result = this.sfp.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.FolderExists(Constants.FOLDER_ValidFolderPath, null));
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
            var folderMapping = new FolderMappingInfo();

            Assert.Throws<ArgumentNullException>(() => this.sfp.FolderExists(null, folderMapping));
        }

        [Test]
        public void ExistsFolder_Calls_DirectoryWrapper_Exists()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            this.directoryWrapper.Verify(dw => dw.Exists(Constants.FOLDER_ValidFolderPath), Times.Once());
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            var result = this.sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(false);

            var result = this.sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            Assert.That(result, Is.False);
        }

        [Test]
        public void GetFileAttributes_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetFileAttributes(null));
        }

        // [Test]
        // public void GetFileAttributes_Calls_FileWrapper_GetAttributes()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // sfp.GetFileAttributes(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.GetAttributes(Constants.FOLDER_ValidFilePath), Times.Once());
        // }

        // [Test]
        // public void GetFileAttributes_Returns_File_Attributes_When_File_Exists()
        // {
        //    var expectedFileAttributes = FileAttributes.Normal;

        // fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidFilePath)).Returns(expectedFileAttributes);

        // var result = sfp.GetFileAttributes(fileInfo.Object);

        // Assert.AreEqual(expectedFileAttributes, result);
        // }
        [Test]
        public void GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()
        {
            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this.fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidFilePath)).Throws<FileNotFoundException>();

            var result = this.sfp.GetFileAttributes(this.fileInfo.Object);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetFiles_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetFiles(null));
        }

        [Test]
        public void GetFiles_Calls_DirectoryWrapper_GetFiles()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.GetFiles(this.folderInfo.Object);

            this.directoryWrapper.Verify(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath));
        }

        [Test]
        public void GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new[] { string.Empty, string.Empty, string.Empty };

            this.directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this.sfp.GetFiles(this.folderInfo.Object);

            Assert.That(files, Has.Length.EqualTo(filesReturned.Length));
        }

        [Test]
        public void GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new[] { "C:\\folder\\file1.txt", "C:\\folder\\file2.txt", "C:\\folder\\file3.txt" };
            var expectedValues = new[] { "file1.txt", "file2.txt", "file3.txt" };

            this.directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this.sfp.GetFiles(this.folderInfo.Object);

            Assert.That(files, Is.EqualTo(expectedValues).AsCollection);
        }

        [Test]
        public void GetFileStream_Throws_On_Null_Folder()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetFileStream(null, Constants.FOLDER_ValidFileName));
        }

        [Test]
        public void GetFileStream_Throws_On_Null_FileName()
        {
            Assert.Throws<ArgumentException>(() => this.sfp.GetFileStream(this.folderInfo.Object, null));
        }

        [Test]
        public void GetFileStream_Calls_FileWrapper_OpenRead()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.GetFileStream(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.fileWrapper.Verify(fw => fw.OpenRead(Constants.FOLDER_ValidFilePath), Times.Once());
        }

        [Test]
        public void GetFileStream_Returns_Valid_Stream_When_File_Exists()
        {
            var validFileBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var memoryStream = new MemoryStream(validFileBytes);

            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidFilePath)).Returns(memoryStream);

            var result = this.sfp.GetFileStream(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

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

            Assert.That(resultBytes, Is.EqualTo(validFileBytes));
        }

        [Test]
        public void GetFileStream_Returns_Null_When_File_Does_Not_Exist()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidFilePath)).Throws<FileNotFoundException>();

            var result = this.sfp.GetFileStream(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetImageUrl_Calls_IconControllerWrapper_IconURL()
        {
            var iconControllerWrapper = new Mock<IIconController>();
            IconControllerWrapper.RegisterInstance(iconControllerWrapper.Object);

            this.sfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderStandard", "32x32"), Times.Once());
        }

        [Test]
        public void GetLastModificationTime_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetLastModificationTime(null));
        }

        // [Test]
        // public void GetLastModificationTime_Calls_FileWrapper_GetLastWriteTime()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // sfp.GetLastModificationTime(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidFilePath), Times.Once());
        // }

        // [Test]
        // public void GetLastModificationTime_Returns_Valid_Date_When_File_Exists()
        // {
        //    var expectedDate = DateTime.Now;

        // fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidFilePath)).Returns(expectedDate);

        // var result = sfp.GetLastModificationTime(fileInfo.Object);

        // Assert.AreEqual(expectedDate, result);
        // }
        [Test]
        public void GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()
        {
            var expectedDate = Null.NullDate;

            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this.fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidFilePath)).Throws<FileNotFoundException>();

            var result = this.sfp.GetLastModificationTime(this.fileInfo.Object);

            Assert.That(result, Is.EqualTo(expectedDate));
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList());
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
            var folderMapping = new FolderMappingInfo();

            Assert.Throws<ArgumentNullException>(() => this.sfp.GetSubFolders(null, folderMapping).ToList());
        }

        [Test]
        public void GetSubFolders_Calls_DirectoryWrapper_GetDirectories()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            this.directoryWrapper.Verify(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath), Times.Once());
        }

        [Test]
        public void GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()
        {
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this.directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            Assert.That(result, Has.Count.EqualTo(subFolders.Length));
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

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this.directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            Assert.That(result, Is.EqualTo(expectedSubFolders).AsCollection);
        }

        [Test]
        public void GetFileUrl_WhenCurrentPortalSettingsReturnsNull_DontThrow()
        {
            // arrange
            var sfp = new Mock<StandardFolderProvider>
            {
                CallBase = true,
            };

            sfp.Setup(x => x.GetPortalSettings(Constants.CONTENT_ValidPortalId))
                .Returns(this.GetPortalSettingsMock());

            this.fileInfo.Setup(x => x.FileName)
                .Returns(Constants.FOLDER_ValidFileName);

            this.fileInfo.Setup(x => x.PortalId)
                .Returns(Constants.CONTENT_ValidPortalId);

            this.portalControllerMock.Setup(x => x.GetCurrentPortalSettings())
                .Returns<PortalSettings>(null);

            // act
            string fileUrl = null;
            TestDelegate action = () => fileUrl = sfp.Object.GetFileUrl(this.fileInfo.Object);

            // assert
            Assert.DoesNotThrow(action);
            Assert.That(fileUrl, Is.Not.Null);
        }

        [Test]
        [TestCase("(")]
        [TestCase(")")]
        [TestCase("")]
        public void GetFileUrl_ReturnsStandardUrl_WhenFileUrlDoesNotContainInvalidCharactes(string fileNameChar)
        {
            // Arrange
            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            var portalSettingsMock = this.GetPortalSettingsMock();
            sfp.Setup(fp => fp.GetPortalSettings(Constants.CONTENT_ValidPortalId)).Returns(portalSettingsMock);
            this.fileInfo.Setup(f => f.FileName).Returns($"MyFileName {fileNameChar} Copy");
            this.fileInfo.Setup(f => f.PortalId).Returns(Constants.CONTENT_ValidPortalId);

            // Act
            var fileUrl = sfp.Object.GetFileUrl(this.fileInfo.Object);

            // Assert
            Assert.That(fileUrl.ToLowerInvariant().Contains("linkclick"), Is.False);
        }

        [Test]
        [TestCase("?")]
        [TestCase("&")]
        [TestCase("+")]
        [TestCase(";")]
        [TestCase(":")]
        [TestCase("@")]
        [TestCase("=")]
        [TestCase("$")]
        [TestCase(",")]
        [TestCase("%")]
        public void GetFileUrl_ReturnsLinkclickUrl_WhenFileUrlContainsInvalidCharactes(string fileNameChar)
        {
            // Arrange
            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            var portalSettingsMock = this.GetPortalSettingsMock();
            sfp.Setup(fp => fp.GetPortalSettings(Constants.CONTENT_ValidPortalId)).Returns(portalSettingsMock);
            this.fileInfo.Setup(f => f.FileName).Returns($"MyFileName {fileNameChar} Copy");
            this.fileInfo.Setup(f => f.PortalId).Returns(Constants.CONTENT_ValidPortalId);

            // Act
            var fileUrl = sfp.Object.GetFileUrl(this.fileInfo.Object);

            // Assert
            Assert.That(fileUrl.ToLowerInvariant().Contains("linkclick"), Is.True);
        }

        [Test]
        public void IsInSync_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.IsInSync(null));
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_In_Sync()
        {
            this.fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this.fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var result = sfp.Object.IsInSync(this.fileInfo.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_Not_In_Sync()
        {
            this.fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this.fileInfo.Object)).Returns(Constants.FOLDER_ModifiedFileHash);

            var result = sfp.Object.IsInSync(this.fileInfo.Object);

            Assert.That(result, Is.True);
        }

        [Test]
        public void RenameFile_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.RenameFile(null, Constants.FOLDER_ValidFileName));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void RenameFile_Throws_On_NullOrEmpty_NewFileName(string newFileName)
        {
            Assert.Throws<ArgumentException>(() => this.sfp.RenameFile(this.fileInfo.Object, newFileName));
        }

        [Test]
        public void RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()
        {
            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            this.fileWrapper.Verify(fw => fw.Move(Constants.FOLDER_ValidFilePath, Constants.FOLDER_OtherValidFilePath), Times.Once());
        }

        [Test]
        public void RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()
        {
            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this.sfp.RenameFile(this.fileInfo.Object, Constants.FOLDER_ValidFileName);

            this.fileWrapper.Verify(fw => fw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void SetFileAttributes_Throws_On_Null_File()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.SetFileAttributes(null, FileAttributes.Archive));
        }

        // [Test]
        // public void SetFileAttributes_Calls_FileWrapper_SetAttributes()
        // {
        //    const FileAttributes validFileAttributes = FileAttributes.Archive;

        // fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // sfp.SetFileAttributes(fileInfo.Object, validFileAttributes);

        // fileWrapper.Verify(fw => fw.SetAttributes(Constants.FOLDER_ValidFilePath, validFileAttributes), Times.Once());
        // }
        [Test]
        public void SupportsFileAttributes_Returns_True()
        {
            var result = this.sfp.SupportsFileAttributes();

            Assert.That(result, Is.True);
        }

        [Test]
        public void UpdateFile_Throws_On_Null_Folder()
        {
            var stream = new Mock<Stream>();

            Assert.Throws<ArgumentNullException>(() => this.sfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void UpdateFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
            var stream = new Mock<Stream>();

            Assert.Throws<ArgumentException>(() => this.sfp.UpdateFile(this.folderInfo.Object, fileName, stream.Object));
        }

        [Test]
        public void UpdateFile_Throws_On_Null_Content()
        {
            Assert.Throws<ArgumentNullException>(() => this.sfp.UpdateFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, null));
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(true);

            var stream = new Mock<Stream>();

            this.sfp.UpdateFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this.fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Once());
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Create()
        {
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

            var stream = new Mock<Stream>();

            this.sfp.UpdateFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this.fileWrapper.Verify(fw => fw.Create(Constants.FOLDER_ValidFilePath), Times.Once());
        }

        private Dictionary<string, string> GetPortalSettingsDictionaryMock()
        {
            var portalSettingsDictionary = new Dictionary<string, string>();
            portalSettingsDictionary.Add("AddCachebusterToResourceUris", true.ToString());

            return portalSettingsDictionary;
        }

        private PortalSettings GetPortalSettingsMock()
        {
            var portalSettingsMock = new Mock<PortalSettings>();
            portalSettingsMock.Object.HomeDirectory = "/portals/" + Constants.CONTENT_ValidPortalId;
            portalSettingsMock.Object.PortalId = Constants.CONTENT_ValidPortalId;
            portalSettingsMock.Object.EnableUrlLanguage = false;
            portalSettingsMock.Object.GUID = Guid.NewGuid();

            return portalSettingsMock.Object;
        }
    }
}
