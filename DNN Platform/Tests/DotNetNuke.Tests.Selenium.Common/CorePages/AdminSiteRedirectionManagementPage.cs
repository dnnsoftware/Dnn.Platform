using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class AdminSiteRedirectionManagementPage : BasePage
	{
		public AdminSiteRedirectionManagementPage(IWebDriver driver) : base(driver) { }

		public static string AdminSiteRedirectionManagementUrl = "/Admin/SiteRedirectionManagement";

		public override string PageTitleLabel
		{
			get { return "Site Redirection Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Site Redirection Management"; }
		}

		public override string PreLoadedModule
		{
			get { return "SiteRedirectionModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminSiteRedirectionManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.SiteRedirectionManagementLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminSiteRedirectionManagementOption);
		}
	}
}
