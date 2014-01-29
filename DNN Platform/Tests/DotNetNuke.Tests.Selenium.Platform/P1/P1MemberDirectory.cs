using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;

namespace DNNSelenium.Platform.P1
{
	[TestFixture]
	[Category("P1")]
	class P1MemberDirectory : Common.Tests.P1.P1MemberDirectory
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}
	}
}
