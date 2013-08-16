using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class IdeasPage : BasePage
	{
		public IdeasPage(IWebDriver driver) : base(driver) { }

		public static string IdeasUrl = "/Ideas";

		public static string PageTitleLabel = "Ideas";
		public static string PageHeader = "Ideas";
		public static string PageLink = "//div[@class = 'topHeader']//ul[@id = 'dnn_pnav']/li/a[contains(@href, 'Ideas')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + IdeasUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
