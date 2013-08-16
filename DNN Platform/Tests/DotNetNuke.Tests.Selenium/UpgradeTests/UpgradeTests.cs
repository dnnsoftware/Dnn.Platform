using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using DotNetNuke.Tests.DNNSelenium.CorePages;
using DotNetNuke.Tests.DNNSelenium.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.UpgradeTests
{
	class UpgradeTests : TestBase
	{
		private void RunUpgradeTest(XElement settings)
		{
			string testName = settings.Attribute("name").Value;
			string baseUrl = settings.Attribute("baseURL").Value;
			string browser = settings.Attribute("browser").Value;
			string userName = settings.Attribute("UserName").Value;
			string password = settings.Attribute("Password").Value;

			IWebDriver driver = StartBrowser(browser);

			Trace.WriteLine("Running TEST: '" + testName + "'");

			UpgradePage upgradePage = new UpgradePage(driver);

			upgradePage.OpenUsingUrl(baseUrl);

			upgradePage.FillAccountInfo(userName, password);

			upgradePage.ClickOnUpgradeButton();

			upgradePage.WaitForUpgradingProcessToFinish();

			upgradePage.ClickOnSeeLogsButton();

			upgradePage.WaitForLogContainer();

			Trace.WriteLine("Assert Log records: ");
			StringAssert.DoesNotContain("ERROR", upgradePage.FindElement(By.XPath(UpgradePage.UpgraderLogContainer)).Text, "PLZ check log file, it contains error messages");

			upgradePage.ClickOnVisitWebsiteButton();

			var installerPage = new InstallerPage(driver);
			installerPage.WelcomeScreen();

			LoginPage loginPage = new LoginPage(driver);

			loginPage.WaitForElement(By.XPath("//*[@id='" + LoginPage.LoginLink + "' and not(contains(@href, 'Logoff'))]"), 20).WaitTillVisible(20).Click();

			loginPage.WaitAndSwitchToFrame(30);

			loginPage.DoLoginUsingLoginLink(userName, password);

			loginPage.WaitAndSwitchToWindow(30);

			HostSettingsPage hostSettingsPage = new HostSettingsPage(driver);
			hostSettingsPage.OpenUsingButtons(baseUrl);

			hostSettingsPage.Click(By.XPath(HostSettingsPage.LogsTab));
			SlidingSelect.SelectByValue(driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionOne,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent));
			StringAssert.Contains("the script ran successfully", hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text, "The Log File is not empty");

			SlidingSelect.SelectByValue(driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionTwo,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent));
			StringAssert.DoesNotContain("ERROR", hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text, "The Installer Log File contains ERRORS");
		}

		[TestCaseSource("UpgradeData")]
		[Category("Upgrade")]
		public void UpgradeTest(XElement settings)
		{
			TryTest(RunUpgradeTest, settings);
		}

		private IEnumerable UpgradeData
		{
			get { return GetTestData(@"UpgradeTests\" + Settings.Default.UpgradeDataFile); }
		}

		private IEnumerable GetTestData(string fileName)
		{
			var doc = XDocument.Load(fileName);

			return from settings in doc.Descendants("settings")
				   select new object[] { settings };
		}

	}
}
