using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	class HostHtmlEditorManagerPage : BasePage
	{
		public HostHtmlEditorManagerPage(IWebDriver driver) : base(driver) { }

		public static string HostHtmlEditorManagerUrl = "/Host/HTMLEditorManager";

		public override string PageTitleLabel
		{
			get { return "HTML Editor Manager"; }
		}

		public override string PageHeaderLabel
		{
			get { return "HTML Editor Manager"; }
		}

		public override string PreLoadedModule
		{
			get { return "RadEditorManagerModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostHtmlEditorManagerUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.HtmlEditorManagerLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostAdvancedSettings, ControlPanelIDs.HostHtmlEditorManagerOption);
		}
	}
}
