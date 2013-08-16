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

namespace DotNetNuke.Tests.DNNSelenium.SocialBVT
{
	[TestFixture]
	[Category("SocialBVT")]
	public class Navigation : TestBase
	{
		IWebDriver _driver = null;
		private LoginPage _loginPage = null;
		private string _baseUrl = null;

		[TestFixtureSetUp]
		public void Login()
		{
			var doc = XDocument.Load(@"SocialBVT\" + Settings.Default.SocialBVTDataFile);

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
			Assert.IsFalse(currentPage.ElementPresent(By.XPath("//div[contains(@id, 'dnnSkinMessage') and contains(@class, 'dnnFormValidationSummary')]")),
						"The error message is present on the current page");

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

			Type pageClassType = Type.GetType("DotNetNuke.Tests.DNNSelenium." + pageClassName);

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

			Type pageClassType = Type.GetType("DotNetNuke.Tests.DNNSelenium." + pageClassName);
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

				var fiPageHeader = pageClassType.GetField("PageHeader");
				var pageHeader = (string)fiPageHeader.GetValue(null);

				var basePage = (BasePage)navToPage;
				StringAssert.Contains(pageHeader.ToUpper(), basePage.WaitForElement(By.XPath("//span[@id= 'dnn_dnnBreadcrumb_lblBreadCrumb']/a[last()]")).Text.ToUpper(),
						"The wrong page is opened or The title of " + pageHeader + " page is changed");

				VerifyStandardPageLayout(basePage);
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot create class " + pageClassName);
			}
		}

		[TestCase("CorePages.AdminPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminPage", "OpenUsingButtons")]

		[TestCase("CorePages.AdminAdvancedUrlManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminAdvancedUrlManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminAdvancedUrlManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminDevicePreviewManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminDevicePreviewManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminDevicePreviewManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminEventViewerPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminEventViewerPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminEventViewerPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminExtensionsPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminExtensionsPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminExtensionsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminFileManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminFileManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminFileManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminGoogleAnalyticsProPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminGoogleAnalyticsProPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminGoogleAnalyticsProPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminLanguagesPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminLanguagesPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminLanguagesPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminListsPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminListsPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminListsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminNewslettersPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminNewslettersPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminNewslettersPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminPageManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminPageManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminPageManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminRecycleBinPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminRecycleBinPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminRecycleBinPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSearchEnginePage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSearchEnginePage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSearchEnginePage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSecurityRolesPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSecurityRolesPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSecurityRolesPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSiteLogPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSiteLogPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSiteLogPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSiteRedirectionManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSiteRedirectionManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSiteRedirectionManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSiteSettingsPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSiteSettingsPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSiteSettingsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSiteWizardPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSiteWizardPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSiteWizardPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminSkinsPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminSkinsPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminSkinsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminTaxonomyPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminTaxonomyPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminTaxonomyPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminUserAccountsPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminUserAccountsPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminUserAccountsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.AdminVendorsPage", "OpenUsingUrl")]
		[TestCase("CorePages.AdminVendorsPage", "OpenUsingButtons")]
		[TestCase("CorePages.AdminVendorsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostPage", "OpenUsingButtons")]

		[TestCase("CorePages.HostActivateYourLicensePage", "OpenUsingUrl")]
		[TestCase("CorePages.HostActivateYourLicensePage", "OpenUsingButtons")]
		[TestCase("CorePages.HostActivateYourLicensePage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostAdvancedUrlManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostAdvancedUrlManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostAdvancedUrlManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostApplicationIntegrityPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostApplicationIntegrityPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostApplicationIntegrityPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostConfigurationManagerPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostConfigurationManagerPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostConfigurationManagerPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostDashboardPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostDashboardPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostDashboardPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostDeviceDetectionManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostDeviceDetectionManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostDeviceDetectionManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostFileManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostFileManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostFileManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostExtensionsPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostExtensionsPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostExtensionsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostHealthMonitoringPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostHealthMonitoringPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostHealthMonitoringPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostHtmlEditorManagerPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostHtmlEditorManagerPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostHtmlEditorManagerPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostManageWebSitesPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostManageWebSitesPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostManageWebSitesPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostListsPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostListsPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostListsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSchedulePage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSchedulePage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSchedulePage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSecurityCenterPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSecurityCenterPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSecurityCenterPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSettingsPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSettingsPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSettingsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSiteGroupsPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSiteGroupsPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSiteGroupsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSiteManagementPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSiteManagementPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSiteManagementPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSqlPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSqlPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSqlPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostSuperUserAccountsPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostSuperUserAccountsPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostSuperUserAccountsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostUserSwitcherPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostUserSwitcherPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostUserSwitcherPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostVendorsPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostVendorsPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostVendorsPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.HostWhatsNewPage", "OpenUsingUrl")]
		[TestCase("CorePages.HostWhatsNewPage", "OpenUsingButtons")]
		[TestCase("CorePages.HostWhatsNewPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.ManageRolesPage", "OpenUsingUrl")]
		[TestCase("CorePages.ManageRolesPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.ManageUsersPage", "OpenUsingUrl")]
		[TestCase("CorePages.ManageUsersPage", "OpenUsingControlPanel")]

		[TestCase("CorePages.UserAccountPage", "OpenUsingUrl")]
		[TestCase("CorePages.UserAccountPage", "OpenUsingLink")]

		[TestCase("SocialPages.AdminGamingMechanicsPage", "OpenUsingUrl")]
		[TestCase("SocialPages.AdminGamingMechanicsPage", "OpenUsingButtons")]
		[TestCase("SocialPages.AdminGamingMechanicsPage", "OpenUsingControlPanel")]

		[TestCase("SocialPages.HomePage", "OpenUsingUrl")]
		[TestCase("SocialPages.HomePage", "OpenUsingLink")]

		[TestCase("SocialPages.AnswersPage", "OpenUsingUrl")]
		[TestCase("SocialPages.AnswersPage", "OpenUsingLink")]

		[TestCase("SocialPages.DiscussionsPage", "OpenUsingUrl")]
		[TestCase("SocialPages.DiscussionsPage", "OpenUsingLink")] 

		[TestCase("SocialPages.EventsPage", "OpenUsingUrl")]
		[TestCase("SocialPages.EventsPage", "OpenUsingLink")]

		[TestCase("SocialPages.GroupsPage", "OpenUsingUrl")]
		[TestCase("SocialPages.GroupsPage", "OpenUsingLink")]

		[TestCase("SocialPages.IdeasPage", "OpenUsingUrl")]
		[TestCase("SocialPages.IdeasPage", "OpenUsingLink")]

		[TestCase("SocialPages.NewsPage", "OpenUsingUrl")]
		[TestCase("SocialPages.NewsPage", "OpenUsingLink")]

		[TestCase("SocialPages.WikiPage", "OpenUsingUrl")]
		[TestCase("SocialPages.WikiPage", "OpenUsingLink")]

		[TestCase("SocialPages.ActivitiesPage", "OpenUsingUrl")]
		[TestCase("SocialPages.ActivitiesPage", "OpenUsingLink")]

		[TestCase("SocialPages.BadgesPage", "OpenUsingUrl")]
		[TestCase("SocialPages.BadgesPage", "OpenUsingLink")]

		[TestCase("SocialPages.LeaderboardPage", "OpenUsingUrl")]
		[TestCase("SocialPages.LeaderboardPage", "OpenUsingLink")]
		public void NavigationToPage(string pageClassName, string openMethod)
		{
			DoNavigateToPage(pageClassName, openMethod);
		}

		[TestCase("CorePages.HostTechnicalSupportPage", "OpenHostTechnicalSupportPageUsingButtons")]
		[TestCase("CorePages.HostTechnicalSupportPage", "OpenHostTechnicalSupportPageUsingControlPanel")]

		[TestCase("CorePages.HostKnowledgeBasePage", "OpenHostKnowledgeBasePageUsingButtons")]
		[TestCase("CorePages.HostKnowledgeBasePage", "OpenHostKnowledgeBasePageUsingControlPanel")]

		[TestCase("CorePages.HostMySupportTicketsPage", "OpenHostMySupportTicketsPageUsingButtons")]
		[TestCase("CorePages.HostMySupportTicketsPage", "OpenHostMySupportTicketsPageUsingControlPanel")]
		public void NavigationToOutsidePagePart1(string pageClassName, string openMethod)
		{
			DoNavigateToOutsidePage(pageClassName, openMethod);
		}

		[TestCase("CorePages.HostLicenseManagementPage", "OpenHostLicenseManagementPageUsingButtons")]
		[TestCase("CorePages.HostLicenseManagementPage", "OpenHostLicenseManagementPageUsingControlPanel")]

		[TestCase("CorePages.HostSoftwareAndDocumentationPage", "OpenHostSoftwareAndDocumentationPageUsingButtons")]
		[TestCase("CorePages.HostSoftwareAndDocumentationPage", "OpenHostSoftwareAndDocumentationPageUsingControlPanel")]
		[Category("Local only")]
		public void NavigationToOutsidePagePart2(string pageClassName, string openMethod)
		{
			DoNavigateToOutsidePage(pageClassName, openMethod);
		}

	}
}
