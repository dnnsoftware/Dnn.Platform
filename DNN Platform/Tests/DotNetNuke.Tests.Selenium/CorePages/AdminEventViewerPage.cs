using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class AdminEventViewerPage : BasePage 
	{
		public AdminEventViewerPage(IWebDriver driver) : base(driver) { }

		public static string AdminEventViewerUrl = "/Admin/LogViewer";

		public static string PageTitleLabel = "Log Viewer";
		public static string PageHeader = "Event Viewer";

		public static string TypeDropDownArrow = "LogViewer_ddlLogType_Arrow";
		public static string TypeDropDownList = "//div[contains(@id, 'LogViewer_ddlLogType_DropDown')]";

		public static string NoLogItemsMessage = "//div[contains(@id, 'LogViewer_UP')]/div/span[contains(@id, 'lblMessage')]";
		public static string NoLogItemsMessageText = "There are no log items.";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminEventViewerUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.EventViewerLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminCommonSettings, BasePage.AdminEventViewerOption);

		}

		public void FilterByType(string type)
		{
			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + TypeDropDownArrow + "')]"), By.XPath(TypeDropDownList), type);

			WaitForElement(By.XPath("//div[contains(@class, 'dnnlvContent')]/div[last()]")).WaitTillEnabled();
		}
	}
}
