using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostDashboardPage : BasePage
	{
		public HostDashboardPage(IWebDriver driver) : base(driver) { }

		public static string HostDashboardUrl = "/Host/Dashboard";

		public override string PageTitleLabel
		{
			get { return "Dashboard"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Dashboard"; }
		}

		public override string PreLoadedModule
		{
			get { return "DashboardModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostDashboardUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.DashboardLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostCommonSettings, ControlPanelIDs.HostDashboardOption);
		}
	}
}
