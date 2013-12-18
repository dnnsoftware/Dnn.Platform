using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DNNSelenium.Common.CorePages;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.BaseClasses.BasePages
{
	public class FileManagementPage : BasePage
	{
		public FileManagementPage(IWebDriver driver) : base(driver)
		{
		}

		public static string FolderTreeView = "//div[contains(@id, '_View_FolderTreeView')]";
		public static string FileView = "//div[contains(@id, '_View_Grid')]";
		public static string FileViewItems = FileView + "//tbody/tr[contains(@id, '_View_Grid_')]";
		public static string PageSizeArrow = "//a[contains(@id, '_PageSizeComboBox_Arrow')]";
		public static string PageSizeDropDown = "//div[contains(@id, 'PageSizeComboBox_DropDown')]";

		public static string DeleteButton = "//button[@id = 'DigitalAssetsDeleteBtnId']";
		public static string RenameButton = "//button[@id = 'DigitalAssetsRenameBtnId']";
		public static string CreateFolderButton = "//button[@id = 'DigitalAssetsCreateFolderBtnId']/span";
		public static string CreateNewFolderPopup = "//div[@aria-describedby = 'dnnModuleDigitalAssetsCreateFolderModal']";
		public static string FolderNameTextBox = "//input[contains(@id, '_FolderNameTextBox')]";
		public static string FolderTypeArrow = "//a[contains(@id, '_View_FolderTypeComboBox_Arrow')]";
		public static string FolderTypeDropDown = "//div[contains(@id, '_FolderTypeComboBox_DropDown')]";
		public static string SaveButton = "//button[@id = 'save_button']";
		public static string CopyFilesButton = "//button[@id = 'destination_button']";

		public static string UploadFileButton = "//button[@id = 'DigitalAssetsUploadFilesBtnId']";

		public static string FileSearchBox = "//div[@id = 'dnnModuleDigitalAssetsSearchBox']/input";
		public static string FileSearchIcon = "//div[@id = 'dnnModuleDigitalAssetsSearchBox']/a";

		public static string ContextMenuCreateFolderOption = "//a[contains(@class, 'permission_ADD')]/span[text() = 'Create Folder']";
		public static string ContextMenuDeleteFolderOption = "//a[contains(@class, 'permission_DELETE')]/span[text() = 'Delete Folder']";
		public static string ContextMenuMoveFolderOption = "//a[contains(@class, 'permission_COPY')]/span[text() = 'Move']";
		public static string ContextMenuMoveOption = "//a[contains(@class, 'permission_COPY')]/span[text() = 'Move']";
		public static string ContextMenuCopyOption = "//a[contains(@class, ' permission_COPY onlyFiles')]/span[text() = 'Copy']";
		public static string UploadFilesPopup = "//div[@aria-describedby = 'dnnModuleDigitalAssetsUploadFileModal']";
		public static string ChooseFilesButton = "//input[@id = 'dnnModuleDigitalAssetsUploadFileDialogInput']";
		public static string CloseButton = "//a[@id = 'dnnModuleDigitalAssetsUploadFileDialogClose']";

		public static string ZipFilePopup = "//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]/div[contains(text(), 'compressed zip file')]";
		public static string KeepCompressedButton = "//button[@id = 'keepCompressed_button']";
		public static string ExpandButton = "//button[@id = 'expandFile_button']";
		public static string FinishedUploadFlag = "//div[@class = 'dnnModuleDigitalAssetsUploadFileProgress']/div[contains(@class, 'finished')]";

		public static string PopupDeleteButton =
			"//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]//button[@type = 'button']/span[contains(text(), 'Delete')]";

		public static string ManageFolderTypesOption = "//span[text() = 'Manage Folder Types']";

		private void FillFolderInfo(string folderType, string folderName)
		{
			WaitForElement(By.XPath(CreateNewFolderPopup)).WaitTillVisible();

			WaitAndType(By.XPath(FolderNameTextBox), folderName);

			if (FindElement(By.XPath(FolderTypeArrow)).Displayed)
			{
				SlidingSelectByValue(By.XPath(FolderTypeArrow), By.XPath(FolderTypeDropDown), folderType);
			}

			Click(By.XPath(SaveButton));

			Thread.Sleep(1000);
		}


		public void CreateFolder(string folderType, string folderName)
		{
			Trace.WriteLine(TraceLevelPage + "Create Folder:" + folderName);

			Thread.Sleep(1000);

			WaitForElement(By.XPath(FileViewItems + "[last()]"));

			WaitForElement(By.XPath((CreateFolderButton))).WaitTillEnabled().Click();

			FillFolderInfo(folderType, folderName);
		}


		public void CreateSubFolder(string folderName, string subFolderType, string subFolderName)
		{
			Trace.WriteLine(TraceLevelComposite + "Create Subfolder:" + subFolderName);

			Thread.Sleep(1000);

			WaitForElement(By.XPath(FileViewItems + "[last()]"));

			TreeViewSelectFromContextMenu(folderName, ContextMenuCreateFolderOption);

			FillFolderInfo(subFolderType, subFolderName);
		}


		public void TreeViewSelectFromContextMenu(string folderName, string option)
		{
			Trace.WriteLine(TraceLevelComposite + "Select from Context menu:");

			WaitForElement(By.XPath(FolderTreeView + "//li[@title = '" + folderName + "']")).Info();

			Actions builder = new Actions(_driver);
			builder.ContextClick(
				FindElement(By.XPath(FolderTreeView + "//li/div/span[text() = '" + folderName + "']"))).
				MoveToElement(WaitForElement(By.XPath(option)).WaitTillEnabled()).Build().Perform();

			Thread.Sleep(1000);

			Trace.WriteLine(TraceLevelPage + "Click on option: " + option);
			Click(By.XPath(option));
		}


		public void FileViewSelectFromContextMenu(string fileName, string option)
		{
			Trace.WriteLine(TraceLevelComposite + "Select from Context menu:");

			WaitForElement(By.XPath(FileView + "//tr/td/div[@title = '" + fileName + "']")).Info();

			Actions builder = new Actions(_driver);
			builder.ContextClick(
				FindElement(By.XPath(FileView + "//tr/td/div[@title = '" + fileName + "']"))).
				MoveToElement(WaitForElement(By.XPath(option)).WaitTillEnabled()).Build().Perform();

			Thread.Sleep(1000);

			Trace.WriteLine(TraceLevelPage + "Click on option: " + option);
			WaitAndClick(By.XPath(option));
		}


		public void SelectSubFolderFromTreeView(string folderName, string subFolderName)
		{
			Trace.WriteLine(TraceLevelComposite + "Select the Subfolder from treeview : " + folderName + "/" + subFolderName);

			if (ElementPresent(By.XPath(FolderTreeView + "//li/div[span[text() = '" + folderName + "']]/span[@class = 'rtPlus']")))
			{
				Click(By.XPath(FolderTreeView + "//li/div[span[text() = '" + folderName + "']]/span[@class = 'rtPlus']"));
			}

			FindElement(By.XPath(FolderTreeView + "//li/div/span[text() = '" + subFolderName + "']")).WaitTillEnabled().Click();

			Thread.Sleep(1000);

			WaitForElement(By.XPath("//div[contains(@class, 'dnnModuleDigitalAssetsMainLoading') and @style= 'display: none;']"), 60);

			WaitForElement(By.XPath(FileViewItems + "[last()]"));
		}


		public void SelectFolderFromTreeView(string folderName, string subFolderName)
		{
			Trace.WriteLine(TraceLevelComposite + "Select the Folder from treeview : " + folderName);

			//WaitForElement(By.XPath(FolderTreeView + "//li/div/span[text() = '" + folderName + "']")).ScrollIntoView().Click();

			//WaitForElement(By.XPath(FileViewItems + "[last()]"));

			//Thread.Sleep(1000);

			if (ElementPresent(By.XPath(FolderTreeView + "//li/div[span[text() = '" + folderName + "']]/span[@class = 'rtPlus']")))
			{
				Click(By.XPath(FolderTreeView + "//li/div[span[text() = '" + folderName + "']]/span[@class = 'rtPlus']"));
			}

			FindElement(By.XPath(FolderTreeView + "//li/div/span[text() = '" + subFolderName + "']")).WaitTillEnabled().Click();

			Thread.Sleep(1000);

			WaitForElement(By.XPath("//div[contains(@class, 'dnnModuleDigitalAssetsMainLoading') and @style= 'display: none;']"), 60);

			WaitForElement(By.XPath(FileViewItems + "[last()]"));

		}


		public void UploadFileToFolder(string folderName, string fileToUpload)
		{
			Trace.WriteLine(TraceLevelComposite + "Upload file: " + fileToUpload + " to folder " + folderName);

			WaitForElement(By.XPath(UploadFileButton)).WaitTillEnabled(60).Click();

			WaitForElement(By.XPath(UploadFilesPopup));

			WaitForElement(By.XPath(ChooseFilesButton)).SendKeys(Path.GetFullPath(@"P1\FilesToUpload\" + fileToUpload));

			WaitForElement(By.XPath(FinishedUploadFlag));

			Click(By.XPath(CloseButton));
		}


		public void UploadZipFileToFolder(string folderName, string fileToUpload, By actionButton)
		{
			Trace.WriteLine(TraceLevelComposite + "Upload ZIP file: " + fileToUpload + " to folder " + folderName);

			WaitAndClick(By.XPath(UploadFileButton));

			WaitForElement(By.XPath(UploadFilesPopup));

			WaitForElement(By.XPath(ChooseFilesButton)).SendKeys(Path.GetFullPath(@"P1\FilesToUpload\" + fileToUpload));

			WaitForElement(By.XPath(ZipFilePopup));

			WaitAndClick(actionButton);

			WaitForElement(By.XPath(FinishedUploadFlag));

			Click(By.XPath(CloseButton));
		}


		public void UploadNotAllowedFileType(string folderName, string fileToUpload)
		{
			Trace.WriteLine(TraceLevelComposite + "Upload file: " + fileToUpload + " to folder " + folderName);

			WaitAndClick(By.XPath(UploadFileButton));

			WaitForElement(By.XPath(UploadFilesPopup));

			WaitForElement(By.XPath(ChooseFilesButton)).SendKeys(Path.GetFullPath(@"P1\FilesToUpload\" + fileToUpload));

			WaitForElement(By.XPath("//span[contains(@class, 'dnnModuleDigitalAssetsUploadFileFileName')]"));

		}


		public void SetItemsPerPage(string itemsPerPage)
		{
			Trace.WriteLine(TraceLevelComposite + "Set number of items per a page: " + itemsPerPage);

			WaitForElement(By.XPath(PageSizeArrow)).ScrollIntoView();

			SlidingSelectByValue(By.XPath(PageSizeArrow), By.XPath(PageSizeDropDown), itemsPerPage);

			Thread.Sleep(1000);
		}


		public void MoveFolderFromTreeView(string folderName, string folderTo)
		{
			Trace.WriteLine(TraceLevelComposite + "Move folder: " + folderName);

			TreeViewSelectFromContextMenu(folderName, ContextMenuMoveFolderOption);

			WaitForConfirmationBox(15);

			WaitForElement(By.XPath("//div[contains(@id, '_View_DestinationTreeView')]//span[contains(text(), '" + folderTo + "')]")).ScrollIntoView().Click();

			FindElement(By.XPath(CopyFilesButton)).WaitTillEnabled().Click();

			Thread.Sleep(2000);
		}


		public void DeleteFolderFromTreeView(string folderName)
		{
			Trace.WriteLine(TraceLevelComposite + "Delete folder: " + folderName);

			TreeViewSelectFromContextMenu(folderName, ContextMenuDeleteFolderOption);

			WaitForConfirmationBox(15);

			WaitForElement(By.XPath(PopupDeleteButton)).Click();

			Thread.Sleep(2000);
		}


		public void DeleteUsingToolBar(string folderName, string folderNameToDelete)
		{
			Trace.WriteLine(TraceLevelComposite + "Delete folder: " + folderName + "/" + folderNameToDelete);

			CheckBoxCheck(By.XPath(FileView + "//tr[td/div[@title = '" + folderNameToDelete + "']]/td/input"));

			WaitAndClick(By.XPath(DeleteButton));

			WaitForConfirmationBox(15);

			WaitForElement(By.XPath(PopupDeleteButton)).Click();

			Thread.Sleep(2000);
		}


		public void RenameUsingToolBar(string folderName, string folderNameToEdit, string newName)
		{
			Trace.WriteLine(TraceLevelComposite + "Rename folder: " + folderName + "/" + folderNameToEdit);

			CheckBoxCheck(By.XPath(FileView + "//tr[td/div[@title = '" + folderNameToEdit + "']]/td/input"));

			WaitAndClick(By.XPath(RenameButton));

			WaitAndClear(By.XPath("//input[contains(@id, '_ItemNameEdit')]"));

			Type(By.XPath("//input[contains(@id, '_ItemNameEdit')]"), newName);

			FindElement(By.XPath("//input[contains(@id, '_ItemNameEdit')]")).SendKeys(Keys.Enter);

			Thread.Sleep(1000);
		}


		public void MoveFile(string folderFrom, string folderTo, string fileToMove)
		{
			Trace.WriteLine(TraceLevelComposite + "MOVE the file " + fileToMove + " from folder " + folderFrom + " to folder " + folderTo);

			//SelectFolderFromTreeView("Root", folderFrom);

			FileViewSelectFromContextMenu(fileToMove, ContextMenuMoveOption);

			WaitForConfirmationBox(15);

			WaitForElement(By.XPath("//div[contains(@id, '_View_DestinationTreeView')]//span[contains(text(), '" + folderTo + "')]")).ScrollIntoView().Click();

			FindElement(By.XPath(CopyFilesButton)).WaitTillEnabled().Click();

			Thread.Sleep(1000);
		}


		public void CopyFile(string folderFrom, string folderTo, string fileToMove)
		{
			Trace.WriteLine(TraceLevelComposite + "COPY the file " + fileToMove + " from folder " + folderFrom + " to folder " + folderTo);

			//SelectFolderFromTreeView("Root", folderFrom);

			FileViewSelectFromContextMenu(fileToMove, ContextMenuCopyOption);

			WaitForConfirmationBox(15);

			WaitForElement(By.XPath("//div[contains(@id, '_View_DestinationTreeView')]//span[contains(text(), '" + folderTo + "')]")).ScrollIntoView().Click();

			FindElement(By.XPath(CopyFilesButton)).WaitTillEnabled().Click();

			Thread.Sleep(1000);
		}


		public void SearchForFile(string fileToSearch)
		{
			Trace.WriteLine(TraceLevelPage + "Search for file:" + fileToSearch);
			WaitAndType(By.XPath(FileSearchBox), fileToSearch);
			FindElement(By.XPath(FileSearchIcon)).Click();

			Thread.Sleep(1000);
		}
	}
}
