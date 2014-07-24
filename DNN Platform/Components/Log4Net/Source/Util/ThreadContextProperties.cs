using System;
using System.Threading;

namespace log4net.Util
{
	public sealed class ThreadContextProperties : ContextPropertiesBase
	{
		private readonly static LocalDataStoreSlot s_threadLocalSlot;

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

		static ThreadContextProperties()
		{
			ThreadContextProperties.s_threadLocalSlot = Thread.AllocateDataSlot();
		}

		internal ThreadContextProperties()
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

		internal PropertiesDictionary GetProperties(bool create)
		{
			PropertiesDictionary data = (PropertiesDictionary)Thread.GetData(ThreadContextProperties.s_threadLocalSlot);
			if (data == null && create)
			{
				data = new PropertiesDictionary();
				Thread.SetData(ThreadContextProperties.s_threadLocalSlot, data);
			}
			return data;
		}

		public void Remove(string key)
		{
			PropertiesDictionary properties = this.GetProperties(false);
			if (properties != null)
			{
				properties.Remove(key);
			}
		}
	}
}