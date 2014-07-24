using log4net.Util.TypeConverters;
using System;

namespace log4net.Layout
{
	public class RawLayoutConverter : IConvertFrom
	{
		public RawLayoutConverter()
		{
		}

		public bool CanConvertFrom(Type sourceType)
		{
			return typeof(ILayout).IsAssignableFrom(sourceType);
		}

		public object ConvertFrom(object source)
		{
			ILayout layout = source as ILayout;
			if (layout == null)
			{
				throw ConversionNotSupportedException.Create(typeof(IRawLayout), source);
			}
			return new Layout2RawLayoutAdapter(layout);
		}
	}
}