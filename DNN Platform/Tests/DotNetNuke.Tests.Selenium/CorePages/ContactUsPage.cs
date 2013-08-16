using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class ContactUsPage : BasePage
	{
		public ContactUsPage(IWebDriver driver) : base(driver) { }

		public static string ContactUsUrl = "/ContactUs";

		public static string ContactUsLink = "//div[@id ='nav']//a[contains(@href, 'ContactUs')]";

		public static string PageTitleLabel = "ContactUs";

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
