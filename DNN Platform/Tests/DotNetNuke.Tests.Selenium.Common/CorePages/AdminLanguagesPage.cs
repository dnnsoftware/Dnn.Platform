using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminLanguagesPage : BasePage
	{
		public AdminLanguagesPage(IWebDriver driver) : base(driver) { }

		public static string AdminLanguagesUrl = "/Admin/Languages";

		public override string PageTitleLabel
		{
			get { return "Language Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Languages"; }
		}

		public override string PreLoadedModule
		{
			get { return "LanguagesModule"; }
		}

		public static string LanguagesTab = "//a[@href = '#Languages']";
		public static string EnableLocalizedContent = "//a[contains(@id, '_languageEnabler_cmdEnableLocalizedContent')]";
		public static string DisableLocalizedContent = "//a[contains(@id, '_languageEnabler_cmdDisableLocalization')]";
		public static string TranslatePageCheckBox = "//input[contains(@id, '_EnableLocalizedContent_chkAllPagesTranslatable')]";
		public static string EnableLocalizedContentUpdateButton = "//a[contains(@id, '_EnableLocalizedContent_updateButton')]";
		public static string ProgressBar = "//div[contains(@id, '_EnableLocalizedContent_pageCreationProgressArea_Panel')]";
		public static string LanguagesTable = "//table[contains(@id, 'languageEnabler_languagesGrid')]";
		public static string LocalizationTable = LanguagesTable + "//th[5]/table[contains(@class, 'DnnGridNestedTable')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminLanguagesUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.LanguagesLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminLanguagesOption);
		}

		public string SetLanguageName(string language)
		{
			string option = null;

			switch (language)
			{
				case "en":
					{
						option = "English (United States)";
						break;
					}
				case "de":
					{
						option = "German (Germany)";
						break;
					}
				case "es":
					{
						option = "Spanish (Spain)";
						break;
					}
				case "fr":
					{
						option = "French (France)";
						break;
					}
				case "it":
					{
						option = "Italian (Italy)";
						break;
					}
				case "nl":
					{
						option = "Dutch (Netherlands)";
						break;
					}
			}

			return option;
		} 

		public void EnableLanguage(string packName)
		{
			OpenTab(By.XPath(LanguagesTab));

			WaitForElement(By.XPath(LanguagesTable + "//span[text() = '" + packName + "']")).Info();

			CheckBoxCheck(By.XPath("//tr[td//span[text() = '" + packName + "']]/td/input"));

			Thread.Sleep(1000);
		}

		public void DisableLanguage(string packName)
		{
			OpenTab(By.XPath(LanguagesTab));

			WaitForElement(By.XPath(LanguagesTable + "//span[text() = '" + packName + "']")).Info();

			CheckBoxUncheck(By.XPath("//tr[td//span[text() = '" + packName + "']]/td/input"));

			Thread.Sleep(1000);
		}

		public void PageTranslation(By checkBoxName, CheckBox.ActionType action)
		{
			WaitForElement(checkBoxName);

			if (action == CheckBox.ActionType.Check)
			{
				CheckBoxCheck(checkBoxName);
			}

			else
			{
				CheckBoxUncheck(checkBoxName);
			}
		}

		public void EnableLocalization(CheckBox.ActionType action)
		{
			OpenTab(By.XPath(LanguagesTab));

			WaitAndClick(By.XPath(EnableLocalizedContent));

			PageTranslation(By.XPath(TranslatePageCheckBox), action);

			WaitAndClick(By.XPath(EnableLocalizedContentUpdateButton));

			FindElement(By.XPath(ProgressBar)).WaitTillVisible();

			if (action == CheckBox.ActionType.Check)
			{
				WaitForElement(By.XPath(DisableLocalizedContent), 5*60).WaitTillVisible();
			}
			else
			{
				Thread.Sleep(1000);
			}
		}

		public void DisableLocalization()
		{
			OpenTab(By.XPath(LanguagesTab));

			WaitAndClick(By.XPath(DisableLocalizedContent));

			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}
	}
}
