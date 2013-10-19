using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class AdminSearchAdminPage : BasePage
	{
		public AdminSearchAdminPage(IWebDriver driver) : base(driver) { }

		public static string AdminSearchAdminUrl = "/Admin/SearchAdmin";

		public override string PageTitleLabel
		{
			get { return "Search Admin"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Search Admin"; }
		}

		public override string PreLoadedModule
		{
			get { return "SearchAdminModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminSearchAdminUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.SearchAdminLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminSearchAdminOption);
		}
	}
}
