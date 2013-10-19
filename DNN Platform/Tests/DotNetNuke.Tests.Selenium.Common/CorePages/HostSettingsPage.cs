using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.Properties;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostSettingsPage : BasePage
	{
		public HostSettingsPage(IWebDriver driver) : base(driver) { }

		public static string HostSettingsUrl = "/Host/HostSettings";

		public override string PageTitleLabel
		{
			get { return "Host Settings"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Host Settings"; }
		}

		public override string PreLoadedModule
		{
			get { return "HostSettingsModule"; }
		}

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

		public static string OtherSettingsTab = "//li[@aria-controls = 'otherSettings']/a";
		public static string AllowContentLocalizationCheckBox = "//input[contains(@id, '_chkEnableContentLocalization')]";

		public static string UpdateButton = "//a[contains(@id, '_HostSettings_cmdUpdate')]";

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
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.HostSettingsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostCommonSettings, ControlPanelIDs.HostHostSettingsOption);
		}

		public void EnableContentLocalization()
		{
			OpenTab(By.XPath(OtherSettingsTab));

			CheckBoxCheck(By.XPath(AllowContentLocalizationCheckBox));

			ClickOnButton(By.XPath(UpdateButton));

			Thread.Sleep(Settings.Default.WaitFactor * 1000);
		}
	}
}
 