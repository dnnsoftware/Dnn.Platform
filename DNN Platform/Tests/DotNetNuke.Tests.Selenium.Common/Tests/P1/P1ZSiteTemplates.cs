using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.DemoPages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	public abstract class P1ZSiteTemplates : CommonTestSteps
	{
		private string _siteName;
		private string _childSiteName;
		private string _languagePackToDeploy;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("hostSiteManagement");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;
			_siteName = settings.Attribute("WebsiteName").Value;

			_languagePackToDeploy = testSettings.Attribute("languagePackToDeploy").Value;
			_childSiteName = testSettings.Attribute("childSiteName").Value;

			string testName = testSettings.Attribute("name").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();
		}

		[SetUp]
		public void RunBeforeEachTest()
		{
			Trace.WriteLine("Run before each test");
			_logContent = LogContent();
		}

		[TearDown]
		public void CleanupAfterEachTest()
		{
			Trace.WriteLine("Run after each test");
			VerifyLogs(_logContent);
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
		}

		[Test]
		public void Test001_ExportTemplateWithNoContent()
		{
			string childSiteName = _childSiteName + "001";

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);
			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitAndClick(By.XPath(HostSiteManagementPage.ExportSiteTemplateButton));
			hostSiteManagementPage.AccordionOpen(By.XPath(HostSiteManagementPage.AdvancedConfigurationAccordion));

			int orignalNumber =
				hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PagesList)).Count;

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.ExportSiteTemplate(_siteName, "NoContentTemplate", "NoContentTemplate");

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.AddNewChildSite(_baseUrl, childSiteName, "title", "NoContentTemplate");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl + "/" + childSiteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "The page Module should be present");
			Assert.IsTrue(adminPageManagementPage.ElementPresent(By.XPath(AdminPageManagementPage.Module)));

			Trace.WriteLine(BasePage.TraceLevelPage + "The number of pages is correct");
			Assert.That(adminPageManagementPage.FindElements(By.XPath(HostSiteManagementPage.PagesList)).Count, Is.EqualTo(orignalNumber), "");

			var aboutUsPage = new AboutUsPage(_driver);
			aboutUsPage.OpenUsingLink(_baseUrl + "/" + childSiteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "The page doesn't have a Content");
			Assert.IsFalse(adminPageManagementPage.ElementPresent(By.XPath(AboutUsPage.PageContent)));
		}

		[Test]
		public void Test002_ExportTemplateWithContent()
		{
			string childSiteName = _childSiteName + "002";

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);
			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitAndClick(By.XPath(HostSiteManagementPage.ExportSiteTemplateButton));
			hostSiteManagementPage.AccordionOpen(By.XPath(HostSiteManagementPage.AdvancedConfigurationAccordion));

			int orignalNumber =
				hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PagesList)).Count;

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.ExportSiteTemplateWithContent(_siteName, "ContentTemplate", "ContentTemplate");

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.AddNewChildSite(_baseUrl, childSiteName, "title", "ContentTemplate");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl + "/" + childSiteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "The page Module should be present");
			Assert.IsTrue(adminPageManagementPage.ElementPresent(By.XPath(AdminPageManagementPage.Module)));

			Trace.WriteLine(BasePage.TraceLevelPage + "The number of pages is correct");
			Assert.That(adminPageManagementPage.FindElements(By.XPath(HostSiteManagementPage.PagesList)).Count, Is.EqualTo(orignalNumber), "");

			var aboutUsPage = new AboutUsPage(_driver);
			aboutUsPage.OpenUsingLink(_baseUrl + "/" + childSiteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "The page contain a Content");
			Assert.IsTrue(adminPageManagementPage.ElementPresent(By.XPath(AboutUsPage.PageContent)));
		}

		[Test]
		public void Test003_ExportTemplateSomePages()
		{
			string childSiteName = _childSiteName + "003";
			const int orignalNumber = 25;

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);
			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitAndClick(By.XPath(HostSiteManagementPage.ExportSiteTemplateButton));
			hostSiteManagementPage.AccordionOpen(By.XPath(HostSiteManagementPage.AdvancedConfigurationAccordion));

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.ExportSiteTemplateSomePages(_siteName, "AdminPagesTemplate", "Admin pages, Demo pages No Content Template");

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.AddNewChildSite(_baseUrl, childSiteName, "title", "AdminPagesTemplate");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl + "/" + childSiteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "The page Module should be present");
			Assert.IsTrue(adminPageManagementPage.ElementPresent(By.XPath(AdminPageManagementPage.Module)));

			Trace.WriteLine(BasePage.TraceLevelPage + "The number of pages is correct");
			Assert.That(adminPageManagementPage.FindElements(By.XPath(HostSiteManagementPage.PagesList)).Count, Is.EqualTo(orignalNumber), "");

			var contactUsPage = new ContactUsPage(_driver);
			contactUsPage.OpenUsingLink(_baseUrl + "/" + childSiteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "The page doesn't have a Content");
			Assert.IsFalse(adminPageManagementPage.ElementPresent(By.XPath(AboutUsPage.PageContent)));
		}

		[Test]
		public void Test004_ExportTemplateWithLanguages_Preconditions()
		{
			var adminAdvancedSettingsPage = new AdminAdvancedSettingsPage(_driver);
			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.DeployLanguagePack(adminAdvancedSettingsPage.SetLanguageName("de"));

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.DeployLanguagePack(adminAdvancedSettingsPage.SetLanguageName("es"));

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.DeployLanguagePack(adminAdvancedSettingsPage.SetLanguageName("fr"));

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.DeployLanguagePack(adminAdvancedSettingsPage.SetLanguageName("it"));

			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);
			adminAdvancedSettingsPage.DeployLanguagePack(adminAdvancedSettingsPage.SetLanguageName("nl"));

			var adminLanguagesPage = new AdminLanguagesPage(_driver);
			adminLanguagesPage.OpenUsingButtons(_baseUrl);
			adminLanguagesPage.EnableLanguage(adminLanguagesPage.SetLanguageName("de"));

			adminLanguagesPage.OpenUsingButtons(_baseUrl);
			adminLanguagesPage.EnableLanguage(adminLanguagesPage.SetLanguageName("es"));

			adminLanguagesPage.OpenUsingButtons(_baseUrl);
			adminLanguagesPage.EnableLanguage(adminLanguagesPage.SetLanguageName("fr"));

			adminLanguagesPage.OpenUsingButtons(_baseUrl);
			adminLanguagesPage.EnableLanguage(adminLanguagesPage.SetLanguageName("it"));

			adminLanguagesPage.OpenUsingButtons(_baseUrl);
			adminLanguagesPage.EnableLanguage(adminLanguagesPage.SetLanguageName("nl"));

			var hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.OpenUsingButtons(_baseUrl);
			hostSettingsPage.EnableContentLocalization();

			adminLanguagesPage.OpenUsingButtons(_baseUrl);
			adminLanguagesPage.EnableLocalization(CheckBox.ActionType.Check);
		}


		[Test]
		public void Test004_ExportTemplateWithOneLanguage()
		{
			string childSiteName = _childSiteName + "004";

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);
			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.ExportSiteTemplateWithContentAndLanguage(_siteName, "LanguageTemplate", "LanguageTemplate", _languagePackToDeploy);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.AddNewChildSite(_baseUrl, childSiteName, "title", "LanguageTemplate");

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl + "/" + childSiteName);
			adminSiteSettingsPage.DisablePopups();
			
			var aboutUsPage = new AboutUsPage(_driver);
			aboutUsPage.OpenUsingLink(_baseUrl + "/" + childSiteName);

			aboutUsPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);
			aboutUsPage.OpenTab(By.XPath(ControlPanelIDs.LocalizationTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Two language flags should be available");
			Assert.That(aboutUsPage.FindElements(By.XPath(ControlPanelIDs.LanguageIcon)).Count, Is.EqualTo(2),
						"The language flag number is not correct");

			Trace.WriteLine(BasePage.TraceLevelPage + "Two language pages should be available");
			Assert.That(aboutUsPage.FindElements(By.XPath("//tr[@class = 'pageHeaderRow']/th")).Count, Is.EqualTo(2));
		}


		[Test]
		public void Test005_ExportTemplateWithAllLanguagesAndContent()
		{
			string childSiteName = _childSiteName + "005";

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);
			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.ExportSiteTemplateWithContent(_siteName, "AllLanguageTemplate", "AllLanguageTemplate");

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.AddNewChildSite(_baseUrl, childSiteName, "title", "AllLanguageTemplate");

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl + "/" + childSiteName);
			adminSiteSettingsPage.DisablePopups();

			var aboutUsPage = new AboutUsPage(_driver);
			aboutUsPage.OpenUsingLink(_baseUrl + "/" + childSiteName);

			aboutUsPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);
			aboutUsPage.OpenTab(By.XPath(ControlPanelIDs.LocalizationTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Six language flags should be available");
			Assert.That(aboutUsPage.FindElements(By.XPath(ControlPanelIDs.LanguageIcon)).Count, Is.EqualTo(6),
						"The language flag number is not correct");

			Trace.WriteLine(BasePage.TraceLevelPage + "Six language pages should be available");
			Assert.That(aboutUsPage.FindElements(By.XPath("//tr[@class = 'pageHeaderRow']/th")).Count, Is.EqualTo(6));
		}
	}
}
