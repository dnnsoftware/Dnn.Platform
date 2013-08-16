using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class AnswersPage : BasePage
	{
		public AnswersPage(IWebDriver driver) : base(driver) { }

		public static string AnswersUrl = "/Answers";

		public static string PageTitleLabel = "Answers";
		public static string PageHeader = "Answers";
		public static string PageLink = "//div[@class = 'topHeader']//ul[@id = 'dnn_pnav']/li/a[contains(@href, 'Answers')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AnswersUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(PageLink));
		}
	}
}
