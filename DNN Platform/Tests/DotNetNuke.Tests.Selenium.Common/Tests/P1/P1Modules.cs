using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1Modules : CommonTestSteps
	{
		protected abstract string DataFileLocation { get; }

		protected abstract Modules.ModuleIDs HtmlModuleDictionary { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("modules");

			string testName = testSettings.Attribute("name").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			//Preconditions for
			/*var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.AddPagesInBulk("Page", 76, "Web", "Home");

			var blankPage = new BlankPage(_driver);

			var module = new Modules(_driver);

			int pageNumber = 1;
			while (pageNumber < 77)
			{
				blankPage.OpenUsingUrl(_baseUrl, "Home/Page" + pageNumber);
				blankPage.SetPageToEditMode("Home/Page" + pageNumber);

				string moduleNumber =
						module.WaitForElement(By.XPath(Modules.LocationDescription["ContentPane"].IdWhenOnPage +
													   HtmlModuleDictionary.IdWhenOnPage + "/a")).GetAttribute("name");

				module.DeleteModule(moduleNumber);
				pageNumber++;

			}

			blankPage.OpenUsingUrl(_baseUrl, "Home/Page61");
			module.OpenModulePanelUsingControlPanel();
			int i = 1;
			while (i < 5)
			{
				module.AddNewModuleUsingMenu(HtmlModuleDictionary.IdWhenOnBanner,
											HtmlModuleDictionary.IdWhenOnPage,
											"ContentPane");
				i++;
			}*/
		}

		//[TestFixtureTearDown]
		public void DeleteData()
		{
			OpenMainPageAndLoginAsHost();

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			int pageNumber = 1;

			while (pageNumber < 75)
			{
				adminPageManagementPage.OpenUsingButtons(_baseUrl);
				adminPageManagementPage.DeletePage("Page" + pageNumber, "Web");

				pageNumber++;
			}
		}

		public void AddModuleToContentPaneOnNewPage(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName, string location)
		{
			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, location);
			string moduleNumber =
				blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + location + moduleName);
			Assert.IsTrue(
				blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void AddHTMLModuleToPaneOnPage(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName,
													  string location)
		{
			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new HTML Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, location);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + location + moduleName);
			Assert.IsTrue(
				blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void AddHTMLModuleToToAllPanesOnOnePage(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName,
															   string location)
		{
			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, location);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + location + moduleName);
			Assert.IsTrue(
				blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void MoveModuleToLeftPane(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName, string newLocation)
		{
			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription[newLocation].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			blankPage.SetPageToEditMode(pageName);
			string moduleNumber = blankPage.WaitForElement(By.XPath(moduleNameOnPage + "/a")).GetAttribute("name");
			module.MoveModuleUsingMenu(moduleNumber, moduleNameOnPage, newLocation);

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module new location: " + locationOnPage + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage)), "Module is not found");
		}

		public void MoveHTMLModuleWithinContentPaneToTop (string moduleIdOnPage, string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module to TOP'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			string contentPaneOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			var module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber4, moduleIdOnPage, "Top");

			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber4), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber1), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber3), "Module is not found");
		}

		public void MoveHTMLModuleWithinContentPaneToBottom(string moduleIdOnPage, string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module to BOTTOM'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			string contentPaneOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			var module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber1, moduleIdOnPage, "Bottom");

			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber3), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber4), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber1), "Module is not found");
		}

		public void MoveHTMLModuleWithinContentPaneUp(string moduleIdOnPage, string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module UP'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			string contentPaneOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			var module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber3, moduleIdOnPage, "Up");

			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber1), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber3), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber4), "Module is not found");
		}

		public void MoveHTMLModuleWithinContentPaneDown(string moduleIdOnPage, string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a HTML Module DOWN'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			string contentPaneOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			var module = new Modules(_driver);
			module.SetPageToEditMode(pageName);
			string moduleNumber1 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name");
			string moduleNumber2 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name");
			string moduleNumber3 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name");
			string moduleNumber4 = blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name");

			module.MoveModuleUsingMenu(moduleNumber2, moduleIdOnPage, "Down");

			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][1]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber1), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][2]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber3), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][3]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber2), "Module is not found");
			Assert.That(blankPage.WaitForElement(By.XPath(contentPaneOnPage + "/div[contains(@class, 'DnnModule')][4]/a")).GetAttribute("name"),
						Is.EqualTo(moduleNumber4), "Module is not found");
		}

		public void DragDropModuleToContentPaneOnNewPage(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName,
																 string location)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag & Drop a new Module to Content Pane'");

			var module = new Modules(_driver);

			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;
			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();

			module.AddNewModuleUsingDragAndDrop(moduleNameOnBanner, location);
			string moduleNumber =
				blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + location + moduleNameOnPage);
			Assert.IsTrue(
				blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void DragAndDropModuleToLeftPane(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName, string newLocation)
		{
			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription[newLocation].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag and drop a Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			blankPage.SetPageToEditMode(pageName);
			string moduleNumber = blankPage.WaitForElement(By.XPath(moduleNameOnPage + "/a")).GetAttribute("name");
			module.MoveModuleUsingDragAndDrop(moduleNumber, locationOnPage);

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module new location: " + locationOnPage + moduleNameOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage)), "Module is not found");
		}

		public void DragAngDropHTMLModulesToAllPanesOnOnePage(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName, string location)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag & Drop HTML Module'");

			var module = new Modules(_driver);

			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;
			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drad And Drop a new Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingDragAndDrop(moduleNameOnBanner, location);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + location + moduleNameOnPage);
			Assert.IsTrue(
				blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void DragAngDropHTMLModulesToAllPanesOnPage(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName,
		                                                            string location)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag & Drop HTML Module'");

			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Drag And Drop a new Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingDragAndDrop(moduleNameOnBanner, location);
			string moduleNumber = blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationOnPage + moduleNameOnPage);
			Assert.IsTrue(
				blankPage.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void AddExistingModuleNoCopy(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add Existing Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			var module = new Modules(_driver);
			module.OpenExistingModulePanelUsingControlPanel();

			string moduleNameOnPage = modulesDescription["ContactUsModule"].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription["ContactUsModule"].IdWhenOnBanner;
			string locationOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			blankPage.FolderSelectByValue(By.XPath(Modules.PageDropDownId), "Home");
			blankPage.WaitForElement(By.XPath(Modules.List + "[1]"));

			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, "ContentPane");
			string moduleNumber =
				blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationOnPage +
							moduleNameOnPage);
			Assert.IsTrue(
				blankPage.ElementPresent(
					By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void EditContentOfExistingModuleWithoutCopy(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleContent)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit content to HTML Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageToEditMode(pageName);

			string moduleNameOnPage = modulesDescription["ContactUsModule"].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			var module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(locationOnPage + moduleNameOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLModule(moduleNumber, moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(moduleContent,
						Is.EqualTo(
							module.FindElement(
								By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber +
										 "')]//div[contains(@id, 'lblContent')]")).Text));

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content changed on 'Home' page");
			Assert.That(moduleContent,
						Is.EqualTo(
							module.FindElement(
								By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber +
										 "')]//div[contains(@id, 'lblContent')]")).Text));
		}

		public void AddExistingModuleWithCopy(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add Existing Module, Make a Copy'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);

			var module = new Modules(_driver);
			module.OpenExistingModulePanelUsingControlPanel();

			string moduleNameOnPage = modulesDescription["ContactUsModule"].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription["ContactUsModule"].IdWhenOnBanner;
			string locationOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			blankPage.FolderSelectByValue(By.XPath(Modules.PageDropDownId), "Home");
			blankPage.WaitForElement(By.XPath(Modules.List + "[1]"));
			blankPage.CheckBoxCheck(By.XPath(Modules.MakeACopyCheckbox));

			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, "ContentPane");
			string moduleNumber =
				blankPage.WaitForElement(By.XPath(locationOnPage + "/div[last()]/a")).GetAttribute("name");

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + locationOnPage +
							moduleNameOnPage);
			Assert.IsTrue(
				blankPage.ElementPresent(
					By.XPath(locationOnPage + moduleNameOnPage + "/a[@name='" + moduleNumber + "']")),
				"Module is not found");
		}

		public void EditContentOfExistingModuleWithCopy(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleContent)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit content to HTML Module'");

			var module = new Modules(_driver);

			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			string moduleNameOnPage = modulesDescription["ContactUsModule"].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			Trace.WriteLine(BasePage.TraceLevelPage + "Find the original module number on 'Home' page");
			string originalModuleNumber =
				mainPage.WaitForElement(By.XPath(Modules.LocationDescription["FooterRightPane"].IdWhenOnPage + "/div[last()]/a")).GetAttribute("name");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageToEditMode(pageName);

			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumberOnNewPage =
				module.WaitForElement(By.XPath(locationOnPage + moduleNameOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLModule(moduleNumberOnNewPage, moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(moduleContent,
			            Is.EqualTo(
				            module.FindElement(
					            By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumberOnNewPage +
					                     "')]//div[contains(@id, 'lblContent')]")).Text));

			mainPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content changed on 'Home' page");
			Assert.That(moduleContent,
			            Is.Not.EqualTo(
				            module.WaitForElement(
					            By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + originalModuleNumber +
					                     "')]//div[contains(@id, 'lblContent')]")).Text));
		
		}

	}
}