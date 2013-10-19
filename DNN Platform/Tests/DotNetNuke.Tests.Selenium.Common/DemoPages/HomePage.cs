using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class HomePage : BasePage
	{
		public HomePage(IWebDriver driver) : base(driver) { }

		public static string HomePageUrl = "/Default.aspx";

		public static string HomeLink = "//div[@class ='navbar']//a[text() = 'Home']";

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Home' page:");
			GoToUrl(baseUrl + HomePageUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Home' page:");
			Click(By.XPath(HomeLink));
		}
	}
}
