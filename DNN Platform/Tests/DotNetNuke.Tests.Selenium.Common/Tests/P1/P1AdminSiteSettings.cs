using System.Diagnostics;
using System.Drawing;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1AdminSiteSettings : CommonTestSteps
	{
		private string _userWithPublicRegistration;
		private string _userWithVerifiedRegistration;
		private string _userWithPrivateRegistration;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("siteSettings");

			string testName = testSettings.Attribute("name").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();

			_userWithPublicRegistration = testSettings.Attribute("userWithPublicRegistration").Value;
			_userWithVerifiedRegistration = testSettings.Attribute("userWithVerifiedRegistration").Value;
			_userWithPrivateRegistration = testSettings.Attribute("userWithPrivateRegistration").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		[Test]
		public void Test001_VerifyNoneRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'None' registration option'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.NoneRadioButton);

			loginPage.LetMeOut();
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' link is NOT present on the screen");
			Assert.IsFalse(loginPage.ElementPresent(By.XPath(ControlPanelIDs.RegisterLink)),
			               "The Register link is present on the screen");

			loginPage.OpenUsingUrl(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' button is NOT present on the screen");
			Assert.IsFalse(loginPage.ElementPresent(By.XPath(LoginPage.RegisterFrameButton)),
			               "The Register button is present on the screen");
		}

		[Test]
		public void Test002_VerifyPrivateRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'Private' registration option'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.PrivateRadioButton);

			loginPage.RegisterUser(_userWithPrivateRegistration, "DisplayName", "User@Email.com", "www3434");

			loginPage.LoginUsingLoginLink(_userWithPrivateRegistration, "www3434");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Warning message is present");
			Assert.IsTrue(loginPage.ElementPresent(By.XPath(LoginPage.NotAuthorizedWarningMessage)),
			              "The Warning message is not present");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Warning message text is correct");
			Assert.That(loginPage.WaitForElement(By.XPath(LoginPage.NotAuthorizedWarningMessage)).Text,
			            Is.EqualTo(LoginPage.NotAuthorizedWarningMessageText),
			            "The Warning message text is not correct");

			loginPage.LoginAsHost(_baseUrl);

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is NOT authorized");
			Assert.IsFalse(
				manageUsersPage.ElementPresent(
					By.XPath("//tr[td[text() = '" + _userWithPrivateRegistration + "']]/td/img[contains(@id, '_imgApproved')]")),
				"The User is authorized");
		}

		[Test]
		public void Test003_VerifyPublicRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'Public' registration option'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.PublicRadioButton);

			loginPage.RegisterUser(_userWithPublicRegistration, "DisplayName", "User@Email.com", "www3434");

			var manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);
			manageUserProfilePage.OpenTab(By.XPath(ManageUserProfilePage.ManageAccountTab));
			manageUserProfilePage.AccordionOpen(By.XPath(ManageUserProfilePage.AccountSettings));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User can Edit Profile");
			Assert.IsTrue(manageUserProfilePage.FindElement(By.XPath(ManageUserProfilePage.DisplayName)).Enabled,
			              "Display Name textbox is disabled");

			loginPage.LoginAsHost(_baseUrl);

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is authorized");
			Assert.IsTrue(
				manageUsersPage.ElementPresent(
					By.XPath("//tr[td[text() = '" + _userWithPublicRegistration + "']]/td/img[contains(@id, '_imgApproved')]")),
				"The User is not authorized");
		}

		[Test]
		public void Test004_VerifyVerifiedRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'Verified' registration option'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.VerifiedRadioButton);

			loginPage.RegisterUser(_userWithVerifiedRegistration, "DisplayName", "User@Email.com", "www3434");

			var manageUserProfilePage = new ManageUserProfilePage(_driver);
			manageUserProfilePage.OpenUsingLink(_baseUrl);
			manageUserProfilePage.OpenTab(By.XPath(ManageUserProfilePage.ManageAccountTab));
			manageUserProfilePage.AccordionOpen(By.XPath(ManageUserProfilePage.AccountSettings));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User can Edit Profile");
			Assert.IsTrue(manageUserProfilePage.FindElement(By.XPath(ManageUserProfilePage.DisplayName)).Enabled,
			              "Display Name textbox is disabled");

			loginPage.LoginAsHost(_baseUrl);

			var manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the User is authorized");
			Assert.IsFalse(
				manageUsersPage.ElementPresent(
					By.XPath("//tr[td[text() = '" + _userWithVerifiedRegistration + "']]/td/img[contains(@id, '_imgApproved')]")),
				"The User is authorized");
		}
	}
}