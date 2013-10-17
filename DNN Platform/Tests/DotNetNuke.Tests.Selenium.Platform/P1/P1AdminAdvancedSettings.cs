using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.P1
{
	[TestFixture]
	[Category("P1")]
	public class P1AdminAdvancedSettings : Common.Tests.P1.P1AdminAdvancedSettings
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}

		[Test]
		public void Test005_OptionalModulesIsNotAvailable()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Optional Modules Is Not Available'");

			var adminAdvancedSettingsPage = new AdminAdvancedSettingsPage(_driver);
			adminAdvancedSettingsPage.OpenUsingButtons(_baseUrl);

			adminAdvancedSettingsPage.OpenTab(By.XPath(AdminAdvancedSettingsPage.OptionalModulesTab));

			Trace.WriteLine(BasePage.TraceLevelPage + "Optional Modules should not be available");
			Assert.IsFalse(
				adminAdvancedSettingsPage.ElementPresent(
					By.XPath(AdminAdvancedSettingsPage.OptionalModulesTable)),
				"Optional Module table is available");

			Trace.WriteLine(BasePage.TraceLevelPage + "Correct Warning message is present");
			Assert.That(adminAdvancedSettingsPage.WaitForElement(By.XPath(AdminAdvancedSettingsPage.OptionalModulesWarningMessage)).Text, 
				Is.StringContaining(AdminAdvancedSettingsPage.OptionalModulesWarningMessageText),
				"The message is not present or message text is not correct");
		}
	}
}
