using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Compilation;

using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.Upgrade
{
	/// <summary>
	/// This class is used to write log into separate installer log file.
	/// </summary>
	internal class DnnInstallLogger
	{
		private static StackFrame CallingFrame
		{
			get
			{
				StackFrame frame = null;
				StackFrame[] stack = new StackTrace().GetFrames();

				int frameDepth = 0;
				if (stack != null)
				{
					Type reflectedType = stack[frameDepth].GetMethod().ReflectedType;
					while (reflectedType == BuildManager.GetType("DotNetNuke.Services.Exceptions.Exceptions", false) || reflectedType == typeof(DnnInstallLogger))
					{
						frameDepth++;
						reflectedType = stack[frameDepth].GetMethod().ReflectedType;
					}
					frame = stack[frameDepth];
				}
				return frame;
			}
		}

		private static Type CallingType
		{
			get
			{
				return CallingFrame.GetMethod().DeclaringType;
			}
		}

		#region InstallLog

		public static void InstallLogError(object message)
		{
			LogInstaller("[ERROR]", message.ToString());
		}

		public static void InstallLogError(string message, Exception exception)
		{
			LogInstaller("[ERROR]", message.ToString(CultureInfo.InvariantCulture) + " : " + exception);
		}

		public static void InstallLogInfo(object message)
		{
			LogInstaller("[INFO]", message.ToString());
		}

		#endregion

		private static void LogInstaller(string logType, string message)
		{
			string logFile = "InstallerLog" + DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + DateTime.Now.Month.ToString(CultureInfo.InvariantCulture) + DateTime.Now.Day.ToString(CultureInfo.InvariantCulture) + ".resources";
			string logfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Portals\_default\Logs\" + logFile);
			using (var writer = new StreamWriter(logfilePath, true))
			{
				writer.WriteLine(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + " " + logType + " " + CallingType + " " + message);
			}

		}
	}
}
