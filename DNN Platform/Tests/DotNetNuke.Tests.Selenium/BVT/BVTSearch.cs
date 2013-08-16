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

namespace DotNetNuke.Tests.DNNSelenium.BVT
{		
	[TestFixture]
	[Category("BVT")]
	public class BVTSearch : TestBase
	{
			IWebDriver _driver = null;
			private LoginPage _loginPage = null;
			private string _baseUrl = null;

			[TestFixtureSetUp]
			public void Login()
			{
				var doc = XDocument.Load(@"BVT\" + Settings.Default.BVTDataFile);

				XElement settings = doc.Document.Element("Tests").Element("settings");

				_driver = StartBrowser(settings.Attribute("browser").Value);
				_baseUrl = settings.Attribute("baseURL").Value;

				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Test BVT'");
				Trace.WriteLine(BasePage.PreconditionsKeyWord);

				var mainPage = new MainPage(_driver);
				mainPage.OpenUsingUrl(_baseUrl);

				_loginPage = new LoginPage(_driver);
				_loginPage.OpenUsingUrl(_baseUrl);
				_loginPage.DoLoginUsingUrl("host", "dnnhost");
			}

			private void VerifyQuickSearch(BasePage currentPage)
			{
				currentPage.WaitAndType(By.XPath(BasePage.SearchBox), "awesome");
				currentPage.WaitForElement(By.XPath("//ul[@class = 'searchSkinObjectPreview']"), 60);

				Assert.IsTrue(currentPage.ElementPresent(By.XPath("//li/a[@class = 'searchSkinObjectPreview_more']")), 
						"The link 'See More Results' is missing");

				Assert.That(currentPage.FindElements(By.XPath("//ul[@class = 'searchSkinObjectPreview']/li[@data-url]")).Count, Is.AtLeast(1),
						"At least one item is displayed");
			}

			private void VerifySearchResults(BasePage currentPage)
			{
				currentPage.WaitAndType(By.XPath(BasePage.SearchBox), "awesome");
				currentPage.Click(By.XPath(BasePage.SearchButton));

				SearchPage searchPage = new SearchPage(_driver);
				searchPage.WaitForElement(By.XPath("//div[@class = 'dnnSearchResultContainer']"), 60);

				Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT Page Title for '" + SearchPage.PageTitleLabel + "' page:");
				StringAssert.Contains(SearchPage.PageTitleLabel, searchPage.WaitForElement(By.XPath("//span[contains(@id, '" + BasePage.PageTitle + "')]")).Text,
							"The wrong page is opened or The title of " + SearchPage.PageTitleLabel + " page is changed");

				Assert.That(searchPage.FindElements(By.XPath(SearchPage.ResultsList)).Count, Is.AtLeast(1),
						"At least one item is displayed");

				StringAssert.AreNotEqualIgnoringCase(searchPage.FindElement(By.XPath(SearchPage.TitleOfFirstFoundElement)).Text, "No Results Found",
						"'No Results Found' record is displayed");

				//Trace.WriteLine(BasePage.TraceLevelPage + "Total result number '" + searchPage.FindElement(By.XPath(SearchPage.ResultNumber)).Text);
				//StringAssert.AreEqualIgnoringCase(searchPage.FindElement(By.XPath(SearchPage.ResultNumber)).Text, "About 23 Results",
				//		"Result number is not correct");
			}

			/*private BasePage OpenPage(string pageClassName, string openMethod)
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Navigation To " + pageClassName + "'");

				Type pageClassType = Type.GetType("DotNetNuke.Tests.DNNSelenium.CorePages." + pageClassName);

				var navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

				var miOpen = pageClassType.GetMethod(openMethod);
				if (miOpen != null)
				{
					miOpen.Invoke(navToPage, new object[] { _baseUrl });
				}
				else
				{
					Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
				}

				return (BasePage)navToPage;
			}*/

			[Test]
			[Category(@"EEandPEpackage")]
			public void VerifySearchUrlCrawlerOnHostSchedulePage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Url Crawler on Host Schedule Page'");

				HostSchedulePage hostSchedulePage = new HostSchedulePage(_driver);
				hostSchedulePage.OpenUsingButtons(_baseUrl);

				StringAssert.Contains("dnnCheckbox-checked",
									hostSchedulePage.WaitForElement(By.XPath("//tr[td[text() = '" + HostSchedulePage.SearchUrlCrawlerName + "']]//span[input[contains(@id, 'ViewSchedule_dgSchedule')]]/span")).GetAttribute("class"),
									"Url Crawler is not enabled");
			}

			[Test]
			[Category(@"EEandPEpackage")]
			public void VerifySearchFileCrawlerOnHostSchedulePage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify File Crawler on Host Schedule Page'");

				HostSchedulePage hostSchedulePage = new HostSchedulePage(_driver);
				hostSchedulePage.OpenUsingButtons(_baseUrl);

				StringAssert.Contains("dnnCheckbox-checked",
									hostSchedulePage.WaitForElement(By.XPath("//tr[td[text() = '" + HostSchedulePage.SearchFileCrawlerName + "']]//span[input[contains(@id, 'ViewSchedule_dgSchedule')]]/span")).GetAttribute("class"),
									"File Crawler is not enabled");
			}

			[Test]
			[Category(@"CEpackage")]
			[Category(@"EEandPEpackage")]
			public void VerifySearchSiteCrawlerOnHostSchedulePage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Verify Site Crawler on Host Schedule Page'");

				HostSchedulePage hostSchedulePage = new HostSchedulePage(_driver);
				hostSchedulePage.OpenUsingButtons(_baseUrl);

				StringAssert.Contains("dnnCheckbox-checked",
									hostSchedulePage.WaitForElement(By.XPath("//tr[td[text() = '" + HostSchedulePage.SearchSiteCrawlerName + "']]//span[input[contains(@id, 'ViewSchedule_dgSchedule')]]/span")).GetAttribute("class"),
									"Site Crawler is not enabled");
			}

			[Test]
			public void QuickSearchOnMainPage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Quick Search Main Page'");

				var loginPage = CreatePage<LoginPage>(); //new LoginPage(_driver);
				
				loginPage.LetMeOut();
				loginPage.OpenUsingUrl(_baseUrl);
				loginPage.DoLoginUsingLoginLink("host", "dnnhost");

				//MainPage mainPage = new MainPage(_driver);
				//mainPage.OpenUsingUrl(_baseUrl);
				//VerifyQuickSearch(mainPage);
				VerifyQuickSearch(OpenPage("MainPage", "OpenUsingUrl", _baseUrl));
			}

			[Test]
			public void SearchResultsOnMainPage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Results Main Page'");

				var loginPage = CreatePage<LoginPage>();
				loginPage.LetMeOut();
				loginPage.OpenUsingUrl(_baseUrl);
				loginPage.DoLoginUsingLoginLink("host", "dnnhost");

				//MainPage mainPage = new MainPage(_driver);
				//mainPage.OpenUsingUrl(_baseUrl);
				//VerifySearchResults(mainPage);
				VerifyQuickSearch(OpenPage("MainPage", "OpenUsingUrl", _baseUrl));
			}

			[Test]
			public void QuickSearchOnLoginPage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Quick Search On Login Page'");

				var loginPage = CreatePage<LoginPage>();
				loginPage.LetMeOut();
				loginPage.OpenUsingUrl(_baseUrl);

				VerifyQuickSearch(loginPage);
			}

			[Test]
			public void SearchResultsOnLoginPage()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Results On Login Page'");

				var loginPage = CreatePage<LoginPage>();
				loginPage.LetMeOut();
				loginPage.OpenUsingUrl(_baseUrl);

				VerifySearchResults(loginPage);
			}


			[TestCase("AdminSiteLogPage", "OpenUsingButtons")]
			[TestCase("HostDashboardPage", "OpenUsingButtons")]
			[TestCase("FileUploadPage", "OpenUsingControlPanel")]
			[TestCase("BlankPage", "OpenCopyPageFrameUsingControlPanel")]
			[TestCase("ManageUsersPage", "OpenAddNewUserFrameUsingControlPanel")]
			public void SearchResultsPage(string pageClassName, string openMethod)
			{
				VerifySearchResults(OpenPage(pageClassName, openMethod, _baseUrl));
			}

			[TestCase("AdminEventViewerPage", "OpenUsingButtons")]
			[TestCase("HostExtensionsPage", "OpenUsingButtons")]
			[TestCase("PageImportPage", "OpenUsingControlPanel")]
			[TestCase("BlankPage", "OpenAddNewPageFrameUsingControlPanel")]
			[TestCase("ManageUsersPage", "OpenAddNewUserFrameUsingControlPanel")]
			public void QuickSearch(string pageClassName, string openMethod)
			{
				VerifyQuickSearch(OpenPage(pageClassName, openMethod, _baseUrl));
			}

			[Test]
			public void QuickSearchOnPageSettingsFrame()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Quick Search On Page Settings frame'");

				//BlankPage blankPage = new BlankPage(_driver);
				var blankPage = CreatePage<BlankPage>();

				blankPage.SelectMenuOption(BasePage.ControlPanelEditPageOption, BasePage.PageSettingsOption);

				VerifyQuickSearch(blankPage);
			}

			[Test]
			public void SearchResultsOnPageSettingsFrame()
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "'Search Results on Page Settings frame'");

				//BlankPage blankPage = new BlankPage(_driver);
				var blankPage = CreatePage<BlankPage>();

				blankPage.SelectMenuOption(BasePage.ControlPanelEditPageOption, BasePage.PageSettingsOption);

				VerifySearchResults(blankPage);
			}
	}
}
