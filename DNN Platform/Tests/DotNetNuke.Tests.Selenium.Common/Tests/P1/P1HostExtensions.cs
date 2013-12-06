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
	public abstract class P1HostExtensions : CommonTestSteps
	{
		private string _hostExtensionNameToInstall;
		private string _hostFileToUpload;

		private string _hostExtensionToEdit;
		private string _newDescription;

		private string _extensionNameToCreate;
		private string _friendlyExtensionNameToCreate;

		private string _moduleNameToCreate;

		private string _adminExtensionNameToInstall;
		private string _adminFileToUpload;

		private string _adminExtensionToEdit;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("hostExtensions");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_hostExtensionNameToInstall = testSettings.Attribute("hostExtensionNameToInstall").Value;
			_hostFileToUpload = testSettings.Attribute("hostFileToUpload").Value;

			_hostExtensionToEdit = testSettings.Attribute("hostExtensionToEdit").Value;
			_newDescription = testSettings.Attribute("newDescription").Value;

			_extensionNameToCreate = testSettings.Attribute("extensionNameToCreate").Value;
			_friendlyExtensionNameToCreate = testSettings.Attribute("friendlyExtensionNameToCreate").Value;

			_moduleNameToCreate = testSettings.Attribute("moduleNameToCreate").Value;

			_adminExtensionNameToInstall = testSettings.Attribute("adminExtensionNameToInstall").Value;
			_adminFileToUpload = testSettings.Attribute("adminFileToUpload").Value;

			_adminExtensionToEdit = testSettings.Attribute("adminExtensionToEdit").Value;

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
		public void Test001_InstallHostExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install a Host Extension'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.InstallExtension(@"P1\" + _hostFileToUpload);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(
				hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _hostExtensionNameToInstall + "']]")),
				"The Extension name is not present in the list");
		}

		[Test]
		public void Test002_EditHostExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Host Extension'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			hostExtensionsPage.EditExtensionDescription(_hostExtensionToEdit, _newDescription);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension description is present in the list");
			Assert.That(
				hostExtensionsPage.FindElement(
					By.XPath("//tr[td/span[text() = '" + _hostExtensionToEdit + "']]/td[contains(@class, 'Header')]/span")).Text,
				Is.EqualTo(_newDescription),
				"The Extension name is not present in the list");
		}

		[Test]
		public void Test003_DeleteHostExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Host Extension'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count;
			
			hostExtensionsPage.DeleteExtension(_hostExtensionNameToInstall);

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count,
			            Is.EqualTo(itemNumber - 1),
			            "The Extension is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsFalse(
				hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _hostExtensionNameToInstall + "']]")),
				"The Extension name is present in the list");
		}

		[Test]
		public void Test004_CreateNewExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Create a new Extension'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.CreateNewExtension(_extensionNameToCreate, _friendlyExtensionNameToCreate, "Module");

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(
				hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _friendlyExtensionNameToCreate + "']]")),
				"The Extension name is not present in the list");
		}

		[Test]
		public void Test005_CreateNewModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Create a new Module'");

			var hostExtensionsPage = new HostExtensionsPage(_driver);
			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			int itemNumber = hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count;

			hostExtensionsPage.CreateNewModule(_moduleNameToCreate, "Admin", "Banners", "file.cs");

			hostExtensionsPage.OpenUsingButtons(_baseUrl);
			hostExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(hostExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _moduleNameToCreate + "']]")),
			              "The Extension name is not present in the list");
		}

		[Test]
		public void Test006_InstallAdminExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Install an Admin Extension'");

			var adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);

			int itemNumber = adminExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count;

			adminExtensionsPage.InstallExtension(@"P1\" + _adminFileToUpload);

			adminExtensionsPage.OpenUsingButtons(_baseUrl);
			adminExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(adminExtensionsPage.FindElements(By.XPath(ExtensionsPage.ModulesExtensionsList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The Extension is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Extension name is present in the list");
			Assert.IsTrue(
				adminExtensionsPage.ElementPresent(By.XPath("//tr[td/span[text() = '" + _adminExtensionNameToInstall + "']]")),
				"The Extension name is not present in the list");
		}

		[Test]
		public void Test007_EditAdminExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Admin Extension'");

			var adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);
			adminExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));

			adminExtensionsPage.EditExtensionPermissions(_adminExtensionToEdit);

			adminExtensionsPage.OpenUsingButtons(_baseUrl);
			adminExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModulesAccordion));
			adminExtensionsPage.EditExtension(_adminExtensionToEdit);
			adminExtensionsPage.AccordionOpen(By.XPath(ExtensionsPage.ModuleSettingsAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Permission is granted");
			Assert.That(
				adminExtensionsPage.WaitForElement(By.XPath(AdminExtensionsPage.PermissionTable + "'All Users'" + "]]/td/img")).
					GetAttribute("src"),
				Is.StringContaining("Grant"),
				"The Permission is not granted");
		}

		[Test]
		public void Test008_VerifyAdminLimitedAccess()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Admin limited access to Extensions'");

			var adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);

			Assert.That(adminExtensionsPage.FindElements(By.XPath(ExtensionsPage.ButtonsList)).Count, Is.EqualTo(1),
			            "The number of available Action buttons is not correct");
		}
	}
}