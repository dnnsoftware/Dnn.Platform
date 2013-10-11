using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostSchedulePage : BasePage
	{
		public HostSchedulePage(IWebDriver driver) : base(driver) { }

		public static string HostScheduleUrl = "/Host/Schedule";

		public override string PageTitleLabel
		{
			get { return "Schedule"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Schedule"; }
		}

		public override string PreLoadedModule
		{
			get { return "SchedulerModule"; }
		}

		public static string SearchUrlCrawlerName = "Search: Url Crawler";
		public static string SearchUrlCrawlerEnabled = "//tr[td[text() = '" + SearchUrlCrawlerName + "']]//span/input[contains(@id, 'ViewSchedule_dgSchedule')";

		public static string SearchFileCrawlerName = "Search: File Crawler";
		public static string SearchFileCrawlerEnabled = "//tr[td[text() = '" + SearchFileCrawlerName + "']]//span/input[contains(@id, 'ViewSchedule_dgSchedule')";

		public static string SearchSiteCrawlerName = "Search: Site Crawler";
		public static string SearchCrawlerEnabled = "//tr[td[text() = '" + SearchSiteCrawlerName + "']]//span/input[contains(@id, 'ViewSchedule_dgSchedule')";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostScheduleUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.ScheduleLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostAdvancedSettings, ControlPanelIDs.HostScheduleOption);
		}
	}
}
