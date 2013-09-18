using System.Collections;
using System.Linq;
using System.Xml.Linq;
using DNNSelenium.Platform.Properties;

namespace DNNSelenium.Platform.InstallerTests
{
	public class InstallerTests : Common.Tests.Installer.InstallerTests
	{
		protected override string LayoutDataFileLocation
		{
			get { return @"InstallerTests\" + Settings.Default.LayoutInstallerDataFile; }
		}

		protected override string InstallerDataFileLocation
		{
			get { return @"InstallerTests\" + Settings.Default.InstallerDataFile; }
		}

		protected override string LogDataFileLocation
		{
			get { return @"InstallerTests\" + Settings.Default.InstallerLogFile; }
		}
	}
}
