using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	public abstract class P1Journal: CommonTestSteps
	{
		private string _userNameNumberOne;
		private string _userNameNumberTwo;
		private string _userNameNumberThree;
		private string _userDisplayNameNumberOne;
		private string _userDisplayNameNumberTwo;
		private string _userDisplayNameNumberThree;
		private string _password;
		private string _subject;
		private string _comment;
		private string _fileNameToAttach;
		private string _pictureNameToAttach;
		private string _pageName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("journal");

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
			_comment = testSettings.Attribute("comment").Value;
			_fileNameToAttach = testSettings.Attribute("fileNameToAttach").Value;
			_pictureNameToAttach = testSettings.Attribute("pictureNameToAttach").Value;
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

			AddModule(_pageName, Modules.CommonModulesDescription, "JournalModule", "ContentPane");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberTwo, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.AddNewPostWithVisibilityPermission(_subject + " Community Members", "Community Members");
			module.AddNewPostWithVisibilityPermission(_subject + " Private", "Private");
			module.AddNewPostWithVisibilityPermission(_subject + " Everyone", "Everyone");

			OpenMainPageAndLoginAsHost();
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

			RemoveUsedPage(_pageName);
		}

		[Test]
		public void Test001_AnonymousAccessToJournal()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Anonymous User Access To Jounal'");

			var loginPage = new LoginPage(_driver);
			loginPage.LetMeOut();

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new JournalModule(_driver);
			module.FindElement(By.XPath("//div[@id = 'journalItems']/div[last()]")).Info();

			Trace.WriteLine("ASSERT the number of Messages are correct :");
			Assert.That(module.FindElements(By.XPath("//div[@id = 'journalItems']/div")).Count, Is.EqualTo(1), "The number of Messages are incorrect");

			Trace.WriteLine("ASSERT the visibility is correct :");
			Assert.That(module.WaitForElement(By.XPath("//div[@id = 'journalItems']/div[1]//p[1]")).Text, Is.StringContaining("Everyone"),
					 "The wrong message is shown");
			Assert.IsFalse(module.ElementPresent(By.XPath("//div[@id = 'journalItems']/div[2]//p[1]")),
								 "Visibility permission is not respected for this message");
			Assert.IsFalse(module.ElementPresent(By.XPath("//div[@id = 'journalItems']/div[3]//p[1]")),
								 "Visibility permission is not respected for this message");
		}

		[Test]
		public void Test002_RegularUserAccessToJournal()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Authorized User Access To Journal'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberOne, _password);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new JournalModule(_driver);
			module.FindElement(By.XPath("//div[@id = 'journalItems']/div[last()]")).Info();

			Trace.WriteLine("ASSERT the number of Messages are correct :");
			Assert.That(module.FindElements(By.XPath("//div[@id = 'journalItems']/div")).Count, Is.EqualTo(2), "The number of Messages are incorrect");

			Trace.WriteLine("ASSERT the visibility is correct :");
			Assert.That(module.WaitForElement(By.XPath("//div[@id = 'journalItems']/div[1]//p[1]")).Text, Is.StringContaining("Everyone"),
					 "The wrong message is shown");
			Assert.That(module.WaitForElement(By.XPath("//div[@id = 'journalItems']/div[2]//p[1]")).Text, Is.StringContaining("Community"),
								 "The wrong message is shown");
			Assert.IsFalse(module.ElementPresent(By.XPath("//div[@id = 'journalItems']/div[3]//p[1]")),
						   "Visibility permission is not respected for this message");
		}

		[Test]
		public void Test003_PrivatePostOwnerAccessToJournal()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Authorized User Access To Journal'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberTwo, _password);

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new JournalModule(_driver);
			module.FindElement(By.XPath("//div[@id = 'journalItems']/div[last()]")).Info();

			Trace.WriteLine("ASSERT the number of Messages are correct :");
			Assert.That(module.FindElements(By.XPath("//div[@id = 'journalItems']/div")).Count, Is.EqualTo(3), "The number of Messages are incorrect");

			Trace.WriteLine("ASSERT the visibility is correct :");
			Assert.That(module.WaitForElement(By.XPath("//div[@id = 'journalItems']/div[1]//p[1]")).Text, Is.StringContaining("Everyone"),
					 "The wrong message is shown");
			Assert.That(module.WaitForElement(By.XPath("//div[@id = 'journalItems']/div[2]//p[1]")).Text, Is.StringContaining("Private"),
								 "The wrong message is shown");
			Assert.That(module.WaitForElement(By.XPath("//div[@id = 'journalItems']/div[3]//p[1]")).Text, Is.StringContaining("Community"),
						   "The wrong message is shown");
		}

		[Test]
		public void Test004_UserPostsMessage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can post the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.AddNewPost(_subject);

			Trace.WriteLine("ASSERT the author displayed correctly:");
			Assert.That(module.WaitForElement(By.XPath("//span[@class = 'authorname']/a")).Text,
						Is.EqualTo(_userDisplayNameNumberThree),
						"Message author is not found");

			Trace.WriteLine("ASSERT the message displayed correctly:");
			Assert.That(module.WaitForElement(By.XPath("//div[@class = 'journalitem']/p[1]")).Text,
						Is.StringContaining(_userDisplayNameNumberThree + _subject),
						"Message text is not displayed correctly");
		}

		[Test]
		public void Test005_UserCommentsPost()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can comment the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.CommentPost(_subject, _comment);

			Trace.WriteLine("ASSERT the original message is displayed:");
			Assert.That(module.WaitForElement(By.XPath("//div[@class = 'journalitem']/p[1]")).Text,
						Is.EqualTo(_userDisplayNameNumberThree + _subject),
						"Message text is stiil displayed");

			Trace.WriteLine("ASSERT the comment message is displayed:");
			Assert.That(module.WaitForElement(By.XPath("//div[p[@class = 'journalfooter']]//li/p")).Text,
						Is.StringContaining(_userDisplayNameNumberThree + _comment),
						"Message text is stiil displayed");
		}

		[Test]
		public void Test006_UserDeletesMessage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can delete the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.DeletePost(_subject);

			Trace.WriteLine("ASSERT the posts are NOT displayed:");
			Assert.That(module.FindElements(By.XPath("//div[@id = 'journalItems']/child::*")).Count,
						Is.EqualTo(0),
						"The posts are still displayed");
		}

		[Test]
		public void Test007_UserAttachesFileToPost()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can attach the file to the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.AddNewPostWithAttachedFile(_subject, _fileNameToAttach);

			Trace.WriteLine("ASSERT the file displayed correctly:");
			Assert.IsTrue(module.ElementPresent(By.XPath("//div[@class = 'journalitem']//div/a[contains(text(), '" + _fileNameToAttach + "')]")),
						"File name is not displayed correctly");
		}

		[Test]
		public void Test008_UserAttachesPictureToPost()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can attach the picture to the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.AddNewPostWithAttachedPicture(_subject, _pictureNameToAttach);

			Trace.WriteLine("ASSERT the picture displayed correctly:");
			Assert.IsTrue(module.ElementPresent(By.XPath("//div[@class = 'journalitem']/div/a[contains(@href, '" + _pictureNameToAttach + "')]/img")),
						"Message text is not displayed correctly");
		}

		[Test]
		public void Test009_UserAttachesFileFromWebsiteToPost()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'The authorized User can attach the picture to the message :'");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginUsingDirectUrl(_baseUrl, _userNameNumberThree, _password);

			var userAccountPage = new UserAccountPage(_driver);
			userAccountPage.OpenActivityFeedLink(_baseUrl);

			var module = new JournalModule(_driver);
			module.AddNewPostWithAttachedWebsitePicture(_subject, _pictureNameToAttach);

			Trace.WriteLine("ASSERT the picture displayed correctly:");
			Assert.IsTrue(module.ElementPresent(By.XPath("//div[@id = 'journalItems']/div[1]/div[@class = 'journalitem']/div/img[contains(@src, '" + _pictureNameToAttach + "')]")),
						"Message text is not displayed correctly");
		}

	}
}
