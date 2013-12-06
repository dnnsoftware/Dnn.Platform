using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.Installer
{
	public abstract class InstallerTests : TestBase
	{
		public void RunLayoutTest(XElement settings)
		{
			string testName = settings.Attribute("name").Value;
			string baseUrl = settings.Attribute("baseURL").Value;
			string browser = settings.Attribute("browser").Value;
			string installerLanguage = settings.Attribute("InstallerLanguage").Value;
			string edition = settings.Attribute("edition").Value;
			string template = settings.Attribute("Template").Value;
			string language = settings.Attribute("Language").Value;

			IWebDriver driver = StartBrowser(browser);

			Trace.WriteLine("Running TEST: '" + testName + "'");

			InstallerPage installerPage = new InstallerPage(driver);

			installerPage.OpenUsingUrl(baseUrl);

			installerPage.SetInstallerLanguage(installerLanguage);
			installerPage.SetDictionary(installerLanguage);

			#region Layout Tests on Account Info Tab
			//Tab conditions; Current Tab - Account Info
			Trace.WriteLine(BasePage.TraceLevelPage + "The current Account Info Tab:");

			//Verify Installation page title
			installerPage.WaitForElement(By.Id(InstallerPage.InstallerPageTitle)).WaitTillVisible(20);
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify page title: '" + InstallerPage.InstallerPageTitle + "'" + " 'DotNetNuke Installation'");
			Utilities.SoftAssert(() => StringAssert.Contains(installerPage.Translate("PageTitle"), installerPage.FindElement(By.Id(InstallerPage.InstallerPageTitle)).Text,
							"The page title is missing or incorrect"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify Tabs condition on Account Info Tab: ");
			Utilities.SoftAssert(() => StringAssert.Contains("active", installerPage.FindElement(By.Id(InstallerPage.AccountInfoTab)).GetAttribute("class"), "Account Info Tab is not active"));
			Utilities.SoftAssert(() => StringAssert.Contains("disabled", installerPage.FindElement(By.Id(InstallerPage.InstallInfoTab)).GetAttribute("class"), "Installation Tab is not disabled"));
			Utilities.SoftAssert(() => StringAssert.Contains("disabled", installerPage.FindElement(By.Id(InstallerPage.WebInfoTab)).GetAttribute("class"), "View Website Tab is not disabled"));

			//flags
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the number of flags:");
			Utilities.SoftAssert(() => Assert.AreEqual(6, installerPage.FindElements(By.XPath("//div[@id='languageFlags']/a")).Count(), "The number of flags are incorrect"));

			//flags bubble help
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the flag bubble-help is present:");
			Utilities.SoftAssert(() => Assert.That("English (United States)", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.EnglishIconId)).GetAttribute("title")), "English flag is missing the bubble-help"));
			Utilities.SoftAssert(() => Assert.That("Deutsch (Deutschland)", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.GermanIconId)).GetAttribute("title")), "German flag is missing the bubble-help"));
			Utilities.SoftAssert(() => Assert.That("Espanol (Espana)", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.SpanishIconId)).GetAttribute("title")), "Spanish flag is missing the bubble-help"));
			Utilities.SoftAssert(() => Assert.That("italiano (Italia)", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.ItalianIconId)).GetAttribute("title")), "Italian flag is missing the bubble-help"));
			Utilities.SoftAssert(() => Assert.That("francais (France)", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.FrenchIconId)).GetAttribute("title")), "French flag is missing the bubble-help"));
			Utilities.SoftAssert(() => Assert.That("Nederlands (Nederland)", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.HollandIconId)).GetAttribute("title")), "Nederlands flag is missing the bubble-help"));

			//Templates; Community edition has only 2 options - Blank and Default
			if (edition != "community")
			{
				Trace.WriteLine(BasePage.TraceLevelPage + "Verify the number of options for Template drop-down: '" + edition + "' edition");
				Utilities.SoftAssert(() => Assert.AreEqual(3, SlidingSelect.CountAllOptions(driver, By.Id(InstallerPage.TemplateArrow), By.XPath(InstallerPage.TemplateDropdown)),
					"Template drop-down contains incorrect number of options"));
			}
			else
			{
				Trace.WriteLine(BasePage.TraceLevelPage + "Verify the number of options for Template drop-down: '" + edition + "' edition");
				Utilities.SoftAssert(() => Assert.AreEqual(2, SlidingSelect.CountAllOptions(driver, By.Id(InstallerPage.TemplateArrow), By.XPath(InstallerPage.TemplateDropdown)),
					"Template drop-down contains incorrect number of options"));
			}


			//Languages
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify the number of options for Language drop-down:");
			Utilities.SoftAssert(() => Assert.AreEqual(6, SlidingSelect.CountAllOptions(driver, By.Id(InstallerPage.LanguageArrow), By.XPath(InstallerPage.LanguageDropdown)),
					"Language drop-down contains incorrect number of options"));

			//mandatory fields
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify mandatory fields:");
			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.UserNameLabel)).GetAttribute("class"), "Username field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.PasswordLabel)).GetAttribute("class"), "Password field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.ConfirmPasswordLabel)).GetAttribute("class"), "Confirm Password field does not have \"required\" class"));

			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.WebsiteNameLabel)).GetAttribute("class"), "WebsiteName field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.TemplateLabel)).GetAttribute("class"), "Template field has \"required\" class when it shouldn't"));
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.LanguageLabel)).GetAttribute("class"), "Language field has \"required\" class when it shouldn't"));

			RadioButton.Select(driver, By.Id(InstallerPage.DatabaseSetupCustom));

			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseServerLabel)).GetAttribute("class"), "Server Name field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseFilenameLabel)).GetAttribute("class"), "Database File Name field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseObjectQualifierLabel)).GetAttribute("class"), "Object Qualifier field has the \"required\" class and it shouldnt"));

			RadioButton.Select(driver, By.Id(InstallerPage.DatabaseSetupTypeSqlServer));

			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseServerLabel)).GetAttribute("class"), "Server Name field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseNameLabel)).GetAttribute("class"), "Database Name field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseObjectQualifierLabel)).GetAttribute("class"), "Object Qualifier field has the \"required\" class and it shouldnt"));

			RadioButton.Select(driver, By.Id(InstallerPage.DatabaseSecurityUserDefined));

			Utilities.SoftAssert(() => StringAssert.Contains("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabaseUserNameLabel)).GetAttribute("class"), "Database Username field does not have \"required\" class"));
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnFormRequired", installerPage.FindElement(By.Id(InstallerPage.DatabasePasswordLabel)).GetAttribute("class"), "Database Password field has the \"required\" class and it shouldnt"));

			#endregion

			installerPage.OpenUsingUrl(baseUrl);
			installerPage.FillAccountInfo(settings);

			installerPage.SetWebSiteLanguage(language);

			installerPage.ClickOnContinueButton();

			//Tab conditions; Current Tab - Installation Tab
			Trace.WriteLine(BasePage.TraceLevelPage + "The current is Installation Tab:");
			installerPage.WaitForElement(By.XPath("//p[@id='" + InstallerPage.FileAndFolderPermissionCheck + "' and @class ='step-done']"), 25);


			#region Layout Tests on Installation Tab
			//Verify Installation page title
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify page title: '" + InstallerPage.InstallerPageTitle + "'" + " 'DotNetNuke Installation'");
			Utilities.SoftAssert(() => StringAssert.Contains(installerPage.Translate("PageTitle"), installerPage.FindElement(By.Id(InstallerPage.InstallerPageTitle)).Text,
							"The page title is missing or incorrect"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify Tabs condition on Installation Tab: ");
			Utilities.SoftAssert(() => StringAssert.Contains("disabled", installerPage.FindElement(By.Id(InstallerPage.AccountInfoTab)).GetAttribute("class"), "Account Info Tab is not disabled"));
			Utilities.SoftAssert(() => StringAssert.Contains("active", installerPage.FindElement(By.Id(InstallerPage.InstallInfoTab)).GetAttribute("class"), "Installation Tab is not active"));
			Utilities.SoftAssert(() => StringAssert.Contains("disabled", installerPage.FindElement(By.Id(InstallerPage.WebInfoTab)).GetAttribute("class"), "View Website Tab is not disabled"));

			//Verify buttons are enabled/disabled
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify 'Retry' button is disabled:");
			Utilities.SoftAssert(() => StringAssert.Contains("dnnDisabledAction", installerPage.FindElement(By.Id(InstallerPage.RetryButton)).GetAttribute("class"), "Retry button is enabled"));
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify 'See Logs' button is disabled:");
			Utilities.SoftAssert(() => StringAssert.Contains("dnnDisabledAction", installerPage.FindElement(By.Id(InstallerPage.SeeLogsButton)).GetAttribute("class"), "See Logs button is enabled"));
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify 'Visit Site' button is disabled:");
			Utilities.SoftAssert(() => StringAssert.Contains("dnnDisabledAction", installerPage.FindElement(By.Id(InstallerPage.VisitSiteButton)).GetAttribute("class"), "Visit Site button is enabled"));

			#endregion

			installerPage.WaitForInstallationProcessToFinish();

			#region Layout Tests on View Website Tab
			//Tab conditions; Current Tab - View Website Tab
			Trace.WriteLine(BasePage.TraceLevelPage + "The current is View Website Tab:");
			//Verify View  page title
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify page title: '" + InstallerPage.InstallerPageTitle + "'" + " 'DotNetNuke Installation'");
			Utilities.SoftAssert(() => StringAssert.Contains(installerPage.Translate("PageTitle"), installerPage.FindElement(By.Id(InstallerPage.InstallerPageTitle)).Text,
							"The page title is missing or incorrect"));

			//Tab conditions; Current Tab - View Website Tab
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify Tabs condition on View Website Tab: ");
			Utilities.SoftAssert(() => StringAssert.Contains("disabled", installerPage.FindElement(By.Id(InstallerPage.AccountInfoTab)).GetAttribute("class"), "Account Info Tab is not disabled"));
			Utilities.SoftAssert(() => StringAssert.Contains("disabled", installerPage.FindElement(By.Id(InstallerPage.InstallInfoTab)).GetAttribute("class"), "Installation Tab is not disabled"));
			Utilities.SoftAssert(() => StringAssert.Contains("active", installerPage.FindElement(By.Id(InstallerPage.WebInfoTab)).GetAttribute("class"), "View Website Tab is not active"));

			//Verify there is no extra steps/all required steps are present
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify number of installation steps: ");
			Utilities.SoftAssert(() => Assert.AreEqual(5, installerPage.FindElements(By.XPath("//div[@id='" + InstallerPage.InstallationSteps + "']/p")).Count(), "The number of intallation steps are incorrect"));

			//Verify that installation steps are marked as "done"
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify installation steps marked as 'done': ");
			Utilities.SoftAssert(() => Assert.That("step-done", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.FileAndFolderPermissionCheck)).GetAttribute("class")),
				"Step -FileAndFolderPermissionCheck- is not marked as \"done\""));
			Utilities.SoftAssert(() => Assert.That("step-done", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.DatabaseInstallation)).GetAttribute("class")),
				"Step -DatabaseInstallation- is not marked as \"done\""));
			Utilities.SoftAssert(() => Assert.That("step-done", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.ExtensionsInstallation)).GetAttribute("class")),
				"Step -ExtensionsInstallation- is not marked as \"done\""));
			Utilities.SoftAssert(() => Assert.That("step-done", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.WebsiteCreation)).GetAttribute("class")),
				"Step -WebsiteCreation- is not marked as \"done\""));
			Utilities.SoftAssert(() => Assert.That("step-done", Is.EqualTo(installerPage.FindElement(By.Id(InstallerPage.SuperUserCreation)).GetAttribute("class")),
				"Step -SuperUserCreation- is not marked as \"done\""));

			//Verify buttons are enabled/disabled
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify 'Retry' button is disabled:");
			Utilities.SoftAssert(() => StringAssert.Contains("dnnDisabledAction", installerPage.FindElement(By.Id(InstallerPage.RetryButton)).GetAttribute("class"), "Retry button is enabled"));
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify 'See Logs' button is enabled:");
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnDisabledAction", installerPage.FindElement(By.Id(InstallerPage.SeeLogsButton)).GetAttribute("class"), "See Logs button is disabled"));
			Trace.WriteLine(BasePage.TraceLevelPage + "Verify 'Visit Site' button is enabled:");
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("dnnDisabledAction", installerPage.FindElement(By.Id(InstallerPage.VisitSiteButton)).GetAttribute("class"), "Visit Site button is disabled"));

			#endregion
		}

		[TestCaseSource("LayoutTestData")]
		[Category("Installer")]
		public void LayoutTest(XElement settings)
		{
			TryTest(RunLayoutTest, settings);
		}

		private void RunInstallTest(XElement settings)
		{
			string testName = settings.Attribute("name").Value;
			string baseUrl = settings.Attribute("baseURL").Value;
			string browser = settings.Attribute("browser").Value;
			string template = settings.Attribute("Template").Value;
			string installerLanguage = settings.Attribute("InstallerLanguage").Value;
			string language = settings.Attribute("Language").Value;
			string username = settings.Attribute("UserName").Value;
			string password = settings.Attribute("Password").Value;
			string websiteName = settings.Attribute("WebsiteName").Value;

			IWebDriver driver = StartBrowser(browser);

			Trace.WriteLine("Running TEST: '" + testName + "'");

			InstallerPage installerPage = new InstallerPage(driver);

			installerPage.OpenUsingUrl(baseUrl);

			installerPage.SetInstallerLanguage(installerLanguage);
			installerPage.SetDictionary(installerLanguage);

			installerPage.OpenUsingUrl(baseUrl);
			installerPage.FillAccountInfo(settings);

			installerPage.SetWebSiteLanguage(language);

			installerPage.ClickOnContinueButton();

			installerPage.WaitForInstallationProcessToFinish();

			installerPage.ClickOnVisitWebsiteButton();

			Trace.WriteLine(BasePage.TraceLevelComposite + "Login to the site first time, using 'Visit Website' button:");

			Trace.WriteLine(BasePage.TraceLevelComposite + "Set site language: '" + language + "'");
			installerPage.SetDictionary(language);

			Trace.WriteLine(BasePage.TraceLevelPage + "Verify frame title: '" + installerPage.CurrentFrameTitle() + "'");
			Utilities.SoftAssert(() => StringAssert.AreEqualIgnoringCase(installerPage.Translate("WelcomeScreenTitle"), installerPage.CurrentFrameTitle(),
								 "The Welcome Screen title is missing or incorrect"));

			installerPage.WaitAndSwitchToFrame(60);

			installerPage.WaitForElement(By.Id(InstallerPage.IntroVideo), 60).WaitTillVisible(60);
			installerPage.WaitForElement(By.Id(InstallerPage.WhatIsNew), 60).WaitTillVisible(60);

			installerPage.WaitForElement(By.Id(InstallerPage.LetMeAtIn), 60).WaitTillEnabled().Click();

			installerPage.WaitAndSwitchToWindow(60);

			LoginPage loginPage = new LoginPage(driver);

			Trace.WriteLine("Verify Website Name: '" + websiteName + "'");
			Utilities.SoftAssert(() => StringAssert.Contains(websiteName, installerPage.CurrentWindowTitle(),
										"The website name is incorrect"));

			switch (template)
			{
				case "Default Template":
					{
						loginPage.LoginUsingLoginLinkAndFrame(username, password);

						//default template; look for menu options, 4 options should be present
						Trace.WriteLine("Assert current Template: Default: ");
						Assert.AreEqual(4, installerPage.FindElements(By.XPath("//ul[@class ='nav nav-pills']/li")).Count(),
										"This is not a Default page or The number of options are incorrect");

						loginPage.LoginUsingUrl(baseUrl, username, password);
						//default template; look for menu options, 4 options should be present
						Trace.WriteLine("Assert current Template: Default: ");
						Assert.AreEqual(4, installerPage.FindElements(By.XPath("//ul[@class = 'nav nav-pills']/li")).Count(),
										"This is not a Default page or The number of options are incorrect");

						break;
					}
				case "Mobile Template":
					{
						Trace.WriteLine("Assert current Template: Mobile: ");
						Assert.AreEqual(3, installerPage.FindElements(By.XPath("//ol[@class='mobileInstruction']/li")).Count(),
								"This is not a Mobile page or The mobile instructions are not present on the page or The number of mobile instructions steps are incorrect");

						break;
					}
				case "Blank Template":
					{
						loginPage.LoginUsingLoginLinkAndFrame(username, password);

						//blank template; look for the "Home" option (only one option is present)
						Trace.WriteLine("Assert current Template: Blank: ");
						Assert.AreEqual(1, installerPage.FindElements(By.XPath("//ul[@id='dnn_pnav']/li")).Count(),
										"This is not a Blank page or The number of options are incorrect");

						loginPage.LoginUsingUrl(baseUrl, username, password);

						//blank template; look for the "Home" option (only one option is present)
						Trace.WriteLine("Assert current Template: Blank: ");
						Assert.AreEqual(1, installerPage.FindElements(By.XPath("//ul[@id='dnn_pnav']/li")).Count(),
										"This is not a Blank page or The number of options are incorrect");
						break;
					}
			}

		}

		[TestCaseSource("InstallerData")]
		[Category("Installer")]
		public void InstallCompleteTest(XElement settings)
		{
			TryTest(RunInstallTest, settings);
		}

		private void RunInstallerLogTest(XElement settings)
		{
			string testName = settings.Attribute("name").Value;
			string baseUrl = settings.Attribute("baseURL").Value;
			string browser = settings.Attribute("browser").Value;
			string installerLanguage = settings.Attribute("InstallerLanguage").Value;
			string language = settings.Attribute("Language").Value;

			IWebDriver driver = StartBrowser(browser);

			Trace.WriteLine("Running TEST: '" + testName + "'");

			InstallerPage installerPage = new InstallerPage(driver);

			installerPage.OpenUsingUrl(baseUrl);

			installerPage.SetInstallerLanguage(installerLanguage);
			installerPage.SetDictionary(installerLanguage);

			installerPage.OpenUsingUrl(baseUrl);
			installerPage.FillAccountInfo(settings);

			installerPage.SetWebSiteLanguage(language);

			installerPage.ClickOnContinueButton();

			installerPage.WaitForInstallationProcessToFinish();

			installerPage.ClickOnSeeLogsButton();

			installerPage.WaitForLogContainer();

			Trace.WriteLine("Assert Log records: ");
			StringAssert.DoesNotContain("ERROR", installerPage.FindElement(By.Id(InstallerPage.InstallerLogContainer)).Text, "PLZ check log file, it contains error messages");

			installerPage.ClickOnVisitWebsiteButton();

			installerPage.WelcomeScreen();

			Trace.WriteLine("Assert Log files do not contain ERRORs ");

			HostSettingsPage hostSettingsPage = new HostSettingsPage(driver);
			hostSettingsPage.SetDictionary(language);
			hostSettingsPage.OpenUsingButtons(baseUrl);

			hostSettingsPage.WaitAndClick(By.XPath(HostSettingsPage.LogsTab));
			SlidingSelect.SelectByValue(driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionOne,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);
			StringAssert.Contains(hostSettingsPage.Translate("Successfull Log"), hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text, "The Log File is not empty");

			SlidingSelect.SelectByValue(driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionTwo,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);
			StringAssert.DoesNotContain("ERROR", hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text, "The Installer Log File contains ERRORS");
		}

		[TestCaseSource("InstallerLogData")]
		[Category("Installer")]
		public void InstallerLogTest(XElement settings)
		{
			TryTest(RunInstallerLogTest, settings);
		}

		protected abstract string LayoutDataFileLocation { get; }
		public IEnumerable LayoutTestData
		{
			get { return GetTestData(LayoutDataFileLocation); }
		}

		protected abstract string InstallerDataFileLocation { get; }
		public IEnumerable InstallerData
		{
			get { return GetTestData(InstallerDataFileLocation); }
		}

		protected abstract string LogDataFileLocation { get; }
		public IEnumerable InstallerLogData
		{
			get { return GetTestData(LogDataFileLocation); }
		}

		public IEnumerable GetTestData(string fileName)
		{
			var doc = XDocument.Load(fileName);

			return from settings in doc.Descendants("settings")
				   select new object[] { settings };
		}
	}
}
