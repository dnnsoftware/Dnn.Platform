using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class PageImportPage : BasePage
	{
		public PageImportPage(IWebDriver driver) : base(driver) { }

		public static string PageImportUrl = "/Home/ctl/ImportTab";

		public override string PageTitleLabel
		{
			get { return "Import Page"; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public static string FolderDropDownId = "//div[contains(@id, 'Import_cboFolders')]/div/a";
		public static string TemplateDropDownArrow = "Import_cboTemplate_Arrow";
		public static string TemplateDropDownId = "//div[contains(@id, 'Import_cboTemplate_DropDown')]";

		public static string CreateNewPageRadioButton = "//input[contains(@id, 'Import_optMode_0')]";
		public static string ReplaceCurrentPageRadioButton = "//input[contains(@id, 'Import_optMode_1')]";

		public static string PageNameTextBox = "//input[contains(@id, 'Import_txtTabName')]";

		public static string InsertPageBeforeRadioButton = "//input[contains(@id, 'Import_rbInsertPosition_0')]";
		public static string InsertPageAfterRadioButton = "//input[contains(@id, 'Import_rbInsertPosition_1')]";
		public static string InsertPageAddToEndRadioButton = "//input[contains(@id, 'Import_rbInsertPosition_2')]";

		public static string PagePositionDropDownArrow = "Import_cboPositionTab_Arrow";
		public static string PagePositionDropDownId = "//div[contains(@id, 'Import_cboPositionTab_DropDown')]";
		public static string PagePositionDropDownList = "//div[contains(@id, 'Import_cboPositionTab_DropDown')]//li";

		public static string ViewImportedPageRadioButton = "//input[contains(@id, 'Import_optRedirect_0')]";
		public static string EditImportedPageRadioButton = "//input[contains(@id, 'Import_optRedirect_1')]";

		public static string ImportPageButton = "//a[contains(@id, 'Import_cmdImport')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + PageImportUrl);
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			SelectMenuOption(ControlPanelIDs.ControlPanelPagesOption, ControlPanelIDs.PagesImportPageOption);
		}

		public void ImportPage(string templateName, string pageName, string insertPageAfter)
		{
			WaitForElement(By.XPath(ControlPanelIDs.PageTitleID));

			LoadableSelectByValue(By.XPath(FolderDropDownId), "Templates");

			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + TemplateDropDownArrow + "')]"), By.XPath(TemplateDropDownId), templateName);

			RadioButtonSelect(By.XPath(CreateNewPageRadioButton));

			Clear(By.XPath(PageNameTextBox));
			Type(By.XPath(PageNameTextBox), pageName);

			RadioButtonSelect(By.XPath(InsertPageAfterRadioButton));

			SlidingSelectByValue(By.XPath("//a[contains(@id, '" + PagePositionDropDownArrow + "')]"), By.XPath(PagePositionDropDownId), insertPageAfter);

			RadioButtonSelect(By.XPath(ViewImportedPageRadioButton));

			Click(By.XPath(ImportPageButton));

			Thread.Sleep(1000);
		}
	}
}
