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
	public abstract class BVTZLogs : CommonTestSteps
	{
		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void Login()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Logs BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();
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

			var adminEventViewerPage = new AdminEventViewerPage(_driver);
			adminEventViewerPage.OpenUsingButtons(_baseUrl);

			adminEventViewerPage.FilterByType(type);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT " + AdminEventViewerPage.NoLogItemsMessageText +
			                "' message is present.");
			Assert.IsTrue(adminEventViewerPage.ElementPresent(By.XPath(AdminEventViewerPage.NoLogItemsMessage)),
			              "'" + AdminEventViewerPage.NoLogItemsMessageText + "' message is not found. " +
			              adminEventViewerPage.FindElements(
				              By.XPath("//div[contains(@class, 'dnnlvContent')]/div[contains(@class, 'dnnLogItemHeader')]")).Count +
			              " " + type + " are present in the list");
		}

	}
}