using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class WikiPage : BasePage
	{
		public WikiPage(IWebDriver driver) : base(driver) { }

		public static string WikiUrl = "/Wiki";

		public static string PageTitleLabel = "";
		public static string PageHeader = "Wiki";
		public static string PageLink = "//div[@class = 'topHeader']//ul[@id = 'dnn_pnav']/li/a[contains(@href, 'Wiki')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + WikiUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
