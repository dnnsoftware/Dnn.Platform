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

		public static string PageHeader = "Getting Started";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Main' page:");
			GoToUrl(baseUrl + HomePageUrl);
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectMenuOption(ControlPanelIDs.ControlPanelHelpOption, ControlPanelIDs.HelpGettingStartedOption);
		}
	}
}
