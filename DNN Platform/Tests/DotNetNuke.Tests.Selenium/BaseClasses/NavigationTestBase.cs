using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

namespace DotNetNuke.Tests.DNNSelenium.BaseClasses
{
	class NavigationTestBase
	{
		private IWebDriver _driver;

		internal IWebDriver StartBrowser(string browserType)
		{
			if (_driver != null)
			{
				_driver.Quit();
			}

			switch (browserType)
			{
				case "ie":
					{
						_driver = new InternetExplorerDriver();
						break;
					}
				case "firefox":
					{
						const string fileFirebug = "E:/DotNetNuke/Selenium/firebug-1.11.1-fx.xpi";
						const string fileNetExport = "E:/DotNetNuke/Selenium/netExport-0.9b3.xpi";
						const string resultDir = "E:/DotNetNuke/Results";

						FirefoxProfile firefoxProfile = new FirefoxProfile();
						firefoxProfile.AcceptUntrustedCertificates = true;

						firefoxProfile.AddExtension(fileFirebug);
						firefoxProfile.AddExtension(fileNetExport);

						firefoxProfile.SetPreference("extensions.firebug.currentVersion", "1.11.1"); // Avoid startup screen
						firefoxProfile.SetPreference("extensions.firebug.previousPlacement", 1); //Previous Firebug UI placement within the browser (0-unknown, 1-in browser, 2-in a new window, 3-minimized
						firefoxProfile.SetPreference("extensions.firebug.onByDefault", true);
						firefoxProfile.SetPreference("extensions.firebug.defaultPanelName", "net"); //Panel shown by default; "console", "html", "stylesheet", "script", "dom", "net" + Panels from extensions

						firefoxProfile.SetPreference("extensions.firebug.netexport.alwaysEnableAutoExport", true);
						firefoxProfile.SetPreference("extensions.firebug.netexport.defaultLogDir", resultDir);
						firefoxProfile.SetPreference("extensions.firebug.netexport.showPreview", false);

						firefoxProfile.SetPreference("extensions.firebug.allPagesActivation", "on"); //Specifies whether Firebug shall be enabled by default for all websites
						firefoxProfile.SetPreference("extensions.firebug.net.enableSites", true); //Specifies whether the Net Panel is enabled

						_driver = new FirefoxDriver(firefoxProfile);
						break;
					}
				case "chrome":
					{
						_driver = new ChromeDriver();
						break;
					}
				}

			_driver.Manage().Window.Maximize();


			return _driver;
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			if (_driver != null)
			{
				_driver.Quit();
				_driver = null;
			}
		}
	}
}
