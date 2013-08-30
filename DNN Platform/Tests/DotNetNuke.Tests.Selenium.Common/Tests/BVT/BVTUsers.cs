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
	[Category("BVT")]
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

		[Test]
		public void Test004_RegisterUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Register the User'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.RegisterUser(_registeredUserName, _registeredUserDisplayName, "registereduser@mail.com", _registeredUserPassword);

			_driver.Navigate().Refresh();

			loginPage.LoginAsHost(_baseUrl);

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingUrl(_baseUrl);

			manageUsersPage.AuthorizeUser(_registeredUserName);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			loginPage.LoginUsingLoginLink(_registeredUserName, _registeredUserPassword);

			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is present on the screen");
			Assert.That(mainPage.FindElement(By.XPath(ControlPanelIDs.RegisterLink)).Text, Is.EqualTo(_registeredUserDisplayName),
			            "The registered User is not logged in correctly");
		}

		[Test]
		public void Test005_RegisteredUserChangesProfile()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Registered User changes Profile'");

			var loginPage = new LoginPage(_driver);
			loginPage.LetMeOut();

			loginPage.OpenUsingUrl(_baseUrl);
			loginPage.DoLoginUsingUrl(_registeredUserName, _registeredUserPassword);

			var manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);

			manageUserProfilePage.AddCity(_cityName);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenUsingLink(_baseUrl);

			userAccountPage.OpenMyProfileInfo();

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the City Info is present on the screen");
			Assert.That(userAccountPage.FindElement(By.XPath(UserAccountPage.LocationCity)).Text, Is.EqualTo(_cityName),
			            "The City Info is not displayed correctly");
		}
	}
}