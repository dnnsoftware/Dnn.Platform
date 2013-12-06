using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.Properties;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminSiteSettingsPage : BasePage
	{
		public AdminSiteSettingsPage(IWebDriver driver) : base(driver) { }

		public static string AdminSiteSettingsUrl = "/Admin/SiteSettings";

		public override string PageTitleLabel
		{
			get { return "Site Settings"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Site Settings"; }
		}

		public override string PreLoadedModule
		{
			get { return "SiteManagementModule"; }
		}

		public static string AdvancedSettingsTab = "//a[@href = '#ssAdvancedSettings']";
		public static string UsabilitySettingsAccordion = "//h2[@id='dnnSitePanel-Usability']/a";
		public static string EnablePopups = "//input[contains(@id, 'SiteSettings_enablePopUpsCheckBox')]";

		public static string UserAccountSettingsTab = "//a[@href = '#ssUserAccountSettings']";
		public static string RegistrationSettingsAccordion = "//h2[@id='dnnSitePanel-Registration']/a";
		public static string NoneRadioButton = "//input[contains(@id, '_SiteSettings_optUserRegistration_0')]";
		public static string PrivateRadioButton = "//input[contains(@id, '_SiteSettings_optUserRegistration_1')]";
		public static string PublicRadioButton = "//input[contains(@id, '_SiteSettings_optUserRegistration_2')]";
		public static string VerifiedRadioButton = "//input[contains(@id, '_SiteSettings_optUserRegistration_3')]";

		public static string UpdateButton = "//a[contains(@id, 'SiteSettings_cmdUpdate')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminSiteSettingsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.SiteSettingsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminCommonSettings, ControlPanelIDs.AdminSiteSettingsOption);
		}

		public void DisablePopups()
		{
			OpenTab(By.XPath(AdvancedSettingsTab));
			AccordionOpen(By.XPath(UsabilitySettingsAccordion));

			CheckBoxUncheck( By.XPath(EnablePopups));

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(Settings.Default.WaitFactor * 3000);

			FindElement(By.XPath(UpdateButton)).WaitTillEnabled(60);
		}

		public void SetUserRegistrationType(string registrationType)
		{
			OpenTab(By.XPath(UserAccountSettingsTab));
			AccordionOpen(By.XPath(RegistrationSettingsAccordion));

			RadioButtonSelect(By.XPath(registrationType));

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(Settings.Default.WaitFactor * 3000);
		}
	}
}
