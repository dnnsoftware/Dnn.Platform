using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.P1
{
	[SetUpFixture]
	[Category("P1")] 
	public class P1Setup : Common.Tests.BVT.BVTSetup
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}
	}
}