using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.Upgrade
{
	[TestFixture]
	public abstract class BVTNavigation : CommonTestSteps
	{
		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void Login()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		public void VerifyStandardPageLayout(BasePage currentPage)
		{
			StringAssert.Contains(currentPage.PageTitleLabel.ToUpper(),
								  currentPage.WaitForElement(By.XPath(ControlPanelIDs.PageTitleID)).
									  Text.ToUpper(),
								  "The wrong page is opened");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the error message is not present");
			Assert.IsFalse(
				currentPage.ElementPresent(
					By.XPath(
						"//div[contains(@id, 'dnnSkinMessage') and contains(@class, 'dnnFormValidationSummary')]")),
				"The error message is present on the current page");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the error popup is not present");
			Assert.IsFalse(
				currentPage.ElementPresent(
					By.XPath(
						"//div[contains(@class, 'dnnFormPopup') and contains(@style, 'display: block;')]//span[contains(text(), 'Error')]")),
				"The error popup is present on the current page");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Copyright notice is present");
			Utilities.SoftAssert(
				() => StringAssert.Contains(ControlPanelIDs.CopyrightText, currentPage.FindElement(By.Id(ControlPanelIDs.CopyrightNotice)).Text,
											"Copyright notice is not present or contains wrong text message"));
		}

		[TestCase("Common", "CorePages.AdminAdvancedSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminDevicePreviewManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminExtensionsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminFileManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminLanguagesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminListsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminNewslettersPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminPageManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminRecycleBinPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSearchAdminPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSearchEnginePage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSecurityRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteRedirectionManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteWizardPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSkinsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminTaxonomyPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminUserAccountsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminVendorsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostConfigurationManagerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostDeviceDetectionManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostFileManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostHtmlEditorManagerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostListsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSchedulePage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSiteManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSqlPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSuperUserAccountsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostVendorsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.PageImportPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.PageExportPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.UserAccountPage", "OpenUsingLink")]
		[TestCase("Common", "CorePages.ManageUserProfilePage", "OpenUsingLink")]
		public void NavigationToPage(string assyName, string pageClassName, string openMethod)
		{
			VerifyStandardPageLayout(OpenPage(assyName, pageClassName, openMethod));
		}
	}
}