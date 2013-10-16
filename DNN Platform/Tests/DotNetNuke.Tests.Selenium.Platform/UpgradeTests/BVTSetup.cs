using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.UpgradeTests
{
	[SetUpFixture]
	[Category("Upgrade")]
	public class BVTSetup : Common.Tests.Upgrade.BVTSetup
	{
		protected override string DataFileLocation
		{
			get { return @"UpgradeTests\" + Settings.Default.BVTDataFile; }
		}
	}
}