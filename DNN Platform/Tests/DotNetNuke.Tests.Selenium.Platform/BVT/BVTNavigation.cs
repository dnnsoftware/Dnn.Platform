using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.BVT
{
	[TestFixture]
	[Category("BVT")]
	public class BVTNavigation : Common.Tests.BVT.BVTNavigation
	{
		protected override string DataFileLocation
		{
			get { return @"BVT\" + Settings.Default.BVTDataFile; }
		}

		[TestCase("Platform", "CorePages.AdminPage", "OpenUsingUrl")]
		[TestCase("Platform", "CorePages.AdminPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingUrl")]
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingControlPanel")]
		[TestCase("Platform", "CorePages.HostPage", "OpenUsingUrl")]
		[TestCase("Platform", "CorePages.HostPage", "OpenUsingButtons")]
		public void NavigationToPageCEpackage(string assyName, string pageClassName, string openMethod)
		{
			VerifyStandardPageLayout(OpenPage(assyName, pageClassName, openMethod));
		}

		[Test]
		public void NumberOfLinksOnHostPageCEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number of Links on Host Page'");

			HostBasePage hostPage = new HostPage(_driver);
			hostPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Host page: " + HostPage.NumberOfLinks);
			Assert.That(hostPage.FindElements(By.XPath(HostBasePage.FeaturesList)).Count, Is.EqualTo(HostPage.NumberOfLinks),
			            "The number of links on Host page is not correct");

			VerifyStandardPageLayout(hostPage);
		}

		[Test]
		public void NumberOfLinksOnToAdminPageCEPackage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number Of Links on Admin Page'");

			var adminPage = new AdminPage(_driver);
			adminPage.OpenUsingUrl(_baseUrl);

			Trace.WriteLine(BasePage.TraceLevelPage + "ASSERT the number of links on Admin page: " + AdminPage.NumberOfLinks);
			Assert.That(adminPage.FindElements(By.XPath(AdminBasePage.FeaturesList)).Count, Is.EqualTo(AdminPage.NumberOfLinks),
			            "The number of links on Admin page is not correct");
		}
	}
}