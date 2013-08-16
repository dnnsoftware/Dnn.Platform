using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.SmokeTest
{
	[TestFixture]
	[Category("Smoke")]
	public class Smoke : TestBase
	{
		private string _baseUrl = null;
		private IWebDriver _driver = null;

		private string _siteAlias = null;
		private string _siteName = null;

		private string _pageName = null;
		private string _userName = null;

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"SmokeTest\" + Settings.Default.SmokeDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			_siteAlias = settings.Attribute("siteAlias").Value;
			_siteName = settings.Attribute("siteName").Value;
			_pageName = settings.Attribute("pageName").Value;
			_userName = settings.Attribute("userName").Value;

			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			var installerPage = new InstallerPage(_driver);
			installerPage.OpenUsingUrl(_baseUrl);
			//installerPage.WelcomeScreen();

			LoginPage loginPage = new LoginPage(_driver);

			loginPage.WaitForElement(By.XPath("//*[@id='" + LoginPage.LoginLink + "' and not(contains(@href, 'Logoff'))]"), 20).WaitTillVisible(20).Click();

			loginPage.WaitAndSwitchToFrame(30);

			loginPage.DoLoginUsingLoginLink("host", "dnnhost");

			loginPage.WaitAndSwitchToWindow(30);

			AdminSiteSettingsPage adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);
			adminSiteSettingsPage.DisablePopups();
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
		public void Test002_DeleteSite()
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

		[Test]
		public void Test003_AddPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Page'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);

			blankPage.AddNewPage(_pageName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the user redirected to newly created page: " + "http://" + _baseUrl.ToLower() + "/" + _pageName);
			Assert.That(blankPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _pageName),
				"The page URL is not correct");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + " is present in the list");
			Assert.IsTrue(adminPageManagementPage.ElementPresent(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[@class= 'rtLI']//span[contains(text(), '" + _pageName + "')]")),
				"The page is not present in the list");
		}

		[Test]
		public void Test004_DeletePage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			blankPage.DeletePage(_pageName);

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the list");
			Assert.IsFalse(adminPageManagementPage.ElementPresent(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[@class= 'rtLI']//span[contains(text(), '" + _pageName + "')]")),
				"The page is present in the list");

			AdminRecycleBinPage adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in Recycle Bin");
			Assert.IsTrue(adminRecycleBinPage.ElementPresent(By.XPath(AdminRecycleBinPage.RecycleBinPageContainerOption + "[contains(text(), '" + _pageName + "')]")));
		}

		[Test]
		public void Test005_AddUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new User'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.AddNewUser(_userName, "User Display Name", "user@Email.com", "pAssWrd90");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count, Is.EqualTo(itemNumber + 1),
						"The User is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is present in the list");
			Assert.IsTrue(manageUsersPage.ElementPresent(By.XPath("//tr/td[text() = '" + _userName + "']")),
						"The User is not added correctly");
		}

		[Test]
		public void Test006_DeleteUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the User'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.DeleteUser(_userName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list is not changed");
			Assert.That(manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count, Is.EqualTo(itemNumber),
						"The User is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is present in the list");
			Assert.IsTrue(manageUsersPage.ElementPresent(By.XPath("//tr/td[text() = '" + _userName + "']")),
						"The User is not deleted correctly");
		}
	}
}
