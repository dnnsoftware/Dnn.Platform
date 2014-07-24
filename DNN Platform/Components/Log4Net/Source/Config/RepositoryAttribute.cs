using System;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly)]
	[Serializable]
	public class RepositoryAttribute : Attribute
	{
		private string m_name;

		private Type m_repositoryType;

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

		public Type RepositoryType
		{
			get
			{
				return this.m_repositoryType;
			}
			set
			{
				this.m_repositoryType = value;
			}
		}

		public RepositoryAttribute()
		{
		}

		public RepositoryAttribute(string name)
		{
			this.m_name = name;
		}
	}
}