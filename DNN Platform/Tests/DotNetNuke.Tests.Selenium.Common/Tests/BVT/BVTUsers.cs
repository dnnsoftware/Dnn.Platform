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
	public abstract class BVTUsers : CommonTestSteps
	{
		public string _userName;
		public string _userDisplayName;
		public string _registeredUserName;
		public string _registeredUserDisplayName;
		public string _registeredUserPassword;
		public string _cityName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("users");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;
			_userName = testSettings.Attribute("userName").Value;
			_userDisplayName = testSettings.Attribute("userDisplayName").Value;

			_registeredUserName = testSettings.Attribute("registeredUserName").Value;
			_registeredUserDisplayName = testSettings.Attribute("registeredUserDisplayName").Value;

			_registeredUserPassword = testSettings.Attribute("registeredUserPassword").Value;
			_cityName = testSettings.Attribute("cityName").Value;

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
		public void Test001_AddUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new User'");

			var manageUsersPage = new ManageUsersPage(_driver);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.WaitForElement(By.XPath(ManageUsersPage.UsersTable));
			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.AddNewUser(_userName, _userDisplayName, "user@mail.com", "wordPass30");

			manageUsersPage.WaitForElement(By.XPath(ManageUsersPage.UsersTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count, Is.EqualTo(itemNumber + 1),
			            "The User is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is present in the list");
			Assert.IsTrue(manageUsersPage.ElementPresent(By.XPath("//tr/td[text() = '" + _userName + "']")),
			              "The User is not added correctly");
		}

		[Test]
		public void Test002_DeleteUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the User'");

			var manageUsersPage = new ManageUsersPage(_driver);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.WaitForElement(By.XPath(ManageUsersPage.UsersTable));
			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.DeleteUser(_userName);

			manageUsersPage.WaitForElement(By.XPath(ManageUsersPage.UsersTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list is not changed");
			Assert.That(manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count, Is.EqualTo(itemNumber),
			            "The User is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is present in the list");
			Assert.IsTrue(manageUsersPage.ElementPresent(By.XPath("//tr/td[text() = '" + _userName + "']")),
			              "The User is not deleted correctly");
		}

		[Test]
		public void Test003_RemoveDeletedUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the User'");

			var manageUsersPage = new ManageUsersPage(_driver);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.WaitForElement(By.XPath(ManageUsersPage.UsersTable));
			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.RemoveDeletedUser(_userName);

			manageUsersPage.WaitForElement(By.XPath(ManageUsersPage.UsersTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count, Is.EqualTo(itemNumber - 1),
			            "The User is not removed correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is not present in the list");
			Assert.IsFalse(manageUsersPage.ElementPresent(By.XPath("//tr[td[text() = '" + _userName + "']]")),
			               "The User is not removed correctly");
		}

	}
}