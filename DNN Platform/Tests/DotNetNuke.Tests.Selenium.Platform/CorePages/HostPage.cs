using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.CorePages
{
	public class HostPage : HostBasePage
	{
		public HostPage (IWebDriver driver) : base (driver) {}

		//public static string SiteGroupsLink = "//div[a/img[contains(@src, 'portalgroups_32X32.png')]]";

		public static int NumberOfLinks = 13;
	}
}
