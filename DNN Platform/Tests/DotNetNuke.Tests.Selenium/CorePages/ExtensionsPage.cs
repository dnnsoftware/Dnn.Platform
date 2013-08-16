using System.Diagnostics;
using System.IO;
using System.Threading;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class ExtensionsPage : BasePage
	{
		public ExtensionsPage(IWebDriver driver) : base(driver)
		{
		}

		public static string ButtonsList = "//div[contains(@id, '_ModuleContent')]/ul[contains(@class, 'dnnActions')]/li/a";
		public static string InstallExtensionButton = "//a[contains(@id, '_Extensions_cmdInstall')]";

		public static string ModulesAccordion = "//h2[@id='Panel-Modules']/a";
		public static string ModulesExtensionsList = "//table[contains(@id, '_extensionsGrid_0')]//tr";

		//Install Extension page
		public static string UploadFileButton = "//input[contains(@id, '_Install_wizInstall_cmdBrowse')]";
		public static string NextButtonStart = "//a[contains(@id, '_nextButtonStart')]";
		public static string NextButtonStep = "//a[contains(@id, '_nextButtonStep')]";
		public static string Return1Button = "//a[contains(@id, '_finishButtonStep')]";
		public static string AcceptCheckBox = "//input[contains(@id, 'wizInstall_chkAcceptLicense')]";

		public static string ModuleSettingsAccordion = "//h2[contains(@id, '_ModuleEditor_moduleSettingsHead')]/a";
		public static string PackageSettingsAccordion = "//h2[@id='dnnPanel-ExtensionPackageSettings']/a";
		public static string UpdateExtensionButton = "//a[contains(@id, '_EditExtension_cmdUpdate')]";

		public void EditExtension(string extensionName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit the Extension, clicking on Edit icon:");

			IWebElement element = WaitForElement(By.XPath("//tr[td/span[contains(text(), '" + extensionName + "')]]/td/a[contains(@href, 'Edit')]"));
			ScrollIntoView(element, 200);
			element.Click();
		}

		public void InstallExtension(string fileToUpload)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Install extension: " + fileToUpload);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Install Extension Button: ");
			ScrollIntoView(WaitForElement(By.XPath(InstallExtensionButton)), 200);
			FindElement(By.XPath(InstallExtensionButton)).Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Upload File: ");
			WaitForElement(By.XPath(UploadFileButton)).SendKeys(Path.GetFullPath(@"P1\" + fileToUpload));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			Click(By.XPath(NextButtonStart));
			WaitForElement(By.XPath("//h2/span[text() = 'Package Information']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			WaitForElement(By.XPath("//h2/span[text() = 'Release Notes']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			WaitForElement(By.XPath("//h2/span[text() = 'Review License']"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Accept CheckBox: ");
			WaitForElement(By.XPath(AcceptCheckBox)).WaitTillEnabled(30);
			//Trace.WriteLine(WaitForElement(By.XPath(AcceptCheckBox)).Info());
			CheckBoxCheck(By.XPath(AcceptCheckBox));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next Button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			WaitForElement(By.XPath("//h2/span[text() = 'Package Installation Report']"));
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Return Button: ");
			WaitForElement(By.XPath(Return1Button), 60).Click();

			Thread.Sleep(1000);
		}
	}
}
