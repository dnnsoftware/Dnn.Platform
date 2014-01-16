using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.Properties;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostSiteManagementPage : BasePage
	{
		public HostSiteManagementPage(IWebDriver driver) : base(driver) { }

		public static string HostSiteManagementUrl = "/Host/SiteManagement";

		public override string PageTitleLabel
		{
			get { return "Site Management"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Site Management"; }
		}

		public override string PreLoadedModule
		{
			get { return "SiteManagementModule"; }
		}

		public static string PortalsTable = "//div[contains(@id, '_Portals_grdPortals')]";
		public static string PortalsList = "//tr[contains(@id, 'Portals_grdPortals')]";
		public static string AddNewSiteButton = "//a[contains(@id, 'Portals_createSite')]";
		public static string ExportSiteTemplateButton = "//a[contains(@id, 'Portals_exportSite')]";

		public static string ParentSiteRadioButton = "dnn_ctr321_Signup_optType_0";
		public static string ChildSiteRadioButton = "dnn_ctr321_Signup_optType_1";
		public static string SiteAliasTextBox = "//input[contains(@id, 'Signup_txtPortalAlias')]";
		public static string TitleTextBox = "//input[contains(@id, 'Signup_txtPortalName')]";
		public static string AdminNameTextBox = "//input[contains(@id, 'Signup_txtUsername')]";
		public static string FirstNameTextBox = "//input[contains(@id, 'Signup_txtFirstName')]";
		public static string LastNameTextBox = "//input[contains(@id, 'Signup_txtLastName')]";
		public static string EmailTextBox = "//input[contains(@id, 'Signup_txtEmail')]";
		public static string PasswordTextBox = "//input[contains(@id, 'Signup_txtPassword')]";
		public static string ConfirmTextBox = "//input[contains(@id, 'Signup_txtConfirm')]";
		public static string EnableCurrentUserAsAdmin = "//input[contains(@id, 'Signup_useCurrent')]";
		public static string TemplateArrow = "//a[contains(@id, 'Signup_cboTemplate_Arrow')]";
		public static string TemplateDropDown = "//div[contains(@id, '_Signup_cboTemplate_DropDown')]";

		public static string SiteNameDescriptionTextBox = "//textarea[contains(@id, 'SiteSettings_txtDescription')]";

		public static string CreateSiteFrameButton = "//a[contains(@id, 'Signup_cmdUpdate')]";
		public static string UpdateFrameButton = "//a[contains(@id, 'SiteSettings_cmdUpdate')]";
		public static string DeleteFrameButton = "//a[contains(@id, 'SiteSettings_cmdDelete')]";
		public static string CancelButton = "//a[contains(@id, 'Signup_cmdCancel')]";

		public static string SiteCreatedConfirmationMessage = "//div[contains(@id, 'Signup_UP')]/div[contains(@id, 'dnnSkinMessage')]/span[contains(@id, 'lblMessage')]";
		public static string SiteCreatedConfirmationMessageText = "There was an error sending confirmation emails - There is a problem with the configuration of your SMTP Server. Mail was not sent.. However, the website was created.";

		public static string BasicSettingsFrameTab = "//a[@href = '#ssBasicSettings']";
		public static string AdvancedSettingsFrameTab = "//a[@href = '#erAdvancedSettings']";

		public static string BasicConfigurationAccordion = "//h2[@id='H1']/a";
		public static string AdvancedConfigurationAccordion = "//h2[@id='dnnSettings']/a";

		public static string SiteArrow = "//a[contains(@id, 'Template_cboPortals_Arrow')]";
		public static string SiteDropDown = "//div[contains(@id, 'Template_cboPortals_DropDown')]";
		public static string TemplateFileNameTextBox = "//input[contains(@id, 'Template_txtTemplateName')]";
		public static string TemplateDescriptionTextBox = "//textarea[contains(@id, 'Template_txtDescription')]";
		public static string IncludeContentCheckBox = "//input[contains(@id, 'Template_chkContent')]";
		public static string FilesCheckBox = "//input[contains(@id, 'Template_chkFiles')]";
		public static string PagesList = "//div[contains(@id, 'ctlPages')]//ul[@class = 'rtUL']//li";
		public static string ExportTemplateButton = "//a[contains(@id, 'Template_cmdExport')]";

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostSiteManagementUrl);

			WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.SiteManagementLink));

			WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostCommonSettings, ControlPanelIDs.HostSiteManagementOption);

			WaitForElement(By.XPath(HostSiteManagementPage.PortalsTable));
		}

		public void AddNewChildSite(string baseUrl, string siteName, string title, string template)
		{
			WaitAndClick(By.XPath(AddNewSiteButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new child site:");

			WaitForElement(By.Id(ChildSiteRadioButton));
			RadioButtonSelect(By.Id(ChildSiteRadioButton));

			WaitForElement(By.XPath("//body/form[@id='Form']/span[1]"));

			Clear(By.XPath(SiteAliasTextBox));
			Type(By.XPath(SiteAliasTextBox), baseUrl + "/" + siteName);
			Type(By.XPath(TitleTextBox), title);

			SlidingSelect.SelectByValue(_driver, By.XPath(TemplateArrow), By.XPath(TemplateDropDown), template, SlidingSelect.SelectByValueType.Contains);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Create Site' button:");
			ClickOnButton(By.XPath(CreateSiteFrameButton));

			WaitForElement(By.XPath(SiteCreatedConfirmationMessage), Settings.Default.WaitFactor * 60);
		}

		public void AddNewParentSite(string siteName, string title, string template)
		{
			WaitAndClick(By.XPath(AddNewSiteButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new parent site:");

			WaitForElement(By.Id(ParentSiteRadioButton));

			Clear(By.XPath(SiteAliasTextBox));
			Type(By.XPath(SiteAliasTextBox), siteName);
			Type(By.XPath(TitleTextBox), title);

			SlidingSelect.SelectByValue(_driver, By.XPath(TemplateArrow), By.XPath(TemplateDropDown), template, SlidingSelect.SelectByValueType.Contains);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Create Site' button:");
			ClickOnButton(By.XPath(CreateSiteFrameButton));

			WaitForElement(By.XPath(SiteCreatedConfirmationMessage), 60);
		}

		public void EditSite(string siteName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit site:");
			WaitAndClick(By.XPath("//tr[td/span[contains(@id, 'lblPortalAliases')]/a[contains(string(), '" + siteName + "')]]/td/a[contains(@href, 'Edit')]"));
		}

		public void DeleteSite(string siteName)
		{
			EditSite(siteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the site:");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Delete' button:");
			ClickOnButton(By.XPath(DeleteFrameButton));

			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}

		public void AddDescriptionToSite(string siteName, string siteNameDescription)
		{
			EditSite(siteName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Add description to the site:");

			WaitAndClick(By.XPath(BasicSettingsFrameTab));
			Type(By.XPath(SiteNameDescriptionTextBox), siteNameDescription);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on 'Update' button:");
			ClickOnButton(By.XPath(UpdateFrameButton));

			Thread.Sleep(1000);
		}

		public void NavigateToSite(string siteName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Navigate to the site:");

			Click(By.XPath("//span[contains(@id, 'Portals_grdPortals')]/a[contains(string(), '" + siteName + "')]"));

			Thread.Sleep(1000);
		}

		public void ExportTemplateBasicConfiguration(string siteName, string templateFileName, string templateDescription)
		{
			WaitForElement(By.XPath(BasicConfigurationAccordion));

			AccordionOpen(By.XPath(BasicConfigurationAccordion));

			SlidingSelectByValue(By.XPath(SiteArrow), By.XPath(SiteDropDown), siteName);

			Type(By.XPath(TemplateFileNameTextBox), templateFileName);

			Type(By.XPath(TemplateDescriptionTextBox), templateDescription);
		}

		public void TemplateParameters(By checkBoxName, CheckBox.ActionType action)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Include Content");

			AccordionOpen(By.XPath(AdvancedConfigurationAccordion));

			WaitForElement(checkBoxName).ScrollIntoView();

			if (action == CheckBox.ActionType.Check)
			{
				CheckBoxCheck(checkBoxName);
			}

			else
			{
				CheckBoxUncheck(checkBoxName);
			}
		}

		public void ExportSiteTemplate(string siteName, string templateFileName, string templateDescription)
		{
			WaitAndClick(By.XPath(ExportSiteTemplateButton));

			ExportTemplateBasicConfiguration(siteName, templateFileName, templateDescription);

			ClickOnButton(By.XPath(ExportTemplateButton));

			Thread.Sleep(1000);
		}

		public void ExportSiteTemplateWithContent(string siteName, string templateFileName, string templateDescription)
		{
			WaitAndClick(By.XPath(ExportSiteTemplateButton));

			ExportTemplateBasicConfiguration(siteName, templateFileName, templateDescription);

			TemplateParameters(By.XPath(IncludeContentCheckBox), CheckBox.ActionType.Check);

			ClickOnButton(By.XPath(ExportTemplateButton));

			Thread.Sleep(1000);
		}

		public void ExportSiteTemplateSomePages(string siteName, string templateFileName, string templateDescription)
		{
			WaitAndClick(By.XPath(ExportSiteTemplateButton));

			ExportTemplateBasicConfiguration(siteName, templateFileName, templateDescription);

			AccordionOpen(By.XPath(AdvancedConfigurationAccordion));

			WaitAndClick(By.XPath("//div[span[text() = 'My Website']]/span[contains(@class, 'hecked')]"));

			WaitAndClick(By.XPath("//div[span[contains(text(), 'Admin')]]/span[contains(@class, 'hecked')]"));

			WaitAndClick(By.XPath("//div[span[contains(text(), 'Our Products')]]/span[contains(@class, 'hecked')]"));

			WaitAndClick(By.XPath("//div[span[contains(text(), 'Contact Us')]]/span[contains(@class, 'hecked')]"));

			ClickOnButton(By.XPath(ExportTemplateButton));

			Thread.Sleep(1000);
		}

		public void ExportSiteTemplateWithLanguage(string siteName, string templateFileName, string templateDescription, string language)
		{
			WaitAndClick(By.XPath(ExportSiteTemplateButton));

			ExportTemplateBasicConfiguration(siteName, templateFileName, templateDescription);

			TemplateParameters(By.XPath(IncludeContentCheckBox), CheckBox.ActionType.Check);

			ClickOnButton(By.XPath(ExportTemplateButton));

			Thread.Sleep(1000);
		}

		public string SetLanguageName(string language)
		{
			string option = null;

			switch (language)
			{
				case "en":
					{
						option = "en-US";
						break;
					}
				case "de":
					{
						option = "de-DE";
						break;
					}
				case "es":
					{
						option = "es-ES";
						break;
					}
				case "fr":
					{
						option = "fr-FR";
						break;
					}
				case "it":
					{
						option = "it-IT";
						break;
					}
				case "nl":
					{
						option = "nl-NL";
						break;
					}
			}

			return option;
		}

		public void SelectLanguage(string language)
		{
			string[] languages = { "fr-FR", "de-DE", "es-ES", "nl-NL", "it-IT"};

			foreach (var pack in languages)
			{
				string element =
					"//table[contains(@id, '_Template_chkLanguages')]//input[@value = '" + pack + "']";

				if (ElementPresent(By.XPath(element)))
				{
					FindElement(By.XPath(element)).ScrollIntoView();
					CheckBoxUncheck(By.XPath(element));
				}
			}

			string packToInclude =
					"//table[contains(@id, '_Template_chkLanguages')]//input[@value = '" + language + "']";
			if (ElementPresent(By.XPath(packToInclude)))
			{
				FindElement(By.XPath(packToInclude)).ScrollIntoView();
				CheckBoxCheck(By.XPath(packToInclude));
			}
		}

		public void ExportSiteTemplateWithContentAndLanguage(string siteName, string templateFileName, string templateDescription, string language)
		{
			WaitAndClick(By.XPath(ExportSiteTemplateButton));

			ExportTemplateBasicConfiguration(siteName, templateFileName, templateDescription);

			TemplateParameters(By.XPath(IncludeContentCheckBox), CheckBox.ActionType.Check);

			SelectLanguage(SetLanguageName(language));

			ClickOnButton(By.XPath(ExportTemplateButton));

			Thread.Sleep(1000);
		}
	}
}
