namespace log4net.Repository.Hierarchy
{
	internal sealed class LoggerKey
	{
		private readonly string m_name;

		private readonly int m_hashCache;

		internal LoggerKey(string name)
		{
			this.m_name = string.Intern(name);
			this.m_hashCache = name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			LoggerKey loggerKey = obj as LoggerKey;
			if (loggerKey == null)
			{
				return false;
			}
			return this.m_name == loggerKey.m_name;
		}

		public override int GetHashCode()
		{
			return this.m_hashCache;
		}
	}
}