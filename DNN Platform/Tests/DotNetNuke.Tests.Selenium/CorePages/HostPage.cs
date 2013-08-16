using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostPage : BasePage
	{
		public HostPage (IWebDriver driver) : base (driver) {}

		public static string HostPageUrl = "/Host";

		public static string PageTitleLabel = "Basic Features";
		public static string PageHeader = "";

		public static string HostTopMenuLink = "//ul[@id='ControlNav']/li[2]/a";

		public static string ConfigurationManagerLink = "//div[a/img[contains(@src, 'Configuration_32X32_Standard.png')]]";
		public static string DashboardLink = "//div[a/img[contains(@src, 'Dashboard_32X32_Standard.png')]]";
		public static string DeviceDetectionManagement = "//div[a/img[contains(@src, 'mobiledevicedet_32X32.png')]]";
		public static string DigitalAssetLink = "//div[a/img[contains(@src, 'Files_32X32_Standard.png')]]";
		public static string ExtensionsLink = "//div[a/img[contains(@src, 'Extensions_32X32_Standard.png')]]";
		public static string HostSettingsLink = "//div[a/img[contains(@src, 'Hostsettings_32X32_Standard.png')]]";
		public static string HtmlEditorManagerLink = "//div[a/img[contains(@src, 'radeditor_config_large.png')]]";
		public static string ListsLink = "//div[a/img[contains(@src, 'Lists_32X32_Standard.png')]]";
		public static string ScheduleLink = "//div[a/img[contains(@src, 'ScheduleHistory_32X32_Standard.png')]]";
		public static string SearchAdminLink = "//div[a/img[contains(@src, 'Search_32x32_Standard.png') and @alt='Search Admin']]";
		public static string SiteManagementLink = "//div[a/img[contains(@src, 'SiteSettings_32X32_Standard.png')]]";
		public static string SqlLink = "//div[a/img[contains(@src, 'Sql_32x32_Standard.png')]]";
		public static string SuperUsersAccountsLink = "//div[a/img[contains(@src, 'Hostuser_32X32_Standard.png')]]";
		public static string VendorsLink = "//div[a/img[contains(@src, 'Vendors_32X32_Standard.png')]]";
		public static string WhatsNewLink = "//div[a/img[contains(@src, 'Whatsnew_32X32_Standard.png')]]";

		public static string ActivateYourLicenseLink = "//div[a/img[contains(@src, 'Activatelicense_32X32_Standard.png')]]";
		public static string AdvancedUrlManagementLink = "//div[a/img[contains(@src, 'AdvancedUrlMngmt_32x32.png')]]";
		public static string ApplicationIntegrityLink = "//div[a/img[contains(@src, 'Appintegrity_32X32_Standard.png')]]";
		public static string HealthMonitoringLink = "//div[a/img[contains(@src, 'Health_32X32_Standard.png')]]";
		public static string KnowledgeBaseLink = "//div[a/img[contains(@src, 'Kb_32X32_Standard.png')]]";
		public static string LicenseManagementLink = "//div[a/img[contains(@src, 'Licensemanagement_32X32_Standard.png')]]";
		public static string ManageWebServiceLink = "//div[a/img[contains(@src, 'Webserver_32x32_Standard.png')]]";
		public static string MySupportTicketsLink = "//div[a/img[contains(@src, 'Mytickets_32X32_Standard.png')]]";
		public static string SearchCrawlerLink = "//div[a/img[contains(@src, 'Search_32x32_Standard.png') and @alt='Search Crawler Admin']]";
		public static string SecurityCenterLink = "//div[a/img[contains(@src, 'SecurityRoles_32X32_Standard.png')]]";
		public static string SharePointConnectorLink = "//div[a/img[contains(@src, 'SharePoint_32x32.png')]]";
		public static string SiteGroupsLink = "//div[a/img[contains(@src, 'portalgroups_32X32.png')]]";
		public static string SoftwareAndDocumentationLink = "//div[a/img[contains(@src, 'Software_32X32_Standard.png')]]";
		public static string TechnicalSupportLink = "//div[a/img[contains(@src, 'Support_32X32_Standard.png')]]";
		public static string UserSwitcherLink = "//div[a/img[contains(@src, 'Users_32x32_Standard.png')]]";

		public static string FeaturesList = "//div[contains(@id, 'ViewConsole_Console')]/div/div[@class = 'console-large']";
		public static int NumberOfLinksCEPackage = 27;
		public static int NumberOfLinksPEPackage = 27;
		public static int NumberOfLinksEEPackage = 28;
		public static int NumberOfLinksSocial = 27;

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostPageUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostTopMenuLink));
		}

		#region Open Outside Pages

		public void OpenHostKnowledgeBasePageUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Knowledge Base' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.KnowledgeBaseLink));
		}

		public void OpenHostKnowledgeBasePageUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Knowledge Base' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostCommonSettings, BasePage.HostKnowledgeBaseOption);
		}

		public void OpenHostLicenseManagementPageUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'License Management' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.LicenseManagementLink));
		}

		public void OpenHostLicenseManagementPageUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'License Management' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostAdvancedSettings, BasePage.HostLicenseManagementOption);
		}

		public void OpenHostMySupportTicketsPageUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Support Tickets' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.MySupportTicketsLink));
		}

		public void OpenHostMySupportTicketsPageUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Support Tickets' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostAdvancedSettings, BasePage.HostMySupportTicketsOption);
		}

		public void OpenHostSoftwareAndDocumentationPageUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Software And Documentation' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.SoftwareAndDocumentationLink));
		}

		public void OpenHostSoftwareAndDocumentationPageUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Software And Documentation' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostCommonSettings, BasePage.HostSoftwareAndDocumentationOption);
		}

		public void OpenHostTechnicalSupportPageUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Technical Support' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.TechnicalSupportLink));
		}

		public void OpenHostTechnicalSupportPageUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host 'Technical Support' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostCommonSettings, BasePage.HostTechnicalSupportOption);
		}

		#endregion 

	}
}
