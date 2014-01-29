using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace DNNSelenium.Common.CorePages
{
	public class InstallerPage : BaseClasses.BasePages.InstallerPage
	{
		public InstallerPage(IWebDriver driver)
			: base(driver)
		{
		}

		#region IDs

		public static string LanguageLabel = "lblLanguage_lblLabel";
		public static string LanguageArrow = "languageList_Arrow";
		public static string LanguageDropdown = "//div[@id='languageList_DropDown']";

		public static string EnglishIconId = "lang_en_US";
		public static string GermanIconId = "lang_de_DE";
		public static string SpanishIconId = "lang_es_ES";
		public static string FrenchIconId = "lang_fr_FR";
		public static string ItalianIconId = "lang_it_IT";
		public static string HollandIconId = "lang_nl_NL";

		#endregion

		public string SetLanguageOptionName(string language)
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
						option = "Deutsch (Deutschland)";
						break;
					}
				case "es":
					{
						option = "Español (España, alfabetización internacional)";
						break;
					}
				case "fr":
					{
						option = "français (France)";
						break;
					}
				case "it":
					{
						option = "italiano (Italia)";
						break;
					}
				case "nl":
					{
						option = "Nederlands (Nederland)";
						break;
					}
			}

			return option;
		} 

		public void SetInstallerLanguage(string installerLanguage)
		{
			Trace.WriteLine(BasePage.TraceLevelElement + "Select the language flag: '" + installerLanguage + "'");
			switch (installerLanguage)
			{
				case "en":
					{
						WaitForElement(By.Id(EnglishIconId), 10).WaitTillVisible(10).Click();
						WaitForElement(By.XPath("//a[@id='" + EnglishIconId + "'and contains(@class, 'selectedFlag')]"));
						break;
					}
				case "de":
					{
						WaitForElement(By.Id(GermanIconId)).Click();
						WaitForElement(By.XPath("//a[@id='" + GermanIconId + "'and contains(@class, 'selectedFlag')]"));
						break;
					}
				case "es":
					{
						WaitForElement(By.Id(SpanishIconId)).Click();
						WaitForElement(By.XPath("//a[@id='" + SpanishIconId + "'and contains(@class, 'selectedFlag')]"));
						break;
					}
				case "fr":
					{
						WaitForElement(By.Id(FrenchIconId)).Click();
						WaitForElement(By.XPath("//a[@id='" + FrenchIconId + "'and contains(@class, 'selectedFlag')]"));
						break;
					}
				case "it":
					{
						WaitForElement(By.Id(ItalianIconId)).Click();
						WaitForElement(By.XPath("//a[@id='" + ItalianIconId + "'and contains(@class, 'selectedFlag')]"));
						break;
					}
				case "nl":
					{
						WaitForElement(By.Id(HollandIconId)).Click();
						WaitForElement(By.XPath("//a[@id='" + HollandIconId + "'and contains(@class, 'selectedFlag')]"));
						break;
					}
			}
		}

		public void WelcomeScreen()
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Login to the site first time, using 'Visit Website' button:");

			WaitForElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style,'display: block;')]"), 30);

			WaitForElement(By.XPath("//div/iframe[contains(@src, 'GettingStarted')]"), 60);
			_driver.SwitchTo().Frame(0);

			WaitForElement(By.XPath("//div/a[last()]"), 60).WaitTillVisible(60);
			
			_driver.SwitchTo().DefaultContent();

			WaitForElement(By.XPath("//div[contains(@class, 'footer-left-side')]/label")).WaitTillEnabled(30);

			Actions action = new Actions(_driver);
			action.MoveToElement(FindElement(By.XPath("//div[contains(@class, 'footer-left-side')]/label"))).Click().Build().Perform();

			//WaitAndClick(By.XPath("//div[contains(@style,'display: block;')]//button[@role = 'button' and @title = 'close']"));

			//WaitForElementNotPresent(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style,'display: block;')]"), 30);

			FindElement(By.XPath("//div[contains(@class, 'dnnFormPopup') and contains(@style,'display: block;')]")).SendKeys(
				Keys.Escape);
		}

		public void SetWebSiteLanguage(string language)
		{
			Trace.WriteLine(BasePage.TraceLevelPage + "Set Website Language:");
			SlidingSelectByValue(By.Id(LanguageArrow), By.XPath(LanguageDropdown), SetLanguageOptionName(language));
		}
	}
}
