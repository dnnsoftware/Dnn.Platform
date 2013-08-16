using System.Diagnostics;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.P1
{
	[TestFixture("Security Role with Fee P1", 
				"45.00",
				"5",
				"Day(s)")]
	[Category("P1")]
	class P1SecurityRoles : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _roleName = null;
		private string _serviceFee = null;
		private string _numberInBillingPeriod = null;
		private string _billingPeriod = null;
		private string _publicRoleName = "Public Security Role P1";
		private string _pendingRoleName = "Pending Security Role P1";
		private string _autoAssignedRoleName = "Auto Assigned Security Role P1";

		public P1SecurityRoles(string roleName, 
							string serviceFee,
							string numberInBillingPeriod,
							string billingPeriod)
		{
			this._roleName = roleName;
			this._serviceFee = serviceFee;
			this._numberInBillingPeriod = numberInBillingPeriod;
			this._billingPeriod = billingPeriod;
		}

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Security Roles P1'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[Test]
		public void Test001_AddSecurityRoleWithFees()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Security Role with Fees'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewSecurityRoleWithFees(_roleName, _serviceFee, _numberInBillingPeriod, _billingPeriod);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count, Is.EqualTo(itemNumber + 1),
						"The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Service Fee is present in the list");
			Assert.That(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleName + "']]/td/span[contains(@id, '_Label1')]")).Text, Is.EqualTo(_serviceFee),
						"The service fee is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Billing Period is present in the list");
			Assert.That(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleName + "']]/td/span[contains(@id, '_Label2')]")).Text, Is.EqualTo(_numberInBillingPeriod),
						"The Billing period is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Billing Frequency is present in the list");
			Assert.That(manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleName + "']]/td/span[contains(@id, '_lblBillingFrequency')]")).Text, Is.EqualTo(_billingPeriod),
						"The Billing frequency is not added correctly");
		}

		[Test]
		public void Test002_AddPublicSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Public Security Role'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewPublicSecurityRole(_publicRoleName);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count, Is.EqualTo(itemNumber + 1),
						"The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Public is enabled");
			Assert.IsTrue(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _publicRoleName + "']]/td/img[contains(@id, '_imgApproved')]")),
						"The public role is not enabled");
		}

		[Test]
		public void Test003_AddPendingSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new pending Security Role'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewSecurityRoleWithStatus(_pendingRoleName, "Pending");

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count, Is.EqualTo(itemNumber + 1),
						"The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Manage Users link is disabled");
			Assert.IsFalse(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _pendingRoleName + "']]/td/a/img[contains(@src, 'Users_16X16_Standard.png')]")),
						"The Manage Users link is enabled");

		}

		[Test]
		public void Test004_AddAutoAssignedSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new auto assigned Security Role'");

			ManageRolesPage manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewAutoAssignedSecurityRole(_autoAssignedRoleName);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count, Is.EqualTo(itemNumber + 1),
						"The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Auto checkbox is enabled");
			Assert.IsTrue(manageRolesPage.ElementPresent(By.XPath("//tr[td[text() = '" + _autoAssignedRoleName + "']]/td/img[contains(@id, '_Image1')]")),
						"The Auto checkbox link is disabled");

		}
	}
}
