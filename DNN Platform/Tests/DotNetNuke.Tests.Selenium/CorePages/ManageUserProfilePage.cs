using System.Diagnostics;
using System.Threading;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class ManageUserProfilePage : BasePage
	{
		public ManageUserProfilePage(IWebDriver driver) : base(driver) { }

		public static string PageTitleLabel = "Profile";
		public static string PageHeader = "Activity Feed";

		public static string ManageAccountTab = "//div[@id='dnnEditUser']//a[contains(@href, 'dnnUserDetails')]";
		public static string ManageProfileTab = "//div[@id='dnnEditUser']//a[contains(@href, 'dnnProfileDetails')]";

		public static string ManagePasswordAccordion = "//div[@id = 'dnnUserDetails']//h2[@id='H1']/a";
		public static string AddressDetailsAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Address Details')]";
		public static string LocationInformationAccordion = "//div[contains(@id, '_Profile_ProfileProperties')]/h2/a[contains(text(), 'Location Information')]";
		public static string CityTextBox = "//input[contains(@id, 'ProfileProperties_City')]";

		public static string UpdateButton = "//a[contains(@id, 'Profile_cmdUpdate')]";


		public void OpenUsingLink(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			WaitAndClick(By.Id(RegisteredUserLink));
			WaitAndClick(By.XPath(UserAccountPage.MyAccountButton));
		}

		/*public void OpenManageProfileTab()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage Profile' button:");
			WaitAndClick(By.XPath(ManageProfileTab));
		}*/

		/*public void ClickOnUpdateButton()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			FindElement(By.XPath(UpdateButton)).ScrollIntoView().WaitTillVisible().Click();
		}*/

		public void AddCity(string city)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add City to Location Information:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Manage Profile' button:");
			OpenTab(By.XPath(ManageProfileTab));

			AccordionOpen(By.XPath(LocationInformationAccordion));

			WaitAndType(By.XPath(CityTextBox), city);

			//ClickOnUpdateButton();
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(1000);
		}

	}
}
