using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.P1 
{
	[TestFixture]
	[Category("P1")]
	public class P1ZSiteTemplates : Common.Tests.P1.P1ZSiteTemplates
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}
	}
}
