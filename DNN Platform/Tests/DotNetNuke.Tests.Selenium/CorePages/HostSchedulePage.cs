using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostSchedulePage : BasePage
	{
		public HostSchedulePage(IWebDriver driver) : base(driver) { }

		public static string HostScheduleUrl = "/Host/Schedule";

		public static string PageTitleLabel = "Schedule";
		public static string PageHeader = "Schedule";

		public static string SearchUrlCrawlerName = "Search: Url Crawler";
		public static string SearchFileCrawlerName = "Search: File Crawler";
		public static string SearchSiteCrawlerName = "Search: Site Crawler";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostScheduleUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.ScheduleLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostAdvancedSettings, BasePage.HostScheduleOption);
		}
	}
}
