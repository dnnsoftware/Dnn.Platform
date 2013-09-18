using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostActivateYourLicensePage : BasePage
	{
		public HostActivateYourLicensePage(IWebDriver driver) : base(driver) { }

		public static string HostActivateYourLicenseUrl = "/Host/ProfessionalFeatures/ActivateYourLicense";

		public override string PageTitleLabel
		{
			get { return "Activate your License"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Activate your License"; }
		}

		public static string LicensesList = "//table[contains(@id, '_ViewLicx_dgLicense')]/tbody/tr[contains(@id, '_ViewLicx_dgLicense')]";
		public static string NoLicenseMessage = "//div[contains(@class, 'dnnFormWarning')]/span[contains(@id, '_lblNoRecordsTemplate')]";
		public static string NoLicenseMessageText = "No Licenses have been activated.";

		public static string AddLicenseButton = "//a[contains(@id, '_cmdAddLicense')]";
		public static string LicenseTypeArrow = "//a[contains(@id, '_lstLicenseType_Arrow')]";
		public static string LicenseTypeDropDown = "//div[contains(@id, '_lstLicenseType_DropDown')]";
		public static string WebServerArrow = "//a[contains(@id, '_lstServers_Arrow')]";
		public static string WebserverDropDown = "//div[contains(@id, '_lstServers_DropDown')]";
		public static string AccountEmailTextBox = "//input[contains(@id, '_txtAccount')]";
		public static string InvoiceNumberTextBox = "//input[contains(@id, '_txtInvoice')]";
		public static string AutomaticActivationButton = "//a[contains(@id, '_cmdAutoActivation')]";
		public static string SuccessfullActivationConfirmationMessage = "//div[contains(@id, '_ViewLicx_UP')]/div[contains(@class, 'dnnFormSuccess')]";
		public static string SuccessfullDeleteConfirmationMessage = "//div[contains(@id, '_ViewLicx_UP')]/div[contains(@class, 'dnnFormSuccess')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostActivateYourLicenseUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.ActivateYourLicenseLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostAdvancedSettings, ControlPanelIDs.HostActivateYourLicenseOption);
		}

		public void AddLicenseWithAutomaticActivation(string serverName, string accountEmail, string invoiceNumber)
		{
			WaitAndClick(By.XPath(AddLicenseButton));

			WaitForElement(By.XPath(LicenseTypeDropDown));
			SlidingSelectByValue(By.XPath(LicenseTypeArrow), By.XPath(LicenseTypeDropDown), "Production");
			SlidingSelectByValue(By.XPath(WebServerArrow), By.XPath(WebserverDropDown), serverName);

			Type(By.XPath(AccountEmailTextBox), accountEmail);
			Type(By.XPath(InvoiceNumberTextBox), invoiceNumber);

			Click(By.XPath(AutomaticActivationButton));

			Thread.Sleep(1000);

			WaitForElement(By.XPath(SuccessfullActivationConfirmationMessage));
		}

		public void DeleteLicense(string invoiceNumber)
		{
			WaitAndClick(By.XPath("//tr[td[text() = '" + invoiceNumber + "']]/td/input[contains(@id, '_grdRowDelete')]"));

			IAlert alert = _driver.SwitchTo().Alert();
			alert.Accept();

			WaitForElement(By.XPath(SuccessfullDeleteConfirmationMessage));
		}
	}
}
