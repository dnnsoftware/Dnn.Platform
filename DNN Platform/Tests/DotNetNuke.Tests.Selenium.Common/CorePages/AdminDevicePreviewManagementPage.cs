using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	internal class AdminDevicePreviewManagementPage : BasePage
	{
		public static string AdminDevicePreviewManagementUrl = "/Admin/DevicePreviewManagement";

		public override string PageTitleLabel
		{
			get { return "Device Preview Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Device Preview Management"; }
		}

		public override string PreLoadedModule
		{
			get { return "DevicePreviewModule"; }
		}

		public AdminDevicePreviewManagementPage(IWebDriver driver) : base(driver)
		{
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminDevicePreviewManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.DevicePreviewManagementLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminDevicePreviewManagementOption);
		}
	}
}