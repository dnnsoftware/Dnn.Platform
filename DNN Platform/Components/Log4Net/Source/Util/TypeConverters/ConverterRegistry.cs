using log4net.Layout;

using System;
using System.Collections;
using System.Net;
using System.Text;

namespace log4net.Util.TypeConverters
{
	public sealed class ConverterRegistry
	{
		private readonly static Type declaringType;

		private static Hashtable s_type2converter;

		static ConverterRegistry()
		{
			ConverterRegistry.declaringType = typeof(ConverterRegistry);
			ConverterRegistry.s_type2converter = new Hashtable();
			ConverterRegistry.AddConverter(typeof(bool), typeof(BooleanConverter));
			ConverterRegistry.AddConverter(typeof(Encoding), typeof(EncodingConverter));
			ConverterRegistry.AddConverter(typeof(Type), typeof(TypeConverter));
			ConverterRegistry.AddConverter(typeof(PatternLayout), typeof(PatternLayoutConverter));
			ConverterRegistry.AddConverter(typeof(PatternString), typeof(PatternStringConverter));
			ConverterRegistry.AddConverter(typeof(IPAddress), typeof(IPAddressConverter));
		}

		private ConverterRegistry()
		{
		}

		public static void AddConverter(Type destinationType, object converter)
		{
			if (destinationType != null && converter != null)
			{
				lock (ConverterRegistry.s_type2converter)
				{
					ConverterRegistry.s_type2converter[destinationType] = converter;
				}
			}
		}

		public static void AddConverter(Type destinationType, Type converterType)
		{
			ConverterRegistry.AddConverter(destinationType, ConverterRegistry.CreateConverterInstance(converterType));
		}

		private static object CreateConverterInstance(Type converterType)
		{
			object obj;
			if (converterType == null)
			{
				throw new ArgumentNullException("converterType", "CreateConverterInstance cannot create instance, converterType is null");
			}
			if (typeof(IConvertFrom).IsAssignableFrom(converterType) || typeof(IConvertTo).IsAssignableFrom(converterType))
			{
				try
				{
					obj = Activator.CreateInstance(converterType);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(ConverterRegistry.declaringType, string.Concat("Cannot CreateConverterInstance of type [", converterType.FullName, "], Exception in call to Activator.CreateInstance"), exception);
					return null;
				}
				return obj;
			}
			else
			{
				LogLog.Error(ConverterRegistry.declaringType, string.Concat("Cannot CreateConverterInstance of type [", converterType.FullName, "], type does not implement IConvertFrom or IConvertTo"));
			}
			return null;
		}

		private static object GetConverterFromAttribute(Type destinationType)
		{
			object[] customAttributes = destinationType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
			if (customAttributes != null && (int)customAttributes.Length > 0)
			{
				TypeConverterAttribute typeConverterAttribute = customAttributes[0] as TypeConverterAttribute;
				if (typeConverterAttribute != null)
				{
					Type typeFromString = SystemInfo.GetTypeFromString(destinationType, typeConverterAttribute.ConverterTypeName, false, true);
					return ConverterRegistry.CreateConverterInstance(typeFromString);
				}
			}
			return null;
		}

		public static IConvertFrom GetConvertFrom(Type destinationType)
		{
			IConvertFrom convertFrom;
			lock (ConverterRegistry.s_type2converter)
			{
				IConvertFrom item = ConverterRegistry.s_type2converter[destinationType] as IConvertFrom;
				if (item == null)
				{
					item = ConverterRegistry.GetConverterFromAttribute(destinationType) as IConvertFrom;
					if (item != null)
					{
						ConverterRegistry.s_type2converter[destinationType] = item;
					}
				}
				convertFrom = item;
			}
			return convertFrom;
		}

		public static IConvertTo GetConvertTo(Type sourceType, Type destinationType)
		{
			IConvertTo convertTo;
			lock (ConverterRegistry.s_type2converter)
			{
				IConvertTo item = ConverterRegistry.s_type2converter[sourceType] as IConvertTo;
				if (item == null)
				{
					item = ConverterRegistry.GetConverterFromAttribute(sourceType) as IConvertTo;
					if (item != null)
					{
						ConverterRegistry.s_type2converter[sourceType] = item;
					}
				}
				convertTo = item;
			}
			return convertTo;
		}
	}
}