using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.P1
{
	[TestFixture]
	[Category("BVT")]
	public class P1Navigation : Common.Tests.P1.P1Navigation
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
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

	}
}