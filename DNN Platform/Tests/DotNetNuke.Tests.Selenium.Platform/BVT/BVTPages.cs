using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.BVT
{
	[TestFixture]
	[Category("BVT")]
	public class BVTPages : Common.Tests.BVT.BVTPages
	{
		protected override string DataFileLocation
		{
			get { return @"BVT\" + Settings.Default.BVTDataFile; }
		}

		[Test]
		public void Test001_AddPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Page'");

			var blankPage = new BlankPage(_driver);

			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);

			blankPage.AddNewPage(_pageName);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the user redirected to newly created page: " + "http://" +
							_baseUrl.ToLower() + "/" + _pageName);
			Assert.That(blankPage.CurrentWindowUrl(), Is.EqualTo("http://" + _baseUrl.ToLower() + "/" + _pageName),
						"The page URL is not correct");

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + " is present in the list");
			Assert.IsTrue(
				adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[@class= 'rtLI']//span[contains(text(), '" + _pageName + "')]")),
				"The page is not present in the list");
		}

		[Test]
		public void Test002_EditDefaultHTMLModuleOnPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Default HTML Module on the Page'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);
			blankPage.SetPageToEditMode();

			var module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber = module.WaitForElement(By.XPath(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLModule(moduleNumber, _pageContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module content is present on the screen");
			Assert.That(
				blankPage.FindElement(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-')]//div[contains(@id, 'lblContent')]")).
					Text, Is.EqualTo(_pageContent));
		}

		[Test]
		public void Test003_EditPageSettings()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Page Settings'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);
			blankPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);

			blankPage.EditPageTitle(_pageTitle);

			Trace.WriteLine("Verify Page title: '" + _pageTitle + "'");
			StringAssert.Contains(_pageTitle, blankPage.CurrentWindowTitle(), "The Page title is not correct");
		}

		[Test]
		public void Test004_DeletePage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete a Page'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			blankPage.DeletePage(_pageName);

			var adminPageManagementPage = new AdminPageManagementPage(_driver);
			adminPageManagementPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in the list");
			Assert.IsFalse(
				adminPageManagementPage.ElementPresent(
					By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//li[@class= 'rtLI']//span[contains(text(), '" + _pageName + "')]")),
				"The page is present in the list");

			var adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is present in Recycle Bin");
			Assert.IsTrue(
				adminRecycleBinPage.ElementPresent(
					By.XPath(AdminRecycleBinPage.RecycleBinPageContainerOption + "[contains(text(), '" + _pageName + "')]")));
		}

		[Test]
		public void Test005_RemovePageFromRecycleBin()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the Page from Recycling Bin'");

			var adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			adminRecycleBinPage.RemovePage(_pageName);

			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the page: " + _pageName + "is NOT present in Recycle Bin");
			Assert.IsFalse(
				adminRecycleBinPage.ElementPresent(
					By.XPath(AdminRecycleBinPage.RecycleBinPageContainerOption + "[contains(text(), '" + _pageName + "')]")));
		}
	}
}