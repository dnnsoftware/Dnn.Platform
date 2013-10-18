using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.UpgradeTests
{
	[TestFixture]
	[Category("BVToverUpgrade")]
	public class BVTUsers : Common.Tests.BVT.BVTUsers
	{
		protected override string DataFileLocation
		{
			get { return @"UpgradeTests\" + Settings.Default.BVTDataFile; }
		}
	}
}