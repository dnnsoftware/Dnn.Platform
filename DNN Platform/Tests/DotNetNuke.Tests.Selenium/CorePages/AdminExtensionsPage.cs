using System.Diagnostics;
using System.Threading;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	internal class AdminExtensionsPage : ExtensionsPage
	{
		public AdminExtensionsPage(IWebDriver driver) : base(driver)
		{
		}

		public static string PageTitleLabel = "Extensions";
		public static string PageHeader = "Extensions";

		public static string AdminExtensionsUrl = "/Admin/Extensions";

		public static string UpdateDesctopModuleButton = "//a[contains(@id, '_ModuleEditor_cmdUpdate')]";
		public static string PermissionTable = "//div[contains(@class, 'dnnPermissionsGrid')]//tr[td[text() = ";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminExtensionsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.ExtensionsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminAdvancedSettings,
			                    BasePage.AdminExtensionsOption);
		}

		public void EditExtensionPermissions(string extensionName)
		{
			EditExtension(extensionName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Open Module Settings accordion: ");
			AccordionOpen(By.XPath(ModuleSettingsAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Permission table: ");
			IWebElement element = WaitForElement(By.XPath(PermissionTable + "'All Users'" + "]]/td/img"));
			ScrollIntoView(element, 200);
			element.Click();
			//Click(By.XPath(PermissionTable + "'All Users'" + "]]/td/img"));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Update Desctop Module Button: ");
			ClickOnButton(By.XPath(UpdateDesctopModuleButton));
		}
	}
}