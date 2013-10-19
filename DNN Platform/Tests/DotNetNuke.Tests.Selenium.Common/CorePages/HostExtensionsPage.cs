using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class HostExtensionsPage : ExtensionsPage
	{
		public HostExtensionsPage(IWebDriver driver) : base(driver) { }

		public static string HostExtensionsUrl = "/Host/Extensions";

		public override string PageTitleLabel
		{
			get { return "Extensions"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Extensions"; }
		}

		public override string PreLoadedModule
		{
			get { return "ExtensionsModule"; }
		}

		public static string CreateNewExtensionButton = "//a[contains(@id, '_createExtensionLink')]";
		public static string CreateNewModuleButton = "//a[contains(@id, '_createModuleLink')]";

		//Install Extension page
		public static string DescriptionTestBox = "//textarea[contains(@id, '_description_TextBox')]"; 

		public static string UninstallExtensionButton = "//a[contains(@id, '_EditExtension_cmdDelete')]";
		public static string UnistallPackageButton = "//a[contains(@id, '_cmdUninstall')]";
		public static string Return2Button = "//a[contains(@id, '_UnInstall_cmdReturn2')]";
		public static string DeleteCheckBox = "//input[@id = 'dnn_ctr_UnInstall_chkDelete']";

		public static string ExtensionTypeArrow = "//a[contains(@id, '_cboExtensionType_Arrow')]";
		public static string ExtensionTypeDropDown = "//div[contains(@id, '_wizNewExtension_cboExtensionType_DropDown')]";
		public static string ExtensionNameTextBox = "//input[contains(@id, '_extensionName_TextBox')]";
		public static string FriendlyNameTextBox = "//input[contains(@id, '_extensionFriendlyName_TextBox')]";

		public static string CreateModuleFromArrow = "//a[contains(@id, '_cboCreate_Arrow')]";
		public static string CreateModuleFromDropDown = "//div[contains(@id, '_cboCreate_DropDown')]";
		public static string CreateModuleButton = "//a[contains(@id, '_cmdCreate')]";
		public static string OwnerFolderArrow = "//a[contains(@id, '_cboOwner_Arrow')]";
		public static string OwnerFolderDropDown = "//div[contains(@id, 'cboOwner_DropDown')]";
		public static string ModuleFolderArrow = "//a[contains(@id, '_cboModule_Arrow')]";
		public static string ModuleFolderDropDown = "//div[contains(@id, '_cboModule_DropDown')]";
		public static string LanguageRadioButton = "//input[contains(@id, '_rblLanguage_0')]";
		public static string FileNameTextBox = "//input[contains(@id, '_EditModuleDefinition_txtFile')]";
		public static string ModuleNameTextBox = "//input[contains(@id, '_EditModuleDefinition_txtName')]";
		

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + HostExtensionsUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(HostBasePage.HostTopMenuLink));
			WaitAndClick(By.XPath(HostBasePage.ExtensionsLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Host '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelHostOption, ControlPanelIDs.ControlPanelHostCommonSettings, ControlPanelIDs.HostExtensionsOption);
		}

		public void EditExtensionDescription(string extensionName, string description)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Edit Extension Description: ");
			EditExtension(extensionName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Open Package Settings accordion: ");
			AccordionOpen(By.XPath(PackageSettingsAccordion));

			Trace.WriteLine(BasePage.TraceLevelPage + "Clear Description textbox: ");
			Clear(By.XPath(DescriptionTestBox));
			Trace.WriteLine(BasePage.TraceLevelPage + "Type in :" + description);
			Type(By.XPath(DescriptionTestBox), description);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Update Extension button :");
			ClickOnButton(By.XPath(UpdateExtensionButton));
		}

		public void DeleteExtension(string extensionName)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Delete Extension: ");
			EditExtension(extensionName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Uninstall Extension button :");
			ClickOnButton(By.XPath(UninstallExtensionButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Uninstall Package button :");
			ClickOnButton(By.XPath(UnistallPackageButton));

			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Return button :");
			ClickOnButton(By.XPath(Return2Button));
		}

		public void DeleteLanguagePack(string panelName, string packName)
		{
			IWebElement element = WaitForElement( By.XPath(
				panelName + "/following-sibling :: *//tr[td/span[contains(text(), '" + SetLanguageName(packName) + "')]][last()]//a/img[contains(@src, 'Delete')]"));
			ScrollIntoView(element, 100);
			element.Click();

			WaitForElement(By.XPath(DeleteCheckBox)).ScrollIntoView();
			WaitForElement(By.XPath(DeleteCheckBox)).WaitTillEnabled(30);
			//WaitForElement(By.XPath(DeleteCheckBox)).Info();
			Thread.Sleep(100);
			CheckBoxCheck(By.XPath(DeleteCheckBox));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Uninstall Package button :");
			ClickOnButton(By.XPath(HostExtensionsPage.UnistallPackageButton));

			ClickYesOnConfirmationBox();

			//Thread.Sleep(1000);
			WaitForElement(By.XPath(HostExtensionsPage.Return2Button));

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Return button :");
			ClickOnButton(By.XPath(HostExtensionsPage.Return2Button));

			Thread.Sleep(3000);
		}

		public void CreateNewExtension(string extensionName, string friedlyName, string extensionType)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Create New Extension: ");

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Create New Extension Button: ");
			IWebElement element = WaitForElement(By.XPath(CreateNewExtensionButton));
			ScrollIntoView(element, 200);
			element.Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Select: " + extensionType + " in Extension Type DropDown :");
			WaitForElement(By.XPath(ExtensionTypeArrow)).WaitTillEnabled();
			SlidingSelectByValue(By.XPath(ExtensionTypeArrow), By.XPath(ExtensionTypeDropDown), extensionType);

			Trace.WriteLine(BasePage.TraceLevelPage + "Type in: " + extensionName);
			Type(By.XPath(ExtensionNameTextBox), extensionName);
			Trace.WriteLine(BasePage.TraceLevelPage + "Type in: " + friedlyName);
			Type(By.XPath(FriendlyNameTextBox), friedlyName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next button: ");
			Click(By.XPath(NextButtonStart));
			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Next button: ");
			WaitForElement(By.XPath(NextButtonStep), 60).Click();

			Thread.Sleep(1000);
		}

		public void CreateNewModule(string moduleName, string ownerFolder, string moduleFolder, string fileName)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Create New Module: ");
			IWebElement element = WaitForElement(By.XPath(CreateNewModuleButton));
			ScrollIntoView(element, 200);
			element.Click();

			Trace.WriteLine(BasePage.TraceLevelPage + "Select: " + "New" + " in Create Module From DropDown :");
			WaitForElement(By.XPath(CreateModuleFromArrow)).WaitTillEnabled();
			SlidingSelectByValue(By.XPath(CreateModuleFromArrow), By.XPath(CreateModuleFromDropDown), "New");
			Thread.Sleep(1000);

			Trace.WriteLine(BasePage.TraceLevelPage + "Select: " + ownerFolder + " in Owner Folder :");
			SlidingSelectByValue(By.XPath(OwnerFolderArrow), By.XPath(OwnerFolderDropDown), ownerFolder);

			Trace.WriteLine(BasePage.TraceLevelPage + "Select: " + moduleFolder + " in Module Folder :");
			SlidingSelectByValue(By.XPath(ModuleFolderArrow), By.XPath(ModuleFolderDropDown), moduleFolder);

			Trace.WriteLine(BasePage.TraceLevelPage + "Select Language Radio Button: ");
			RadioButtonSelect(By.XPath(LanguageRadioButton));

			Trace.WriteLine(BasePage.TraceLevelPage + "Type in: " + fileName);
			Type(By.XPath(FileNameTextBox), fileName);
			Trace.WriteLine(BasePage.TraceLevelPage + "Type in: " + moduleName);
			Type(By.XPath(ModuleNameTextBox), moduleName);

			Trace.WriteLine(BasePage.TraceLevelPage + "Click on Create Module button: ");
			ClickOnButton(By.XPath(CreateModuleButton));
		}
	}
}
