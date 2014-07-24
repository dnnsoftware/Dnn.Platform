using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security;
using System.Threading;

namespace log4net.Util
{
	public sealed class SystemInfo
	{
		private const string DEFAULT_NULL_TEXT = "(null)";

		private const string DEFAULT_NOT_AVAILABLE_TEXT = "NOT AVAILABLE";

		public readonly static Type[] EmptyTypes;

		private readonly static Type declaringType;

		private static string s_hostName;

		private static string s_appFriendlyName;

		private static string s_nullText;

		private static string s_notAvailableText;

		private static DateTime s_processStartTime;

		public static string ApplicationBaseDirectory
		{
			get
			{
				return AppDomain.CurrentDomain.BaseDirectory;
			}
		}

		public static string ApplicationFriendlyName
		{
			get
			{
				if (SystemInfo.s_appFriendlyName == null)
				{
					try
					{
						SystemInfo.s_appFriendlyName = AppDomain.CurrentDomain.FriendlyName;
					}
					catch (SecurityException securityException)
					{
						LogLog.Debug(SystemInfo.declaringType, "Security exception while trying to get current domain friendly name. Error Ignored.");
					}
					if (SystemInfo.s_appFriendlyName == null || SystemInfo.s_appFriendlyName.Length == 0)
					{
						try
						{
							SystemInfo.s_appFriendlyName = Path.GetFileName(SystemInfo.EntryAssemblyLocation);
						}
						catch (SecurityException securityException1)
						{
						}
					}
					if (SystemInfo.s_appFriendlyName == null || SystemInfo.s_appFriendlyName.Length == 0)
					{
						SystemInfo.s_appFriendlyName = SystemInfo.s_notAvailableText;
					}
				}
				return SystemInfo.s_appFriendlyName;
			}
		}

		public static string ConfigurationFileLocation
		{
			get
			{
				return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			}
		}

		public static int CurrentThreadId
		{
			get
			{
				return Thread.CurrentThread.ManagedThreadId;
			}
		}

		public static string EntryAssemblyLocation
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}

		public static string HostName
		{
			get
			{
				if (SystemInfo.s_hostName == null)
				{
					try
					{
						SystemInfo.s_hostName = Dns.GetHostName();
					}
					catch (SocketException socketException)
					{
					}
					catch (SecurityException securityException)
					{
					}
					if (SystemInfo.s_hostName == null || SystemInfo.s_hostName.Length == 0)
					{
						try
						{
							SystemInfo.s_hostName = Environment.MachineName;
						}
						catch (InvalidOperationException invalidOperationException)
						{
						}
						catch (SecurityException securityException1)
						{
						}
					}
					if (SystemInfo.s_hostName == null || SystemInfo.s_hostName.Length == 0)
					{
						SystemInfo.s_hostName = SystemInfo.s_notAvailableText;
					}
				}
				return SystemInfo.s_hostName;
			}
		}

		public static string NewLine
		{
			get
			{
				return Environment.NewLine;
			}
		}

		public static string NotAvailableText
		{
			get
			{
				return SystemInfo.s_notAvailableText;
			}
			set
			{
				SystemInfo.s_notAvailableText = value;
			}
		}

		public static string NullText
		{
			get
			{
				return SystemInfo.s_nullText;
			}
			set
			{
				SystemInfo.s_nullText = value;
			}
		}

		public static DateTime ProcessStartTime
		{
			get
			{
				return SystemInfo.s_processStartTime;
			}
		}

		static SystemInfo()
		{
			SystemInfo.EmptyTypes = new Type[0];
			SystemInfo.declaringType = typeof(SystemInfo);
			SystemInfo.s_processStartTime = DateTime.Now;
			string str = "(null)";
			string str1 = "NOT AVAILABLE";
			string appSetting = SystemInfo.GetAppSetting("log4net.NullText");
			if (appSetting != null && appSetting.Length > 0)
			{
				LogLog.Debug(SystemInfo.declaringType, string.Concat("Initializing NullText value to [", appSetting, "]."));
				str = appSetting;
			}
			string appSetting1 = SystemInfo.GetAppSetting("log4net.NotAvailableText");
			if (appSetting1 != null && appSetting1.Length > 0)
			{
				LogLog.Debug(SystemInfo.declaringType, string.Concat("Initializing NotAvailableText value to [", appSetting1, "]."));
				str1 = appSetting1;
			}
			SystemInfo.s_notAvailableText = str1;
			SystemInfo.s_nullText = str;
		}

		private SystemInfo()
		{
		}

		public static string AssemblyFileName(Assembly myAssembly)
		{
			return Path.GetFileName(myAssembly.Location);
		}

		public static string AssemblyLocationInfo(Assembly myAssembly)
		{
			string location;
			if (myAssembly.GlobalAssemblyCache)
			{
				return "Global Assembly Cache";
			}
			try
			{
				location = myAssembly.Location;
			}
			catch (SecurityException securityException)
			{
				location = "Location Permission Denied";
			}
			return location;
		}

		public static string AssemblyQualifiedName(Type type)
		{
			return string.Concat(type.FullName, ", ", type.Assembly.FullName);
		}

		public static string AssemblyShortName(Assembly myAssembly)
		{
			string fullName = myAssembly.FullName;
			int num = fullName.IndexOf(',');
			if (num > 0)
			{
				fullName = fullName.Substring(0, num);
			}
			return fullName.Trim();
		}

		public static string ConvertToFullPath(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			string localPath = "";
			try
			{
				string applicationBaseDirectory = SystemInfo.ApplicationBaseDirectory;
				if (applicationBaseDirectory != null)
				{
					Uri uri = new Uri(applicationBaseDirectory);
					if (uri.IsFile)
					{
						localPath = uri.LocalPath;
					}
				}
			}
			catch
			{
			}
			if (localPath == null || localPath.Length <= 0)
			{
				return Path.GetFullPath(path);
			}
			return Path.GetFullPath(Path.Combine(localPath, path));
		}

		public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string parameterName, object actualValue, string message)
		{
			return new ArgumentOutOfRangeException(parameterName, actualValue, message);
		}

		public static Hashtable CreateCaseInsensitiveHashtable()
		{
			return new Hashtable(StringComparer.OrdinalIgnoreCase);
		}

		public static string GetAppSetting(string key)
		{
			string item;
			try
			{
				item = ConfigurationManager.AppSettings[key];
			}
			catch (Exception exception)
			{
				LogLog.Error(SystemInfo.declaringType, "Exception while reading ConfigurationSettings. Check your .config file is well formed XML.", exception);
				return null;
			}
			return item;
		}

		public static Type GetTypeFromString(Type relativeType, string typeName, bool throwOnError, bool ignoreCase)
		{
			return SystemInfo.GetTypeFromString(relativeType.Assembly, typeName, throwOnError, ignoreCase);
		}

		public static Type GetTypeFromString(string typeName, bool throwOnError, bool ignoreCase)
		{
			return SystemInfo.GetTypeFromString(Assembly.GetCallingAssembly(), typeName, throwOnError, ignoreCase);
		}

		public static Type GetTypeFromString(Assembly relativeAssembly, string typeName, bool throwOnError, bool ignoreCase)
		{
			if (typeName.IndexOf(',') != -1)
			{
				return Type.GetType(typeName, throwOnError, ignoreCase);
			}
			Type type = relativeAssembly.GetType(typeName, false, ignoreCase);
			if (type != null)
			{
				return type;
			}
			Assembly[] assemblies = null;
			try
			{
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
			}
			catch (SecurityException securityException)
			{
			}
			if (assemblies != null)
			{
				Assembly[] assemblyArray = assemblies;
				for (int i = 0; i < (int)assemblyArray.Length; i++)
				{
					Assembly assembly = assemblyArray[i];
					type = assembly.GetType(typeName, false, ignoreCase);
					if (type != null)
					{
						Type type1 = SystemInfo.declaringType;
						string[] strArrays = new string[] { "Loaded type [", typeName, "] from assembly [", assembly.FullName, "] by searching loaded assemblies." };
						LogLog.Debug(type1, string.Concat(strArrays));
						return type;
					}
				}
			}
			if (throwOnError)
			{
				string[] strArrays1 = new string[] { "Could not load type [", typeName, "]. Tried assembly [", relativeAssembly.FullName, "] and all loaded assemblies" };
				throw new TypeLoadException(string.Concat(strArrays1));
			}
			return null;
		}

		public static Guid NewGuid()
		{
			return Guid.NewGuid();
		}

		public static bool TryParse(string s, out int val)
		{
			double num;
			val = 0;
			try
			{
				if (double.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
				{
					val = Convert.ToInt32(num);
					return true;
				}
			}
			catch
			{
			}
			return false;
		}

		public static bool TryParse(string s, out long val)
		{
			double num;
			val = (long)0;
			try
			{
				if (double.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
				{
					val = Convert.ToInt64(num);
					return true;
				}
			}
			catch
			{
			}
			return false;
		}
	}
}