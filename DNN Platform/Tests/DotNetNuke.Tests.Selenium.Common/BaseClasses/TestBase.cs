using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

namespace DNNSelenium.Common.BaseClasses
{
	public class TestBase
	{
		private IWebDriver _driver;

		protected IWebDriver StartBrowser(string browserType)
		{
			if (_driver != null)
			{
				_driver.Quit();
			}

			_driver = StartDriver (browserType);

			return _driver;
		}

		public static IWebDriver StartDriver (string browserType)
		{
			Trace.WriteLine("Start browser: '" + browserType + "'");

			IWebDriver driver = null;
			switch (browserType)
			{
				case "ie":
					{
						driver = new InternetExplorerDriver("Drivers");
						break;
					}
				case "firefox":
					{
						FirefoxProfile firefoxProfile = new FirefoxProfile();
						firefoxProfile.EnableNativeEvents = true;
						firefoxProfile.AcceptUntrustedCertificates = true;

						driver = new FirefoxDriver(firefoxProfile);
						break;
					}
				case "chrome":
					{
						ChromeOptions chromeOptions = new ChromeOptions();
						chromeOptions.AddArgument("--disable-keep-alive");

						driver = new ChromeDriver("Drivers", chromeOptions);
						break;
					}
			}

			//driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(3));
			driver.Manage().Window.Maximize();
			return driver;
		}

		protected void TryTest (Action<XElement> test, XElement settings)
		{
			try
			{
				test(settings);
			}
			catch (Exception e)
			{
				Trace.WriteLine("EXCEPTION =>" + e.Message + e.StackTrace);
				throw;
			}
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			Trace.WriteLine("Stop browser");

			if (_driver != null)
			{
				_driver.Quit();
				_driver = null;
			}
		}
	}
}
