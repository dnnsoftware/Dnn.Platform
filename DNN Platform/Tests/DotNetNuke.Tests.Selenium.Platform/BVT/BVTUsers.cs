using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.BVT
{
	[TestFixture]
	[Category("BVT")]
	public class BVTUsers : Common.Tests.BVT.BVTUsers
	{
		protected override string DataFileLocation
		{
			get { return @"BVT\" + Settings.Default.BVTDataFile; }
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
			loginPage.DoLogin(_registeredUserName, _registeredUserPassword);

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