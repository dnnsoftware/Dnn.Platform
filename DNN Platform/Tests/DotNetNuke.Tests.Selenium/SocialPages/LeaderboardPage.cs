using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class LeaderboardPage : BasePage
	{
		public LeaderboardPage(IWebDriver driver) : base(driver) { }

		public static string LeaderboardUrl = "/Home/Leaderboard";

		public static string PageTitleLabel = "Community Leaderboard";
		public static string PageHeader = "Leaderboard";
		public static string PageLink = "//div[@class = 'dnnRight']/strong/a[contains(@href, 'Leaderboard')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + LeaderboardUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl);
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
