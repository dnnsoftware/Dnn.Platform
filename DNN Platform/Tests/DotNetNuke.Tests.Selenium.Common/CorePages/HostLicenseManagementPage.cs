using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostLicenseManagementPage : BaseOutsidePage
	{
		public HostLicenseManagementPage(IWebDriver driver) : base(driver) { }

		public static string OutsideUrl = "https://www.dnnsoftware.com/login?returnurl=/Support/Success-Network/License-Management";
		public static string WindowTitle = "Secure Login";

	}
}
