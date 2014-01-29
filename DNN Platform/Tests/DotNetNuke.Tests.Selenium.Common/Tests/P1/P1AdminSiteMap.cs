using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{
	public abstract class P1AdminSiteMap : CommonTestSteps
	{
		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("adminSiteMap");

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			string testName = testSettings.Attribute("name").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			//_logContent = LogContent();

		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			//VerifyLogs(_logContent);
		}

		[Test]
		public void SiteMap()
		{
			var siteMap = new AdminSearchEnginePage(_driver);

			_driver.Navigate().GoToUrl("http://" + _baseUrl + "/SiteMap.aspx");

			MemoryStream xmlStream = new MemoryStream();
			StreamWriter writer = new StreamWriter(xmlStream);
			writer.Write(_driver.PageSource);
			writer.Flush();
			xmlStream.Seek(0, SeekOrigin.Begin);

			XmlReader reader = XmlReader.Create(xmlStream);
			XElement xmlDocumentRoot = XElement.Load(reader);

			XmlNameTable nameTable = reader.NameTable;
			XmlNamespaceManager namespaceManager = new XmlNamespaceManager(nameTable);
			namespaceManager.AddNamespace("a", "http://www.sitemaps.org/schemas/sitemap/0.9");

			//now query with your xml - remeber to prefix the default namespace
			var items = xmlDocumentRoot.XPathSelectElements("//a:urlset/a:url", namespaceManager);

			foreach (var item in items)
			{
				Trace.WriteLine("Display name: {0}", item.XPathSelectElement("a:loc", namespaceManager).Value);
			}
			// OR get a list of all display names using linq
			var displayNames = items.Select(x => x.XPathSelectElement("a:loc", namespaceManager).Value).ToList();

		}
	}
}
