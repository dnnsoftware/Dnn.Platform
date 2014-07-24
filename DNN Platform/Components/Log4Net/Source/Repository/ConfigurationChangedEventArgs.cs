using System;
using System.Collections;

namespace log4net.Repository
{
	public class ConfigurationChangedEventArgs : EventArgs
	{
		private readonly ICollection configurationMessages;

		public ICollection ConfigurationMessages
		{
			get
			{
				return this.configurationMessages;
			}
		}

		public ConfigurationChangedEventArgs(ICollection configurationMessages)
		{
			this.configurationMessages = configurationMessages;
		}
	}
}