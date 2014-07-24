namespace log4net.Util
{
	public abstract class ContextPropertiesBase
	{
		public abstract object this[string key]
		{
			get;
			set;
		}

		protected ContextPropertiesBase()
		{
		}
	}
}