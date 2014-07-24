using System;

namespace log4net.Config
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
	[Obsolete("Use AliasRepositoryAttribute instead of AliasDomainAttribute")]
	[Serializable]
	public sealed class AliasDomainAttribute : AliasRepositoryAttribute
	{
		public AliasDomainAttribute(string name) : base(name)
		{
		}
	}
}