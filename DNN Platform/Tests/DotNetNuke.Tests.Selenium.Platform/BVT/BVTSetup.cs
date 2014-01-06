using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.BVT
{
	[SetUpFixture]
	[Category("BVT")] 
	public class BVTSetup : Common.Tests.BVT.BVTSetup
	{
		protected override string DataFileLocation
		{
			get { return @"BVT\" + Settings.Default.BVTDataFile; }
		}
	}
}