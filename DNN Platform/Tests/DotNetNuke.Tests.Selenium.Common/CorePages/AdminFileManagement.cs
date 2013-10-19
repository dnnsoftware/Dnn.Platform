using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminFileManagementPage : BasePage
	{
		public AdminFileManagementPage(IWebDriver driver) : base(driver) { }

		public static string AdminDigitalAssetsManagementUrl = "/Admin/FileManagement";

		public override string PageTitleLabel
		{
			get { return "File Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "File Management"; }
		}

		public override string PreLoadedModule
		{
			get { return "DigitalAssetManagementModule"; }
		}

		public static string FileSearchBox = "//div[@id = 'dnnModuleDigitalAssetsSearchBox']/input";
		public static string FileSearchIcon = "//div[@id = 'dnnModuleDigitalAssetsSearchBox']/a";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminDigitalAssetsManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.DigitalAssetsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminCommonSettings, ControlPanelIDs.AdminFileManagementOption);
		}

		public void SearchForFile(string fileToSearch)
		{
			Type(By.XPath(AdminFileManagementPage.FileSearchBox), fileToSearch);
			FindElement(By.XPath(AdminFileManagementPage.FileSearchIcon)).Click();

			Thread.Sleep(1000);
		}
	}
}
