using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostDeviceDetectionManagementPage : BasePage
	{
		public HostDeviceDetectionManagementPage(IWebDriver driver) : base(driver) { }

		public static string HostDeviceDetectionManagementUrl = "/Host/DeviceDetectionManagement";

		public override string PageTitleLabel
		{
			get { return "Device Detection"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Device Detection"; }
		}

		public override string PreLoadedModule
		{
			get { return "DeviceDetectionModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostDeviceDetectionManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.DeviceDetectionManagement));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostAdvancedSettings, ControlPanelIDs.HostDeviceDetectionManagementOption);
		}
	}
}
