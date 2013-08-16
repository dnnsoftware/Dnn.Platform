using System.Diagnostics;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.BVT
{
	[TestFixture("Security Role BVT", 
				"Security Role Description, BVT", 
				"assigned Security Role BVT", 
				"User Name BVT", 
				"Display Name BVT", 
				"user10@mail.com", 
				"pAssword10")]
	[Category("BVT")]
	public class BVTSecurityRoles : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _roleName = null;
		private string _roleDescription = null;
		private string _assignedRoleName = null;
		private string _userName = null;
		private string _userDisplayName = null;
		private string _userEmail = null;
		private string _userPassword = null;

		public BVTSecurityRoles(string roleName, 
							string roleDescription,
							string assignedRoleName,
							string userName,
							string userDisplayName,
							string userEmail,
							string userPassword)
		{
			this._roleName = roleName;
			this._roleDescription = roleDescription;
			this._assignedRoleName = assignedRoleName;
			this._userName = userName;
			this._userDisplayName = userDisplayName;
			this._userEmail = userEmail;
			this._userPassword = userPassword;
		}

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Security Roles BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);
			manageRolesPage.OpenUsingControlPanel(_baseUrl);
			manageRolesPage.AddNewSecurityRole(_assignedRoleName);

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(_userName, _userDisplayName, _userEmail, _userPassword);
		}

		[Test]
		public void Test001_AddSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Security Role'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewSecurityRole(_roleName);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(itemNumber + 1, Is.EqualTo(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count),
						"The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role is present in the list");
			Assert.IsTrue(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _roleName + "']]")),
				"The Security role is not added correctly");
		}

		[Test]
		public void Test003_DeleteSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Security Role'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver); 

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.DeleteSecurityRole(_roleName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(itemNumber - 1, Is.EqualTo(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count),
						"The security role is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role is not present in the list");
			Assert.IsFalse(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _roleName + "']]")),
						"The Security role is not deleted correctly");
		}

		[Test]
		public void Test002_EditSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Security Role'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			manageRolesPage.AddDescriptionToSecurityRole(_roleName, _roleDescription);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security description is present in the list");
			Assert.That(_roleDescription, Is.EqualTo(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleName + "']]/td[4]")).Text),
						"The role description is not added correctly");
		}

		[Test]
		public void Test004_AssignRoleToUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Assign the Role to User'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			manageUsersPage.ManageRoles(_userName);

			manageUsersPage.AssignRoleToUser(_assignedRoleName);

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of Users assigned to the Role");
			Assert.That("1", Is.EqualTo(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _assignedRoleName + "']]/td[13]")).Text),
						"The role is not assigned correctly to User");
		}
	}
}
