using System.Diagnostics;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Platform.Properties;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.P1
{
    [TestFixture]
    [Category("P1")]
    public class P1AdminEventViewerPltfrm : Common.Tests.P1.P1AdminEventViewer
    {
        protected override string DataFileLocation
        {
            get { return @"P1\" + Settings.Default.P1DataFile; }
        }

    }
}
