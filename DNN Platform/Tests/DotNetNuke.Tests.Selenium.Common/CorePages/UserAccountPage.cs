using System.Diagnostics;
using System.IO;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

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

			Thread.Sleep(1000);
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

		public void OpenMessagesLink(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Open 'Messages' Link:");
			OpenUsingLink(baseUrl);
			WaitAndClick(By.XPath(UserAccountPage.MessagesLink));

			Thread.Sleep(1000);
		}

		public void OpenActivityFeedLink(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Open 'Activity Feed' Link:");
			OpenUsingLink(baseUrl);
			WaitAndClick(By.XPath(UserAccountPage.ActivityFeedLink));

			Thread.Sleep(1000);
		}

	}
}
