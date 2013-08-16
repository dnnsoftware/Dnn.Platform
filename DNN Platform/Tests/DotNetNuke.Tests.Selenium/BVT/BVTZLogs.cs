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

namespace DotNetNuke.Tests.DNNSelenium.BVT
{
	[TestFixture]
	[Category("BVT")]
	public class BVTZLogs : TestBase
	{
		private IWebDriver _driver = null;
		private LoginPage _loginPage = null;
		private string _baseUrl = null;

		[TestFixtureSetUp]
		public void Login()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Logs BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[TestCase("General Exception")]
		[TestCase("Module Load Exception")]
		[TestCase("Page Load Exception")]
		[TestCase("Scheduler Exception")]
		[TestCase("Scheduler Event Failure")]
		[TestCase("User Locked Out")]
		[TestCase("User Not Approved")]
		[TestCase("HTTP Error Code 404 Page Not Found")]
		[TestCase("Password Sent Failure")]
		[TestCase("Log Notification Failure")]
		[TestCase("Login Failure")]
		public void ExeptionsInLogViewer(string type)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "Verify Log records for '" + type + "'");

			AdminEventViewerPage adminEventViewerPage = new AdminEventViewerPage(_driver);
			adminEventViewerPage.OpenUsingButtons(_baseUrl);

			adminEventViewerPage.FilterByType(type);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT " + AdminEventViewerPage.NoLogItemsMessageText + "' message is present.");
			Assert.IsTrue(adminEventViewerPage.ElementPresent(By.XPath(AdminEventViewerPage.NoLogItemsMessage)),
						  "'" + AdminEventViewerPage.NoLogItemsMessageText + "' message is not found. " + adminEventViewerPage.FindElements(By.XPath("//div[contains(@class, 'dnnlvContent')]/div[contains(@class, 'dnnLogItemHeader')]")).Count() + " " + type +" are present in the list");

		}

		[Category(@"EEpackage")]
		[TestCase("SP List Register Failed")]
		[TestCase("SP List Delete Failed")]
		[TestCase("Submitting of Item Failed")]
		[TestCase("DNN Submissions Analyze Failed")]
		public void ExeptionsInEELogViewer(string type)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "Verify Log records for '" + type + "'");

			AdminEventViewerPage adminEventViewerPage = new AdminEventViewerPage(_driver);
			adminEventViewerPage.OpenUsingButtons(_baseUrl);

			adminEventViewerPage.FilterByType(type);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT " + AdminEventViewerPage.NoLogItemsMessageText + "' message is present.");
			Assert.IsTrue(adminEventViewerPage.ElementPresent(By.XPath(AdminEventViewerPage.NoLogItemsMessage)),
						  "'" + AdminEventViewerPage.NoLogItemsMessageText + "' message is not found. " + adminEventViewerPage.FindElements(By.XPath("//div[contains(@class, 'dnnlvContent')]/div[contains(@class, 'dnnLogItemHeader')]")).Count() + " " + type + " are present in the list");

		}
	}
}
