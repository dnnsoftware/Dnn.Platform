using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.P1
{
	[TestFixture]
	[Category("P1")]
	public class P1ManageWebServers : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;
		private string _serverName = null;

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("page");

			string testName = pageSettings.Attribute("name").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

			HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.OpenUsingButtons(_baseUrl);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.BasicSettingsTab)).Click();
			_serverName = hostSettingsPage.FindElement(By.XPath(HostSettingsPage.HostName)).Text;

		}

		[Test]
		public void Test001_VerifyDefaultSettingsOnCaching()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Default Settings On Caching'");

			HostManageWebSitesPage hostManageWebSitesPage = new HostManageWebSitesPage(_driver);
			hostManageWebSitesPage.OpenUsingButtons(_baseUrl);
			hostManageWebSitesPage.AccordionOpen(By.XPath(HostManageWebSitesPage.CachingAccordion));

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.CachingProviderDefaultSelection)).GetAttribute("value"), 
						Is.EqualTo(HostManageWebSitesPage.CachingProviderDefaultValue),
						"The Default value is not correct"); 
			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.WebFarmCheckbox)).FindElement(By.XPath("./following-sibling::*")).GetAttribute("class"),
						Is.Not.Contains("checked"),
						"The 'Web Farm' checkbox is checked");
			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.IISAppNameCheckbox)).FindElement(By.XPath("./following-sibling::*")).GetAttribute("class"),
						Is.Not.Contains("checked"),
						"The 'IIS App Name' checkbox is checked");
		}

		[Test]
		public void Test002_VerifyDefaultSettingsOnServers()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Default Settings On Servers'");

			HostManageWebSitesPage hostManageWebSitesPage = new HostManageWebSitesPage(_driver);
			hostManageWebSitesPage.OpenUsingButtons(_baseUrl);
			hostManageWebSitesPage.AccordionOpen(By.XPath(HostManageWebSitesPage.ServersAccordion));

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.ServerName)).Text,
						Is.EqualTo(_serverName),
						"The Server name is not correct");

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.Url)).Text,
						Is.Empty,
						"The Url should be hidden");

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.ServerEnabled)).Text,
						Is.EqualTo("No"),
						"The Server should be disabled by default");
		}

		[Test]
		public void Test003_EditSettingsOnServers()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit default Settings On Servers'");

			HostManageWebSitesPage hostManageWebSitesPage = new HostManageWebSitesPage(_driver);
			hostManageWebSitesPage.OpenUsingButtons(_baseUrl);
			hostManageWebSitesPage.AccordionOpen(By.XPath(HostManageWebSitesPage.ServersAccordion));

			hostManageWebSitesPage.EnableServer();

			hostManageWebSitesPage.OpenUsingButtons(_baseUrl);
			hostManageWebSitesPage.AccordionOpen(By.XPath(HostManageWebSitesPage.ServersAccordion));
			Assert.That(hostManageWebSitesPage.WaitForElement(By.XPath(HostManageWebSitesPage.Url)).Text,
						Is.EqualTo(_baseUrl),
						"The Url should be shown");

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.ServerEnabled)).Text,
						Is.EqualTo("Yes"),
						"The Server should be enabled");
		}

		[Test]
		public void Test004_VerifyDefaultSettingsOnMemoryUsage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Default Settings On Memory Usage'");

			HostManageWebSitesPage hostManageWebSitesPage = new HostManageWebSitesPage(_driver);
			hostManageWebSitesPage.OpenUsingButtons(_baseUrl);
			hostManageWebSitesPage.AccordionOpen(By.XPath(HostManageWebSitesPage.MemoryUsageAccordion));

			Assert.That(hostManageWebSitesPage.WaitForElement(By.XPath(HostManageWebSitesPage.ServerNameOnMemoryUsage)).Text,
						Is.EqualTo(_serverName),
						"The Server name is not correct");

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.MemoryLimit)).Text,
						Is.Not.Empty,
						"The Memory Limit should be displayed");

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.PrivateBytes)).Text,
						Is.Not.Empty,
						"The Private Bytes should be displayed");

			Assert.That(hostManageWebSitesPage.FindElement(By.XPath(HostManageWebSitesPage.CacheObjects)).Text,
						Is.Not.Empty,
						"The Cache Objects should be displayed");
		}

		[Test]
		public void Test005_VerifyDefaultSettingsOnSSLOffloadHeader()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Default Settings On SSL Offload Header'");

			HostManageWebSitesPage hostManageWebSitesPage = new HostManageWebSitesPage(_driver);
			hostManageWebSitesPage.OpenUsingButtons(_baseUrl);
			hostManageWebSitesPage.AccordionOpen(By.XPath(HostManageWebSitesPage.SSLOffloadHeaderAccordion));

			Assert.That(hostManageWebSitesPage.WaitForElement(By.XPath(HostManageWebSitesPage.HeaderValueTextBox)).Text,
						Is.Empty,
						"The Header Value should be NOT be set by default");
		}
	}
}
