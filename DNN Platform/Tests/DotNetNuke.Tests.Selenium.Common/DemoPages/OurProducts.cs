using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class OurProductsPage : BasePage
	{
		public OurProductsPage(IWebDriver driver) : base(driver) { }

		public static string OurProductsUrl = "/Our-Products";

		public static string OurProductsLink = "//div[@class ='navbar']//a[contains(@href, 'Our-Products')]";

		public override string PageTitleLabel
		{
			get { return "Our Products"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Our Products"; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + OurProductsUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			Click(By.XPath(OurProductsLink));
		}
	}
}
