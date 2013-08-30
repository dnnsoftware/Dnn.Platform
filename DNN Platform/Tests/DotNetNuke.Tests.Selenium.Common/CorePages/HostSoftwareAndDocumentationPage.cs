using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostSoftwareAndDocumentationPage : BaseOutsidePage
	{
		public HostSoftwareAndDocumentationPage(IWebDriver driver) : base(driver) { }

		public static string OutsideUrl = "http://www.dnnsoftware.com/Community/Download/Manuals";
		public static string WindowTitle = "Manuals";

	}
}
