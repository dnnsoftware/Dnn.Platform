using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostSoftwareAndDocumentationPage : BaseOutsidePage
	{
		public HostSoftwareAndDocumentationPage(IWebDriver driver) : base(driver) { }

		public static string OutsideUrl = "http://www.dnnsoftware.com/Community/Download/Manuals";
		public static string WindowTitle = "Manuals";

	}
}
