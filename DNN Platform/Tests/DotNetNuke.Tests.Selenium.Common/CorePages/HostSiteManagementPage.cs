using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostSiteManagementPage : BasePage
	{
		public HostSiteManagementPage(IWebDriver driver) : base(driver) { }

		public static string HostSiteManagementUrl = "/Host/SiteManagement";

		public override string PageTitleLabel
		{
			get { return "Site Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Site Management"; }
		}

		public override string PreLoadedModule
		{
			get { return "SiteManagementModule"; }
		}

		public static string PortalsTable = "//div[contains(@id, '_Portals_grdPortals')]";
		public static string PortalsList = "//tr[contains(@id, 'Portals_grdPortals')]";
		public static string AddNewSiteButton = "//a[contains(@id, 'Portals_createSite')]";

		public static string ParentSiteRadioButton = "dnn_ctr321_Signup_optType_0";
		public static string ChildSiteRadioButton = "dnn_ctr321_Signup_optType_1";
		public static string SiteAliasTextBox = "//input[contains(@id, 'Signup_txtPortalAlias')]";
		public static string TitleTextBox = "//input[contains(@id, 'Signup_txtPortalName')]";
		public static string AdminNameTextBox = "//input[contains(@id, 'Signup_txtUsername')]";
		public static string FirstNameTextBox = "//input[contains(@id, 'Signup_txtFirstName')]";
		public static string LastNameTextBox = "//input[contains(@id, 'Signup_txtLastName')]";
		public static string EmailTextBox = "//input[contains(@id, 'Signup_txtEmail')]";
		public static string PasswordTextBox = "//input[contains(@id, 'Signup_txtPassword')]";
		public static string ConfirmTextBox = "//input[contains(@id, 'Signup_txtConfirm')]";
		public static string EnableCurrentUserAsAdmin = "//input[contains(@id, 'Signup_useCurrent')]";

		public static string SiteNameDescriptionTextBox = "//textarea[contains(@id, 'SiteSettings_txtDescription')]";

		public static string CreateSiteFrameButton = "//a[contains(@id, 'Signup_cmdUpdate')]";
		public static string UpdateFrameButton = "//a[contains(@id, 'SiteSettings_cmdUpdate')]";
		public static string DeleteFrameButton = "//a[contains(@id, 'SiteSettings_cmdDelete')]";
		public static string CancelButton = "//a[contains(@id, 'Signup_cmdCancel')]";

		public static string SiteCreatedConfirmationMessage = "//div[contains(@id, 'Signup_UP')]/div[contains(@id, 'dnnSkinMessage')]/span[contains(@id, 'lblMessage')]";
		public static string SiteCreatedConfirmationMessageText = "There was an error sending confirmation emails - There is a problem with the configuration of your SMTP Server. Mail was not sent.. However, the website was created.";

		public static string BasicSettingsFrameTab = "//a[@href = '#ssBasicSettings']";
		public static string AdvancedSettingsFrameTab = "//a[@href = '#erAdvancedSettings']";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostSiteManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.SiteManagementLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostCommonSettings, ControlPanelIDs.HostSiteManagementOption);
		}

		public void AddNewChildSite(string baseUrl, string siteName, string title)
		{
			WaitAndClick(By.XPath(AddNewSiteButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new child site:");

			WaitForElement(By.Id(ChildSiteRadioButton));
			RadioButtonSelect(By.Id(ChildSiteRadioButton));

			WaitForElement(By.XPath("//body/form[@id='Form']/span[1]"));

			Clear(By.XPath(SiteAliasTextBox));
			Type(By.XPath(SiteAliasTextBox), baseUrl + "/" + siteName);
			Type(By.XPath(TitleTextBox), title);

			CheckBoxCheck( By.XPath(EnableCurrentUserAsAdmin));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Create Site' button:");
			ClickOnButton(By.XPath(CreateSiteFrameButton));

			WaitForElement(By.XPath(SiteCreatedConfirmationMessage), 60);
		}

		public void EditSite(string baseUrl, string siteName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit site:");
			WaitAndClick(By.XPath("//tr[td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + baseUrl + "/" + siteName + "')]]/td/a[contains(@href, 'Edit')]"));
		}

		public void DeleteSite(string baseUrl, string siteName)
		{
			EditSite(baseUrl, siteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the site:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Delete' button:");
			ClickOnButton(By.XPath(DeleteFrameButton));

			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}

		public void AddDescriptionToSite(string baseUrl, string siteName, string siteNameDescription)
		{
			EditSite(baseUrl, siteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Add description to the site:");

			WaitAndClick(By.XPath(BasicSettingsFrameTab));
			Type(By.XPath(SiteNameDescriptionTextBox), siteNameDescription);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateFrameButton));

			Thread.Sleep(1000);
		}

		public void NavigateToChildSite(string baseUrl, string siteName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Navigate to the site:");

			Click(By.XPath("//span[contains(@id, 'Portals_grdPortals')]/a[contains(string(), '" + baseUrl + "/" + siteName + "')]"));

			Thread.Sleep(1000);
		}
	}
}
