using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.UpgradeTests
{
	[TestFixture]
	[Category("BVT")]
	public class BVTChildSite : Common.Tests.BVT.BVTChildSite
	{
		protected override string DataFileLocation
		{
			get { return @"UpgradeTests\" + Settings.Default.BVTDataFile; }
		}
	}
}