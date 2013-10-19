using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.CorePages
{
	public class Modules : BasePage
	{
		public Modules(IWebDriver driver) : base(driver) { }

		public override string PageTitleLabel
		{
			get { return ""; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public struct ModuleIDs
		{
			public ModuleIDs(string IdWhenOnBanner, string IdWhenOnPage)
			{
				this.IdWhenOnBanner = IdWhenOnBanner;
				this.IdWhenOnPage = IdWhenOnPage;
			}

			public string IdWhenOnBanner;
			public string IdWhenOnPage;
		}

		public static string ModulePanel = "//div[@id = 'ControlBar_Module_AddNewModule']";
		public static string PageDropDownId = "//div[@id = 'ControlBar_PageList']";
		public static string MakeACopyCheckbox = "//input[@id = 'ControlBar_Module_chkCopyModule']";

		public static int NumberOfAvailableModules = 34;
		public static string List = "//ul[contains(@class, 'ModuleList')]/li";

		public static string ModuleListMessage = "//p[contains(@class, 'ModuleListMessage_InitialMessage')]";
		
		public static Dictionary<string, ModuleIDs> CommonModulesDescription = new Dictionary<string, ModuleIDs>
			                                                                  {
																				  {"AccountLoginModule", new ModuleIDs ("//div[img[contains(@src, 'authentication.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Authentication')]")},
																				  {"AccountRegistrationModule", new ModuleIDs ("//div[img[contains(@src, 'Users_32x32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Registration')]")},
																				  {"AdvancedSettingsModule", new ModuleIDs ("//div[span[text() = 'AdvancedSettings']]", 
																					  "//div[contains(@class, 'DnnModule-AdvancedSettings')]")},
																				  {"BannersModule", new ModuleIDs ("//div[img[contains(@src, 'banners.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Banners')]")},
																				  {"ConfigurationManagerModule", new ModuleIDs ("//div[img[contains(@src, 'Configuration_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-ConfigurationManager')]")},
																				  {"ConsoleModule", new ModuleIDs ("//div[img[contains(@src, 'console.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Console')]")},
																				  {"ContentListModule", new ModuleIDs ("//div[img[contains(@src, 'contentList.png')]]", 
																					  "//div[contains(@class, 'DnnModule-ContentList')]")},
																				  {"DashboardModule", new ModuleIDs ("//div[img[contains(@src, 'Dashboard_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Dashboard')]")},
																				  {"DDRMenuModule", new ModuleIDs ("//div[span[text() = 'DDR Menu']]", 
																					  "//div[contains(@class, 'DnnModule-DDRMenu')]")},
																				  {"DeviceDetectionModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-51Degreesmobi')]")},
																				  {"DevicePreviewModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeModulesPreviewProfileManagement')]")},
																				  {"DigitalAssetManagementModule", new ModuleIDs ("//div[span[contains(text(), 'Digital Asset')]]", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeModulesDigitalAssets')]")},
																				  {"ExtensionsModule", new ModuleIDs ("//div[img[contains(@src, 'Extensions_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Extensions')]")},
																				  {"GoogleAnalyticsModule", new ModuleIDs ("//div[span[text() = 'Google Analytics']]", 
																					  "//div[contains(@class, 'DnnModule-GoogleAnalytics')]")},
																				  {"HostSettingsModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-HostSettings')]")},
																				  {"HtmlModule", new ModuleIDs ("//div[img[contains(@src, 'html.png')]]", 
																					  "//div[contains(@class, 'DNN_HTML DnnModule')]")},
																				  {"JournalModule", new ModuleIDs ("//div[img[contains(@src, 'journal_32X32.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Journal')]")},
																				  {"LanguagesModule", new ModuleIDs ("//div[img[contains(@src, 'Languages_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Languages')]")},
																				  {"ListsModule", new ModuleIDs ("//div[img[contains(@src, 'Lists_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Lists')]")},
																				  {"LogViewerModule", new ModuleIDs ("//div[img[contains(@src, 'ViewStats_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-LogViewer')]")},
																				  {"MemberDirectoryModule", new ModuleIDs ("//div[img[contains(@src, 'member_list_32X32.png')]]", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeModulesMemberDirectory')]")},
																				  {"MessageCenterModule", new ModuleIDs ("//div[img[contains(@src, 'messaging_32X32.png')]]", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeModulesCoreMessaging')]")},
																				  {"ModuleCreator", new ModuleIDs ("//div[span[text() = 'Module Creator']]", 
																					  "//div[contains(@class, 'DnnModule-ModuleCreator')]")},
																				  {"NewslettersModule", new ModuleIDs ("//div[img[contains(@src, 'Newsletters_32X32.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Newsletters')]")},
																				  {"PagesModule", new ModuleIDs ("//div[img[contains(@src, 'Tabs_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Tabs')]")},
																				  {"ProfessionalPreviewModule", new ModuleIDs ("//div[span[text() = 'ProfessionalPreview']]", 
																					  "//div[contains(@class, 'DnnModule-ProfessionalPreview')]")},
																				  {"RadEditorManagerModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeRadEditorProvider')]")},
																				  {"RazorHostModule", new ModuleIDs ("//div[span[text() = 'Razor Host']]", 
																					  "//div[contains(@class, 'DnnModule-DNNCorpRazorHost')]")},
																				  {"RecycleBinModule", new ModuleIDs ("//div[img[contains(@src, 'Trash_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-RecycleBin')]")},
																				  {"SearchAdminModule", new ModuleIDs ("//div[span[text() = 'Search Admin']]", 
																					  "//div[contains(@class, 'DnnModule-SearchAdmin')]")},
																				  {"SearchResultsModule", new ModuleIDs ("//div[span[text() = 'Search Results']]", 
																					  "//div[contains(@class, 'DnnModule-SearchResults')]")},
																				  {"SecurityRolesModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-Security')]")},
																				  {"SchedulerModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-Scheduler')]")},
																				  {"SiteLogModule", new ModuleIDs ("//div[img[contains(@src, 'SiteLog_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-SiteLog')]")},
																				  {"SiteManagementModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-Portals')]")},
																				  {"SiteRedirectionModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeMobileManagement')]")},
																				  {"SiteWizardModule", new ModuleIDs ("//div[img[contains(@src, 'Wizard_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-SiteWizard')]")},
																				  {"SiteMapModule", new ModuleIDs ("//div[img[contains(@src, 'Sitemap_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Sitemap')]")},
																				  {"SkinsModule", new ModuleIDs ("//div[img[contains(@src, 'Skins_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Skins')]")},
																				  {"SocialGroupsModule", new ModuleIDs ("//div[img[contains(@src, 'Social_Groups_32X32.png')]]", 
																					  "//div[contains(@class, 'DnnModule-SocialGroups')]")},
																				  {"SQLModule", new ModuleIDs ("", 
																					  "//div[contains(@class, 'DnnModule-SQL')]")},
																				  {"TaxonomyManagerModule", new ModuleIDs ("//div[img[contains(@src, 'Tag_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-DotNetNukeTaxonomy')]")},
																				  {"VendorsModule", new ModuleIDs ("//div[img[contains(@src, 'Vendors_32X32_Standard.png')]]", 
																					  "//div[contains(@class, 'DnnModule-Vendors')]")},
																				  {"ViewProfileModule", new ModuleIDs ("//div[img[contains(@src, 'viewProfile.png')]]", 
																					  "//div[contains(@class, 'DnnModule-ViewProfile')]")},
																				  {"ContactUsModule", new ModuleIDs ("//div[span[text() = 'Contact Us']]", 
																					  "//div[contains(@class, 'DNN_HTML DnnModule')]")},
			                                                                  };


		public struct LocationIDs
		{
			public LocationIDs(string IdWhenOnUpperList, string IdWhenOnPage, string IdWhenOnModuleDropDown)
			{
				this.IdWhenOnUpperList = IdWhenOnUpperList;
				this.IdWhenOnPage = IdWhenOnPage;
				this.IdWhenOnModuleDropDown = IdWhenOnModuleDropDown;
			}

			public string IdWhenOnUpperList;
			public string IdWhenOnPage;
			public string IdWhenOnModuleDropDown;
		}

		public static Dictionary<string, LocationIDs> LocationDescription = new Dictionary<string, LocationIDs>
			                                                                  {
																					{"ContentPane", new LocationIDs ("//li[@data-pane = 'contentPane']", 
																					  "//div[@id = 'dnn_contentPane']",
																					  "")},
																					{"ContentPaneLower", new LocationIDs ("//li[@data-pane = 'contentPaneLower']", 
																					  "//div[contains(@id, 'dnn_contentPaneLower')]",
																					  "//li[text() = 'To contentPaneLower']")},
																					{"LeftPane", new LocationIDs ("//li[@data-pane = 'leftPane']", 
																					  "//div[contains(@id,'dnn_leftPane')]", 
																					  "//li[text() = 'To leftPane']")},
																					{"RightPane", new LocationIDs ("//li[@data-pane = 'rightPane']", 
																					  "//div[contains(@id,'dnn_rightPane')]", "")},
																					{"SideBarPane", new LocationIDs ("//li[@data-pane = 'sidebarPane']", 
																					  "//div[contains(@id,'dnn_sidebarPane')]", 
																					  "//li[text() = 'To sidebarPane']")},
																					{"FooterLeftOuterPane", new LocationIDs ("//li[@data-pane = 'footerLeftOuterPane']", 
																					  "//div[contains(@id,'dnn_footerLeftOuterPane')]",
																					  "//li[text() = 'To footerLeftOuterPane']")},
																					{"FooterLeftPane", new LocationIDs ("//li[@data-pane = 'footerLeftPane']", 
																					  "//div[contains(@id, 'dnn_footerLeftPane')]",
																					  "//li[text() = 'To footerLeftPane']")},
																					{"FooterCenterPane", new LocationIDs ("//li[@data-pane = 'footerCenterPane']", 
																					  "//div[contains(@id,'dnn_footerCenterPane')]",
																					  "//li[text() = 'To footerCenterPane']")},
																					{"FooterRightPane", new LocationIDs ("//li[@data-pane = 'footerRightPane']",
																					  "//div[contains(@id, 'dnn_footerRightPane')]", 
																					  "//li[text() = 'To footerRightPane']")},
																					{"FooterRightOuterPane", new LocationIDs ("//li[@data-pane = 'footerRightOuterPane']",
																					  "//div[@id = 'dnn_footerRightOuterPane']", 
																					  "//li[text() = 'To footerRightOuterPane']")},
																					{"LeftPaneLowerLeft", new LocationIDs ("//li[@data-pane = 'leftPaneLowerLeft']", 
																					  "", 
																					  "")},
																					{"LeftPaneLowerRightPane", new LocationIDs ("//li[@data-pane = 'leftPaneLowerRight']", 
																					  "", 
																					  "")},
																					{"LeftPaneBottom", new LocationIDs ("//li[@data-pane = 'leftPaneBottom']", 
																					  "", 
																					  "")},
																					{"Top", new LocationIDs ("", 
																					  "", 
																					  "//li[text() = 'Top']")},
																					{"Bottom", new LocationIDs ("", 
																					  "", 
																					  "//li[text() = 'Bottom']")},
																					{"Up", new LocationIDs ("", 
																					  "", 
																					  "//li[text() = 'Up']")},
																					{"Down", new LocationIDs ("", 
																					  "", 
																					  "//li[text() = 'Down']")},
																			  };

		public static string ModuleLocatorMenu = "/div[contains(@class, 'ModuleLocator_Menu')]";

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

		public static string ModuleSettingsTab = "//a[@href = '#msModuleSettings']";
		public static string PageSettingsTab = "//a[@href = '#msPageSettings']";

		public static string ModuleTitleTextBox = "//input[contains(@id, 'ModuleSettings_txtTitle')]";
		public static string ModuleContainerArrow = "//a[contains(@id, '_moduleContainerCombo_Arrow')]";
		public static string ModuleContainerDropDown = "//div[contains(@id, '_moduleContainerCombo_DropDown')]";

		public static string UpdateButton = "//a[contains(@id, 'ModuleSettings_cmdUpdate')]";


		public void OpenModulePanelUsingControlPanel()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Add Module' option");
			SelectMenuOption(ControlPanelIDs.ControlPanelModulesOption, ControlPanelIDs.ModulesAddNewModuleOption);
			WaitForElement(By.XPath(List + "[1]"));
		}

		public void OpenExistingModulePanelUsingControlPanel()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Add Module' option");
			SelectMenuOption(ControlPanelIDs.ControlPanelModulesOption, ControlPanelIDs.ModulesAddExistingModuleOption);
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
				FindElement(By.XPath(ControlPanelIDs.SearchBox)).ScrollIntoView().WaitTillVisible();
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


		public void AddNewModuleUsingDragAndDrop(string moduleName, string location)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add a Module, Using Drag&Drop:");

			WaitForElement(By.XPath(moduleName)).ScrollIntoView().WaitTillVisible().Click();

			Trace.WriteLine(TraceLevelPage + "Mouse over module: '" + moduleName);

			Actions action = new Actions(_driver);
			action.MoveToElement(FindElement(By.XPath(moduleName))).Perform();

			action.DragAndDrop(FindElement(By.XPath(moduleName)), FindElement(By.XPath(Modules.LocationDescription[location].IdWhenOnPage)).ScrollIntoView());
			action.Build().Perform();
	
			Thread.Sleep(1000);
		}

		public void AddNewModuleUsingMenu(string moduleName, string moduleNameOnPage, string location)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add a Module, using Menu:");

			WaitForElement(By.XPath(moduleName)).ScrollIntoView().WaitTillVisible();

			Trace.WriteLine(TraceLevelPage + "Mouse over menu option: '" + moduleName + ModuleLocatorMenu + "'");

			Actions builder = new Actions(_driver);
			builder.MoveToElement(FindElement(By.XPath(moduleName))).Perform();

			builder.MoveToElement(WaitForElement(By.XPath(moduleName + ModuleLocatorMenu))).Perform();

			Thread.Sleep(100);
			FindElement(By.XPath(Modules.LocationDescription[location].IdWhenOnUpperList)).ScrollIntoView().WaitTillVisible().Click(); 

			Trace.WriteLine(TraceLevelPage + "Wait for Module on the page: '" + moduleName);
			WaitForElement(By.XPath(Modules.LocationDescription[location].IdWhenOnPage + moduleNameOnPage), 60);

			Thread.Sleep(1000);
		}

		public void MoveModuleUsingDragAndDrop(string moduleNumber, string newLocation)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Move a Module, using Drag and Drop:");

			Actions action = new Actions(_driver);

			action.DragAndDrop(FindElement(By.XPath("//div[a[@name = '" + moduleNumber + "']]/div")), FindElement(By.XPath(newLocation))).Build().Perform();

			Thread.Sleep(1000);
		}

		public void MoveModuleUsingMenu(string moduleNumber, string moduleNameOnPage, string newLocation)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Move a Module, using Menu:");

			SelectFromMovingMenu(moduleNumber, Modules.LocationDescription[newLocation].IdWhenOnModuleDropDown);

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

		public void AddContentToHTMLModule(string moduleNumber, string moduleContent)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add content to HTML Module:");

			SelectFromEditMenu(moduleNumber, EditContentOption);
			Trace.WriteLine("Module number edited: " + moduleNumber);

			Trace.WriteLine(BasePage.TraceLevelPage + "Wait for frame:");
			WaitForElement(By.XPath("//iframe[contains(@id, 'contentIframe')]"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Switch to frame:");
			_driver.SwitchTo().Frame(0);

			Click(By.XPath("//body"));
			Type(By.XPath("//body"), Keys.Control + "a");
			Type(By.XPath("//body"), Keys.Delete);
			Type(By.XPath("//body"), moduleContent);

			_driver.SwitchTo().DefaultContent();
			WaitAndClick(By.XPath("//div[contains(@id, '" + moduleNumber + "_EditHTML_UP')]" + SaveFrameButton));

			Thread.Sleep(1000);
		}

		public void ChangeModuleTitle(string moduleNumber, string moduleTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Edit Module Settings: Change Module Title");

			SelectFromSettingsMenu(moduleNumber, SettingsOption);

			OpenTab(By.XPath(ModuleSettingsTab));

			WaitAndClear(By.XPath(ModuleTitleTextBox));
			Type(By.XPath(ModuleTitleTextBox), moduleTitle);

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void SetModuleContainer(string moduleNumber, string moduleContainer)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Edit Module Settings: Set Module Container");

			SelectFromSettingsMenu(moduleNumber, SettingsOption);

			OpenTab(By.XPath(PageSettingsTab));
			WaitForElement(By.XPath(ModuleContainerArrow));

			SlidingSelectByValue(By.XPath(ModuleContainerArrow), By.XPath(ModuleContainerDropDown), moduleContainer);

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}
	}
}
