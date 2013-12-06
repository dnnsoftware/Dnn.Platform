using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Platform.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.CorePages
{
	class AdminGoogleAnalyticsPage : BasePage
	{
		public AdminGoogleAnalyticsPage(IWebDriver driver) : base(driver) { }

		public static string AdminGoogleAnalyticsUrl = "/Admin/GoogleAnalytics";

		public override string PageTitleLabel
		{
			get { return "Google Analytics"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Google Analytics"; }
		}

		public override string PreLoadedModule
		{
			get { return "GoogleAnalyticsModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminGoogleAnalyticsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.GoogleAnalyticsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, BaseClasses.PlatformControlPanelIDs.AdminGoogleAnalyticsOption);
		}
	}
}
