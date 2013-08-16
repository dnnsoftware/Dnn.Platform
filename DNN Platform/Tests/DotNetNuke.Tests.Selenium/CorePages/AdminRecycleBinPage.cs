using System.Diagnostics;
using System.Threading;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class AdminRecycleBinPage : BasePage
	{
		public AdminRecycleBinPage(IWebDriver driver) : base(driver) { }

		public static string AdminRecycleBinUrl = "/Admin/RecycleBin";

		public static string PageTitleLabel = "Recycle Bin";
		public static string PageHeader = "Recycle Bin";

		public static string PagesTab = "//a[@href = '#rbTabs']";
		public static string ModulesTab = "//a[@href = '#rbModules']";

		public static string RecycleBinPageContainer = "//select[contains(@id, 'RecycleBin_tabsListBox')]";
		public static string RecycleBinPageContainerOption = "//select[contains(@id, 'RecycleBin_tabsListBox')]/option";
		public static string RecycleBinModuleContainer = "//select[contains(@id, 'RecycleBin_modulesListBox')]";
		public static string RecycleBinModuleContainerOption = "//select[contains(@id, 'RecycleBin_modulesListBox')]/option";

		public static string DeleteSelectedPage = "//a[contains(@id, 'RecycleBin_cmdDeleteTab')]";
		public static string DeleteSelectedModule = "//a[contains(@id, 'RecycleBin_cmdDeleteModule')]";
		public static string EmptyRecyclingBin = "//a[contains(@id, 'RecycleBin_cmdEmpty')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminRecycleBinUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminPage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminPage.RecycleBinLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelAdminOption, BasePage.ControlPanelAdminCommonSettings, BasePage.AdminRecycleBinOption);
		}

		public void EmptyRecycleBin()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Empty Recycling Bin:");

			WaitAndClick(By.XPath(EmptyRecyclingBin));
			WaitForConfirmationBox(30);
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}

		public void RemovePage(string pageName)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Remove Page: " + pageName);

			OpenTab(By.XPath(PagesTab));
			WaitAndClick(By.XPath(RecycleBinPageContainerOption + "[contains(text(), '" + pageName  + "')]"));

			ClickOnButton(By.XPath(DeleteSelectedPage));
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}

		public void RemoveModule(string moduleName)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Remove Module: " + moduleName);

			OpenTab(By.XPath(ModulesTab));
			WaitAndClick(By.XPath(RecycleBinModuleContainerOption + "[contains(text(), '" + moduleName + "')]"));

			ClickOnButton(By.XPath(DeleteSelectedModule));
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}
	}
}
