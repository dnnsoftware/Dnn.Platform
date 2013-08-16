using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class AdminVendorsPage : BasePage
	{
		public AdminVendorsPage(IWebDriver driver) : base(driver) { }

		public static string AdminVendorsUrl = "/Admin/Vendors";

		public static string PageTitleLabel = "Vendors";
		public static string PageHeader = "Vendors";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminVendorsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.VendorsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminAdvancedSettings, BasePage.AdminVendorsOption);
		}
	}
}
