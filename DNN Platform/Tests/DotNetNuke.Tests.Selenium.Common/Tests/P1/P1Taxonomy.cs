using System.Diagnostics;
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

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs();
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
	}
}