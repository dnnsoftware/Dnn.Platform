using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class ManageRolesPage : BasePage
	{
		public ManageRolesPage(IWebDriver driver) : base(driver) { }

		public static string ManageRolesUrl = "/Admin/SecurityRoles";

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

		//Roles ID's
		public static string SecurityRolesTable = "//div[contains(@id, '_Roles_grdRoles')]";
		public static string SecurityRolesList = "//tr[contains(@id, 'Roles_grdRoles')]";
		public static string AddNewRoleButton = "//a[contains(@id, 'Roles_cmdAddRole')]";
		public static string UpdateRoleFrameButton = "//a[contains(@id, 'EditRoles_cmdUpdate')]";
		public static string DeleteRoleFrameButton = "//a[contains(@id, 'EditRoles_cmdDelete')]";

		public static string BasicSettingsFrameTab = "//a[@href = '#erBasicSettings']";
		public static string AdvancedSettingsFrameTab = "//a[@href = '#erAdvancedSettings']";

		public static string RoleNameTextBox = "//input[contains(@id, 'EditRoles_txtRoleName')]";
		public static string RoleNameDescriptionTextBox = "//textarea[contains(@id, 'EditRoles_txtDescription')]";
		public static string PublicRoleCheckBox = "//input[contains(@id, 'EditRoles_chkIsPublic')]";
		public static string AutoAssignmentCheckBox = "//input[contains(@id, 'EditRoles_chkAutoAssignment')]";
		public static string StatusArrow = "EditRoles_statusList_Arrow";
		public static string StatusDropDown = "//div[contains(@id, 'EditRoles_statusList_DropDown')]";
		public static string StatusDropDownList = "//div[contains(@id, 'EditRoles_statusList_DropDown')]//li";

		public static string ServiceFeeTextBox = "//input[contains(@id,'_txtServiceFee')]";
		public static string NumberInBillingPeriodTextBox = "//input[contains(@id, '_txtBillingPeriod')]";
		public static string BillingPeriodArrow = "cboBillingFrequency_Arrow";
		public static string BillingPeriodDropDown = "//div[contains(@id, 'cboBillingFrequency_DropDown')]";
		public static string BillingPeriodList = "//div[contains(@id, 'cboBillingFrequency_DropDown')]//li";

		//Role Groups ID's
		public static string AddNewRoleGroupButton = "//a[contains(@id, 'Roles_cmdAddRoleGroup')]";
		public static string UpdateGroupFrameButton = "//a[contains(@id, 'EditGroups_cmdUpdate')]";
		public static string DeleteGroupFrameButton = "//a[contains(@id, 'EditGroups_cmdDelete')]";

		public static string RoleGroupNameTextBox = "//input[contains(@id, 'EditGroups_txtRoleGroupName')]";
		public static string RoleGroupNameDescriptionTextBox = "//textarea[contains(@id, 'EditGroups_txtDescription')]";

		public static string FilterByGroupArrow = "Roles_cboRoleGroups_Arrow";
		public static string FilterByGroupDropdown = "//div[contains(@id, 'cboRoleGroups_DropDown')]";
		public static string FilterByGroupDropdownList = "//div[contains(@id, 'cboRoleGroups_DropDown')]//li";
		public static string RoleGroupEditIcon = "//a[contains(@id,'Roles_lnkEditGroup')]";
		public static string RoleGroupDeleteIcon = "//input[contains(@id,'Roles_cmdDelete')]";


		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + ManageRolesUrl);
			WaitForElement(By.Id(ControlPanelIDs.CopyrightNotice));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			SelectMenuOption(ControlPanelIDs.ControlPanelUsersOption, ControlPanelIDs.UsersManageRolesOption);

			WaitForElement(By.Id(ControlPanelIDs.CopyrightNotice));
		}

		public void AddNewSecurityRole(string roleName)
		{
			WaitAndClick(By.XPath(AddNewRoleButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Security Role:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Basic Settings' Tab:");
			OpenTab(By.XPath(BasicSettingsFrameTab));

			Type(By.XPath(RoleNameTextBox), roleName);

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void AddNewPublicSecurityRole(string roleName)
		{
			WaitAndClick(By.XPath(AddNewRoleButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Public Security Role:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Basic Settings' Tab:");
			OpenTab(By.XPath(BasicSettingsFrameTab));

			Type(By.XPath(RoleNameTextBox), roleName);
			CheckBoxCheck(By.XPath(PublicRoleCheckBox));

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void AddNewAutoAssignedSecurityRole(string roleName)
		{
			WaitAndClick(By.XPath(AddNewRoleButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Auto assigned Security Role:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Basic Settings' Tab:");
			OpenTab(By.XPath(BasicSettingsFrameTab));

			Type(By.XPath(RoleNameTextBox), roleName);
			CheckBoxCheck(By.XPath(AutoAssignmentCheckBox));

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void AddNewSecurityRoleWithFees(string roleName, string serviceFee, string numberInBillingPeriod, string billingPeriod)
		{
			WaitAndClick(By.XPath(AddNewRoleButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Security Role:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Basic Settings' Tab:");
			OpenTab(By.XPath(BasicSettingsFrameTab));

			Type(By.XPath(RoleNameTextBox), roleName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Advanced Settings' Tab:");
			OpenTab(By.XPath(AdvancedSettingsFrameTab));

			WaitAndType(By.XPath(ServiceFeeTextBox), serviceFee);
			Type(By.XPath(NumberInBillingPeriodTextBox), numberInBillingPeriod);
			SlidingSelectByValue(By.XPath("//a[contains(@id, '" + BillingPeriodArrow + "')]"), By.XPath(BillingPeriodDropDown), billingPeriod);

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void AddNewSecurityRoleWithStatus(string roleName, string status)
		{
			WaitAndClick(By.XPath(AddNewRoleButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Security Role:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Basic Settings' Tab:");
			OpenTab(By.XPath(BasicSettingsFrameTab));

			Type(By.XPath(RoleNameTextBox), roleName);
			SlidingSelectByValue(By.XPath("//a[contains(@id, '" + StatusArrow + "')]"), By.XPath(StatusDropDown), status);

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void EditSecurityRole(string roleName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit the Security Role:");
			WaitAndClick(By.XPath("//tr[td[text() ='" + roleName + "']]/td/a[contains(@href, 'Edit')]"));
		}

		public void DeleteSecurityRole(string roleName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the Security Role:");

			EditSecurityRole(roleName);

			WaitAndClick(By.XPath(DeleteRoleFrameButton));
			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}

		public void AddDescriptionToSecurityRole(string roleName, string roleNameDescription)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add description to the Security Role:");

			EditSecurityRole(roleName);

			WaitAndClick(By.XPath(BasicSettingsFrameTab));
			Type(By.XPath(RoleNameDescriptionTextBox), roleNameDescription);

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void AssignSecurityRoleToGroup(string roleName, string roleGroupName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Assign Security Role To a Group:");

			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + FilterByGroupArrow + "')]"), By.XPath(FilterByGroupDropdown), "< All Roles >");

			EditSecurityRole(roleName);

			WaitAndClick(By.XPath(BasicSettingsFrameTab));
			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + FilterByGroupArrow + "')]"), By.XPath(FilterByGroupDropdown), roleGroupName);

			Click(By.XPath(UpdateRoleFrameButton));

			Thread.Sleep(1000);
		}

		public void AddNewSecurityRoleGroup(string roleGroupName)
		{
			WaitAndClick(By.XPath(AddNewRoleGroupButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Security Role Group:");

			WaitAndType(By.XPath(RoleGroupNameTextBox), roleGroupName);

			Click(By.XPath(UpdateGroupFrameButton));

			Thread.Sleep(1000);
		}

		public void EditSecurityRoleGroup(string roleGroupName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit the Security Role Group:");

			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + FilterByGroupArrow + "')]"), By.XPath(FilterByGroupDropdown), roleGroupName);

			WaitAndClick(By.XPath(RoleGroupEditIcon));
		}

		public void AddDescriptionToSecurityRoleGroup(string roleGroupName, string roleGroupNameDescription)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add description to the Security Role Group:");

			EditSecurityRoleGroup(roleGroupName);

			WaitAndType(By.XPath(RoleGroupNameDescriptionTextBox), roleGroupNameDescription);

			Click(By.XPath(UpdateGroupFrameButton));

			Thread.Sleep(1000);
		}

		public void DeleteSecurityRoleGroup(string roleGroupName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the Security Role Group:");

			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + FilterByGroupArrow + "')]"), By.XPath(FilterByGroupDropdown), roleGroupName);

			WaitForElement(By.XPath(RoleGroupDeleteIcon)).WaitTillVisible().Click();
			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();

			Thread.Sleep(3000);
		}
	}
}
