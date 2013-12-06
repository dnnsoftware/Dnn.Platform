using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.BVT
{
	[TestFixture]
	[Category("BVT")]
	public class BVTModules : Common.Tests.BVT.BVTModules
	{
		protected override string DataFileLocation
		{
			get { return @"BVT\" + Settings.Default.BVTDataFile; }
		}

		[Test]
		public void Test001_AddModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add a new Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new Modules(_driver);
			module.OpenModulePanelUsingControlPanel();

			module.AddNewModuleUsingMenu(Modules.CommonModulesDescription["HtmlModule"].IdWhenOnBanner,
			                             Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage, "LeftPane");

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the Module location: " + Modules.LocationDescription["LeftPane"].IdWhenOnPage +
							Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage);
			Assert.IsTrue(blankPage.ElementPresent(By.XPath(Modules.LocationDescription["LeftPane"].IdWhenOnPage + Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage)),
			              "Module is not found");
		}

		[Test]
		public void Test002_EditModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Add content to HTML Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(Modules.LocationDescription["LeftPane"].IdWhenOnPage + Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage + "/a")).GetAttribute("name");

			module.AddContentToHTMLModule(moduleNumber, _moduleContent);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module Content is present on the page");
			Assert.That(_moduleContent,
			            Is.EqualTo(
				            module.FindElement(
					            By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber +
					                     "')]//div[contains(@id, 'lblContent')]")).Text));
		}

		[Test]
		public void Test003_EditModuleSettings()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Edit Module settings'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new Modules(_driver);

			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(Modules.LocationDescription["LeftPane"].IdWhenOnPage + Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage + "/a")).GetAttribute("name");
			module.ChangeModuleTitle(moduleNumber, _moduleTitle);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT a new Module Title is present on the page");
			StringAssert.Contains(_moduleTitle.ToUpper(),
								  blankPage.WaitForElement(
									  By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber +
											   "')]" + ControlPanelIDs.PageTitleID)).Text.ToUpper(),
								  "The  new Module Title is not saved correctly");
		}

		[Test]
		public void Test004_DeleteModule()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Delete the Module'");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, _pageName);

			var module = new Modules(_driver);
			Trace.WriteLine(BasePage.TraceLevelElement + "Find the Module number:");
			string moduleNumber =
				module.WaitForElement(By.XPath(Modules.LocationDescription["LeftPane"].IdWhenOnPage + Modules.CommonModulesDescription["HtmlModule"].IdWhenOnPage + "/a")).GetAttribute("name");
			module.DeleteModule(moduleNumber);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module " + moduleNumber + " deleted");
			Assert.IsFalse(module.ElementPresent(By.XPath("//div[contains(@class, 'DNN_HTML DnnModule-" + moduleNumber + "')]")),
						   "The Module is not deleted correctly");
		}

		[Test]
		public void Test005_RemoveModuleFromRecycleBin()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Remove the Module from Recycling Bin'");

			var adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			adminRecycleBinPage.RemoveModule(_moduleTitle);

			adminRecycleBinPage.OpenUsingButtons(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the module: " + _moduleTitle + " is NOT present in Recycle Bin");
			Assert.IsFalse(
				adminRecycleBinPage.ElementPresent(
					By.XPath(AdminRecycleBinPage.RecycleBinModuleContainerOption + "[contains(text(), '" + _moduleTitle + "')]")));
		}
	}
}