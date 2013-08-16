using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SocialPages
{
	class AdminGamingMechanicsPage : BasePage
	{
		public AdminGamingMechanicsPage(IWebDriver driver) : base(driver) { }

		public static string GamingMechanicsUrl = "/Admin/Gaming-Mechanics";

		public static string PageHeaderLabel = "Gaming Mechanics";
		public static string PageHeader = "Gaming Mechanics";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageHeaderLabel + "' page:");
			GoToUrl(baseUrl + GamingMechanicsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageHeaderLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.GamingMechanicsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageHeaderLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminAdvancedSettings, BasePage.GamingMechanicsOption);
		} 
	}
}
