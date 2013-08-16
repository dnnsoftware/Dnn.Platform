using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class UserAccountPage : BasePage
	{
		public UserAccountPage(IWebDriver driver) : base(driver) { }

		public static string UserAccountUrl = "/ActivityFeed/userId/1";

		public static string PageTitleLabel = "Navigation";
		public static string PageHeader = "Activity Feed";

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
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open 'User Account' page:");
			WaitAndClick(By.Id(RegisteredUserLink));
		}

		public void OpenMyProfileInfo()
		{
			WaitAndClick(By.XPath(MyProfileLink));
		}
	}
}
