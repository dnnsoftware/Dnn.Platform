using log4net.Core;

namespace log4net.Util
{
	public abstract class LevelMappingEntry : IOptionHandler
	{
		private log4net.Core.Level m_level;

		public log4net.Core.Level Level
		{
			get
			{
				return this.m_level;
			}
			set
			{
				this.m_level = value;
			}
		}

		protected LevelMappingEntry()
		{
		}

		public virtual void ActivateOptions()
		{
		}
	}
}