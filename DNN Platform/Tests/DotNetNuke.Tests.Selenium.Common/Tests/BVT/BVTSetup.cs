using System;
using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.BVT
{
	[SetUpFixture]
	public abstract class BVTSetup
	{
		protected abstract string DataFileLocation { get; }

		[SetUp]
		public void RunBeforeBVTTests()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "BVT Setup");

			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			string testName = settings.Attribute("name").Value;
			string installerLanguage = settings.Attribute("InstallerLanguage").Value;

			IWebDriver driver = TestBase.StartDriver(settings.Attribute("browser").Value);
			string baseUrl = settings.Attribute("baseURL").Value;
			bool dummyProvider = settings.Attribute("runWithDummyCachingProvider")
										 .Value.Equals("yes", StringComparison.InvariantCultureIgnoreCase);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			var installerPage = new InstallerPage(driver);

			installerPage.OpenUsingUrl(baseUrl);

			installerPage.SetInstallerLanguage(installerLanguage);
			installerPage.SetDictionary(installerLanguage);

			installerPage.FillAccountInfo(settings);

			installerPage.ClickOnContinueButton();

			installerPage.WaitForInstallationProcessToFinish();

			installerPage.ClickOnVisitWebsiteButton();

			installerPage.WelcomeScreen();

			var adminSiteSettingsPage = new AdminSiteSettingsPage(driver);
			adminSiteSettingsPage.OpenUsingButtons(baseUrl);
			adminSiteSettingsPage.DisablePopups();

			if (dummyProvider)
			{
				var hostExtensionsPage = new HostExtensionsPage(driver);
				hostExtensionsPage.OpenUsingButtons(baseUrl);
				hostExtensionsPage.InstallExtension(@"Drivers\" + "DummySerializationCachingProvider_01.00.01_Install.zip");
			}
			
			driver.Quit();
		}

		[TearDown]
		public void RunAfterBVTTests()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "BVT Teardown");
		}
	}
}