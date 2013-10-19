using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class MainPage : BasePage
	{
		public MainPage(IWebDriver driver) : base(driver) { }

		public static string MainPageUrl = "/Default.aspx";

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
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Main' page:");
			GoToUrl(baseUrl + MainPageUrl);
		}
	}
}
