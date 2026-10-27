// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.RichTextEditor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Dnn.RichTextEditorProvider.Components.FileBrowser;
    using DotNetNuke.Services.FileSystem;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileBrowserServiceTests
    {
        private const int PortalId = 0;
        private const int UserId = 7;
        private const int StandardMappingId = 1;
        private const int SecureMappingId = 2;

        private Mock<IFolderManager> folderManager;
        private Mock<IFileManager> fileManager;
        private Mock<IFolderMappingController> folderMappingController;
        private Mock<IFolderPermissionChecker> permissions;
        private FileBrowserService service;

        [SetUp]
        public void SetUp()
        {
            this.folderManager = new Mock<IFolderManager>();
            this.fileManager = new Mock<IFileManager>();
            this.folderMappingController = new Mock<IFolderMappingController>();
            this.permissions = new Mock<IFolderPermissionChecker>();
            this.folderMappingController.Setup(c => c.GetFolderMapping(PortalId, StandardMappingId)).Returns(new FolderMappingInfo(PortalId, "Standard", "StandardFolderProvider"));
            this.folderMappingController.Setup(c => c.GetFolderMapping(PortalId, SecureMappingId)).Returns(new FolderMappingInfo(PortalId, "Secure", "SecureFolderProvider"));
            this.service = new FileBrowserService(this.folderManager.Object, this.fileManager.Object, this.folderMappingController.Object, this.permissions.Object, extension => "application/octet-stream");
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("/", "")]
        [TestCase("Images", "Images/")]
        [TestCase("/Images/Banners/", "Images/Banners/")]
        [TestCase("Images\\Banners", "Images/Banners/")]
        [TestCase("Images/./Banners//", "Images/Banners/")]
        [TestCase("Images/Banners/..", "Images/")]
        [TestCase("../../Images", "Images/")]
        [TestCase("Images/../..", "")]
        public void NormalizePath_ReturnsDnnFolderPath(string path, string expected)
        {
            Assert.That(FileBrowserService.NormalizePath(path), Is.EqualTo(expected));
        }

        [TestCase(@"C:\fakepath\my photo.jpg", "my photo.jpg")]
        [TestCase("../../web.config.jpg", "web.config.jpg")]
        [TestCase("a#b&c%d+e.png", "a_b_c_d_e.png")]
        [TestCase("<script>.jpg", "_script_.jpg")]
        [TestCase(".png", "file.png")]
        [TestCase("name.", "name")]
        public void CleanFileName_ReturnsSafeName(string uploadedName, string expected)
        {
            Assert.That(FileBrowserService.CleanFileName(uploadedName), Is.EqualTo(expected));
        }

        [TestCase("photo.jpg", true)]
        [TestCase("a&b.jpg", true)]
        [TestCase("a\"b.jpg", false)]
        [TestCase("a<b.jpg", false)]
        [TestCase("", false)]
        public void IsValidName_AcceptsExistingNamesTheBrowserCanShow(string name, bool expected)
        {
            Assert.That(FileBrowserService.IsValidName(name), Is.EqualTo(expected));
        }

        [TestCase("Banners", true)]
        [TestCase("a&b", false)]
        [TestCase("a#b", false)]
        [TestCase("..", false)]
        [TestCase(" padded", false)]
        [TestCase("a/b", false)]
        public void IsValidNewName_RejectsNamesThatBreakUrls(string name, bool expected)
        {
            Assert.That(FileBrowserService.IsValidNewName(name), Is.EqualTo(expected));
        }

        [Test]
        public void GetBrowsableFolder_ReturnsFolder_WhenUserCanBrowse()
        {
            var folder = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanBrowse(folder)).Returns(true);

            Assert.That(this.service.GetBrowsableFolder(PortalId, "/Images"), Is.SameAs(folder));
        }

        [Test]
        public void GetBrowsableFolder_ReturnsNull_WhenUserCannotBrowse()
        {
            this.SetUpFolder("Images/");

            Assert.That(this.service.GetBrowsableFolder(PortalId, "Images/"), Is.Null);
        }

        [Test]
        public void GetBrowsableFolder_ReturnsNull_WhenFolderDoesNotExist()
        {
            this.permissions.Setup(p => p.CanBrowse(It.IsAny<IFolderInfo>())).Returns(true);

            Assert.That(this.service.GetBrowsableFolder(PortalId, "Missing/"), Is.Null);
        }

        [Test]
        public void GetSubfolderNames_ListsOnlyBrowsableSubfolders_WithParentLinkBelowRoot()
        {
            var parent = this.SetUpFolder("Images/");
            var visible = this.SetUpFolder("Images/Banners/", "Banners");
            var hidden = this.SetUpFolder("Images/Private/", "Private");
            var alsoVisible = this.SetUpFolder("Images/Archive/", "Archive");
            this.folderManager.Setup(m => m.GetFolders(parent)).Returns(new[] { visible, hidden, alsoVisible, });
            this.permissions.Setup(p => p.CanBrowse(visible)).Returns(true);
            this.permissions.Setup(p => p.CanBrowse(alsoVisible)).Returns(true);

            Assert.That(this.service.GetSubfolderNames(parent), Is.EqualTo(new[] { "..", "Archive", "Banners", }));
        }

        [Test]
        public void GetSubfolderNames_HasNoParentLink_AtRoot()
        {
            var root = this.SetUpFolder(string.Empty);
            this.folderManager.Setup(m => m.GetFolders(root)).Returns(new IFolderInfo[0]);

            Assert.That(this.service.GetSubfolderNames(root), Is.Empty);
        }

        [Test]
        public void GetFiles_FiltersImagesAndUnsafeNames()
        {
            var folder = this.SetUpFolder("Images/");
            this.folderManager.Setup(m => m.GetFiles(folder)).Returns(new[] { CreateFile("logo.png"), CreateFile("report.pdf"), CreateFile("bad\".jpg"), });

            Assert.Multiple(() =>
            {
                Assert.That(this.service.GetFiles(folder, false).Select(f => f.FileName), Is.EqualTo(new[] { "logo.png", "report.pdf", }));
                Assert.That(this.service.GetFiles(folder, true).Select(f => f.FileName), Is.EqualTo(new[] { "logo.png", }));
            });
        }

        [Test]
        public void GetPermissions_ReflectsFolderPermissions_AndDisablesUnsupportedActions()
        {
            var folder = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanBrowse(folder)).Returns(true);
            this.permissions.Setup(p => p.CanAddFiles(folder)).Returns(true);

            var result = this.service.GetPermissions(folder);

            Assert.Multiple(() =>
            {
                Assert.That(result["allowFiles"], Is.True);
                Assert.That(result["allowFileUpload"], Is.True);
                Assert.That(result["allowFileRemove"], Is.False);
                Assert.That(result["allowFolderCreate"], Is.False);
                Assert.That(result["allowFileRename"], Is.False);
                Assert.That(result["allowFolderRemove"], Is.False);
                Assert.That(result["allowImageCrop"], Is.False);
            });
        }

        [Test]
        public void HasDirectUrls_IsTrueOnlyForStandardFolders()
        {
            var standard = this.SetUpFolder("Images/");
            var secure = this.SetUpFolder("Secure/", mappingId: SecureMappingId);

            Assert.Multiple(() =>
            {
                Assert.That(this.service.HasDirectUrls(standard), Is.True);
                Assert.That(this.service.HasDirectUrls(secure), Is.False);
            });
        }

        [Test]
        public void AddFile_IsDenied_WithoutAddPermission()
        {
            var folder = this.SetUpFolder("Images/");

            var result = this.service.AddFile(folder, "photo.jpg", new MemoryStream(), UserId, null);

            Assert.That(result.Error, Is.EqualTo(FileBrowserError.PermissionDenied));
            this.fileManager.Verify(m => m.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void AddFile_SavesUnderAFreeName_WithoutOverwriting()
        {
            var folder = this.SetUpFolder("Images/");
            var saved = CreateFile("photo_2.jpg");
            this.permissions.Setup(p => p.CanAddFiles(folder)).Returns(true);
            this.fileManager.Setup(m => m.FileExists(folder, "photo.jpg", true)).Returns(true);
            this.fileManager.Setup(m => m.FileExists(folder, "photo_1.jpg", true)).Returns(true);
            this.fileManager.Setup(m => m.AddFile(folder, "photo_2.jpg", It.IsAny<Stream>(), false, false, "application/octet-stream", UserId)).Returns(saved);

            var result = this.service.AddFile(folder, @"C:\fakepath\photo.jpg", new MemoryStream(), UserId, null);

            Assert.That(result.Value, Is.SameAs(saved));
        }

        [Test]
        public void AddFile_RejectsFileOverTheUploadLimit()
        {
            var folder = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanAddFiles(folder)).Returns(true);

            var result = this.service.AddFile(folder, "big.jpg", new MemoryStream(new byte[2049]), UserId, 2048);

            Assert.That(result.Error, Is.EqualTo(FileBrowserError.FileTooLarge));
            this.fileManager.Verify(m => m.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void AddFile_ReportsDisallowedExtension()
        {
            var folder = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanAddFiles(folder)).Returns(true);
            this.fileManager.Setup(m => m.AddFile(folder, "run.exe", It.IsAny<Stream>(), false, false, It.IsAny<string>(), UserId)).Throws<InvalidFileExtensionException>();

            var result = this.service.AddFile(folder, "run.exe", new MemoryStream(), UserId, null);

            Assert.That(result.Error, Is.EqualTo(FileBrowserError.ExtensionNotAllowed));
        }

        [Test]
        public void CreateFolder_IsDenied_WithoutWritePermission()
        {
            var parent = this.SetUpFolder("Images/");

            var result = this.service.CreateFolder(parent, "Banners");

            Assert.That(result.Error, Is.EqualTo(FileBrowserError.PermissionDenied));
            this.folderManager.Verify(m => m.AddFolder(It.IsAny<FolderMappingInfo>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void CreateFolder_RejectsInvalidName()
        {
            var parent = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanAddFolder(parent)).Returns(true);

            Assert.That(this.service.CreateFolder(parent, "../Secret").Error, Is.EqualTo(FileBrowserError.InvalidName));
        }

        [Test]
        public void CreateFolder_RejectsExistingFolder()
        {
            var parent = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanAddFolder(parent)).Returns(true);
            this.folderManager.Setup(m => m.FolderExists(PortalId, "Images/Banners/")).Returns(true);

            Assert.That(this.service.CreateFolder(parent, "Banners").Error, Is.EqualTo(FileBrowserError.AlreadyExists));
        }

        [Test]
        public void CreateFolder_AddsSubfolderWithParentMapping()
        {
            var parent = this.SetUpFolder("Images/");
            var created = this.SetUpFolder("Images/Banners/", "Banners");
            this.permissions.Setup(p => p.CanAddFolder(parent)).Returns(true);
            this.folderManager.Setup(m => m.AddFolder(It.Is<FolderMappingInfo>(f => f.FolderProviderType == "StandardFolderProvider"), "Images/Banners/")).Returns(created);

            Assert.That(this.service.CreateFolder(parent, "Banners").Value, Is.SameAs(created));
        }

        [Test]
        public void RemoveFile_IsDenied_WithoutWritePermission()
        {
            var folder = this.SetUpFolder("Images/");

            var result = this.service.RemoveFile(folder, "photo.jpg");

            Assert.That(result.Error, Is.EqualTo(FileBrowserError.PermissionDenied));
            this.fileManager.Verify(m => m.DeleteFile(It.IsAny<IFileInfo>()), Times.Never);
        }

        [Test]
        public void RemoveFile_DeletesFile()
        {
            var folder = this.SetUpFolder("Images/");
            var file = CreateFile("photo.jpg");
            this.permissions.Setup(p => p.CanDeleteFiles(folder)).Returns(true);
            this.fileManager.Setup(m => m.GetFile(folder, "photo.jpg")).Returns(file);

            var result = this.service.RemoveFile(folder, "photo.jpg");

            Assert.That(result.Succeeded, Is.True);
            this.fileManager.Verify(m => m.DeleteFile(file), Times.Once);
        }

        [Test]
        public void RemoveFile_ReportsMissingFile()
        {
            var folder = this.SetUpFolder("Images/");
            this.permissions.Setup(p => p.CanDeleteFiles(folder)).Returns(true);

            Assert.That(this.service.RemoveFile(folder, "missing.jpg").Error, Is.EqualTo(FileBrowserError.FileNotFound));
        }

        [Test]
        public void GetViewableFile_ReturnsFile_WhenUserCanViewFolder()
        {
            var folder = this.SetUpFolder("Secure/Docs/", mappingId: SecureMappingId);
            var file = CreateFile("plan.pdf");
            this.permissions.Setup(p => p.CanView(folder)).Returns(true);
            this.fileManager.Setup(m => m.GetFile(folder, "plan.pdf")).Returns(file);

            Assert.That(this.service.GetViewableFile(PortalId, "/Secure/Docs/plan.pdf"), Is.SameAs(file));
        }

        [Test]
        public void GetViewableFile_ReturnsNull_WhenUserCannotViewFolder()
        {
            var folder = this.SetUpFolder("Secure/Docs/", mappingId: SecureMappingId);
            this.fileManager.Setup(m => m.GetFile(folder, "plan.pdf")).Returns(CreateFile("plan.pdf"));

            Assert.That(this.service.GetViewableFile(PortalId, "Secure/Docs/plan.pdf"), Is.Null);
        }

        private static IFileInfo CreateFile(string fileName)
        {
            var file = new Mock<IFileInfo>();
            file.SetupGet(f => f.FileName).Returns(fileName);
            return file.Object;
        }

        private IFolderInfo SetUpFolder(string folderPath, string folderName = null, int mappingId = StandardMappingId)
        {
            var folder = new Mock<IFolderInfo>();
            folder.SetupGet(f => f.FolderPath).Returns(folderPath);
            folder.SetupGet(f => f.FolderName).Returns(folderName ?? folderPath.TrimEnd('/').Split('/').Last());
            folder.SetupGet(f => f.PortalID).Returns(PortalId);
            folder.SetupGet(f => f.FolderMappingID).Returns(mappingId);
            this.folderManager.Setup(m => m.GetFolder(PortalId, folderPath)).Returns(folder.Object);
            return folder.Object;
        }
    }
}
