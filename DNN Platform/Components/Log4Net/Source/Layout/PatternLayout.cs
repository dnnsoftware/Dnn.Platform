using log4net.Core;
using log4net.Layout.Pattern;
using log4net.Util;
using log4net.Util.PatternStringConverters;
using System;
using System.Collections;
using System.IO;

namespace log4net.Layout
{
	public class PatternLayout : LayoutSkeleton
	{
		public const string DefaultConversionPattern = "%message%newline";

		public const string DetailConversionPattern = "%timestamp [%thread] %level %logger %ndc - %message%newline";

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

		static PatternLayout()
		{
			PatternLayout.s_globalRulesRegistry = new Hashtable(45)
			{
				{ "literal", typeof(LiteralPatternConverter) },
				{ "newline", typeof(NewLinePatternConverter) },
				{ "n", typeof(NewLinePatternConverter) },
				{ "aspnet-cache", typeof(AspNetCachePatternConverter) },
				{ "aspnet-context", typeof(AspNetContextPatternConverter) },
				{ "aspnet-request", typeof(AspNetRequestPatternConverter) },
				{ "aspnet-session", typeof(AspNetSessionPatternConverter) },
				{ "c", typeof(LoggerPatternConverter) },
				{ "logger", typeof(LoggerPatternConverter) },
				{ "C", typeof(TypeNamePatternConverter) },
				{ "class", typeof(TypeNamePatternConverter) },
				{ "type", typeof(TypeNamePatternConverter) },
				{ "d", typeof(log4net.Layout.Pattern.DatePatternConverter) },
				{ "date", typeof(log4net.Layout.Pattern.DatePatternConverter) },
				{ "exception", typeof(ExceptionPatternConverter) },
				{ "F", typeof(FileLocationPatternConverter) },
				{ "file", typeof(FileLocationPatternConverter) },
				{ "l", typeof(FullLocationPatternConverter) },
				{ "location", typeof(FullLocationPatternConverter) },
				{ "L", typeof(LineLocationPatternConverter) },
				{ "line", typeof(LineLocationPatternConverter) },
				{ "m", typeof(MessagePatternConverter) },
				{ "message", typeof(MessagePatternConverter) },
				{ "M", typeof(MethodLocationPatternConverter) },
				{ "method", typeof(MethodLocationPatternConverter) },
				{ "p", typeof(LevelPatternConverter) },
				{ "level", typeof(LevelPatternConverter) },
				{ "P", typeof(log4net.Layout.Pattern.PropertyPatternConverter) },
				{ "property", typeof(log4net.Layout.Pattern.PropertyPatternConverter) },
				{ "properties", typeof(log4net.Layout.Pattern.PropertyPatternConverter) },
				{ "r", typeof(RelativeTimePatternConverter) },
				{ "timestamp", typeof(RelativeTimePatternConverter) },
				{ "stacktrace", typeof(StackTracePatternConverter) },
				{ "stacktracedetail", typeof(StackTraceDetailPatternConverter) },
				{ "t", typeof(ThreadPatternConverter) },
				{ "thread", typeof(ThreadPatternConverter) },
				{ "x", typeof(NdcPatternConverter) },
				{ "ndc", typeof(NdcPatternConverter) },
				{ "X", typeof(log4net.Layout.Pattern.PropertyPatternConverter) },
				{ "mdc", typeof(log4net.Layout.Pattern.PropertyPatternConverter) },
				{ "a", typeof(log4net.Layout.Pattern.AppDomainPatternConverter) },
				{ "appdomain", typeof(log4net.Layout.Pattern.AppDomainPatternConverter) },
				{ "u", typeof(log4net.Layout.Pattern.IdentityPatternConverter) },
				{ "identity", typeof(log4net.Layout.Pattern.IdentityPatternConverter) },
				{ "utcdate", typeof(log4net.Layout.Pattern.UtcDatePatternConverter) },
				{ "utcDate", typeof(log4net.Layout.Pattern.UtcDatePatternConverter) },
				{ "UtcDate", typeof(log4net.Layout.Pattern.UtcDatePatternConverter) },
				{ "w", typeof(log4net.Layout.Pattern.UserNamePatternConverter) },
				{ "username", typeof(log4net.Layout.Pattern.UserNamePatternConverter) }
			};
		}

		public PatternLayout() : this("%message%newline")
		{
		}

		public PatternLayout(string pattern)
		{
			this.IgnoresException = true;
			this.m_pattern = pattern;
			if (this.m_pattern == null)
			{
				this.m_pattern = "%message%newline";
			}
			this.ActivateOptions();
		}

		public override void ActivateOptions()
		{
			this.m_head = this.CreatePatternParser(this.m_pattern).Parse();
			for (PatternConverter i = this.m_head; i != null; i = i.Next)
			{
				PatternLayoutConverter patternLayoutConverter = i as PatternLayoutConverter;
				if (patternLayoutConverter != null && !patternLayoutConverter.IgnoresException)
				{
					this.IgnoresException = false;
					return;
				}
			}
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

		protected virtual PatternParser CreatePatternParser(string pattern)
		{
			PatternParser patternParser = new PatternParser(pattern);
			foreach (DictionaryEntry sGlobalRulesRegistry in PatternLayout.s_globalRulesRegistry)
			{
				ConverterInfo converterInfo = new ConverterInfo()
				{
					Name = (string)sGlobalRulesRegistry.Key,
					Type = (Type)sGlobalRulesRegistry.Value
				};
				patternParser.PatternConverters[sGlobalRulesRegistry.Key] = converterInfo;
			}
			foreach (DictionaryEntry mInstanceRulesRegistry in this.m_instanceRulesRegistry)
			{
				patternParser.PatternConverters[mInstanceRulesRegistry.Key] = mInstanceRulesRegistry.Value;
			}
			return patternParser;
		}

		public override void Format(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			for (PatternConverter i = this.m_head; i != null; i = i.Next)
			{
				i.Format(writer, loggingEvent);
			}
		}
	}
}