using System.Diagnostics;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.SmokeTest
{
	[TestFixture]
	[Category("Smoke")]
	public class SmokeTest : Common.Tests.Smoke.SmokeTest
	{
		protected override string DataFileLocation
		{
			get { return @"SmokeTest\" + Settings.Default.BVTDataFile; }
		}
	}
}
