using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostAdvancedUrlManagementPage : BasePage
	{
		public HostAdvancedUrlManagementPage (IWebDriver driver) : base (driver) {}

		public static string HostAdvancedUrlManagementUrl = "/Host/ProfessionalFeatures/AdvancedUrlManagement";

		public static string PageTitleLabel = "Advanced URL Management";
		public static string PageHeader = "Advanced URL Management";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostAdvancedUrlManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.AdvancedUrlManagementLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostAdvancedSettings, BasePage.HostAdvancedUrlManagementOption);
		}
	}
}
