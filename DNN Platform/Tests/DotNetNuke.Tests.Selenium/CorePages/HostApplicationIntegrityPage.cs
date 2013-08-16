using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostApplicationIntegrityPage : BasePage
	{
		public HostApplicationIntegrityPage(IWebDriver driver) : base(driver) { }

		public static string HostApplicationIntegrityUrl = "/Host/ProfessionalFeatures/ApplicationIntegrity";

		public static string PageTitleLabel = "Application Integrity";
		public static string PageHeader = "Application Integrity";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostApplicationIntegrityUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.ApplicationIntegrityLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostAdvancedSettings, BasePage.HostApplicationIntegrityOption);
		}
	}
}
