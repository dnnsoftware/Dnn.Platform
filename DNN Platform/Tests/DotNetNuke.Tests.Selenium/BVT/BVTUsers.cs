using System.Diagnostics;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.BVT
{
	[TestFixture("UserBVT",
					"User BVT",
					"user@mail.com",
					"wordPass30",
					"RegisteredUserBVT",
					"Registered User BVT",
					"registereduser@mail.com",
					"wordRTYU34",
					"Vancouver")]

	[Category("BVT")]
	public class BVTUsers : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _userName = null;
		private string _userDisplayName = null;
		private string _userEmail = null;
		private string _userPassword = null;
		private string _registeredUserName = null;
		private string _registeredUserDisplayName = null;
		private string _registeredUserEmail = null;
		private string _registeredUserPassword = null;
		private string _cityName = null;

		public BVTUsers(string userName, 
					string userDisplayName,
					string userEmail,
					string userPassword,
					string registeredUserName,
					string registeredUserDisplayName,
					string registeredUserEmail,
					string registeredUserPassword,
					string cityName)
		{
			this._userName = userName;
			this._userDisplayName = userDisplayName;
			this._userEmail = userEmail;
			this._userPassword = userPassword;
			this._registeredUserName = registeredUserName;
			this._registeredUserDisplayName = registeredUserDisplayName;
			this._registeredUserEmail = registeredUserEmail;
			this._registeredUserPassword = registeredUserPassword;
			this._cityName = cityName;
		}

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Users BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[Test]
		public void Test001_AddUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new User'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.AddNewUser(_userName, _userDisplayName, _userEmail, _userPassword);

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

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.DeleteUser(_userName);

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

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageUsersPage.FindElements(By.XPath(ManageUsersPage.UsersList)).Count;

			manageUsersPage.RemoveDeletedUser(_userName);

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

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage.LetMeOut();

			_loginPage.DoRegisterUsingRegisterLink(_registeredUserName, _registeredUserDisplayName, _registeredUserEmail, _registeredUserPassword);

			_driver.Navigate().Refresh();

			_loginPage.WaitForElement(By.XPath("//*[@id='" + LoginPage.LoginLink + "' and not(contains(@href, 'Logoff'))]"), 20).WaitTillVisible(20).Click();

			_loginPage.DoLoginUsingLoginLink("host", "dnnhost");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingUrl(_baseUrl);

			manageUsersPage.AuthorizeUser(_registeredUserName);

			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			_loginPage.LoginUsingLoginLink(_registeredUserName, _registeredUserPassword);

			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is present on the screen");
			Assert.That(mainPage.FindElement(By.Id(BasePage.RegisteredUserLink)).Text, Is.EqualTo(_registeredUserDisplayName),
					"The registered User is not logged in correctly");
		}

		[Test]
		public void Test005_RegisteredUserChangesProfile()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Registered User changes Profile'");

			_loginPage.LetMeOut();

			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl(_registeredUserName, _registeredUserPassword);

			ManageUserProfilePage manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);

			manageUserProfilePage.AddCity(_cityName);

			UserAccountPage userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenUsingLink(_baseUrl);

			userAccountPage.OpenMyProfileInfo();

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the City Info is present on the screen");
			Assert.That(userAccountPage.FindElement(By.XPath(UserAccountPage.LocationCity)).Text, Is.EqualTo(_cityName),
					"The City Info is not displayed correctly");
		}
	}
}
