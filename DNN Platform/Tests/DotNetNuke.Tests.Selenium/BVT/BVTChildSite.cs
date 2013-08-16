using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.BVT
{
	[TestFixture("childsitebvt", "ChildSiteTitleBVT", "Child Site, BVT")]
    [Category("BVT")]
	public class BVTChildSite : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _siteAlias;
		private string _siteName;
		private string _siteDescription;

		public BVTChildSite(string siteAlias, string siteName, string siteDescription)
		{
			this._siteAlias = siteAlias;
			this._siteName = siteName;
			this._siteDescription = siteDescription;
		}

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			
			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value.ToLower();

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Child Site BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[Test]
		public void Test001_AddNewSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Child Site'");

			HostSiteManagementPage hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			int originaltemNumber = hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count;

			hostSiteManagementPage.AddNewChildSite(_baseUrl, _siteAlias, _siteName);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(originaltemNumber + 1, Is.EqualTo(hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count),
					"The Child site is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Site name is present in the list");
			Assert.IsTrue(hostSiteManagementPage.ElementPresent(By.XPath("//tr/td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + _baseUrl + "/" + _siteAlias + "')]")),
					"The Child site is not added correctly");
		}

		[Test]
		public void Test002_EditSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Child Site'");

			HostSiteManagementPage hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.AddDescriptionToSite(_baseUrl, _siteAlias, _siteDescription);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.EditSite(_baseUrl, _siteAlias);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Site description is present");
			Assert.That(hostSiteManagementPage.FindElement(By.XPath(HostSiteManagementPage.SiteNameDescriptionTextBox)).Text, Is.EqualTo(_siteDescription), 
				"The site description is not added correctly");
		}

		[Test]
		public void Test003_NavigateToChildSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigate to Child Site'");

			HostSiteManagementPage hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			hostSiteManagementPage.NavigateToChildSite(_baseUrl, _siteAlias);

			InstallerPage installerPage = new InstallerPage(_driver);

			installerPage.WelcomeScreen();

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT current window Title");
			Assert.That(installerPage.CurrentWindowTitle(), Is.StringContaining(_siteName + " > Home"),
			              "The website name is not correct");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT current window Url");
			Assert.That(installerPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl + "/" + _siteAlias),
						  "The website URL is not correct");
		}

		[Test]
		public void Test004_DeleteSite()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Child Site'");

			HostSiteManagementPage hostSiteManagementPage = new HostSiteManagementPage(_driver);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			int itemNumber = hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count;

			hostSiteManagementPage.DeleteSite(_baseUrl, _siteAlias);

			hostSiteManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostSiteManagementPage.FindElements(By.XPath(HostSiteManagementPage.PortalsList)).Count, Is.EqualTo(itemNumber - 1),
					"The Child site is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Site name is not present in the list");
			Assert.IsFalse(hostSiteManagementPage.ElementPresent(By.XPath("//tr/td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + _baseUrl + "/" + _siteAlias + "')]")),
			"The Child site is not added correctly");
		}
	}
}
