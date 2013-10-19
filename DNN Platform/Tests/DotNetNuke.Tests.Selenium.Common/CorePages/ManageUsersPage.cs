using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class ManageUsersPage : BasePage
	{
		public ManageUsersPage(IWebDriver driver) : base(driver) { }

		public static string ManageUsersUrl = "/Admin/UserAccounts";

		public override string PageTitleLabel
		{
			get { return "User Accounts"; }
		}

		public override string PageHeaderLabel
		{
			get { return "User Accounts"; }
		}

		public override string PreLoadedModule
		{
			get { return "SecurityRolesModule"; }
		}

		#region ID's

		public static string UsersTable = "//div[contains(@id, '_Users_grdUsers')]";
		public static string UsersList = "//tr[contains(@id, 'Users_grdUsers')]";
		public static string AddNewUserButton = "//a[contains(@id, '_Users_addUser')]";
		public static string RemoveDeletedUsers = "//a[contains(@id, '_Users_cmdRemoveDeleted')]";

		public static string UserNameTextBox = "//input[contains(@id, '_userName_TextBox')]";
		public static string DisplayNameTextBox = "//input[contains(@id, '_displayName_TextBox')]";
		public static string EmailAddressTextBox = "//input[contains(@id, '_userForm_email_email_TextBox')]";
		public static string PasswordTextBox = "//input[contains(@id, '_ManageUsers_User_txtPassword')]";
		public static string ConfirmPasswordTextBox = "//input[contains(@id, '_ManageUsers_User_txtConfirm')]";

		public static string AddNewUserFrameButton = "//a[contains(@id, 'ManageUsers_cmdAdd')]";
		public static string CancelFrameButton = "//a[contains(@id, '_ManageUsers_cmdCancel')]";

		public static string ManageUserCredentialsFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnUserDetails')]";
		public static string ManageRolesFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnRoleDetails')]";
		public static string ManagePasswordFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnPasswordDetails')]";
		public static string ManageProfileFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnProfileDetails')]";

		public static string RemoveFrameButton = "//a[contains(@id, 'ManageUsers_User_cmdRemove')]";
		public static string UnAuthorizeFrameButton = "//a[contains(@id, 'ManageUsers_Membership_cmdUnAuthorize')]";
		public static string AuthorizeFrameButton = "//a[contains(@id, 'ManageUsers_Membership_cmdAuthorize')]";

		public static string AuthorizedConfirmationMessage = "//div[contains(@id, 'ManageUsers_UP')]/div[contains(@id, 'dnnSkinMessage')]/span[contains(@id, 'lblMessage')]";
		public static string AuthorizedConfirmationMessageText = "User successfully Authorized";

		public static string SecurityRolesArrow = "SecurityRoles_cboRoles_Arrow";
		public static string SecurityRolesDropdown = "//div[contains(@id, 'SecurityRoles_cboRoles_DropDown')]";

		public static string AddRoleToUserFrameButton = "//a[contains(@id, 'SecurityRoles_cmdAdd')]";

		#endregion
		 
		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + ManageUsersUrl);
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			SelectMenuOption(ControlPanelIDs.ControlPanelUsersOption, ControlPanelIDs.UsersManageUsersOption);
		}

		public void OpenAddNewUserFrameUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Use 'Add New User' option");
			SelectMenuOption(ControlPanelIDs.ControlPanelUsersOption, ControlPanelIDs.AddNewUserOption);
		}

		public void AddNewUser(string userName, string displayName, string emailAddress, string password)
		{
			WaitAndClick(By.XPath(AddNewUserButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new User:");

			WaitAndType(By.XPath(UserNameTextBox), userName);
			Type(By.XPath(DisplayNameTextBox), displayName);
			Type(By.XPath(EmailAddressTextBox), emailAddress);
			Type(By.XPath(PasswordTextBox), password);
			Type(By.XPath(ConfirmPasswordTextBox), password);

			Click(By.XPath(AddNewUserFrameButton));

			Thread.Sleep(1000);
		}

		public void DeleteUser(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the User:");

			Click(By.XPath("//tr[td[text() = '" + userName + "']]/td/input[contains(@id, '_Delete')]"));
			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();
		}

		public void RemoveDeletedUser(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Remove the User:");
			EditUser(userName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Create Page' button:");
			ClickOnButton(By.XPath(RemoveFrameButton));
			
			Thread.Sleep(1000);
		}

		public void EditUser(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit the User:");

			Click(By.XPath("//tr[td[text() = '" + userName + "']]/td/a[contains(@href, 'Edit')]"));
		}

		public void AuthorizeUser(string userName)
		{
			EditUser(userName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Authorize the User:");

			WaitAndClick(By.XPath(ManageUserCredentialsFrameTab));
			Click(By.XPath(AuthorizeFrameButton));

			WaitForElement(By.XPath(AuthorizedConfirmationMessage));
		}

		public void ManageRoles(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Manage user Roles:");
			WaitForElement(By.XPath("//tr[td[contains(text(), '" + userName + "')]]/td/a[contains(@href, 'Roles')]")).WaitTillVisible(20);
			Click(By.XPath("//tr[td[contains(text(), '" + userName + "')]]/td/a[contains(@href, 'Roles')]"));
		}

		public void AssignRoleToUser(string roleName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Assign the Role to User:");

			WaitForElement(By.XPath("//a[contains(@id, '" + SecurityRolesArrow + "')]")).WaitTillVisible();
			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + SecurityRolesArrow + "')]"), By.XPath(SecurityRolesDropdown), roleName);

			WaitAndClick(By.XPath(AddRoleToUserFrameButton));

			WaitForElement(By.XPath("//tr[td[contains(text(), '" + roleName + "')]]"));
		}
	}
}
