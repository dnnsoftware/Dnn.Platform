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

namespace DNNSelenium.Common.BaseClasses
{
	public class CommonTestSteps : TestBase
	{
		protected IWebDriver _driver = null;
		protected string _baseUrl = null;

		private const string DNNSeleniumPrefix = "DNNSelenium.";

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

		public BaseOutsidePage OpenOutsidePage(string assyName, string pageClassName, string openMethod)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Navigation To " + pageClassName + "'");

			string fullAssyName = DNNSeleniumPrefix + assyName;
			Type pageClassType = Type.GetType(fullAssyName + "." + pageClassName + ", " + fullAssyName);
			object navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

			var hostPage = new HostBasePage(_driver);
			Type myType = hostPage.GetType();

			MethodInfo miOpen = myType.GetMethod(openMethod);
			if (miOpen != null)
			{
				miOpen.Invoke(hostPage, new object[] { _baseUrl });
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
			}

			return (BaseOutsidePage)navToPage;
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

		public void VerifyLogs()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Verify Logs on Host Settings page: '");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.SetDictionary("en");
			hostSettingsPage.OpenUsingUrl(_baseUrl);

			hostSettingsPage.WaitAndClick(By.XPath(HostSettingsPage.LogsTab));
			SlidingSelect.SelectByValue(_driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionOne,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);
			Utilities.SoftAssert(() => StringAssert.DoesNotContain("error", hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text.ToLower(), "ERROR in the Log"));
		}

	}
}
