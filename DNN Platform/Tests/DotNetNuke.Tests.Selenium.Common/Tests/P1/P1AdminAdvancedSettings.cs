using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1AdminAdvancedSettings : CommonTestSteps
	{
		private string _providerToInstall;
		private string _authSystemToInstall;
		public string _optionalModuleToInstall;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("adminConfigurationSettings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_providerToInstall = testSettings.Attribute("providerToInstall").Value;
			_authSystemToInstall = testSettings.Attribute("authSystemToInstall").Value;
			_optionalModuleToInstall = testSettings.Attribute("optionalModuleToInstall").Value;

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
		public void Test001_InstallProvider()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install Provider'");

			var adminAdvancedSettingsPage = new AdminAdvancedSettingsPage(_driver);
			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);

			adminAdvancedSettingsPage.InstallProvider(_providerToInstall);

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.OpenTab(By.XPath(AdminAdvancedSettingsPage.ProvidersTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Installed Provider should not be available in the list");
			Assert.IsFalse(
				adminAdvancedSettingsPage.ElementPresent(
					By.XPath(AdminAdvancedSettingsPage.ProvidersTable + "//td/span[text() = '" + _providerToInstall + "']")),
				"Provider is present in the list");
		}

		[Test]
		public void Test002_UnInstallProvider()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'UnInstall Provider'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);

			hostExtensionsPage.OpenUsingUrl(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ProvidersAccordion));

			int itemNumber =
			hostExtensionsPage.FindElements(
				By.XPath(ExtensionsPage.ProvidersList)).Count;

			hostExtensionsPage.DeleteExtension(_providerToInstall);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ProvidersAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ProvidersList)).Count, Is.EqualTo(itemNumber - 1),
						"The Extension is not deleted correctly");
		}


		[Test]
		public void Test003_InstallAuthenticationSystem()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install Authentication System'");

			var adminAdvancedSettingsPage = new AdminAdvancedSettingsPage(_driver);
			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);

			adminAdvancedSettingsPage.InstallAuthenticationSystem(_authSystemToInstall);

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.OpenTab(By.XPath(AdminAdvancedSettingsPage.AuthenticationSystemsTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Installed Authentication System should not be available in the list");
			Assert.IsFalse(
				adminAdvancedSettingsPage.ElementPresent(
					By.XPath(AdminAdvancedSettingsPage.AuthenticationSystemsTable + "//td/span[text() = '" + _authSystemToInstall + "']")),
				"Authentication System is present in the list");
		}

		[Test]
		public void Test004_UnInstallAuthenticationSystem()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'UnInstall Authentication System'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);

			hostExtensionsPage.OpenUsingUrl(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.AuthenticationSystemsAccordion));

			int itemNumber =
			hostExtensionsPage.FindElements(
				By.XPath(ExtensionsPage.AuthenticationSystemsList)).Count;

			hostExtensionsPage.DeleteExtension(_authSystemToInstall);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.AuthenticationSystemsAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.AuthenticationSystemsList)).Count, Is.EqualTo(itemNumber - 1),
						"The Extension is not deleted correctly");
		}
	}
}
