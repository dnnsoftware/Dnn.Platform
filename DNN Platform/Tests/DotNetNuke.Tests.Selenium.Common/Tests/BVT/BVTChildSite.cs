using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.BVT
{
	[TestFixture]
	public abstract class BVTChildSite : CommonTestSteps
	{
		public string _siteAlias;
		public string _siteName;
		public string _siteDescription;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load (DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("childSite");

			string testName = testSettings.Attribute("name").Value;

			_siteAlias = testSettings.Attribute("siteAlias").Value;
			_siteName = testSettings.Attribute("siteName").Value;

			_siteDescription = testSettings.Attribute("siteDescription").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value.ToLower();

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
		public void Test001_AddNewSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Child Site'");

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));
			int originaltemNumber = hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count;

			hostSiteManagementPage.AddNewChildSite(_baseUrl, _siteAlias, _siteName);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(originaltemNumber + 1,
			            Is.EqualTo(hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count),
			            "The Child site is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Site name is present in the list");
			Assert.IsTrue(
				hostSiteManagementPage.ElementPresent(
					By.XPath("//tr/td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + _baseUrl + "/" + _siteAlias +
					         "')]")),
				"The Child site is not added correctly");
		}

		[Test]
		public void Test002_EditSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Child Site'");

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			
			hostSiteManagementPage.AddDescriptionToSite(_baseUrl, _siteAlias, _siteDescription);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.EditSite(_baseUrl, _siteAlias);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Site description is present");
			Assert.That(hostSiteManagementPage.WaitForElement(By.XPath(HostSiteManagementPage.SiteNameDescriptionTextBox)).Text,
			            Is.EqualTo(_siteDescription),
			            "The site description is not added correctly");
		}

		[Test]
		public void Test003_NavigateToChildSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigate to Child Site'");

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.NavigateToChildSite(_baseUrl, _siteAlias);

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT current window Title");
			Assert.That(mainPage.CurrentWindowTitle(), Is.StringContaining(_siteName + " > Home"),
			            "The website name is not correct");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT current window Url");
			Assert.That(mainPage.CurrentWindowUrl(), Is.StringStarting("http://" + _baseUrl + "/" + _siteAlias),
			            "The website URL is not correct");
		}

		[Test]
		public void Test004_DeleteSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Child Site'");

			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));
			int itemNumber = hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count;

			hostSiteManagementPage.DeleteSite(_baseUrl, _siteAlias);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			hostSiteManagementPage.WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count,
			            Is.EqualTo(itemNumber - 1),
			            "The Child site is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Site name is not present in the list");
			Assert.IsFalse(
				hostSiteManagementPage.ElementPresent(
					By.XPath("//tr/td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + _baseUrl + "/" + _siteAlias +
					         "')]")),
				"The Child site is not added correctly");
		}
	}
}