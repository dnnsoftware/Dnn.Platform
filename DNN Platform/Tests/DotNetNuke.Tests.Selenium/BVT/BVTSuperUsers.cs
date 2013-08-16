using System.Diagnostics;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.BVT
{
	[TestFixture("SuperUserBVT", 
				"SuperUserBVT", 
				"superuser@mail.com", 
				"pAss10word",
				"604-888-1185")]
	[Category("BVT")]
	public class BVTSuperUsers : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _superUserName = null;
		private string _superUserDisplayName = null;
		private string _superUserEmail = null;
		private string _superUserPassword = null;
		private string _phoneNumber = null;

		public BVTSuperUsers(string superUserName, 
						string superUserDisplayName,
						string superUserEmail,
						string superUserPassword,
						string phoneNumber)
		{
			this._superUserName = superUserName;
			this._superUserDisplayName = superUserDisplayName;
			this._superUserEmail = superUserEmail;
			this._superUserPassword = superUserPassword;
			this._phoneNumber = phoneNumber;
		}

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'SuperUsers BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[Test]
		public void Test001_AddSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Super User'");

			HostSuperUserAccountsPage hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			int itemNumber = hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count;

			hostSuperUserAccountsPage.AddNewUser(_superUserName, _superUserDisplayName, _superUserEmail, _superUserPassword);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count, Is.EqualTo(itemNumber + 1),
					"The Super user is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the SuperUser is present in the list");
			Assert.IsTrue(hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]")),
					"The Super user is not added correctly");
		}

		[Test]
		public void Test002_EditSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Super User'");

			HostSuperUserAccountsPage hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			hostSuperUserAccountsPage.AddPhoneNumber(_superUserName, _phoneNumber);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Phone Number is present in the list");
			Assert.IsTrue(hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]/td/span[text() = '" + _phoneNumber  + "']")),
					"The Phone Number is not added correctly");
		}

		[Test]
		public void Test003_DeleteSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Super User'");

			HostSuperUserAccountsPage hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			int itemNumber = hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count;

			hostSuperUserAccountsPage.DeleteUser(_superUserName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list is not changed");
			Assert.That(hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count, Is.EqualTo( itemNumber),
					"The Super user is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the SuperUser is present in the list");
			Assert.IsTrue(hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]")),
					"The Super user is not deleted correctly");
		}

		[Test]
		public void Test004_RemoveDeletedSuperUser()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the Super User'");

			HostSuperUserAccountsPage hostSuperUserAccountsPage = new HostSuperUserAccountsPage(_driver);

			hostSuperUserAccountsPage.OpenUsingButtons(_baseUrl);

			int itemNumber = hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count;

			hostSuperUserAccountsPage.RemoveDeletedUser(_superUserName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(hostSuperUserAccountsPage.FindElements(By.XPath(HostSuperUserAccountsPage.SuperUsersList)).Count, Is.EqualTo(itemNumber - 1),
					"The Super user is not removed correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the SuperUser is not present in the list");
			Assert.IsFalse(hostSuperUserAccountsPage.ElementPresent(By.XPath("//tr[td[contains(text(), '" + _superUserName + "')]]")),
					"The Super user is not removed correctly");
		} 

	}

}
