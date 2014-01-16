using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
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
		}

		[SetUp]
		public void RunBeforeEachTest()
		{
			Trace.WriteLine("Run before each test");
			_logContent = LogContent();
		}

		[TearDown]
		public void CleanupAfterEachTest()
		{
			Trace.WriteLine("Run after each test");
			VerifyLogs(_logContent);
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
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
				() => Assert.IsNotEmpty(currentPage.FindElement(By.XPath(ControlPanelIDs.MessageLink)).GetAttribute("title"),
				                        "The Message Link or Message Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Notification Link or Notification Link bubble-help is present");
			Utilities.SoftAssert(
				() => Assert.IsNotEmpty(currentPage.FindElement(By.XPath(ControlPanelIDs.NotificationLink)).GetAttribute("title"),
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

		[TestCase("Common", "CorePages.AdminAdvancedSettingsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminDevicePreviewManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminExtensionsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminFileManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminLanguagesPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminListsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminNewslettersPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminPageManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminRecycleBinPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSearchAdminPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSearchEnginePage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSecurityRolesPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteRedirectionManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteSettingsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSiteWizardPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminSkinsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminTaxonomyPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminUserAccountsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.AdminVendorsPage", "OpenUsingButtons")]
		
		[TestCase("Common", "CorePages.HostConfigurationManagerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostDeviceDetectionManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostFileManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSettingsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostHtmlEditorManagerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostListsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSchedulePage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSiteManagementPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSqlPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostSuperUserAccountsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostVendorsPage", "OpenUsingButtons")]
		
		[TestCase("Common", "CorePages.FileUploadPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.PageImportPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.PageExportPage", "OpenUsingControlPanel")]

		[TestCase("Common", "CorePages.ManageRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.UserAccountPage", "OpenUsingLink")]
		[TestCase("Common", "CorePages.ManageUserProfilePage", "OpenUsingLink")]

		[TestCase("Common", "DemoPages.AboutUsPage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.ContactUsPage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.HomePage", "OpenUsingLink")]
		[TestCase("Common", "DemoPages.OurProductsPage", "OpenUsingLink")]
		//[TestCase("Common", "DemoPages.GettingStartedPage", "OpenUsingControlPanel")]
		//[TestCase("Common", "DemoPages.GettingStartedPage", "OpenUsingUrl")]
		public void NavigationToPage(string assyName, string pageClassName, string openMethod)
		{
			VerifyStandardPageLayout(OpenPage(assyName, pageClassName, openMethod));
		}

		//[Test]
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
		}

		public void NumberOfLinksOnPage(BasePage currentPage, string featureList, int numberOfLinks)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on page: " +
							numberOfLinks);

			currentPage.WaitForElement(By.XPath(featureList + "/div[last()]"));
			Assert.That(currentPage.FindElements(By.XPath(featureList)).Count,
						Is.EqualTo(numberOfLinks),
						"The number of links on page is not correct");
		}
	}
}