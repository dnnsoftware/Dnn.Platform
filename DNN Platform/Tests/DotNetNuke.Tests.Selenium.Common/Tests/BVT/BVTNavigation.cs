using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.BVT
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

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Message Link or Message Link bubble-help is present");
			Utilities.SoftAssert(
				() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(CorePacket.TheInstance.MessageLink)).GetAttribute("title"),
				                        "The Message Link or Message Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Notification Link or Notification Link bubble-help is present");
			Utilities.SoftAssert(
				() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(ControlPanelIDs.NotificationLink)).GetAttribute("title"),
				                        "The Notification Link or Notification Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage +
			                "ASSERT The Registered User Link or Registered User Link bubble-help is present");
			Utilities.SoftAssert(
				() => Assert.IsNotEmpty(currentPage.FindElement(By.XPath(ControlPanelIDs.RegisterLink)).GetAttribute("title"),
				                        "The Registered User Link or Registered User Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The User Avatar or User Avatar bubble-help is present");
			Utilities.SoftAssert(
				() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(ControlPanelIDs.UserAvatar)).GetAttribute("title"),
				                        "The User Avatar or User Avatar bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Logout Link or Logout Link bubble-help is present");
			Utilities.SoftAssert(
				() => Assert.IsNotEmpty(currentPage.FindElement(By.XPath(ControlPanelIDs.LogoutLink)).GetAttribute("title"),
				                        "The Logout Link or Logout Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Box is present");
			Assert.IsTrue(currentPage.ElementPresent(By.XPath(ControlPanelIDs.SearchBox)),
			              "The Search Box is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Button is present");
			Assert.IsTrue(currentPage.ElementPresent(By.XPath(ControlPanelIDs.SearchButton)),
			              "The Search Button is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Copyright notice is present");
			Utilities.SoftAssert(
				() => StringAssert.Contains(ControlPanelIDs.CopyrightText, currentPage.FindElement(By.Id(ControlPanelIDs.CopyrightNotice)).Text,
				                            "Copyright notice is not present or contains wrong text message"));
		}

		[TestCase("Common", "CorePages.AdminAdvancedSettingsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminAdvancedSettingsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminAdvancedSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminDevicePreviewManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminDevicePreviewManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminDevicePreviewManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminExtensionsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminExtensionsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminExtensionsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminFileManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminFileManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminFileManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminLanguagesPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminLanguagesPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminLanguagesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminListsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminListsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminListsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminNewslettersPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminNewslettersPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminNewslettersPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminPageManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminPageManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminPageManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminRecycleBinPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminRecycleBinPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminRecycleBinPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSearchAdminPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSearchAdminPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSearchAdminPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSearchEnginePage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSearchEnginePage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSearchEnginePage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSecurityRolesPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSecurityRolesPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSecurityRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteRedirectionManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSiteRedirectionManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteRedirectionManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteSettingsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSiteSettingsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteWizardPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSiteWizardPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteWizardPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSkinsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminSkinsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSkinsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminTaxonomyPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminTaxonomyPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminTaxonomyPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminUserAccountsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminUserAccountsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminUserAccountsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminVendorsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.AdminVendorsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminVendorsPage", "OpenUsingControlPanel")]
		
		[TestCase("Common", "CorePages.HostConfigurationManagerPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostConfigurationManagerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostConfigurationManagerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostDeviceDetectionManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostDeviceDetectionManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostDeviceDetectionManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostFileManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostFileManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostFileManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSettingsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostSettingsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostHtmlEditorManagerPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostHtmlEditorManagerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostHtmlEditorManagerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostListsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostListsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostListsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSchedulePage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostSchedulePage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSchedulePage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSiteManagementPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostSiteManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSiteManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSqlPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostSqlPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSqlPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSuperUserAccountsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostSuperUserAccountsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSuperUserAccountsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostVendorsPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.HostVendorsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostVendorsPage", "OpenUsingControlPanel")]
		
		[TestCase("Common", "CorePages.FileUploadPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.FileUploadPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.PageImportPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.PageImportPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.PageExportPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.PageExportPage", "OpenUsingControlPanel")]

		[TestCase("Common", "CorePages.ManageRolesPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.ManageRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.UserAccountPage", "OpenUsingUrl")]
		[TestCase("Common", "CorePages.UserAccountPage", "OpenUsingLink")]
		[TestCase("Common", "CorePages.ManageUserProfilePage", "OpenUsingLink")]

		[TestCase("Common", "DemoPages.AboutUsPage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.AboutUsPage", "OpenUsingUrl")]
		[TestCase("Common", "DemoPages.ContactUsPage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.ContactUsPage", "OpenUsingUrl")]
		[TestCase("Common", "DemoPages.HomePage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.HomePage", "OpenUsingUrl")]
		[TestCase("Common", "DemoPages.OurProductsPage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.OurProductsPage", "OpenUsingUrl")]
		//[TestCase("Common", "DemoPages.GettingStartedPage", "OpenUsingControlPanel")]
		//[TestCase("Common", "DemoPages.GettingStartedPage", "OpenUsingUrl")]
		public void NavigationToPage(string assyName, string pageClassName, string openMethod)
		{
			VerifyStandardPageLayout(OpenPage(assyName, pageClassName, openMethod));
		}

		[Test]
		public void NavigationToLoginPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation To Login Page'");

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var loginPage = new LoginPage(_driver);
			loginPage.LetMeOut();
			loginPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Page Title for '" + loginPage.PageTitleLabel + "' page:");
			StringAssert.Contains(loginPage.PageTitleLabel,
			                      loginPage.WaitForElement(By.XPath(ControlPanelIDs.PageTitleID)).Text,
			                      "The wrong page is opened or The title of " + loginPage.PageTitleLabel + " page is changed");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Box is present'");
			Assert.IsTrue(loginPage.ElementPresent(By.XPath(ControlPanelIDs.SearchBox)),
			              "The Search Box is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Button is present'");
			Assert.IsTrue(loginPage.ElementPresent(By.XPath(ControlPanelIDs.SearchButton)),
			              "The Search Button is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Copyright notice is present'");
			Utilities.SoftAssert(
				() => StringAssert.Contains(ControlPanelIDs.CopyrightText, loginPage.FindElement(By.Id(ControlPanelIDs.CopyrightNotice)).Text,
				                            "Copyright notice is not present or contains wrong text message"));

			loginPage.DoLoginUsingLoginLink("host", "dnnhost");
		}

		public void NumberOfLinksOnPage(BasePage currentPage, string featureList, int numberOfLinks)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on page: " +
							numberOfLinks);
			Assert.That(currentPage.FindElements(By.XPath(featureList)).Count,
						Is.EqualTo(numberOfLinks),
						"The number of links on page is not correct");
		}
	}
}