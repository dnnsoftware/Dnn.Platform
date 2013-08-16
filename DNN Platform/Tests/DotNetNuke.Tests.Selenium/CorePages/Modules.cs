using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class Modules : BasePage
	{
		public Modules(IWebDriver driver) : base(driver) { }

		public static string ModulePanel = "//div[@id = 'ControlBar_Module_AddNewModule']";
		public static string PageDropDownId = "//div[@id = 'ControlBar_PageList']";
		public static string MakeACopyCheckbox = "//input[@id = 'ControlBar_Module_chkCopyModule']";

		public static int NumberOfAvailableModulesEEPackage = 50;
		public static string List = "//ul[contains(@class, 'ModuleList')]/li";

		public static string ModuleListMessage = "//p[contains(@class, 'ModuleListMessage_InitialMessage')]";
		
		//Module ID's in the Uppper List
		public static string AccountLoginModule = "//div[img[contains(@src, 'authentication.png')]]";
		public static string AccountRegistrationModule = "//div[img[contains(@src, 'Users_32x32_Standard.png')]]";
		public static string AdvancedURLManagementModule = "//div[img[contains(@src, 'AdvancedUrlMngmt_32x32.png')]]";
		public static string AdvancedSettingsModule = "//div[span[text() = 'AdvancedSettings']]";
		public static string BannersModule = "//div[img[contains(@src, 'banners.png')]]";
		public static string CommerceModule = "//div[span[text() = 'Commerce']]";
		public static string ConfigurationManagerModule = "//div[img[contains(@src, 'Configuration_32X32_Standard.png')]]";
		public static string ConsoleModule = "//div[img[contains(@src, 'console.png')]]";
		public static string ContentListModule = "//div[img[contains(@src, 'contentList.png')]]";
		public static string DashboardModule = "//div[img[contains(@src, 'Dashboard_32X32_Standard.png')]]";
		public static string DDRMenuModule = "//div[span[text() = 'DDR Menu']]";
		public static string DigitalAssetManagementModule = "//div[span[contains(text(), 'Digital Asset')]]";
		public static string DocumentLibraryModule = "//div[img[contains(@src, 'Document_Library_32px.png')]]";
		public static string ClientCapabilityProviderModule = "//div[img[contains(@src, 'mobiledevicedet_16X16.png')]]";
		public static string ExtensionsModule = "//div[img[contains(@src, 'Extensions_32X32_Standard.png')]]";
		public static string FileIntegrityCheckerModule = "//div[img[contains(@src, 'integrityChecker.png')]]";
		public static string GoogleAnalyticsModule = "//div[span[text() = 'Google Analytics']]";
		public static string GoogleAnalyticsProModule = "//div[span[text() = 'Google Analytics Professional']]";
		public static string HealthMonitoringModule = "//div[img[contains(@src, 'healthMonitoring.png')]]";
		public static string HtmlProModule = "//div[img[contains(@src, 'htmlPro.png')]]";
		public static string HtmlModule = "//div[img[contains(@src, 'html.png')]]";
		public static string JournalModule = "//div[img[contains(@src, 'journal_32X32.png')]]";
		public static string LanguagesModule = "//div[img[contains(@src, 'Languages_32X32_Standard.png')]]";
		public static string LicenceActivationManagerModule = "//div[img[contains(@src, 'licenseActivation.png')]]";
		public static string ListsModule = "//div[img[contains(@src, 'Lists_32X32_Standard.png')]]";
		public static string LogViewerModule = "//div[img[contains(@src, 'ViewStats_32X32_Standard.png')]]";
		public static string MemberDirectoryModule = "//div[img[contains(@src, 'member_list_32X32.png')]]";
		public static string MessageCenterModule = "//div[img[contains(@src, 'messaging_32X32.png')]]";
		public static string MyModulesModule = "//div[img[contains(@src, 'myModules.png')]]";
		public static string NewslettersModule = "//div[img[contains(@src, 'Newsletters_32X32.png')]]";
		public static string PagesModule = "//div[img[contains(@src, 'Tabs_32X32_Standard.png')]]";
		public static string ProfessionalPreviewModule = "//div[span[text() = 'ProfessionalPreview']]";
		public static string RadEditorManagerModule = "//div[span[text() = 'RadEditor Manager']]";
		public static string RazorHostModule = "//div[span[text() = 'Razor Host']]";
		public static string RecycleBinModule = "//div[img[contains(@src, 'Trash_32X32_Standard.png')]]";
		public static string SearchAdminModule = "//div[span[text() = 'Search Admin']]";
		public static string SearchResultsModule = "//div[span[text() = 'Search Results']]";
		public static string SearchCrawlerAdminModule = "//div[span[text() = 'SearchCrawler Admin']]";
		public static string SecurityCenterModule = "//div[img[contains(@src, 'securityCenter.png')]]";
		public static string SharePointViewerModule = "//div[img[contains(@src, 'SharePoint_32x32.png')]]";
		public static string SiteLogModule = "//div[img[contains(@src, 'SiteLog_32X32_Standard.png')]]";
		public static string SiteWizardModule = "//div[img[contains(@src, 'Wizard_32X32_Standard.png')]]";
		public static string SiteMapModule = "//div[img[contains(@src, 'Sitemap_32X32_Standard.png')]]";
		public static string SkinsModule = "//div[img[contains(@src, 'Skins_32X32_Standard.png')]]";
		public static string SocialGroupsModule = "//div[img[contains(@src, 'Social_Groups_32X32.png')]]";
		public static string TaxonomyManagerModule = "//div[img[contains(@src, 'Tag_32X32_Standard.png')]]";
		public static string UserSwitcherModule = "//div[img[contains(@src, 'userSwitcher.png')]]";
		public static string VendorsModule = "//div[img[contains(@src, 'Vendors_32X32_Standard.png')]]";
		public static string ViewProfileModule = "//div[img[contains(@src, 'viewProfile.png')]]";
		public static string WebServerManagerModule = "//div[img[contains(@src, 'webServerManager.png')]]";
		public static string WhatsNewModule = "//div[img[contains(@src, 'Whatsnew_32X32_Standard.png')]]";

		public static string ContactUsModule = "//div[span[text() = 'Contact Us']]";

		//Module ID's on the Page
		public static string AccountLoginModuleOnPage = "//div[contains(@class, 'DnnModule-Authentication')]";
		public static string AccountRegistrationModuleOnPage = "//div[contains(@class, 'DnnModule-Registration')]";
		public static string AdvancedURLManagementModuleOnPage = "//div[contains(@class, 'DNN_AdvUrlManagement')]";
		public static string AdvancedSettingsModuleOnPage = "//div[contains(@class, 'DnnModule-AdvancedSettings')]";
		public static string BannersModuleOnPage = "//div[contains(@class, 'DnnModule-Banners')]";
		public static string CommerceModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpCommerce')]";
		public static string ConfigurationManagerModuleOnPage = "//div[contains(@class, 'DnnModule-ConfigurationManager')]";
		public static string ConsoleModuleOnPage = "//div[contains(@class, 'DnnModule-Console')]";
		public static string ContentListModuleOnPage = "//div[contains(@class, 'DnnModule-ContentList')]";
		public static string DashboardModuleOnPage = "//div[contains(@class, 'DnnModule-Dashboard')]";
		public static string DDRMenuModuleOnPage = "//div[contains(@class, 'DnnModule-DDRMenu')]";
		public static string DigitalAssetManagementModuleOnPage = "//div[contains(@class, 'DnnModule-DotNetNukeModulesDigitalAssets')]";
		public static string DocumentLibraryModuleOnPage = "//div[contains(@class, 'DnnModule-DocumentLibrary')]";
		public static string ClientCapabilityProviderModuleOnPage = "//div[contains(@class, ' DnnModule-51Degreesmobi')]";
		public static string ExtensionsModuleOnPage = "//div[contains(@class, 'DnnModule-Extensions')]";
		public static string FileIntegrityCheckerModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpIntegrityChecker')]";
		public static string GoogleAnalyticsModuleOnPage = "//div[contains(@class, 'DnnModule-GoogleAnalytics')]";
		public static string GoogleAnalyticsProModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpGoogleAnalytics')]";
		public static string HealthMonitoringModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpHealthMonitoring')]";
		public static string HtmlProModuleOnPage = "//div[contains(@class, 'DnnModule-DNN_HTML')]";
		public static string HtmlModuleOnPage = "//div[contains(@class, 'DNN_HTML DnnModule')]";
		public static string JournalModuleOnPage = "//div[contains(@class, 'DnnModule-Journal')]";
		public static string LanguagesModuleOnPage = "//div[contains(@class, 'DnnModule-Languages ')]";
		public static string LicenceActivationManagerModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpLicenseActivation')]";
		public static string ListsModuleOnPage = "//div[contains(@class, 'DnnModule-Lists')]";
		public static string LogViewerModuleOnPage = "//div[contains(@class, 'DnnModule-LogViewer')]";
		public static string MemberDirectoryModuleOnPage = "//div[contains(@class, 'DnnModule-DotNetNukeModulesMemberDirectory ')]";
		public static string MessageCenterModuleOnPage = "//div[contains(@class, 'DnnModule-DotNetNukeModulesCoreMessaging ')]";
		public static string MyModulesModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpMyModules')]";
		public static string NewslettersModuleOnPage = "//div[contains(@class, 'DnnModule-Newsletters')]";
		public static string PagesModuleOnPage = "//div[contains(@class, 'DnnModule-Tabs')]";
		public static string ProfessionalPreviewModuleOnPage = "//div[contains(@class, 'DnnModule-ProfessionalPreview')]";
		public static string RadEditorManagerModuleOnPage = "//div[contains(@class, 'DnnModule-DotNetNukeRadEditorProvider')]";
		public static string RazorHostModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpRazorHost')]";
		public static string RecycleBinModuleOnPage = "//div[contains(@class, 'DnnModule-RecycleBin')]";
		public static string SearchAdminModuleOnPage = "//div[contains(@class, 'DnnModule-SearchAdmin')]";
		public static string SearchResultsModuleOnPage = "//div[contains(@class, 'DnnModule-SearchResults')]";
		public static string SearchCrawlerAdminModuleOnPage = "//div[contains(@class, 'DnnModule-SearchCrawlerAdmin')]";
		public static string SecurityCenterModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpSecurityCenter')]";
		public static string SharePointViewerModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpSharePointViewer')]";
		public static string SiteLogModuleOnPage = "//div[contains(@class, 'DnnModule-SiteLog')]";
		public static string SiteWizardModuleOnPage = "//div[contains(@class, 'DnnModule-SiteWizard')]";
		public static string SiteMapModuleOnPage = "//div[contains(@class, 'DnnModule-Sitemap')]";
		public static string SkinsModuleOnPage = "//div[contains(@class, 'DnnModule-Skins')]";
		public static string SocialGroupsModuleOnPage = "//div[contains(@class, 'DnnModule-SocialGroups')]";
		public static string TaxonomyManagerModuleOnPage = "//div[contains(@class, 'DnnModule-DotNetNukeTaxonomy')]";
		public static string UserSwitcherModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpUserSwitcher')]";
		public static string VendorsModuleOnPage = "//div[contains(@class, 'DnnModule-Vendors')]";
		public static string ViewProfileModuleOnPage = "//div[contains(@class, 'DnnModule-ViewProfile')]";
		public static string WebServerManagerModuleOnPage = "//div[contains(@class, 'DnnModule-DNNCorpWebServerManager')]";
		public static string WhatsNewModuleOnPage = "//div[contains(@class, 'DnnModule-WhatsNew')]";

		//Pane ID's on the page
		public static string ContentPaneID = "//div[@id = 'content']/div[1]";
		public static string ContentPaneLowerID = "//div[contains(@id, 'dnn_contentPaneLower')]";
		public static string LeftPaneID = "//div[contains(@id,'dnn_leftPane')]";
		public static string SideBarPaneID = "//div[contains(@id,'dnn_sidebarPane')]";
		public static string FooterLeftOuterPaneID = "//div[contains(@id,'dnn_footerLeftOuterPane')]";
		public static string FooterLeftPaneID = "//div[contains(@id, 'dnn_footerLeftPane')]";
		public static string FooterCenterPaneID = "//div[contains(@id,'dnn_footerCenterPane')]";
		public static string FooterRightPaneID = "//div[contains(@id, 'dnn_footerRightPane')]";
		public static string FooterRightOuterPaneID = "//div[@id = 'dnn_footerRightOuterPane']";

		//Drop-down with Location Options in the Upper List
		public static string ModuleLocatorMenu = "/div[contains(@class, 'ModuleLocator_Menu')]";
		public static string ContentPaneOption = "//li[@data-pane = 'contentPane']";
		public static string ContentPaneLowerOption = "//li[@data-pane = 'contentPaneLower']";
		public static string LeftPaneOption = "//li[@data-pane = 'leftPane']";
		public static string RightPaneOption = "//li[@data-pane = 'rightPane']";
		public static string SideBarPaneOption = "//li[@data-pane = 'sidebarPane']";
		public static string LeftPaneLowerLeftOption = "//li[@data-pane = 'leftPaneLowerLeft']";
		public static string LeftPaneLowerRightPaneOption = "//li[@data-pane = 'leftPaneLowerRight']";
		public static string LeftPaneBottomOption = "//li[@data-pane = 'leftPaneBottom']";
		public static string FooterLeftOuterPaneOption = "//li[@data-pane = 'footerLeftOuterPane']";
		public static string FooterLeftPaneOption = "//li[@data-pane = 'footerLeftPane']";
		public static string FooterCenterPaneOption = "//li[@data-pane = 'footerCenterPane']";
		public static string FooterRightPaneOption = "//li[@data-pane = 'footerRightPane']";
		public static string FooterRightOuterPaneOption = "//li[@data-pane = 'footerRightOuterPane']";

		public static string EditContentFrame = "//iframe[contains(@id, 'txtContent_contentIframe')]";
		public static string EditContentFrameTab = "//a[@href = '#currentContent']";
		public static string SaveFrameButton = "//a[contains(@id, 'EditHTML_cmdSave')]";

		public static string ModuleActionsId = "moduleActions-";
		public static string SettingsIcon = "//li[@class = 'actionMenuAdmin']";
		public static string DirectionsIcon = "//li[@class = 'actionMenuMove']";
		public static string EditIcon = "//li[@class = 'actionMenuEdit']";

		public static string SettingsOption = "//span[contains(text(), 'Settings')]";
		public static string DeleteOption = "//span[contains(text(), 'Delete')]";
		public static string EditContentOption = "//span[contains(text(), 'Edit Content')]";

		//Module Drop-down Options with Locations
		public static string TopOption = "//li[text() = 'Top']";
		public static string UpOption = "//li[text() = 'Up']";
		public static string DownOption = "//li[text() = 'Down']";
		public static string BottomOption = "//li[text() = 'Bottom']";
		public static string ToLeftPane = "//li[text() = 'To leftPane']";
		public static string ToSideBarPane = "//li[text() = 'To sidebarPane']";
		public static string ToLowerContentPane = "//li[text() = 'To contentPaneLower']";
		public static string ToFooterLeftPane = "//li[text() = 'To footerLeftPane']";
		public static string ToFooterLeftOuterPane = "//li[text() = 'To footerLeftOuterPane']";
		public static string ToFooterCenterPane = "//li[text() = 'To footerCenterPane']";
		public static string ToFooterRightPane = "//li[text() = 'To footerRightPane']";
		public static string ToFooterRightOuterPane = "//li[text() = 'To footerRightOuterPane']";

		public static string ModuleSettingsTab = "//a[@href = '#msModuleSettings']";

		public static string ModuleTitleTextBox = "//input[contains(@id, 'ModuleSettings_txtTitle')]";

		public static string UpdateButton = "//a[contains(@id, 'ModuleSettings_cmdUpdate')]";


		public void OpenModulePanelUsingControlPanel()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Add Module' option");
			SelectMenuOption(BasePage.ControlPanelModulesOption, BasePage.ModulesAddNewModuleOption);
			WaitForElement(By.XPath(List + "[1]"));
		}

		public void OpenExistingModulePanelUsingControlPanel()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Add Module' option");
			SelectMenuOption(BasePage.ControlPanelModulesOption, BasePage.ModulesAddExistingModuleOption);
			WaitForElement(By.XPath(ModuleListMessage));
			FindElement(By.XPath(Modules.PageDropDownId)).WaitTillVisible();
		}

		public void SelectFromEditMenu(string moduleNumber, string option)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Module Edit icon: ");
			string editIcon = "//div[@id = '" + ModuleActionsId + moduleNumber + "']" + EditIcon + "/a";
			string optionID = "//div[@id = '" + ModuleActionsId + moduleNumber + "']" + EditIcon + option;

			if (FindElement(By.XPath(editIcon)).Displayed)
			{
				FindElement(By.XPath(BasePage.SearchBox)).ScrollIntoView().WaitTillVisible();
			}

			WaitForElement(By.XPath(editIcon));
			//WaitForElement(By.XPath(editIcon)).ScrollIntoView();
			SelectMenuOption(editIcon, optionID);
		}

		public void SelectFromSettingsMenu(string moduleNumber, string option)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Module Settings icon: ");
			string settingsIcon = "//div[@id = '" + ModuleActionsId + moduleNumber + "']" + SettingsIcon + "/a";
			string optionID = "//div[@id = '" + ModuleActionsId + moduleNumber + "']" + SettingsIcon + option;

			WaitForElement(By.XPath(settingsIcon)).ScrollIntoView();
			SelectMenuOption(settingsIcon, optionID);
		}

		public void SelectFromMovingMenu(string moduleNumber, string newLocation)
		{
			Trace.WriteLine(TraceLevelPage + "Click on Directions Icon: ");
			string directionsIcon = "//div[@id = '" + ModuleActionsId + moduleNumber + "']" + DirectionsIcon + "/a";
			string locationOption = "//div[@id = '" + ModuleActionsId + moduleNumber + "']" + DirectionsIcon + newLocation;

			WaitForElement(By.XPath(directionsIcon)).Click();
			FindElement(By.XPath(locationOption)).WaitTillVisible().Click();
		}


		public void AddNewModuleUsingDragAndDrop(string moduleName, string moduleNameOnPage, string location)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add a Module, Using Drag&Drop:");

			WaitForElement(By.XPath(moduleName)).ScrollIntoView().WaitTillVisible().Click();

			Trace.WriteLine(TraceLevelPage + "Mouse over module: '" + moduleName);

			Actions action = new Actions(_driver);
			action.MoveToElement(FindElement(By.XPath(moduleName))).Perform();

			action.DragAndDrop(FindElement(By.XPath(moduleName)), FindElement(By.XPath(location)).ScrollIntoView());
			action.Build().Perform();

			/*IWebElement dragElement=FindElement(By.XPath(moduleName));  
			IWebElement dropElement=FindElement(By.XPath(location)); 

			Actions builder = new Actions(_driver);
			builder.ClickAndHold(dragElement)
				.MoveToElement(dropElement)
				.Release(dropElement);

			builder.Build().Perform();*/
	
			Thread.Sleep(1000);
		}

		public void AddNewModuleUsingMenu(string moduleName, string moduleNameOnPage, string location, string locationID)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add a Module, using Menu:");

			WaitForElement(By.XPath(moduleName)).ScrollIntoView().WaitTillVisible();

			Trace.WriteLine(TraceLevelPage + "Mouse over menu option: '" + moduleName + ModuleLocatorMenu + "'");

			Actions builder = new Actions(_driver);
			builder.MoveToElement(FindElement(By.XPath(moduleName))).Perform();
			
			builder.MoveToElement(WaitForElement(By.XPath(moduleName + ModuleLocatorMenu))).Perform();

			Thread.Sleep(100);
			FindElement(By.XPath(location)).ScrollIntoView().WaitTillVisible().Click(); 

			Trace.WriteLine(TraceLevelPage + "Wait for Module on the page: '" + moduleNameOnPage);
			WaitForElement(By.XPath(locationID + moduleNameOnPage), 60);

			Thread.Sleep(1000);
		}

		public void MoveModuleUsingDragAndDrop(string moduleNumber, string moduleNameOnPage, string newLocation)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Move a Module, using Drag and Drop:");

			Actions action = new Actions(_driver);

			action.DragAndDrop(FindElement(By.XPath("//div[a[@name = '" + moduleNumber + "']]/div")), FindElement(By.XPath(newLocation))).Build().Perform();

			Thread.Sleep(1000);
		}

		public void MoveModuleUsingMenu(string moduleNumber, string moduleNameOnPage, string newLocation)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Move a Module, using Menu:");

			SelectFromMovingMenu(moduleNumber, newLocation);

			Trace.WriteLine(TraceLevelPage + "Wait for Module arrives to new Location: '" + moduleNameOnPage);
			Thread.Sleep(1000);
			WaitForElement(By.XPath(moduleNameOnPage + "/a[@name='" + moduleNumber + "']"), 60);

			Thread.Sleep(1000);
		}

		public void DeleteModule(string moduleNumber)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Delete the Module:");

			SelectFromSettingsMenu(moduleNumber, DeleteOption);

			Actions builder = new Actions(_driver);
			builder.MoveByOffset(300,100).Build().Perform();

			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();
		}

		public void AddContentToHTMLProModule(string moduleNumber, string moduleContent)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add content to Module:");

			SelectFromEditMenu(moduleNumber, EditContentOption);
			Trace.WriteLine("Module number edited" + moduleNumber);

			WaitAndClick(By.XPath("//div[contains(@id, '" + moduleNumber + "_EditHTML_UP')]" + EditContentFrameTab));

			_driver.SwitchTo().Frame(FindElement(By.XPath(EditContentFrame)));

			Click(By.XPath("//body"));
			Type(By.XPath("//body"), Keys.Control + "a");
			Type(By.XPath("//body"), Keys.Delete);
			Type(By.XPath("//body"), moduleContent);

			_driver.SwitchTo().DefaultContent();
			WaitAndClick(By.XPath("//div[contains(@id, '" + moduleNumber + "_EditHTML_UP')]" + SaveFrameButton));

			Thread.Sleep(1000);
		}

		public void AddContentToHTMLModule(string moduleNumber, string moduleContent)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add content to Module:");

			SelectFromEditMenu(moduleNumber, EditContentOption);
			Trace.WriteLine("Module number edited: " + moduleNumber);

			Trace.WriteLine(BasePage.TraceLevelPage + "Wait for frame:");
			WaitForElement(By.XPath("//iframe[contains(@id, 'contentIframe')]"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Switch to frame:");
			_driver.SwitchTo().Frame(0);
			Clear(By.XPath("//body"));
			Type(By.XPath("//body"), moduleContent);

			_driver.SwitchTo().DefaultContent();
			WaitAndClick(By.XPath("//div[contains(@id, '" + moduleNumber + "_EditHTML_UP')]" + SaveFrameButton));

			Thread.Sleep(1000);
		}

		public void EditModuleSettings(string moduleNumber, string moduleTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Edit Module Settings:");

			SelectFromSettingsMenu(moduleNumber, SettingsOption);

			OpenTab(By.XPath(ModuleSettingsTab));
			WaitForElement(By.XPath(ModuleTitleTextBox));

			Clear(By.XPath(ModuleTitleTextBox));
			Type(By.XPath(ModuleTitleTextBox), moduleTitle);

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}
	}
}
