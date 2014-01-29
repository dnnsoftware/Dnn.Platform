using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class AdminSecurityRolesPage : BasePage
	{ 
		public AdminSecurityRolesPage(IWebDriver driver) : base(driver) { }

		public static string AdminSecurityRolesUrl = "/Admin/SecurityRoles";

		public static string UserNameArrow = "//a[contains(@id, 'SecurityRoles_cboUsers_Arrow')]";
		public static string UserNameDropDown = "//div[contains(@id, '_SecurityRoles_cboUsers_DropDown')]";
		public static string AddUserToRoleButton = "//a[contains(@id, '_SecurityRoles_cmdAdd')]";

		public override string PageTitleLabel
		{
			get { return "Security Roles"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Security Roles"; }
		}

		public override string PreLoadedModule
		{
			get { return "SecurityRolesModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminSecurityRolesUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.SecurityRolesLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminCommonSettings, ControlPanelIDs.AdminSecurityRolesOption);
		}

		public void ManageUsers(string roleName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Manage Users:");
			WaitForElement(By.XPath("//tr[td[contains(text(), '" + roleName + "')]]/td/a[@title = 'Manage Users']")).WaitTillVisible(20);
			Click(By.XPath("//tr[td[contains(text(), '" + roleName + "')]]/td/a[@title = 'Manage Users']"));
		}

		public void AssignRoleToUser(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Assign the Role to User:");

			WaitForElement(By.XPath(UserNameArrow)).WaitTillVisible();
			//SlidingSelectByValue(By.XPath(UserNameArrow), By.XPath(UserNameDropDown), userName);
			new SlidingSelect(_driver, By.XPath(UserNameArrow), By.XPath(UserNameDropDown)).SelectByValue(userName, SlidingSelect.SelectByValueType.Contains);

			Thread.Sleep(1000);

			WaitAndClick(By.XPath(AddUserToRoleButton));

			WaitForElement(By.XPath("//td[a[contains(string(), '" + userName + "')]]"));
		}
	}
}
