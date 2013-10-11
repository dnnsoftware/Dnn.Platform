using System.Diagnostics;
using System.IO;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class FileUploadPage : BasePage
	{
		public FileUploadPage(IWebDriver driver) : base(driver) { }

		public static string FileUploadUrl = "/Home/ctl/WebUpload";

		public override string PageTitleLabel
		{
			get { return "Upload File"; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public static string ChooseFileButton = "//input[contains(@id, '_WebUpload_cmdBrowse')]";
		public static string UploadFileButton = "//a[contains(@id, '_WebUpload_cmdAdd')]";
		public static string FolderDropDown = "//div[contains(@id, '_WebUpload_ddlFolders')]";

		public static string SuccessfulConfirmationMessage =
			"//div[contains(@class, 'dnnFormSuccess')]/span[contains(@id, '_lblMessage')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + FileUploadUrl);
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");

			Trace.WriteLine(TraceLevelElement + "Select menu option: '" + ControlPanelIDs.ControlPanelToolsOption + "'");

			Click(By.XPath(ControlPanelIDs.ControlPanelToolsOption));
			
			FindElement(By.XPath(ControlPanelIDs.ToolsGoButton)).WaitTillVisible();

			FindElement(By.XPath(ControlPanelIDs.ToolsFileUploadOption)).WaitTillVisible().Click();
		}

		public void UploadFile(string fileToUpload, string folder)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Upload File: ");

			WaitForElement(By.XPath(ChooseFileButton)).SendKeys(Path.GetFullPath(@"P1\" + fileToUpload));

			FolderSelectByValue(By.XPath(FolderDropDown), folder);

			Click(By.XPath(UploadFileButton));

			WaitForElement(By.XPath(SuccessfulConfirmationMessage));
		}
	}
}

