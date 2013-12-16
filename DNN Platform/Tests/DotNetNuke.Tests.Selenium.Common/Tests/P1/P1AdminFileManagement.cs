using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1AdminFileManagement : CommonTestSteps
	{
		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("adminFileManagement");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();

		}

		//[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		public void Test_CreateFolder(string folderType, string folderName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Create Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.CreateFolder(folderType, folderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the folder is present in the list");
			Assert.IsTrue(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FolderTreeView + "//li/div/span[text() = '" + folderName + "']")),
						"The Folder is not created correctly");
		}

		public void Test_CreateSubFolder(string subFolderType, string folderName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Create SubFolder:'");

			string subFolderName = folderName + subFolderType + "SubFolder";

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.CreateSubFolder(folderName, subFolderType, subFolderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the subfolder is present in the list");
			Assert.IsTrue(adminFileManagementPage.ElementPresent(
						By.XPath(FileManagementPage.FolderTreeView + "//li[@title = '" + folderName + "']/ul/li[@title = '" + subFolderName + "']")),
						"The Subfolder is not created correctly");
		}

		public void Test_DeleteFolderUsingToolBar(string folderName, string nameToDelete)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.DeleteUsingToolBar(folderName, nameToDelete);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the Folder is NOT present in the list");
			Assert.IsFalse(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//trtr[contains(@style, 'visibility: visible;')]/td/div[@title = '" + nameToDelete + "']")),
						"The Folder is not deleted correctly");
		}

		public void Test_RenameFolderUsingToolBar(string folderName, string nameToEdit, string newName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Rename Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.RenameUsingToolBar(folderName, nameToEdit, newName);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the Folder is NOT present in the list");
			Assert.IsFalse(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//tr/td/div[@title = '" + nameToEdit + "']")),
						"The Folder is not renamed correctly");
		}

		public void Test_DeleteFolder(string folderName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.DeleteFolderFromTreeView(folderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the folder is NOT present in the list");
			Assert.IsFalse(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FolderTreeView + "//li/div/span[text() = '" + folderName + "']")),
						"The Folder is not deleted correctly");
		}

		public void Test_UploadFileToFolder(string folderName, string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadFileToFolder(folderName, fileToUpload);

			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + fileToUpload + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");
		}

		public void Test_UploadZipFileToFolder(string folderName, string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload ZIP File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadZipFileToFolder(folderName, fileToUpload, By.XPath(FileManagementPage.KeepCompressedButton));

			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + fileToUpload + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");
		}

		public void Test_UploadFileToSubFolder(string folderName, string subFolderName, string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectSubFolderFromTreeView(folderName, subFolderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadFileToFolder(folderName, fileToUpload);

			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectSubFolderFromTreeView(folderName, subFolderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + fileToUpload + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");
		}

		public void Test_UploadZipFileToSubfolder(string folderName, string subFolderName, string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload ZIP File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectSubFolderFromTreeView(folderName, subFolderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadZipFileToFolder(folderName, fileToUpload, By.XPath(FileManagementPage.KeepCompressedButton));

			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectSubFolderFromTreeView(folderName, subFolderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + fileToUpload + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");
		}

		public void Test_UploadDecompressedZipFileToFolder(string folderName, string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload decompressed ZIP File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadZipFileToFolder(folderName, fileToUpload, By.XPath(FileManagementPage.ExpandButton));

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the subfolder is present in the list");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "/span[span[text() = '" + fileToUpload.Replace(".zip", "") + "']]/img")).GetAttribute("src"),
						Is.StringContaining("/Folder"),
						"The Subfolder is not created correctly");

			adminFileManagementPage.SelectSubFolderFromTreeView(folderName, fileToUpload.Replace(".zip", ""));

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + "Owl.jpg" + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + "Owls.jpg" + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + "OwlToo.jpg" + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");

			adminFileManagementPage.SelectSubFolderFromTreeView(fileToUpload.Replace(".zip", ""), fileToUpload.Replace("Folder.zip", "") + "SubFolder");

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + "bird01.jpg" + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + "bird02.jpg" + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the size of uploaded file is correct");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath(FileManagementPage.FileView + "//tr[td/div[@title = '" + "bird03.jpg" + "']]/td[@class = 'dnnModuleDigitalAssetsGrid-SizeColumn']")).Text,
						Is.Not.EqualTo("0.0 KB"),
						"The File is not loaded correctly");

		}

		public void Test_UploadNotAllowedToFolder(string folderName, string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload Not Allowed File type to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadNotAllowedFileType(folderName, fileToUpload);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the warning message is displayed");
			Assert.That(adminFileManagementPage.WaitForElement(
						By.XPath("//span[@class = 'dnnModuleDigitalAssetsErrorMessage']")).Text,
						Is.EqualTo("File extension not allowed"),
						"The warning message is not displayed");
		}

		[TestCase("Standard", "StandardFolder")]
		[TestCase("Secure", "SecureFolder")]
		[TestCase("Database", "DatabaseFolder")]
		public void Test001_CreateFolder(string folderType, string folderName)
		{
			Test_CreateFolder(folderType, folderName);
		}

		[Test, Combinatorial]
		public void Test002_CreateSubFolder(
			[ValuesAttribute("StandardFolder", 
							"SecureFolder", 
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("Standard", 
							"Secure", 
							"Database")] string subFolderType)
		{
			Test_CreateSubFolder(subFolderType, folderName);
		}

		
		[Test, Combinatorial]
		public void Test003_UploadFile(
			[ValuesAttribute("StandardFolder", 
							"SecureFolder", 
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("Birds_files.bmp",
							"Birds_files.docx",
							"Birds_files.gif",
							"Birds_files.jpg",
							"Birds_files.mp3",
							"Birds_files.pdf",
							"Birds_files.png",
							"Birds_files.pptx",
							"Birds_files.rar",
							"Birds_files.swf",
							"Birds_files.txt",
							"Birds_files.xlsx",
							"Birds_files.xml")] string fileToUpload)
		{
			Test_UploadFileToFolder(folderName, fileToUpload);
		}

		[Test, Combinatorial]
		public void Test004_UploadZipFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,

			[ValuesAttribute("Birds_files.zip")] string fileToUpload)
		{
			Test_UploadZipFileToFolder(folderName, fileToUpload);
		}

		[Test, Combinatorial]
		public void Test005_UploadDecompressedZipFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("BirdsFolder.zip")] string fileToUpload)
		{
			Test_UploadDecompressedZipFileToFolder(folderName, fileToUpload);
		}

		[Test, Combinatorial]
		public void Test006_UploadNotAllowedFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("Birds_files.exe",
							 "Birds_files.log.resources",
							 "Birds_files")] string fileToUpload)
		{
			Test_UploadNotAllowedToFolder(folderName, fileToUpload);
		}

		[Test, Combinatorial]
		public void Test007_UploadFileToSubFolder(
			[ValuesAttribute("StandardFolder")] string folderName,
			[ValuesAttribute("StandardFolderStandardSubFolder")] string subFolderName,
			[ValuesAttribute("Birds_files.bmp",
							"Birds_files.docx",
							"Birds_files.gif",
							"Birds_files.jpg",
							"Birds_files.mp3",
							"Birds_files.pdf",
							"Birds_files.png",
							"Birds_files.pptx",
							"Birds_files.rar",
							"Birds_files.swf",
							"Birds_files.txt",
							"Birds_files.xlsx",
							"Birds_files.xml")] string fileToUpload)
		{
			Test_UploadFileToSubFolder(folderName, subFolderName, fileToUpload);
		}

		[Test, Combinatorial]
		public void Test008_UploadZipFileToSubFolder(
			[ValuesAttribute("StandardFolder")] string folderName,
			[ValuesAttribute("StandardFolderStandardSubFolder")] string subFolderName,
			[ValuesAttribute("Birds_files.zip")] string fileToUpload)
		{
			Test_UploadZipFileToSubfolder(folderName, subFolderName, fileToUpload);
		}

		[TestCase("Root", "*.pdf", 4)]
		[TestCase("Root", "Awesome-Cycles-Logo.png", 2)]
		[TestCase("StandardFolder", "*.txt", 2)]
		[TestCase("Images", "Awesome-Cycles-Logo.png", 1)]
		public void Test009_Search(string folderName, string pattern, int results)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'SEARCH:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);
			
			adminFileManagementPage.SearchForFile(pattern);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the results");
			Assert.That(adminFileManagementPage.FindElements(By.XPath(FileManagementPage.FileViewItems + "[contains(@style, 'visibility: visible;')]")).Count, Is.EqualTo(results),
				"The number of results is not correct");

		}

		[Test, Combinatorial]
		public void Test010_DeleteFileUsingToolBar(
			[ValuesAttribute("StandardFolder")] string folderName,

			[ValuesAttribute("Birds_files.bmp",
							"Birds_files.docx",
							"Birds_files.gif",
							"Birds_files.jpg",
							"Birds_files.mp3",
							"Birds_files.pdf",
							"Birds_files.png",
							"Birds_files.pptx",
							"Birds_files.rar",
							"Birds_files.swf",
							"Birds_files.txt",
							"Birds_files.xlsx",
							"Birds_files.xml")] string nameToDelete)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete File:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.DeleteUsingToolBar(folderName, nameToDelete);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the File is NOT present in the list");
			Assert.IsFalse(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//tr/td/div[@title = '" + nameToDelete + "']")),
						"The File is not deleted correctly");
		}

		[Test, Combinatorial]
		public void Test011_DeleteFolderUsingToolBar(
			[ValuesAttribute("StandardFolder")] string folderName,

			[ValuesAttribute("StandardFolderStandardSubFolder",
							"StandardFolderSecureSubFolder",
							"StandardFolderDatabaseSubFolder")] string nameToDelete)
		{
			Test_DeleteFolderUsingToolBar(folderName, nameToDelete);
		}

		[TestCase("StandardFolder")]
		[TestCase("SecureFolder")]
		[TestCase("DatabaseFolder")]
		public void Test012_DeleteFolderFromTreeView(string folderName)
		{
			Test_DeleteFolder(folderName);
		}



		[TestCase("Standard", "StandardFolder")]
		[TestCase("Secure", "SecureFolder")]
		[TestCase("Database", "DatabaseFolder")]
		public void Test0301_MoveFilePreconditionsAddFolders(string folderType, string folderName)
		{
			CreateFolder(folderType, folderName);
		}



		[Test, Combinatorial]
		public void Test0302_MoveFile_PreconditionsAddFolders(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("Standard",
							"Secure",
							"Database")] string folderType)
		{
			CreateFolder(folderType, folderName + "MoveTo" + folderType + "Folder");
		}



		[Test, Combinatorial]
		public void Test0303_MoveFile_PreconditionsUploadFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string prefix,
			[ValuesAttribute("Birds.jpg",
							"Birds.pdf")] string fileToUpload)
		{
			UploadFileToFolder("Root", folderName, prefix + fileToUpload);
		}

		[Test, Combinatorial]
		public void Test0304_MoveFile_PreconditionsUploadZipFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string prefix,
			[ValuesAttribute("Birds.zip")] string fileToUpload)

		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadZipFileToFolder(folderName, prefix + fileToUpload, By.XPath(FileManagementPage.KeepCompressedButton));

		}

		[Test, Combinatorial]
		public void Test0305_MoveFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderFrom,
			[ValuesAttribute("SecureFolder",
							"StandardFolder",
							"DatabaseFolder")] string prefix,
			[ValuesAttribute("Birds.jpg",
							"Birds.pdf",
							"Birds.zip")] string fileToMove)
		{
		
			var folderTo = folderFrom + "MoveTo" + prefix;
			var fullFileNameToMove = prefix + fileToMove;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move File to Folder:'" + fullFileNameToMove);

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.MoveFile(folderFrom, folderTo, fullFileNameToMove);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderTo);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the File is present in the list");
			Assert.IsTrue(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//tr/td/div[@title = '" + fullFileNameToMove + "']")),
						"The File is not moved correctly");

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderFrom);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the File is NOT present in the list");
			Assert.IsFalse(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//tr[contains(@style, 'visibility: visible;')]/td/div[@title = '" + fullFileNameToMove + "']")),
						"The File is not moved correctly");
		}

		[TestCase("StandardFolder")]
		[TestCase("SecureFolder")]
		[TestCase("DatabaseFolder")]
		[TestCase("StandardFolderMoveToStandardFolder")]
		[TestCase("StandardFolderMoveToSecureFolder")]
		[TestCase("StandardFolderMoveToDatabaseFolder")]
		[TestCase("SecureFolderMoveToStandardFolder")]
		[TestCase("SecureFolderMoveToSecureFolder")]
		[TestCase("SecureFolderMoveToDatabaseFolder")]
		[TestCase("DatabaseFolderMoveToStandardFolder")]
		[TestCase("DatabaseFolderMoveToSecureFolder")]
		[TestCase("DatabaseFolderMoveToDatabaseFolder")]
		public void Test0306_MoveFile_Postconditions_DeleteFolderFromTreeView(string folderName)
		{
			DeleteFolder(folderName);
		}



		[TestCase("Standard", "StandardFolder")]
		[TestCase("Secure", "SecureFolder")]
		[TestCase("Database", "DatabaseFolder")]
		public void Test0201_CopyFilePreconditionsAddFolders(string folderType, string folderName)
		{
			CreateFolder(folderType, folderName);
		}



		[Test, Combinatorial]
		public void Test0202_CopyFile_PreconditionsAddFolders(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("Standard",
							"Secure",
							"Database")] string folderType)
		{
			CreateFolder(folderType, folderName + "CopyTo" + folderType + "Folder");
		}



		[Test, Combinatorial]
		public void Test0203_CopyFile_PreconditionsUploadFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string prefix,
			[ValuesAttribute("Birds.jpg",
							"Birds.pdf")] string fileToUpload)
		{
			UploadFileToFolder("Root", folderName, prefix + fileToUpload);
		}



		[Test, Combinatorial]
		public void Test0204_CopyFile_PreconditionsUploadZipFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderName,
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string prefix,
			[ValuesAttribute("Birds.zip")] string fileToUpload)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderName);

			adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadZipFileToFolder(folderName, prefix + fileToUpload, By.XPath(FileManagementPage.KeepCompressedButton));

		}

		[Test, Combinatorial]
		public void Test0205_CopyFile(
			[ValuesAttribute("StandardFolder",
							"SecureFolder",
							"DatabaseFolder")] string folderFrom,
			[ValuesAttribute("SecureFolder",
							"StandardFolder",
							"DatabaseFolder")] string prefix,
			[ValuesAttribute("Birds.jpg",
							"Birds.pdf",
							"Birds.zip")] string fileToMove)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Copy File to Folder:'" + prefix + fileToMove);

			var folderTo = folderFrom + "CopyTo" + prefix;

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SelectFolderFromTreeView("Root", folderFrom);

			adminFileManagementPage.CopyFile(folderFrom, folderTo, prefix + fileToMove);

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderTo);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the File is  present in the list");
			Assert.IsTrue(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//tr/td/div[@title = '" + prefix + fileToMove + "']")),
						"The File is not copied correctly");

			adminFileManagementPage.SelectFolderFromTreeView("Root", folderFrom);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the File is NOT present in the list");
			Assert.IsTrue(adminFileManagementPage.ElementPresent(By.XPath(FileManagementPage.FileView + "//tr/td/div[@title = '" + prefix + fileToMove + "']")),
						"The File is not copied correctly");
		}



		[TestCase("StandardFolder")]
		[TestCase("SecureFolder")]
		[TestCase("DatabaseFolder")]
		[TestCase("StandardFolderCopyToStandardFolder")]
		[TestCase("StandardFolderCopyToSecureFolder")]
		[TestCase("StandardFolderCopyToDatabaseFolder")]
		[TestCase("SecureFolderCopyToStandardFolder")]
		[TestCase("SecureFolderCopyToSecureFolder")]
		[TestCase("SecureFolderCopyToDatabaseFolder")]
		[TestCase("DatabaseFolderCopyToStandardFolder")]
		[TestCase("DatabaseFolderCopyToSecureFolder")]
		[TestCase("DatabaseFolderCopyToDatabaseFolder")]
		public void Test0206_CopyFile_Postconditions_DeleteFolderFromTreeView(string folderName)
		{
			DeleteFolder(folderName);
		}
	}
}
