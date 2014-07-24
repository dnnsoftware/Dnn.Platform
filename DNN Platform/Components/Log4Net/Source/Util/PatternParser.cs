using log4net.Core;
using System;
using System.Collections;
using System.Globalization;

namespace log4net.Util
{
	public sealed class PatternParser
	{
		private const char ESCAPE_CHAR = '%';

		private PatternConverter m_head;

		private PatternConverter m_tail;

		private string m_pattern;

		private Hashtable m_patternConverters = new Hashtable();

		private readonly static Type declaringType;

		public Hashtable PatternConverters
		{
			get
			{
				return this.m_patternConverters;
			}
		}

		static PatternParser()
		{
			PatternParser.declaringType = typeof(PatternParser);
		}

		public PatternParser(string pattern)
		{
			this.m_pattern = pattern;
		}

		private void AddConverter(PatternConverter pc)
		{
			if (this.m_head != null)
			{
				this.m_tail = this.m_tail.SetNext(pc);
				return;
			}
			PatternConverter patternConverter = pc;
			PatternConverter patternConverter1 = patternConverter;
			this.m_tail = patternConverter;
			this.m_head = patternConverter1;
		}

		private string[] BuildCache()
		{
			string[] strArrays = new string[this.m_patternConverters.Keys.Count];
			this.m_patternConverters.Keys.CopyTo(strArrays, 0);
			Array.Sort(strArrays, 0, (int)strArrays.Length, PatternParser.StringLengthComparer.Instance);
			return strArrays;
		}

		public PatternConverter Parse()
		{
			string[] strArrays = this.BuildCache();
			this.ParseInternal(this.m_pattern, strArrays);
			return this.m_head;
		}

		private void ParseInternal(string pattern, string[] matches)
		{
			int length = 0;
		Label0:
			while (length < pattern.Length)
			{
				int num = pattern.IndexOf('%', length);
				if (num < 0 || num == pattern.Length - 1)
				{
					this.ProcessLiteral(pattern.Substring(length));
					length = pattern.Length;
				}
				else if (pattern[num + 1] != '%')
				{
					this.ProcessLiteral(pattern.Substring(length, num - length));
					length = num + 1;
					FormattingInfo formattingInfo = new FormattingInfo();
					if (length < pattern.Length && pattern[length] == '-')
					{
						formattingInfo.LeftAlign = true;
						length++;
					}
					while (length < pattern.Length && char.IsDigit(pattern[length]))
					{
						if (formattingInfo.Min < 0)
						{
							formattingInfo.Min = 0;
						}
						char chr = pattern[length];
						formattingInfo.Min = formattingInfo.Min * 10 + int.Parse(chr.ToString(CultureInfo.InvariantCulture), NumberFormatInfo.InvariantInfo);
						length++;
					}
					if (length < pattern.Length && pattern[length] == '.')
					{
						length++;
					}
					while (length < pattern.Length && char.IsDigit(pattern[length]))
					{
						if (formattingInfo.Max == 2147483647)
						{
							formattingInfo.Max = 0;
						}
						char chr1 = pattern[length];
						formattingInfo.Max = formattingInfo.Max * 10 + int.Parse(chr1.ToString(CultureInfo.InvariantCulture), NumberFormatInfo.InvariantInfo);
						length++;
					}
					int length1 = pattern.Length - length;
					int num1 = 0;
					while (num1 < (int)matches.Length)
					{
						if (matches[num1].Length > length1 || string.Compare(pattern, length, matches[num1], 0, matches[num1].Length, false, CultureInfo.InvariantCulture) != 0)
						{
							num1++;
						}
						else
						{
							length = length + matches[num1].Length;
							string str = null;
							if (length < pattern.Length && pattern[length] == '{')
							{
								length++;
								int num2 = pattern.IndexOf('}', length);
								if (num2 >= 0)
								{
									str = pattern.Substring(length, num2 - length);
									length = num2 + 1;
								}
							}
							this.ProcessConverter(matches[num1], str, formattingInfo);
							goto Label0;
						}
					}
				}
				else
				{
					this.ProcessLiteral(pattern.Substring(length, num - length + 1));
					length = num + 2;
				}
			}
		}

		private void ProcessConverter(string converterName, string option, FormattingInfo formattingInfo)
		{
			Type type = PatternParser.declaringType;
			object[] objArray = new object[] { "Converter [", converterName, "] Option [", option, "] Format [min=", formattingInfo.Min, ",max=", formattingInfo.Max, ",leftAlign=", formattingInfo.LeftAlign, "]" };
			LogLog.Debug(type, string.Concat(objArray));
			ConverterInfo item = (ConverterInfo)this.m_patternConverters[converterName];
			if (item == null)
			{
				LogLog.Error(PatternParser.declaringType, string.Concat("Unknown converter name [", converterName, "] in conversion pattern."));
				return;
			}
			PatternConverter properties = null;
			try
			{
				properties = (PatternConverter)Activator.CreateInstance(item.Type);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				LogLog.Error(PatternParser.declaringType, string.Concat("Failed to create instance of Type [", item.Type.FullName, "] using default constructor. Exception: ", exception.ToString()));
			}
			properties.FormattingInfo = formattingInfo;
			properties.Option = option;
			properties.Properties = item.Properties;
			IOptionHandler optionHandler = properties as IOptionHandler;
			if (optionHandler != null)
			{
				optionHandler.ActivateOptions();
			}
			this.AddConverter(properties);
		}

		private void ProcessLiteral(string text)
		{
			if (text.Length > 0)
			{
				this.ProcessConverter("literal", text, new FormattingInfo());
			}
		}

		private sealed class StringLengthComparer : IComparer
		{
			public readonly static PatternParser.StringLengthComparer Instance;

			static StringLengthComparer()
			{
				PatternParser.StringLengthComparer.Instance = new PatternParser.StringLengthComparer();
			}

			private StringLengthComparer()
			{
			}

			public int Compare(object x, object y)
			{
				string str = x as string;
				string str1 = y as string;
				if (str == null && str1 == null)
				{
					return 0;
				}
				if (str == null)
				{
					return 1;
				}
				if (str1 == null)
				{
					return -1;
				}
				return str1.Length.CompareTo(str.Length);
			}
		}
	}
}