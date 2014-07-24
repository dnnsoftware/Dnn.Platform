using System;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly)]
	[Obsolete("Use RepositoryAttribute instead of DomainAttribute")]
	[Serializable]
	public sealed class DomainAttribute : RepositoryAttribute
	{
		public DomainAttribute()
		{
		}

		public DomainAttribute(string name) : base(name)
		{
		}
	}
}