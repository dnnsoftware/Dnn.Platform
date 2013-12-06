using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.DemoPages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1Pages : CommonTestSteps
	{
		private string _pageName;
		private string _pageDescription;

		private string _pageName1;
		private string _pageName2;
		private string _pageName3;
		private string _pageName4;

		private string _addWebAfter;
		private string _addHostAfter;

		private string _moveAfterWebPage;
		private string _moveAfterHostPage;

		private string _templateName;
		private string _templateDescription;

		private string _importedPageName;
		private string _insertPageAfter;

		private string _copiedPageName;
		private string _parentPageName;
		private string _copyFromPage;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("page");

			string testName = testSettings.Attribute("name").Value;
			_pageName = testSettings.Attribute("pageName").Value;
			_pageDescription = testSettings.Attribute("pageDescription").Value;

			_pageName1 = testSettings.Attribute("pageName1").Value;
			_pageName2 = testSettings.Attribute("pageName2").Value;
			_pageName3 = testSettings.Attribute("pageName3").Value;
			_pageName4 = testSettings.Attribute("pageName4").Value;

			_addWebAfter = testSettings.Attribute("addWebAfter").Value;
			_addHostAfter = testSettings.Attribute("addHostAfter").Value;

			_moveAfterWebPage = testSettings.Attribute("moveAfterWebPage").Value;
			_moveAfterHostPage = testSettings.Attribute("moveAfterHostPage").Value;

			_templateName = testSettings.Attribute("templateName").Value;
			_templateDescription = testSettings.Attribute("templateDescription").Value;

			_importedPageName = testSettings.Attribute("importedPageName").Value;
			_insertPageAfter = testSettings.Attribute("insertPageAfter").Value;

			_copiedPageName = testSettings.Attribute("copiedPageName").Value;
			_parentPageName = testSettings.Attribute("parentPageName").Value;
			_copyFromPage = testSettings.Attribute("copyFromPage").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		[Test]
		public void Test001_AddWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Website Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPage(_pageName, "Web", _addWebAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addWebAfter +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page is not present in the list");
		}

		[Test]
		public void Test002_AddWebMultiplePages()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add new Website Pages'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPages(_pageName1, _pageName2, _pageName3, _pageName4, "Web", _addWebAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName1 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addWebAfter +
					         " ']]//div/span[text() = '" + _pageName1 + " ']")),
				"The page " + _pageName1 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName2 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addWebAfter +
					         " ']]//div/span[text() = '" + _pageName2 + " ']")),
				"The page " + _pageName2 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName3 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" +
					         _pageName3 + " ']")),
				"The page " + _pageName3 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName4 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" +
					         _pageName4 + " ']")),
				"The page " + _pageName4 + " is not present in the list");
		}

		[Test]
		public void Test003_EditWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit a Web Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddDescriptionToPage(_pageName, _pageDescription, "Web");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _addWebAfter + "/" + _pageName);
			blankPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page description: " + _pageName1 + "is saved correctly");
			Assert.That(_pageDescription, Is.EqualTo(blankPage.WaitForElement(By.XPath(BlankPage.PageDescriptionTextBox)).Text),
			            "The page description is not added correctly");
		}

		[Test]
		public void Test004_MoveWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.MovePage(_pageName, _moveAfterWebPage, "Web");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the old location");
			Assert.IsFalse(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addWebAfter +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page " + _pageName + " is present in the list after Page '" + _addWebAfter + "'");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the new location");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _moveAfterWebPage +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page " + _pageName + " is not present in the list");
		}


		[Test]
		public void Test005_DeleteWebPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.DeletePage(_pageName, "Web");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is  NOT present in the list");
			Assert.IsFalse(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addWebAfter +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page " + _pageName + " is present in the list");
		}


		[Test]
		public void Test006_AddHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Host Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPage(_pageName, "Host", _addHostAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.RadioButtonSelect(By.XPath(AdminPageManagementPage.HostPagesRadioButton));
			adminPageManagementPage.WaitForElement(By.XPath(AdminPageManagementPage.PageList + "//span[text() = 'Host ']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addHostAfter +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page is not present in the list");
		}

		[Test]
		public void Test007_AddHostMultiplePages()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add new Host Pages'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddPages(_pageName1, _pageName2, _pageName3, _pageName4, "Host", _addHostAfter);

			adminPageManagementPage.OpenUsingButtons(_baseUrl);
			adminPageManagementPage.RadioButtonSelect(By.XPath(AdminPageManagementPage.HostPagesRadioButton));
			adminPageManagementPage.WaitForElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = 'Host ']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName1 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addHostAfter +
					         " ']]//div/span[text() = '" + _pageName1 + " ']")),
				"The page " + _pageName1 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName2 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addHostAfter +
					         " ']]//div/span[text() = '" + _pageName2 + " ']")),
				"The page " + _pageName2 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName3 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" +
					         _pageName3 + " ']")),
				"The page " + _pageName3 + " is not present in the list");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName4 + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _pageName2 + " ']]//div/span[text() = '" +
					         _pageName4 + " ']")),
				"The page " + _pageName4 + " is not present in the list");
		}


		[Test]
		public void Test008_EditHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit a Host Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.AddDescriptionToPage(_pageName, _pageDescription, "Host");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, "Host/" + _addHostAfter + "/" + _pageName);
			blankPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page description: " + _pageName1 + "is saved correctly");
			Assert.That(_pageDescription, Is.EqualTo(blankPage.WaitForElement(By.XPath(BlankPage.PageDescriptionTextBox)).Text),
			            "The page description is not added correctly");
		}

		[Test]
		public void Test009_MoveHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Move a Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.MovePage(_pageName, _moveAfterHostPage, "Host");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the old location");
			Assert.IsFalse(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addHostAfter +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page " + _pageName + " is present in the list after Page '" + _addHostAfter + "'");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in the new location");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _moveAfterHostPage +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page " + _pageName + " is not present in the list");
		}


		[Test]
		public void Test010_DeleteHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			adminPageManagementPage.DeletePage(_pageName, "Host");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is  NOT present in the list");
			Assert.IsFalse(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _addHostAfter +
					         " ']]//div/span[text() = '" + _pageName + " ']")),
				"The page " + _pageName + " is present in the list");
		}


		[Test]
		public void Test011_CopyPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Copy a Page'");
			var blankPage = new BlankPage(_driver);

			blankPage.OpenCopyPageFrameUsingControlPanel(_baseUrl);
			blankPage.CopyPage(_copiedPageName, _parentPageName, _copyFromPage);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the user redirected to newly created page: " + "http://" +
			                _baseUrl.ToLower() + "/" + _parentPageName + "/" + _copiedPageName);
			Assert.That(blankPage.CurrentWindowUrl(),
			            Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _parentPageName + "/" + _copiedPageName),
			            "The page URL is not correct");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT 3 new modules added.");
			Assert.That(blankPage.FindElements(By.XPath("//li[@class = 'actionMenuMove']")).Count, Is.EqualTo(3),
			            "The Modules are not added correctly");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _copiedPageName + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath(AdminPageManagementPage.PageList + "//li[div/span[text() = '" + _parentPageName +
					         " ']]//div/span[text() = '" + _copiedPageName + " ']")),
				"The page is not present in the list");
		}

		[Test]
		public void Test012_ExportPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Export a Page'");

			var pageExportPage = new PageExportPage(_driver);
			pageExportPage.OpenUsingControlPanel(_baseUrl + ContactUsPage.ContactUsUrl);

			pageExportPage.ExportPage(_templateName, _templateDescription);

			var pageImportPage = new PageImportPage(_driver);
			pageImportPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Export Template is present in the Import dropdown");
			Assert.IsTrue(
				pageImportPage.ElementPresent(By.XPath(PageImportPage.TemplateDropDownId + "//li[text() = '" + _templateName + "']")),
				"Template Name is not present in the list for Import");
		}

		[Test]
		public void Test013_ImportPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Import a Page'");

			var pageImportPage = new PageImportPage(_driver);
			pageImportPage.OpenUsingControlPanel(_baseUrl);

			pageImportPage.ImportPage(_templateName, _importedPageName, _insertPageAfter);

			//Assert.That(pageImportPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _importedPageName),
			//					"The page URL is not correct");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _importedPageName + "is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath("//li[div/span[text() = '" + _insertPageAfter + " ']]/following-sibling::li[div/span[text() = '" +
					         _importedPageName + " ']]")),
				"The page is not present in the list");
		}
	}
}