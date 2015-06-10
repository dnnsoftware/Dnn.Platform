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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using log4net.Layout;
using log4net.Core;
using log4net.DateFormatter;
using log4net.Layout.Pattern;
using log4net.Util;

namespace log4net.Util
{
	/// <summary>
	/// Most of the work of the <see cref="PatternLayout"/> class
	/// is delegated to the PatternParser class.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <c>PatternParser</c> processes a pattern string and
	/// returns a chain of <see cref="PatternConverter"/> objects.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class PatternParser
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pattern">The pattern to parse.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="PatternParser" /> class 
		/// with the specified pattern string.
		/// </para>
		/// </remarks>
		public PatternParser(string pattern) 
		{
			m_pattern = pattern;
		}

		#endregion Public Instance Constructors

		#region Public Instance Methods

		/// <summary>
		/// Parses the pattern into a chain of pattern converters.
		/// </summary>
		/// <returns>The head of a chain of pattern converters.</returns>
		/// <remarks>
		/// <para>
		/// Parses the pattern into a chain of pattern converters.
		/// </para>
		/// </remarks>
		public PatternConverter Parse()
		{
			string[] converterNamesCache = BuildCache();

			ParseInternal(m_pattern, converterNamesCache);

			return m_head;
		}

		#endregion Public Instance Methods

		#region Public Instance Properties

		/// <summary>
		/// Get the converter registry used by this parser
		/// </summary>
		/// <value>
		/// The converter registry used by this parser
		/// </value>
		/// <remarks>
		/// <para>
		/// Get the converter registry used by this parser
		/// </para>
		/// </remarks>
		public Hashtable PatternConverters
		{
			get { return m_patternConverters; }
		}

		#endregion Public Instance Properties

		#region Private Instance Methods

		/// <summary>
		/// Build the unified cache of converters from the static and instance maps
		/// </summary>
		/// <returns>the list of all the converter names</returns>
		/// <remarks>
		/// <para>
		/// Build the unified cache of converters from the static and instance maps
		/// </para>
		/// </remarks>
		private string[] BuildCache()
		{
			string[] converterNamesCache = new string[m_patternConverters.Keys.Count];
			m_patternConverters.Keys.CopyTo(converterNamesCache, 0);

			// sort array so that longer strings come first
			Array.Sort(converterNamesCache, 0, converterNamesCache.Length, StringLengthComparer.Instance);

			return converterNamesCache;
		}

		#region StringLengthComparer

		/// <summary>
		/// Sort strings by length
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="IComparer" /> that orders strings by string length.
		/// The longest strings are placed first
		/// </para>
		/// </remarks>
		private sealed class StringLengthComparer : IComparer
		{
			public static readonly StringLengthComparer Instance = new StringLengthComparer();

			private StringLengthComparer()
			{
			}

			#region Implementation of IComparer

			public int Compare(object x, object y)
			{
				string s1 = x as string;
				string s2 = y as string;

				if (s1 == null && s2 == null)
				{
					return 0;
				}
				if (s1 == null)
				{
					return 1;
				}
				if (s2 == null)
				{
					return -1;
				}

				return s2.Length.CompareTo(s1.Length);
			}
		
			#endregion
		}

		#endregion // StringLengthComparer

		/// <summary>
		/// Internal method to parse the specified pattern to find specified matches
		/// </summary>
		/// <param name="pattern">the pattern to parse</param>
		/// <param name="matches">the converter names to match in the pattern</param>
		/// <remarks>
		/// <para>
		/// The matches param must be sorted such that longer strings come before shorter ones.
		/// </para>
		/// </remarks>
		private void ParseInternal(string pattern, string[] matches)
		{
			int offset = 0;
			while(offset < pattern.Length)
			{
				int i = pattern.IndexOf('%', offset);
				if (i < 0 || i == pattern.Length - 1)
				{
					ProcessLiteral(pattern.Substring(offset));
					offset = pattern.Length;
				}
				else
				{
					if (pattern[i+1] == '%')
					{
						// Escaped
						ProcessLiteral(pattern.Substring(offset, i - offset + 1));
						offset = i + 2;
					}
					else
					{
						ProcessLiteral(pattern.Substring(offset, i - offset));
						offset = i + 1;

						FormattingInfo formattingInfo = new FormattingInfo();

						// Process formatting options

						// Look for the align flag
						if (offset < pattern.Length)
						{
							if (pattern[offset] == '-')
							{
								// Seen align flag
								formattingInfo.LeftAlign = true;
								offset++;
							}
						}
						// Look for the minimum length
						while (offset < pattern.Length && char.IsDigit(pattern[offset]))
						{
							// Seen digit
							if (formattingInfo.Min < 0)
							{
								formattingInfo.Min = 0;
							}
							formattingInfo.Min = (formattingInfo.Min * 10) + int.Parse(pattern[offset].ToString(CultureInfo.InvariantCulture), System.Globalization.NumberFormatInfo.InvariantInfo);
							offset++;
						}
						// Look for the separator between min and max
						if (offset < pattern.Length)
						{
							if (pattern[offset] == '.')
							{
								// Seen separator
								offset++;
							}
						}
						// Look for the maximum length
						while (offset < pattern.Length && char.IsDigit(pattern[offset]))
						{
							// Seen digit
							if (formattingInfo.Max == int.MaxValue)
							{
								formattingInfo.Max = 0;
							}
							formattingInfo.Max = (formattingInfo.Max * 10) + int.Parse(pattern[offset].ToString(CultureInfo.InvariantCulture), System.Globalization.NumberFormatInfo.InvariantInfo);
							offset++;
						}

						int remainingStringLength = pattern.Length - offset;

						// Look for pattern
						for(int m=0; m<matches.Length; m++)
						{
							if (matches[m].Length <= remainingStringLength)
							{
								if (String.Compare(pattern, offset, matches[m], 0, matches[m].Length, false, System.Globalization.CultureInfo.InvariantCulture) == 0)
								{
									// Found match
									offset = offset + matches[m].Length;

									string option = null;

									// Look for option
									if (offset < pattern.Length)
									{
										if (pattern[offset] == '{')
										{
											// Seen option start
											offset++;
											
											int optEnd = pattern.IndexOf('}', offset);
											if (optEnd < 0)
											{
												// error
											}
											else
											{
												option = pattern.Substring(offset, optEnd - offset);
												offset = optEnd + 1;
											}
										}
									}

									ProcessConverter(matches[m], option, formattingInfo);
									break;
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Process a parsed literal
		/// </summary>
		/// <param name="text">the literal text</param>
		private void ProcessLiteral(string text)
		{
			if (text.Length > 0)
			{
				// Convert into a pattern
				ProcessConverter("literal", text, new FormattingInfo());
			}
		}

		/// <summary>
		/// Process a parsed converter pattern
		/// </summary>
		/// <param name="converterName">the name of the converter</param>
		/// <param name="option">the optional option for the converter</param>
		/// <param name="formattingInfo">the formatting info for the converter</param>
		private void ProcessConverter(string converterName, string option, FormattingInfo formattingInfo)
		{
			LogLog.Debug(declaringType, "Converter ["+converterName+"] Option ["+option+"] Format [min="+formattingInfo.Min+",max="+formattingInfo.Max+",leftAlign="+formattingInfo.LeftAlign+"]");

			// Lookup the converter type
            ConverterInfo converterInfo = (ConverterInfo)m_patternConverters[converterName];
			if (converterInfo == null)
			{
				LogLog.Error(declaringType, "Unknown converter name ["+converterName+"] in conversion pattern.");
			}
			else
			{
				// Create the pattern converter
				PatternConverter pc = null;
				try
				{
                    pc = (PatternConverter)Activator.CreateInstance(converterInfo.Type);
				}
				catch(Exception createInstanceEx)
				{
                    LogLog.Error(declaringType, "Failed to create instance of Type [" + converterInfo.Type.FullName + "] using default constructor. Exception: " + createInstanceEx.ToString());
				}

				// formattingInfo variable is an instance variable, occasionally reset 
				// and used over and over again
				pc.FormattingInfo = formattingInfo;
				pc.Option = option;
                pc.Properties = converterInfo.Properties;

			    IOptionHandler optionHandler = pc as IOptionHandler;
				if (optionHandler != null)
				{
					optionHandler.ActivateOptions();
				}

				AddConverter(pc);
			}
		}

		/// <summary>
		/// Resets the internal state of the parser and adds the specified pattern converter 
		/// to the chain.
		/// </summary>
		/// <param name="pc">The pattern converter to add.</param>
		private void AddConverter(PatternConverter pc) 
		{
			// Add the pattern converter to the list.

			if (m_head == null) 
			{
				m_head = m_tail = pc;
			}
			else 
			{
				// Set the next converter on the tail
				// Update the tail reference
				// note that a converter may combine the 'next' into itself
				// and therefore the tail would not change!
				m_tail = m_tail.SetNext(pc);
			}
		}

		#endregion Protected Instance Methods

		#region Private Constants

		private const char ESCAPE_CHAR = '%';
  
		#endregion Private Constants

		#region Private Instance Fields

		/// <summary>
		/// The first pattern converter in the chain
		/// </summary>
		private PatternConverter m_head;

		/// <summary>
		///  the last pattern converter in the chain
		/// </summary>
		private PatternConverter m_tail;

		/// <summary>
		/// The pattern
		/// </summary>
		private string m_pattern;

		/// <summary>
		/// Internal map of converter identifiers to converter types
		/// </summary>
		/// <remarks>
		/// <para>
		/// This map overrides the static s_globalRulesRegistry map.
		/// </para>
		/// </remarks>
		private Hashtable m_patternConverters = new Hashtable();

		#endregion Private Instance Fields

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the PatternParser class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(PatternParser);

	    #endregion Private Static Fields
	}
}
