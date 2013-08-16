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
	class P1AdminExtensions : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _adminExtensionToEdit = null;
		
		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("adminExtensions");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = pageSettings.Attribute("name").Value;

			_adminExtensionToEdit = pageSettings.Attribute("adminExtensionToEdit").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
			
			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser("Admin", "Admin", "admin@mail.com", "dnnadmin");
			manageUsersPage.ManageRoles("Admin");
			manageUsersPage.AssignRoleToUser("Administrators");
			
			_loginPage.LoginUsingUrl(_baseUrl, "Admin", "dnnadmin");
		}

		[Test]
		public void Test001_EditAdminExtension()
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
		public void Test002_VerifyAdminLimitedAccess()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Admin limited access to Extensions'");

			AdminExtensionsPage adminExtensionsPage = new AdminExtensionsPage(_driver);
			adminExtensionsPage.OpenUsingButtons(_baseUrl);

			Assert.That(adminExtensionsPage.FindElements(By.XPath(AdminExtensionsPage.ButtonsList)).Count, Is.EqualTo(0),
				 "The number of available Action buttons is not correct");
		}
	}
}
