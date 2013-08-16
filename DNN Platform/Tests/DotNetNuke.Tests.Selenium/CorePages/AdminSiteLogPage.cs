using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class AdminSiteLogPage : BasePage
	{
		public AdminSiteLogPage(IWebDriver driver) : base(driver) { }

		public static string AdminSiteLogUrl = "/Admin/SiteLog";

		public static string PageTitleLabel = "Site Log";
		public static string PageHeader = "Site Log";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminSiteLogUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.SiteLogLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminAdvancedSettings, BasePage.AdminSiteLogOption);

		}
	}
}
