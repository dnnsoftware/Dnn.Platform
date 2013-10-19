using System;
using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.BVT
{
	[TestFixture]
	public abstract class BVTSearch : CommonTestSteps
	{
		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void Login()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Test BVT'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		public void VerifyQuickSearch(BasePage currentPage)
		{
			currentPage.WaitAndType(By.XPath(ControlPanelIDs.SearchBox), "home");
			currentPage.WaitForElement(By.XPath("//ul[@class = 'searchSkinObjectPreview']"), 60);

			Assert.IsTrue(currentPage.ElementPresent(By.XPath("//li/a[@class = 'searchSkinObjectPreview_more']")),
			              "The link 'See More Results' is missing");

			Assert.That(currentPage.FindElements(By.XPath("//ul[@class = 'searchSkinObjectPreview']/li[@data-url]")).Count,
			            Is.AtLeast(1),
			            "At least one item is displayed");
		}

		public void VerifySearchResults(BasePage currentPage)
		{
			currentPage.WaitAndType(By.XPath(ControlPanelIDs.SearchBox), "home");
			currentPage.Click(By.XPath(ControlPanelIDs.SearchButton));

			var searchPage = new SearchPage(_driver);
			searchPage.WaitForElement(By.XPath("//div[@class = 'dnnSearchResultContainer']"), 60);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Page Title for '" + searchPage.PageHeaderLabel + "' page:");
			StringAssert.Contains(searchPage.PageHeaderLabel.ToUpper(),
								  searchPage.WaitForElement(By.XPath(ControlPanelIDs.PageHeaderID)).Text.ToUpper(),
			                      "The wrong page is opened or The title of " + searchPage.PageHeaderLabel + " page is changed");

			Assert.That(searchPage.FindElements(By.XPath(SearchPage.ResultsList)).Count, Is.AtLeast(1),
			            "At least one item is displayed");

			StringAssert.AreNotEqualIgnoringCase(searchPage.FindElement(By.XPath(SearchPage.TitleOfFirstFoundElement)).Text,
			                                     "No Results Found",
			                                     "'No Results Found' record is displayed");

			//Trace.WriteLine(BasePage.TraceLevelPage + "Total result number '" + searchPage.FindElement(By.XPath(SearchPage.ResultNumber)).Text);
			//StringAssert.AreEqualIgnoringCase(searchPage.FindElement(By.XPath(SearchPage.ResultNumber)).Text, "About 23 Results",
			//		"Result number is not correct");
		}

		[TestCase("Common", "CorePages.AdminSiteLogPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostDashboardPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.FileUploadPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.BlankPage", "OpenCopyPageFrameUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenAddNewUserFrameUsingControlPanel")]
		public void SearchResultsPage(string assyName, string pageClassName, string openMethod)
		{
			VerifySearchResults(OpenPage(assyName, pageClassName, openMethod));
		}

		[TestCase("Common", "CorePages.AdminEventViewerPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.HostExtensionsPage", "OpenUsingButtons")]
		[TestCase("Common", "CorePages.PageImportPage", "OpenUsingControlPanel")]
		[TestCase("Common", "CorePages.BlankPage", "OpenAddNewPageFrameUsingControlPanel")]
		[TestCase("Common", "CorePages.ManageUsersPage", "OpenAddNewUserFrameUsingControlPanel")]
		public void QuickSearch(string assyName, string pageClassName, string openMethod)
		{
			VerifyQuickSearch(OpenPage(assyName, pageClassName, openMethod));
		}

		[Test]
		public void QuickSearchOnLoginPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Quick Search On Login Page'");

			var loginPage = new LoginPage(_driver);

			loginPage.LetMeOut();
			loginPage.OpenUsingUrl(_baseUrl);

			VerifyQuickSearch(loginPage);
		}

		[Test]
		public void QuickSearchOnMainPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Quick Search Main Page'");

			var loginPage = new LoginPage(_driver);

			loginPage.LetMeOut();
			loginPage.OpenUsingUrl(_baseUrl);
			loginPage.DoLoginUsingLoginLink("host", "dnnhost");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);
			VerifyQuickSearch(mainPage);
		}

		[Test]
		public void QuickSearchOnPageSettingsFrame()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Quick Search On Page Settings frame'");

			BlankPage blankPage = new BlankPage(_driver);

			blankPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);

			VerifyQuickSearch(blankPage);
		}

		[Test]
		public void SearchResultsOnLoginPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Results On Login Page'");

			var loginPage = new LoginPage(_driver);

			loginPage.LetMeOut();
			loginPage.OpenUsingUrl(_baseUrl);

			VerifySearchResults(loginPage);
		}

		[Test]
		public void SearchResultsOnMainPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Results Main Page'");

			var loginPage = new LoginPage(_driver);

			loginPage.LetMeOut();
			loginPage.OpenUsingUrl(_baseUrl);
			loginPage.DoLoginUsingLoginLink("host", "dnnhost");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);
			VerifySearchResults(mainPage);
		}

		[Test]
		public void SearchResultsOnPageSettingsFrame()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Results on Page Settings frame'");

			BlankPage blankPage = new BlankPage(_driver);

			blankPage.SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.PageSettingsOption);

			VerifySearchResults(blankPage);
		}

		[Test]
		public void VerifySearchSiteCrawlerOnHostSchedulePage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Site Crawler on Host Schedule Page'");

			var hostSchedulePage = new HostSchedulePage(_driver);
			hostSchedulePage.OpenUsingButtons(_baseUrl);

			Assert.IsTrue(hostSchedulePage.ElementPresent(By.XPath(HostSchedulePage.SearchCrawlerEnabled + " and @checked ='checked']")),
				"Site Crawler is not enabled");
		}

	}
}