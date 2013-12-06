using System.Diagnostics;
using System.Threading;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using OpenQA.Selenium;

namespace DNNSelenium.Common.CorePages
{
	public class AdminTaxonomyPage : BasePage
	{
		public AdminTaxonomyPage(IWebDriver driver) : base(driver) { }

		public static string AdminTaxonomyUrl = "/Admin/Taxonomy";

		public static string VocabularyTable = "//table[contains(@id, '_VocabularyList_vocabulariesGrid')]";
		public static string VocabularyList = "//tr[contains(@id, 'VocabularyList_vocabulariesGrid')]";
		public static string CreateNewVocabularyButton = "//a[contains(@id,'VocabularyList_hlAddVocab')]";

		public static string VocabularyNameTextbox = "//input[contains(@id,'_nameTextBox')]";
		public static string VocabularyDescriptionTextBox = "//textarea[contains(@id,'_descriptionTextBox')]";

		public static string VocabularyTypeSimple = "//input[contains(@id, '_editVocabularyControl_typeList_0')]";
		public static string VocabularyTypeHierarchy = "//input[contains(@id, '_editVocabularyControl_typeList_1')]";

		public static string VocabularyScopeApplication = "//input[contains(@id, '_editVocabularyControl_scopeList_0')]";
		public static string VocabularyScopeWebsite = "//input[contains(@id, '_editVocabularyControl_scopeList_1')]";

		public static string CreateVocabularyButton = "//a[contains(@id,'CreateVocabulary_saveVocabulary')]";
		public static string CancelButton = "//a[contains(@id,'CreateVocabulary_cancelCreate')]";

		public static string EditUpdateButton = "//a[contains(@id,'EditVocabulary_saveVocabulary')]";
		public static string EditAddTermButton = "//a[contains(@id,'EditVocabulary_addTermButton')]";
		public static string EditCancelButton = "//a[contains(@id,'EditVocabulary_cancelEdit')]";
		public static string DeleteButton = "//a[contains(@id,'EditVocabulary_deleteVocabulary')]";

		public override string PageTitleLabel
		{
			get { return "Taxonomy Manager"; }
		}

		public override string PageHeaderLabel
		{
			get { return "Taxonomy"; }
		}

		public override string PreLoadedModule
		{
			get { return "TaxonomyManagerModule"; }
		}

		public void OpenUsingUrl(string baseUrl)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			GoToUrl(baseUrl + AdminTaxonomyUrl);
		}

		public void OpenUsingButtons(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			WaitAndClick(By.XPath(AdminBasePage.AdminTopMenuLink));
			WaitAndClick(By.XPath(AdminBasePage.TaxonomyLink));
		}

		public void OpenUsingControlPanel(string baseUrl)
		{
			GoToUrl(baseUrl);
			Trace.WriteLine(BasePage.TraceLevelPage + "Open Admin '" + PageTitleLabel + "' page:");
			SelectSubMenuOption(ControlPanelIDs.ControlPanelAdminOption, ControlPanelIDs.ControlPanelAdminAdvancedSettings, ControlPanelIDs.AdminTaxonomyOption);
		}

		public void EditVocabulary(string vocabularyName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Edit the Vocabulary:");
			WaitAndClick(By.XPath("//tr[td[text() ='" + vocabularyName + "']]/td/a[contains(@href, 'Edit')]"));
		}

		public string SetType(string type)
		{
			string option = null;

			switch (type)
			{
				case "simple":
					{
						option = VocabularyTypeSimple;
						break;
					}
				case "hierarchy":
					{
						option = VocabularyTypeHierarchy;
						break;
					}
			}

			return option;
		}

		public string SetScope(string scope)
		{
			string option = null;

			switch (scope)
			{
				case "application":
					{
						option = VocabularyScopeApplication;
						break;
					}
				case "website":
					{
						option = VocabularyTypeHierarchy;
						break;
					}
			}

			return option;
		} 

		public void AddNewVocabulary(string vocabularyName, string type, string scope)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add a new Vocabulary:");

			WaitAndClick(By.XPath(CreateNewVocabularyButton));

			WaitAndType(By.XPath(VocabularyNameTextbox), vocabularyName);
			
			RadioButtonSelect(By.XPath(SetType(type)));

			RadioButtonSelect(By.XPath(SetScope(scope)));

			Click(By.XPath(CreateVocabularyButton));

			Thread.Sleep(1000);
		}

		public void DeleteVocabulary(string vocabularyName)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Delete the Vocabulary:");

			EditVocabulary(vocabularyName);

			WaitAndClick(By.XPath(DeleteButton));
			WaitForConfirmationBox(60);
			ClickYesOnConfirmationBox();

			Thread.Sleep(1000);
		}

		public void AddDescription(string vocabularyName, string vocabularyDescription)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Add a description to Vocabulary:");
			EditVocabulary(vocabularyName);

			WaitAndClear(By.XPath(VocabularyDescriptionTextBox));
			Type(By.XPath(VocabularyDescriptionTextBox), vocabularyDescription);

			Click(By.XPath(EditUpdateButton));

			Thread.Sleep(1000);
		}
	}
}
