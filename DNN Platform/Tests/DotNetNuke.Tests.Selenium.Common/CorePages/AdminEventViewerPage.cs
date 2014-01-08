using System;
using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminEventViewerPage : BasePage 
	{
		public AdminEventViewerPage(IWebDriver driver) : base(driver) { }

		public static string AdminEventViewerUrl = "/Admin/LogViewer";

		public override string PageTitleLabel
		{
			get { return "Log Viewer"; }
		}
		
		public override string PageHeaderLabel
		{
			get { return "Event Viewer"; }
		}

		public override string PreLoadedModule
		{
			get { return "LogViewerModule"; }
		}

		public static string TypeDropDownArrow = "LogViewer_ddlLogType_Arrow";
		public static string TypeDropDownList = "//div[contains(@id, 'LogViewer_ddlLogType_DropDown')]";

		public static string NoLogItemsMessage = "//div[contains(@id, 'LogViewer_UP')]/div/span[contains(@id, 'lblMessage')]";
		public static string NoLogItemsMessageText = "There are no log items.";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminEventViewerUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.EventViewerLink));
		}

        public void EditLogSettings(string roleName)
        {
            Trace.WriteLine(BasePage.TraceLevelPage + "Edit the Log Settings:");
            string xpath = "//tr[td[text() ='" + roleName + "']]/td/input[contains(@src, 'Edit')]";
            //try
            //{
            //    WaitAndClick(By.XPath(xpath));
            //}
            //catch
            //{
            WaitScrollAndClick(By.XPath(xpath));
            //}
        }

        public void EnableLogItem(string itemName)
        {
            EditLogSettings(itemName);
            try
            {
                if (!ElementPresent(By.XPath("//span[contains(@class,'dnnCheckbox dnnCheckbox-checked')]")))
                {
                    string checkBoxName = "//input[contains(@id, '_EditLogTypes_chkIsActive')]";
                    CheckBoxCheck(By.XPath(checkBoxName));
                Thread.Sleep(500);
                    ClickOnButton(By.XPath("//a[contains(@id, '_EditLogTypes_cmdUpdate')]"));
                }
                else { ClickOnButton(By.XPath("//a[contains(@id, '_EditLogTypes_cmdCancel')]")); }
                Thread.Sleep(500);
            }
            catch
            {
                Trace.WriteLine("ELEMENT Not found");
            }
        }

        public void DisableLogItem(string itemName)
        {
            EditLogSettings(itemName);
            try
            {
                if (ElementPresent(By.XPath("//span[contains(@class,'dnnCheckbox dnnCheckbox-checked')]")))
                {
                    string checkBoxName = "//input[contains(@id, '_EditLogTypes_chkIsActive')]";
                    CheckBoxUncheck(By.XPath(checkBoxName));
                Thread.Sleep(500);
                    ClickOnButton(By.XPath("//a[contains(@id, '_EditLogTypes_cmdUpdate')]"));
                }
                else { ClickOnButton(By.XPath("//a[contains(@id, '_EditLogTypes_cmdCancel')]")); }
                Thread.Sleep(500);
            }
            catch
            {
                Trace.WriteLine("ELEMENT Not found");
            }
        }

        public void SetupEventViewer(string[] logsettingitems, bool isdisabled = false)
        {
            ClickOnButton(By.Id("dnn_ctr455_LogViewer_editSettings"));
            try
            {
                if (isdisabled)
                {
                    foreach (string item in logsettingitems) { DisableLogItem(item); }
                }

                foreach (string item in logsettingitems) { EnableLogItem(item); }
            }
            catch (Exception ex) { Trace.WriteLine(ex.ToString()); }
        }
        public void ClearEventViewer()
        {
            ClickOnButton(By.XPath("//a[contains(@id, '_LogViewer_btnClear')]"));
            WaitForConfirmationBox(15);
            ClickYesOnConfirmationBox();
        }
		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminCommonSettings, ControlPanelIDs.AdminEventViewerOption);
		}

		public void FilterByType(string type)
		{
			SlidingSelectByValue( By.XPath("//a[contains(@id, '" + TypeDropDownArrow + "')]"), By.XPath(TypeDropDownList), type);

			Thread.Sleep(1000);
			WaitForElement(By.XPath("//div[contains(@class, 'dnnlvContent')]/div[last()]")).WaitTillEnabled();
		}
	}
}
