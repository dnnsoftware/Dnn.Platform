using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class MainPage : BasePage
	{
		public MainPage(IWebDriver driver) : base(driver) { }

		public static string MainPageUrl = "/Default.aspx";

		public static string PageTitleLabel = "";
		public static string PageHeader = "";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Main' page:");
			GoToUrl(baseUrl + MainPageUrl);
		}
	}
}
