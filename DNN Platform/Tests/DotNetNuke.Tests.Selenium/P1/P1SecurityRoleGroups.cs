using System.Diagnostics;
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
	class P1SecurityRoleGroups : TestBase
	{
			private string _baseUrl = null;
			private LoginPage _loginPage = null;
			private IWebDriver _driver = null;
			private string _roleGroupName = null;
			private string _roleGroupDescription = null;
			private string _roleName = null;
			private string _assignedRoleGroupName = null;

			[TestFixtureSetUp]
			public void LoginToSite()
			{
				var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

				XElement settings = doc.Document.Element("Tests").Element("settings");
				XElement groupSettings = doc.Document.Element("Tests").Element("securityRoleGroup");

				string testName = groupSettings.Attribute("name").Value;
				_roleGroupName = groupSettings.Attribute("roleGroupName").Value;
				_roleGroupDescription = groupSettings.Attribute("roleGroupDescription").Value;
				_roleName = groupSettings.Attribute("roleName").Value;
				_assignedRoleGroupName = groupSettings.Attribute("assignedRoleGroupName").Value;

				_driver = StartBrowser(settings.Attribute("browser").Value);
				_baseUrl = settings.Attribute("baseURL").Value;

				Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
				Trace.WriteLine(BasePage.PreconditionsKeyWord);

				MainPage mainPage = new MainPage(_driver);
				mainPage.OpenUsingUrl(_baseUrl);

				_loginPage = new LoginPage(_driver);
				_loginPage.OpenUsingUrl(_baseUrl);
				_loginPage.DoLoginUsingUrl("host", "dnnhost");

				ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);
				manageRolesPage.OpenUsingControlPanel(_baseUrl);
				manageRolesPage.AddNewSecurityRole(_roleName);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);
				manageRolesPage.AddNewSecurityRoleGroup(_assignedRoleGroupName); 
			}

			[Test]
			public void Test001_AddSecurityRoleGroup()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Security Role Group'");

				ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);

				int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.FilterByGroupDropdownList)).Count;

				manageRolesPage.AddNewSecurityRoleGroup(_roleGroupName);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the drop-down increased by 1");
				Assert.That(itemNumber + 1, Is.EqualTo(manageRolesPage.FindElements(By.XPath(ManageRolesPage.FilterByGroupDropdownList)).Count),
							"The security role is not added correctly");

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role group is present in the list");
				Assert.IsTrue(manageRolesPage.ElementPresent(By.XPath(ManageRolesPage.FilterByGroupDropdownList + "[text() = '" + _roleGroupName + "']")),
					"The Security role is not added correctly");
			}

			[Test]
			public void Test003_DeleteSecurityRoleGroup()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Security Role Group'");

				ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);

				int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.FilterByGroupDropdownList)).Count;

				manageRolesPage.DeleteSecurityRoleGroup(_roleGroupName);

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the drop-down decreased by 1");
				Assert.That(itemNumber - 1, Is.EqualTo(manageRolesPage.FindElements(By.XPath(ManageRolesPage.FilterByGroupDropdownList)).Count),
							"The security role is not deleted correctly");

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role is not present in the list");
				Assert.IsFalse(manageRolesPage.ElementPresent(By.XPath(ManageRolesPage.FilterByGroupDropdownList + "[text() = '" + _roleGroupName + "']")),
							"The Security role is not deleted correctly");
			}

			[Test]
			public void Test002_EditSecurityRoleGroup()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Security Role Group'");

				ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);

				manageRolesPage.AddDescriptionToSecurityRoleGroup(_roleGroupName, _roleGroupDescription);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);
				manageRolesPage.EditSecurityRoleGroup(_roleGroupName);

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role Group description is present in the box");
				Assert.That(_roleGroupDescription, Is.EqualTo(manageRolesPage.FindElement(By.XPath(ManageRolesPage.RoleGroupNameDescriptionTextBox)).Text),
							"The role description is not added correctly");
			}

			[Test]
			public void Test004_AssignRoleToRoleGroup()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Assign the Role to Role Group'");

				ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);
				manageRolesPage.SlidingSelectByValue(By.XPath("//a[contains(@id, '" + ManageRolesPage.FilterByGroupArrow + "')]"), By.XPath(ManageRolesPage.FilterByGroupDropdown), _assignedRoleGroupName); 

				int itemNumber = manageRolesPage.FindElements(By.XPath("//table/tbody/tr[contains(@id, 'Roles_grdRoles')]")).Count;

				manageRolesPage.OpenUsingControlPanel(_baseUrl);
				manageRolesPage.AssignSecurityRoleToGroup(_roleName, _assignedRoleGroupName);

				manageRolesPage.OpenUsingControlPanel(_baseUrl);
				manageRolesPage.SlidingSelectByValue(By.XPath("//a[contains(@id, '" + ManageRolesPage.FilterByGroupArrow + "')]"), By.XPath(ManageRolesPage.FilterByGroupDropdown), _assignedRoleGroupName);
				manageRolesPage.WaitForElement(By.XPath("//table[contains(@id, 'Roles_grdRoles')]/tbody/tr[last()]"));

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of Security roles assigned to the Group");
				Assert.That(itemNumber + 1, Is.EqualTo(manageRolesPage.FindElements(By.XPath("//table/tbody/tr[contains(@id, 'Roles_grdRoles')]")).Count),
							"The role is not assigned correctly to Group");

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Security role is present in the Group list");
				Assert.IsTrue(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _roleName + "']]")),
					"The Role is not assigned correctly to Group");
			}
		}
	} 
