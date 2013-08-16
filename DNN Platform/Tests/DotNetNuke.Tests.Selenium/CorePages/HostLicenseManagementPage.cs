using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostLicenseManagementPage : BaseOutsidePage
	{
		public HostLicenseManagementPage(IWebDriver driver) : base(driver) { }

		public static string OutsideUrl = "http://www.dnnsoftware.com/tabid/1205/Default.aspx";
		public static string WindowTitle = "Secure Login";

	}
}
