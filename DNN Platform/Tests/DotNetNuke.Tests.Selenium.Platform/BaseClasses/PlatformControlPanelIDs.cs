using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DNNSelenium.Common.BaseClasses;
using OpenQA.Selenium;

namespace DNNSelenium.Platform.BaseClasses
{
	class PlatformControlPanelIDs : ControlPanelIDs
	{
		public static string AdminGoogleAnalyticsOption = "//dl[@id='controlbar_admin_advanced']//li[@data-tabname='Google Analytics']/a[1]";

	}
}
