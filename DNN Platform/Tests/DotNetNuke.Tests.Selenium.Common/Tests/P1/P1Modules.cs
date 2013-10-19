using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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

			//Create a Page
			var blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);
			blankPage.AddNewPage("page");

			//Delete Default HTML Module on page
			blankPage.OpenUsingUrl(_baseUrl,"/page");
			var module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(Modules.LocationDescription["ContentPane"].IdWhenOnPage + Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage + "/a")).GetAttribute("name");
			module.DeleteModule(moduleNumber);

			//Export new page as Default
			var pageExportPage = new PageExportPage(_driver);
			pageExportPage.OpenUsingControlPanel(_baseUrl + "/page");
			pageExportPage.ExportPage("Default", "NewDefault");

			//Create set of pages with New Default
			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.AddPagesInBulk("Page", 76, "Web", "Home");
			
			//Create a page with 4 HtmlModuleDictionary Modules on page
			blankPage.OpenUsingUrl(_baseUrl, "Home/Page61");
			module.OpenModulePanelUsingControlPanel();
			int i = 1;
			while (i < 5)
			{
				module.AddNewModuleUsingMenu(HtmlModuleDictionary.IdWhenOnBanner, HtmlModuleDictionary.IdWhenOnPage, "ContentPane");
				i++;
			}
		}

		//[TestFixtureTearDown]
		public void DeleteData()
		{
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

			blankPage.SetPageToEditMode();
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
			module.SetPageToEditMode();
			
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
			module.SetPageToEditMode();
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
			module.SetPageToEditMode();
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
			module.SetPageToEditMode();
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

			blankPage.SetPageToEditMode();
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

		public void GetHTMLModule(string assyName, string moduleClassName, string moduleNumber, string moduleContent)
		{
			string fullAssyName = "DNNSelenium." + assyName;
			Type moduleClassType = Type.GetType(fullAssyName + "." + moduleClassName + ", " + fullAssyName);
			object module = Activator.CreateInstance(moduleClassType, new object[] { _driver });

			MethodInfo miOpen = moduleClassType.GetMethod("AddContentToHTMLModule");
			if (miOpen != null)
			{
				miOpen.Invoke(module, new object[] { moduleNumber, moduleContent });
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + "AddContentToHTMLModule" + "for class " + moduleClassName);
			}

		}

		public void EditContentOfExistingModuleWithoutCopy(string assyName, string moduleClassName, Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleContent)
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit content to HTML Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageToEditMode();

			string moduleNameOnPage = modulesDescription["ContactUsModule"].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription["ContentPane"].IdWhenOnPage;

			var module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(locationOnPage + moduleNameOnPage + "/a")).GetAttribute("name");

			//module.AddContentToHTMLModule(moduleNumber, moduleContent);
			GetHTMLModule (assyName, moduleClassName, moduleNumber, moduleContent);

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

		public void EditContentOfExistingModuleWithCopy(string assyName, string moduleClassName, Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleContent)
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
			blankPage.SetPageToEditMode();

			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumberOnNewPage =
				module.WaitForElement(By.XPath(locationOnPage + moduleNameOnPage + "/a")).GetAttribute("name");

			GetHTMLModule(assyName, moduleClassName, moduleNumberOnNewPage, moduleContent);

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

		[TestCase("Common", "CorePages.AdminAdvancedSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminDevicePreviewManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminExtensionsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminFileManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminLanguagesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminListsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminNewslettersPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminPageManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminRecycleBinPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSearchAdminPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSearchEnginePage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSecurityRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteRedirectionManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSiteWizardPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminSkinsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminTaxonomyPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminUserAccountsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.AdminVendorsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostConfigurationManagerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostDeviceDetectionManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostFileManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostHtmlEditorManagerPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostListsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSchedulePage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSettingsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSiteManagementPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSqlPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostSuperUserAccountsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.HostVendorsPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageRolesPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenUsingControlPanel")]
		public void Test0015_UpdateModuleSettings(string assyName, string pageClassName, string openMethod)
		{
			UpdateModuleSettings(assyName, pageClassName, openMethod, Modules.CommonModulesDescription);
		}

		public void UpdateModuleSettings(string assyName, string pageClassName, string openMethod, Dictionary<string, Modules.ModuleIDs> modulesDescription)
		{
			BasePage currentPage = OpenPage(assyName, pageClassName, openMethod);

			currentPage.SetPageToEditMode();

			var module = new Modules(_driver);

			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(Modules.LocationDescription["ContentPane"].IdWhenOnPage +
												modulesDescription[currentPage.PreLoadedModule].IdWhenOnPage + "/a")).GetAttribute("name");
			module.ChangeModuleTitle(moduleNumber, "Module Updated Name");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT a new Module Title is present on the page");
			StringAssert.Contains("Module Updated Name".ToUpper(),
								  currentPage.WaitForElement(
									  By.XPath(modulesDescription[currentPage.PreLoadedModule].IdWhenOnPage + "//span[contains(@id, '_dnnTITLE_titleLabel')]")).Text.ToUpper(),
								  "The new Module Title is not saved correctly");
		}

		public void DeleteModule(Dictionary<string, Modules.ModuleIDs> modulesDescription, string pageName, string moduleName,
													   string location)
		{
			var module = new Modules(_driver);

			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string locationOnPage = Modules.LocationDescription[location].IdWhenOnPage;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageToEditMode();

			string moduleNumber =
				module.WaitForElement(By.XPath(locationOnPage + moduleNameOnPage + "/a")).GetAttribute("name");

			module.DeleteModule(moduleNumber);

			blankPage.OpenUsingUrl(_baseUrl, pageName);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module " + moduleNumber + " deleted");
			Assert.IsFalse(module.ElementPresent(By.XPath(locationOnPage + moduleNameOnPage + "/a[@name = '" + moduleNumber + "']")),
						   "The Module is not deleted correctly");
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
		public void Test016_DeleteModule(string pageName, string moduleName, string location)
		{
			DeleteModule(Modules.CommonModulesDescription, pageName, moduleName, location);
		}
	}
}