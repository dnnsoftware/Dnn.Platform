using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;
using InstallerPage = DNNSelenium.Common.CorePages.InstallerPage;

namespace DNNSelenium.Common.BaseClasses
{
	public class CommonTestSteps : TestBase
	{
		protected IWebDriver _driver = null;
		protected string _baseUrl = null;

		public const string DNNSeleniumPrefix = "DNNSelenium.";
		public string _logContent;

		public BasePage OpenPage(string assyName, string pageClassName, string openMethod)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Navigation To " + pageClassName + "'");

			string fullAssyName = DNNSeleniumPrefix + assyName;
			Type pageClassType = Type.GetType(fullAssyName + "." + pageClassName + ", " + fullAssyName);
			object navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

			MethodInfo miOpen = pageClassType.GetMethod(openMethod);
			if (miOpen != null)
			{
				miOpen.Invoke(navToPage, new object[] { _baseUrl });
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
			}

			return (BasePage)navToPage;
		}

		public void OpenMainPageAndLoginAsHost()
		{
			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);
		}

		public void CreateAdminAndLoginAsAdmin(string userName, string displayName, string email, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Admin User and Login as Admin user'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(userName, displayName, email, password);
			manageUsersPage.ManageRoles(userName);
			manageUsersPage.AssignRoleToUser("Administrators");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginUsingLoginLink(userName, password);
		}

		public string LogContent()
		{
			HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.SetDictionary("en");
			hostSettingsPage.OpenUsingUrl(_baseUrl);

			hostSettingsPage.WaitAndClick(By.XPath(HostSettingsPage.LogsTab));
			SlidingSelect.SelectByValue(_driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionOne,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);

			return hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text.ToLower();
		}

		public void VerifyLogs(string logContentBeforeTests)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Verify Logs on Host Settings page: '");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			string logContentAfterTests = LogContent();
			StringAssert.AreEqualIgnoringCase(logContentAfterTests, logContentBeforeTests, "ERROR in the Log");
		}

		public void CreateChildSiteAndPrepareSettings(string childSiteName)
		{
			//create a child site
			HostSiteManagementPage hostSiteMgmtPage = new HostSiteManagementPage(_driver);
			hostSiteMgmtPage.OpenUsingButtons(_baseUrl);
			hostSiteMgmtPage.AddNewChildSite(_baseUrl, childSiteName, "Child Site");

			//navigate to child site
			hostSiteMgmtPage.OpenUsingButtons(_baseUrl);
			hostSiteMgmtPage.NavigateToChildSite(_baseUrl, childSiteName);

			//close welcome screen on child site
			var installerPage = new InstallerPage(_driver);
			installerPage.WelcomeScreen();

			//disable popups on child site
			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(_baseUrl);
			adminSiteSettingsPage.DisablePopups();
		}
	}
}
