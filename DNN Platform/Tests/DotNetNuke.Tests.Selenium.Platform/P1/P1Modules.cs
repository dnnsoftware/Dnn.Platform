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
		[TestCase("Home/Page33", "RazorHostModule", "ContentPane")]
		[TestCase("Home/Page34", "RecycleBinModule", "ContentPane")]
		[TestCase("Home/Page35", "SearchAdminModule", "ContentPane")]
		[TestCase("Home/Page36", "SearchResultsModule", "ContentPane")]
		[TestCase("Home/Page40", "SiteLogModule", "ContentPane")]
		[TestCase("Home/Page41", "SiteWizardModule", "ContentPane")]
		[TestCase("Home/Page42", "SiteMapModule", "ContentPane")]
		[TestCase("Home/Page43", "SkinsModule", "ContentPane")]
		[TestCase("Home/Page44", "SocialGroupsModule", "ContentPane")]
		[TestCase("Home/Page45", "TaxonomyManagerModule", "ContentPane")]
		[TestCase("Home/Page47", "VendorsModule", "ContentPane")]
		[TestCase("Home/Page48", "ViewProfileModule", "ContentPane")]
		[TestCase("Home/Page74", "ModuleCreator", "ContentPane")]
		public void Test002_AddModuleToContentPaneOnNewPage(string pageName, string moduleName, string location)
		{
			AddModuleToContentPaneOnNewPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/Page51", "HtmlModule", "ContentPane")]
		[TestCase("Home/Page52", "HtmlModule", "LeftPane")]
		[TestCase("Home/Page53", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/Page54", "HtmlModule", "SideBarPane")]
		[TestCase("Home/Page55", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/Page56", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/Page57", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/Page58", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/Page59", "HtmlModule", "FooterRightOuterPane")]
		public void Test003_AddHTMLModuleToPaneOnPage(string pageName, string moduleName,
													  string location)
		{
			AddHTMLModuleToPaneOnPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/Page60", "HtmlModule", "ContentPane")]
		[TestCase("Home/Page60", "HtmlModule", "LeftPane")]
		[TestCase("Home/Page60", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/Page60", "HtmlModule", "SideBarPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterRightOuterPane")]
		public void Test004_AddHTMLModuleToToAllPanesOnOnePage(string pageName, string moduleName,
															   string location)
		{
			AddHTMLModuleToToAllPanesOnOnePage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/Page26", "MemberDirectoryModule", "LeftPane")]
		[TestCase("Home/Page27", "MessageCenterModule", "LeftPane")]
		[TestCase("Home/Page29", "NewslettersModule", "LeftPane")]
		[TestCase("Home/Page30", "PagesModule", "LeftPane")]
		[TestCase("Home/Page31", "ProfessionalPreviewModule", "LeftPane")]
		[TestCase("Home/Page33", "RazorHostModule", "LeftPane")]
		[TestCase("Home/Page34", "RecycleBinModule", "LeftPane")]
		[TestCase("Home/Page35", "SearchAdminModule", "LeftPane")]
		[TestCase("Home/Page36", "SearchResultsModule", "LeftPane")]
		[TestCase("Home/Page40", "SiteLogModule", "LeftPane")]
		[TestCase("Home/Page41", "SiteWizardModule", "LeftPane")]
		[TestCase("Home/Page42", "SiteMapModule", "LeftPane")]
		[TestCase("Home/Page43", "SkinsModule", "LeftPane")]
		[TestCase("Home/Page44", "SocialGroupsModule", "LeftPane")]
		[TestCase("Home/Page45", "TaxonomyManagerModule", "LeftPane")]
		[TestCase("Home/Page47", "VendorsModule", "LeftPane")]
		[TestCase("Home/Page48", "ViewProfileModule", "LeftPane")]
		[TestCase("Home/Page74", "ModuleCreator", "LeftPane")]
		public void Test005_MoveModuleToLeftPane(string pageName, string moduleName, string newLocation)
		{
			MoveModuleToLeftPane(Modules.CommonModulesDescription, pageName, moduleName, newLocation);
		}

		[TestCase("Home/Page61")]
		public void Test0061_MoveHTMLModuleWithinContentPaneToTop(string pageName)
		{
			MoveHTMLModuleWithinContentPaneToTop(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/Page61")]
		public void Test0062_MoveHTMLModuleWithinContentPaneToBottom(string pageName)
		{
			MoveHTMLModuleWithinContentPaneToBottom(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/Page61")]
		public void Test0063_MoveHTMLModuleWithinContentPaneUp(string pageName)
		{
			MoveHTMLModuleWithinContentPaneUp(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/Page61")]
		public void Test0064_MoveHTMLModuleWithinContentPaneDown(string pageName)
		{
			MoveHTMLModuleWithinContentPaneDown(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, pageName);
		}

		[TestCase("Home/Page1", "AccountLoginModule", "ContentPane")]
		[TestCase("Home/Page2", "AccountRegistrationModule", "ContentPane")]
		[TestCase("Home/Page4", "AdvancedSettingsModule", "ContentPane")]
		[TestCase("Home/Page5", "BannersModule", "ContentPane")]
		[TestCase("Home/Page7", "ConfigurationManagerModule", "ContentPane")]
		[TestCase("Home/Page8", "ConsoleModule", "ContentPane")]
		[TestCase("Home/Page9", "ContentListModule", "ContentPane")]
		[TestCase("Home/Page10", "DashboardModule", "ContentPane")]
		[TestCase("Home/Page11", "DDRMenuModule", "ContentPane")]
		[TestCase("Home/Page12", "DigitalAssetManagementModule", "ContentPane")]
		[TestCase("Home/Page15", "ExtensionsModule", "ContentPane")]
		[TestCase("Home/Page17", "GoogleAnalyticsModule", "ContentPane")]
		[TestCase("Home/Page20", "HtmlModule", "ContentPane")]
		[TestCase("Home/Page21", "JournalModule", "ContentPane")]
		[TestCase("Home/Page22", "LanguagesModule", "ContentPane")]
		[TestCase("Home/Page24", "ListsModule", "ContentPane")]
		[TestCase("Home/Page25", "LogViewerModule", "ContentPane")]
		public void Test007_DragDropModuleToContentPaneOnNewPage(string pageName, string moduleName,
																 string location)
		{
			DragDropModuleToContentPaneOnNewPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/Page1", "AccountLoginModule", "LeftPane")]
		[TestCase("Home/Page2", "AccountRegistrationModule", "LeftPane")]
		[TestCase("Home/Page4", "AdvancedSettingsModule", "LeftPane")]
		[TestCase("Home/Page5", "BannersModule", "LeftPane")]
		[TestCase("Home/Page7", "ConfigurationManagerModule", "LeftPane")]
		[TestCase("Home/Page8", "ConsoleModule", "LeftPane")]
		[TestCase("Home/Page9", "ContentListModule", "LeftPane")]
		[TestCase("Home/Page10", "DashboardModule", "LeftPane")]
		[TestCase("Home/Page11", "DDRMenuModule", "LeftPane")]
		[TestCase("Home/Page12", "DigitalAssetManagementModule", "LeftPane")]
		[TestCase("Home/Page15", "ExtensionsModule", "LeftPane")]
		[TestCase("Home/Page17", "GoogleAnalyticsModule", "LeftPane")]
		[TestCase("Home/Page20", "HtmlModule", "LeftPane")]
		[TestCase("Home/Page21", "JournalModule", "LeftPane")]
		[TestCase("Home/Page22", "LanguagesModule", "LeftPane")]
		[TestCase("Home/Page24", "ListsModule", "LeftPane")]
		[TestCase("Home/Page25", "LogViewerModule", "LeftPane")]
		public void Test008_DragAndDropModuleToLeftPane(string pageName, string moduleName, string newLocation)
		{
			DragAndDropModuleToLeftPane(Modules.CommonModulesDescription, pageName, moduleName, newLocation);
		}

		[TestCase("Home/Page71", "HtmlModule", "ContentPane")]
		[TestCase("Home/Page71", "HtmlModule", "LeftPane")]
		[TestCase("Home/Page71", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/Page71", "HtmlModule", "SideBarPane")]
		[TestCase("Home/Page71", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/Page71", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/Page71", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/Page71", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/Page71", "HtmlModule", "FooterRightOuterPane")]
		public void Test009_DragAngDropHTMLModulesToAllPanesOnOnePage(string pageName, string moduleName,
																	   string location)
		{
			DragAngDropHTMLModulesToAllPanesOnOnePage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/Page62", "HtmlModule", "ContentPane")]
		[TestCase("Home/Page63", "HtmlModule", "LeftPane")]
		[TestCase("Home/Page64", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/Page65", "HtmlModule", "SideBarPane")]
		[TestCase("Home/Page66", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/Page67", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/Page68", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/Page69", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/Page70", "HtmlModule", "FooterRightOuterPane")]
		public void Test0010_DragAngDropHTMLModulesToAllPanesOnPage(string pageName, string moduleName,
																	string location)
		{
			DragAngDropHTMLModulesToAllPanesOnPage(Modules.CommonModulesDescription, pageName, moduleName, location);
		}

		[TestCase("Home/Page72")]
		public void Test0011_AddExistingModuleNoCopy(string pageName)
		{
			AddExistingModuleNoCopy(Modules.CommonModulesDescription, pageName);
		}

		[TestCase("Home/Page72", "Terrible Cycles, Inc.")]
		public void Test0012_EditContentOfExistingModule(string pageName, string moduleContent)
		{
			EditContentOfExistingModuleWithoutCopy("Common", "CorePages.Modules", Modules.CommonModulesDescription, pageName, moduleContent);
		}

		[TestCase("Home/Page73")]
		public void Test0013_AddExistingModuleWithCopy(string pageName)
		{
			AddExistingModuleWithCopy(Modules.CommonModulesDescription, pageName);
		}

		[TestCase("Home/Page73", "Awesome Cycles, Inc.")]
		public void Test0014_EditContentOfExistingModule(string pageName, string moduleContent)
		{
			EditContentOfExistingModuleWithCopy("Common", "CorePages.Modules", Modules.CommonModulesDescription, pageName, moduleContent);
		}

		
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingControlPanel")]
		public void Test0015_UpdateModuleSettings(string assyName, string pageClassName, string openMethod)
		{
			UpdateModuleSettings(assyName, pageClassName, openMethod, Modules.CommonModulesDescription);
		}

		[TestCase("Home/Page60", "HtmlModule", "ContentPane")]
		[TestCase("Home/Page60", "HtmlModule", "LeftPane")]
		[TestCase("Home/Page60", "HtmlModule", "ContentPaneLower")]
		[TestCase("Home/Page60", "HtmlModule", "SideBarPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterLeftOuterPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterLeftPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterCenterPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterRightPane")]
		[TestCase("Home/Page60", "HtmlModule", "FooterRightOuterPane")]
		public void Test016_DeleteModule(string pageName, string moduleName, string location)
		{
			DeleteModule(Modules.CommonModulesDescription, pageName, moduleName, location);
		}
	}
}