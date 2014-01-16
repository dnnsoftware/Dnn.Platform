using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.P1
{
	[TestFixture]
	[Category("P1")]
	public class P1AdminPages : Common.Tests.P1.P1AdminPages
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}
	}
}