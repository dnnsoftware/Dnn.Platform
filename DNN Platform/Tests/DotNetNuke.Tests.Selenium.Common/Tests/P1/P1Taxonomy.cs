using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	[TestFixture]
	[Category("P1")]
	public abstract class P1Taxonomy : CommonTestSteps
	{
		private string _vocabularyName;
		private string _vocabularyDescription;
		private string _vocabularyType;
		private string _vocabularyScope;
        private string _appScopeVocabularyName;
	    private string _childSiteName;
        private string _websiteScopeVocabularyName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("taxonomy");

			string testName = testSettings.Attribute("name").Value;
			_vocabularyName = testSettings.Attribute("vocabularyName").Value;
			_vocabularyDescription = testSettings.Attribute("vocabularyDescription").Value;
		    _vocabularyType = testSettings.Attribute("vocabularyType").Value;
			_vocabularyScope = testSettings.Attribute("vocabularyScope").Value;
            _appScopeVocabularyName = testSettings.Attribute("appScopeVocabularyName").Value;
		    _childSiteName = testSettings.Attribute("childSiteName").Value;
            _websiteScopeVocabularyName = testSettings.Attribute("websiteScopeVocabularyName").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			//CreateChildSiteAndPrepareSettings(_childSiteName);
			
			_logContent = LogContent();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}

		[Test]
		public void Test001_AddNewVocabulary()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Vocabulary");

			var adminTaxonomyPage = new AdminTaxonomyPage(_driver);
			adminTaxonomyPage.OpenUsingButtons(_baseUrl);

			int itemNumber = adminTaxonomyPage.FindElements(By.XPath(AdminTaxonomyPage.VocabularyList)).Count;

			adminTaxonomyPage.AddNewVocabulary(_vocabularyName, _vocabularyType, _vocabularyScope);

			adminTaxonomyPage.OpenUsingButtons(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list increased by 1");
			Assert.That( adminTaxonomyPage.FindElements(By.XPath(AdminTaxonomyPage.VocabularyList)).Count, Is.EqualTo(itemNumber + 1),
			            "The Vocabulary is not added correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is present in the list");
			Assert.IsTrue(adminTaxonomyPage.ElementPresent(By.XPath("//tr[td[text() = '" + _vocabularyName + "']]")),
			              "The Vocabulary is not added correctly");
		}

		[Test]
		public void Test002_EditVocabulary()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit the Vocabulary'");

			AdminTaxonomyPage adminTaxonomyPage = new AdminTaxonomyPage(_driver);
			adminTaxonomyPage.OpenUsingButtons(_baseUrl);

			adminTaxonomyPage.AddDescription(_vocabularyName, _vocabularyDescription);
			
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary Description is updated in the list");
			Assert.That(adminTaxonomyPage.WaitForElement(By.XPath("//tr[td[text() = '" + _vocabularyName + "']]/td[3]")).Text, Is.EqualTo(_vocabularyDescription),
						"The Vocabulary description is not updated correctly");
		}

		[Test]
		public void Test003_DeleteVocabulary()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Vocabulary'");

			AdminTaxonomyPage adminTaxonomyPage = new AdminTaxonomyPage(_driver);
			adminTaxonomyPage.OpenUsingButtons(_baseUrl);

			//adminTaxonomyPage.WaitForElement(By.XPath(AdminTaxonomyPage.VocabularyTable));
			int itemNumber = adminTaxonomyPage.FindElements(By.XPath(AdminTaxonomyPage.VocabularyList)).Count;

			adminTaxonomyPage.DeleteVocabulary(_vocabularyName);

			adminTaxonomyPage.OpenUsingButtons(_baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of elements in the list decreased by 1");
			Assert.That(adminTaxonomyPage.FindElements(By.XPath(AdminTaxonomyPage.VocabularyList)).Count,
						Is.EqualTo(itemNumber - 1),
						"The Vocabulary is not deleted correctly");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is not present in the list");
			Assert.IsFalse(adminTaxonomyPage.ElementPresent(By.XPath("//tr[td[text() = '" + _vocabularyName + "']]")),
						   "The Vocabulary is not deleted correctly");
		}

		[Test]//Testcase trace: Test-671
	    public void Test004_Host_VocabularyWithApplicationScopeShouldBeAvailableOnChildSite()
	    {
	        Trace.WriteLine(BasePage.RunningTestKeyWord + "'Host: Add Vocabulary with Application scope - it should be available on child site'");

            //Open 'Taxonomy' page
            AdminTaxonomyPage adminTaxonomyPage = new AdminTaxonomyPage(_driver);
			adminTaxonomyPage.OpenUsingButtons(_baseUrl);

            // Click on 'Create New Vocabulary' button and Add a Vocabulary Name and Scope=Application and Click on 'Create Vocabulary' button
            adminTaxonomyPage.AddNewVocabulary(_appScopeVocabularyName, "simple", "application");

            //Assert the Vocabulary's scope is Application
            Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is present and its scope is Application");
            Assert.That(adminTaxonomyPage.WaitForElement(By.XPath("//tr[td[text() = '" + _appScopeVocabularyName + "']]/td[5]")).Text, Is.EqualTo("Application"),
                        "The Vocabulary scope is not 'Application'");

            //Navigate to Child site using its url and Open 'Taxonomy' page
			adminTaxonomyPage.OpenUsingButtons(_baseUrl + "/" + _childSiteName);

            //Assert the new vocabulary exists and Assert the new vocabulary's scope is Application
            Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is present in child site and its scope is Application");
            Assert.That(adminTaxonomyPage.WaitForElement(By.XPath("//tr[td[text() = '" + _appScopeVocabularyName + "']]/td[5]")).Text, Is.EqualTo("Application"),
                        "The Vocabulary is not present or its scope is not 'Application'");
	    }

		[Test] //Testcase trace: Test-672
        public void Test005_Host_VocabularyWithWebsiteScopeShouldNotBeAvailableOnChildSite()
        {
            Trace.WriteLine(BasePage.RunningTestKeyWord + "'Host: Add Vocabulary with Website scope - it should not be available on child site'");

            //Open 'Taxonomy' page
            AdminTaxonomyPage adminTaxonomyPage = new AdminTaxonomyPage(_driver);
            adminTaxonomyPage.OpenUsingButtons(_baseUrl);

            // Click on 'Create New Vocabulary' button and Add a Vocabulary Name and Scope=Application and Click on 'Create Vocabulary' button
            adminTaxonomyPage.AddNewVocabulary(_websiteScopeVocabularyName, "simple", "website");

            //Assert the Vocabulary's scope is Website
            Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is present and its scope is Website");
            Assert.That(adminTaxonomyPage.WaitForElement(By.XPath("//tr[td[text() = '" + _websiteScopeVocabularyName + "']]/td[5]")).Text, Is.EqualTo("Website"),
                        "The Vocabulary scope is not 'Website'");

            //Navigate to Child site using its url and Open 'Taxonomy' page
			adminTaxonomyPage.OpenUsingButtons(_baseUrl + "/" + _childSiteName);

            //Assert the new vocabulary exists and Assert the new vocabulary's scope is Application
            Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Vocabulary is NOT present");
            Assert.IsFalse(adminTaxonomyPage.ElementPresent(By.XPath("//tr[td[text() = '" + _websiteScopeVocabularyName + "']]")), 
                "The Vocabulary with Webiste scope is present on child site");
        }

	}
}