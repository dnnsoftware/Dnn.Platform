using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.CorePages
{
	public class AdminPageManagementPage : BasePage
	{
		public AdminPageManagementPage(IWebDriver driver) : base(driver) { }

		public static string AdminPageManagementUrl = "/Admin/Pages";

		public override string PageTitleLabel
		{
			get { return "Page Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Page Management"; }
		}

		public override string PreLoadedModule
		{
			get { return "PagesModule"; }
		}

		public static string PageList = "//div[contains(@id, 'Tabs_ctlPages')]";

		public static string WebsitePagesRadioButton = "//input[contains(@id, 'Tabs_rblMode_0')]";
		public static string HostPagesRadioButton = "//input[contains(@id, 'Tabs_rblMode_1')]";
		public static string ExpandAllLink = "//a[contains(@id, 'Tabs_cmdExpandTree')]";
		public static string HostPage = "//div[contains(@id, 'Tabs_ctlPages')]//span[text() = 'Host ']";

		public static string ContextMenuAddPageOption = "//div[contains(@id, 'ctlPages_ctlContext_detached')]//a[span[contains(text(),'Add Page')]]";
		public static string ContextMenuDeletePageOption = "//div[contains(@id, 'ctlPages_ctlContext_detached')]//a[span[contains(text(),'Delete')]]";

		public static string PageNameTextBox = "//textarea[contains(@id, 'Tabs_txtBulk')]";
		public static string CreatePageButton = "//a[contains(@id, 'Tabs_btnBulkCreate')]";
		public static string OperationConfirmationMessage = "//div[contains(@id, 'Tabs_UP')]/div/span[contains(@id, 'lblMessage')]";

		public static string UpdatePageButton = "//a[contains(@id, 'Tabs_cmdUpdate')]";
		public static string SEOSettingsAccordion = "//h2[@id='Panel-SEO']/a";
		public static string PageDescriptionTextBox = "//textarea[contains(@id, 'Tabs_txtDescription')]";

		public static string ModulesAccordion = "//h2[@id='Panel-Modules']/a";
		public static string DeleteHTMLIcon = "//tr[td[contains(text(), 'HTML')]]/td/input[contains(@id, 'cmdDeleteModule')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminPageManagementUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.PageManagementLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminCommonSettings, ControlPanelIDs.AdminPageManagementOption);
		}

		public void OpenPageList(string pageType, string waitForPageName)
		{
			Thread.Sleep(1000);
			Trace.WriteLine(BasePage.TraceLevelPage + "Select Page type :");
			if (pageType == "Web")
			{
				Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'WEB' button :");
				//RadioButtonSelect(By.XPath(WebsitePagesRadioButton));
			}
			else
			{
				Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'HOST' button :");
				RadioButtonSelect(By.XPath(HostPagesRadioButton));
				WaitForElement(By.XPath(HostPage));
			}

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on link 'Expand All'");
			WaitAndClick(By.XPath(ExpandAllLink));
			WaitForElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = '" + waitForPageName + " ']")).Info();
		}

		public void SelectFromContextMenu(string pageName, string option)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Select from Context menu:");
			Actions builder = new Actions(_driver);
			builder.ContextClick(
				FindElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = '" + pageName + " ']"))).
				MoveToElement(WaitForElement(By.XPath(option))).Build().Perform();

			FindElement(By.XPath(option)).Info();

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on option: " + option);
			Click(By.XPath(option));
		}

		public void AddPage(string pageName, string pageType, string addPageAfter)
		{
			OpenPageList(pageType, addPageAfter);

			SelectFromContextMenu(addPageAfter, ContextMenuAddPageOption);

			WaitAndType(By.XPath(PageNameTextBox), pageName);

			Click(By.XPath(CreatePageButton));

			WaitForElement(By.XPath(OperationConfirmationMessage)); 
		}

		public void AddPages(string pageName1, string pageName2, string pageName3, string pageName4, string pageType, string addPageAfter)
		{
			OpenPageList(pageType, addPageAfter);

			SelectFromContextMenu(addPageAfter, ContextMenuAddPageOption);

			WaitAndType(By.XPath(PageNameTextBox), pageName1);
			Type(By.XPath(PageNameTextBox), Keys.Enter);

			Type(By.XPath(PageNameTextBox), pageName2);
			Type(By.XPath(PageNameTextBox), Keys.Enter);

			Type(By.XPath(PageNameTextBox), ">" + pageName3);
			Type(By.XPath(PageNameTextBox), Keys.Enter);

			Type(By.XPath(PageNameTextBox), ">" + pageName4);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Create Page' button:");
			Click(By.XPath(CreatePageButton));

			WaitForElement(By.XPath(OperationConfirmationMessage));
		}

		public void AddPagesInBulk(string pageName, int pageAmount, string pageType, string addPageAfter)
		{
			OpenPageList(pageType, addPageAfter);

			SelectFromContextMenu(addPageAfter, ContextMenuAddPageOption);

			WaitForElement(By.XPath(PageNameTextBox));

			int pageNumber = 1;
			while (pageNumber < pageAmount + 1)
			{
				Type(By.XPath(PageNameTextBox), pageName + pageNumber);
				Type(By.XPath(PageNameTextBox), Keys.Enter);
				pageNumber++;
			}
			
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Create Page' button:");
			Click(By.XPath(CreatePageButton));

			WaitForElement(By.XPath(OperationConfirmationMessage));
		}
		public void DeletePage(string pageName, string pageType)
		{
			OpenPageList(pageType, pageName);

			SelectFromContextMenu(pageName, ContextMenuDeletePageOption);

			//IAlert alert = _driver.SwitchTo().Alert();
			//alert.Accept();
			WaitForConfirmationBox(30);
			ClickYesOnConfirmationBox();

			WaitForElement(By.XPath(OperationConfirmationMessage));
		}

		public void MovePage(string pageName, string newLocation, string pageType)
		{
			OpenPageList(pageType, pageName);

			Actions action = new Actions(_driver);
			action.DragAndDrop(FindElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = '" + pageName + " ']")),
								FindElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = '" + newLocation + " ']"))).
								Build().
								Perform();

			WaitForElement(By.XPath(OperationConfirmationMessage));
		}

		public void AddDescriptionToPage(string pageName, string pageDescription, string pageType)
		{
			OpenPageList(pageType, pageName);

			WaitForElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = '" + pageName + " ']")).Click();

			AccordionOpen(By.XPath(SEOSettingsAccordion));

			WaitAndType(By.XPath(PageDescriptionTextBox), pageDescription);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdatePageButton));

			Thread.Sleep(1000);
			//WaitForElement(By.XPath(OperationConfirmationMessage));
		}

		public void DeleteModuleOnPage(string pageName, string pageType)
		{
			OpenPageList(pageType, pageName);

			WaitForElement(By.XPath("//div[contains(@id, 'Tabs_ctlPages')]//span[text() = '" + pageName + " ']")).ScrollIntoView().Click();

			AccordionOpen(By.XPath(ModulesAccordion));

			WaitAndClick(By.XPath(DeleteHTMLIcon));

			WaitForElement(By.XPath("//div[contains(@class, 'dnnFormWarning')]/span[contains(@id, '_lblNoRecords')]"));
		}
	}
}
