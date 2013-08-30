using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminLanguagesPage : BasePage
	{
		public AdminLanguagesPage(IWebDriver driver) : base(driver) { }

		public static string AdminLanguagesUrl = "/Admin/Languages";

		public override string PageTitleLabel
		{
			get { return "Language Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Languages"; }
		}

		public static string LanguagesTab = "//a[@href = '#Languages']";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminLanguagesUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.LanguagesLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminLanguagesOption);
		}

		public void EnableLanguage(string packName)
		{
			OpenTab(By.XPath(LanguagesTab));

			WaitForElement(By.XPath("//table[contains(@id, '_languageEnabler_languagesGrid')]//span[text() = 'German (Germany)']"));

			CheckBoxCheck(By.XPath("//tr[td//span[text() = 'German (Germany)']]/td/input"));

			Thread.Sleep(1000);
		}

		public void DisableLanguage(string packName)
		{
			OpenTab(By.XPath(LanguagesTab));

			WaitForElement(By.XPath("//table[contains(@id, '_languageEnabler_languagesGrid')]//span[text() = 'German (Germany)']"));

			CheckBoxUncheck(By.XPath("//tr[td//span[text() = 'German (Germany)']]/td/input"));

			Thread.Sleep(1000);
		}
	}
}
