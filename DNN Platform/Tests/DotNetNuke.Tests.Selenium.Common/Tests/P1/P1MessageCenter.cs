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
	public  abstract class P1MessageCenter : CommonTestSteps
	{
		private string _userNameNumberOne;
		private string _userNameNumberTwo;
		private string _userNameNumberThree;
		private string _userDisplayNameNumberOne;
		private string _userDisplayNameNumberTwo;
		private string _userDisplayNameNumberThree;
		private string _password;
		private string _subject;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("messageCenter");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_userNameNumberOne = testSettings.Attribute("userNameNumberOne").Value;
			_userNameNumberTwo = testSettings.Attribute("userNameNumberTwo").Value;
			_userNameNumberThree = testSettings.Attribute("userNameNumberThree").Value;
			_userDisplayNameNumberOne = testSettings.Attribute("userDisplayNameNumberOne").Value;
			_userDisplayNameNumberTwo = testSettings.Attribute("userDisplayNameNumberTwo").Value;
			_userDisplayNameNumberThree = testSettings.Attribute("userDisplayNameNumberThree").Value;
			_password = testSettings.Attribute("password").Value;
			_subject = testSettings.Attribute("subject").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(_userNameNumberOne, _userDisplayNameNumberOne, "user10@mail.com", _password);
			manageUsersPage.AddNewUser(_userNameNumberTwo, _userDisplayNameNumberTwo, "user10@mail.com", _password);
			manageUsersPage.AddNewUser(_userNameNumberThree, _userDisplayNameNumberThree, "user10@mail.com", _password);
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
			manageUsersPage.DeleteUser(_userNameNumberOne);
			manageUsersPage.DeleteUser(_userNameNumberTwo);
			manageUsersPage.DeleteUser(_userNameNumberThree);
			manageUsersPage.RemoveDeletedUsers();
		}

		[Test]
		public void Test001_UserSendsMessage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can send the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenMessagesLink(_baseUrl);

			var module = new MessageCenterModule(_driver);
			module.ComposeNewMessage(_userDisplayNameNumberOne, _subject, "Test Message");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT New message is present in the inbox:");
			Assert.That(module.WaitForElement(By.XPath("//ul[@id = 'inbox']//li[@class = 'ListCol-3']//a")).Text,
						Is.EqualTo(_subject),
						"Message subject is not found");

			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberOne, _password);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Message icon contains number '1' for incoming message :");
			Assert.That(loginPage.WaitForElement(By.XPath(ControlPanelIDs.MessageLink + "/span")).Text, Is.EqualTo("1"));

			module.OpenMessagesUsingIcon();
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT New message is present in the list:");
			Assert.That(module.WaitForElement(By.XPath("//ul[@id = 'inbox']//li[@class = 'ListCol-3']//a")).Text,
						Is.EqualTo(_subject),
						"Message subject is not found");
		}

		[Test]
		public void Test002_UserRepliesToMessage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can reply to the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberOne, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenMessagesLink(_baseUrl);

			var module = new MessageCenterModule(_driver);
			module.ReplyMessage(_subject, "Reply to Test Message");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Replied message is present in the Send box:");
			module.WaitAndClick(By.XPath(MessageCenterModule.MessagesTab));
			module.WaitAndClick(By.XPath(MessageCenterModule.SentTab));

			Assert.That(module.WaitForElement(By.XPath("//ul[@id = 'inbox']/li[1]//li[@class = 'ListCol-3']//a")).Text,
						Is.EqualTo(_subject),
						"Message subject is not found");

			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Message icon contains number '1' for incoming message :");
			Assert.That(loginPage.WaitForElement(By.XPath(ControlPanelIDs.MessageLink + "/span")).Text, Is.EqualTo("1"));

			module.OpenMessagesUsingIcon();
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT New message is present in the list:");
			Assert.That(module.WaitForElement(By.XPath("//ul[@id = 'inbox']//li[@class = 'ListCol-3']//a")).Text,
						Is.EqualTo(_subject),
						"Message subject is not found");
		}

		[Test]
		public void Test003_SendNotification()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Send Notification'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.RegisterUser("RegisteredUserName", "RegisteredUserDisplayName", "registereduser@mail.com", _password);

			loginPage.LoginAsHost(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Notification icon contains number '1' for incoming notification :");
			Assert.That(loginPage.WaitForElement(By.XPath(ControlPanelIDs.NotificationLink + "/span")).Text, Is.EqualTo("1"));

			var module = new MessageCenterModule(_driver);
			module.OpenNotificationsUsingIcon();

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT New notification is present in the list:");
			Assert.That(module.WaitForElement(By.XPath("//ul[@class = 'messages']//li[@class = 'ListCol-3']//span")).Text,
						Is.StringContaining("New User Registration"),
						"Notification subject is not found");
		}

		[Test]
		public void Test004_DismissNotification()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Dismiss Notification'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var module = new MessageCenterModule(_driver);
			module.OpenNotificationsUsingIcon();

			module.DismissNotification(MessageCenterModule.NotificationSubject);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT New notification is Not present in the list:");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@class = 'messages']//li[@class = 'ListCol-3']//span[contains(text(), '" + MessageCenterModule.NotificationSubject + "')]")),
						"Notification subject is found");
		}
	}
}
