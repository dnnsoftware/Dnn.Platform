using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class AdminSearchAdminPage : BasePage
	{
		public AdminSearchAdminPage(IWebDriver driver) : base(driver) { }

		public static string AdminSearchAdminUrl = "/Admin/SearchAdmin";

		public static string PageTitleLabel = "Search Admin";
		public static string PageHeader = "Search Admin";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminSearchAdminUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.SearchAdminLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminAdvancedSettings, BasePage.AdminSearchAdminOption);

		}
	}
}
