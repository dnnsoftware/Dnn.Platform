using log4net.Core;
using log4net.Util.TypeConverters;
using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace log4net.Util
{
	public sealed class OptionConverter
	{
		private const string DELIM_START = "${";

		private const char DELIM_STOP = '}';

		private const int DELIM_START_LEN = 2;

		private const int DELIM_STOP_LEN = 1;

		private readonly static Type declaringType;

		static OptionConverter()
		{
			OptionConverter.declaringType = typeof(OptionConverter);
		}

		private OptionConverter()
		{
		}

		public static bool CanConvertTypeTo(Type sourceType, Type targetType)
		{
			if (sourceType == null || targetType == null)
			{
				return false;
			}
			if (targetType.IsAssignableFrom(sourceType))
			{
				return true;
			}
			IConvertTo convertTo = ConverterRegistry.GetConvertTo(sourceType, targetType);
			if (convertTo != null && convertTo.CanConvertTo(targetType))
			{
				return true;
			}
			IConvertFrom convertFrom = ConverterRegistry.GetConvertFrom(targetType);
			if (convertFrom != null && convertFrom.CanConvertFrom(sourceType))
			{
				return true;
			}
			return false;
		}

		public static object ConvertStringTo(Type target, string txt)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			if (typeof(string) == target || typeof(object) == target)
			{
				return txt;
			}
			IConvertFrom convertFrom = ConverterRegistry.GetConvertFrom(target);
			if (convertFrom != null && convertFrom.CanConvertFrom(typeof(string)))
			{
				return convertFrom.ConvertFrom(txt);
			}
			if (target.IsEnum)
			{
				return OptionConverter.ParseEnum(target, txt, true);
			}
			Type[] typeArray = new Type[] { typeof(string) };
			MethodInfo method = target.GetMethod("Parse", typeArray);
			if (method == null)
			{
				return null;
			}
			object[] objArray = new object[] { txt };
			return method.Invoke(null, BindingFlags.InvokeMethod, null, objArray, CultureInfo.InvariantCulture);
		}

		public static object ConvertTypeTo(object sourceInstance, Type targetType)
		{
			Type type = sourceInstance.GetType();
			if (targetType.IsAssignableFrom(type))
			{
				return sourceInstance;
			}
			IConvertTo convertTo = ConverterRegistry.GetConvertTo(type, targetType);
			if (convertTo != null && convertTo.CanConvertTo(targetType))
			{
				return convertTo.ConvertTo(sourceInstance, targetType);
			}
			IConvertFrom convertFrom = ConverterRegistry.GetConvertFrom(targetType);
			if (convertFrom == null || !convertFrom.CanConvertFrom(type))
			{
				string[] str = new string[] { "Cannot convert source object [", sourceInstance.ToString(), "] to target type [", targetType.Name, "]" };
				throw new ArgumentException(string.Concat(str), "sourceInstance");
			}
			return convertFrom.ConvertFrom(sourceInstance);
		}

		public static object InstantiateByClassName(string className, Type superClass, object defaultValue)
		{
			object obj;
			if (className != null)
			{
				try
				{
					Type typeFromString = SystemInfo.GetTypeFromString(className, true, true);
					if (superClass.IsAssignableFrom(typeFromString))
					{
						obj = Activator.CreateInstance(typeFromString);
					}
					else
					{
						Type type = OptionConverter.declaringType;
						string[] strArrays = new string[] { "OptionConverter: A [", className, "] object is not assignable to a [", superClass.FullName, "] variable." };
						LogLog.Error(type, string.Concat(strArrays));
						obj = defaultValue;
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(OptionConverter.declaringType, string.Concat("Could not instantiate class [", className, "]."), exception);
					return defaultValue;
				}
				return obj;
			}
			return defaultValue;
		}

		private static object ParseEnum(Type enumType, string value, bool ignoreCase)
		{
			return Enum.Parse(enumType, value, ignoreCase);
		}

		public static string SubstituteVariables(string value, IDictionary props)
		{
			int num;
			StringBuilder stringBuilder = new StringBuilder();
			int num1 = 0;
			while (true)
			{
				num = value.IndexOf("${", num1);
				if (num == -1)
				{
					if (num1 == 0)
					{
						return value;
					}
					stringBuilder.Append(value.Substring(num1, value.Length - num1));
					return stringBuilder.ToString();
				}
				stringBuilder.Append(value.Substring(num1, num - num1));
				int num2 = value.IndexOf('}', num);
				if (num2 == -1)
				{
					break;
				}
				num = num + 2;
				string str = value.Substring(num, num2 - num);
				string item = props[str] as string;
				if (item != null)
				{
					stringBuilder.Append(item);
				}
				num1 = num2 + 1;
			}
			object[] objArray = new object[] { "[", value, "] has no closing brace. Opening brace at position [", num, "]" };
			throw new LogException(string.Concat(objArray));
		}

		public static bool ToBoolean(string argValue, bool defaultValue)
		{
			bool flag;
			if (argValue != null && argValue.Length > 0)
			{
				try
				{
					flag = bool.Parse(argValue);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(OptionConverter.declaringType, string.Concat("[", argValue, "] is not in proper bool form."), exception);
					return defaultValue;
				}
				return flag;
			}
			return defaultValue;
		}

		public static long ToFileSize(string argValue, long defaultValue)
		{
			long num;
			if (argValue == null)
			{
				return defaultValue;
			}
			string upper = argValue.Trim().ToUpper(CultureInfo.InvariantCulture);
			long num1 = (long)1;
			int num2 = upper.IndexOf("KB");
			int num3 = num2;
			if (num2 == -1)
			{
				int num4 = upper.IndexOf("MB");
				num3 = num4;
				if (num4 == -1)
				{
					int num5 = upper.IndexOf("GB");
					num3 = num5;
					if (num5 != -1)
					{
						num1 = (long)1073741824;
						upper = upper.Substring(0, num3);
					}
				}
				else
				{
					num1 = (long)1048576;
					upper = upper.Substring(0, num3);
				}
			}
			else
			{
				num1 = (long)1024;
				upper = upper.Substring(0, num3);
			}
			if (upper != null)
			{
				upper = upper.Trim();
				if (SystemInfo.TryParse(upper, out num))
				{
					return num * num1;
				}
				LogLog.Error(OptionConverter.declaringType, string.Concat("OptionConverter: [", upper, "] is not in the correct file size syntax."));
			}
			return defaultValue;
		}
	}
}