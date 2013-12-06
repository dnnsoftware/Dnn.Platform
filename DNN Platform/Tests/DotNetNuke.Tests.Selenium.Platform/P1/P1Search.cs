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
	public class P1Search : Common.Tests.P1.P1Search
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}

		[TestCase("Platform", "CorePages.AdminPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.HostPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingButtons")]
		public void SearchResultsCEPage(string assyName, string pageClassName, string openMethod)
		{
			VerifySearchResults(OpenPage(assyName, pageClassName, openMethod));
		}

		[TestCase("Platform", "CorePages.AdminPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.HostPage", "OpenUsingButtons")]
		[TestCase("Platform", "CorePages.AdminGoogleAnalyticsPage", "OpenUsingButtons")]
		public void QuickSearchCE(string assyName, string pageClassName, string openMethod)
		{
			VerifyQuickSearch(OpenPage(assyName, pageClassName, openMethod));
		}
	}
}