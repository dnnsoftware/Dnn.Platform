using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetNuke.Tests.DNNSelenium.BaseClasses
{
    public static class Utilities
    {
	    public static List<Exception> ExceptionList = new List<Exception>();

		public  static void SoftAssert (Action assert)
		{
			try
			{
				assert();
			}
			catch (Exception e)
			{
				ExceptionList.Add(e);
				Trace.WriteLine("Assert Failed =>" + e.Message + e.StackTrace);
			}
		}
    }
}
