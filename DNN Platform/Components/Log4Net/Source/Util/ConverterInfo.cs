namespace log4net.Util
{
	public sealed class ConverterInfo
	{
		private string m_name;

		private System.Type m_type;

		private readonly PropertiesDictionary properties = new PropertiesDictionary();

		public string Name
		{
			get
			{
				return this.m_name;
			}
			set
			{
				this.m_name = value;
			}
		}

		public PropertiesDictionary Properties
		{
			get
			{
				return this.properties;
			}
		}

		public System.Type Type
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		public ConverterInfo()
		{
		}

		public void AddProperty(PropertyEntry entry)
		{
			this.properties[entry.Key] = entry.Value;
		}
	}
}