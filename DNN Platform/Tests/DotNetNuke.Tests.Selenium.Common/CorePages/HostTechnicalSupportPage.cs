using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostTechnicalSupportPage : BaseOutsidePage
	{
		public HostTechnicalSupportPage(IWebDriver driver) : base(driver) { }

		public static string OutsideUrl = "http://customers.dnnsoftware.com/Main/Default.aspx";
		public static string WindowTitle = "Portal - DNN";

	}
	
}
