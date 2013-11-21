using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.Upgrade
{
	public abstract class BVTSetup
	{
		protected abstract string DataFileLocation { get; }

		[SetUp]
		public void RunBeforeBVTTests()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Upgrade Setup");

			XDocument doc = XDocument.Load(DataFileLocation);

			XElement settings = doc.Document.Element("Tests").Element("settings");

			IWebDriver driver = TestBase.StartDriver(settings.Attribute("browser").Value);
			string baseUrl = settings.Attribute("baseURL").Value;

			string userName = settings.Attribute("UserName").Value;
			string password = settings.Attribute("Password").Value;

			Trace.WriteLine("Running TEST: 'BVT over upgraded site'");

			UpgradePage upgradePage = new UpgradePage(driver);

			upgradePage.OpenUsingUrl(baseUrl);

			upgradePage.FillAccountInfo(userName, password);

			upgradePage.ClickOnUpgradeButton();

			upgradePage.WaitForUpgradingProcessToFinish();

			upgradePage.ClickOnSeeLogsButton();

			upgradePage.ClickOnVisitWebsiteButton();

			LoginPage loginPage = new LoginPage(driver);

			loginPage.WaitForElement(By.XPath(ControlPanelIDs.LoginLink), 60).WaitTillVisible(20).Click();

			loginPage.WaitAndSwitchToFrame(30);

			loginPage.DoLogin(userName, password);

			loginPage.WaitAndSwitchToWindow(30);

			upgradePage.WelcomeScreen();

			var adminSiteSettingsPage = new AdminSiteSettingsPage(driver);
			adminSiteSettingsPage.OpenUsingButtons(baseUrl);
			adminSiteSettingsPage.DisablePopups();

			driver.Quit();
		}

		[TearDown]
		public void RunAfterBVTTests()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "BVT Teardown");
		}
	}
}
