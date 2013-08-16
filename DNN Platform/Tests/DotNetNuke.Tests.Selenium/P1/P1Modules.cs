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

namespace DotNetNuke.Tests.DNNSelenium.P1
{
	[TestFixture]
	[Category("P1")]
	class P1Modules : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("page");

			string testName = pageSettings.Attribute("name").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

			//Preconditions for
			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.AddPagesInBulk("Page", 73, "Web", "Home");

			BlankPage blankPage = new BlankPage(_driver);

			Modules module = new Modules(_driver);
			
			int pageNumber = 1;
			while (pageNumber < 74)
			{
				blankPage.OpenUsingUrl(_baseUrl, "Home/Page" + pageNumber);
				blankPage.SetPageToEditMode("Home/Page" + pageNumber);
				string moduleNumber = module.WaitForElement(By.XPath(Modules.ContentPaneID + Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");
				module.DeleteModule(moduleNumber);
				pageNumber++;
			}

			blankPage.OpenUsingUrl(_baseUrl, "Home/Page61");
			module.OpenModulePanelUsingControlPanel();
			int i = 1;
			while (i < 5)
			{
				module.AddNewModuleUsingMenu(Modules.HtmlProModule, Modules.HtmlProModuleOnPage, Modules.ContentPaneOption, Modules.ContentPaneID);
				i++;
			}
			
		}

		//[TestFixtureTearDown]
		public void DeleteData()
		{
			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			int pageNumber = 1;

			while (pageNumber < 51)
			{
				adminPageManagementPage.OpenUsingButtons(_baseUrl);
				adminPageManagementPage.DeletePage("Page" + pageNumber, "Web");
	
				pageNumber++;
			}

		}

		[Test]
		public void Test001_CountAvailableModules()
		{
			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Modules modules = new Modules(_driver);
			modules.OpenModulePanelUsingControlPanel();

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of available Modules: " + Modules.NumberOfAvailableModulesEEPackage);
			Assert.That(modules.FindElements(By.XPath(Modules.List)).Count, Is.EqualTo(Modules.NumberOfAvailableModulesEEPackage),
						"The number of available Modules is not correct");
		}

		[TestCase("Home/Page26", "MemberDirectoryModule", "MemberDirectoryModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page27", "MessageCenterModule", "MessageCenterModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page28", "MyModulesModule", "MyModulesModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page29", "NewslettersModule", "NewslettersModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page30", "PagesModule", "PagesModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page31", "ProfessionalPreviewModule", "ProfessionalPreviewModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page32", "RadEditorManagerModule", "RadEditorManagerModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page33", "RazorHostModule", "RazorHostModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page34", "RecycleBinModule", "RecycleBinModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page35", "SearchAdminModule", "SearchAdminModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page36", "SearchResultsModule", "SearchResultsModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page37", "SearchCrawlerAdminModule", "SearchCrawlerAdminModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page38", "SecurityCenterModule", "SecurityCenterModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page39", "SharePointViewerModule", "SharePointViewerModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page40", "SiteLogModule", "SiteLogModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page41", "SiteWizardModule", "SiteWizardModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page42", "SiteMapModule", "SiteMapModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page43", "SkinsModule", "SkinsModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page44", "SocialGroupsModule", "SocialGroupsModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page45", "TaxonomyManagerModule", "TaxonomyManagerModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page46", "UserSwitcherModule", "UserSwitcherModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page47", "VendorsModule", "VendorsModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page48", "ViewProfileModule", "ViewProfileModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page49", "WebServerManagerModule", "WebServerManagerModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page50", "WhatsNewModule", "WhatsNewModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		public void Test002_AddModuleToContentPaneOnNewPage(string pageName, string moduleName, string moduleNameOnPage, string location, string locationID)
		{
			Modules module = new Modules(_driver);

			var fiModuleName = module.GetType().GetField(moduleName);
			moduleName = (string)fiModuleName.GetValue(null);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocation = module.GetType().GetField(location);
			location = (string)fiLocation.GetValue(null);

			var fiLocationID = module.GetType().GetField(locationID);
			locationID = (string)fiLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleName, moduleNameOnPage, location, locationID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationID + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page51", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page52", "HtmlProModule", "HtmlProModuleOnPage", "LeftPaneOption", "LeftPaneID")]
		[TestCase("Home/Page53", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneLowerOption", "ContentPaneLowerID")]
		[TestCase("Home/Page54", "HtmlProModule", "HtmlProModuleOnPage", "SideBarPaneOption", "SideBarPaneID")]
		[TestCase("Home/Page55", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftOuterPaneOption", "FooterLeftOuterPaneID")]
		[TestCase("Home/Page56", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftPaneOption", "FooterLeftPaneID")]
		[TestCase("Home/Page57", "HtmlProModule", "HtmlProModuleOnPage", "FooterCenterPaneOption", "FooterCenterPaneID")]
		[TestCase("Home/Page58", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightPaneOption", "FooterRightPaneID")]
		[TestCase("Home/Page59", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightOuterPaneOption", "FooterRightOuterPaneID")]
		public void Test003_AddHTMLModuleToPaneOnPage(string pageName, string moduleName, string moduleNameOnPage, string location, string locationID)
		{
			Modules module = new Modules(_driver);

			var fiModuleName = module.GetType().GetField(moduleName);
			moduleName = (string)fiModuleName.GetValue(null);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocation = module.GetType().GetField(location);
			location = (string)fiLocation.GetValue(null);

			var fiLocationID = module.GetType().GetField(locationID);
			locationID = (string)fiLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new HTML Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleName, moduleNameOnPage, location, locationID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationID + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneOption", "ContentPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "LeftPaneOption", "LeftPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneLowerOption", "ContentPaneLowerID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "SideBarPaneOption", "SideBarPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftOuterPaneOption", "FooterLeftOuterPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftPaneOption", "FooterLeftPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "FooterCenterPaneOption", "FooterCenterPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightPaneOption", "FooterRightPaneID")]
		[TestCase("Home/Page60", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightOuterPaneOption", "FooterRightOuterPaneID")]
		public void Test004_AddHTMLModuleToToAllPanesOnOnePage(string pageName, string moduleName, string moduleNameOnPage, string location, string locationID)
		{
			Modules module = new Modules(_driver);

			var fiModuleName = module.GetType().GetField(moduleName);
			moduleName = (string)fiModuleName.GetValue(null);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocation = module.GetType().GetField(location);
			location = (string)fiLocation.GetValue(null);

			var fiLocationID = module.GetType().GetField(locationID);
			locationID = (string)fiLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleName, moduleNameOnPage, location, locationID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationID + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}


		[TestCase("Home/Page26", "MemberDirectoryModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page27", "MessageCenterModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page28", "MyModulesModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page29", "NewslettersModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page30", "PagesModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page31", "ProfessionalPreviewModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page32", "RadEditorManagerModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page33", "RazorHostModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page34", "RecycleBinModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page35", "SearchAdminModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page36", "SearchResultsModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page37", "SearchCrawlerAdminModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page38", "SecurityCenterModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page39", "SharePointViewerModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page40", "SiteLogModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page41", "SiteWizardModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page42", "SiteMapModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page43", "SkinsModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page44", "SocialGroupsModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page45", "TaxonomyManagerModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page46", "UserSwitcherModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page47", "VendorsModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page48", "ViewProfileModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page49", "WebServerManagerModuleOnPage", "ToLeftPane", "LeftPaneID")]
		[TestCase("Home/Page50", "WhatsNewModuleOnPage", "ToLeftPane", "LeftPaneID")]
		public void Test005_MoveModuleToLeftPane(string pageName, string moduleNameOnPage, string newLocation, string newLocationID)
		{
			Modules module = new Modules(_driver);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocation = module.GetType().GetField(newLocation);
			newLocation = (string)fiLocation.GetValue(null);

			var fiNewLocationID = module.GetType().GetField(newLocationID);
			newLocationID = (string)fiNewLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			blankPage.SetPageToEditMode(pageName);
			string moduleNumber = blankPage.WaitForElement(By.XPath(moduleNameOnPage + "/a")).GetAttribute("name");
			module.MoveModuleUsingMenu(moduleNumber, moduleNameOnPage, newLocation);

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module new location: " + newLocationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(newLocationID + moduleNameOnPage)), "Module is not found");
		}


		[TestCase("Home/Page61")]
		public void Test0061_MoveHTMLModuleWithinContentPaneToTop(string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module to TOP'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			Modules module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber4, Modules.HtmlProModuleOnPage, Modules.TopOption);

			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber4), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber1), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber3), "Module is not found");
		}

		[TestCase("Home/Page61")]
		public void Test0062_MoveHTMLModuleWithinContentPaneToBottom(string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module to BOTTOM'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			Modules module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber1, Modules.HtmlProModuleOnPage, Modules.BottomOption);

			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber3), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber4), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber1), "Module is not found");
		}

		[TestCase("Home/Page61")]
		public void Test0063_MoveHTMLModuleWithinContentPaneUp(string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module UP'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			Modules module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber3, Modules.HtmlProModuleOnPage, Modules.UpOption);

			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber1), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber3), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber4), "Module is not found");
		}

		[TestCase("Home/Page61")]
		public void Test0064_MoveHTMLModuleWithinContentPaneDown(string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module DOWN'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			Modules module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber2, Modules.HtmlProModuleOnPage, Modules.DownOption);

			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[2]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber1), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[3]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber3), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[4]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[5]/a")).GetAttribute("name"), Is.EqualTo(moduleNumber4), "Module is not found");
		}

		[TestCase("Home/Page1", "AccountLoginModule", "AccountLoginModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page2", "AccountRegistrationModule", "AccountRegistrationModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page3", "AdvancedURLManagementModule", "AdvancedURLManagementModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page4", "AdvancedSettingsModule", "AdvancedSettingsModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page5", "BannersModule", "BannersModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page6", "CommerceModule", "CommerceModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page7", "ConfigurationManagerModule", "ConfigurationManagerModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page8", "ConsoleModule", "ConsoleModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page9", "ContentListModule", "ContentListModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page10", "DashboardModule", "DashboardModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page11", "DDRMenuModule", "DDRMenuModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page12", "DigitalAssetManagementModule", "DigitalAssetManagementModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page13", "DocumentLibraryModule", "DocumentLibraryModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page14", "ClientCapabilityProviderModule", "ClientCapabilityProviderModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page15", "ExtensionsModule", "ExtensionsModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page16", "FileIntegrityCheckerModule", "FileIntegrityCheckerModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page17", "GoogleAnalyticsModule", "GoogleAnalyticsModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page18", "GoogleAnalyticsProModule", "GoogleAnalyticsProModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page19", "HealthMonitoringModule", "HealthMonitoringModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page20", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page21", "JournalModule", "JournalModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page22", "LanguagesModule", "LanguagesModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page23", "LicenceActivationManagerModule", "LicenceActivationManagerModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page24", "ListsModule", "ListsModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page25", "LogViewerModule", "LogViewerModuleOnPage", "ContentPaneID")]
		public void Test007_DragDropModuleToContentPaneOnNewPage(string pageName, string moduleName, string moduleNameOnPage, string locationID)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag & Drop a new Module to Content Pane'");

			Modules module = new Modules(_driver);

			var fiModuleName = module.GetType().GetField(moduleName);
			moduleName = (string)fiModuleName.GetValue(null);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocationID = module.GetType().GetField(locationID);
			locationID = (string)fiLocationID.GetValue(null);

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();

			module.AddNewModuleUsingDragAndDrop(moduleName, moduleNameOnPage, locationID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationID + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page1", "AccountLoginModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page2", "AccountRegistrationModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page3", "AdvancedURLManagementModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page4", "AdvancedSettingsModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page5", "BannersModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page6", "CommerceModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page7", "ConfigurationManagerModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page8", "ConsoleModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page9", "ContentListModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page10", "DashboardModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page11", "DDRMenuModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page12", "DigitalAssetManagementModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page13", "DocumentLibraryModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page14", "ClientCapabilityProviderModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page15", "ExtensionsModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page16", "FileIntegrityCheckerModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page17", "GoogleAnalyticsModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page18", "GoogleAnalyticsProModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page19", "HealthMonitoringModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page20", "HtmlProModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page21", "JournalModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page22", "LanguagesModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page23", "LicenceActivationManagerModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page24", "ListsModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page25", "LogViewerModuleOnPage", "LeftPaneID")]
		public void Test008_DragAndDropModuleToLeftPane(string pageName, string moduleNameOnPage, string newLocationID)
		{
			Modules module = new Modules(_driver);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiNewLocationID = module.GetType().GetField(newLocationID);
			newLocationID = (string)fiNewLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag and drop a Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			blankPage.SetPageToEditMode(pageName);
			string moduleNumber = blankPage.WaitForElement(By.XPath(moduleNameOnPage + "/a")).GetAttribute("name");
			module.MoveModuleUsingDragAndDrop(moduleNumber, moduleNameOnPage, newLocationID);

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module new location: " + newLocationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(newLocationID + moduleNameOnPage)), "Module is not found");
		}

		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneLowerID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "SideBarPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftOuterPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "FooterCenterPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightPaneID")]
		[TestCase("Home/Page71", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightOuterPaneID")]
		public void Test009_DragAngDropHTMLModulesToAllPanesOnOnePage(string pageName, string moduleName, string moduleNameOnPage, string locationID)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag & Drop HTML Module'");

			Modules module = new Modules(_driver);

			var fiModuleName = module.GetType().GetField(moduleName);
			moduleName = (string)fiModuleName.GetValue(null);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocationID = module.GetType().GetField(locationID);
			locationID = (string)fiLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drad And Drop a new Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingDragAndDrop(moduleName, moduleNameOnPage, locationID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationID + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page62", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneID")]
		[TestCase("Home/Page63", "HtmlProModule", "HtmlProModuleOnPage", "LeftPaneID")]
		[TestCase("Home/Page64", "HtmlProModule", "HtmlProModuleOnPage", "ContentPaneLowerID")]
		[TestCase("Home/Page65", "HtmlProModule", "HtmlProModuleOnPage", "SideBarPaneID")]
		[TestCase("Home/Page66", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftOuterPaneID")]
		[TestCase("Home/Page67", "HtmlProModule", "HtmlProModuleOnPage", "FooterLeftPaneID")]
		[TestCase("Home/Page68", "HtmlProModule", "HtmlProModuleOnPage", "FooterCenterPaneID")]
		[TestCase("Home/Page69", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightPaneID")]
		[TestCase("Home/Page70", "HtmlProModule", "HtmlProModuleOnPage", "FooterRightOuterPaneID")]
		public void Test0010_DragAngDropHTMLModulesToAllPanesOnPage(string pageName, string moduleName, string moduleNameOnPage, string locationID)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag & Drop HTML Module'");

			Modules module = new Modules(_driver);

			var fiModuleName = module.GetType().GetField(moduleName);
			moduleName = (string)fiModuleName.GetValue(null);

			var fiModuleNameOnPage = module.GetType().GetField(moduleNameOnPage);
			moduleNameOnPage = (string)fiModuleNameOnPage.GetValue(null);

			var fiLocationID = module.GetType().GetField(locationID);
			locationID = (string)fiLocationID.GetValue(null);

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag And Drop a new Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingDragAndDrop(moduleName, moduleNameOnPage, locationID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationID + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationID + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page72")]
		public void Test0011_AddExistingModuleNoCopy(string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add Existing Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			Modules module = new Modules(_driver);
			module.OpenExistingModulePanelUsingControlPanel();

			blankPage.LoadableSelectByValue(By.XPath(Modules.PageDropDownId), "Home");
			blankPage.WaitForElement(By.XPath(Modules.List + "[1]"));

			module.AddNewModuleUsingMenu(Modules.ContactUsModule, Modules.HtmlProModuleOnPage, Modules.ContentPaneOption, Modules.ContentPaneID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + Modules.ContentPaneID + Modules.ContactUsModule);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(Modules.ContentPaneID + Modules.HtmlProModuleOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page72", "Terrible Cycles, Inc.")]
		public void Test0012_EditContentOfExistingModule(string pageName, string moduleContent)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit content to HTML Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageToEditMode(pageName);

			Modules module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.ContentPaneID + Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLProModule(moduleNumber, moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(moduleContent, Is.EqualTo(module.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]//div[contains(@id, 'lblContent')]")).Text));

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content changed on 'Home' page");
			Assert.That(moduleContent, Is.EqualTo(module.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]//div[contains(@id, 'lblContent')]")).Text));
		}

		[TestCase("Home/Page73")]
		public void Test0013_AddExistingModuleWithCopy(string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add Existing Module, Make a Copy'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			Modules module = new Modules(_driver);
			module.OpenExistingModulePanelUsingControlPanel();

			blankPage.LoadableSelectByValue(By.XPath(Modules.PageDropDownId), "Home");
			blankPage.WaitForElement(By.XPath(Modules.List + "[1]"));
			blankPage.CheckBoxCheck(By.XPath(Modules.MakeACopyCheckbox));

			module.AddNewModuleUsingMenu(Modules.ContactUsModule, Modules.HtmlProModuleOnPage, Modules.ContentPaneOption, Modules.ContentPaneID);
			string moduleNumber = blankPage.WaitForElement(By.XPath(Modules.ContentPaneID + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + Modules.ContentPaneID + Modules.ContactUsModule);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(Modules.ContentPaneID + Modules.HtmlProModuleOnPage + "/a[@name='" + moduleNumber + "']")), "Module is not found");
		}

		[TestCase("Home/Page73", "Awesome Cycles, Inc.")]
		public void Test0014_EditContentOfExistingModule(string pageName, string moduleContent)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit content to HTML Module'");

			Modules module = new Modules(_driver);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "Find the original module number on 'Home' page");
			string originalModuleNumber = mainPage.WaitForElement(By.XPath(Modules.FooterRightPaneID + "/div[last()]/a")).GetAttribute("name");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageToEditMode(pageName);

			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumberOnNewPage = module.WaitForElement(By.XPath(Modules.ContentPaneID + Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLProModule(moduleNumberOnNewPage, moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(moduleContent, Is.EqualTo(module.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumberOnNewPage + "')]//div[contains(@id, 'lblContent')]")).Text));

			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content changed on 'Home' page");
			Assert.That(moduleContent, Is.Not.EqualTo(module.WaitForElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + originalModuleNumber + "')]//div[contains(@id, 'lblContent')]")).Text));
		}
	}
}
