using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.CorePages
{
	public class AdminPage : AdminBasePage
	{
		public AdminPage (IWebDriver driver) : base (driver) {}

		public static string GoogleAnalyticsLink = "//div[a/img[contains(@src, 'GoogleAnalytics_32X32_Standard.png')]]";
		
		public static int NumberOfLinks = 22;
	}
}
