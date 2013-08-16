using System.Collections.Generic;
using System.Diagnostics;
using DotNetNuke.Tests.DNNSelenium.BaseClasses;
using OpenQA.Selenium;

namespace DotNetNuke.Tests.DNNSelenium.CorePages
{
	class HostSettingsPage : BasePage
	{
		public HostSettingsPage(IWebDriver driver) : base(driver) { }

		public static string HostSettingsUrl = "/Host/HostSettings";

		public static string PageTitleLabel = "Host Settings";
		public static string PageHeader = "Host Settings";

		public static string LogsTab = "//li[@aria-controls = 'logSettings']/a";
		public static string LogFilesDropDownArrow = "HostSettings_ddlLogs_Arrow";
		public static string LogFilesDropDownList = "//div[contains(@id, 'HostSettings_ddlLogs_DropDown')]";
		public static string OptionOne = "log.resources";
		public static string OptionTwo = "InstallerLog";
		public static string LogContent = "//textarea[contains(@id, 'HostSettings_txtLogContents')]";

		# region Dictionary
		private static readonly Dictionary<string, string> EnDictionary = new Dictionary<string, string>
			                                                {
				                                                {"Successfull Log", "the script ran successfully"},				                                        
															};

		private static readonly Dictionary<string, string> DeDictionary = new Dictionary<string, string>
			                                                {
				                                                {"Successfull Log", "Skript erfolgreich ausgeführt wurde"},
															};

		private static readonly Dictionary<string, string> EsDictionary = new Dictionary<string, string>
			                                                {
				                                                {"Successfull Log", "script se ejecutó correctamente"},
															};

		private static readonly Dictionary<string, string> FrDictionary = new Dictionary<string, string>
			                                                {
				                                                {"Successfull Log", "généralement qu'il s'exécute correctement"},
															};

		private static readonly Dictionary<string, string> ItDictionary = new Dictionary<string, string>
			                                                {
				                                                {"Successfull Log", "Questo indica in genere che funziona correttamente"},
															};

		private static readonly Dictionary<string, string> NlDictionary = new Dictionary<string, string>
			                                                {
				                                                {"Successfull Log", "script succesvol uitgevoerd is"},
															};
		private static readonly Dictionary<string, Dictionary<string, string>> DictSelector = new Dictionary
			<string, Dictionary<string, string>>
			                                                {
																{"en", EnDictionary},
																{"de", DeDictionary},
																{"es", EsDictionary},
																{"fr", FrDictionary},
																{"it", ItDictionary},
																{"nl", NlDictionary},
			                                                };
		#endregion Dictionary

		public static string BasicSettingsTab = "//li[@aria-controls = 'basicSettings']/a";
		public static string HostName = "//span[contains(@id, '_HostSettings_lblHostName')]";

		public void SetDictionary(string language)
		{
			_translate = DictSelector[language];
		}
				
		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostSettingsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostPage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostPage.HostSettingsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(BasePage.ControlPanelHostOption, BasePage.ControlPanelHostCommonSettings, BasePage.HostHostSettingsOption);
		}

	}
}
 