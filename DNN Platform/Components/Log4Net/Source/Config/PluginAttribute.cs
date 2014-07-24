using log4net.Core;
using log4net.Plugin;
using log4net.Util;
using System;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
	[Serializable]
	public sealed class PluginAttribute : Attribute, IPluginFactory
	{
		private string m_typeName;

		private System.Type m_type;

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

		public string TypeName
		{
			get
			{
				return this.m_typeName;
			}
			set
			{
				this.m_typeName = value;
			}
		}

		public PluginAttribute(string typeName)
		{
			this.m_typeName = typeName;
		}

		public PluginAttribute(System.Type type)
		{
			this.m_type = type;
		}

		public IPlugin CreatePlugin()
		{
			System.Type mType = this.m_type;
			if (this.m_type == null)
			{
				mType = SystemInfo.GetTypeFromString(this.m_typeName, true, true);
			}
			if (!typeof(IPlugin).IsAssignableFrom(mType))
			{
				throw new LogException(string.Concat("Plugin type [", mType.FullName, "] does not implement the log4net.IPlugin interface"));
			}
			return (IPlugin)Activator.CreateInstance(mType);
		}

		public override string ToString()
		{
			if (this.m_type == null)
			{
				return string.Concat("PluginAttribute[Type=", this.m_typeName, "]");
			}
			return string.Concat("PluginAttribute[Type=", this.m_type.FullName, "]");
		}
	}
}