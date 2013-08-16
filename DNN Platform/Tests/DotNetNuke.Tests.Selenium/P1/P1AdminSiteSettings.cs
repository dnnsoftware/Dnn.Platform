using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
	public class P1AdminSiteSettings : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("page");

			string testName = pageSettings.Attribute("name").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[Test]
		public void Test001_VerifyNoneRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'None' registration option'");

			AdminSiteSettingsPage adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.NoneRadioButton);

			_loginPage.LetMeOut();
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' link is NOT present on the screen");
			Assert.IsFalse(_loginPage.ElementPresent(By.Id(BasePage.RegisteredUserLink)),
					"The Register link is present on the screen");

			_loginPage.OpenUsingUrl(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' button is NOT present on the screen");
			Assert.IsFalse(_loginPage.ElementPresent(By.XPath(LoginPage.RegisterFrameButton)),
					"The Register button is present on the screen");
		}

		[Test]
		public void Test001_VerifyPublicRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'Public' registration option'");

			AdminSiteSettingsPage adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.PublicRadioButton);

			_loginPage.LetMeOut();
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' link is NOT present on the screen");
			Assert.IsFalse(_loginPage.ElementPresent(By.Id(BasePage.RegisteredUserLink)),
					"The Register link is present on the screen");

			_loginPage.OpenUsingUrl(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' button is NOT present on the screen");
			Assert.IsFalse(_loginPage.ElementPresent(By.XPath(LoginPage.RegisterFrameButton)),
					"The Register button is present on the screen");
		}

		[Test]
		public void Test001_VerifyVerifiedRegistration()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify 'Verified' registration option'");

			AdminSiteSettingsPage adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);

			adminSiteSettingsPage.SetUserRegistrationType(AdminSiteSettingsPage.VerifiedRadioButton);

			_loginPage.LetMeOut();
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' link is NOT present on the screen");
			Assert.IsFalse(_loginPage.ElementPresent(By.Id(BasePage.RegisteredUserLink)),
					"The Register link is present on the screen");

			_loginPage.OpenUsingUrl(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the 'Register' button is NOT present on the screen");
			Assert.IsFalse(_loginPage.ElementPresent(By.XPath(LoginPage.RegisterFrameButton)),
					"The Register button is present on the screen");
		}
	}
}
