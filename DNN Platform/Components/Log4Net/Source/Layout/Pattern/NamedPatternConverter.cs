using log4net.Core;
using log4net.Util;
using System;
using System.IO;

namespace log4net.Layout.Pattern
{
	internal abstract class NamedPatternConverter : PatternLayoutConverter, IOptionHandler
	{
		protected int m_precision;

		private readonly static Type declaringType;

		static NamedPatternConverter()
		{
			NamedPatternConverter.declaringType = typeof(NamedPatternConverter);
		}

		protected NamedPatternConverter()
		{
		}

		public void ActivateOptions()
		{
			int num;
			this.m_precision = 0;
			if (this.Option != null)
			{
				string str = this.Option.Trim();
				if (str.Length > 0)
				{
					if (SystemInfo.TryParse(str, out num))
					{
						if (num > 0)
						{
							this.m_precision = num;
							return;
						}
						LogLog.Error(NamedPatternConverter.declaringType, string.Concat("NamedPatternConverter: Precision option (", str, ") isn't a positive integer."));
						return;
					}
					LogLog.Error(NamedPatternConverter.declaringType, string.Concat("NamedPatternConverter: Precision option \"", str, "\" not a decimal integer."));
				}
			}
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			string fullyQualifiedName = this.GetFullyQualifiedName(loggingEvent);
			if (this.m_precision <= 0)
			{
				writer.Write(fullyQualifiedName);
				return;
			}
			int length = fullyQualifiedName.Length;
			int num = length - 1;
			for (int i = this.m_precision; i > 0; i--)
			{
				num = fullyQualifiedName.LastIndexOf('.', num - 1);
				if (num == -1)
				{
					writer.Write(fullyQualifiedName);
					return;
				}
			}
			writer.Write(fullyQualifiedName.Substring(num + 1, length - num - 1));
		}

		protected abstract string GetFullyQualifiedName(LoggingEvent loggingEvent);
	}
}