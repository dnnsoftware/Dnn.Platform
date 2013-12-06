using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class AboutUsPage : BasePage
	{
		public AboutUsPage(IWebDriver driver) : base(driver) { }

		public static string AboutUsUrl = "/About-Us";

		public static string AboutUsLink = "//div[@class ='navbar']//a[contains(@href, 'About-Us')]";

		public override string PageTitleLabel
		{
			get { return "About Us"; }
		}

		public override string PageHeaderLabel
		{
			get { return "About Us"; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
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
