using System;
using System.Runtime.Remoting.Messaging;
using System.Security;

namespace log4net.Util
{
	public sealed class LogicalThreadContextProperties : ContextPropertiesBase
	{
		private const string c_SlotName = "log4net.Util.LogicalThreadContextProperties";

		private bool m_disabled;

		private readonly static Type declaringType;

		public override object this[string key]
		{
			get
			{
				PropertiesDictionary properties = this.GetProperties(false);
				if (properties == null)
				{
					return null;
				}
				return properties[key];
			}
			set
			{
				this.GetProperties(true)[key] = value;
			}
		}

		static LogicalThreadContextProperties()
		{
			LogicalThreadContextProperties.declaringType = typeof(LogicalThreadContextProperties);
		}

		internal LogicalThreadContextProperties()
		{
		}

		public void Clear()
		{
			PropertiesDictionary properties = this.GetProperties(false);
			if (properties != null)
			{
				properties.Clear();
			}
		}

		private static PropertiesDictionary GetCallContextData()
		{
			return CallContext.GetData("log4net.Util.LogicalThreadContextProperties") as PropertiesDictionary;
		}

		internal PropertiesDictionary GetProperties(bool create)
		{
			PropertiesDictionary propertiesDictionaries;
			if (!this.m_disabled)
			{
				try
				{
					PropertiesDictionary callContextData = LogicalThreadContextProperties.GetCallContextData();
					if (callContextData == null && create)
					{
						callContextData = new PropertiesDictionary();
						LogicalThreadContextProperties.SetCallContextData(callContextData);
					}
					propertiesDictionaries = callContextData;
				}
				catch (SecurityException securityException1)
				{
					SecurityException securityException = securityException1;
					this.m_disabled = true;
					LogLog.Warn(LogicalThreadContextProperties.declaringType, "SecurityException while accessing CallContext. Disabling LogicalThreadContextProperties", securityException);
					if (create)
					{
						return new PropertiesDictionary();
					}
					return null;
				}
				return propertiesDictionaries;
			}
			if (create)
			{
				return new PropertiesDictionary();
			}
			return null;
		}

		public void Remove(string key)
		{
			PropertiesDictionary properties = this.GetProperties(false);
			if (properties != null)
			{
				properties.Remove(key);
			}
		}

		private static void SetCallContextData(PropertiesDictionary properties)
		{
			CallContext.SetData("log4net.Util.LogicalThreadContextProperties", properties);
		}
	}
}