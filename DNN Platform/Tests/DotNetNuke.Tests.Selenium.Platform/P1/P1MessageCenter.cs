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
	public class P1MessageCenter : Common.Tests.P1.P1MessageCenter
	{
		protected override string DataFileLocation
		{
			get { return @"P1\" + Settings.Default.P1DataFile; }
		}
	}
}
