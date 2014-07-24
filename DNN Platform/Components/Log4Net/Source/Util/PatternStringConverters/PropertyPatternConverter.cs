using System.IO;

namespace log4net.Util.PatternStringConverters
{
	internal sealed class PropertyPatternConverter : PatternConverter
	{
		public PropertyPatternConverter()
		{
		}

		protected override void Convert(TextWriter writer, object state)
		{
			CompositeProperties compositeProperty = new CompositeProperties();
			PropertiesDictionary properties = LogicalThreadContext.Properties.GetProperties(false);
			if (properties != null)
			{
				compositeProperty.Add(properties);
			}
			PropertiesDictionary propertiesDictionaries = ThreadContext.Properties.GetProperties(false);
			if (propertiesDictionaries != null)
			{
				compositeProperty.Add(propertiesDictionaries);
			}
			compositeProperty.Add(GlobalContext.Properties.GetReadOnlyProperties());
			if (this.Option == null)
			{
				PatternConverter.WriteDictionary(writer, null, compositeProperty.Flatten());
				return;
			}
			PatternConverter.WriteObject(writer, null, compositeProperty[this.Option]);
		}
	}
}