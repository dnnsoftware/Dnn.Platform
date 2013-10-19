using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.BVT
{
	[TestFixture ]
	public abstract class BVTModules : CommonTestSteps
	{
		public string _pageName;
		public string _moduleContent;
		public string _moduleTitle;

		protected abstract string DataFileLocation { get; }

		[TestFixtureSetUp]
		public void LoginToSite()
		{
			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");
			XElement testSettings = doc.Document.Element("Tests").Element("modules");

			string testName = testSettings.Attribute("name").Value;

			_pageName = testSettings.Attribute("pageName").Value;
			_moduleContent = testSettings.Attribute("moduleContent").Value;

			_moduleTitle = testSettings.Attribute("moduleTitle").Value;

			_driver = StartBrowser(settings.Attribute("browser").Value);
			_baseUrl = settings.Attribute("baseURL").Value;

			Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
			Trace.WriteLine(BasePage.PreconditionsKeyWord);

			OpenMainPageAndLoginAsHost();

			_logContent = LogContent();

			var blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);
			blankPage.AddNewPage(_pageName);
		}

		[TestFixtureTearDown]
		public void Cleanup()
		{
			VerifyLogs(_logContent);
		}
	}
}