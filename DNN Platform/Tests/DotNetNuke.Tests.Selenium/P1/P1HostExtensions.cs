using System.Diagnostics;
using System.Threading;
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
	public class P1HostExtensions : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _hostExtensionNameToInstall = null;
		private string _hostFileToUpload = null;

		private string _hostExtensionToEdit = null;
		private string _newDescription = null;

		private string _extensionNameToCreate = null;
		private string _friendlyExtensionNameToCreate = null;

		private string _moduleNameToCreate = null;

		private string _adminExtensionNameToInstall = null;
		private string _adminFileToUpload = null;

		private string _adminExtensionToEdit = null;


		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("hostExtensions");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = pageSettings.Attribute("name").Value;

			_hostExtensionNameToInstall = pageSettings.Attribute("hostExtensionNameToInstall").Value;
			_hostFileToUpload = pageSettings.Attribute("hostFileToUpload").Value;

			_hostExtensionToEdit = pageSettings.Attribute("hostExtensionToEdit").Value;
			_newDescription = pageSettings.Attribute("newDescription").Value;

			_extensionNameToCreate = pageSettings.Attribute("extensionNameToCreate").Value;
			_friendlyExtensionNameToCreate = pageSettings.Attribute("friendlyExtensionNameToCreate").Value;

			_moduleNameToCreate = pageSettings.Attribute("moduleNameToCreate").Value;

			_adminExtensionNameToInstall = pageSettings.Attribute("adminExtensionNameToInstall").Value;
			_adminFileToUpload = pageSettings.Attribute("adminFileToUpload").Value;

			_adminExtensionToEdit = pageSettings.Attribute("adminExtensionToEdit").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

		}

		[Test]
		public void Test001_InstallHostExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install a Host Extension'");

			HostExtensionsPage hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.InstallExtension(_hostFileToUpload);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count, Is.EqualTo(itemNumber + 1),
						"The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _hostExtensionNameToInstall + "']]")),
						"The Extension name is not present in the list");
		}

		[Test]
		public void Test002_EditHostExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Host Extension'");

			HostExtensionsPage hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			hostExtensionsPage.EditExtensionDescription(_hostExtensionToEdit, _newDescription);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension description is present in the list");
			Assert.That(hostExtensionsPage.FindElement(By.XPath("//tr[td/span[text() = '" + _hostExtensionToEdit + "']]/td[contains(@class, 'Header')]/span")).Text, Is.EqualTo(_newDescription),
						"The Extension name is not present in the list");
		}

		[Test]
		public void Test003_DeleteHostExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Host Extension'");

			HostExtensionsPage hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.DeleteExtension(_hostExtensionNameToInstall);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count, Is.EqualTo(itemNumber - 1),
						"The Extension is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsFalse(hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _hostExtensionNameToInstall + "']]")),
						"The Extension name is present in the list");
		}

		[Test]
		public void Test004_CreateNewExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Create a new Extension'");

			HostExtensionsPage hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.CreateNewExtension(_extensionNameToCreate, _friendlyExtensionNameToCreate, "Module");

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count, Is.EqualTo(itemNumber + 1),
						"The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _friendlyExtensionNameToCreate + "']]")),
						"The Extension name is not present in the list");
		}

		[Test]
		public void Test005_CreateNewModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Create a new Module'");

			HostExtensionsPage hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.CreateNewModule(_moduleNameToCreate, "Admin", "Banners", "file.cs");

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(HostExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count, Is.EqualTo(itemNumber + 1),
						"The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _moduleNameToCreate + "']]")),
						"The Extension name is not present in the list");
		}

		[Test]
		public void Test006_InstallAdminExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install an Admin Extension'");

			AdminExtensionsPage adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);

			int itemNumber = adminExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count;

			adminExtensionsPage.InstallExtension(_adminFileToUpload);

			adminExtensionsPage.OpenUsingButtons(_baseUrl);
			adminExtensionsPage.AccordionOpen(By.XPath(AdminExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(adminExtensionsPage.FindElements(By.XPath(HostExtensionsPage.ModulesExtensionsList)).Count, Is.EqualTo(itemNumber + 1),
						"The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(adminExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _adminExtensionNameToInstall + "']]")),
						"The Extension name is not present in the list");
		}

		[Test]
		public void Test007_EditAdminExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Admin Extension'");

			AdminExtensionsPage adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);
			adminExtensionsPage.AccordionOpen(By.XPath(AdminExtensionsPage.ModulesAccordion));

			adminExtensionsPage.EditExtensionPermissions(_adminExtensionToEdit);

			adminExtensionsPage.OpenUsingButtons(_baseUrl);
			adminExtensionsPage.AccordionOpen(By.XPath(AdminExtensionsPage.ModulesAccordion));
			adminExtensionsPage.EditExtension(_adminExtensionToEdit);
			adminExtensionsPage.AccordionOpen(By.XPath(AdminExtensionsPage.ModuleSettingsAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Permission is granted");
			Assert.That(adminExtensionsPage.WaitForElement(By.XPath(AdminExtensionsPage.PermissionTable + "'All Users'" + "]]/td/img")).GetAttribute("src"),
						Is.StringContaining("Grant"),
						"The Permission is not granted");
		}

		[Test]
		public void Test008_VerifyAdminLimitedAccess()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Admin limited access to Extensions'");

			AdminExtensionsPage adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);

			Assert.That(adminExtensionsPage.FindElements(By.XPath(AdminExtensionsPage.ButtonsList)).Count, Is.EqualTo(1),
				 "The number of available Action buttons is not correct");
		}
	}
}
