using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostActivateYourLicensePage : BasePage
	{
		public HostActivateYourLicensePage(IWebDriver driver) : base(driver) { }

		public static string HostActivateYourLicenseUrl = "/Host/ProfessionalFeatures/ActivateYourLicense";

		public static string PageTitleLabel = "Activate your License";
		public static string PageHeader = "Activate your License";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostActivateYourLicenseUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.ActivateYourLicenseLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostAdvancedSettings, BasePage.HostActivateYourLicenseOption);
		}
	}
}
