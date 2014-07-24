using System;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly)]
	[Obsolete("Use XmlConfiguratorAttribute instead of DOMConfiguratorAttribute")]
	[Serializable]
	public sealed class DOMConfiguratorAttribute : XmlConfiguratorAttribute
	{
		public DOMConfiguratorAttribute()
		{
		}
	}
}