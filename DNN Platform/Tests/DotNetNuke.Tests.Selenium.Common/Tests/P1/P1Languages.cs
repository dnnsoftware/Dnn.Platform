using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1Languages : CommonTestSteps
	{
		private string _languagePackToDeploy;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("languages");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_languagePackToDeploy = testSettings.Attribute("languagePackToDeploy").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		[Test]
		public void Test001_InstallLanguagePack()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install Language pack'");

			var adminAdvancedSettingsPage = new AdminAdvancedSettingsPage(_driver);
			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);

			adminAdvancedSettingsPage.DeployLanguagePack(adminAdvancedSettingsPage.SetLanguageName(_languagePackToDeploy));

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.OpenTab(By.XPath(AdminAdvancedSettingsPage.LanguagePacksTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Deployed Language Pack should not be available in the list");
			Assert.IsFalse(
				adminAdvancedSettingsPage.ElementPresent(
					By.XPath(AdminAdvancedSettingsPage.LanguagePackTable + "//td/span[text() = '" + adminAdvancedSettingsPage.SetLanguageName(_languagePackToDeploy) +
					         "']")),
				"The Language pack is present in the list");
		}

		[Test]
		public void Test002_EnableLanguage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Enable Language'");

			var adminLanguagesPage = new AdminLanguagesPage(_driver);
			adminLanguagesPage.OpenUsingButtons(_baseUrl);

			adminLanguagesPage.EnableLanguage(adminLanguagesPage.SetLanguageName(_languagePackToDeploy));

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "Two language flags should be available");
			Assert.That(adminLanguagesPage.FindElements(By.XPath(ControlPanelIDs.LanguageIcon)).Count, Is.EqualTo(2),
			            "The language flag number is not correct");
		}

		[Test]
		public void Test003_EnableContentLocalization()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Enable Content Localization'");

			var hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.OpenUsingButtons(_baseUrl);

			hostSettingsPage.EnableContentLocalization();

			var adminLanguagesPage = new AdminLanguagesPage(_driver);
			adminLanguagesPage.OpenUsingButtons(_baseUrl);

			adminLanguagesPage.EnableLocalization();

			adminLanguagesPage.WaitForElement(By.XPath(AdminLanguagesPage.LanguagesTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "The localization table should be present");
			Assert.IsTrue(adminLanguagesPage.ElementPresent(By.XPath(AdminLanguagesPage.LocalizationTable)),
						"The Localization table is not present");
		}

		[Test]
		public void Test004_DisableContentLocalization()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Disable Content Localization'");

			var adminLanguagesPage = new AdminLanguagesPage(_driver);
			adminLanguagesPage.OpenUsingButtons(_baseUrl);

			adminLanguagesPage.DisableLocalization();

			Trace.WriteLine(BasePage.TraceLevelPage + "The localization table should NOT be present");
			Assert.IsFalse(adminLanguagesPage.ElementPresent(By.XPath(AdminLanguagesPage.LocalizationTable)),
						"The Localization table is still present");
		}

		[Test]
		public void Test005_DisableLanguage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Enable Language'");

			var adminLanguagesPage = new AdminLanguagesPage(_driver);
			adminLanguagesPage.OpenUsingButtons(_baseUrl);

			adminLanguagesPage.DisableLanguage(adminLanguagesPage.SetLanguageName(_languagePackToDeploy));

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "Two language flags should NOT be available");
			Assert.That(adminLanguagesPage.FindElements(By.XPath(ControlPanelIDs.LanguageIcon)).Count, Is.EqualTo(0),
			            "The language flag number is not correct");
		}

		[Test]
		public void Test006_UnInstallLanguagePack()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'UnInstall Language pack'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);

			//Extension pack
			hostExtensionsPage.OpenUsingUrl(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ExtensionLanguagePacksAccordion));

			int itemNumber =
			hostExtensionsPage.FindElements(
				By.XPath(
					ExtensionsPage.ExtensionLanguagePacksPanel + "/following-sibling :: *//tr[td/span[contains(text(), '" +
					hostExtensionsPage.SetLanguageName(_languagePackToDeploy) + "')]]")).Count;

			while (itemNumber  > 0)
			{
				Trace.WriteLine(BasePage.TraceLevelComposite + "Delete Extension: ");

				hostExtensionsPage.DeleteLanguagePack(ExtensionsPage.ExtensionLanguagePacksPanel, _languagePackToDeploy);

				itemNumber = itemNumber - 1;
			}

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.CoreLanguagePacksAccordion));

			hostExtensionsPage.DeleteLanguagePack(ExtensionsPage.CoreLanguagePacksPanel, _languagePackToDeploy);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.CoreLanguagePacksAccordion)); 

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list is zero");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.CoreLanguagePacksList)).Count, Is.EqualTo(0),
			            "The Extension is not deleted correctly");
		}
	}
}