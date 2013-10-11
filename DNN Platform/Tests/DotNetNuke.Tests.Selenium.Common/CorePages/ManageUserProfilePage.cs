using System.Diagnostics;
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
		
		public static string ManageAccountTab = "//div[@id='dnnEditUser']//a[contains(@href, 'dnnUserDetails')]";
		public static string ManageProfileTab = "//div[@id='dnnEditUser']//a[contains(@href, 'dnnProfileDetails')]";

		public static string AccountSettings = "//h2[@id='dnnPanel-AccountSettings']/a";
		public static string DisplayName = "//input[contains(@id, '_displayName_TextBox')]";

		public static string ManagePasswordAccordion = "//div[@id = 'dnnUserDetails']//h2[@id='H1']/a";

		public static string AddressDetailsAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Address Details')]";

		public static string LocationInformationAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Location Information')]";
		public static string CityTextBox = "//input[contains(@id, 'ProfileProperties_City')]";

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

	}
}
