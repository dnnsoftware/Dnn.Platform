using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostSuperUserAccountsPage : BasePage
	{
		public HostSuperUserAccountsPage(IWebDriver driver) : base(driver) { }

		public static string HostSuperUserAccountsUrl = "/Host/SuperuserAccounts";

		public override string PageTitleLabel
		{
			get { return "SuperUser Accounts"; }
		}

		public override string PageHeaderLabel
		{
			get { return "SuperUser Accounts"; }
		}

		public override string PreLoadedModule
		{
			get { return "SecurityRolesModule"; }
		}

		#region ID's

		public static string SuperUsersTable = "//div[contains(@id, '_Users_grdUsers')]";
		public static string SuperUsersList = "//tr[contains(@id, 'Users_grdUsers')]";
		public static string AddNewUserButton = "//a[contains(@id, '_Users_addUser')]";
		public static string RemoveDeletedUsers = "//a[contains(@id, '_Users_cmdRemoveDeleted')]";

		public static string UserNameTextBox = "//input[contains(@id, '_userName_TextBox')]";
		public static string DisplayNameTextBox = "//input[contains(@id, '_displayName_TextBox')]";
		public static string EmailAddressTextBox = "//input[contains(@id, '_userForm_email_email_TextBox')]";
		public static string PasswordTextBox = "//input[contains(@id, '_ManageUsers_User_txtPassword')]";
		public static string ConfirmPasswordTextBox = "//input[contains(@id, '_ManageUsers_User_txtConfirm')]";

		public static string AddNewUserFrameButton = "//a[contains(@id, 'ManageUsers_cmdAdd')]";
		public static string CancelFrameButton = "//a[contains(@id, '_ManageUsers_cmdCancel')]";

		public static string ManageAccountFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnUserDetails')]";
		public static string ManagePasswordFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnPasswordDetails')]";
		public static string ManageProfileFrameTab = "//div[@id='dnnManageUsers']//a[contains(@href, 'dnnProfileDetails')]";

		public static string RemoveFrameButton = "//a[contains(@id, 'ManageUsers_User_cmdRemove')]";
		public static string UnAuthorizeFrameButton = "//a[contains(@id, 'ManageUsers_Membership_cmdUnAuthorize')]";
		public static string AuthorizeFrameButton = "//a[contains(@id, 'ManageUsers_Membership_cmdAuthorize')]";

		public static string AuthorizedConfirmationMessage = "//div[contains(@id, 'ManageUsers_UP')]/div[contains(@id, 'dnnSkinMessage')]/span[contains(@id, 'lblMessage')]";
		public static string AuthorizedConfirmationMessageText = "User successfully Authorized";

		public static string ContactInformationAccordion = "//div[contains(@id, 'ManageUsers_Profile_ProfileProperties')]/h2/a[contains(text(), 'Contact Information')]";
		public static string PhoneNumberTextBox = "//input[contains(@id, 'ProfileProperties_Telephone')]";

		public static string UpdateButton = "//a[contains(@id, 'Profile_cmdUpdate')]";

		#endregion

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostSuperUserAccountsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.SuperUsersAccountsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostAdvancedSettings, ControlPanelIDs.HostSuperuserAccountsOption);
		}

		public void AddNewUser(string userName, string displayName, string emailAddress, string password)
		{
			WaitAndClick(By.XPath(AddNewUserButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new SuperUser:");

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
			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the SuperUser:");
			Click(By.XPath("//tr[td[contains(text(), '" + userName + "')]]/td/input[contains(@id, '_Delete')]"));
			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();
		}

		public void RemoveDeletedUser(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Remove the SuperUser:");
			/*Click(By.XPath(RemoveDeletedUsers));
			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();*/

			EditSuperUser(userName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Manage Account' Tab:");
			OpenTab(By.XPath(ManageAccountFrameTab));

			WaitAndClick(By.XPath(RemoveFrameButton));

			Thread.Sleep(1000);
		}

		public void EditSuperUser(string userName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit the SuperUser:");
			WaitAndClick(By.XPath("//tr[td[text() ='" + userName + "']]/td/a[contains(@href, 'Edit')]"));
		}

		/*public void OpenManageAccountTab()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Manage Account' Tab:");
			WaitAndClick(By.XPath(ManageAccountFrameTab));
		}*/

		/*public void OpenManageProfileTab()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Manage Profile' Tab:");
			WaitAndClick(By.XPath(ManageProfileFrameTab));
		}*/

		public void AddPhoneNumber(string userName, string phoneNumber)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add phone number to SuperUser:");

			EditSuperUser(userName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'Manage Profile' Tab:");
			OpenTab(By.XPath(ManageProfileFrameTab));

			AccordionOpen(By.XPath(ContactInformationAccordion));

			WaitAndType(By.XPath(PhoneNumberTextBox), phoneNumber);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}
	}
}
