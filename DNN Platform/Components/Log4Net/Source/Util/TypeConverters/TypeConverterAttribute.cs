using System;

namespace log4net.Util.TypeConverters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface)]
	public sealed class TypeConverterAttribute : Attribute
	{
		private string m_typeName;

		public string ConverterTypeName
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

		public TypeConverterAttribute()
		{
		}

		public TypeConverterAttribute(string typeName)
		{
			this.m_typeName = typeName;
		}

		public TypeConverterAttribute(Type converterType)
		{
			this.m_typeName = SystemInfo.AssemblyQualifiedName(converterType);
		}
	}
}