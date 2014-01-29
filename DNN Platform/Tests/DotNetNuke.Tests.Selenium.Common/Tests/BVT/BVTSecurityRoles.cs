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
	public abstract class BVTSecurityRoles : CommonTestSteps
	{
		public string _roleName;
		public string _roleDescription;
		public string _assignedRoleName;
		public string _userName;
		public string _userDisplayName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("securityRoles");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_roleName = testSettings.Attribute("roleName").Value;
			_roleDescription = testSettings.Attribute("roleDescription").Value;

			_assignedRoleName = testSettings.Attribute("assignedRoleName").Value;
			_userName = testSettings.Attribute("userName").Value;
			_userDisplayName = testSettings.Attribute("userDisplayName").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			var manageRolesPage = new ManageRolesPage(_driver);
			manageRolesPage.OpenUsingControlPanel(_baseUrl);
			manageRolesPage.AddNewSecurityRole(_assignedRoleName + "001");
			manageRolesPage.AddNewSecurityRole(_assignedRoleName + "002");

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(_userName + "001", _userDisplayName + "001", "user10@mail.com", "pAssword10");
			manageUsersPage.AddNewUser(_userName + "002", _userDisplayName + "002", "user10@mail.com", "pAssword10");

			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		[Test]
		public void Test001_AddSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Security Role'");

			var manageRolesPage = new ManageRolesPage(_driver);
			manageRolesPage.OpenUsingControlPanel(_baseUrl);
			manageRolesPage.WaitForElement(By.XPath(ManageRolesPage.SecurityRolesTable));
			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewSecurityRole(_roleName);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			manageRolesPage.WaitForElement(By.XPath(ManageRolesPage.SecurityRolesTable));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(itemNumber + 1,
			            Is.EqualTo(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count),
			            "The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role is present in the list");
			Assert.IsTrue(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _roleName + "']]")),
			              "The Security role is not added correctly");
		}

		[Test]
		public void Test002_EditSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Security Role'");

			var manageRolesPage = new ManageRolesPage(_driver);
			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			manageRolesPage.AddDescriptionToSecurityRole(_roleName, _roleDescription);

			manageRolesPage.WaitForElement(By.XPath(ManageRolesPage.SecurityRolesTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security description is present in the list");
			Assert.That(_roleDescription,
			            Is.EqualTo(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleName + "']]/td[4]")).Text),
			            "The role description is not added correctly");
		}

		[Test]
		public void Test003_DeleteSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Security Role'");

			var manageRolesPage = new ManageRolesPage(_driver);
			manageRolesPage.OpenUsingControlPanel(_baseUrl);
			manageRolesPage.WaitForElement(By.XPath(ManageRolesPage.SecurityRolesTable));
			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.DeleteSecurityRole(_roleName);

			manageRolesPage.WaitForElement(By.XPath(ManageRolesPage.SecurityRolesTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(itemNumber - 1,
			            Is.EqualTo(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count),
			            "The security role is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role is not present in the list");
			Assert.IsFalse(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _roleName + "']]")),
			               "The Security role is not deleted correctly");
		}

		[Test]
		public void Test004_AssignRoleToUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Assign the Role to User'");

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			manageUsersPage.ManageRoles(_userName + "001");

			manageUsersPage.AssignRoleToUser(_assignedRoleName + "001");

			var manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of Users assigned to the Role");
			Assert.That(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _assignedRoleName + "001']]/td[13]")).Text,
			            Is.EqualTo("1"),
			            "The role is not assigned correctly to User");
		}

		[Test]
		public void Test005_AssignUserToRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Assign the User th Role'");

			var adminSecurityRolesPage = new AdminSecurityRolesPage(_driver);
			adminSecurityRolesPage.OpenUsingControlPanel(_baseUrl);

			adminSecurityRolesPage.ManageUsers(_assignedRoleName + "002");

			adminSecurityRolesPage.AssignRoleToUser(_userName + "002");

			adminSecurityRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of Users assigned to the Role");
			Assert.That(adminSecurityRolesPage.FindElement(By.XPath("//tr[td[contains(string(), '" + _assignedRoleName + "002')]]/td[13]")).Text,
						Is.EqualTo("1"),
						"The role is not assigned correctly to User");
		}
	}
}