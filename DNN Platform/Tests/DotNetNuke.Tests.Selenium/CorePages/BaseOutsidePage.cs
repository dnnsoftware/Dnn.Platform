using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class BaseOutsidePage
	{
		protected readonly IWebDriver _driver;

		private BaseOutsidePage() {}
		public BaseOutsidePage (IWebDriver driver)
		{
			_driver = driver;
		}

		public void Click(By locator)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Click on: '" + locator + "'");
			_driver.FindElement(locator).Click();
		}

		public string CurrentWindowTitle()
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "The current window title is: '" + _driver.Title + "'");
			return _driver.Title;
		}

		public string CurrentWindowUrl()
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "The current window url is: '" + _driver.Url + "'");
			return _driver.Url;
		}

		public void GoToUrl(string url)
		{
			Trace.WriteLine(BasePage.TraceLevelLow + "Open url: http://" + url);
			if (_driver != null) _driver.Navigate().GoToUrl("http://" + url);
		}

	}
}
