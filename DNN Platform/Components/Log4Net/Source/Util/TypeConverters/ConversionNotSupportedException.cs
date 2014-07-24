using System;
using System.Runtime.Serialization;

namespace log4net.Util.TypeConverters
{
	[Serializable]
	public class ConversionNotSupportedException : ApplicationException
	{
		public ConversionNotSupportedException()
		{
		}

		public ConversionNotSupportedException(string message) : base(message)
		{
		}

		public ConversionNotSupportedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ConversionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public static ConversionNotSupportedException Create(Type destinationType, object sourceValue)
		{
			return ConversionNotSupportedException.Create(destinationType, sourceValue, null);
		}

		public static ConversionNotSupportedException Create(Type destinationType, object sourceValue, Exception innerException)
		{
			if (sourceValue == null)
			{
				return new ConversionNotSupportedException(string.Concat("Cannot convert value [null] to type [", destinationType, "]"), innerException);
			}
			object[] type = new object[] { "Cannot convert from type [", sourceValue.GetType(), "] value [", sourceValue, "] to type [", destinationType, "]" };
			return new ConversionNotSupportedException(string.Concat(type), innerException);
		}
	}
}