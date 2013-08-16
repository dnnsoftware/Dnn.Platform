using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class EventsPage : BasePage
	{
		public EventsPage(IWebDriver driver) : base(driver) { }

		public static string EventsUrl = "/Events";

		public static string PageTitleLabel = "Social Events";
		public static string PageHeader = "Events";
		public static string PageLink = "//div[@class = 'topHeader']//ul[@id = 'dnn_pnav']/li/a[contains(@href, 'Events')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + EventsUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
