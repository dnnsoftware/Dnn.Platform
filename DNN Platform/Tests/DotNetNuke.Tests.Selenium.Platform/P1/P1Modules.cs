using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.P1
{
	[TestFixture]
	[Category("P1")]
	public class P1Modules : Common.Tests.P1.P1Modules
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}

		protected override Modules.ModuleIDs HtmlModuleDictionary
		{
			get { return Modules.CommonModulesDescription["HtmlModule"]; }
		}

		[Test]
		public void Test001_CountAvailableModules()
		{
			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			var modules = new Modules(_driver);
			modules.OpenModulePanelUsingControlPanel();

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of available Modules: " +
			                Modules.NumberOfAvailableModules);
			Assert.That(modules.FindElements(By.XPath(Modules.List)).Count, Is.EqualTo(Modules.NumberOfAvailableModules),
			            "The number of available Modules is not correct");
		}

		[TestCase("Home/Page26", "MemberDirectoryModule", "ContentPane")]
		[TestCase("Home/Page27", "MessageCenterModule", "ContentPane")]
		[TestCase("Home/Page29", "NewslettersModule", "ContentPane")]
		[TestCase("Home/Page30", "PagesModule", "ContentPane")]
		[TestCase("Home/Page31", "ProfessionalPreviewModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page33", "RazorHostModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page34", "RecycleBinModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page35", "SearchAdminModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page36", "SearchResultsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page40", "SiteLogModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page41", "SiteWizardModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page42", "SiteMapModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page43", "SkinsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page44", "SocialGroupsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page45", "TaxonomyManagerModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page47", "VendorsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page48", "ViewProfileModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page74", "ModuleCreator", "ContentPane")]
		public void Test002_AddModuleToContentPaneOnNewPage(string pageName, string moduleName, string location)
		{
			AddModuleToContentPaneOnNewPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/P1Modules/Page51", "HtmlModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page52", "HtmlModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page53", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/P1Modules/Page54", "HtmlModule", "SideBarPane")]
		[TestCase("Home/P1Modules/Page55", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/P1Modules/Page56", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/P1Modules/Page57", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/P1Modules/Page58", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/P1Modules/Page59", "HtmlModule", "FooterRightOuterPane")]
		public void Test003_AddHTMLModuleToPaneOnPage(string pageName, string moduleName,
													  string location)
		{
			AddHTMLModuleToPaneOnPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/P1Modules/Page60", "HtmlModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "SideBarPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterRightOuterPane")]
		public void Test004_AddHTMLModuleToToAllPanesOnOnePage(string pageName, string moduleName,
															   string location)
		{
			AddHTMLModuleToToAllPanesOnOnePage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/P1Modules/Page26", "MemberDirectoryModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page27", "MessageCenterModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page29", "NewslettersModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page30", "PagesModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page31", "ProfessionalPreviewModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page33", "RazorHostModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page34", "RecycleBinModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page35", "SearchAdminModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page36", "SearchResultsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page40", "SiteLogModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page41", "SiteWizardModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page42", "SiteMapModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page43", "SkinsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page44", "SocialGroupsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page45", "TaxonomyManagerModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page47", "VendorsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page48", "ViewProfileModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page74", "ModuleCreator", "LeftPane")]
		public void Test005_MoveModuleToLeftPane(string pageName, string moduleName, string newLocation)
		{
			MoveModuleToLeftPane(Modules.CommonModulesDescription, pageName, moduleName, newLocation);
		}

		[TestCase("Home/P1Modules/Page61")]
		public void Test0061_MoveHTMLModuleWithinContentPaneToTop(string pageName)
		{
			MoveHTMLModuleWithinContentPaneToTop(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/P1Modules/Page61")]
		public void Test0062_MoveHTMLModuleWithinContentPaneToBottom(string pageName)
		{
			MoveHTMLModuleWithinContentPaneToBottom(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/P1Modules/Page61")]
		public void Test0063_MoveHTMLModuleWithinContentPaneUp(string pageName)
		{
			MoveHTMLModuleWithinContentPaneUp(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/P1Modules/Page61")]
		public void Test0064_MoveHTMLModuleWithinContentPaneDown(string pageName)
		{
			MoveHTMLModuleWithinContentPaneDown(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/P1Modules/Page1", "AccountLoginModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page2", "AccountRegistrationModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page4", "AdvancedSettingsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page5", "BannersModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page7", "ConfigurationManagerModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page8", "ConsoleModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page9", "ContentListModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page10", "DashboardModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page11", "DDRMenuModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page12", "DigitalAssetManagementModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page15", "ExtensionsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page17", "GoogleAnalyticsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page20", "HtmlModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page21", "JournalModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page22", "LanguagesModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page24", "ListsModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page25", "LogViewerModule", "ContentPane")]
		public void Test007_DragDropModuleToContentPaneOnNewPage(string pageName, string moduleName,
																 string location)
		{
			DragDropModuleToContentPaneOnNewPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/P1Modules/Page1", "AccountLoginModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page2", "AccountRegistrationModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page4", "AdvancedSettingsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page5", "BannersModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page7", "ConfigurationManagerModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page8", "ConsoleModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page9", "ContentListModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page10", "DashboardModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page11", "DDRMenuModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page12", "DigitalAssetManagementModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page15", "ExtensionsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page17", "GoogleAnalyticsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page20", "HtmlModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page21", "JournalModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page22", "LanguagesModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page24", "ListsModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page25", "LogViewerModule", "LeftPane")]
		public void Test008_DragAndDropModuleToLeftPane(string pageName, string moduleName, string newLocation)
		{
			DragAndDropModuleToLeftPane(Modules.CommonModulesDescription, pageName, moduleName, newLocation);
		}

		[TestCase("Home/P1Modules/Page71", "HtmlModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "SideBarPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/P1Modules/Page71", "HtmlModule", "FooterRightOuterPane")]
		public void Test009_DragAngDropHTMLModulesToAllPanesOnOnePage(string pageName, string moduleName,
																	   string location)
		{
			DragAngDropHTMLModulesToAllPanesOnOnePage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/P1Modules/Page62", "HtmlModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page63", "HtmlModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page64", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/P1Modules/Page65", "HtmlModule", "SideBarPane")]
		[TestCase("Home/P1Modules/Page66", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/P1Modules/Page67", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/P1Modules/Page68", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/P1Modules/Page69", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/P1Modules/Page70", "HtmlModule", "FooterRightOuterPane")]
		public void Test0010_DragAngDropHTMLModulesToAllPanesOnPage(string pageName, string moduleName,
																	string location)
		{
			DragAngDropHTMLModulesToAllPanesOnPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/P1Modules/Page72")]
		public void Test0011_AddExistingModuleNoCopy(string pageName)
		{
			AddExistingModuleNoCopy(Modules.CommonModulesDescription, pageName);
		}

		[TestCase("Home/P1Modules/Page72", "Terrible Cycles, Inc.")]
		public void Test0012_EditContentOfExistingModule(string pageName, string moduleContent)
		{
			EditContentOfExistingModuleWithoutCopy("Common", "CorePages.Modules", Modules.CommonModulesDescription, pageName, moduleContent);
		}

		[TestCase("Home/P1Modules/Page73")]
		public void Test0013_AddExistingModuleWithCopy(string pageName)
		{
			AddExistingModuleWithCopy(Modules.CommonModulesDescription, pageName);
		}

		[TestCase("Home/P1Modules/Page73", "Awesome Cycles, Inc.")]
		public void Test0014_EditContentOfExistingModule(string pageName, string moduleContent)
		{
			EditContentOfExistingModuleWithCopy("Common", "CorePages.Modules", Modules.CommonModulesDescription, pageName, moduleContent);
		}

		
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingControlPanel")]
		public void Test0015_UpdateModuleSettings(string assyName, string pageClassName, string openMethod)
		{
			UpdateModuleSettings(assyName, pageClassName, openMethod, Modules.CommonModulesDescription);
		}

		[TestCase("Home/P1Modules/Page60", "HtmlModule", "ContentPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "LeftPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "SideBarPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/P1Modules/Page60", "HtmlModule", "FooterRightOuterPane")]
		public void Test016_DeleteModule(string pageName, string moduleName, string location)
		{
			DeleteModule(Modules.CommonModulesDescription, pageName, moduleName, location);
		}
	}
}