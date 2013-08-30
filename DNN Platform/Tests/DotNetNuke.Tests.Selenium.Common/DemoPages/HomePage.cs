using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class HomePage : BasePage
	{
		public HomePage(IWebDriver driver) : base(driver) { }

		public static string HomePageUrl = "/Default.aspx";

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public static string PageHeader = "";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Main' page:");
			GoToUrl(baseUrl + HomePageUrl);
		}
	}
}
