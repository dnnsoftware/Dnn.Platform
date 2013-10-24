using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminExtensionsPage : ExtensionsPage
	{
		public AdminExtensionsPage(IWebDriver driver) : base(driver) { }

		public override string PageTitleLabel
		{
			get { return "Extensions"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Extensions"; }
		}

		public override string PreLoadedModule
		{
			get { return "ExtensionsModule"; }
		}

		public static string AdminExtensionsUrl = "/Admin/Extensions";

		public static string UpdateDesktopModuleButton = "//a[contains(@id, '_ModuleEditor_cmdUpdate')]";
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
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.ExtensionsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings,
			                    ControlPanelIDs.AdminExtensionsOption);
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

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Update Desktop Module Button: ");
			ClickOnButton(By.XPath(UpdateDesktopModuleButton));
		}
	}
}