using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using DNNSelenium.Common.Tests.Upgrade;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.UpgradeTests
{
	[Category("Upgrade")]
	public class UpgradeTests : Common.Tests.Upgrade.UpgradeTests
	{
		protected override string DataFileLocation
		{
			get { return @"UpgradeTests\" + Settings.Default.UpgradeDataFile; }
		}
	}
}
