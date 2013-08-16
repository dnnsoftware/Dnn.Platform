using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostTechnicalSupportPage : BaseOutsidePage
	{
		public HostTechnicalSupportPage(IWebDriver driver) : base(driver) { }

		public static string OutsideUrl = "http://customers.dnnsoftware.com/Main/Default.aspx";
		public static string WindowTitle = "Portal - DNN";

	}
	
}
