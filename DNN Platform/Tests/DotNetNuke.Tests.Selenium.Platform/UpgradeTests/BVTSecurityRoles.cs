using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.UpgradeTests
{
	[TestFixture]
	[Category("BVToverUpgrade")]
	public class BVTSecurityRoles : Common.Tests.BVT.BVTSecurityRoles
	{
		protected override string DataFileLocation
		{
			get { return @"UpgradeTests\" + Settings.Default.BVTDataFile; }
		}
	}
}