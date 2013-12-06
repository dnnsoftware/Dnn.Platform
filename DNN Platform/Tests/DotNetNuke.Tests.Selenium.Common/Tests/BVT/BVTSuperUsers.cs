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
	public abstract class BVTSuperUsers : CommonTestSteps
	{
		public string _superUserName;
		public string _superUserDisplayName;
		public string _superUserEmail;
		public string _superUserPassword;
		public string _phoneNumber;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("superUsers");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;
			_superUserName = testSettings.Attribute("superUserName").Value;
			_superUserDisplayName = testSettings.Attribute("superUserDisplayName").Value;

			_superUserEmail = testSettings.Attribute("superUserEmail").Value;
			_superUserPassword = testSettings.Attribute("superUserPassword").Value;

			_phoneNumber = testSettings.Attribute("phoneNumber").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		[Test]
		public void Test001_AddSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Super User'");

			var hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);
			hostSuperUserAccountsPage.WaitForElement(By.XPath(HostSuperUserAccountsPage.SuperUsersTable));
			int itemNumber = hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count;

			hostSuperUserAccountsPage.AddNewUser(_superUserName, _superUserDisplayName, _superUserEmail, _superUserPassword);

			hostSuperUserAccountsPage.WaitForElement(By.XPath(HostSuperUserAccountsPage.SuperUsersTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The Super user is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the SuperUser is present in the list");
			Assert.IsTrue(
				hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]")),
				"The Super user is not added correctly");
		}

		[Test]
		public void Test002_EditSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Super User'");

			var hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			hostSuperUserAccountsPage.AddPhoneNumber(_superUserName, _phoneNumber);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Phone Number is present in the list");
			Assert.IsTrue(
				hostSuperUserAccountsPage.ElementPresent(
					By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]/td/span[text() = '" + _phoneNumber + "']")),
				"The Phone Number is not added correctly");
		}

		[Test]
		public void Test003_DeleteSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Super User'");

			var hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);
			hostSuperUserAccountsPage.WaitForElement(By.XPath(HostSuperUserAccountsPage.SuperUsersTable));
			int itemNumber = hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count;

			hostSuperUserAccountsPage.DeleteUser(_superUserName);

			hostSuperUserAccountsPage.WaitForElement(By.XPath(HostSuperUserAccountsPage.SuperUsersTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list is not changed");
			Assert.That(hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count,
			            Is.EqualTo(itemNumber),
			            "The Super user is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the SuperUser is present in the list");
			Assert.IsTrue(
				hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]")),
				"The Super user is not deleted correctly");
		}

		[Test]
		public void Test004_RemoveDeletedSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the Super User'");

			var hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);
			hostSuperUserAccountsPage.WaitForElement(By.XPath(HostSuperUserAccountsPage.SuperUsersTable));
			int itemNumber = hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count;

			hostSuperUserAccountsPage.RemoveDeletedUser(_superUserName);

			hostSuperUserAccountsPage.WaitForElement(By.XPath(HostSuperUserAccountsPage.SuperUsersTable));
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count,
			            Is.EqualTo(itemNumber - 1),
			            "The Super user is not removed correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the SuperUser is not present in the list");
			Assert.IsFalse(
				hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]")),
				"The Super user is not removed correctly");
		}
	}
}