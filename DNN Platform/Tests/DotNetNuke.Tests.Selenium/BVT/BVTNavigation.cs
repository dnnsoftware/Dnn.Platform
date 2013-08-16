using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml;
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
	public class BVTNavigation : TestBase
	{
		IWebDriver _driver = null;
		private LoginPage _loginPage = null;
		private string _baseUrl = null;

		[TestFixtureSetUp]
		public void Login()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost"); 
		}

		private void VerifyStandardPageLayout(BasePage currentPage)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the error message is not present");
			Assert.IsFalse(currentPage.ElementPresent(By.XPath("//div[contains(@id, 'UPPanel')]//div[contains(@id, 'dnnSkinMessage') and contains(@class, 'dnnFormValidationSummary')]")),
						"The error message is present on the current page");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Message Link or Message Link bubble-help is present");
			Utilities.SoftAssert(() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(CorePacket.TheInstance.MessageLink)).GetAttribute("title"), 
				"The Message Link or Message Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Notification Link or Notification Link bubble-help is present");
			Utilities.SoftAssert(() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(BasePage.NotificationLink)).GetAttribute("title"), 
				"The Notification Link or Notification Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Registered User Link or Registered User Link bubble-help is present");
			Utilities.SoftAssert(() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(BasePage.RegisteredUserLink)).GetAttribute("title"), 
				"The Registered User Link or Registered User Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The User Avatar or User Avatar bubble-help is present");
			Utilities.SoftAssert(() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(BasePage.UserAvatar)).GetAttribute("title"), 
				"The User Avatar or User Avatar bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Logout Link or Logout Link bubble-help is present");
			Utilities.SoftAssert(() => Assert.IsNotEmpty(currentPage.FindElement(By.Id(BasePage.LogoutLink)).GetAttribute("title"), 
				"The Logout Link or Logout Link bubble-help is missing."));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Box is present");
			Assert.IsTrue(currentPage.ElementPresent(By.XPath(BasePage.SearchBox)),
						"The Search Box is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Button is present");
			Assert.IsTrue(currentPage.ElementPresent(By.XPath(BasePage.SearchButton)),
						"The Search Button is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Copyright notice is present");
			Utilities.SoftAssert(() => StringAssert.Contains(BasePage.CopyrightText, currentPage.FindElement(By.Id(BasePage.CopyrightNotice)).Text,
				"Copyright notice is not present or contains wrong text message"));
		}

		private void DoNavigateToOutsidePage(string pageClassName, string openMethod)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation To " + pageClassName + "'");

			Type pageClassType = Type.GetType("DotNetNuke.Tests.DNNSelenium.CorePages." + pageClassName);
			if (pageClassType != null)
			{
				var navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

				HostPage hostPage = new HostPage(_driver);
				Type myType = hostPage.GetType();

				var miOpen = myType.GetMethod(openMethod);
				if (miOpen != null)
				{
					miOpen.Invoke(hostPage, new object[] { _baseUrl });
				}
				else
				{
					Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
				}

				var fiWindowTitle = pageClassType.GetField("WindowTitle");
				var windowTitle = (string)fiWindowTitle.GetValue(null);

				var fiOutsideUrl = pageClassType.GetField("OutsideUrl");
				var outsideUrl = (string)fiOutsideUrl.GetValue(null);

				var basePage = (BaseOutsidePage)navToPage;

				Utilities.SoftAssert(
				() => Assert.That(basePage.CurrentWindowTitle(), Is.EqualTo(windowTitle), "Current window Title is not correct."));

				Assert.That(basePage.CurrentWindowUrl(), Is.EqualTo(outsideUrl), "Current window URL is not correct.");
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot create class " + pageClassName);
			}
		}

		private void DoNavigateToPage(string pageClassName, string openMethod)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation To " + pageClassName + "'");

			Type pageClassType = Type.GetType("DotNetNuke.Tests.DNNSelenium.CorePages." + pageClassName);
			if (pageClassType != null)
			{
				var navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

				var miOpen = pageClassType.GetMethod(openMethod);
				if (miOpen != null)
				{
					miOpen.Invoke(navToPage, new object[] { _baseUrl });
				}
				else
				{
					Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
				}

				var fiPageTitleLabel = pageClassType.GetField("PageTitleLabel");
				var pageTitleLabel = (string)fiPageTitleLabel.GetValue(null);

				var basePage = (BasePage)navToPage;
				StringAssert.Contains(pageTitleLabel.ToUpper(), basePage.WaitForElement(By.XPath("//span[contains(@id, '" + BasePage.PageTitle + "')]")).Text.ToUpper(),
						"The wrong page is opened or The title of " + pageTitleLabel + " page is changed");

				VerifyStandardPageLayout(basePage);
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot create class " + pageClassName);
			}
		}

		[Test]
		public void NavigationToLoginPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation To Login Page'");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LetMeOut();
			loginPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Page Title for '" + LoginPage.PageTitleLabel + "' page:");
			StringAssert.Contains(LoginPage.PageTitleLabel, loginPage.WaitForElement(By.XPath("//span[contains(@id, '" + BasePage.PageTitle + "')]")).Text,
				"The wrong page is opened or The title of " + LoginPage.PageTitleLabel + " page is changed");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Box is present'");
			Assert.IsTrue(loginPage.ElementPresent(By.XPath(BasePage.SearchBox)),
						"The Search Box is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT The Search Button is present'");
			Assert.IsTrue(loginPage.ElementPresent(By.XPath(BasePage.SearchButton)),
						"The Search Button is missing.");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Copyright notice is present'");
			Utilities.SoftAssert(() => StringAssert.Contains(BasePage.CopyrightText, loginPage.FindElement(By.Id(BasePage.CopyrightNotice)).Text,
				"Copyright notice is not present or contains wrong text message"));

			loginPage.DoLoginUsingLoginLink("host", "dnnhost");
		}

		[Test]
		public void NavigationToMainPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation To Main Page'");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);
			VerifyStandardPageLayout(mainPage);
		}

		[Test]
		[Category(@"CEpackage")]
		public void NumberOfLinksOnToAdminPageCEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number Of Links on Admin Page'");

			AdminPage adminPage = new AdminPage(_driver);
			adminPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Admin page: " + AdminPage.NumberOfLinksCEPackage);
			Assert.That(adminPage.FindElements(By.XPath(AdminPage.FeaturesList)).Count, Is.EqualTo(AdminPage.NumberOfLinksCEPackage),
						"The number of links on Admin page is not correct");
		}

		[Test]
		[Category(@"PEpackage")]
		public void NumberOfLinksOnToAdminPagePEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number Of Links on Admin Page'");

			AdminPage adminPage = new AdminPage(_driver);
			adminPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Admin page: " + AdminPage.NumberOfLinksPEPackage);
			Assert.That(adminPage.FindElements(By.XPath(AdminPage.FeaturesList)).Count, Is.EqualTo(AdminPage.NumberOfLinksPEPackage),
						"The number of links on Admin page is not correct");
		}

		[Test]
		[Category(@"EEpackage")]
		public void NumberOfLinksOnToAdminPageEEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number Of Links on Admin Page'");

			AdminPage adminPage = new AdminPage(_driver);
			adminPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Admin page: " + AdminPage.NumberOfLinksEEPackage);
			Assert.That(adminPage.FindElements(By.XPath(AdminPage.FeaturesList)).Count, Is.EqualTo(AdminPage.NumberOfLinksEEPackage),
						"The number of links on Admin page is not correct");
		}


		[TestCase("AdminPage", "OpenUsingUrl")]
		[TestCase("AdminPage", "OpenUsingButtons")]

		[TestCase("AdminAdvancedSettingsPage", "OpenUsingUrl")]
		[TestCase("AdminAdvancedSettingsPage", "OpenUsingButtons")]
		[TestCase("AdminAdvancedSettingsPage", "OpenUsingControlPanel")]

		[TestCase("AdminDevicePreviewManagementPage", "OpenUsingUrl")]
		[TestCase("AdminDevicePreviewManagementPage", "OpenUsingButtons")]
		[TestCase("AdminDevicePreviewManagementPage", "OpenUsingControlPanel")]

		[TestCase("AdminEventViewerPage", "OpenUsingUrl")]
		[TestCase("AdminEventViewerPage", "OpenUsingButtons")]
		[TestCase("AdminEventViewerPage", "OpenUsingControlPanel")]

		[TestCase("AdminExtensionsPage", "OpenUsingUrl")]
		[TestCase("AdminExtensionsPage", "OpenUsingButtons")]
		[TestCase("AdminExtensionsPage", "OpenUsingControlPanel")]

		[TestCase("AdminFileManagementPage", "OpenUsingUrl")]
		[TestCase("AdminFileManagementPage", "OpenUsingButtons")]
		[TestCase("AdminFileManagementPage", "OpenUsingControlPanel")]

		[TestCase("AdminLanguagesPage", "OpenUsingUrl")]
		[TestCase("AdminLanguagesPage", "OpenUsingButtons")]
		[TestCase("AdminLanguagesPage", "OpenUsingControlPanel")]

		[TestCase("AdminListsPage", "OpenUsingUrl")]
		[TestCase("AdminListsPage", "OpenUsingButtons")]
		[TestCase("AdminListsPage", "OpenUsingControlPanel")]

		[TestCase("AdminNewslettersPage", "OpenUsingUrl")]
		[TestCase("AdminNewslettersPage", "OpenUsingButtons")]
		[TestCase("AdminNewslettersPage", "OpenUsingControlPanel")]

		[TestCase("AdminPageManagementPage", "OpenUsingUrl")]
		[TestCase("AdminPageManagementPage", "OpenUsingButtons")]
		[TestCase("AdminPageManagementPage", "OpenUsingControlPanel")]

		[TestCase("AdminRecycleBinPage", "OpenUsingUrl")]
		[TestCase("AdminRecycleBinPage", "OpenUsingButtons")]
		[TestCase("AdminRecycleBinPage", "OpenUsingControlPanel")]

		[TestCase("AdminSearchAdminPage", "OpenUsingUrl")]
		[TestCase("AdminSearchAdminPage", "OpenUsingButtons")]
		[TestCase("AdminSearchAdminPage", "OpenUsingControlPanel")]

		[TestCase("AdminSearchEnginePage", "OpenUsingUrl")]
		[TestCase("AdminSearchEnginePage", "OpenUsingButtons")]
		[TestCase("AdminSearchEnginePage", "OpenUsingControlPanel")]

		[TestCase("AdminSecurityRolesPage", "OpenUsingUrl")]
		[TestCase("AdminSecurityRolesPage", "OpenUsingButtons")]
		[TestCase("AdminSecurityRolesPage", "OpenUsingControlPanel")]

		[TestCase("AdminSiteLogPage", "OpenUsingUrl")]
		[TestCase("AdminSiteLogPage", "OpenUsingButtons")]
		[TestCase("AdminSiteLogPage", "OpenUsingControlPanel")]

		[TestCase("AdminSiteRedirectionManagementPage", "OpenUsingUrl")]
		[TestCase("AdminSiteRedirectionManagementPage", "OpenUsingButtons")]
		[TestCase("AdminSiteRedirectionManagementPage", "OpenUsingControlPanel")]

		[TestCase("AdminSiteSettingsPage", "OpenUsingUrl")]
		[TestCase("AdminSiteSettingsPage", "OpenUsingButtons")]
		[TestCase("AdminSiteSettingsPage", "OpenUsingControlPanel")]

		[TestCase("AdminSiteWizardPage", "OpenUsingUrl")]
		[TestCase("AdminSiteWizardPage", "OpenUsingButtons")]
		[TestCase("AdminSiteWizardPage", "OpenUsingControlPanel")]

		[TestCase("AdminSkinsPage", "OpenUsingUrl")]
		[TestCase("AdminSkinsPage", "OpenUsingButtons")]
		[TestCase("AdminSkinsPage", "OpenUsingControlPanel")]

		[TestCase("AdminTaxonomyPage", "OpenUsingUrl")]
		[TestCase("AdminTaxonomyPage", "OpenUsingButtons")]
		[TestCase("AdminTaxonomyPage", "OpenUsingControlPanel")]

		[TestCase("AdminUserAccountsPage", "OpenUsingUrl")]
		[TestCase("AdminUserAccountsPage", "OpenUsingButtons")]
		[TestCase("AdminUserAccountsPage", "OpenUsingControlPanel")]

		[TestCase("AdminVendorsPage", "OpenUsingUrl")]
		[TestCase("AdminVendorsPage", "OpenUsingButtons")]
		[TestCase("AdminVendorsPage", "OpenUsingControlPanel")]

		[TestCase("HostPage", "OpenUsingUrl")]
		[TestCase("HostPage", "OpenUsingButtons")]

		[TestCase("HostActivateYourLicensePage", "OpenUsingUrl")]
		[TestCase("HostActivateYourLicensePage", "OpenUsingButtons")]
		[TestCase("HostActivateYourLicensePage", "OpenUsingControlPanel")]

		[TestCase("HostAdvancedUrlManagementPage", "OpenUsingUrl")]
		[TestCase("HostAdvancedUrlManagementPage", "OpenUsingButtons")]
		[TestCase("HostAdvancedUrlManagementPage", "OpenUsingControlPanel")]

		[TestCase("HostApplicationIntegrityPage", "OpenUsingUrl")]
		[TestCase("HostApplicationIntegrityPage", "OpenUsingButtons")]
		[TestCase("HostApplicationIntegrityPage", "OpenUsingControlPanel")]

		[TestCase("HostConfigurationManagerPage", "OpenUsingUrl")]
		[TestCase("HostConfigurationManagerPage", "OpenUsingButtons")]
		[TestCase("HostConfigurationManagerPage", "OpenUsingControlPanel")]

		[TestCase("HostDashboardPage", "OpenUsingUrl")]
		[TestCase("HostDashboardPage", "OpenUsingButtons")]
		[TestCase("HostDashboardPage", "OpenUsingControlPanel")]

		[TestCase("HostDeviceDetectionManagementPage", "OpenUsingUrl")]
		[TestCase("HostDeviceDetectionManagementPage", "OpenUsingButtons")]
		[TestCase("HostDeviceDetectionManagementPage", "OpenUsingControlPanel")]

		[TestCase("HostFileManagementPage", "OpenUsingUrl")]
		[TestCase("HostFileManagementPage", "OpenUsingButtons")]
		[TestCase("HostFileManagementPage", "OpenUsingControlPanel")]

		[TestCase("HostExtensionsPage", "OpenUsingUrl")]
		[TestCase("HostExtensionsPage", "OpenUsingButtons")]
		[TestCase("HostExtensionsPage", "OpenUsingControlPanel")]

		[TestCase("HostHealthMonitoringPage", "OpenUsingUrl")]
		[TestCase("HostHealthMonitoringPage", "OpenUsingButtons")]
		[TestCase("HostHealthMonitoringPage", "OpenUsingControlPanel")]

		[TestCase("HostHtmlEditorManagerPage", "OpenUsingUrl")]
		[TestCase("HostHtmlEditorManagerPage", "OpenUsingButtons")]
		[TestCase("HostHtmlEditorManagerPage", "OpenUsingControlPanel")]

		[TestCase("HostManageWebSitesPage", "OpenUsingUrl")]
		[TestCase("HostManageWebSitesPage", "OpenUsingButtons")]
		[TestCase("HostManageWebSitesPage", "OpenUsingControlPanel")]

		[TestCase("HostListsPage", "OpenUsingUrl")]
		[TestCase("HostListsPage", "OpenUsingButtons")]
		[TestCase("HostListsPage", "OpenUsingControlPanel")]

		[TestCase("HostSchedulePage", "OpenUsingUrl")]
		[TestCase("HostSchedulePage", "OpenUsingButtons")]
		[TestCase("HostSchedulePage", "OpenUsingControlPanel")]

		[TestCase("HostSecurityCenterPage", "OpenUsingUrl")]
		[TestCase("HostSecurityCenterPage", "OpenUsingButtons")]
		[TestCase("HostSecurityCenterPage", "OpenUsingControlPanel")]

		[TestCase("HostSettingsPage", "OpenUsingUrl")]
		[TestCase("HostSettingsPage", "OpenUsingButtons")]
		[TestCase("HostSettingsPage", "OpenUsingControlPanel")]

		[TestCase("HostSiteManagementPage", "OpenUsingUrl")]
		[TestCase("HostSiteManagementPage", "OpenUsingButtons")]
		[TestCase("HostSiteManagementPage", "OpenUsingControlPanel")]

		[TestCase("HostSqlPage", "OpenUsingUrl")]
		[TestCase("HostSqlPage", "OpenUsingButtons")]
		[TestCase("HostSqlPage", "OpenUsingControlPanel")]

		[TestCase("HostSuperUserAccountsPage", "OpenUsingUrl")]
		[TestCase("HostSuperUserAccountsPage", "OpenUsingButtons")]
		[TestCase("HostSuperUserAccountsPage", "OpenUsingControlPanel")]

		[TestCase("HostUserSwitcherPage", "OpenUsingUrl")]
		[TestCase("HostUserSwitcherPage", "OpenUsingButtons")]
		[TestCase("HostUserSwitcherPage", "OpenUsingControlPanel")]

		[TestCase("HostVendorsPage", "OpenUsingUrl")]
		[TestCase("HostVendorsPage", "OpenUsingButtons")]
		[TestCase("HostVendorsPage", "OpenUsingControlPanel")]

		[TestCase("HostWhatsNewPage", "OpenUsingUrl")]
		[TestCase("HostWhatsNewPage", "OpenUsingButtons")]
		[TestCase("HostWhatsNewPage", "OpenUsingControlPanel")]

		[TestCase("FileUploadPage", "OpenUsingUrl")]
		[TestCase("FileUploadPage", "OpenUsingControlPanel")]

		[TestCase("PageImportPage", "OpenUsingUrl")]
		[TestCase("PageImportPage", "OpenUsingControlPanel")]

		[TestCase("PageExportPage", "OpenUsingUrl")]
		[TestCase("PageExportPage", "OpenUsingControlPanel")]

		[TestCase("ManageRolesPage", "OpenUsingUrl")]
		[TestCase("ManageRolesPage", "OpenUsingControlPanel")]

		[TestCase("ManageUsersPage", "OpenUsingUrl")]
		[TestCase("ManageUsersPage", "OpenUsingControlPanel")]

		[TestCase("UserAccountPage", "OpenUsingUrl")]
		[TestCase("UserAccountPage", "OpenUsingLink")]

		[TestCase("ManageUserProfilePage", "OpenUsingLink")]
		public void NavigationToPage(string pageClassName, string openMethod)
		{
			DoNavigateToPage (pageClassName, openMethod);
		}

		[Category(@"EEpackage")]
		[TestCase("AdminContentStagingPage", "OpenUsingUrl")]
		[TestCase("AdminContentStagingPage", "OpenUsingButtons")]
		[TestCase("AdminContentStagingPage", "OpenUsingControlPanel")]

		[TestCase("AdminSharePointConnectorPage", "OpenUsingUrl")]
		[TestCase("AdminSharePointConnectorPage", "OpenUsingButtons")]
		[TestCase("AdminSharePointConnectorPage", "OpenUsingControlPanel")]

		[TestCase("HostSharePointConnectorPage", "OpenUsingUrl")]
		[TestCase("HostSharePointConnectorPage", "OpenUsingButtons")]
		[TestCase("HostSharePointConnectorPage", "OpenUsingControlPanel")]
		public void NavigationToPageEEpackage(string pageClassName, string openMethod)
		{
			DoNavigateToPage(pageClassName, openMethod);
		}

		[Category(@"EEandPEpackage")]
		[TestCase("AdminAdvancedUrlManagementPage", "OpenUsingUrl")]
		[TestCase("AdminAdvancedUrlManagementPage", "OpenUsingButtons")]
		[TestCase("AdminAdvancedUrlManagementPage", "OpenUsingControlPanel")]

		[TestCase("AdminGoogleAnalyticsProPage", "OpenUsingUrl")]
		[TestCase("AdminGoogleAnalyticsProPage", "OpenUsingButtons")]
		[TestCase("AdminGoogleAnalyticsProPage", "OpenUsingControlPanel")]

		[TestCase("HostSiteGroupsPage", "OpenUsingUrl")]
		[TestCase("HostSiteGroupsPage", "OpenUsingButtons")]
		[TestCase("HostSiteGroupsPage", "OpenUsingControlPanel")]
		public void NavigationToPageEEandPEpackage(string pageClassName, string openMethod)
		{
			DoNavigateToPage(pageClassName, openMethod);
		}


		[Category(@"CEpackage")]
		[TestCase("AdminGoogleAnalyticsPage", "OpenAdminGoogleAnalyticsUsingUrl")]
		[TestCase("AdminGoogleAnalyticsPage", "OpenUsingButtons")]
		[TestCase("AdminGoogleAnalyticsPage", "OpenAdminGoogleAnalyticsPageUsingControlPanel")]
		public void NavigationToPageCEpackage(string pageClassName, string openMethod)
		{
			DoNavigateToPage(pageClassName, openMethod);
		}

		[Test]
		[Category(@"CEpackage")]
		public void NumberOfLinksOnHostPageCEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number of Links on Host Page'");

			HostPage hostPage = new HostPage(_driver);
			hostPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Host page: " + HostPage.NumberOfLinksCEPackage);
			Assert.That(hostPage.FindElements(By.XPath(HostPage.FeaturesList)).Count, Is.EqualTo(HostPage.NumberOfLinksCEPackage),
						"The number of links on Host page is not correct");

			VerifyStandardPageLayout(hostPage);
		}

		[Test]
		[Category(@"PEpackage")]
		public void NumberOfLinksOnHostPagePEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number of Links on Host Page'");

			HostPage hostPage = new HostPage(_driver);
			hostPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Host page: " + HostPage.NumberOfLinksPEPackage);
			Assert.That(hostPage.FindElements(By.XPath(HostPage.FeaturesList)).Count, Is.EqualTo(HostPage.NumberOfLinksPEPackage),
						"The number of links on Host page is not correct");

			VerifyStandardPageLayout(hostPage);
		}

		[Test]
		[Category(@"EEpackage")]
		public void NumberOfLinksOnHostPageEEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number of Links on Host Page'");

			HostPage hostPage = new HostPage(_driver);
			hostPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Host page: " + HostPage.NumberOfLinksEEPackage);
			Assert.That(hostPage.FindElements(By.XPath(HostPage.FeaturesList)).Count, Is.EqualTo(HostPage.NumberOfLinksEEPackage),
						"The number of links on Host page is not correct");

			VerifyStandardPageLayout(hostPage);
		}

		[TestCase("HostTechnicalSupportPage", "OpenHostTechnicalSupportPageUsingButtons")]
		[TestCase("HostTechnicalSupportPage", "OpenHostTechnicalSupportPageUsingControlPanel")]

		[TestCase("HostKnowledgeBasePage", "OpenHostKnowledgeBasePageUsingButtons")]
		[TestCase("HostKnowledgeBasePage", "OpenHostKnowledgeBasePageUsingControlPanel")]

		[TestCase("HostMySupportTicketsPage", "OpenHostMySupportTicketsPageUsingButtons")]
		[TestCase("HostMySupportTicketsPage", "OpenHostMySupportTicketsPageUsingControlPanel")]
		public void NavigationToOutsidePagePart1(string pageClassName, string openMethod)
		{
			DoNavigateToOutsidePage(pageClassName, openMethod);
		}

		[TestCase("HostLicenseManagementPage", "OpenHostLicenseManagementPageUsingButtons")]
		[TestCase("HostLicenseManagementPage", "OpenHostLicenseManagementPageUsingControlPanel")]

		[TestCase("HostSoftwareAndDocumentationPage", "OpenHostSoftwareAndDocumentationPageUsingButtons")]
		[TestCase("HostSoftwareAndDocumentationPage", "OpenHostSoftwareAndDocumentationPageUsingControlPanel")]
		[Category("Local only")]
		public void NavigationToOutsidePagePart2(string pageClassName, string openMethod)
		{
			DoNavigateToOutsidePage(pageClassName, openMethod);
		}

	}
}
