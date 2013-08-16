using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class FileUploadPage : BasePage
	{
		public FileUploadPage(IWebDriver driver) : base(driver) { }

		public static string FileUploadUrl = "/Home/ctl/WebUpload";

		public static string PageTitleLabel = "Upload File";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + FileUploadUrl);
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");

			Trace.WriteLine(TraceLevelElement + "Select menu option: '" + ControlPanelToolsOption + "'");

			Click(By.XPath(ControlPanelToolsOption));
			
			FindElement(By.XPath(ToolsGoButton)).WaitTillVisible();

			FindElement(By.XPath(ToolsFileUploadOption)).WaitTillVisible().Click();
		}
	}
}

