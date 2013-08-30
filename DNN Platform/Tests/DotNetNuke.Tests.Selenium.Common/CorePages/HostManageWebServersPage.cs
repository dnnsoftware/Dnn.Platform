using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostManageWebServersPage : BasePage
	{
		public HostManageWebServersPage(IWebDriver driver) : base(driver) { }

		public static string HostManageWebSitesUrl = "/Host/ProfessionalFeatures/ManageWebServers";

		public override string PageTitleLabel
		{
			get { return "Manage Web Servers"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Manage Web Servers"; }
		}

		public static string CachingAccordion = "//h2[@id='dnnSitePanel-EditServersCaching']/a";
		public static string CachingProviderDropDown = "//div[contains(@id, '_CachingProviders_DropDown')]";
		public static string CachingProviderArrow = "//a[contains(@id, '_CachingProviders_Arrow')]";
		public static string CachingProviderDefaultSelection = "//input[contains(@id, '_CachingProviders_Input')]";
		public static string CachingProviderDefaultValue = "WebRequestCachingProvider";
		public static string WebFarmCheckbox = "//input[contains(@id, '_EditServers_chkWebFarm')]";
		public static string IISAppNameCheckbox = "//input[contains(@id, '_EditServers_chkUseIISAppName')]";


		public static string ServersAccordion = "//h2[@id='dnnSitePanel-EditServersServers']/a";
		public static string ServerName = "//span[contains(@id, '_lstServers_lblServerName_0')]";
		public static string Url = "//*[contains(@id, '_lstServers_lblUrl_0')]";
		public static string ServerEnabled = "//span[contains(@id, '_lstServers_lblEnabled_0')]";
		public static string EditServerButton = "//a[contains(@id, '_EditServers_lstServers_Edit_0')]";
		public static string SaveButton = "//a[contains(@id, '_EditServers_lstServers_Save_0')]";
		public static string EnableServerCheckbox = "//input[contains(@id, '_lstServers_chkEnabled_0')]";

		public static string MemoryUsageAccordion = "//h2[@id='dnnSitePanel-EditServersMemory']/a";
		public static string ServerNameOnMemoryUsage = "//span[contains(@id, '_EditServers_lblServer')]";
		public static string MemoryLimit = "//span[contains(@id, '_EditServers_lblLimit')]";
		public static string PrivateBytes = "//span[contains(@id, '_EditServers_plPrivateBytes')]";
		public static string CacheObjects = "//span[contains(@id, '_EditServers_plCount')]";

		public static string SSLOffloadHeaderAccordion = "//h2[@id='dnnSitePanel-SSLOffload']/a";
		public static string HeaderValueTextBox = "//input[contains(@id, '_EditServers_txtSSloffload')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostManageWebSitesUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.ManageWebServiceLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostAdvancedSettings, ControlPanelIDs.HostManageWebServersOption);
		}

		public void EnableServer()
		{
			WaitForElement(By.XPath(EditServerButton)).Click();
			Thread.Sleep(1000);

			WaitForElement(By.XPath(EnableServerCheckbox));
			CheckBoxCheck(By.XPath(EnableServerCheckbox));

			Click(By.XPath(SaveButton));
			Thread.Sleep(1000);
		}
	}
}
