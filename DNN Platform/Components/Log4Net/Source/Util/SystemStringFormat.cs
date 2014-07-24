using System;
using System.Text;

namespace log4net.Util
{
	public sealed class SystemStringFormat
	{
		private readonly IFormatProvider m_provider;

		private readonly string m_format;

		private readonly object[] m_args;

		private readonly static Type declaringType;

		static SystemStringFormat()
		{
			SystemStringFormat.declaringType = typeof(SystemStringFormat);
		}

		public SystemStringFormat(IFormatProvider provider, string format, params object[] args)
		{
			this.m_provider = provider;
			this.m_format = format;
			this.m_args = args;
		}

		private static void RenderArray(Array array, StringBuilder buffer)
		{
			if (array == null)
			{
				buffer.Append(SystemInfo.NullText);
				return;
			}
			if (array.Rank != 1)
			{
				buffer.Append(array.ToString());
				return;
			}
			buffer.Append("{");
			int length = array.Length;
			if (length > 0)
			{
				SystemStringFormat.RenderObject(array.GetValue(0), buffer);
				for (int i = 1; i < length; i++)
				{
					buffer.Append(", ");
					SystemStringFormat.RenderObject(array.GetValue(i), buffer);
				}
			}
			buffer.Append("}");
		}

		private static void RenderObject(object obj, StringBuilder buffer)
		{
			if (obj == null)
			{
				buffer.Append(SystemInfo.NullText);
				return;
			}
			try
			{
				buffer.Append(obj);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				buffer.Append("<Exception: ").Append(exception.Message).Append(">");
			}
			catch
			{
				buffer.Append("<Exception>");
			}
		}

		private static string StringFormat(IFormatProvider provider, string format, params object[] args)
		{
			string str;
			try
			{
				if (format != null)
				{
					str = (args != null ? string.Format(provider, format, args) : format);
				}
				else
				{
					str = null;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Warn(SystemStringFormat.declaringType, string.Concat("Exception while rendering format [", format, "]"), exception);
				str = SystemStringFormat.StringFormatError(exception, format, args);
			}
			catch
			{
				LogLog.Warn(SystemStringFormat.declaringType, string.Concat("Exception while rendering format [", format, "]"));
				str = SystemStringFormat.StringFormatError(null, format, args);
			}
			return str;
		}

		private static string StringFormatError(Exception formatException, string format, object[] args)
		{
			string str;
			try
			{
				StringBuilder stringBuilder = new StringBuilder("<log4net.Error>");
				if (formatException == null)
				{
					stringBuilder.Append("Exception during StringFormat");
				}
				else
				{
					stringBuilder.Append("Exception during StringFormat: ").Append(formatException.Message);
				}
				stringBuilder.Append(" <format>").Append(format).Append("</format>");
				stringBuilder.Append("<args>");
				SystemStringFormat.RenderArray(args, stringBuilder);
				stringBuilder.Append("</args>");
				stringBuilder.Append("</log4net.Error>");
				str = stringBuilder.ToString();
			}
			catch (Exception exception)
			{
				LogLog.Error(SystemStringFormat.declaringType, "INTERNAL ERROR during StringFormat error handling", exception);
				str = "<log4net.Error>Exception during StringFormat. See Internal Log.</log4net.Error>";
			}
			catch
			{
				LogLog.Error(SystemStringFormat.declaringType, "INTERNAL ERROR during StringFormat error handling");
				str = "<log4net.Error>Exception during StringFormat. See Internal Log.</log4net.Error>";
			}
			return str;
		}

		public override string ToString()
		{
			return SystemStringFormat.StringFormat(this.m_provider, this.m_format, this.m_args);
		}
	}
}