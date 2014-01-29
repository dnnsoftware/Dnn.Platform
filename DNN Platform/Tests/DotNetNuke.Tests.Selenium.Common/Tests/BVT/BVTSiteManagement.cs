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
	public abstract class BVTSiteManagement : CommonTestSteps
	{
		public string _childSiteAlias;
		public string _childSiteTitle;
		public string _childSiteDescription;
		public string _parentSiteAlias;
		public string _parentSiteTitle;
		public string _parentSiteDescription;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("siteManagement");

			string testName = testSettings.Attribute("name").Value;

			_childSiteAlias = testSettings.Attribute("childSiteAlias").Value;
			_childSiteTitle = testSettings.Attribute("childSiteTitle").Value;
			_childSiteDescription = testSettings.Attribute("childSiteDescription").Value;

			_parentSiteAlias = testSettings.Attribute("parentSiteAlias").Value;
			_parentSiteTitle = testSettings.Attribute("parentSiteTitle").Value;
			_parentSiteDescription = testSettings.Attribute("parentSiteDescription").Value;

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

		public void AddNewSite(string siteType, string siteAlias, string siteTitle)
		{
			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			int originaltemNumber = hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count;

			if (siteType == "ChildSite")
			{
				hostSiteManagementPage.AddNewChildSite(_baseUrl, siteAlias, siteTitle, "Default Website");
			}
			else
			{
				hostSiteManagementPage.AddNewParentSite(siteAlias + _baseUrl, siteTitle, "Default Website");
			}

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine("ASSERT the number of elements in the list increased by 1");
			Assert.That(originaltemNumber + 1,
						Is.EqualTo(hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count),
						"The Site is not added correctly");

			Trace.WriteLine("ASSERT the Site name is present in the list");
			Assert.IsTrue(hostSiteManagementPage.ElementPresent(
					By.XPath("//tr/td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + siteAlias + "')]")),
					"The Site is not added correctly");
		}

		public void EditSite(string siteName, string siteDescription)
		{
			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.AddDescriptionToSite(siteName, siteDescription);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.EditSite(siteName);

			Trace.WriteLine("ASSERT the Site description is present");
			Assert.That(hostSiteManagementPage.WaitForElement(By.XPath(HostSiteManagementPage.SiteNameDescriptionTextBox)).Text,
			            Is.EqualTo(siteDescription),
			            "The site description is not added correctly");
		}

		public void NavigateToSite(string siteName, string siteTitle)
		{
			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.NavigateToSite(siteName);

			var mainPage = new MainPage(_driver);

			Trace.WriteLine("ASSERT current window Title");
			Assert.That(mainPage.CurrentWindowTitle(), Is.StringContaining(siteTitle + " > Home"),
			            "The website name is not correct");

			Trace.WriteLine("ASSERT current window Url");
			Assert.That(mainPage.CurrentWindowUrl(), Is.StringStarting("http://" + siteName),
			            "The website URL is not correct");
		}

		public void DeleteSite(string siteName)
		{
			var hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);
			int itemNumber = hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count;

			hostSiteManagementPage.DeleteSite(siteName);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine("ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count,
			            Is.EqualTo(itemNumber - 1),
			            "The Site is not deleted correctly");

			Trace.WriteLine("ASSERT the Site name is not present in the list");
			Assert.IsFalse(
				hostSiteManagementPage.ElementPresent(
					By.XPath("//tr/td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + siteName + "')]")),
				"The Site is not added correctly");
		}


		[Test]
		public void Test001_AddNewChildSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Child Site'");

			AddNewSite("ChildSite", _childSiteAlias, _childSiteTitle);
		}

		[Test]
		public void Test002_AddNewParentSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Parent Site'");

			AddNewSite("ParentSite", _parentSiteAlias, _parentSiteTitle);
		}

		[Test]
		public void Test003_EditChildSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Child Site'");

			EditSite(_baseUrl + "/" + _childSiteAlias, _childSiteDescription);
		}

		[Test]
		public void Test004_EditParentSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Parent Site'");

			EditSite(_parentSiteAlias + _baseUrl, _parentSiteDescription);
		}


		[Test]
		public void Test005_NavigateToChildSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigate to Child Site'");

			NavigateToSite(_baseUrl + "/" + _childSiteAlias, _childSiteTitle);
		}

		[Test]
		public void Test006_NavigateToParentSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigate to Parent Site'");

			NavigateToSite(_parentSiteAlias + _baseUrl, _parentSiteTitle);
		}

		[Test]
		public void Test007_DeleteChildSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Child Site'");

			DeleteSite(_baseUrl + "/" + _childSiteAlias);
		}

		[Test]
		public void Test008_DeleteParentSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Parent Site'");

			DeleteSite(_parentSiteAlias + _baseUrl);
		}
	}
}