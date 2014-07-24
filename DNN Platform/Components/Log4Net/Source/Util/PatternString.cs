using log4net.Core;
using log4net.Util.PatternStringConverters;
using System;
using System.Collections;
using System.Globalization;
using System.IO;

namespace log4net.Util
{
	public class PatternString : IOptionHandler
	{
		private static Hashtable s_globalRulesRegistry;

		private string m_pattern;

		private PatternConverter m_head;

		private Hashtable m_instanceRulesRegistry = new Hashtable();

		public string ConversionPattern
		{
			get
			{
				return this.m_pattern;
			}
			set
			{
				this.m_pattern = value;
			}
		}

		static PatternString()
		{
			PatternString.s_globalRulesRegistry = new Hashtable(15)
			{
				{ "appdomain", typeof(AppDomainPatternConverter) },
				{ "date", typeof(DatePatternConverter) },
				{ "env", typeof(EnvironmentPatternConverter) },
				{ "envFolderPath", typeof(EnvironmentFolderPathPatternConverter) },
				{ "identity", typeof(IdentityPatternConverter) },
				{ "literal", typeof(LiteralPatternConverter) },
				{ "newline", typeof(NewLinePatternConverter) },
				{ "processid", typeof(ProcessIdPatternConverter) },
				{ "property", typeof(PropertyPatternConverter) },
				{ "random", typeof(RandomStringPatternConverter) },
				{ "username", typeof(UserNamePatternConverter) },
				{ "utcdate", typeof(UtcDatePatternConverter) },
				{ "utcDate", typeof(UtcDatePatternConverter) },
				{ "UtcDate", typeof(UtcDatePatternConverter) }
			};
		}

		public PatternString()
		{
		}

		public PatternString(string pattern)
		{
			this.m_pattern = pattern;
			this.ActivateOptions();
		}

		public virtual void ActivateOptions()
		{
			this.m_head = this.CreatePatternParser(this.m_pattern).Parse();
		}

		public void AddConverter(ConverterInfo converterInfo)
		{
			if (converterInfo == null)
			{
				throw new ArgumentNullException("converterInfo");
			}
			if (!typeof(PatternConverter).IsAssignableFrom(converterInfo.Type))
			{
				throw new ArgumentException(string.Concat("The converter type specified [", converterInfo.Type, "] must be a subclass of log4net.Util.PatternConverter"), "converterInfo");
			}
			this.m_instanceRulesRegistry[converterInfo.Name] = converterInfo;
		}

		public void AddConverter(string name, Type type)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			ConverterInfo converterInfo = new ConverterInfo()
			{
				Name = name,
				Type = type
			};
			this.AddConverter(converterInfo);
		}

		private PatternParser CreatePatternParser(string pattern)
		{
			PatternParser patternParser = new PatternParser(pattern);
			foreach (DictionaryEntry sGlobalRulesRegistry in PatternString.s_globalRulesRegistry)
			{
				ConverterInfo converterInfo = new ConverterInfo()
				{
					Name = (string)sGlobalRulesRegistry.Key,
					Type = (Type)sGlobalRulesRegistry.Value
				};
				patternParser.PatternConverters.Add(sGlobalRulesRegistry.Key, converterInfo);
			}
			foreach (DictionaryEntry mInstanceRulesRegistry in this.m_instanceRulesRegistry)
			{
				patternParser.PatternConverters[mInstanceRulesRegistry.Key] = mInstanceRulesRegistry.Value;
			}
			return patternParser;
		}

		public void Format(TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			for (PatternConverter i = this.m_head; i != null; i = i.Next)
			{
				i.Format(writer, null);
			}
		}

		public string Format()
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			this.Format(stringWriter);
			return stringWriter.ToString();
		}
	}
}