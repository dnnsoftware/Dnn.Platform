using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Platform.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.UpgradeTests
{
	[TestFixture]
	[Category("BVToverUpgrade")]
	public class BVTNavigation : Common.Tests.Upgrade.BVTNavigation
	{
		protected override string DataFileLocation
		{
			get { return @"UpgradeTests\" + Settings.Default.BVTDataFile; }
		}

		[TestCase("Platform", "CorePages.AdminPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingControlPanel")]
		[TestCase("Platform", "CorePages.HostPage", "OpenUsingButtons")]
		public void NavigationToPage(string assyName, string pageClassName, string openMethod)
		{
			VerifyStandardPageLayout(OpenPage(assyName, pageClassName, openMethod));
		}
		
	}
}