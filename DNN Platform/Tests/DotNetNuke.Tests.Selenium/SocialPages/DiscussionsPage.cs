using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;


namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class DiscussionsPage : BasePage
	{
		public DiscussionsPage(IWebDriver driver) : base(driver) { }

		public static string DiscussionsUrl = "/Discussions";

		public static string PageTitleLabel = "Discussions";
		public static string PageHeader = "Discussions";
		public static string PageLink = "//div[@class = 'topHeader']//ul[@id = 'dnn_pnav']/li/a[contains(@href, 'Discussions')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + DiscussionsUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
