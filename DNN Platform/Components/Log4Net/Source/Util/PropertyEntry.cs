namespace log4net.Util
{
	public class PropertyEntry
	{
		private string m_key;

		private object m_value;

		public string Key
		{
			get
			{
				return this.m_key;
			}
			set
			{
				this.m_key = value;
			}
		}

		public object Value
		{
			get
			{
				return this.m_value;
			}
			set
			{
				this.m_value = value;
			}
		}

		public PropertyEntry()
		{
		}

		public override string ToString()
		{
			object[] mKey = new object[] { "PropertyEntry(Key=", this.m_key, ", Value=", this.m_value, ")" };
			return string.Concat(mKey);
		}
	}
}