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
		public void NavigationToPage(string assyName, string pageClassName, string openMethod)
		{
			VerifyStandardPageLayout(OpenPage(assyName, pageClassName, openMethod));
		}

		[Test]
		public void NumberOfLinksOnHostPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number of Links on Host Page'");
			NumberOfLinksOnPage(OpenPage("Platform", "CorePages.HostPage", "OpenUsingUrl"), HostBasePage.FeaturesList, HostPage.NumberOfLinks);
		}

		[Test]
		public void NumberOfLinksOnAdminPage()
		{
			Trace.WriteLine(BasePage.RunningTestKeyWord + "'Number of Links on Admin Page'");
			NumberOfLinksOnPage(OpenPage("Platform", "CorePages.AdminPage", "OpenUsingUrl"), AdminBasePage.FeaturesList, AdminPage.NumberOfLinks);
		}

	}
}