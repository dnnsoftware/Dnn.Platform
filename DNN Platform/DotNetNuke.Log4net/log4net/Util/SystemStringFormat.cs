#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace log4net.Util
{
	/// <summary>
	/// Utility class that represents a format string.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Utility class that represents a format string.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class SystemStringFormat
	{
		private readonly IFormatProvider m_provider;
		private readonly string m_format;
		private readonly object[] m_args;

		#region Constructor

		/// <summary>
		/// Initialise the <see cref="SystemStringFormat"/>
		/// </summary>
		/// <param name="provider">An <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
		/// <param name="format">A <see cref="System.String"/> containing zero or more format items.</param>
		/// <param name="args">An <see cref="System.Object"/> array containing zero or more objects to format.</param>
		public SystemStringFormat(IFormatProvider provider, string format, params object[] args)
		{
			m_provider = provider;
			m_format = format;
			m_args = args;
		}

		#endregion Constructor

		/// <summary>
		/// Format the string and arguments
		/// </summary>
		/// <returns>the formatted string</returns>
		public override string ToString()
		{
			return StringFormat(m_provider, m_format, m_args);
		}

		#region StringFormat

		/// <summary>
		/// Replaces the format item in a specified <see cref="System.String"/> with the text equivalent 
		/// of the value of a corresponding <see cref="System.Object"/> instance in a specified array.
		/// A specified parameter supplies culture-specific formatting information.
		/// </summary>
		/// <param name="provider">An <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
		/// <param name="format">A <see cref="System.String"/> containing zero or more format items.</param>
		/// <param name="args">An <see cref="System.Object"/> array containing zero or more objects to format.</param>
		/// <returns>
		/// A copy of format in which the format items have been replaced by the <see cref="System.String"/> 
		/// equivalent of the corresponding instances of <see cref="System.Object"/> in args.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method does not throw exceptions. If an exception thrown while formatting the result the
		/// exception and arguments are returned in the result string.
		/// </para>
		/// </remarks>
		private static string StringFormat(IFormatProvider provider, string format, params object[] args)
		{
			try
			{
				// The format is missing, log null value
				if (format == null)
				{
					return null;
				}

				// The args are missing - should not happen unless we are called explicitly with a null array
				if (args == null)
				{
					return format;
				}

				// Try to format the string
				return String.Format(provider, format, args);
			}
			catch(Exception ex)
			{
				log4net.Util.LogLog.Warn(declaringType, "Exception while rendering format ["+format+"]", ex);
				return StringFormatError(ex, format, args);
			}
#if !NET_2_0 && !MONO_2_0
			catch
			{
				log4net.Util.LogLog.Warn(declaringType, "Exception while rendering format ["+format+"]");
				return StringFormatError(null, format, args);
			}
#endif
		}

		/// <summary>
		/// Process an error during StringFormat
		/// </summary>
		private static string StringFormatError(Exception formatException, string format, object[] args)
		{
			try
			{
				StringBuilder buf = new StringBuilder("<log4net.Error>");

				if (formatException != null)
				{
					buf.Append("Exception during StringFormat: ").Append(formatException.Message);
				}
				else
				{
					buf.Append("Exception during StringFormat");
				}
				buf.Append(" <format>").Append(format).Append("</format>");
				buf.Append("<args>");
				RenderArray(args, buf);
				buf.Append("</args>");
				buf.Append("</log4net.Error>");

				return buf.ToString();
			}
			catch(Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "INTERNAL ERROR during StringFormat error handling", ex);
				return "<log4net.Error>Exception during StringFormat. See Internal Log.</log4net.Error>";
			}
#if !NET_2_0 && !MONO_2_0
			catch
			{
				log4net.Util.LogLog.Error(declaringType, "INTERNAL ERROR during StringFormat error handling");
				return "<log4net.Error>Exception during StringFormat. See Internal Log.</log4net.Error>";
			}
#endif
		}

		/// <summary>
		/// Dump the contents of an array into a string builder
		/// </summary>
		private static void RenderArray(Array array, StringBuilder buffer)
		{
			if (array == null)
			{
				buffer.Append(SystemInfo.NullText);
			}
			else
			{
				if (array.Rank != 1)
				{
					buffer.Append(array.ToString());
				}
				else
				{
					buffer.Append("{");
					int len = array.Length;

					if (len > 0)
					{
						RenderObject(array.GetValue(0), buffer);
						for (int i = 1; i < len; i++)
						{
							buffer.Append(", ");
							RenderObject(array.GetValue(i), buffer);
						}
					}
					buffer.Append("}");
				}
			}
		}

		/// <summary>
		/// Dump an object to a string
		/// </summary>
		private static void RenderObject(Object obj, StringBuilder buffer)
		{
			if (obj == null)
			{
				buffer.Append(SystemInfo.NullText);
			}
			else
			{
				try
				{
					buffer.Append(obj);
				}
				catch(Exception ex)
				{
					buffer.Append("<Exception: ").Append(ex.Message).Append(">");
				}
#if !NET_2_0 && !MONO_2_0
				catch
				{
					buffer.Append("<Exception>");
				}
#endif
			}
		}

		#endregion StringFormat

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the SystemStringFormat class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(SystemStringFormat);

	    #endregion Private Static Fields
	}
}
