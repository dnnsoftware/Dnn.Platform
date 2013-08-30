using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class AboutUsPage : BasePage
	{
		public AboutUsPage(IWebDriver driver) : base(driver) { }

		public static string AboutUsUrl = "/AboutUs";

		public static string AboutUsLink = "//div[@id ='nav']//a[contains(@href, 'AboutUs')]";

		public override string PageTitleLabel
		{
			get { return "About Us"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AboutUsUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			Click(By.XPath(AboutUsLink));
		}
	}
}
