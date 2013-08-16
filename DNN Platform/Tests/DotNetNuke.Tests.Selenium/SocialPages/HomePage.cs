using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class HomePage : BasePage
	{
		public HomePage(IWebDriver driver) : base(driver) { }

		public static string HomeUrl = "";

		public static string PageTitleLabel = "Community";
		public static string PageHeader = "Home";
		public static string PageLink = "//a[contains(@id, 'LOGO_hypLogo')]/img";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HomeUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
