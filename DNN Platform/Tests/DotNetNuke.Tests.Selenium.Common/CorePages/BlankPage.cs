using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.Properties;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class BlankPage : BasePage
	{
		public BlankPage(IWebDriver driver) : base(driver) { }

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public static string PageDetailsFrameTab = "//li[contains(@id, 'ManageTabs_settingTab')]/a";
		public static string CopyPageFrameTab = "//li[contains(@id, 'ManageTabs_copyTab')]/a";
		public static string PermissionsFrameTab = "//li[contains(@id, 'ManageTabs_permissionsTab')]/a";
		public static string AdvancedSettingsTab = "//li[contains(@id, 'ManageTabs_advancedTab')]/a";

		public static string PageNameTextBox = "//input[contains(@id, 'ManageTabs_txtTabName')]";
		public static string PageDescriptionTextBox = "//textarea[contains(@id, 'ManageTabs_txtDescription')]";
		public static string PageTitleTextBox = "//input[contains(@id, 'ManageTabs_txtTitle')]";
		public static string ParentPageArrow = "cboParentTab_Arrow";
		public static string ParentPageDropDown = "//div[contains(@id, 'ManageTabs_cboParentTab')]/div/a";

		public static string CopyFromPageArrow = "cboCopyPage_Arrow";
		public static string CopyFromPageDropDown = "//div[contains(@id, 'ManageTabs_cboCopyPage')]/div/a";

		public static string AddPageFrameButton = "//a[contains(@id, 'ManageTabs_cmdUpdate')]";
		public static string UpdatePageFrameButton = "//a[contains(@id, 'ManageTabs_cmdUpdate')]";

		public static string PermissionTable = "//table[@class = 'dnnPermissionsGrid']";
		public static string UpdateButton = "//a[contains(@id, '_ManageTabs_cmdUpdate')]";

		public static string URLManagementAccordion = "//h2[@id='dnnPanel-TabsUrlManagement-Extension']/a";
		public static string CustomUrlTable = "//table[@id = 'custom-urls']";
		public static string OriginalUrlTable = "//table[@id = 'system-generated-urls']";
		public static string CreateButton = "//a[contains(@data-bind, 'createCustomUrl')]";
		public static string URLPathTextbox = "//input[contains(@data-bind, 'value: path')]";
		public static string SaveButton = "//a[contains(@data-bind, 'click: save')]";

		public void OpenUsingUrl(string baseUrl, string pageName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + pageName + "' page:");
			GoToUrl(baseUrl + "/" + pageName);
		}

		public void OpenAddNewPageFrameUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Add Page' option");
			SelectMenuOption(ControlPanelIDs.ControlPanelPagesOption, ControlPanelIDs.PagesAddNewPageOption);
		}

		public void OpenCopyPageFrameUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Copy Page' option");
			SelectMenuOption(ControlPanelIDs.ControlPanelPagesOption, ControlPanelIDs.PagesCopyPageOption);
		}

		public void AddNewPageUsingFrame(string pageName)
		{
			WaitAndSwitchToFrame(60);

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Page:");

			WaitAndClick(By.XPath(PageDetailsFrameTab));
			Type(By.XPath(PageNameTextBox), pageName);

			Click(By.XPath(AddPageFrameButton));
			WaitAndSwitchToWindow(60);
		}

		public void AddNewPage(string pageName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Page:");

			WaitAndClick(By.XPath(PageDetailsFrameTab));
			Type(By.XPath(PageNameTextBox), pageName);

			Click(By.XPath(AddPageFrameButton));

			Thread.Sleep(Settings.Default.WaitFactor * 2000);

//			WaitForElement(By.XPath("//div[@id = 'Breadcrumb']/span/a[contains(text(), '" + pageName + "')]"), 30).WaitTillVisible(30);

		}

		public void CopyFromContactUsPage()
		{
			WaitForElement(By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_7')]"));

			CheckBoxCheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_0')]"));
			RadioButtonSelect(By.XPath("//tr[2]//input[contains(@id, 'optNew_0')]"));

			CheckBoxCheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_1')]"));
			RadioButtonSelect(By.XPath("//tr[3]//input[contains(@id, 'optCopy_1')]"));

			CheckBoxCheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_2')]"));
			RadioButtonSelect(By.XPath("//tr[4]//input[contains(@id, 'optReference_2')]"));

			CheckBoxUncheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_3')]"));

			CheckBoxUncheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_4')]"));

			CheckBoxUncheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_5')]"));

			CheckBoxUncheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_6')]"));

			CheckBoxUncheck( By.XPath("//input[contains(@id, 'ManageTabs_grdModules_chkModule_7')]"));
		}

		public void CopyPage(string pageName, string parentPage, string copyFromPage)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Copy Page:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Page Details' Tab:");
			OpenTab(By.XPath(PageDetailsFrameTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Enter page name in 'Page Name' text box:");
			Type(By.XPath(PageNameTextBox), pageName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Select Parent Page:");

			LoadableSelectByValue(By.XPath(ParentPageDropDown), parentPage);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Copy Page' Tab:");
			OpenTab(By.XPath(CopyPageFrameTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Select 'Copy From Page':");

			LoadableSelectByValue(By.XPath(CopyFromPageDropDown), copyFromPage);

			CopyFromContactUsPage();

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Add Page' button:");
			ClickOnButton(By.XPath(AddPageFrameButton));

			Thread.Sleep(Settings.Default.WaitFactor * 2000);
		}

		public void DeletePage(string pageName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the Page:");

			SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.DeletePageOption);

			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();

		}

		public void EditPageTitle(string pageTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Edit Page title:");
			OpenTab(By.XPath(PageDetailsFrameTab));
			Clear(By.XPath(PageTitleTextBox));
			Type(By.XPath(PageTitleTextBox), pageTitle);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update Page' button:");
			ClickOnButton(By.XPath(UpdatePageFrameButton));

			Thread.Sleep(Settings.Default.WaitFactor * 1000);
		}

		public void SetPageViewPermissions(string option, string allow)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Set Page View Permissions:");

			SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PagePermissionsOption);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Permission table: ");
			WaitForElement(By.XPath(PermissionTable + "//tr[td[text() = '" + option + "'" + "]]/td[2]/img")).Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Update Button: ");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(Settings.Default.WaitFactor * 1000);
		}

		public void SetPageEditPermissions(string option, string allow)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Set Page View Permissions:");

			SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PagePermissionsOption);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Permission table: ");
			WaitForElement(By.XPath(PermissionTable + "//tr[td[text() = '" + option + "'" + "]]/td[3]/img")).Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Update Button: ");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(Settings.Default.WaitFactor * 1000);
		}

		public void CreateCustomURL(string newUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Create Custom Url:");

			OpenTab(By.XPath(AdvancedSettingsTab));
			AccordionOpen(By.XPath(URLManagementAccordion));

			WaitAndClick(By.XPath(CreateButton));

			WaitForElement(By.XPath(URLPathTextbox)).WaitTillEnabled();

			Type(By.XPath(URLPathTextbox), newUrl);

			Click(By.XPath(SaveButton));

			Thread.Sleep(1000);

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void EditCustomURL(string oldUrl, string modifiedUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Edit Custom Url:");

			OpenTab(By.XPath(AdvancedSettingsTab));
			AccordionOpen(By.XPath(URLManagementAccordion));

			WaitForElement(By.XPath("//table[@id = 'custom-urls']/tbody/tr/td//a[@data-bind = 'click: editUrl']")).WaitTillEnabled().Click();

			WaitForElement(By.XPath(URLPathTextbox)).WaitTillEnabled();

			Clear(By.XPath(URLPathTextbox));

			Type(By.XPath(URLPathTextbox), modifiedUrl);

			Click(By.XPath(SaveButton));

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void DeleteCustomURL(string newUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Delete Custom Url:");

			OpenTab(By.XPath(AdvancedSettingsTab));
			AccordionOpen(By.XPath(URLManagementAccordion));

			WaitForElement(By.XPath("//table[@id = 'custom-urls']/tbody/tr/td//a[@data-bind = 'attachDeleteConfirmation: true']")).WaitTillEnabled().Click();

			WaitForConfirmationBox(15);

			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}
	}
}
