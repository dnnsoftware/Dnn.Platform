using log4net.Core;
using log4net.Util;
using System;
using System.Globalization;
using System.Text;

namespace log4net.Appender
{
	public class AnsiColorTerminalAppender : AppenderSkeleton
	{
		public const string ConsoleOut = "Console.Out";

		public const string ConsoleError = "Console.Error";

		private const string PostEventCodes = "\u001b[0m";

		private bool m_writeToErrorStream;

		private LevelMapping m_levelMapping = new LevelMapping();

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public virtual string Target
		{
			get
			{
				if (!this.m_writeToErrorStream)
				{
					return "Console.Out";
				}
				return "Console.Error";
			}
			set
			{
				if (string.Compare("Console.Error", value.Trim(), true, CultureInfo.InvariantCulture) == 0)
				{
					this.m_writeToErrorStream = true;
					return;
				}
				this.m_writeToErrorStream = false;
			}
		}

		public AnsiColorTerminalAppender()
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			this.m_levelMapping.ActivateOptions();
		}

		public void AddMapping(AnsiColorTerminalAppender.LevelColors mapping)
		{
			this.m_levelMapping.Add(mapping);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			string str = base.RenderLoggingEvent(loggingEvent);
			AnsiColorTerminalAppender.LevelColors levelColor = this.m_levelMapping.Lookup(loggingEvent.Level) as AnsiColorTerminalAppender.LevelColors;
			if (levelColor != null)
			{
				str = string.Concat(levelColor.CombinedColor, str);
			}
			if (str.Length <= 1)
			{
				str = (str[0] == '\n' || str[0] == '\r' ? string.Concat("\u001b[0m", str) : string.Concat(str, "\u001b[0m"));
			}
			else if (str.EndsWith("\r\n") || str.EndsWith("\n\r"))
			{
				str = str.Insert(str.Length - 2, "\u001b[0m");
			}
			else
			{
				str = (str.EndsWith("\n") || str.EndsWith("\r") ? str.Insert(str.Length - 1, "\u001b[0m") : string.Concat(str, "\u001b[0m"));
			}
			if (!this.m_writeToErrorStream)
			{
				Console.Write(str);
				return;
			}
			Console.Error.Write(str);
		}

		[Flags]
		public enum AnsiAttributes
		{
			Bright = 1,
			Dim = 2,
			Underscore = 4,
			Blink = 8,
			Reverse = 16,
			Hidden = 32,
			Strikethrough = 64
		}

		public enum AnsiColor
		{
			Black,
			Red,
			Green,
			Yellow,
			Blue,
			Magenta,
			Cyan,
			White
		}

		public class LevelColors : LevelMappingEntry
		{
			private AnsiColorTerminalAppender.AnsiColor m_foreColor;

			private AnsiColorTerminalAppender.AnsiColor m_backColor;

			private AnsiColorTerminalAppender.AnsiAttributes m_attributes;

			private string m_combinedColor;

			public AnsiColorTerminalAppender.AnsiAttributes Attributes
			{
				get
				{
					return this.m_attributes;
				}
				set
				{
					this.m_attributes = value;
				}
			}

			public AnsiColorTerminalAppender.AnsiColor BackColor
			{
				get
				{
					return this.m_backColor;
				}
				set
				{
					this.m_backColor = value;
				}
			}

			internal string CombinedColor
			{
				get
				{
					return this.m_combinedColor;
				}
			}

			public AnsiColorTerminalAppender.AnsiColor ForeColor
			{
				get
				{
					return this.m_foreColor;
				}
				set
				{
					this.m_foreColor = value;
				}
			}

			public LevelColors()
			{
			}

			public override void ActivateOptions()
			{
				base.ActivateOptions();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\u001b[0;");
				stringBuilder.Append((int)(AnsiColorTerminalAppender.AnsiColor.Green | AnsiColorTerminalAppender.AnsiColor.Blue | AnsiColorTerminalAppender.AnsiColor.Cyan) + (int)this.m_foreColor);
				stringBuilder.Append(';');
				stringBuilder.Append(40 + (int)this.m_backColor);
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Bright) > 0)
				{
					stringBuilder.Append(";1");
				}
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Dim) > 0)
				{
					stringBuilder.Append(";2");
				}
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Underscore) > 0)
				{
					stringBuilder.Append(";4");
				}
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Blink) > 0)
				{
					stringBuilder.Append(";5");
				}
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Reverse) > 0)
				{
					stringBuilder.Append(";7");
				}
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Hidden) > 0)
				{
					stringBuilder.Append(";8");
				}
				if ((int)(this.m_attributes & AnsiColorTerminalAppender.AnsiAttributes.Strikethrough) > 0)
				{
					stringBuilder.Append(";9");
				}
				stringBuilder.Append('m');
				this.m_combinedColor = stringBuilder.ToString();
			}
		}
	}
}