using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class PageExportPage : BasePage
	{
		public PageExportPage(IWebDriver driver) : base(driver) { }

		public static string PageExportUrl = "/Home/ctl/ExportTab";

		public override string PageTitleLabel
		{
			get { return "Export Page"; }
		}

		public override string PageHeaderLabel
		{
			get { return ""; }
		}

		public override string PreLoadedModule
		{
			get { return ""; }
		}

		public static string FolderDropDownId = "//div[contains(@id, 'Export_cboFolders')]/div/a";
		public static string TemplateNameTextBox = "//input[contains(@id, 'Export_txtFile')]";
		public static string DescriptionTextBox = "//textarea[contains(@id, 'Export_txtDescription')]";
		public static string IncludeContentCheckBox = "//input[contains(@id, 'Export_chkContent')]";
		public static string ExportButton = "//a[contains(@id, 'Export_cmdExport')]";

		public static string ExportConfirmationMessage = "//div[contains(@id, 'Export_UPPanel')]//div[contains(@id, 'SkinMessage')]";
		public static string ExportConfirmationMessageTxt = "//div[contains(@id, 'Export_UPPanel')]//div[contains(@id, 'SkinMessage')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + PageExportUrl);
		}
	
		public void OpenUsingControlPanel(string pageUrl)
		{
			GoToUrl(pageUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open '" + PageTitleLabel + "' page:");
			SelectMenuOption(ControlPanelIDs.ControlPanelEditPageOption, ControlPanelIDs.ExportPageOption);
		}

		public void ExportPage(string templateName, string description)
		{
			WaitForElement(By.XPath(ControlPanelIDs.PageTitleID));

			LoadableSelectByValue(By.XPath(FolderDropDownId), "Templates");

			Clear(By.XPath(TemplateNameTextBox));

			Type(By.XPath(TemplateNameTextBox), templateName);

			Type(By.XPath(DescriptionTextBox), description);

			CheckBoxCheck( By.XPath(IncludeContentCheckBox));

			Click(By.XPath(ExportButton));

			WaitForElement(By.XPath(ExportConfirmationMessage));
		}
	}
}
