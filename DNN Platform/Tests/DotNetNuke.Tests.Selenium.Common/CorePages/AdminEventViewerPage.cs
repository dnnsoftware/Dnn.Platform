using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminEventViewerPage : BasePage 
	{
		public AdminEventViewerPage(IWebDriver driver) : base(driver) { }

		public static string AdminEventViewerUrl = "/Admin/LogViewer";

		public override string PageTitleLabel
		{
			get { return "Log Viewer"; }
		}
		
		public override string PageHeaderLabel
		{
			get { return "Event Viewer"; }
		}

		public override string PreLoadedModule
		{
			get { return "LogViewerModule"; }
		}

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
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.EventViewerLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminCommonSettings, ControlPanelIDs.AdminEventViewerOption);
		}

		public void FilterByType(string type)
		{
			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + TypeDropDownArrow + "')]"), By.XPath(TypeDropDownList), type);

			WaitForElement(By.XPath("//div[contains(@class, 'dnnlvContent')]/div[last()]")).WaitTillEnabled();
		}
	}
}
