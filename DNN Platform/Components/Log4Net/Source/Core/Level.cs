using System;

namespace log4net.Core
{
	[Serializable]
	public sealed class Level : IComparable
	{
		public readonly static Level Off;

		public readonly static Level Log4Net_Debug;

		public readonly static Level Emergency;

		public readonly static Level Fatal;

		public readonly static Level Alert;

		public readonly static Level Critical;

		public readonly static Level Severe;

		public readonly static Level Error;

		public readonly static Level Warn;

		public readonly static Level Notice;

		public readonly static Level Info;

		public readonly static Level Debug;

		public readonly static Level Fine;

		public readonly static Level Trace;

		public readonly static Level Finer;

		public readonly static Level Verbose;

		public readonly static Level Finest;

		public readonly static Level All;

		private readonly int m_levelValue;

		private readonly string m_levelName;

		private readonly string m_levelDisplayName;

		public string DisplayName
		{
			get
			{
				return this.m_levelDisplayName;
			}
		}

		public string Name
		{
			get
			{
				return this.m_levelName;
			}
		}

		public int Value
		{
			get
			{
				return this.m_levelValue;
			}
		}

		static Level()
		{
			Level.Off = new Level(2147483647, "OFF");
			Level.Log4Net_Debug = new Level(120000, "log4net:DEBUG");
			Level.Emergency = new Level(120000, "EMERGENCY");
			Level.Fatal = new Level(110000, "FATAL");
			Level.Alert = new Level(100000, "ALERT");
			Level.Critical = new Level(90000, "CRITICAL");
			Level.Severe = new Level(80000, "SEVERE");
			Level.Error = new Level(70000, "ERROR");
			Level.Warn = new Level(60000, "WARN");
			Level.Notice = new Level(50000, "NOTICE");
			Level.Info = new Level(40000, "INFO");
			Level.Debug = new Level(30000, "DEBUG");
			Level.Fine = new Level(30000, "FINE");
			Level.Trace = new Level(20000, "TRACE");
			Level.Finer = new Level(20000, "FINER");
			Level.Verbose = new Level(10000, "VERBOSE");
			Level.Finest = new Level(10000, "FINEST");
			Level.All = new Level(-2147483648, "ALL");
		}

		public Level(int level, string levelName, string displayName)
		{
			if (levelName == null)
			{
				throw new ArgumentNullException("levelName");
			}
			if (displayName == null)
			{
				throw new ArgumentNullException("displayName");
			}
			this.m_levelValue = level;
			this.m_levelName = string.Intern(levelName);
			this.m_levelDisplayName = displayName;
		}

		public Level(int level, string levelName) : this(level, levelName, levelName)
		{
		}

		public static int Compare(Level l, Level r)
		{
			if (l == r)
			{
				return 0;
			}
			if (l == null && r == null)
			{
				return 0;
			}
			if (l == null)
			{
				return -1;
			}
			if (r == null)
			{
				return 1;
			}
			return l.m_levelValue.CompareTo(r.m_levelValue);
		}

		public int CompareTo(object r)
		{
			Level level = r as Level;
			if (level == null)
			{
				throw new ArgumentException(string.Concat("Parameter: r, Value: [", r, "] is not an instance of Level"));
			}
			return Level.Compare(this, level);
		}

		public override bool Equals(object o)
		{
			Level level = o as Level;
			if (level == null)
			{
				return this.Equals(o);
			}
			return this.m_levelValue == level.m_levelValue;
		}

		public override int GetHashCode()
		{
			return this.m_levelValue;
		}

		public static bool operator ==(Level l, Level r)
		{
			if (l == null || r == null)
			{
				return l == r;
			}
			return l.m_levelValue == r.m_levelValue;
		}

		public static bool operator >(Level l, Level r)
		{
			return l.m_levelValue > r.m_levelValue;
		}

		public static bool operator >=(Level l, Level r)
		{
			return l.m_levelValue >= r.m_levelValue;
		}

		public static bool operator !=(Level l, Level r)
		{
			return !(l == r);
		}

		public static bool operator <(Level l, Level r)
		{
			return l.m_levelValue < r.m_levelValue;
		}

		public static bool operator <=(Level l, Level r)
		{
			return l.m_levelValue <= r.m_levelValue;
		}

		public override string ToString()
		{
			return this.m_levelName;
		}
	}
}