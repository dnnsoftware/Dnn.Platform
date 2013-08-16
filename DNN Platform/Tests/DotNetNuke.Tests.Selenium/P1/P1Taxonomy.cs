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
	class P1Taxonomy : TestBase
	{
		private string _baseUrl = null;
		private LoginPage _loginPage = null;
		private IWebDriver _driver = null;

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			var doc = XDocument.Load(@"P1\" + Settings.Default.P1DataFile);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement pageSettings = doc.Document.Element("Tests").Element("taxonomy");

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
		}

		[Test]
		public void Test001_AddNewVocabulary()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Vocabulary");

			AdminTaxonomyPage adminTaxonomyPage = new AdminTaxonomyPage(_driver);
			adminTaxonomyPage.OpenUsingButtons(_baseUrl);

			int itemNumber = adminTaxonomyPage.FindElements(By.XPath("")).Count;

			adminTaxonomyPage.AddNewVocabulary("New Vocabulary");

			adminTaxonomyPage.OpenUsingControlPanel(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That(itemNumber + 1, Is.EqualTo(adminTaxonomyPage.FindElements(By.XPath("")).Count),
						"The vocabulary is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is present in the list");
			Assert.IsTrue(adminTaxonomyPage.ElementPresent(By.XPath("//tr[td[text() = '" + "" + "']]")),
				"The Vocabulary is not added correctly");
		}

	}
}
