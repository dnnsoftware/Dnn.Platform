using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.BVT
{
	[TestFixture("BVTPageForModuleTesting", "Module test string, BVT", "Module Title")]
	[Category("BVT")]
	public class BVTModules : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _pageName = null;
		private string _moduleContent = null;
		private string _moduleTitle = null;

		public BVTModules(string pageName, string moduleContent, string moduleTitle)
		{
			this._pageName = pageName;
			this._moduleContent = moduleContent;
			this._moduleTitle = moduleTitle;
		}

		[TestFixtureSetUp]
		public void LoginToSite() 
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Modules BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);
			blankPage.AddNewPage(_pageName);
		}

		[Test]
		[Category(@"EEandPEpackage")]
		public void Test001_AddModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			Modules module = new Modules(_driver);
			module.OpenModulePanelUsingControlPanel();

			module.AddNewModuleUsingDragAndDrop(Modules.HtmlProModule, Modules.HtmlProModuleOnPage, Modules.LeftPaneID);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + Modules.LeftPaneID + Modules.HtmlProModuleOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(Modules.LeftPaneID + Modules.HtmlProModuleOnPage)), "Module is not found");
		}

		[Test]
		[Category(@"CEpackage")]
		public void Test002_AddModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			Modules module = new Modules(_driver);
			module.OpenModulePanelUsingControlPanel();

			module.AddNewModuleUsingDragAndDrop(Modules.HtmlModule, Modules.HtmlModuleOnPage, Modules.LeftPaneID);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + Modules.LeftPaneID + Modules.HtmlProModuleOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(Modules.LeftPaneID + Modules.HtmlProModuleOnPage)), "Module is not found");
		}

		[Test]
		[Category(@"EEandPEpackage")]
		public void Test003_EditModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add content to HTML Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			Modules module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.LeftPaneID + Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLProModule(moduleNumber, _moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(_moduleContent, Is.EqualTo(module.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]//div[contains(@id, 'lblContent')]")).Text));
		}

		[Test]
		[Category(@"CEpackage")]
		public void Test004_EditModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add content to HTML Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			Modules module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.LeftPaneID + Modules.HtmlModuleOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLModule(moduleNumber, _moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(_moduleContent, Is.EqualTo(module.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]//div[contains(@id, 'lblContent')]")).Text));
		}

		[Test]
		public void Test005_EditModuleSettings()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Module settings'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			Modules module = new Modules(_driver);

			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.LeftPaneID + Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");
			module.EditModuleSettings(moduleNumber, _moduleTitle);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT a new Module Title is present on the page");
			StringAssert.Contains(_moduleTitle.ToUpper(), blankPage.WaitForElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]//span[contains(@id, '" + BasePage.PageTitle + "')]")).Text.ToUpper(),
						"The  new Module Title is not saved correctly");
		}

		[Test]
		public void Test006_DeleteModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Module'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			Modules module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.LeftPaneID + Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");
			module.DeleteModule(moduleNumber);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module " + moduleNumber + " deleted");
			Assert.IsFalse(module.ElementPresent(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]")),
					"The Module is not deleted correctly");
		}

		//[Test]
		public void Test007_RemoveModuleFromRecycleBin()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the Module from Recycling Bin'");

			AdminRecycleBinPage adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			adminRecycleBinPage.RemoveModule(_moduleTitle);

			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module: " + _moduleTitle + " is NOT present in Recycle Bin");
			Assert.IsFalse(adminRecycleBinPage.ElementPresent(By.XPath(AdminRecycleBinPage.RecycleBinModuleContainerOption + "[contains(text(), '" + _moduleTitle + "')]")));
		}
	}
}
