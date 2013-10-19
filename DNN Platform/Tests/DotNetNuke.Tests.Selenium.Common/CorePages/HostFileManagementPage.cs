using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostFileManagementPage : BasePage
	{
		public HostFileManagementPage(IWebDriver driver) : base(driver) { }

		public static string HostDigitalAssetManagementUrl = "/Host/FileManagement";

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

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostDigitalAssetManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.DigitalAssetLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostCommonSettings,
			                    ControlPanelIDs.HostFileManagementOption);
		}
	}
}
