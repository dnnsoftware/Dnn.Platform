using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	public abstract class P1MemberDirectory : CommonTestSteps
	{
		private string _userNameNumberOne;
		private string _userNameNumberTwo;
		private string _userNameNumberThree;
		private string _userDisplayNameNumberOne;
		private string _userDisplayNameNumberTwo;
		private string _userDisplayNameNumberThree;
		private string _password;
		private string _pageName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("memberDirectory");

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
			_pageName = testSettings.Attribute("pageName").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(_userNameNumberOne, _userDisplayNameNumberOne, "user10@mail.com", _password);
			manageUsersPage.AddNewUser(_userNameNumberTwo, _userDisplayNameNumberTwo, "user10@mail.com", _password);
			manageUsersPage.AddNewUser(_userNameNumberThree, _userDisplayNameNumberThree, "user10@mail.com", _password);

			CreatePageAndSetViewPermission(_pageName, "All Users", "allow");

			AddModule(_pageName, Modules.CommonModulesDescription, "MemberDirectoryModule", "ContentPane");

			_logContent = LogContent();
		}

		[TestFixtureTearDown] 
		public void Cleanup()
		{
			VerifyLogs(_logContent);

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.DeleteUser(_userNameNumberOne);
			manageUsersPage.DeleteUser(_userNameNumberTwo);
			manageUsersPage.DeleteUser(_userNameNumberThree);
			manageUsersPage.RemoveDeletedUsers();

			var page = new BlankPage(_driver);
			page.OpenUsingUrl(_baseUrl, _pageName);
			page.DeletePage(_pageName);

			var adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);
			adminRecycleBinPage.EmptyRecycleBin();
		}

		[Test]
		public void Test001_AddAsFriend()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Send Friend Request'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new MemberDirectoryModule(_driver);
			module.AddAsFriend(_userDisplayNameNumberTwo);

			Trace.WriteLine("ASSERT 'Add Friend link changed to 'Pending Request' link'");
			Assert.IsTrue(module.ElementPresent(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + _userDisplayNameNumberTwo + "')]]/ul/li[@class = 'mdFriendPending']")));

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenFriendsLink(_baseUrl);

			userAccountPage.WaitForElement(By.XPath(MemberDirectoryModule.FriendsNotFoundMessage)).Info();

			Trace.WriteLine("ASSERT 'Friends are not found' message is present :");
			Assert.That(userAccountPage.FindElement(By.XPath(MemberDirectoryModule.FriendsNotFoundMessage)).Text,
						Is.EqualTo(MemberDirectoryModule.FriendsNotFoundMessageText),
						"Info message is not found");

			loginPage.LoginUsingDirectUrl(_baseUrl,_userNameNumberTwo, _password);

			Trace.WriteLine("ASSERT Notification icon contains number '1' for incoming request :");
			Assert.That(loginPage.WaitForElement(By.XPath(ControlPanelIDs.NotificationLink + "/span")).Text, Is.EqualTo("1"));

		}

		[Test]
		public void Test002_AcceptFriendRequest()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Accept Friend Request'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberTwo, _password);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new MemberDirectoryModule(_driver);
			module.AcceptFriendRequest(_userDisplayNameNumberThree);

			Trace.WriteLine("ASSERT 'Pending Request' link changed to 'Remove Friend' link'");
			Assert.IsTrue(module.ElementPresent(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + _userDisplayNameNumberThree + "')]]/ul/li[@class = 'mdFriendRemove']")));

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenFriendsLink(_baseUrl);

			Trace.WriteLine("ASSERT New friend is listed:");
			Assert.IsTrue(userAccountPage.ElementPresent(By.XPath("//div[not(@style)]/div/ul[@class = 'MdMemberInfo']//span[contains(text(), '" + _userDisplayNameNumberThree + "')]")), "New Friend is not found");

			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			userAccountPage.OpenFriendsLink(_baseUrl);

			Trace.WriteLine("ASSERT New friend is listed:");
			Assert.IsTrue(userAccountPage.ElementPresent(By.XPath("//div[not(@style)]/div/ul[@class = 'MdMemberInfo']//span[contains(text(), '" + _userDisplayNameNumberTwo + "')]")), "New Friend is not found");
		}

		[Test]
		public void Test003_RemoveFriend()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove Friend'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberTwo, _password);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new MemberDirectoryModule(_driver);
			module.RemoveFriend(_userDisplayNameNumberThree);

			Trace.WriteLine("ASSERT 'Remove Friend' link changed to 'Add Friend' link'");
			Assert.IsTrue(module.ElementPresent(By.XPath("//div[not(@style)]/div[ul[@class = 'MdMemberInfo']//span[contains(text(), '" + _userDisplayNameNumberTwo + "')]]/ul/li[@class = 'mdFriendRequest']")));

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenFriendsLink(_baseUrl);

			userAccountPage.WaitForElement(By.XPath(MemberDirectoryModule.FriendsNotFoundMessage)).Info();

			Trace.WriteLine("ASSERT 'Friends are not found' message is present :");
			Assert.That(userAccountPage.FindElement(By.XPath(MemberDirectoryModule.FriendsNotFoundMessage)).Text,
						Is.EqualTo(MemberDirectoryModule.FriendsNotFoundMessageText),
						"Info message is not found");

			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			userAccountPage.OpenFriendsLink(_baseUrl);

			userAccountPage.WaitForElement(By.XPath(MemberDirectoryModule.FriendsNotFoundMessage)).Info();

			Trace.WriteLine("ASSERT 'Friends are not found' message is present :");
			Assert.That(userAccountPage.FindElement(By.XPath(MemberDirectoryModule.FriendsNotFoundMessage)).Text,
						Is.EqualTo(MemberDirectoryModule.FriendsNotFoundMessageText),
						"Info message is not found");
		}

		[Test]
		public void Test004_HostAccessToMemberDirectory()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Host Access To Member Directory'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new MemberDirectoryModule(_driver);
			module.FindElement(By.XPath("//ul[@id = 'mdMemberList']/li[last()]")).Info();

			Trace.WriteLine("ASSERT the number of Users are correct :");
			Assert.That(module.FindElements(By.XPath("//ul[@id = 'mdMemberList']/li")).Count, Is.EqualTo(4), "The number of Users is incorrect");

			Trace.WriteLine("ASSERT the link visibility is correct :");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[1]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are displayed in User #1 Info");
			Assert.IsTrue(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[2]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are not displayed in User #2 Info");
			Assert.IsTrue(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[3]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are not displayed in User #3 Info");
			Assert.IsTrue(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[4]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are not displayed in User #4 Info");
			
		}

		[Test]
		public void Test005_RegularUserAccessToMemberDirectory()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Authorized User Access To Member Directory'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new MemberDirectoryModule(_driver);
			module.FindElement(By.XPath("//ul[@id = 'mdMemberList']/li[last()]")).Info();

			Trace.WriteLine("ASSERT the number of Users are correct :");
			Assert.That(module.FindElements(By.XPath("//ul[@id = 'mdMemberList']/li")).Count, Is.EqualTo(4), "The number of Users is incorrect");

			Trace.WriteLine("ASSERT the link visibility is correct :");
			Assert.IsTrue(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[1]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
					 "The links are not displayed in User #1 Info");
			Assert.IsTrue(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[2]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are not displayed in User #2 Info");
			Assert.IsTrue(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[3]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are not displayed in User #3 Info");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[4]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are displayed in User #4 Info");
		}

		[Test]
		public void Test006_AnonymousAccessToMemberDirectory()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Anonymous User Access To Member Directory'");

			var loginPage = new LoginPage(_driver);
			loginPage.LetMeOut();

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new MemberDirectoryModule(_driver);
			module.FindElement(By.XPath("//ul[@id = 'mdMemberList']/li[last()]")).Info();

			Trace.WriteLine("ASSERT the number of Users are correct :");
			Assert.That(module.FindElements(By.XPath("//ul[@id = 'mdMemberList']/li")).Count, Is.EqualTo(4), "The number of Users is incorrect");

			Trace.WriteLine("ASSERT the link visibility is correct :");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[1]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
					 "The links are displayed in User #1 Info");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[2]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are displayed in User #2 Info");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[3]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are displayed in User #3 Info");
			Assert.IsFalse(module.ElementPresent(By.XPath("//ul[@id = 'mdMemberList']/li[4]//div[not(@style)]/div[ul[@class = 'mdHoverActions' and not(@style)]]")),
								 "The links are displayed in User #4 Info");
		}

	}
}
