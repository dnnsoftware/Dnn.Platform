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
	[TestFixture("TestPageBVT", "Page test string, BVT", "Page Title")]
	[Category("BVT")]
	public class BVTPages : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		private string _pageName = null;
		private string _pageContent = null;
		private string _pageTitle = null;

		public BVTPages(string pageName, string pageContent, string pageTitle)
		{
			this._pageName = pageName;
			this._pageContent = pageContent;
			this._pageTitle = pageTitle;
		}

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Pages BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");
		}

		[Test]
		public void Test001_AddPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Page'");

			BlankPage blankPage = new BlankPage(_driver);

			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);

			blankPage.AddNewPage(_pageName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the user redirected to newly created page: " + "http://" + _baseUrl.ToLower() + "/" + _pageName);
			Assert.That(blankPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _pageName),
				"The page URL is not correct");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + " is present in the list");
			Assert.IsTrue(adminPageManagementPage.ElementPresent(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[@class= 'rtLI']//span[contains(text(), '" + _pageName + "')]")), 
				"The page is not present in the list");
		}

		[Test]
		[Category(@"EEandPEpackage")]
		public void Test002_EditDefaultHTMLModuleOnPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Default HTML Module on the Page'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);
			blankPage.SetPageToEditMode(_pageName);

			Modules module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLProModule(moduleNumber, _pageContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module content is present on the screen");
			Assert.That(blankPage.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-')]//div[contains(@id, 'lblContent')]")).Text, Is.EqualTo(_pageContent));
		}

		[Test]
		[Category(@"CEpackage")]
		public void Test003_EditDefaultHTMLModuleOnPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Default HTML Module on the Page'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);
			blankPage.SetPageToEditMode(_pageName);

			Modules module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.HtmlProModuleOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLModule(moduleNumber, _pageContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module content is present on the screen");
			Assert.That(blankPage.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-')]//div[contains(@id, 'lblContent')]")).Text, Is.EqualTo(_pageContent));
		}

		[Test]
		public void Test004_EditPageSettings()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Page Settings'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);
			blankPage.SelectMenuOption(BasePage.ControlPanelEditPageOption, BasePage.PageSettingsOption);

			blankPage.EditPageTitle(_pageTitle);

			Trace.WriteLine("Verify Page title: '" + _pageTitle + "'");
			StringAssert.Contains(_pageTitle, blankPage.CurrentWindowTitle(), "The Page title is not correct");
		}

		[Test]
		public void Test005_DeletePage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			blankPage.DeletePage(_pageName);

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the list");
			Assert.IsFalse(adminPageManagementPage.ElementPresent(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[@class= 'rtLI']//span[contains(text(), '" + _pageName + "')]")),
				"The page is present in the list");

			AdminRecycleBinPage adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in Recycle Bin");
			Assert.IsTrue(adminRecycleBinPage.ElementPresent(By.XPath(AdminRecycleBinPage.RecycleBinPageContainerOption + "[contains(text(), '" + _pageName + "')]")));
		}

		//[Test]
		public void Test006_RemovePageFromRecycleBin()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the Page from Recycling Bin'");

			AdminRecycleBinPage adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			adminRecycleBinPage.RemovePage(_pageName);

			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in Recycle Bin");
			Assert.IsFalse(adminRecycleBinPage.ElementPresent(By.XPath(AdminRecycleBinPage.RecycleBinPageContainerOption + "[contains(text(), '" + _pageName + "')]")));
		}
	}
}
