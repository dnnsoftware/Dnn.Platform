using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1SecurityRoles : CommonTestSteps
	{
		private string _roleNameWithServiceFee;
		private string _serviceFee;
		private string _numberInBillingPeriod;
		private string _billingPeriod;
		private string _publicRoleName;
		private string _pendingRoleName;
		private string _autoAssignedRoleName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("securityRoles");

			string testName = testSettings.Attribute("name").Value;
			_roleNameWithServiceFee = testSettings.Attribute("roleNameWithServiceFee").Value;
			_serviceFee = testSettings.Attribute("serviceFee").Value;
			_billingPeriod = testSettings.Attribute("billingPeriod").Value;
			_numberInBillingPeriod = testSettings.Attribute("numberInBillingPeriod").Value;

			_publicRoleName = testSettings.Attribute("publicRoleName").Value;
			_pendingRoleName = testSettings.Attribute("pendingRoleName").Value;
			_autoAssignedRoleName = testSettings.Attribute("autoAssignedRoleName").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

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
		public void Test001_AddSecurityRoleWithFees()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Security Role with Fees'");

			var manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewSecurityRoleWithFees(_roleNameWithServiceFee, _serviceFee, _numberInBillingPeriod, _billingPeriod);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Service Fee is present in the list");
			Assert.That(
				manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleNameWithServiceFee + "']]/td/span[contains(@id, '_Label1')]")).
					Text, Is.EqualTo(_serviceFee),
				"The service fee is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Billing Period is present in the list");
			Assert.That(
				manageRolesPage.FindElement(By.XPath("//tr[td[text() = '" + _roleNameWithServiceFee + "']]/td/span[contains(@id, '_Label2')]")).
					Text, Is.EqualTo(_numberInBillingPeriod),
				"The Billing period is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Billing Frequency is present in the list");
			Assert.That(
				manageRolesPage.FindElement(
					By.XPath("//tr[td[text() = '" + _roleNameWithServiceFee + "']]/td/span[contains(@id, '_lblBillingFrequency')]")).Text,
				Is.EqualTo(_billingPeriod),
				"The Billing frequency is not added correctly");
		}

		[Test]
		public void Test002_AddPublicSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Public Security Role'");

			var manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewPublicSecurityRole(_publicRoleName);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Public is enabled");
			Assert.IsTrue(
				manageRolesPage.ElementPresent(
					By.XPath("//tr[td[text() = '" + _publicRoleName + "']]/td/img[contains(@id, '_imgApproved')]")),
				"The public role is not enabled");
		}

		[Test]
		public void Test003_AddPendingSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new pending Security Role'");

			var manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewSecurityRoleWithStatus(_pendingRoleName, "Pending");

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Manage Users link is disabled");
			Assert.IsFalse(
				manageRolesPage.ElementPresent(
					By.XPath("//tr[td[text() = '" + _pendingRoleName + "']]/td/a/img[contains(@src, 'Users_16X16_Standard.png')]")),
				"The Manage Users link is enabled");
		}

		[Test]
		public void Test004_AddAutoAssignedSecurityRole()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new auto assigned Security Role'");

			var manageRolesPage = new ManageRolesPage(_driver);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			int itemNumber = manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count;

			manageRolesPage.AddNewAutoAssignedSecurityRole(_autoAssignedRoleName);

			manageRolesPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(manageRolesPage.FindElements(By.XPath(ManageRolesPage.SecurityRolesList)).Count,
			            Is.EqualTo(itemNumber + 1),
			            "The security role is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Auto checkbox is enabled");
			Assert.IsTrue(
				manageRolesPage.ElementPresent(
					By.XPath("//tr[td[text() = '" + _autoAssignedRoleName + "']]/td/img[contains(@id, '_Image1')]")),
				"The Auto checkbox link is disabled");
		}
	}
}