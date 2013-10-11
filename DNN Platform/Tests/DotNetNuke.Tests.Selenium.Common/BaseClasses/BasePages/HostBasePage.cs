using System.Diagnostics;
using OpenQA.Selenium;

namespace DNNSelenium.Common.BaseClasses.BasePages
{
	public class HostBasePage : BasePage
	{
		public HostBasePage (IWebDriver driver) : base (driver) {}

		public static string HostPageUrl = "/Host";

		public override string PageTitleLabel
		{
			get { return "Basic Features"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Host"; }
		}

		public override string PreLoadedModule
		{
			get { return "ConsoleModule"; }
		}

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

		public static string FeaturesList = "//div[contains(@id, 'ViewConsole_Console')]/div/div[@class = 'console-large']";

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

	}
}
