using System.Diagnostics;
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
	public class P1Pages : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;
		private string _pageName = null;
		private string _pageDescription = null;

		private string _pageName1 = null;
		private string _pageName2 = null;
		private string _pageName3 = null;
		private string _pageName4 = null;

		private string _addWebAfter = null;
		private string _addHostAfter = null;

		private string _moveAfterWebPage = null;
		private string _moveAfterHostPage = null;

		private string _templateName = null;
		private string _templateDescription = null;

		private string _importedPageName = null;
		private string _insertPageAfter = null;

		private string _copiedPageName = null;
		private string _parentPageName = null;
		private string _copyFromPage = null;

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("page");

			string testName = pageSettings.Attribute("name").Value;
			_pageName = pageSettings.Attribute("pageName").Value;
			_pageDescription = pageSettings.Attribute("pageDescription").Value;

			_pageName1 = pageSettings.Attribute("pageName1").Value;
			_pageName2 = pageSettings.Attribute("pageName2").Value;
			_pageName3 = pageSettings.Attribute("pageName3").Value;
			_pageName4 = pageSettings.Attribute("pageName4").Value;

			_addWebAfter = pageSettings.Attribute("addWebAfter").Value;
			_addHostAfter = pageSettings.Attribute("addHostAfter").Value;

			_moveAfterWebPage = pageSettings.Attribute("moveAfterWebPage").Value;
			_moveAfterHostPage = pageSettings.Attribute("moveAfterHostPage").Value;

			_templateName = pageSettings.Attribute("templateName").Value;
			_templateDescription = pageSettings.Attribute("templateDescription").Value;

			_importedPageName = pageSettings.Attribute("importedPageName").Value;
			_insertPageAfter = pageSettings.Attribute("insertPageAfter").Value;

			_copiedPageName = pageSettings.Attribute("copiedPageName").Value;
			_parentPageName = pageSettings.Attribute("parentPageName").Value;
			_copyFromPage = pageSettings.Attribute("copyFromPage").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			_loginPage = new LoginPage(_driver);
			_loginPage.OpenUsingUrl(_baseUrl);
			_loginPage.DoLoginUsingUrl("host", "dnnhost");

		}

		[Test]
		public void Test001_AddWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Website Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPage(_pageName, "Web", _addWebAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addWebAfter + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page is not present in the list");
		}

		[Test]
		public void Test002_AddWebMultiplePages()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add new Website Pages'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPages(_pageName1, _pageName2, _pageName3, _pageName4, "Web", _addWebAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName1 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addWebAfter + " ']]//div/span[text() = '" + _pageName1 + " ']")),
					"The page " + _pageName1 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName2 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addWebAfter + " ']]//div/span[text() = '" + _pageName2 + " ']")),
					"The page " + _pageName2 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName3 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" + _pageName3 + " ']")),
					"The page " + _pageName3 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName4 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" + _pageName4 + " ']")),
					"The page " + _pageName4 + " is not present in the list");
		}

		[Test]
		public void Test003_EditWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit a Web Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddDescriptionToPage(_pageName, _pageDescription, "Web");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _addWebAfter + "/" + _pageName);
			blankPage.SelectMenuOption(BasePage.ControlPanelEditPageOption, BasePage.PageSettingsOption);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page description: " + _pageName1 + "is saved correctly");
			Assert.That(_pageDescription, Is.EqualTo(blankPage.WaitForElement(By.XPath(BlankPage.PageDescriptionTextBox)).Text),
							"The page description is not added correctly");
		}

		[Test]
		public void Test004_MoveWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.MovePage(_pageName, _moveAfterWebPage, "Web");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the old location");
			Assert.IsFalse(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addWebAfter + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page " + _pageName + " is present in the list after Page '" + _addWebAfter + "'");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the new location");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _moveAfterWebPage + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page " + _pageName + " is not present in the list");
		}


		[Test]
		public void Test005_DeleteWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.DeletePage(_pageName, "Web");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is  NOT present in the list");
			Assert.IsFalse(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addWebAfter + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page " + _pageName + " is present in the list");
		}



		[Test]
		public void Test006_AddHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Host Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPage(_pageName, "Host", _addHostAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.RadioButtonSelect(By.XPath(AdminPageManagementPage.HostPagesRadioButton));
			adminPageManagementPage.WaitForElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = 'Host ']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addHostAfter + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page is not present in the list");
		}

		[Test]
		public void Test007_AddHostMultiplePages()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add new Host Pages'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPages(_pageName1, _pageName2, _pageName3, _pageName4, "Host", _addHostAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.RadioButtonSelect(By.XPath(AdminPageManagementPage.HostPagesRadioButton));
			adminPageManagementPage.WaitForElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = 'Host ']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName1 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addHostAfter + " ']]//div/span[text() = '" + _pageName1 + " ']")),
					"The page " + _pageName1 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName2 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addHostAfter + " ']]//div/span[text() = '" + _pageName2 + " ']")),
					"The page " + _pageName2 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName3 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" + _pageName3 + " ']")),
					"The page " + _pageName3 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName4 + "is present in the list");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" + _pageName4 + " ']")),
					"The page " + _pageName4 + " is not present in the list");
		}


		[Test]
		public void Test008_EditHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit a Host Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddDescriptionToPage(_pageName, _pageDescription, "Host");

			BlankPage blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, "Host/" + _addHostAfter + "/" + _pageName);
			blankPage.SelectMenuOption(BasePage.ControlPanelEditPageOption, BasePage.PageSettingsOption);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page description: " + _pageName1 + "is saved correctly");
			Assert.That(_pageDescription, Is.EqualTo(blankPage.WaitForElement(By.XPath(BlankPage.PageDescriptionTextBox)).Text),
							"The page description is not added correctly");
		}

		[Test]
		public void Test009_MoveHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.MovePage(_pageName, _moveAfterHostPage, "Host");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the old location");
			Assert.IsFalse(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addHostAfter + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page " + _pageName + " is present in the list after Page '" + _addHostAfter + "'");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the new location");
			Assert.IsTrue(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _moveAfterHostPage + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page " + _pageName + " is not present in the list");
		}


		[Test]
		public void Test010_DeleteHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.DeletePage(_pageName, "Host");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is  NOT present in the list");
			Assert.IsFalse(
					adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _addHostAfter + " ']]//div/span[text() = '" + _pageName + " ']")),
					"The page " + _pageName + " is present in the list");
		}


		[Test]
		public void Test011_CopyPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Copy a Page'");
			BlankPage blankPage = new BlankPage(_driver);

			blankPage.OpenCopyPageFrameUsingControlPanel(_baseUrl);
			blankPage.CopyPage(_copiedPageName, _parentPageName, _copyFromPage);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the user redirected to newly created page: " + "http://" + _baseUrl.ToLower() + "/" + _parentPageName + "/" + _copiedPageName);
			Assert.That(blankPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _parentPageName + "/" + _copiedPageName),
								"The page URL is not correct");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT 3 new modules added.");
			Assert.That(blankPage.FindElements(By.XPath("//li[@class = 'actionMenuMove']")).Count, Is.EqualTo(3),
					"The Modules are not added correctly");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _copiedPageName + "is present in the list");
			Assert.IsTrue(
							adminPageManagementPage.ElementPresent(
							By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[div/span[text() = '" + _parentPageName + " ']]//div/span[text() = '" + _copiedPageName + " ']")),
							"The page is not present in the list");
		}

		[Test]
		public void Test012_ExportPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Export a Page'");

			PageExportPage pageExportPage = new PageExportPage(_driver);
			pageExportPage.OpenUsingControlPanel(_baseUrl + ContactUsPage.ContactUsUrl);

			pageExportPage.ExportPage(_templateName, _templateDescription);

			PageImportPage pageImportPage = new PageImportPage(_driver);
			pageImportPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Export Template is present in the Import dropdown");
			Assert.IsTrue(pageImportPage.ElementPresent(By.XPath(PageImportPage.TemplateDropDownId + "//li[text() = '" + _templateName + "']")),
						"Template Name is not present in the list for Import");
		}

		[Test]
		public void Test013_ImportPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Import a Page'");

			PageImportPage pageImportPage = new PageImportPage(_driver);
			pageImportPage.OpenUsingControlPanel(_baseUrl);

			pageImportPage.ImportPage(_templateName, _importedPageName, _insertPageAfter);

			//Assert.That(pageImportPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _importedPageName),
			//					"The page URL is not correct");

			AdminPageManagementPage adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _importedPageName + "is present in the list");
			Assert.IsTrue(
							adminPageManagementPage.ElementPresent(
							By.XPath("//li[div/span[text() = '" + _insertPageAfter + " ']]/following-sibling::li[div/span[text() = '" + _importedPageName + " ']]")),
							"The page is not present in the list");
		}
	}
}