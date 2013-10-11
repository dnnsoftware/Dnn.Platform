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
	public abstract class P1FileUpload : CommonTestSteps
	{
		private string _smallFileToUpload;
		private string _largeFileToUpload;
		private string _folderName;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("fileUpload");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			_smallFileToUpload = testSettings.Attribute("smallfileToUpload").Value;
			_largeFileToUpload = testSettings.Attribute("largefileToUpload").Value;
			_folderName = testSettings.Attribute("folderName").Value;

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
		public void Test001_UploadFileUnder12MB()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload file under 12 MB'");

			var fileUploadPage = new FileUploadPage(_driver);
			fileUploadPage.OpenUsingControlPanel(_baseUrl);

			fileUploadPage.UploadFile(_smallFileToUpload, _folderName);

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SearchForFile(_smallFileToUpload);

			Assert.IsTrue(
				adminFileManagementPage.ElementPresent(
					By.XPath("//div[contains(@class, 'dnnModuleDigitalAssetItemName')]//span/font[text() = '" + _smallFileToUpload +
					         "']")));
		}

		[Test]
		public void Test002_UploadFileOver12MB()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Upload file over 12 MB'");

			var fileUploadPage = new FileUploadPage(_driver);
			fileUploadPage.OpenUsingControlPanel(_baseUrl);

			fileUploadPage.UploadFile(_largeFileToUpload, _folderName);

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);
			adminFileManagementPage.SearchForFile(_largeFileToUpload);

			Assert.IsFalse(
				adminFileManagementPage.ElementPresent(
					By.XPath("//div[contains(@class, 'dnnModuleDigitalAssetItemName')]//span/font[text() = '" + _largeFileToUpload +
					         "']")));
		}
	}
}