using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class BadgesPage : BasePage
	{
		public BadgesPage(IWebDriver driver) : base(driver) { }

		public static string BadgesUrl = "/Home/Badges";

		public static string PageTitleLabel = "Badges";
		public static string PageHeader = "Badges";
		public static string PageLink = "//a[contains(@href, 'Badges')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + BadgesUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl);
			WaitAndClick(By.XPath("//div[@class = 'dnnRight']/strong/a[contains(@href, 'Leaderboard')]"));
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
