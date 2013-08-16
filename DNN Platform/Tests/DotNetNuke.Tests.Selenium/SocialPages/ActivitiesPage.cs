using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class ActivitiesPage : BasePage
	{
		public ActivitiesPage(IWebDriver driver) : base(driver) { }

		public static string ActivitiesUrl = "/Home/Activities";

		public static string PageTitleLabel = "Community Activities";
		public static string PageHeader = "Activities";
		public static string PageLink = "//a[contains(@href, 'Activities')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + ActivitiesUrl);
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
