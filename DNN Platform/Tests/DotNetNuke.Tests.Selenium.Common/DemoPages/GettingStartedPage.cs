using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.DemoPages
{
	public class GettingStartedPage : BasePage
	{
		public GettingStartedPage(IWebDriver driver) : base(driver) { }

		public static string HomePageUrl = "/Getting-Started";

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public override string PageHeaderLabel
		{
			get { return "Getting Started"; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Getting started' page:");
			GoToUrl(baseUrl + HomePageUrl);
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Getting started' page:");

			Click(By.XPath(ControlPanelIDs.ControlPanelHelpOption));

			FindElement(By.XPath(ControlPanelIDs.HelpGettingStartedOption)).WaitTillVisible().Click();

		}
	}
}
