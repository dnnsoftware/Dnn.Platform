using System;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
	[Serializable]
	public class AliasRepositoryAttribute : Attribute
	{
		private string m_name;

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

		public AliasRepositoryAttribute(string name)
		{
			this.Name = name;
		}
	}
}