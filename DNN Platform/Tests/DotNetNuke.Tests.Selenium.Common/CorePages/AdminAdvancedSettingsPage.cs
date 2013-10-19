using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminAdvancedSettingsPage : BasePage
	{
		public AdminAdvancedSettingsPage (IWebDriver driver) : base (driver) {}

		public static string AdminAdvancedSettingsUrl = "/Admin/AdvancedSettings";

		public override string PageTitleLabel
		{
			get { return "Advanced Configuration Settings"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Advanced Configuration Settings"; }
		}

		public override string PreLoadedModule
		{
			get { return "AdvancedSettingsModule"; }
		}

		public static string LanguagePacksTab = "//a[@href = '#asLanguagePacks']";
		public static string LanguagePackTable = "//table[contains(@id, '_AdvancedSettings_languagePacks')]";
		public static string NextButtonStep = "//a[contains(@id, '_nextButtonStep')]";
		public static string Return1Button = "//a[contains(@id, '_finishButtonStep')]";
		public static string AcceptCheckBox = "//input[contains(@id, 'wizInstall_chkAcceptLicense')]";

		public static string PackageInfoScreenTitle = "//h2/span[text() = 'Package Information']";
		public static string ReleaseNotesScreenTitle = "//h2/span[text() = 'Release Notes']";
		public static string ReviewLicenseScreenTitle = "//h2/span[text() = 'Review License']";
		public static string PackageReportScreenTitle = "//h2/span[text() = 'Package Installation Report']";

		public static string ProvidersTab = "//a[@href = '#asProviders']";
		public static string ProvidersTable = "//table[contains(@id, '_AdvancedSettings_providersGrid')]";

		public static string AuthenticationSystemsTab = "//a[@href = '#asAuthenticationSystems']";
		public static string AuthenticationSystemsTable = "//table[contains(@id, '_AdvancedSettings_authSystemsGrid')]";

		public static string OptionalModulesTab = "//a[@href = '#asOptionalModules']";
		public static string OptionalModulesTable = "//table[contains(@id, '_AdvancedSettings_modulesGrid')]";
		public static string OptionalModulesWarningMessage = "//div[contains(@id, '_AdvancedSettings_divNoModules')]";
		public static string OptionalModulesWarningMessageText = "There are no optional modules to install.";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminAdvancedSettingsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.AdvancedConfigurationSettingsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminAdvancedSettingsOption);
		}

		public string SetLanguageName(string language)
		{
			string option = null;

			switch (language)
			{
				case "de":
					{
						option = "Deutsch (Deutschland)";
						break;
					}
				case "es":
					{
						option = "Español (España, alfabetización internacional)";
						break;
					}
				case "fr":
					{
						option = "français (France)";
						break;
					}
				case "it":
					{
						option = "italiano (Italia)";
						break;
					}
				case "nl":
					{
						option = "Nederlands (Nederland)";
						break;
					}
			}

			return option;
		}

		public void DoInstallPackage()
		{
			WaitForElement(By.XPath(PackageInfoScreenTitle));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			WaitForElement(By.XPath(ReleaseNotesScreenTitle));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			WaitForElement(By.XPath(ReviewLicenseScreenTitle));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Accept CheckBox: ");
			WaitForElement(By.XPath(AcceptCheckBox)).ScrollIntoView().WaitTillEnabled(30);
			WaitForElement(By.XPath(AcceptCheckBox)).Info();
			CheckBoxCheck(By.XPath(AcceptCheckBox));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			WaitForElement(By.XPath(PackageReportScreenTitle));
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Return Button: ");
			WaitForElement(By.XPath(Return1Button), 60).Click();

			Thread.Sleep(1000);
		}

		public void DeployLanguagePack(string packName)
		{
			OpenTab(By.XPath(LanguagePacksTab));

			WaitForElement(
				By.XPath(LanguagePackTable + "//tr[td/span[contains(text(), '" + packName + "')]]/td/a")).Click();

			DoInstallPackage();
		}

		public void InstallProvider(string providerName)
		{			
			OpenTab(By.XPath(ProvidersTab));

			WaitForElement(
				By.XPath(ProvidersTable + "//tr[td/span[contains(text(), '" + providerName + "')]]/td/a")).Click();

			DoInstallPackage();
		}

		public void InstallAuthenticationSystem(string authSystemName)
		{
			OpenTab(By.XPath(AuthenticationSystemsTab));

			WaitForElement(
				By.XPath(AuthenticationSystemsTable + "//tr[td/span[contains(text(), '" + authSystemName + "')]]/td/a")).Click();

			DoInstallPackage();
		}

		public void InstallOptionalModule(string moduleName)
		{
			OpenTab(By.XPath(OptionalModulesTab));

			WaitForElement(
				By.XPath(OptionalModulesTable + "//tr[td/span[contains(text(), '" + moduleName + "')]]/td/a")).Click();

			DoInstallPackage();
		}
	}
}
 