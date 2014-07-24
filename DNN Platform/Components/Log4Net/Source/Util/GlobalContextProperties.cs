namespace log4net.Util
{
	public sealed class GlobalContextProperties : ContextPropertiesBase
	{
		private volatile ReadOnlyPropertiesDictionary m_readOnlyProperties = new ReadOnlyPropertiesDictionary();

		private readonly object m_syncRoot = new object();

		public override object this[string key]
		{
			get
			{
				return this.m_readOnlyProperties[key];
			}
			set
			{
				lock (this.m_syncRoot)
				{
					PropertiesDictionary propertiesDictionaries = new PropertiesDictionary(this.m_readOnlyProperties);
					propertiesDictionaries[key] = value;
					this.m_readOnlyProperties = new ReadOnlyPropertiesDictionary(propertiesDictionaries);
				}
			}
		}

		internal GlobalContextProperties()
		{
		}

		public void Clear()
		{
			lock (this.m_syncRoot)
			{
				this.m_readOnlyProperties = new ReadOnlyPropertiesDictionary();
			}
		}

		internal ReadOnlyPropertiesDictionary GetReadOnlyProperties()
		{
			return this.m_readOnlyProperties;
		}

		public void Remove(string key)
		{
			lock (this.m_syncRoot)
			{
				if (this.m_readOnlyProperties.Contains(key))
				{
					PropertiesDictionary propertiesDictionaries = new PropertiesDictionary(this.m_readOnlyProperties);
					propertiesDictionaries.Remove(key);
					this.m_readOnlyProperties = new ReadOnlyPropertiesDictionary(propertiesDictionaries);
				}
			}
		}
	}
}