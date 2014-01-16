using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1AdminExtensions : CommonTestSteps
	{
		private string _adminExtensionToEdit;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("adminExtensions");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_adminExtensionToEdit = testSettings.Attribute("adminExtensionToEdit").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			CreateAdmin("Admin", "Admin", "admin@mail.com", "dnnadmin");
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
			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.DeleteUser("Admin");
			manageUsersPage.RemoveDeletedUsers();
		}

		[Test]
		public void Test001_EditAdminExtension()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Admin Extension'");

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginUsingLoginLink("Admin", "dnnadmin");

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
		public void Test002_VerifyAdminLimitedAccess()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Admin limited access to Extensions'");


			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginUsingLoginLink("Admin", "dnnadmin");

			var adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);

			Assert.That(adminExtensionsPage.FindElements(By.XPath(ExtensionsPage.ButtonsList)).Count, Is.EqualTo(0),
			            "The number of available Action buttons is not correct");
		}
	}
}