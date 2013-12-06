using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class ContactUsPage : BasePage
	{
		public ContactUsPage(IWebDriver driver) : base(driver) { }

		public static string ContactUsUrl = "/Contact-Us";

		public static string ContactUsLink = "//div[@class ='navbar']//a[contains(@href, 'Contact-Us')]";

		public override string PageTitleLabel
		{
			get { return "Visit Us"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Contact Us"; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + ContactUsUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			Click(By.XPath(ContactUsLink));
		}
	}
}
