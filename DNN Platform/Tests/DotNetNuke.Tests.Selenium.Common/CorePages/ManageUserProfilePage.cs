using System.Diagnostics;
using System.IO;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class ManageUserProfilePage : BasePage
	{
		public ManageUserProfilePage(IWebDriver driver) : base(driver) { }

		public override string PageTitleLabel
		{
			get { return "Profile"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Activity Feed"; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public static string ProfilePhoto = "//div[@id = 'UserProfileImg']/span/img[@class = 'ProfilePhoto']";
		public static string ManageAccountTab = "//div[@id='dnnEditUser']//a[contains(@href, 'dnnUserDetails')]";
		public static string ManageProfileTab = "//div[@id='dnnEditUser']//a[contains(@href, 'dnnProfileDetails')]";

		public static string AccountSettings = "//h2[@id='dnnPanel-AccountSettings']/a";
		public static string DisplayName = "//input[contains(@id, '_displayName_TextBox')]";

		public static string ManagePasswordAccordion = "//div[@id = 'dnnUserDetails']//h2[@id='H1']/a";
		public static string CurrentPasswordTextBox = "//input[contains(@id, '_EditUser_Password_txtOldPassword')]";
		public static string NewPasswordTextBox = "//input[contains(@id, '_EditUser_Password_txtNewPassword')]";
		public static string ConfirmPasswordTextBox = "//input[contains(@id, '_EditUser_Password_txtNewConfirm')]";
		public static string ChangePasswordButton = "//a[contains(@id, '_EditUser_Password_cmdUpdate')]";

		public static string AddressDetailsAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Address Details')]";

		public static string LocationInformationAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Location Information')]";
		public static string CityTextBox = "//input[contains(@id, 'ProfileProperties_City')]";

		public static string PreferencesAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Preferences')]";

		public static string ProfileBasicAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2[@id = 'Basic']/a";
		public static string UploadFileButton = "//span/input[@name = 'postfile']";
		public static string FileArrow = "dnn_ctr_EditUser_Profile_ProfileProperties_Photo_PhotoFileControl_FilesComboBox_Arrow";
		public static string FileDropDown = "//div[contains(@id, '_PhotoFileControl_FilesComboBox_DropDown')]";
		public static string PhotoField = "//div[contains(@id, '_EditUser_Profile_ProfileProperties_Photo')]";

		public static string VisibilityPublicOption = "//div[@class = 'dnnFormVisibility dnnDropdownSettings']//input[@type = 'radio' and '@value = '0']";
		public static string VisibilityMembersOnlyOption = "//div[@class = 'dnnFormVisibility dnnDropdownSettings']//input[@type = 'radio' and @value = '1']";
		public static string VisibilityAdminOnlyOption = "//div[@class = 'dnnFormVisibility dnnDropdownSettings']//input[@type = 'radio' and @value = '2']";
		public static string VisibilityFriendsAndGroupsOption = "//div[@class = 'dnnFormVisibility dnnDropdownSettings']//input[@type = 'radio' and @value = '3']";

		public static string UpdateButton = "//a[contains(@id, 'Profile_cmdUpdate')]";


		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");

			string selector = WaitForElement(By.XPath(ControlPanelIDs.CompanyLogo)).GetAttribute("src");

			Trace.WriteLine(BasePage.TraceLevelElement + selector);

			if (selector.EndsWith(ControlPanelIDs.AwesomeCycles))
			{
				WaitAndClick(By.XPath(ControlPanelIDs.RegisterLink));
				WaitAndClick(By.XPath(UserAccountPage.MyAccountButton));
			}
			else
			{
				WaitAndClick(By.XPath(ControlPanelIDs.SocialUserLink));
				WaitAndClick(By.XPath(ControlPanelIDs.SocialEditProfile));
			}
			
		}

		public void AddCity(string city)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add City to Location Information:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage Profile' button:");
			OpenTab(By.XPath(ManageProfileTab));

			AccordionOpen(By.XPath(LocationInformationAccordion));

			WaitAndType(By.XPath(CityTextBox), city);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void AddProfileAvatar(string fileToUpload)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add Avatar on Preferences tab:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage  Profile' button:");
			OpenTab(By.XPath(ManageProfileTab));

			AccordionOpen(By.XPath(ProfileBasicAccordion));

			WaitForElement(By.XPath(UploadFileButton)).SendKeys(Path.GetFullPath(@"P1\" + fileToUpload));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void ChangeProfileAvatar(string fileToUpload, string option)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Remove Avatar on Preferences tab:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage  Profile' button:");
			OpenTab(By.XPath(ManageProfileTab));

			AccordionOpen(By.XPath(ProfileBasicAccordion));

			SlidingSelectByValue(By.Id(FileArrow), By.XPath(FileDropDown), option);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void SetVisibilityPermission(string field, string option)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Set Visibility Permission:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage Profile' button:");
			OpenTab(By.XPath(ManageProfileTab));

			AccordionOpen(By.XPath(ProfileBasicAccordion));

			WaitAndClick(By.XPath(field + "//div[@class = 'dnnButtonArrow']"));
			RadioButtonSelect(By.XPath(field + option));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

		public void ChangePassword(string oldPassword, string newPassword)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Change Password:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage  Account' button:");
			OpenTab(By.XPath(ManageAccountTab));

			AccordionOpen(By.XPath(ManagePasswordAccordion));

			WaitAndType(By.XPath(CurrentPasswordTextBox), oldPassword);
			Type(By.XPath(NewPasswordTextBox), newPassword);
			Type(By.XPath(ConfirmPasswordTextBox), newPassword);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(ChangePasswordButton));

			Thread.Sleep(1000);
		}
	}
}
