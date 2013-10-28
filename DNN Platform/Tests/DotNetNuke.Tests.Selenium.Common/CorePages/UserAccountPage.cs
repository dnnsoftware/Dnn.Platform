using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class UserAccountPage : BasePage
	{
		public UserAccountPage(IWebDriver driver) : base(driver) { }

		public static string UserAccountUrl = "/ActivityFeed/userId/1";

		public override string PageTitleLabel
		{
			get { return "Navigation"; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public static string EditProfileButton = "//div[@class='UserProfileControls']/ul/li/a[contains(text(), 'Edit Profile')]";
		public static string MyAccountButton = "//div[@class='UserProfileControls']/ul/li/a[contains(text(), 'My Account')]";

		public static string ActivityFeedLink = "//div[contains(@id, 'ViewConsole_Console')]//h3[text() = 'Activity Feed']";
		public static string FriendsLink = "//div[contains(@id, 'ViewConsole_Console')]//h3[text() = 'Friends']";
		public static string MessagesLink = "//div[contains(@id, 'ViewConsole_Console')]//h3[text() = 'Messages']";
		public static string MyProfileLink = "//div[contains(@id, 'ViewConsole_Console')]//h3[text() = 'My Profile']";

		public static string FriendsNotFoundMessage = "//div[@class = 'dnnFormMessage dnnFormInfo']";
		public static string FriendsNotFoundMessageText = "No members were found that satisfy the search conditions.";
		public static string LocationCity = "//span[contains(@data-bind, 'text: Location()')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'User Account' page:");
			GoToUrl(baseUrl + UserAccountUrl);
		}

		public void OpenUsingLink(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'User Account' page:");
			GoToUrl(baseUrl);
			WaitAndClick(By.XPath(ControlPanelIDs.RegisterLink));
		}

		public void OpenMyProfileInfo()
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Profile' button:");
			WaitAndClick(By.XPath(MyProfileLink));
		}

		public void OpenFriendsLink(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Open 'Friends' Link:");
			OpenUsingLink(baseUrl);
			WaitAndClick(By.XPath(UserAccountPage.FriendsLink));

			Thread.Sleep(1000);
		}
	}
}
