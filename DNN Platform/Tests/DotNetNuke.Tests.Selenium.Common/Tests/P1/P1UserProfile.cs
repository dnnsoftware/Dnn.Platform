using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	public abstract class P1UserProfile : CommonTestSteps
	{
		private string _registeredUserName;
		private string _registeredUserDisplayName;
		private string _registeredUserPassword;
		private string _newPassword;
		private string _avatarFileToUpload;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("userProfile");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_registeredUserName = testSettings.Attribute("registeredUserName").Value;
			_registeredUserPassword = testSettings.Attribute("registeredUserPassword").Value;
			_registeredUserDisplayName = testSettings.Attribute("registeredUserDisplayName").Value;
			_newPassword = testSettings.Attribute("newPassword").Value;
			_avatarFileToUpload = testSettings.Attribute("avatarFileToUpload").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(_registeredUserName, _registeredUserDisplayName, "user10@mail.com", _registeredUserPassword);

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
			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.DeleteUser(_registeredUserName);
			manageUsersPage.RemoveDeletedUsers();
		}

		[Test]
		public void Test001_RegisteredUserUploadsAvatar()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Registered User uploads Avatar'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _registeredUserName, _registeredUserPassword);

			var manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);

			manageUserProfilePage.AddProfileAvatar(_avatarFileToUpload);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenUsingLink(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT avatar file is loaded correctly");

			//userAccountPage.WaitForElement(By.XPath(ManageUserProfilePage.ProfilePhoto)).Info();
			Assert.That(manageUserProfilePage.WaitForElement(By.XPath(ManageUserProfilePage.ProfilePhoto)).GetAttribute("src"), Is.StringContaining(_avatarFileToUpload),
						"The User Avatar is not added correctly");
		}

		[Test]
		public void Test002_RegisteredUserRemovesAvatar()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Registered User removes Avatar'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _registeredUserName, _registeredUserPassword);

			var manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);

			manageUserProfilePage.ChangeProfileAvatar(_avatarFileToUpload, "<None Specified>");

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenUsingLink(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT avatar file is loaded correctly");
			Assert.That(manageUserProfilePage.WaitForElement(By.XPath(ManageUserProfilePage.ProfilePhoto)).GetAttribute("src"), Is.StringContaining("no_avatar.gif"),
						"The User Avatar is not removed correctly");
		}

		[Test]
		public void Test003_RegisteredUserChangesProfile()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Registered User changes Password'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _registeredUserName, _registeredUserPassword);

			var manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);

			manageUserProfilePage.ChangePassword(_registeredUserPassword, _newPassword);

			loginPage.LoginUsingLoginLink(_registeredUserName, _newPassword);

			var mainPage = new MainPage(_driver);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the name of logged User is correct");
			Assert.That(mainPage.WaitForElement(By.XPath(ControlPanelIDs.RegisterLink)).Text,
						Is.EqualTo(_registeredUserDisplayName),
						"The User is not added correctly");
		}
	}
}
