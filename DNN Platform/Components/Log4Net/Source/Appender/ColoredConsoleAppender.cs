using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace log4net.Appender
{
	public class ColoredConsoleAppender : AppenderSkeleton
	{
		public const string ConsoleOut = "Console.Out";

		public const string ConsoleError = "Console.Error";

        private const UInt32 STD_OUTPUT_HANDLE = unchecked((UInt32)(-11));
        private const UInt32 STD_ERROR_HANDLE = unchecked((UInt32)(-12));

		private readonly static char[] s_windowsNewline;

		private bool m_writeToErrorStream;

		private LevelMapping m_levelMapping = new LevelMapping();

		private StreamWriter m_consoleOutputWriter;

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

		static ColoredConsoleAppender()
		{
			ColoredConsoleAppender.s_windowsNewline = new char[] { '\r', '\n' };
		}

		public ColoredConsoleAppender()
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout property")]
		public ColoredConsoleAppender(ILayout layout) : this(layout, false)
		{
		}

		[Obsolete("Instead use the default constructor and set the Layout & Target properties")]
		public ColoredConsoleAppender(ILayout layout, bool writeToErrorStream)
		{
			this.Layout = layout;
			this.m_writeToErrorStream = writeToErrorStream;
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			this.m_levelMapping.ActivateOptions();
			Stream stream = null;
			stream = (!this.m_writeToErrorStream ? Console.OpenStandardOutput() : Console.OpenStandardError());
			Encoding encoding = Encoding.GetEncoding(ColoredConsoleAppender.GetConsoleOutputCP());
			this.m_consoleOutputWriter = new StreamWriter(stream, encoding, 256)
			{
				AutoFlush = true
			};
			GC.SuppressFinalize(this.m_consoleOutputWriter);
		}

		public void AddMapping(ColoredConsoleAppender.LevelColors mapping)
		{
			this.m_levelMapping.Add(mapping);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			ColoredConsoleAppender.CONSOLE_SCREEN_BUFFER_INFO cONSOLESCREENBUFFERINFO;
			if (this.m_consoleOutputWriter != null)
			{
				IntPtr zero = IntPtr.Zero;
                zero = (!this.m_writeToErrorStream ? ColoredConsoleAppender.GetStdHandle(STD_OUTPUT_HANDLE) : ColoredConsoleAppender.GetStdHandle(STD_ERROR_HANDLE));
				ushort combinedColor = 7;
				ColoredConsoleAppender.LevelColors levelColor = this.m_levelMapping.Lookup(loggingEvent.Level) as ColoredConsoleAppender.LevelColors;
				if (levelColor != null)
				{
					combinedColor = levelColor.CombinedColor;
				}
				string str = base.RenderLoggingEvent(loggingEvent);
				ColoredConsoleAppender.GetConsoleScreenBufferInfo(zero, out cONSOLESCREENBUFFERINFO);
				ColoredConsoleAppender.SetConsoleTextAttribute(zero, combinedColor);
				char[] charArray = str.ToCharArray();
				int length = (int)charArray.Length;
				bool flag = false;
				if (length > 1 && charArray[length - 2] == '\r' && charArray[length - 1] == '\n')
				{
					length = length - 2;
					flag = true;
				}
				this.m_consoleOutputWriter.Write(charArray, 0, length);
				ColoredConsoleAppender.SetConsoleTextAttribute(zero, cONSOLESCREENBUFFERINFO.wAttributes);
				if (flag)
				{
					this.m_consoleOutputWriter.Write(ColoredConsoleAppender.s_windowsNewline, 0, 2);
				}
			}
		}

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern int GetConsoleOutputCP();

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern bool GetConsoleScreenBufferInfo(IntPtr consoleHandle, out ColoredConsoleAppender.CONSOLE_SCREEN_BUFFER_INFO bufferInfo);

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern IntPtr GetStdHandle(uint type);

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=false, SetLastError=true)]
		private static extern bool SetConsoleTextAttribute(IntPtr consoleHandle, ushort attributes);

		[Flags]
		public enum Colors
		{
			Blue = 1,
			Green = 2,
			Cyan = 3,
			Red = 4,
			Purple = 5,
			Yellow = 6,
			White = 7,
			HighIntensity = 8
		}

		private struct CONSOLE_SCREEN_BUFFER_INFO
		{
			public ColoredConsoleAppender.COORD dwSize;

			public ColoredConsoleAppender.COORD dwCursorPosition;

			public ushort wAttributes;

			public ColoredConsoleAppender.SMALL_RECT srWindow;

			public ColoredConsoleAppender.COORD dwMaximumWindowSize;
		}

		private struct COORD
		{
			public ushort x;

			public ushort y;
		}

		public class LevelColors : LevelMappingEntry
		{
			private ColoredConsoleAppender.Colors m_foreColor;

			private ColoredConsoleAppender.Colors m_backColor;

			private ushort m_combinedColor;

			public ColoredConsoleAppender.Colors BackColor
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

			internal ushort CombinedColor
			{
				get
				{
					return this.m_combinedColor;
				}
			}

			public ColoredConsoleAppender.Colors ForeColor
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
				this.m_combinedColor = (ushort)((int)this.m_foreColor + ((int)this.m_backColor << (int)ColoredConsoleAppender.Colors.Red));
			}
		}

		private struct SMALL_RECT
		{
			public ushort Left;

			public ushort Top;

			public ushort Right;

			public ushort Bottom;
		}
	}
}